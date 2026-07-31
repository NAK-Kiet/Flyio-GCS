using System.Diagnostics;

const string url = "http://localhost:5000";
var start = DateTime.UtcNow;

const string HomePage = """
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Fly.io GCS</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  :root { color-scheme: dark; }
  body { font-family: "Segoe UI", system-ui, sans-serif; background: #14171c; color: #d7dbe0; height: 100vh; display: flex; flex-direction: column; overflow: hidden; }

  .titlebar { display: flex; align-items: center; gap: 14px; padding: 6px 12px; background: #1b2027; border-bottom: 1px solid #2a3038; }
  .titlebar .brand { display: flex; align-items: center; gap: 8px; font-weight: 700; color: #ff7b00; font-size: 15px; letter-spacing: .5px; }
  .titlebar .brand .logo { width: 18px; height: 18px; border-radius: 4px; background: linear-gradient(135deg,#ff7b00,#ffb000); }
  .titlebar .menu { display: flex; gap: 18px; margin-left: 18px; color: #9aa4af; font-size: 12px; }
  .titlebar .menu span:hover { color: #ff7b00; cursor: pointer; }
  .titlebar .version { margin-left: auto; color: #6b7480; font-size: 11px; }

  .tabs { display: flex; background: #191e24; border-bottom: 1px solid #2a3038; }
  .tabs .tab { padding: 9px 22px; font-size: 12px; font-weight: 600; letter-spacing: .5px; color: #8a939d; background: none; border: none; border-bottom: 3px solid transparent; cursor: pointer; }
  .tabs .tab:hover { color: #e6eaee; }
  .tabs .tab.active { color: #ff7b00; border-bottom-color: #ff7b00; background: #1d222a; }

  .stage { flex: 1; display: flex; flex-direction: column; min-height: 0; }

  .view { display: none; height: 100%; }
  .view.active { display: block; }

  .flightdata { display: grid; grid-template-columns: 1fr 1fr 300px; grid-template-rows: 1fr auto; gap: 1px; height: 100%; background: #2a3038; }

  .panel { background: #161a20; padding: 10px; overflow: hidden; position: relative; }
  .panel h3 { font-size: 10px; color: #6b7480; letter-spacing: 1.5px; text-transform: uppercase; margin-bottom: 8px; font-weight: 600; }

  .hud-panel { display: flex; flex-direction: column; }
  .hud { flex: 1; display: flex; align-items: center; justify-content: center; min-height: 0; }
  .hud svg { width: 100%; height: 100%; max-height: 440px; }

  .map-panel { position: relative; }
  .map-grid { position: absolute; inset: 0; background:
      linear-gradient(#232931 1px, transparent 1px) 0 0/40px 40px,
      linear-gradient(90deg, #232931 1px, transparent 1px) 0 0/40px 40px,
      #14181d; }
  .map-plane { position: absolute; top: 50%; left: 50%; width: 22px; height: 22px; transform: translate(-50%,-50%); color: #ff7b00; }
  .map-coords { position: absolute; bottom: 8px; left: 10px; font-size: 11px; color: #9aa4af; font-family: Consolas, monospace; }
  .map-note { position: absolute; top: 8px; right: 10px; font-size: 10px; color: #6b7480; }

  .telemetry { display: flex; flex-direction: column; gap: 8px; font-size: 12px; }
  .trow { display: flex; justify-content: space-between; align-items: baseline; }
  .trow .k { color: #8a939d; }
  .trow .v { font-weight: 600; color: #e6eaee; font-family: Consolas, monospace; }
  .trow .v.warn { color: #ffb000; }
  .trow .v.ok { color: #3fb950; }
  .mode-box { background: #ff7b00; color: #14171c; font-weight: 800; padding: 2px 8px; border-radius: 3px; font-size: 12px; }

  .statusbar { display: flex; gap: 22px; align-items: center; padding: 5px 14px; background: #1b2027; border-top: 1px solid #2a3038; font-size: 11px; color: #9aa4af; font-family: Consolas, monospace; }
  .statusbar .hb { width: 9px; height: 9px; border-radius: 50%; background: #3fb950; display: inline-block; }
  .statusbar .item b { color: #d7dbe0; font-weight: 600; }

  .placeholder { height: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 10px; color: #6b7480; font-size: 13px; }
  .placeholder .big { font-size: 26px; font-weight: 700; color: #8a939d; letter-spacing: 1px; }

  .att { position: relative; }
  .att .tape { position: absolute; width: 40px; background: #10141a; border: 1px solid #2a3038; border-radius: 4px; padding: 4px; top: 12px; bottom: 12px; display: flex; flex-direction: column; align-items: center; justify-content: space-around; font-family: Consolas, monospace; font-size: 11px; }
  .att .tape.left { left: 10px; }
  .att .tape.right { right: 10px; }
  .att .tape .v { color: #ff7b00; font-weight: 700; font-size: 15px; }
  .att .tape .l { color: #6b7480; font-size: 9px; text-transform: uppercase; letter-spacing: 1px; }
</style>
</head>
<body>

<div class="titlebar">
  <div class="brand"><span class="logo"></span>FLY.IO</div>
  <div class="menu">
    <span>Menu</span><span>Setup</span><span>Config</span><span>Sim</span><span>Frame</span><span>Status</span>
  </div>
  <div class="version">v1.3.83</div>
</div>

<div class="tabs">
  <button class="tab active" data-tab="flightdata">FLIGHT DATA</button>
  <button class="tab" data-tab="flightplan">FLIGHT PLAN</button>
  <button class="tab" data-tab="setup">INITIAL SETUP</button>
  <button class="tab" data-tab="config">CONFIGURATION</button>
  <button class="tab" data-tab="sim">SIMULATION</button>
</div>

<div class="stage">

  <section class="view active" id="flightdata">
    <div class="flightdata">
      <div class="panel hud-panel">
        <h3>Attitude Indicator</h3>
        <div class="hud">
          <svg viewBox="0 0 400 300" xmlns="http://www.w3.org/2000/svg">
            <defs>
              <linearGradient id="sky" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stop-color="#2e5f8a"/><stop offset="100%" stop-color="#7db5dd"/></linearGradient>
              <linearGradient id="gnd" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stop-color="#7a4a1e"/><stop offset="100%" stop-color="#3a2410"/></linearGradient>
              <clipPath id="dial"><circle cx="200" cy="150" r="120"/></clipPath>
            </defs>
            <circle cx="200" cy="150" r="120" fill="#0c0f13" stroke="#3a414b" stroke-width="3"/>
            <g clip-path="url(#dial)">
              <g id="horizon">
                <rect x="-260" y="-260" width="520" height="260" fill="url(#sky)"/>
                <rect x="-260" y="0" width="520" height="260" fill="url(#gnd)"/>
                <line x1="-260" y1="0" x2="260" y2="0" stroke="#f4f4f4" stroke-width="2"/>
                <g id="pitch" stroke="#f4f4f4" stroke-width="1.5" fill="none" opacity="0.9">
                  <line x1="150" y1="-40" x2="250" y2="-40"/><text x="142" y="-36" font-size="11" fill="#f4f4f4" stroke="none">-20</text>
                  <line x1="165" y1="-20" x2="235" y2="-20"/><text x="157" y="-16" font-size="11" fill="#f4f4f4" stroke="none">-10</text>
                  <line x1="180" y1="0" x2="220" y2="0"/>
                  <line x1="165" y1="20" x2="235" y2="20"/><text x="157" y="24" font-size="11" fill="#f4f4f4" stroke="none">10</text>
                  <line x1="150" y1="40" x2="250" y2="40"/><text x="142" y="44" font-size="11" fill="#f4f4f4" stroke="none">20</text>
                  <line x1="140" y1="60" x2="260" y2="60"/><text x="132" y="64" font-size="11" fill="#f4f4f4" stroke="none">30</text>
                  <line x1="130" y1="80" x2="270" y2="80"/><text x="122" y="84" font-size="11" fill="#f4f4f4" stroke="none">40</text>
                </g>
              </g>
            </g>
            <g stroke="#ff7b00" stroke-width="2" fill="none">
              <polygon points="200,118 212,138 188,138" fill="#ff7b00" stroke="none"/>
              <line x1="200" y1="130" x2="200" y2="150" opacity="0"/>
              <path d="M 200 168 L 172 184 L 228 184 Z" fill="#ff7b00" stroke="none"/>
              <line x1="200" y1="0" x2="200" y2="30"/>
              <line x1="160" y1="38" x2="240" y2="38"/>
              <path d="M 130 150 L 160 150 M 240 150 L 270 150" stroke-width="3"/>
              <line x1="200" y1="270" x2="200" y2="282"/>
            </g>
            <text id="hdgTxt" x="200" y="26" text-anchor="middle" font-size="13" fill="#f4f4f4" font-family="Consolas, monospace">0&deg;</text>
          </svg>
        </div>
      </div>

      <div class="panel map-panel">
        <h3>Map</h3>
        <div class="map-grid"></div>
        <svg class="map-plane" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2l2 8 8 2-8 2-2 8-2-8-8-2 8-2z"/></svg>
        <div class="map-coords" id="mapCoords">GPS: acquiring...</div>
        <div class="map-note">Fly.io Flight Data</div>
      </div>

      <div class="panel telemetry">
        <h3>Telemetry</h3>
        <div class="trow"><span class="k">MODE</span><span class="mode-box" id="tMode">AUTO</span></div>
        <div class="trow"><span class="k">ARMED</span><span class="v" id="tArmed">NO</span></div>
        <div class="trow"><span class="k">ALT (m)</span><span class="v" id="tAlt">-</span></div>
        <div class="trow"><span class="k">GSPD (m/s)</span><span class="v" id="tSpeed">-</span></div>
        <div class="trow"><span class="k">HDG</span><span class="v" id="tHdg">-</span></div>
        <div class="trow"><span class="k">ROLL / PITCH</span><span class="v" id="tAtt">-</span></div>
        <div class="trow"><span class="k">BATT (V)</span><span class="v" id="tVolt">-</span></div>
        <div class="trow"><span class="k">BATT (%)</span><span class="v" id="tPct">-</span></div>
        <div class="trow"><span class="k">RSSI</span><span class="v" id="tRssi">-</span></div>
        <div class="trow"><span class="k">GPS SATS</span><span class="v" id="tSats">-</span></div>
      </div>

      <div class="panel att" style="grid-column: 1 / 4;">
        <h3>Flight Data</h3>
        <div class="att">
          <div class="tape left"><span class="v" id="sAlt">0.0</span><span class="l">ALT m</span></div>
          <div class="tape right"><span class="v" id="sSpeed">0.0</span><span class="l">GSPD m/s</span></div>
          <div style="color:#8a939d; text-align:center; font-size:11px;">GPS: <span id="sGps">3D FIX</span> &nbsp;|&nbsp; Battery: <span id="sBatt">-</span> &nbsp;|&nbsp; RSSI: <span id="sRssi">-</span> &nbsp;|&nbsp; Mode: <span id="sMode">AUTO</span></div>
        </div>
      </div>
    </div>
  </section>

  <section class="view" id="flightplan">
    <div class="placeholder"><div class="big">FLIGHT PLAN</div><div>Waypoint mission planning coming soon</div></div>
  </section>
  <section class="view" id="setup">
    <div class="placeholder"><div class="big">INITIAL SETUP</div><div>Vehicle setup wizard coming soon</div></div>
  </section>
  <section class="view" id="config">
    <div class="placeholder"><div class="big">CONFIGURATION</div><div>Full parameter editor coming soon</div></div>
  </section>
  <section class="view" id="sim">
    <div class="placeholder"><div class="big">SIMULATION</div><div>SITL simulator coming soon</div></div>
  </section>

</div>

<div class="statusbar">
  <span class="item">COM: <b>--</b></span>
  <span class="item">BAUD: <b>115200</b></span>
  <span class="item"><span class="hb" id="hb"></span> HEARTBEAT</span>
  <span class="item">GPS: <b id="sbGps">3D FIX</b></span>
  <span class="item" id="sbLatLon">LAT/LON: --</span>
  <span class="item">ALT: <b id="sbAlt">0.0 m</b></span>
  <span class="item">RSSI: <b id="sbRssi">--</b></span>
  <span class="item" style="margin-left:auto;color:#6b7480;" id="sbClock">--</span>
</div>

<script>
  const $ = id => document.getElementById(id);

  document.querySelectorAll('.tab').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('.tab').forEach(b => b.classList.toggle('active', b === btn));
      document.querySelectorAll('.view').forEach(v => v.classList.toggle('active', v.id === btn.dataset.tab));
    });
  });

  async function tick() {
    try {
      const r = await fetch('/api/telemetry');
      const d = await r.json();
      const roll = d.roll, pitch = d.pitch;

      const horizon = $('horizon');
      horizon.setAttribute('transform', `rotate(${roll} 200 150) translate(0 ${pitch * 3}) rotate(${-roll} 200 150)`);

      $('hdgTxt').textContent = d.heading.toFixed(0) + '\u00b0';
      $('tMode').textContent = d.mode;
      $('tArmed').textContent = d.armed ? 'YES' : 'NO';
      $('tArmed').className = 'v' + (d.armed ? ' warn' : ' ok');
      $('tAlt').textContent = d.altitude.toFixed(1);
      $('tSpeed').textContent = d.speed.toFixed(1);
      $('tHdg').textContent = d.heading.toFixed(0) + '\u00b0';
      $('tAtt').textContent = d.roll.toFixed(1) + ' / ' + d.pitch.toFixed(1);
      $('tVolt').textContent = d.batteryVolt.toFixed(2) + ' V';
      $('tPct').textContent = d.batteryPct.toFixed(0) + '%';
      $('tRssi').textContent = d.rssi + ' dBm';
      $('tSats').textContent = d.gpsSats;

      $('sAlt').textContent = d.altitude.toFixed(1);
      $('sSpeed').textContent = d.speed.toFixed(1);
      $('sBatt').textContent = d.batteryVolt.toFixed(2) + 'V ' + d.batteryPct.toFixed(0) + '%';
      $('sRssi').textContent = d.rssi + ' dBm';
      $('sMode').textContent = d.mode;
      $('sGps').textContent = d.gpsFix >= 3 ? '3D FIX' : 'NO FIX';

      $('sbGps').textContent = d.gpsFix >= 3 ? '3D FIX' : 'NO FIX';
      $('sbAlt').textContent = d.altitude.toFixed(1) + ' m';
      $('sbRssi').textContent = d.rssi + ' dBm';
      $('sbLatLon').textContent = 'LAT/LON: ' + d.lat.toFixed(6) + ', ' + d.lon.toFixed(6);
      $('mapCoords').textContent = 'GPS: ' + d.lat.toFixed(6) + ', ' + d.lon.toFixed(6);

      $('hb').style.background = (Date.now() / 1000) % 1 < 0.5 ? '#3fb950' : '#1f6f33';
    } catch { $('hb').style.background = '#f85149'; }
  }

  setInterval(() => { $('sbClock').textContent = new Date().toLocaleTimeString(); }, 1000);
  setInterval(tick, 1000);
  tick();
</script>
</body>
</html>
""";

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(url);
var app = builder.Build();

