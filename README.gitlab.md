# Fly.io GCS

Internal engineering README — review draft. Intended as the root README in the internal GitLab repository. No publication or branch changes have been performed.

## Project Overview

Fly.io GCS is a Windows desktop Ground Control Station for UAV and UGV systems used in DRDC research, development, and operational workflows. It provides vehicle monitoring, MAVLink telemetry, mission planning, vehicle configuration, simulation workspaces, and embedded DRDC access.

Fly.io GCS originated from the ArduPilot Mission Planner codebase. It retains inherited components, namespaces, and internal project identifiers; the product was not written entirely from scratch. Function availability depends on the connected vehicle, autopilot, and configuration.

## Current Release

| Item | Current repository configuration |
| --- | --- |
| Product | Fly.io GCS |
| Executable | `Fly.io-GCS.exe` |
| Primary OS | Windows 10/11 |
| Build target | .NET Framework 4.7.2 / `net472` |
| Historical output directory | `bin\Release\net461` |

`net461` is the historical output folder name; the project targets `net472`. No release version or release date is asserted here. Obtain the approved package and its version/commit from the team.

## Quick Start

### Team Users

1. Obtain an approved Fly.io GCS release ZIP from the team.
2. Extract the **complete** archive.
3. Keep all files and subdirectories together.
4. Run `Fly.io-GCS.exe`.

Do **not** copy only `Fly.io-GCS.exe`. DLLs, configuration files, native dependencies, resources, WebView2-related files, localization resources, and other runtime content are needed.

### Developers

Use a Windows development environment for the desktop build and execution. Install Git, a .NET SDK compatible with the repository workflow, and .NET Framework 4.7.2 targeting/reference assemblies. The existing build guide specifies .NET SDK 10 or newer. Visual Studio with desktop .NET tooling is an IDE option; VS Code is an editor option.

#### Internal Clone Instructions

For authorized DRDC users, this is an internal/private server. Approved network access, repository permission, and an SSH public key registered with the server are required. Keep private keys, passwords, passphrases, and tokens out of documentation.

```powershell
git clone --recurse-submodules git@192.168.100.120:Kaine/flyio-gcs.git
cd flyio-gcs
git submodule update --init --recursive
```

These commands create `origin` for the internal repository. In a checkout with multiple remotes, inspect `git remote -v` and use the intended internal remote for updates.

Run the restore and build commands in **Build and Run** below.

## Updating Fly.io GCS

Preserve important local changes before updating. Inspect the working tree and current branch; run the pull below when on `main` and ready to integrate `origin/main`.

```powershell
git status
git branch --show-current
git fetch origin
git pull --ff-only origin main
git submodule update --init --recursive
```

Do not pull blindly over local modifications. Review `git diff` and preserve work using the team's normal branch, commit, or stash workflow. If histories diverge, stop and review the branches; do not force an update with destructive reset/clean commands.

## Build and Run

From the repository root in Windows PowerShell:

```powershell
dotnet restore MissionPlanner.csproj --configfile .\nuget.config
dotnet build MissionPlanner.csproj -c Release --no-restore
```

Expected executable:

```text
bin\Release\net461\Fly.io-GCS.exe
```

Launch the application or locate the complete output folder:

```powershell
Start-Process ".\bin\Release\net461\Fly.io-GCS.exe"
explorer ".\bin\Release\net461"
```

`MissionPlanner.csproj` remains an inherited/internal project identifier required for compatibility. Its name does not change the user-facing product name, Fly.io GCS.

If the build reports missing `Drivers\DriverCleanup.exe`, the existing build guide documents building that helper first:

```powershell
dotnet build ExtLibs/DriverCleanup/DriverCleanup.csproj -c Release
```

Then repeat the main build. These instructions describe the workflow; they are not a claim that this checkout has passed release verification.

## Platform Compatibility

| Platform | Run Fly.io GCS | Development | Status |
| --- | --- | --- | --- |
| Windows 10/11 x64 | Native | Yes | Recommended |
| Linux | Not native | Yes | Development only |
| macOS | Not native | Yes | Development only |
| Windows VM | Yes | Yes | Alternative |

Linux and macOS support source editing and development collaboration; desktop build/run validation uses Windows. VS Code is a development/editor environment and does not make a WinForms application run natively on Linux/macOS. A Windows VM needs appropriate USB/serial access and vehicle-specific validation.

## Release Package Structure

The EXE alone is **not** the complete application. Keep the complete approved Release package, including licenses and notices. Depending on the packaged dependency set, it contains:

```text
Fly.io-GCS.exe
Fly.io-GCS.exe.config
DLL dependencies and native libraries
runtimes/
x64/
x86/
plugins/
WebView2-related runtime/support content
localization resources
map and application resources
configuration files
COPYING.txt
NOTICE-Fly.io-GCS.txt
third-party notices
```

This is an illustrative inventory, not a promise that every package has identical directories. Preserve the supplied hierarchy. WebView2 browser data may also be created at runtime; do not distribute personal browser/session data.

## Repository Structure

