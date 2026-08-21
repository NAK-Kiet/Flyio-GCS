# Fly.io GCS

Fly.io GCS is a customized Ground Control Station based on ArduPilot Mission Planner. It preserves Mission Planner's established MAVLink vehicle connectivity, telemetry, mission-planning, and configuration capabilities while adding Fly.io-specific branding, user-interface improvements, configuration workflow improvements, and a foundation for UAV and UGV development.

## Current Features

- ArduPilot and MAVLink vehicle connectivity
- Live vehicle telemetry and Flight Data tools
- Mission planning
- Vehicle configuration tools
- Full Parameter List viewing and editing
- Fly.io dark navy and red theme
- Fly.io splash screen
- Fly.io application and dialog icons
- Improved configuration navigation
- Improved parameter navigation
- COM and MAVLink connection fixes

## Full Parameter List Improvements

The Full Parameter List provides consolidated navigation for related parameter families while preserving every real ArduPilot parameter name and value. Supported grouped families include:

- `BARO`
- `BATT`
- `CAM`
- `FLTMODE`
- `GPS`
- `MAV`
- `OSD`
- `RC`, including `RCMAP` and `RC1` through `RC16`
- `RELAY`
- `RNGFND`
- `RPM`
- `SERIAL`
- `SERVO`, including `SERVO1` through `SERVO16`

Large families such as RC and SERVO use a stable, wrapped selector layout. This keeps available channel selectors visible, avoids horizontal overflow, and makes parameter groups easier to navigate without changing parameter behavior.

## Fly.io UI and Branding

Fly.io GCS includes a dark navy interface with Fly.io red accents, an updated splash screen, updated application and warning-dialog icons, improved parameter-grid readability, and clearer tab and selector layouts.

## Build Instructions

From PowerShell:

```powershell
cd C:\Users\rotha\Flyio-GCS
dotnet build .\MissionPlanner.csproj -c Release --no-restore
```

Run the Release build with:

```powershell
.\bin\Release\net461\MissionPlanner.exe
```

## Repository Branches

- `main` — integrated and stable branch
- `fix/missing-dependencies` — development branch used for current Fly.io GCS development

## ArduPilot Upstream

Fly.io GCS is based on [ArduPilot Mission Planner](https://github.com/ArduPilot/MissionPlanner).

Upstream Mission Planner changes should be reviewed carefully before integration. This helps prevent Fly.io-specific UI, configuration, branding, and connection changes from being accidentally overwritten.

## Future / Integration Areas

Fly.io GCS is intended to support continued UAV and UGV development. Planned integration targets include:

- AI Vision
- LiDAR
- ROS 2
- MQTT and AIoT systems

These items are integration targets and should not be considered fully implemented unless explicitly documented in a release.

## License

Fly.io GCS is derived from ArduPilot Mission Planner and retains all applicable upstream and open-source licensing requirements. Review the repository's license files and upstream licensing terms before redistribution or modification.

## Project Status

Fly.io GCS is under active development.
