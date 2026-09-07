# Fly.io GCS

Fly.io Ground Control Station — a customized desktop ground control station for UAV development, telemetry, mission planning, configuration, simulation, and PX4/ArduPilot integration.

Fly.io GCS is derived from the original ArduPilot ground-control stack. It keeps the proven MAVLink workflows and Fly.io branding while providing a native Windows application for UAV and UGV development.

The project respects upstream Mission Planner functionality, authorship, licensing, and the wider ArduPilot open-source ecosystem. Fly.io-specific changes do not imply authorship of upstream Mission Planner components.

## Features

- Native Windows desktop application with the release executable renamed to `Fly.io-GCS.exe`
- Fly.io dark aviation interface with an amber Fly.io accent theme
- Decorative legacy green removed; green is retained only for connected, healthy, and successful states
- Restored original Fly.io navigation layout: DATA, PLAN, SETUP, CONFIG, HELP, and DRDC
- DATA workspace with live telemetry, HUD, aviation gauges, map workspace, connection controls, and status interface
- PLAN mission planning and existing waypoint workflows
- SETUP, CONFIG, and Full Parameter List workflows
- HELP resources and embedded DRDC GCS integration
- Simulation and firmware workflows inherited from the original GCS stack
- PX4 and ArduPilot compatibility inherited from the original MAVLink/GCS stack

The Full Parameter List preserves real ArduPilot parameter names and values while providing organized navigation for large parameter families such as GPS, OSD, RC, SERIAL, SERVO, and others.

## Fly.io Theme

The Fly.io interface uses a centralized dark-navy palette with these interaction accents:

- Primary: `#EDA63B`
- Hover: `#F5BC62`
- Pressed: `#BB7C24`

Operational and semantic colors are intentionally preserved. For example, green may still indicate healthy GPS, sensor, connection, calibration, or other successful status information; graph and map colors also retain their technical meaning.

## Fly.io DATA Workspace

The DATA workspace presents live Mission Planner flight data in a responsive Fly.io dark-navy interface. The existing aviation HUD occupies the upper-left workspace, the selected DATA page appears below it, and the map fills the right side. A draggable splitter controls the balance between the left workspace and map while preserving a usable layout as the window is resized.

All displayed flight values continue to come from the existing Mission Planner/MAVLink `CurrentState` telemetry pipeline. The interface does not substitute sample values or static dashboard data.

The workspace includes:

- An aviation-style HUD with a blue-sky, brown-earth artificial horizon
- Modern Quick telemetry cards
- A responsive six-instrument Gauges page
- A map-provider selector backed by the available Mission Planner/GMap providers
- Existing Follow, Grid, Location, and More map controls
- Compact map information for latitude, longitude, altitude, heading, and satellite count
- A bottom flight-status bar showing MODE, STATE/armed status, GPS, SAT, HDOP, VIBE, LINK, and RC information

## Application

Release executable:

```text
bin\Release\net461\Fly.io-GCS.exe
```

Basic launch example:

```powershell
cd bin\Release\net461
.\Fly.io-GCS.exe
```

## PX4 and MAVLink development

This repository also contains related development and integration work around PX4 and MAVLink. Experiments have included PX4 SITL testing, a custom PX4 `flyio_hello` module, MAVLink 2 telemetry testing, QGroundControl coexistence, a `mavlink-router` architecture, and Fly.io receive-only telemetry experiments.

These experiments are development activities and test setups. They are related to the desktop Fly.io GCS, but they are not presented as finished production features. The MSI desktop application and Alienware PX4/browser telemetry work should be evaluated separately from the completed native GCS interface.

### Aviation Instruments

The Gauges page uses responsive, vector-rendered Fly.io controls rather than static dashboard images. Its conventional six-pack arrangement is:

| Row | Instruments |
| --- | --- |
| Top | Airspeed Indicator · Attitude Indicator · Altimeter |
| Bottom | Turn Coordinator · Heading Indicator · Vertical Speed Indicator |

The Airspeed Indicator uses airspeed telemetry, the Attitude Indicator uses roll and pitch, the Altimeter uses altitude, the Turn Coordinator uses supported turn-rate telemetry, the Heading Indicator uses heading/yaw, and the Vertical Speed Indicator uses vertical speed. The Heading Indicator has a fixed white aircraft reference while its compass card responds to heading. The Turn Coordinator includes an aircraft representation without inventing unavailable slip/skid telemetry.

## Interface Design

Fly.io GCS uses a dark-navy primary interface with Fly.io amber (`#EDA63B`) for interaction and selection accents. Aviation-oriented instrument presentation keeps flight information readable without replacing its operational meaning. Semantic green indicates healthy states, yellow indicates caution, and red is reserved for warnings, errors, or states such as disarmed where applicable. The DATA workspace, HUD, gauges, map, and status areas resize together while retaining the draggable DATA/map split.