```text
Flyio-GCS/
├── GCSViews/              # Application workspaces
├── Controls/              # WinForms controls and instruments
├── Utilities/             # Shared application utilities
├── Assets/                # Fly.io navigation artwork
├── Properties/            # Settings, resources, manifests
├── Resources/             # Icons, imagery, embedded resources
├── ExtLibs/               # Inherited and third-party libraries
├── run-scripts/           # Build, run, review, packaging helpers
├── MissionPlanner.csproj  # Compatibility build entry point
└── README.md
```

## Vehicle Connection

1. Connect the telemetry or USB hardware.
2. Start Fly.io GCS.
3. Select the correct COM port or connection type.
4. Select the baud rate when required.
5. Connect.
6. Verify a heartbeat.
7. Verify the identified vehicle and vehicle state.
8. Check attitude, GPS, battery, and flight mode against expected values.
9. Perform normal operational safety checks before commanding the vehicle.

Available telemetry depends on the vehicle and stream. Secure the vehicle during testing and follow approved operational procedures. PX4 SITL is not a startup prerequisite.

## Troubleshooting

| Symptom | Checks and next step |
| --- | --- |
| Application does not start | Confirm the complete package was extracted, the expected Windows/.NET environment is available, and inspect the error and application logs. |
| Missing DLL | Re-extract the approved full archive; preserve native and localization subdirectories. Do not download arbitrary replacement DLLs. |
| Build errors | Read the first error; check SDK/targeting assemblies and submodules. For missing DriverCleanup, use the helper build documented above. |
| NuGet restore error | Run the explicit-config restore below; check configured feeds, network/proxy access, and the first restore error. Never paste credentials into an issue. |
| Submodule error | Check repository access and run `git submodule update --init --recursive`. Report the failing submodule and error. |
| Wrong COM port | Check Windows device detection, reconnect the device, and select its actual port and required baud rate. |
| Telemetry not detected | Check vehicle power, cables/radio link, connection type, baud, heartbeat, and MAVLink settings. Close another application holding the serial port. |
| DRDC view fails | Check network access, approved account access, and WebView2 availability; capture the displayed error without session secrets. |
| Windows security warning | Confirm the package source and integrity with the team and follow organizational policy. Do not disable security controls blindly. |
| Git pull conflict/local changes | Use `git status` and `git diff`, preserve work, and review branch divergence before retrying. |

NuGet restore command:

```powershell
dotnet restore MissionPlanner.csproj --configfile .\nuget.config
```

## Feedback and Issue Reporting

Reports are welcome. Include enough context to reproduce the problem:

- Fly.io GCS version and/or commit.
- Windows version.
- Screenshot and exact error message.
- Relevant logs.
- Vehicle/autopilot and connection method.
- Steps to reproduce.
- Expected result and actual result.

Report issues through the internal GitLab project or approved team support channel.

Remove credentials, session data, private infrastructure details, and sensitive vehicle/location information before sharing screenshots or logs.

## Known Limitations

- Windows is the current native desktop target; Linux/macOS are not native releases.
- Some inherited Mission Planner names remain internally.
- Hardware and vehicle behavior must be validated for the connected system.
- Real vehicle validation is separate from UI checks and successful builds.
- DRDC access depends on network availability and authorized account access.
- Simulation and ecosystem compatibility do not imply every vehicle or autopilot combination is validated.

## Release Verification

- [ ] Repository at approved commit
- [ ] Release build succeeds with 0 errors
- [ ] Fly.io-GCS.exe launches
- [ ] DATA checked
- [ ] PLAN checked
- [ ] SETUP checked
- [ ] CONFIG checked
- [ ] SIMULATION checked
- [ ] HELP checked
- [ ] DRDC checked
- [ ] Complete release package created
- [ ] ZIP extracted and tested
- [ ] Git refs verified where applicable

Record vehicle/telemetry validation separately for the tested hardware and operating conditions.

## Internal Use and Confidentiality

Fly.io/DRDC-specific non-public project materials, internal infrastructure, credentials, internal documents, and internal release packages are for authorized team members only. Use approved access and distribution channels.

Unauthorized disclosure or distribution may result in organizational, contractual, disciplinary, or legal consequences where applicable.

These organizational requirements do not override third-party or open-source licenses or their redistribution/source obligations.

## License and Attribution

The user-facing product is **Fly.io GCS**. Project-specific internal materials are subject to applicable organizational distribution and confidentiality requirements. Inherited and third-party code remains subject to its applicable third-party/open-source licenses; this repository is not described as wholly proprietary.

Fly.io GCS is derived from [ArduPilot Mission Planner](https://github.com/ArduPilot/MissionPlanner). Original authors and contributors retain their copyrights. Preserve:

- [COPYING.txt](COPYING.txt), containing GNU GPL version 3.
- [NOTICE-Fly.io-GCS.txt](NOTICE-Fly.io-GCS.txt).
- Mission Planner and ArduPilot attribution.
- Source-file and packaged third-party license notices.

Follow applicable corresponding-source and distribution obligations described in the notices. Product branding does not remove upstream attribution.
