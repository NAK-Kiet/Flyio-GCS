# Fly.io GCS

Fly.io GCS is a DRDC ground control station for UAV and UGV systems.

## Current Features

- Flight Data / HUD, aviation gauges, tuning graph, map and vehicle monitoring
- Mission planning, vehicle setup and configuration through existing workflows
- Simulation workspace and external PX4 SITL testing
- MAVLink telemetry with vehicle health/status and unavailable-data indicators
- Receive-only PX4 telemetry and QGroundControl coexistence
- HELP resources, keyboard shortcuts and embedded DRDC services
- Custom Fly.io dark/orange UI with the supplied navigation artwork
- Portable Windows Release package containing the required runtime files

Vehicle-dependent features require the appropriate connection and supported firmware.
The PX4 receive-only session does not provide outbound vehicle control.

## Navigation

| Page | Purpose |
| --- | --- |
| DATA | HUD, map, telemetry, gauges, tuning graph and connection/status information |
| PLAN | Existing mission planning workspace |
| SETUP | Existing vehicle and hardware setup pages |
| CONFIG | Application options and vehicle parameter screens |
| SIMULATION | Existing simulation controls |
| HELP | DRDC services, shortcuts, product information and upstream acknowledgements |
| DRDC | Embedded DRDC GCS service; sign-in requires an account |

The approved navigation uses proportional transparent icons, white text and Fly.io
orange hover/selected accents. Operational colors retain their semantic meanings.
F2 opens DATA and F3 opens PLAN.

## PX4 / MAVLink Development

The existing development setup has been validated with PX4 SITL and MAVLink
telemetry alongside QGroundControl. QGroundControl uses UDP 14550; Fly.io GCS
passively receives the routed telemetry on UDP 14551. This product/content pass
preserves that architecture and does not reconfigure the simulator or routing.

The passive session reports heartbeat, flight mode, arm state, attitude, GPS,
satellites, heading, ground speed, battery and available estimator/status data.
Unavailable or expired values must remain unavailable. An open UDP listener alone
does not establish a vehicle connection. The passive adapter sends no heartbeat,
stream requests, parameters or vehicle commands. Do not use legacy connection
controls to attach to the same passive session.

See [desktop telemetry setup and tests](tools/telemetry/README.md) and
[routing documentation](tools/routing/README.md) for the existing development
instructions. PX4 runs externally; the SIMULATION page retains its existing
workflows. No integrated PX4 launcher or new ARM, LAND, RTL, AUTO, TAKEOFF or
mission-execution capability is implied.

The existing in-memory telemetry suite can be run without transmitting traffic:

```powershell
dotnet build tools/telemetry/TelemetryChecks.csproj -c Release
.\tools\telemetry\bin\Release\net472\TelemetryChecks.exe
```

Live PX4/QGroundControl coexistence must be checked separately when that setup is
running; packet tests and page checks are not a substitute for a live test.

## Windows Release

Extract and copy the **whole** `Fly.io-GCS-Windows` folder to a writable local
folder, then launch `Fly.io-GCS.exe`. Do not copy only the EXE or run inside the
ZIP. Keep its `.config`, managed/native DLLs, resource folders and license notices
together. No source checkout or .NET SDK is needed to run the package.

Generated package locations:

```text
release\Fly.io-GCS-Windows\Fly.io-GCS.exe
release\Fly.io-GCS-Windows.zip
```

Team prerequisites:

- Windows desktop and .NET Framework 4.7.2 or a compatible later 4.x runtime
- Microsoft Edge WebView2 Runtime for the embedded DRDC service
- Visual C++ v14 runtimes appropriate for the native components being used
- Working graphics drivers; vendor USB/serial drivers when using that hardware
- Network access for online maps and DRDC services; an account for DRDC sign-in

The application is AnyCPU with architecture-specific native dependencies. The
package is tested on a Windows x64 laptop. Optional native/video/hardware features
need validation on the target laptop. A different-folder launch on the build
laptop does not prove a fresh Windows installation has every prerequisite.
Application/user settings retain their existing storage locations for compatibility.
The existing update policy remains unchanged.

## Build

Develop on Windows with Git, a .NET SDK capable of building SDK-style .NET
Framework projects, .NET Framework 4.7.2 targeting/reference assemblies, PowerShell
and network access for NuGet restore. Visual Studio with .NET desktop development
and the 4.7.2 targeting pack is an optional development environment. Obtain the
repository and its required dependencies through your team's authorized access.

Close Fly.io GCS before building to avoid locked DLLs:

```powershell
dotnet restore MissionPlanner.csproj
dotnet build MissionPlanner.csproj -c Release
```

After a successful restore, `--no-restore` can be used for subsequent builds.
The exact output is `bin\Release\net461\Fly.io-GCS.exe`. The historical `net461`
folder name does not change the actual .NET Framework 4.7.2 target.
`MissionPlanner.csproj`, internal MissionPlanner namespaces and compatibility
identifiers remain intentional. `Flyio-GCS.csproj` is another native project entry.
The desktop application does not use the historical `web/` prototype.

Use `run-scripts/package-release.ps1` to prepare a clean runtime package from an
isolated build. Do not zip a development output folder containing local settings,
logs or cached data. Inspect the resulting package and launch a copied package
outside the repository before sharing it. Generated outputs are not source commits.

If restore fails, inspect the first error and confirm NuGet connectivity and the
4.7.2 targeting pack. If build outputs are locked, close the application and retry.
Do not replace repository dependencies to suppress errors. Serial connections
require the correct port/baud rate and exclusive access to that serial port; this
is distinct from the validated routed UDP coexistence setup.

## DRDC

Official website: https://drdc-dev-web.drsys.asia/

The HELP website button and clickable Fly.io brand open this address. The separate
DRDC dashboard integration retains its existing service and authentication flow.

## Upstream / Third-Party Acknowledgements

Fly.io GCS is derived from / based on Mission Planner components in the ArduPilot
ecosystem. Original authors and contributors retain their copyrights. Fly.io
branding does not imply authorship or exclusive ownership of upstream code.

Required licensing, attribution, dependency notices and contributor acknowledgements
are retained. See [COPYING.txt](COPYING.txt), [NOTICE-Fly.io-GCS.txt](NOTICE-Fly.io-GCS.txt),
source-file notices and the packaged third-party notices. Binary redistribution
must satisfy the applicable licenses, including their corresponding-source
requirements. Product wording does not change those obligations.

- [Upstream Mission Planner](https://github.com/ArduPilot/MissionPlanner)
- [Mission Planner contributors](https://github.com/ArduPilot/MissionPlanner/graphs/contributors)
- [ArduPilot documentation](https://ardupilot.org/ardupilot/)