### HELP workspace

The HELP workspace uses a native dark interface consistent with the operational workspaces. Its presentation includes Segoe UI typography, compact action buttons, readable external links, muted secondary text, and dark cards.

HELP provides:

- Online Services for the DRDC GCS Dashboard and Brands & Partnerships
- Quick Help keyboard shortcuts
- System Updates controls and the upstream Mission Planner changelog
- About & Open Source project repository information
- Upstream ArduPilot and Mission Planner attribution, contributor links, and documentation
- An animated **OUR COMMUNITY PARTNERS** marquee

### Community Partners

The HELP page includes a native WinForms Community Partners marquee built from repository-owned logo resources. The logos scroll continuously from left to right using a seamless repeated sequence. Original logo colors and aspect ratios are preserved, and the implementation requires no web, React, or browser-animation dependency.

## Requirements

Development and Release builds are supported on Windows. Based on the current project files, a developer needs:

- Git
- A .NET SDK capable of building SDK-style .NET Framework projects
- .NET Framework 4.7.2 targeting/reference assemblies (`MissionPlanner.csproj` targets `net472`)
- PowerShell or another terminal capable of running the documented commands
- Network access during the initial NuGet restore

Visual Studio with the .NET desktop development tools and the .NET Framework 4.7.2 developer/targeting pack is an optional development environment. The main project is WinForms-based and includes multiple referenced projects and NuGet packages; use the repository project files rather than installing dependencies manually.

Although the project targets .NET Framework 4.7.2, its existing Release configuration writes the application to the historical `bin\Release\net461` output directory.

## Clone from GitHub

Clone the repository and use the `main` branch:

```powershell
git clone https://github.com/NAK-Kiet/Flyio-GCS.git
cd Flyio-GCS
git switch main
```

Alternatively, clone only `main`:

```powershell
git clone --branch main --single-branch https://github.com/NAK-Kiet/Flyio-GCS.git
cd Flyio-GCS
```

## Clone from GitLab

The configured GitLab repository is internal/private and uses SSH:

```powershell
git clone git@192.168.100.120:Kaine/flyio-gcs.git
cd flyio-gcs
git switch main
```

An SSH key authorized for this GitLab repository and network access to its host are required. The SSH remote is a Git transport address, not a browser URL.

## Build

For a fresh clone, restore packages and build the Release configuration:

```powershell
dotnet restore .\MissionPlanner.csproj
dotnet build .\MissionPlanner.csproj -c Release --no-restore
```

For an already-restored working copy:

```powershell
dotnet build .\MissionPlanner.csproj -c Release --no-restore
```

The application is produced at:

```text
bin\Release\net461\Fly.io-GCS.exe
```

## Run

From the repository root in PowerShell:

```powershell
.\bin\Release\net461\Fly.io-GCS.exe
```

You can also open `bin\Release\net461` in File Explorer and double-click `Fly.io-GCS.exe`.

## Basic Usage

1. Start Fly.io GCS.
2. Select the communication port appropriate for the connected vehicle.
3. Select the appropriate baud rate for that connection.
4. Select **CONNECT**.
5. Use **DATA** for telemetry and live Flight Data.
6. Use **PLAN** to create and manage missions.
7. Use **SETUP** for vehicle, hardware, calibration, and firmware setup.
8. Use **CONFIG** for parameters and other vehicle configuration.
9. Use **SIMULATION** for supported SITL workflows.
10. Use **HELP** for Fly.io project information, upstream resources, and shortcuts.
11. Use **DRDC GCS** for the embedded DRDC integration.
12. Select **DISCONNECT** before ending the connection or disconnecting hardware.

Always follow the documentation for the specific flight controller, vehicle, firmware, and connection being used. Verify configuration and safety-critical behavior before operating a vehicle.

## Firmware

Fly.io GCS retains two separate firmware workflows:

- **Install Firmware** provides the current firmware selection and installation workflow.
- **Install Firmware Legacy** preserves the legacy firmware workflow for supported use cases.

They are intentionally separate. Firmware server selection, board detection, flashing behavior, and supported vehicle choices remain part of the underlying Mission Planner/ArduPilot workflows.

## Simulation

The SIMULATION workspace supports SITL selection for:

- Plane
- Rover
- Multirotor
- Helicopter

Vehicle selectors retain light neutral icons in their normal state and use the centralized Fly.io amber/blue palette for hover, selected, and pressed states.

## Updating

From an existing clone on `main`:

```powershell
git switch main
git pull
dotnet restore .\MissionPlanner.csproj
dotnet build .\MissionPlanner.csproj -c Release --no-restore
```

If package inputs have not changed and the repository has already been restored, the restore step may be omitted.

## Troubleshooting

### Build files are locked

