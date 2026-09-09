# FLY.IO GCS

Fly.io Ground Control Station for Drone Operations, Development, Monitoring,
Mission Planning, Simulation, and System Integration.

Fly.io GCS is the customized ground control station used for DRDC drone research,
integration, and operational workflows. It is a Windows desktop ground control
station for supported vehicles, telemetry, missions, configuration, simulation,
and DRDC services.

## What the application provides

| Page | Purpose |
| --- | --- |
| **DATA** | Flight data, HUD, flight instruments, gauges, map, telemetry graph, connection controls, and vehicle status. |
| **PLAN** | Mission planning, waypoint editing, map-based planning, and mission files. |
| **SETUP** | Firmware, hardware, calibration, and vehicle setup workflows. |
| **CONFIG** | Planner options, display settings, telemetry rates, parameters, and application configuration. |
| **SIMULATION** | The existing simulation workspace and simulator selection controls. |
| **HELP** | Product help, keyboard shortcuts, DRDC services, and upstream acknowledgements. |
| **DRDC** | Embedded DRDC ground control service; sign-in requires an account. |

The DATA page includes standard Fly.io/Mission Planner vehicle workflows:
MAVLink-compatible telemetry, heartbeat and vehicle state, flight mode, arm state,
attitude, GPS and satellite information, heading, ground speed, battery data,
status indicators, HUD instruments, gauges, an interactive map, and a tuning graph.
Available values depend on the connected vehicle and telemetry stream.

## Quick Start

### I only want to use Fly.io GCS

1. Obtain `Fly.io-GCS-Windows.zip`.
2. Extract the complete ZIP.
3. Open the extracted folder.
4. Run `Fly.io-GCS.exe`.

Do **not** copy only `Fly.io-GCS.exe`; supporting DLLs and resources are required.

### I want to develop Fly.io GCS

```powershell
git clone --recurse-submodules https://github.com/NAK-Kiet/Flyio-GCS.git
cd Flyio-GCS
git submodule update --init --recursive
dotnet restore MissionPlanner.csproj
dotnet build MissionPlanner.csproj -c Release --no-restore
```

## Use the Windows Release

Normal operators should not need to clone or compile the repository. The
recommended workflow is:

~~~text
Fly.io-GCS-Windows.zip
        |
Extract the complete archive
        |
Open the extracted folder
        |
Run Fly.io-GCS.exe
~~~

The package includes required DLLs, resources, configuration files, native
dependencies, licenses, and notices. Do not distribute the EXE alone. Obtain
Fly.io-GCS-Windows.zip from the DRDC team and keep the complete extracted archive
together.

~~~text
Fly.io-GCS.exe
~~~

Do not copy only the executable. The application requires the supporting files in
the package. The current development output is:

~~~text
bin\Release\net461\Fly.io-GCS.exe
~~~

The complete Windows package is the recommended operator distribution.

## Platform support

| OS | Fly.io GCS desktop | Development |
| --- | --- | --- |
| Windows 10/11 | Native / recommended | Supported development workflow. |
| Linux | Not native | Clone and edit with Git and VS Code; use a Windows VM or remote Windows machine to run the desktop application. |
| macOS | Not native | Clone and edit the source; use a Windows VM or remote Windows machine to run the desktop application. |

VS Code is an editor and development environment. It does not make the Windows
WinForms application run natively on Linux or macOS. Wine may be experimented
with, but it is experimental and unvalidated; reliable serial, USB, graphics, or
WebView2 operation under Wine is not promised.

## Clone from GitHub

Repository: <https://github.com/NAK-Kiet/Flyio-GCS>

~~~powershell
git clone --recurse-submodules https://github.com/NAK-Kiet/Flyio-GCS.git
cd Flyio-GCS
git submodule update --init --recursive
code .
~~~

The `git submodule update --init --recursive` command is important because
inherited Mission Planner dependencies and submodules may be required for a
complete development checkout. The `code .` command requires Visual Studio Code
and its command to be available in PATH. From a PowerShell prompt starting at the
C drive:

