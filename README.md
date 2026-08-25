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
- HELP workspace with project and upstream information
- Embedded DRDC GCS integration
- UAV and UGV support

The Full Parameter List preserves real ArduPilot parameter names and values while providing organized navigation for large parameter families such as GPS, OSD, RC, SERIAL, SERVO, and others.

## Fly.io Theme

The Fly.io interface uses a centralized dark-navy palette with these interaction accents:

- Primary: `#00ADEE`
- Hover: `#25BEF4`
- Pressed: `#0083B8`

Operational and semantic colors are intentionally preserved. For example, green may still indicate healthy GPS, sensor, connection, calibration, or other successful status information; graph and map colors also retain their technical meaning.

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

Clone the development branch after cloning the repository:

```powershell
git clone https://github.com/NAK-Kiet/Flyio-GCS.git
cd Flyio-GCS
git switch fix/missing-dependencies
```

Alternatively, clone only the development branch:

```powershell
git clone --branch fix/missing-dependencies --single-branch https://github.com/NAK-Kiet/Flyio-GCS.git
cd Flyio-GCS
```

## Clone from GitLab

The configured GitLab repository uses SSH:

```powershell
git clone git@192.168.100.120:Kaine/flyio-gcs.git
cd flyio-gcs
git switch fix/missing-dependencies
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

From an existing clone on the development branch:

```powershell
git switch fix/missing-dependencies
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

Fly.io GCS is under active development on the `fix/missing-dependencies` branch.