If a build reports that `Fly.io-GCS.exe` or one of its DLLs cannot be copied because it is in use, close Fly.io GCS before rebuilding. If necessary, stop the process from PowerShell:

```powershell
Get-Process -Name 'Fly.io-GCS' -ErrorAction SilentlyContinue | Stop-Process -Force
```

Then rerun the Release build.

### COM port is already in use

Close other ground-control, serial-terminal, or device-management software using the port. Confirm the correct port and baud rate before reconnecting.

### .NET Framework reference assemblies are missing

Install the .NET Framework 4.7.2 developer/targeting pack or the corresponding Visual Studio individual component, then rebuild. Installing only a runtime may not provide the reference assemblies required for compilation.

### Restore or build fails

Run restore without `--no-restore`, confirm NuGet sources and network access are available, and inspect the first reported error:

```powershell
dotnet restore .\MissionPlanner.csproj
dotnet build .\MissionPlanner.csproj -c Release
```

Do not delete or replace repository dependencies merely to suppress a restore or reference error.

## Upstream

- [ArduPilot Mission Planner](https://github.com/ArduPilot/MissionPlanner)
- [Mission Planner contributors](https://github.com/ArduPilot/MissionPlanner/graphs/contributors)
- [ArduPilot documentation](https://ardupilot.org/ardupilot/)

Upstream changes should be reviewed carefully before integration so Fly.io-specific UI, branding, configuration, and connection changes are not overwritten unintentionally.

## License / Attribution

Fly.io GCS is derived from ArduPilot Mission Planner and retains all applicable upstream and third-party open-source licensing and attribution requirements. Review [LICENSE](LICENSE), repository notices, dependency licenses, and upstream Mission Planner licensing terms before modification or redistribution.

Mission Planner and ArduPilot remain the work of their respective upstream authors and contributors. Fly.io does not claim authorship of upstream components.

## Project Status

Fly.io GCS is under active development on the `main` branch. The public repository is [NAK-Kiet/Flyio-GCS](https://github.com/NAK-Kiet/Flyio-GCS); an internal/private GitLab mirror is configured at `git@192.168.100.120:Kaine/flyio-gcs.git`.

## Native product and release identity

Both `MissionPlanner.csproj` (the compatibility project filename) and the
`Flyio-GCS.csproj` entry point build the native Windows Forms application.
The desktop product does not use the historical `web/` prototype.
AssemblyName is `Fly.io-GCS`; Product and Title are `Fly.io GCS`; Description
is `Fly.io Ground Control Station`. Internal MissionPlanner namespaces and
resource identifiers remain stable.

Primary navigation: FLY.IO (home), Menu (tools, connection options, Help/About,
DRDC), Setup, Config, Sim, Frame, Status (link diagnostics). Workspace tabs
open the existing FLIGHT DATA, FLIGHT PLAN, INITIAL SETUP, CONFIGURATION,
and SIMULATION screens. Frame configuration is available for supported,
connected vehicles after parameters load. No calibration or firmware action
is performed by merely selecting a workspace.

The compact global status strip displays transport, observed vehicle
heartbeat, GPS, position, altitude, RSSI when available, and computer UTC.
Unavailable data is shown as `--`; an open transport alone is not a heartbeat.
The existing flight instruments and their operational colors are preserved.

### External PX4 SITL

Launch PX4 separately using your configurable simulation environment.
Use the existing UDP, UDPCl, or TCP connection selectors with the simulator's
configured endpoint. The Sim menu provides guidance; SIMULATION retains the
working ArduPilot SITL controls. An integrated PX4/WSL launcher is not yet
implemented. No machine-specific PX4 path is embedded. Use distinct endpoints
for concurrent QGroundControl validation. Hardware tests require explicit approval.

### Portable Windows package

Run `powershell -NoProfile -File run-scripts/package-release.ps1` to build into
an isolated staging folder and prepare `artifacts/Fly.io-GCS-Windows.zip`.
The script does not publish. Distribute the full package, including native
DLLs, managed libraries, resource folders, config, COPYING, notices, and
corresponding source as required by the licenses; do not copy only the EXE.
The normal build folder can contain local settings or old outputs and should
not be zipped directly. No executable is renamed after compilation.

Another laptop needs a compatible Windows installation with .NET Framework
4.7.2 or later. The `net461` folder name is historical, not the runtime target.
DRDC needs Microsoft Edge WebView2 Runtime and access to its existing online
service. Serial hardware may need its manufacturer's driver. Maps, firmware,
DRDC, and simulator downloads need network access unless already cached.
Optional video/native components may require their own runtimes. The app
is AnyCPU with architecture-specific native dependencies; validate those
features on the target laptop before distribution. Existing user settings
retain their historical directory for compatibility. Application updates
are disabled until a reviewed Fly.io GCS release feed is configured.
