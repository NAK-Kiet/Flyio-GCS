using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using MissionPlanner.Maps;
using MissionPlanner.Utilities;

namespace MissionPlanner.GCSViews
{
    public partial class FlightData
    {
        FlyioTelemetry flyioTelemetry;
        Timer flyioTelemetryTimer;
        CheckBox flyioSitlEnabled;
        CheckBox flyioShowSitlMonitor;
        TabPage flyioHealthTab;
        TableLayoutPanel flyioHealthLayout;
        Label flyioHealthSummary, flyioHealthDetails;
        Label flyioEstimatorHealth, flyioPreflightWarning;
        Label flyioHudUnavailable;
        GMapOverlay flyioVehicleOverlay;
        GMapMarkerPlane flyioVehicleMarker;
        bool flyioCentered;
        DateTime flyioLastDiagnostic;
        readonly Color flyioHealthy = Color.FromArgb(32, 151, 94);
        readonly Color flyioWarning = Color.FromArgb(245, 158, 11);
        readonly Color flyioError = Color.FromArgb(211, 47, 47);

        void InitializeFlyioSitlTelemetry()
        {
            var tab = flyioHealthTab = new TabPage("VEHICLE HEALTH") { Name = "flyioHealthTab", BackColor = FlyioDataBackground, Padding = new Padding(10), AutoScroll = true };
            var layout = flyioHealthLayout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            flyioSitlEnabled = new CheckBox { Text = "PX4 SITL · receive only · UDP 14551", AutoSize = true,
                ForeColor = FlyioDataText, Checked = true, Margin = new Padding(0, 0, 0, 8) };
            flyioShowSitlMonitor = new CheckBox { Text = "PX4 SITL Monitor", AutoSize = true,
                ForeColor = FlyioDataText, Checked = false, Margin = new Padding(0, 0, 0, 8) };
            flyioHealthSummary = new Label { AutoSize = true, MaximumSize = new Size(650, 0), ForeColor = FlyioDataSecondaryText };
            flyioHealthDetails = new Label { AutoSize = true, MaximumSize = new Size(650, 0), ForeColor = FlyioDataText,
                Margin = new Padding(0, 8, 0, 8) };
            flyioEstimatorHealth = new Label { AutoSize = true, ForeColor = FlyioDataSecondaryText };
            flyioPreflightWarning = new Label { AutoSize = true, MaximumSize = new Size(650, 0), ForeColor = FlyioDataSecondaryText };
            layout.Controls.Add(flyioSitlEnabled); layout.Controls.Add(flyioShowSitlMonitor);
            layout.Controls.Add(flyioHealthSummary); layout.Controls.Add(flyioHealthDetails);
            layout.Controls.Add(flyioEstimatorHealth); layout.Controls.Add(flyioPreflightWarning);
            layout.Controls.Add(new Label { Text = "ARM / DISARM / MODE: BLOCKED — receive-only isolation.\r\nControl-path approval required.",
                AutoSize = true, ForeColor = flyioWarning });
            tab.Controls.Add(layout); tabControlactions.TabPages.Insert(1, tab); tabControlactions.SelectedTab = tabQuick;
            flyioHudUnavailable = new Label { Text = "ATTITUDE UNAVAILABLE", Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter, BackColor = FlyioDataBackground, ForeColor = FlyioDataSecondaryText, Visible = false };
            hud1.Parent.Controls.Add(flyioHudUnavailable);
            flyioVehicleOverlay = new GMapOverlay("flyio-passive-vehicle");
            gMapControl1.Overlays.Add(flyioVehicleOverlay);
            flyioSitlEnabled.CheckedChanged += (sender, args) =>
            {
                if (flyioSitlEnabled.Checked)
                {
                    flyioTelemetry = new FlyioTelemetry(); flyioTelemetry.Start();
                    SetFlyioLegacyActions(false);
                }
                else
                {
                    flyioTelemetry?.Dispose(); flyioTelemetry = null;
                    flyioHudUnavailable.Visible = false; hud1.Visible = true;
                    flyioVehicleOverlay.Markers.Clear(); flyioVehicleMarker = null; flyioCentered = false;
                    flyioHealthSummary.Text = "Passive SITL listener stopped";
                    flyioHealthDetails.Text = "Select receive-only to retry the listener.";
                    SetFlyioLegacyActions(true);
                }
            };
            flyioShowSitlMonitor.CheckedChanged += (sender, args) =>
                ApplyFlyioHealthVisibility(flyioShowSitlMonitor.Checked ||
                    (flyioTelemetry != null && flyioTelemetry.Snapshot().Connected));
            flyioTelemetry = new FlyioTelemetry(); flyioTelemetry.Start();
            SetFlyioLegacyActions(false);
            ApplyFlyioHealthVisibility(false);
            flyioTelemetryTimer = new Timer(components) { Interval = 250 };
            flyioTelemetryTimer.Tick += (sender, args) => UpdateFlyioSitlTelemetry();
            flyioTelemetryTimer.Start();
            Load += (sender, args) => { flyioCentered = false; };
        }

