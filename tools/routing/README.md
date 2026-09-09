# Local PX4 MAVLink routing

Run these commands in Ubuntu-24.04 (or prefix with `wsl.exe -d Ubuntu-24.04 --` from Windows):

```bash
bash /mnt/c/Flyio-GCS/tools/routing/router.sh start
bash /mnt/c/Flyio-GCS/tools/routing/router.sh status
bash /mnt/c/Flyio-GCS/tools/routing/router.sh stop
bash /mnt/c/Flyio-GCS/tools/routing/router.sh restart
```

`start` is idempotent. Lifecycle operations are serialized and a supervisor holds
the single-instance lock for the daemon's lifetime. Stop uses SIGTERM and checks
the executable identity before signalling the recorded PID. Startup refuses an
occupied WSL UDP 14550. The launcher exits with an error if startup/stop fails.
No service, WSL boot settings, or automatic vehicle startup is installed. Start
the router explicitly after WSL starts. A daemon crash is reported by `status`;
restart explicitly after diagnosing the log. The Windows gateway is rediscovered
on every start, including `restart` after network changes.

## Installed binary

- Release: upstream mavlink-router **v4**, glibc x86-64.
- URL: https://github.com/mavlink-router/mavlink-router/releases/download/v4/mavlink-routerd-glibc-x86_64
- Executable: `~/.local/lib/flyio-mavlink-router/v4/mavlink-routerd`
- SHA256: `785b9d653dd13f47716cd0c4a74f68126d39dfa3abb2e91ed60245122c5266a1`
- `--version` verified as `mavlink-router version v4`.
- No apt packages, source checkout, or dependency updates required.

The launcher checks this observed binary hash on every start. The hash records
the downloaded release artifact; it is not an independent publisher signature.

## Endpoints and protection

```text
PX4 127.0.0.1:18570 <-> router WSL 127.0.0.1:14550
                         | <-> Windows gateway:14550 QGroundControl
                         | --> Windows gateway:14551 Fly.io
```

The router's normal UDP endpoints use ephemeral WSL source ports, shown by
`status`. QGC replies to its endpoint's source port; the router forwards those
messages to PX4's learned localhost:18570 peer. PX4 startup is unchanged.
Windows and WSL port 14550 are separate sockets. Never bind a second Windows app
to QGC's port or run the Python helper alongside this router.

`router.conf.in` is expanded into `~/.cache/flyio-mavlink-router/main.conf`.
`BlockSrcSysIn` on FlyioTelemetry enumerates all 256 source IDs; therefore all
MAVLink entering that endpoint is discarded. Outbound telemetry is restricted
to vehicle system ID 1. Fly.io itself remains receive-only. TCP and flight-stack
logging are disabled. QGC normal protocol traffic is bidirectional; do not use
vehicle-control actions during receive-only validation.

The seven required telemetry messages are broadcasts and reach Fly.io without
a GCS heartbeat from it. Responses addressed specifically to QGC need not be
copied to Fly.io. Unknown message forwarding by the router does not add decoding
support to the browser's older MAVLink definitions.

Runtime PID/lock/config/log files live in `~/.cache/flyio-mavlink-router`, outside
both repositories. The launcher retains the previous run's log as
`router.previous.log`; `ReportStats` makes the current log grow during a run.
Use this configuration for attended local SITL work, not unattended deployment.

## Validation performed

On 2026-09-05: binary compatibility, live cutover, idempotent start, QGC heartbeat
and PING return traffic observed by PX4, Fly.io message reception, PX4 restart
without router restart, and router restart without PX4 restart passed.

The isolation test briefly substituted a temporary Windows UDP 14551 socket for
the stopped backend. It sent ten harmless GCS HEARTBEATs (system 253/component
190, sequence 77, autopilot INVALID), never a command. `observe-isolation.py`
passively observed all ten at the router input, zero at PX4:18570, and concurrent
QGC return traffic as a positive control. The backend was then restarted.
The observer requires root for AF_PACKET and sends no traffic itself. It accepts
the exact test-frame base64 as its only argument:

```bash
sudo python3 /mnt/c/Flyio-GCS/tools/routing/observe-isolation.py \
  /QkAAE39vgAAAAAAAAAGCAAAA7zb
```

Do not send test packets into a running backend's bound socket. Repeating the
active test requires arranging a brief backend stop and an exclusive Windows
14551 test socket. Loopback capture counts may include both directions of the
same captured datagram; the marker's absence at PX4 and positive input capture
are the assertions, not a PX4 receive-rate estimate.

## Rollback

Stop the router with the command above. Confirm `status` reports stopped and
`ss -ulnp 'sport = :14550'` shows no WSL listener. Then run the previously tested
one-way helper below. Leave PX4 and Windows endpoints unchanged. This is a
temporary recovery option, not the final router.

```bash
python3 - <<'PY'
import socket
import subprocess
route = subprocess.check_output(['ip', '-4', 'route', 'show', 'default'], text=True).split()
gateway = route[route.index('via') + 1]
with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as receiver, \
     socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as sender:
    receiver.bind(('127.0.0.1', 14550))
    try:
        while True:
            data, source = receiver.recvfrom(65535)
            if source != ('127.0.0.1', 18570):
                continue
            for port in (14550, 14551):
                sender.sendto(data, (gateway, port))
    except KeyboardInterrupt:
        pass
PY
```

Stop this foreground helper with Ctrl+C before starting mavlink-router again.

## Deferred Phase D work (not applied)

- GPS-derived altitude requires an appropriate 3D fix.
- BATTERY_STATUS extension voltages use zero as their unavailable marker.
- Check CUSTOM_MODE_ENABLED before interpreting custom mode.
- Optional web-only definitions for LINK_NODE_STATUS (8) and
  TIME_ESTIMATE_TO_TARGET (380). Desktop definitions remain unchanged.
- Proper simulated vehicle/autostart configuration and live GPS validation.
- No Fly.io ARM/DISARM, commands, modes, parameters, missions, or actuators.
