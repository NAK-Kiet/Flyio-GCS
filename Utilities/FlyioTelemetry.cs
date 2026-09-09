using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MissionPlanner.Utilities
{
    /// <summary>Passive UDP subscriber. Intentionally has no send API or command transport.</summary>
    public sealed class FlyioTelemetry : IDisposable
    {
        readonly object gate = new object();
        readonly MAVLink.MavlinkParse parser = new MAVLink.MavlinkParse();
        readonly Dictionary<uint, object> values = new Dictionary<uint, object>();
        readonly Dictionary<uint, long> times = new Dictionary<uint, long>();
        readonly Dictionary<uint, long> counts = new Dictionary<uint, long>();
        UdpClient socket;
        Thread worker;
        volatile bool stopping;
        byte? systemId, componentId;
        string error;
        long rejected;

        public void Start()
        {
            lock (gate)
            {
                if (worker != null) return;
                try
                {
                    socket = new UdpClient(AddressFamily.InterNetwork);
                    socket.ExclusiveAddressUse = true;
                    socket.Client.Bind(new IPEndPoint(IPAddress.Any, 14551));
                    worker = new Thread(Receive) { IsBackground = true, Name = "Fly.io passive telemetry" };
                    worker.Start();
                }
                catch (SocketException ex) { error = ex.Message; socket?.Close(); socket = null; }
            }
        }

        void Receive()
        {
            try
            {
                while (!stopping)
                {
                    var peer = new IPEndPoint(IPAddress.Any, 0);
                    ProcessDatagram(socket.Receive(ref peer));
                }
            }
            catch (ObjectDisposedException) { }
            catch (SocketException ex) { if (!stopping) lock (gate) error = ex.Message; }
            finally { lock (gate) { socket?.Close(); socket = null; } }
        }

        public void ProcessDatagram(byte[] bytes)
        {
            lock (gate)
            using (var stream = new MemoryStream(bytes, false))
            {
                while (stream.Position < stream.Length)
                {
                    MAVLink.MAVLinkMessage message;
                    try { message = parser.ReadPacket(stream); }
                    catch (EndOfStreamException) { rejected++; break; }
                    if (message == null) { rejected++; continue; }
                    var data = message.data;
                    if (data is MAVLink.mavlink_heartbeat_t hb && message.compid == 1 && message.sysid != 0 &&
                        hb.autopilot == (byte)MAVLink.MAV_AUTOPILOT.PX4 &&
                        (!systemId.HasValue || !Fresh(0, 3)))
                    {
                        // A recovered/new vehicle must never inherit another session's GPS or battery.
                        values.Clear(); times.Clear();
                        systemId = message.sysid; componentId = message.compid;
                    }
                    if (message.sysid != systemId || message.compid != componentId) continue;
                    // Battery 0 is PX4's primary battery; do not mix packs.
                    if (data is MAVLink.mavlink_battery_status_t battery && battery.id != 0) continue;
                    values[message.msgid] = data;
                    times[message.msgid] = Stopwatch.GetTimestamp();
                    counts[message.msgid] = counts.TryGetValue(message.msgid, out var count) ? count + 1 : 1;
                }
            }
        }

        bool Fresh(uint id, double seconds) => times.TryGetValue(id, out var tick) &&
            (Stopwatch.GetTimestamp() - tick) / (double)Stopwatch.Frequency < seconds;
        T? Get<T>(uint id, double seconds = 3) where T : struct =>
            Fresh(id, seconds) && values.TryGetValue(id, out var value) && value is T typed ? typed : (T?)null;
        static double? Finite(double? value) => value.HasValue && !double.IsNaN(value.Value) && !double.IsInfinity(value.Value) ? value : null;
        static double? Degrees(float? value) => Finite(value * 180 / Math.PI);
        static double? Heading(double? value) => value.HasValue ? (value.Value % 360 + 360) % 360 : (double?)null;

        public FlyioTelemetrySnapshot Snapshot()
        {
            lock (gate)
            {
                var s = new FlyioTelemetrySnapshot { Listening = socket != null, Error = error,
                    SystemId = systemId, ComponentId = componentId, Rejected = rejected,
                    Counts = new Dictionary<uint, long>(counts) };
                var hb = Get<MAVLink.mavlink_heartbeat_t>(0);
                s.Connected = hb.HasValue && error == null && !stopping;
                if (!s.Connected) return s;
                s.Armed = (hb.Value.base_mode & 128) != 0;
                s.Mode = Mode(hb.Value);
                s.VehicleStatus = hb.Value.system_status;
                var attitude = Get<MAVLink.mavlink_attitude_t>(30);
                s.Roll = Degrees(attitude?.roll); s.Pitch = Degrees(attitude?.pitch); s.Yaw = Heading(Degrees(attitude?.yaw));
                var gps = Get<MAVLink.mavlink_gps_raw_int_t>(24);
                s.GpsFix = gps?.fix_type;
                s.Satellites = gps?.satellites_visible == 255 ? null : (int?)gps?.satellites_visible;
                var position = Get<MAVLink.mavlink_global_position_int_t>(33);
                if (gps.HasValue && gps.Value.fix_type >= 3 && gps.Value.fix_type <= 6 && position.HasValue &&
                    Math.Abs((long)position.Value.lat) <= 900000000 && Math.Abs((long)position.Value.lon) <= 1800000000)
                {
                    s.Latitude = position.Value.lat / 1e7; s.Longitude = position.Value.lon / 1e7;
                    s.Altitude = position.Value.alt == int.MaxValue ? null : (double?)(position.Value.alt / 1000.0);
                    s.RelativeAltitude = position.Value.relative_alt == int.MaxValue ? null : (double?)(position.Value.relative_alt / 1000.0);
                    if (position.Value.hdg <= 36000) s.Heading = Heading(position.Value.hdg / 100.0);
                }
                var hud = Get<MAVLink.mavlink_vfr_hud_t>(74);
                s.Groundspeed = Finite(hud?.groundspeed); s.Airspeed = Finite(hud?.airspeed); s.Climb = Finite(hud?.climb);
                s.Heading = s.Heading ?? s.Yaw;
                var sys = Get<MAVLink.mavlink_sys_status_t>(1, 5);
                if (sys.HasValue)
                {
                    var v = sys.Value;
                    if (v.voltage_battery != ushort.MaxValue && v.voltage_battery > 0) s.Voltage = v.voltage_battery / 1000.0;
                    if (v.battery_remaining >= 0 && v.battery_remaining <= 100) s.BatteryPercent = v.battery_remaining;
                    s.UnhealthySensors = v.onboard_control_sensors_enabled & v.onboard_control_sensors_present & ~v.onboard_control_sensors_health;
                    // MAV_SYS_STATUS_PREARM_CHECK (bit 28) is meaningful only when advertised.
                    if ((v.onboard_control_sensors_present & (1u << 28)) != 0)
                        s.PrearmHealthy = (v.onboard_control_sensors_enabled & v.onboard_control_sensors_health & (1u << 28)) != 0;
                }
                var pack = Get<MAVLink.mavlink_battery_status_t>(147, 5);
                if (pack.HasValue)
                {
                    var v = pack.Value;
                    int mv = v.voltages?.Where(x => x != ushort.MaxValue).Sum(x => (int)x) ?? 0;
                    mv += v.voltages_ext?.Where(x => x != 0 && x != ushort.MaxValue).Sum(x => (int)x) ?? 0;
                    if (mv > 0) s.Voltage = mv / 1000.0;
                    if (v.battery_remaining >= 0 && v.battery_remaining <= 100) s.BatteryPercent = v.battery_remaining;
                }
                var estimator = Get<MAVLink.mavlink_estimator_status_t>(230, 5);
                s.EstimatorFlags = estimator?.flags;
                var radio = Get<MAVLink.mavlink_radio_status_t>(109, 5);
                if (radio.HasValue && radio.Value.rssi != 255) s.Rssi = radio.Value.rssi;
                var status = Get<MAVLink.mavlink_statustext_t>(253, 30);
                if (status.HasValue)
                {
                    s.Warning = Encoding.UTF8.GetString(status.Value.text).TrimEnd('\0');
                    s.WarningSeverity = status.Value.severity;
                }
                return s;
            }
        }

        static string Mode(MAVLink.mavlink_heartbeat_t hb)
        {
            if ((hb.base_mode & 1) == 0) return "Unavailable";
            uint main = (hb.custom_mode >> 16) & 255, sub = hb.custom_mode >> 24;
            switch (main)
            {
                case 1: return "Manual"; case 2: return "Altitude"; case 3: return "Position";
                case 5: return "Acro"; case 6: return "Offboard"; case 7: return "Stabilized"; case 8: return "Rattitude";
                case 4:
                    switch (sub) { case 1: return "Ready"; case 2: return "Takeoff"; case 3: return "Hold";
                        case 4: return "Mission"; case 5: return "Return"; case 6: return "Land"; default: return "Auto " + sub; }
                default: return "PX4 " + main + "/" + sub;
            }
        }

        public void Dispose()
        {
            stopping = true;
            lock (gate) socket?.Close();
            worker?.Join(1000);
        }
    }

    public sealed class FlyioTelemetrySnapshot
    {
        public bool Listening, Connected;
        public string Error, Mode, Warning;
        public byte? SystemId, ComponentId, VehicleStatus, WarningSeverity;
        public bool? Armed, PrearmHealthy;
        public int? GpsFix, Satellites, BatteryPercent, Rssi;
        public uint? UnhealthySensors;
        public ushort? EstimatorFlags;
        public double? Roll, Pitch, Yaw, Latitude, Longitude, Altitude, RelativeAltitude, Heading, Groundspeed, Airspeed, Climb, Voltage;
        public long Rejected;
        public Dictionary<uint, long> Counts;
    }
}