app.MapGet("/", () => Results.Content(HomePage, "text/html"));

var rng = new Random();
double heading = 0;
double altitude = 120.0;
double speed = 15.0;
double batt = 15.4;
double battPct = 85.0;
int rssi = -58;
const int sats = 12;
const string mode = "AUTO";

app.MapGet("/api/telemetry", () =>
{
    var t = DateTime.UtcNow;
    heading = (heading + 0.9) % 360;
    var roll = Math.Sin(t.Ticks / 8_000_000.0) * 10 + (rng.NextDouble() - 0.5) * 2;
    var pitch = Math.Sin(t.Ticks / 13_000_000.0) * 3 + (rng.NextDouble() - 0.5);
    altitude = Math.Clamp(altitude + (rng.NextDouble() - 0.5) * 0.7, 100, 140);
    speed = Math.Clamp(speed + (rng.NextDouble() - 0.5) * 1.2, 12, 20);
    batt = Math.Max(12.4, batt - 0.00003);
    battPct = Math.Max(20, battPct - 0.0002);
    rssi = -58 + rng.Next(0, 9) - 4;

    return Results.Json(new
    {
        altitude,
        speed,
        heading,
        roll,
        pitch,
        batteryVolt = batt,
        batteryPct = battPct,
        rssi,
        gpsSats = sats,
        gpsFix = 3,
        armed = false,
        mode,
        lat = -35.363262 + rng.NextDouble() * 0.0004,
        lon = 149.165237 + rng.NextDouble() * 0.0004
    });
});

app.MapGet("/api/status", () => Results.Json(new
{
    name = "Fly.io GCS",
    status = "online",
    version = "1.3.83",
    uptimeSeconds = (long)(DateTime.UtcNow - start).TotalSeconds,
    timeUtc = DateTime.UtcNow.ToString("O")
}));

Console.WriteLine($"Fly.io GCS running at {url}");
Console.WriteLine("Press Ctrl+C to stop.");

try
{
    _ = Task.Run(() =>
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch { }
    });
}
catch { }

app.Run();
