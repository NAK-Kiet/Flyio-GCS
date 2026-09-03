# Fly.io GCS

Fly.io GCS is a customized Ground Control Station derived from [ArduPilot Mission Planner](https://github.com/ArduPilot/MissionPlanner). It preserves Mission Planner's established MAVLink connectivity, telemetry, mission-planning, vehicle-configuration, and firmware workflows while adding Fly.io branding, interface improvements, and DRDC integration for UAV and UGV development.

The project respects upstream Mission Planner functionality, authorship, licensing, and the wider ArduPilot open-source ecosystem. Fly.io-specific changes do not imply authorship of upstream Mission Planner components.

## Features

- MAVLink vehicle connectivity over supported communication transports
- DATA workspace for live Flight Data and telemetry
- PLAN workspace for mission planning
- SETUP workspace for vehicle and hardware setup
- CONFIG workspace, including the Full Parameter List
- SIMULATION workspace for SITL workflows
- Separate Install Firmware and Install Firmware Legacy workflows
- Fly.io dark-navy interface, blue/cyan interaction states, and branding
- HELP workspace with a clean white interface, project resources, shortcuts, updates, and upstream information
- Embedded DRDC GCS integration
- UAV and UGV support

The Full Parameter List preserves real ArduPilot parameter names and values while providing organized navigation for large parameter families such as GPS, OSD, RC, SERIAL, SERVO, and others.

## Fly.io Theme

The Fly.io interface uses a centralized dark-navy palette with these interaction accents:

- Primary: `#00ADEE`
- Hover: `#25BEF4`
- Pressed: `#0083B8`

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

### Aviation Instruments

The Gauges page uses responsive, vector-rendered Fly.io controls rather than static dashboard images. Its conventional six-pack arrangement is:

| Row | Instruments |
| --- | --- |
| Top | Airspeed Indicator · Attitude Indicator · Altimeter |
| Bottom | Turn Coordinator · Heading Indicator · Vertical Speed Indicator |

The Airspeed Indicator uses airspeed telemetry, the Attitude Indicator uses roll and pitch, the Altimeter uses altitude, the Turn Coordinator uses supported turn-rate telemetry, the Heading Indicator uses heading/yaw, and the Vertical Speed Indicator uses vertical speed. The Heading Indicator has a fixed white aircraft reference while its compass card responds to heading. The Turn Coordinator includes an aircraft representation without inventing unavailable slip/skid telemetry.

## Interface Design

Fly.io GCS uses a dark-navy primary interface with Fly.io cyan (`#00ADEE`) for interaction and selection accents. Aviation-oriented instrument presentation keeps flight information readable without replacing its operational meaning. Semantic green indicates healthy states, yellow indicates caution, and red is reserved for warnings, errors, or states such as disarmed where applicable. The DATA workspace, HUD, gauges, map, and status areas resize together while retaining the draggable DATA/map split.

### HELP workspace

The HELP workspace uses a clean, website-inspired white interface that is intentionally local to HELP; the operational Fly.io GCS workspaces retain the dark-navy application theme. Its presentation includes Segoe UI typography, black action buttons, dark underlined external links, gray secondary text, and light-gray cards.

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
bin\Release\net461\Flyio-GCS.exe
```

## Run

From the repository root in PowerShell:

```powershell
.\bin\Release\net461\Flyio-GCS.exe
```

You can also open `bin\Release\net461` in File Explorer and double-click `Flyio-GCS.exe`.

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

Vehicle selectors retain light neutral icons in their normal state and use the centralized Fly.io cyan/blue palette for hover, selected, and pressed states.

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

If a build reports that `Flyio-GCS.exe` or one of its DLLs cannot be copied because it is in use, close Fly.io GCS before rebuilding. If necessary, stop the process from PowerShell:

```powershell
Get-Process -Name 'Flyio-GCS' -ErrorAction SilentlyContinue | Stop-Process -Force
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
