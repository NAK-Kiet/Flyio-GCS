# Desktop PX4 telemetry

Use Ubuntu-24.04 and only `/home/alien/PX4-Autopilot`:

```bash
cd /home/alien/PX4-Autopilot
make px4_sitl_default sihsim_quadx
```

In a second WSL shell:

```bash
bash /mnt/c/Flyio-GCS/tools/routing/router.sh start
bash /mnt/c/Flyio-GCS/tools/routing/router.sh status
```

Build `dotnet build MissionPlanner.csproj -c Release` and launch
`bin/Release/net461/Fly.io-GCS.exe`. DATA's VEHICLE HEALTH tab selects the passive
PX4 listener on UDP 14551 by default. QGroundControl retains Windows UDP 14550.
Do not connect the desktop legacy serial/UDP controls to this passive session.
Uncheck the receive-only selector to release 14551; recheck to retry a bind error.
The experimental web listener and desktop cannot own 14551 simultaneously.

The adapter reuses the repository's MAVLink decoder and existing GMap map.
It selects a PX4 autopilot heartbeat, rejects bad CRCs, consumes only its selected
system/component, and uses battery ID 0. GPS/attitude/global-position expire in
3 seconds, system/battery/estimator/radio in 5 seconds, and status text in 30
seconds. Heartbeat loss clears all displayed live fields and removes the marker.
Recovery clears previous session values before accepting new readings.
No heartbeat, stream requests, parameters, or commands are sent by this adapter.
The Fly.io desktop build also suppresses Mission Planner's legacy AutoConnect
listeners on UDP 14550/14551, leaving 14550 exclusively to QGroundControl and
14551 exclusively to this passive subscriber.

`bin/Release/net461/flyio-telemetry.json` is local live diagnostic evidence,
updated once per second. Counts are cumulative accepted packets by MAVLink ID.
Rejected packets include unsupported messages from PX4's newer dialect, not just
corrupt frames. No decoder definitions were upgraded in this milestone.

Unknown battery, airspeed, RSSI, prearm status, and other values remain unavailable.
Estimator status flags are reported separately from prearm readiness. This PX4
session does not advertise the SYS_STATUS prearm-check bit; a healthy estimator
must not be presented as permission to arm. Recent STATUSTEXT is shown with its
severity, but it is not a complete PX4 event-history/preflight checklist.

Focused tests:

```powershell
dotnet build tools/telemetry/TelemetryChecks.csproj -c Release
& tools/telemetry/bin/Release/net472/TelemetryChecks.exe
```

The tests inject packets directly into the decoder in memory; they transmit no
network traffic. `--live` instead passively observes UDP 14551 for 30 seconds;
stop the desktop/web listener first. Outbound control remains blocked pending
the separate review in [CONTROL-DESIGN.md](CONTROL-DESIGN.md).