~~~powershell
cd C:\
git clone --recurse-submodules https://github.com/NAK-Kiet/Flyio-GCS.git
cd Flyio-GCS
git submodule update --init --recursive
code .
~~~

## Clone from DRDC GitLab

This is an **INTERNAL/PRIVATE** GitLab repository. Users need approved DRDC or
internal network access to the private address `192.168.100.120`, GitLab
repository permission, and a registered SSH public key.

~~~powershell
git clone --recurse-submodules git@192.168.100.120:Kaine/flyio-gcs.git Flyio-GCS
cd Flyio-GCS
git submodule update --init --recursive
code .
~~~

Never place private key contents, passphrases, tokens, or passwords in this
repository or its documentation.

## Development requirements

For source development, use Windows 10/11 with Git, PowerShell, NuGet network
access, .NET SDK tooling for SDK-style .NET Framework projects, and .NET Framework
4.7.2 targeting/reference assemblies. The currently validated repository workflow
uses PowerShell or terminal commands with `dotnet restore` and `dotnet build`.
Visual Studio can also be used with appropriate .NET desktop and .NET Framework
tooling. VS Code is suitable for editing, Git operations, and terminal-based
workflows. The desktop runtime remains Windows-native.

## Build from source

~~~powershell
cd C:\Flyio-GCS
dotnet restore MissionPlanner.csproj
dotnet build MissionPlanner.csproj -c Release --no-restore
~~~

A successful build should report 0 Error(s) and produce:

~~~text
C:\Flyio-GCS\bin\Release\net461\Fly.io-GCS.exe
~~~

MissionPlanner.csproj targets net472 (.NET Framework 4.7.2), even though
bin\Release\net461 is a historical output folder name. MissionPlanner.csproj,
internal Mission Planner namespaces, and
compatibility identifiers remain because Fly.io GCS originated from the Mission
Planner codebase. The user-facing product is Fly.io GCS and the executable is
Fly.io-GCS.exe.

| Internal compatibility name | User-facing name |
| --- | --- |
| MissionPlanner.csproj | Fly.io GCS |
| Mission Planner namespaces | Fly.io GCS desktop application |
| bin\Release\net461\ | Windows Release output folder |
| Fly.io-GCS.exe | Fly.io GCS executable |

## Run from source

~~~powershell
cd C:\Flyio-GCS\bin\Release\net461
.\Fly.io-GCS.exe
~~~

Or:

~~~powershell
Start-Process "C:\Flyio-GCS\bin\Release\net461\Fly.io-GCS.exe"
~~~

Close the application before rebuilding when files are locked. Do not distribute
a development folder containing local settings, logs, caches, or build artifacts.

## Update an existing clone safely

~~~powershell
cd C:\Flyio-GCS
git status
git branch --show-current
git fetch origin
~~~

When the working tree is clean and the branch is main:

~~~powershell
git pull --ff-only origin main
~~~

If status shows important local changes, preserve or review them first. Do not
blindly pull, reset, clean, or discard work to force an update.

## Connect a vehicle

1. Connect telemetry or USB hardware and power the vehicle.
2. Start Fly.io GCS.
3. Select the correct COM port and baud rate.
4. Select **CONNECT**.
5. Confirm a heartbeat and vehicle identification.
6. Confirm attitude, GPS, battery, flight mode, and expected status telemetry.
7. Complete normal pre-flight and command-safety checks before operating.

Check physical connection, vehicle power, Windows device detection, telemetry
hardware, COM port, baud rate, and MAVLink configuration before changing firmware.
Test with the vehicle secured and follow the team's approved safety procedures.

## PX4 and QGroundControl development context

PX4 has been used during development and testing, but Fly.io GCS is not PX4-only.
The custom PX4 SITL presentation is not shown in the normal DATA UI in the current
release. PX4 is not required for normal Fly.io GCS operation.

QGroundControl is a separate GCS. Multi-GCS MAVLink testing may require validated
routing. This README does not change firmware or routing configuration.

## Architecture

