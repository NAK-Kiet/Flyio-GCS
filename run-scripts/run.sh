#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

EXE="bin/Release/net461/MissionPlanner.exe"

if [[ ! -f "$EXE" ]]; then
    echo "Build output not found. Run ./run-scripts/build.sh first." >&2
    exit 1
fi

exec "$EXE" "$@"
