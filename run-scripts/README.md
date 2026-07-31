# Fly.io GCS — Build & Run Guide

## Prerequisites

- **.NET SDK 10** (or newer) installed on Windows
- Windows 10/11 with .NET Framework 4.7.2+ (built in)
- This project builds the **MissionPlanner** WinForms application

---

## 1. Built output location

After a successful Release build, the executable is here:

```
bin/Release/net461/MissionPlanner.exe
```

Full path from Windows:

```
C:\Users\Alien\Flyio-GCS\bin\Release\net461\MissionPlanner.exe
```

> Note: the output folder is `net461`, not `net10.0` — `MissionPlanner.csproj`
> targets .NET Framework 4.7.2 and sets `AppendTargetFrameworkToOutputPath=false`.

---

## 2. Build the application

### Option A — Script (recommended)

From the repo root:

```bash
./run-scripts/build.sh
```

This builds `Drivers/DriverCleanup.exe` first (a required dependency), then
compiles `MissionPlanner.csproj` in Release mode.

### Option B — Manual commands

```bash
dotnet build ExtLibs/DriverCleanup/DriverCleanup.csproj -c Release
dotnet build MissionPlanner.csproj -c Release
```

- If `dotnet` is not on your WSL `PATH`, use the Windows SDK directly:
  `/mnt/c/Program Files/dotnet/dotnet.exe`
- On Windows (Git Bash / Command Prompt), plain `dotnet build ...` works.

---

## 3. Run / launch the application

### Option A — Script (recommended)

```bash
./run-scripts/run.sh
```

### Option B — Direct execution

```bash
./bin/Release/net461/MissionPlanner.exe
```

### Option C — Windows-native launch (detached)

Run from PowerShell or Command Prompt:

```powershell
Start-Process -FilePath 'C:\Users\Alien\Flyio-GCS\bin\Release\net461\MissionPlanner.exe'
```

---

## Troubleshooting

| Symptom | Fix |
|---|---|
| `MSB3030: Could not copy Drivers\DriverCleanup.exe` | Run `dotnet build ExtLibs/DriverCleanup/DriverCleanup.csproj -c Release` first (or use `run-scripts/build.sh`) |
| `dotnet: command not found` (in WSL) | Use `/mnt/c/Program Files/dotnet/dotnet.exe` instead of `dotnet` |
| Launch fails with "Access is denied" | Use PowerShell `Start-Process` (Option C) |