~~~mermaid
flowchart LR
    V[Vehicle / Autopilot] -->|MAVLink| F[Fly.io GCS]
    F --> H[HUD and flight instruments]
    F --> M[Map and mission planning]
    F --> C[Vehicle setup and configuration]
    F --> D[DRDC services]
    P[Future / Planned capabilities] -.-> F
~~~

The future/planned node is a planning marker, not an implemented feature.

## Project structure

| Path | Purpose |
| --- | --- |
| GCSViews\ | DATA, PLAN, SETUP, CONFIG, SIMULATION, HELP, and DRDC views. |
| Controls\ | Reusable WinForms controls, instruments, quick views, and status widgets. |
| Utilities\ | Shared utilities and Fly.io telemetry support code. |
| Resources\ | Icons, imagery, themes, and embedded desktop resources. |
| Assets\ | Fly.io navigation artwork used by the desktop interface. |
| Properties\ | Application settings, resources, and manifest files. |
| ExtLibs\ | Inherited and third-party libraries, including MAVLink and map components. |
| tools\ | Development documentation, telemetry checks, and routing reference material. |
| run-scripts\ | Desktop review, build, and packaging helpers. |
| MissionPlanner.csproj | Compatibility project file used to build Fly.io GCS. |

## Troubleshooting

### Fly.io-GCS.exe is missing

~~~powershell
dotnet restore MissionPlanner.csproj
dotnet build MissionPlanner.csproj -c Release --no-restore
~~~

### project.assets.json or NETSDK errors

~~~powershell
dotnet restore MissionPlanner.csproj
~~~

Inspect the first error, confirm NuGet connectivity and the .NET Framework 4.7.2
targeting/reference assemblies, and do not replace dependencies just to suppress
an error.

### Git pull is blocked by local changes

~~~powershell
git status
git diff
~~~

Preserve important work; do not discard it blindly.

### Vehicle is not detected

Check physical connection, vehicle power, Windows device detection, COM port,
baud rate, telemetry hardware, and MAVLink configuration. Confirm another
application does not have exclusive access to the serial port.

## Release validation checklist

- Release build completes with 0 Error(s).
- Fly.io-GCS.exe exists at the expected output path.
- The application launches from the Release package.
- DATA, PLAN, SETUP, CONFIG, SIMULATION, HELP, and DRDC open.
- HUD, map, connection controls, and telemetry views render.
- Required DLLs, resources, configuration, licenses, and notices are included.
- Telemetry checks pass for the tested cases.
- The complete package launches outside the source tree.

## DRDC

[DRDC website](https://drdc-dev-web.drsys.asia/)

Fly.io GCS is developed for DRDC drone research, integration, and operational
workflows. The HELP website card and clickable Fly.io brand open the official
DRDC website. The embedded DRDC page retains its existing authentication flow.

## Repository links

- [GitHub repository](https://github.com/NAK-Kiet/Flyio-GCS)
- [Internal/private DRDC GitLab](http://192.168.100.120:8082) — SSH clone:
  git@192.168.100.120:Kaine/flyio-gcs.git

## License and attribution

The user-facing product is **Fly.io GCS**. The repository also contains inherited
Mission Planner and ArduPilot ecosystem code and other third-party components.
Fly.io-specific or DRDC-specific work may have its own distribution policy, while
inherited and third-party software retains its applicable licenses, copyrights,
attribution, and source-distribution obligations.

Required notices remain in the repository and release package:

- [COPYING.txt](COPYING.txt)
- [NOTICE-Fly.io-GCS.txt](NOTICE-Fly.io-GCS.txt)
- source-file notices and packaged third-party notices
- [Mission Planner](https://github.com/ArduPilot/MissionPlanner)
- [Mission Planner contributors](https://github.com/ArduPilot/MissionPlanner/graphs/contributors)
- [ArduPilot documentation](https://ardupilot.org/ardupilot/)

Product branding does not remove upstream legal attribution.

## Screenshots

The repository contains approved Fly.io navigation artwork in Assets\FlyioNav\.
Release validation screenshots remain local review artifacts and are not treated
as product documentation assets. Add screenshots only after approving stable,
current captures of DATA, PLAN, SETUP, CONFIG, SIMULATION, HELP, and DRDC.
