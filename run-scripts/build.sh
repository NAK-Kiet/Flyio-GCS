#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

if command -v dotnet >/dev/null 2>&1; then
    DOTNET=dotnet
elif [[ -x "/mnt/c/Program Files/dotnet/dotnet.exe" ]]; then
    DOTNET="/mnt/c/Program Files/dotnet/dotnet.exe"
elif [[ -x "/c/Program Files/dotnet/dotnet.exe" ]]; then
    DOTNET="/c/Program Files/dotnet/dotnet.exe"
else
    echo "dotnet not found. Install the .NET SDK and try again." >&2
    exit 1
fi

"$DOTNET" build ExtLibs/DriverCleanup/DriverCleanup.csproj -c Release
"$DOTNET" build MissionPlanner.csproj -c Release
