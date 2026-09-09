# Proposed control path — NOT ENABLED

The implemented desktop subscriber only receives UDP 14551. It has no send API,
does not become MainV2.comPort, and cannot issue commands. The router template
is unchanged from the preserved Alienware experiment: all 256 incoming system
IDs remain blocked on FlyioTelemetry. QGroundControl keeps its bidirectional
endpoint. No ARM, DISARM, or mode command has been transmitted by Fly.io.

## Approval boundary

User approval is required before implementing/enabling a route that admits
Fly.io commands. Do not remove BlockSrcSysIn from FlyioTelemetry.

Proposed addition, for review only (not loaded by router.sh):

```ini
[UdpEndpoint FlyioControl]
Mode = Normal
Address = @WINDOWS_HOST@
Port = 14552
AllowSrcSysIn = 252
AllowSrcCompIn = 190
AllowMsgIdIn = 76
AllowSrcSysOut = 1
AllowMsgIdOut = 77
```

This introduces Windows UDP 14552 for a dedicated command broker, leaving
18570, WSL 14550, QGC Windows 14550, and telemetry Windows 14551 intact.
Message 76 is COMMAND_LONG and message 77 is COMMAND_ACK. Router filters alone
cannot enforce the command payload allowlist, and system/component IDs are
not authentication. Verify v4 filter behavior and targeted ACK routing in SITL
before enabling this configuration.

## Broker responsibilities

Use a local named pipe restricted to the current Windows user, not a public
HTTP endpoint. The broker exclusively owns UDP 14552; Windows firewall rules
must limit inbound packets to the WSL router host. Pin the actual router peer.
Local processes able to impersonate the same user remain inside the trust
boundary; protect the WSL side equivalently. Stronger deployments require an
authenticated transport, not trust in MAVLink IDs.

Accept only ARM/DISARM (MAV_CMD_COMPONENT_ARM_DISARM, 400) and
MAV_CMD_DO_SET_MODE (176), with explicit target 1/1, documented PX4 mode
encoding, all unused parameters zero, and no force-arm/disarm magic values.
Initially allow Manual, Altitude, Position, Stabilized, and Hold only. Reject
takeoff, land, mission, actuator/servo, parameter, offboard, and other commands.

Require an explicit SITL session established locally, fresh autopilot heartbeat
(under 3 seconds), a connected vehicle, stable telemetry, valid GPS/global
position, and known preflight readiness before ARM. Unavailable preflight state
must not be treated as healthy. Require clear per-action ARM/DISARM confirmation
showing target, current mode/state, and intended change; consume each short-lived
confirmation token once. Reject stale state again immediately before sending.

Allow one pending command at a time, send once, and never periodically retry.
Correlate COMMAND_ACK with target/source, command ID, and the pending time window.
Display accepted, denied, unsupported, failed, in-progress, or timed-out result.
An accepted ACK is not proof that the requested state changed: verify subsequent
heartbeat armed/mode state separately. Log request, confirmation, transmission,
ACK, observed state, timeout, and rejection. Disconnect cancels pending requests.

## Required tests after approval

On PX4 SITL only: confirmation cancellation, stale heartbeat, unknown preflight,
wrong identity, invalid payload, duplicate click, ACK timeout/denial, actual state
change, QGC concurrent traffic, and router/PX4 restarts. Prove arbitrary
COMMAND_LONG payloads cannot cross the broker. Re-run ingress isolation checks
to prove UDP 14551 remains blocked. No hardware testing is authorized here.