        void ApplyFlyioHealthVisibility(bool visible)
        {
            if (flyioHealthLayout == null) return;
            foreach (Control child in flyioHealthLayout.Controls)
            {
                if (child == flyioSitlEnabled || child == flyioShowSitlMonitor)
                    continue;
                child.Visible = visible;
            }
            flyioHealthLayout.PerformLayout();
            flyioHealthTab.PerformLayout();
        }

        void SetFlyioLegacyActions(bool enabled)
        {
            lbl_hdop.Visible = enabled; lbl_sats.Visible = enabled;
            // Passive telemetry is deliberately never installed into MainV2.comPort.
            // Existing action tabs must not suggest they control this passive vehicle.
            foreach (TabPage page in tabControlactions.TabPages)
                if (page != tabQuick && page != tabGauges && page.Text != "VEHICLE HEALTH") page.Enabled = enabled;
        }

        static string TelemetryNumber(double? value, string format = "0.0") =>
            value.HasValue ? value.Value.ToString(format, CultureInfo.InvariantCulture) : "Unavailable";

        void Status(Label label, string title, string value, Color color)
        { label.Text = title + "\r\n" + value; label.ForeColor = color; }

        void UpdateFlyioSitlTelemetry()
        {
            if (flyioTelemetry == null || IsDisposed) return;
            var s = flyioTelemetry.Snapshot();
            ApplyFlyioHealthVisibility(flyioShowSitlMonitor.Checked || s.Connected);
            var unavailable = FlyioDataSecondaryText;
            flyioHealthSummary.Text = s.Error != null ? "LISTENER ERROR: " + s.Error : s.Connected
                ? "CONNECTED · heartbeat active · PX4 " + s.SystemId + "/" + s.ComponentId + " · receive only"
                : "DISCONNECTED · heartbeat unavailable or stale (3 s timeout)";
            flyioHealthSummary.ForeColor = s.Error != null ? flyioError : s.Connected ? flyioHealthy : unavailable;
            bool estimatorGood = s.EstimatorFlags.HasValue && (s.EstimatorFlags.Value & 17) == 17 && (s.EstimatorFlags.Value & 3072) == 0;
            flyioEstimatorHealth.Text = "ESTIMATOR: " + (!s.EstimatorFlags.HasValue ? "Unavailable" : estimatorGood ? "Attitude and global position valid" : "Warning: inspect estimator flags");
            flyioEstimatorHealth.ForeColor = !s.EstimatorFlags.HasValue ? unavailable : estimatorGood ? flyioHealthy : flyioWarning;
            flyioPreflightWarning.Text = "STATUS: " + (s.Warning ?? "No recent status message; prearm availability shown above");
            flyioPreflightWarning.ForeColor = s.Warning == null ? unavailable : s.WarningSeverity <= 3 ? flyioError : s.WarningSeverity <= 4 ? flyioWarning : FlyioDataText;
            flyioHealthDetails.Text = "Mode: " + (s.Mode ?? "Unavailable") + "   State: " + (s.Armed.HasValue ? s.Armed.Value ? "ARMED" : "DISARMED" : "Unavailable") +
                "\r\nGPS: " + (s.GpsFix.HasValue ? FormatGpsFix(s.GpsFix.Value) : "Unavailable") + "   Satellites: " + TelemetryNumber(s.Satellites, "0") +
                "\r\nBattery: " + TelemetryNumber(s.Voltage) + " V   " + TelemetryNumber(s.BatteryPercent, "0") + " %" +
                "\r\nEKF flags: " + (s.EstimatorFlags.HasValue ? "0x" + s.EstimatorFlags.Value.ToString("X") : "Unavailable") +
                "   Sensor faults: " + (s.UnhealthySensors.HasValue ? "0x" + s.UnhealthySensors.Value.ToString("X") : "Unavailable") +
                "\r\nPrearm: " + (s.PrearmHealthy.HasValue ? s.PrearmHealthy.Value ? "Checks healthy" : "Checks failing" : "Unavailable") +
                "\r\nLatest status (30 s): " + (s.Warning ?? "Unavailable") +
                "\r\nRSSI (raw): " + TelemetryNumber(s.Rssi, "0") + "   Link quality: unavailable" +
                "\r\nLatitude: " + TelemetryNumber(s.Latitude, "0.0000000") + "   Longitude: " + TelemetryNumber(s.Longitude, "0.0000000") +
                "\r\nMSL: " + TelemetryNumber(s.Altitude) + " m   Relative: " + TelemetryNumber(s.RelativeAltitude) + " m" +
                "\r\nGroundspeed: " + TelemetryNumber(s.Groundspeed) + " m/s   R/P/Y: " + TelemetryNumber(s.Roll) + " / " + TelemetryNumber(s.Pitch) + " / " + TelemetryNumber(s.Yaw);
            flyioMapLatitude.Text = "LAT  " + TelemetryNumber(s.Latitude, "0.0000000");
            flyioMapLongitude.Text = "LON  " + TelemetryNumber(s.Longitude, "0.0000000");
            flyioMapAltitude.Text = "ALT  " + TelemetryNumber(s.RelativeAltitude) + " m";
            flyioMapHeading.Text = "HDG  " + TelemetryNumber(s.Heading, "000") + "°";
            flyioMapSatellites.Text = "SATS  " + TelemetryNumber(s.Satellites, "0");
            Status(flyioModeStatus, "MODE", s.Mode ?? "Unavailable", s.Connected ? FlyioDataText : unavailable);
            Status(flyioArmStatus, "STATE", s.Armed.HasValue ? s.Armed.Value ? "ARMED" : "DISARMED" : "Unavailable", s.Armed.HasValue ? s.Armed.Value ? flyioWarning : flyioHealthy : unavailable);
            Status(flyioGpsStatus, "GPS", s.GpsFix.HasValue ? FormatGpsFix(s.GpsFix.Value) : "Unavailable", !s.GpsFix.HasValue ? unavailable : s.GpsFix >= 3 ? flyioHealthy : flyioWarning);
            Status(flyioSatelliteStatus, "SAT", TelemetryNumber(s.Satellites, "0"), s.Satellites.HasValue ? FlyioDataText : unavailable);
            Status(flyioHdopStatus, "BATTERY", TelemetryNumber(s.Voltage) + " V", s.Voltage.HasValue ? FlyioDataText : unavailable);
            Status(flyioVibeStatus, "CHARGE", TelemetryNumber(s.BatteryPercent, "0") + " %", !s.BatteryPercent.HasValue ? unavailable : s.BatteryPercent <= 15 ? flyioError : s.BatteryPercent <= 30 ? flyioWarning : flyioHealthy);
            Status(flyioLinkStatus, "HEARTBEAT", s.Connected ? "LIVE" : "STALE", s.Connected ? flyioHealthy : unavailable);
            Status(flyioRcStatus, "PREFLIGHT", s.PrearmHealthy.HasValue ? s.PrearmHealthy.Value ? "HEALTHY" : "WARNING" : "Unavailable", !s.PrearmHealthy.HasValue ? unavailable : s.PrearmHealthy.Value ? flyioHealthy : flyioWarning);

            // Existing HUD and instruments, with explicit loss-of-data indication.
            bool attitudeValid = s.Roll.HasValue && s.Pitch.HasValue && s.Heading.HasValue;
            flyioHudUnavailable.Visible = !attitudeValid; hud1.Visible = attitudeValid;
            if (!attitudeValid) flyioHudUnavailable.BringToFront();
            hud1.connected = s.Connected; hud1.mode = s.Mode ?? "Unavailable";
            hud1.message = s.Connected ? s.Warning ?? "" : "TELEMETRY UNAVAILABLE";
            hud1.displayspeed = s.Airspeed.HasValue;
            hud1.displayalt = s.RelativeAltitude.HasValue;
            hud1.status = s.Armed ?? false;
            hud1.roll = (float)(s.Roll ?? double.NaN); hud1.pitch = (float)(s.Pitch ?? double.NaN);
            hud1.heading = (float)(s.Heading ?? double.NaN); hud1.alt = (float)(s.RelativeAltitude ?? double.NaN);
            hud1.groundspeed = (float)(s.Groundspeed ?? double.NaN); hud1.airspeed = (float)(s.Airspeed ?? double.NaN);
            hud1.batterylevel = (float)(s.Voltage ?? double.NaN); hud1.batteryremaining = s.BatteryPercent ?? -1;
            hud1.gpsfix = s.GpsFix ?? 0;
            hud1.Invalidate();
            if (flyioSixPackLayout != null)
            {
                flyioAirspeed.SetValues((float)(s.Airspeed ?? double.NaN), (float)(s.Groundspeed ?? double.NaN), "m/s");
                flyioAttitude.SetValues((float)(s.Roll ?? double.NaN), (float)(s.Pitch ?? double.NaN), "deg");
                flyioAltimeter.SetValues((float)(s.RelativeAltitude ?? double.NaN), 0, "m");
                flyioHeading.SetValues((float)(s.Heading ?? double.NaN), 0, "deg");
                flyioTurn.SetValues(float.NaN, float.NaN, "Unavailable");
                flyioVerticalSpeed.SetValues((float)(s.Climb ?? double.NaN), 0, "m/s");
            }
            quickView1.desc = "REL ALT (m)"; quickView1.number = s.RelativeAltitude ?? double.NaN;
            quickView2.desc = "GROUND (m/s)"; quickView2.number = s.Groundspeed ?? double.NaN;
            quickView3.desc = "HEADING (deg)"; quickView3.number = s.Heading ?? double.NaN;
            quickView4.desc = "GPS SATELLITES"; quickView4.number = s.Satellites ?? double.NaN;
            quickView5.desc = "BATTERY (V)"; quickView5.number = s.Voltage ?? double.NaN;
            quickView6.desc = "BATTERY (%)"; quickView6.number = s.BatteryPercent ?? double.NaN;
            if (s.Latitude.HasValue && s.Longitude.HasValue)
            {
                var point = new PointLatLng(s.Latitude.Value, s.Longitude.Value);
                if (flyioVehicleMarker == null)
                {
                    flyioVehicleMarker = new GMapMarkerPlane(0, point, (float)(s.Heading ?? 0), -1, -1, -1, -1);
                    flyioVehicleOverlay.Markers.Add(flyioVehicleMarker);
                }
                flyioVehicleMarker.Position = point;
                flyioVehicleMarker.Heading = (float)(s.Heading ?? 0);
                flyioVehicleMarker.ToolTipText = "PX4 " + s.SystemId + " · " + (s.Mode ?? "Unavailable");
                if (Visible && (!flyioCentered || flyioFollowButton.Checked))
                {
                    gMapControl1.Position = point;
                    if (!flyioCentered) gMapControl1.Zoom = 17;
                    flyioCentered = true;
                }
            }
            else { flyioVehicleOverlay.Markers.Clear(); flyioVehicleMarker = null; flyioCentered = false; }
            gMapControl1.Invalidate();
            // Local evidence only; no server, network writes, or flight-control path.
            if ((DateTime.UtcNow - flyioLastDiagnostic).TotalSeconds >= 1)
            {
                flyioLastDiagnostic = DateTime.UtcNow;
                try { File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "flyio-telemetry.json"), Newtonsoft.Json.JsonConvert.SerializeObject(s, Newtonsoft.Json.Formatting.Indented)); }
                catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }
    }
}
