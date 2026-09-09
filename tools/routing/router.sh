#!/usr/bin/env bash
# Run from WSL: bash /mnt/c/Flyio-GCS/tools/routing/router.sh start|stop|status|restart
set -euo pipefail
umask 077

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
script="$script_dir/router.sh"
binary="$HOME/.local/lib/flyio-mavlink-router/v4/mavlink-routerd"
expected_sha=785b9d653dd13f47716cd0c4a74f68126d39dfa3abb2e91ed60245122c5266a1
state="$HOME/.cache/flyio-mavlink-router"
mkdir -p "$state/config.d"

router_pid() {
    local pid
    [[ -r "$state/router.pid" ]] || return 1
    read -r pid < "$state/router.pid" || return 1
    [[ "$pid" =~ ^[0-9]+$ ]] || return 1
    [[ "$(readlink "/proc/$pid/exe" 2>/dev/null)" == "$binary" ]] || return 1
    printf '%s\n' "$pid"
}

verify_binary() {
    [[ -x "$binary" ]] || { echo "Missing executable: $binary" >&2; return 1; }
    printf '%s  %s\n' "$expected_sha" "$binary" | sha256sum --check --status
}

run_router() {
    exec 9>"$state/instance.lock"
    flock -n 9 || { echo 'A router supervisor is already running.' >&2; return 1; }
    verify_binary
    local gateway blocked child
    gateway="$(ip -4 route show default | awk '/default/ {print $3; exit}')"
    [[ "$gateway" =~ ^([0-9]{1,3}\.){3}[0-9]{1,3}$ ]] || {
        echo 'Cannot discover the Windows gateway from the WSL default route.' >&2; return 1;
    }
    if [[ -n "$(ss -H -uln 'sport = :14550')" ]]; then
        echo 'WSL UDP 14550 is occupied. Stop the existing router/helper first.' >&2
        return 1
    fi
    blocked="$(seq -s, 0 255)"
    sed -e "s/@WINDOWS_HOST@/$gateway/g" -e "s/@BLOCKED_SOURCES@/$blocked/g" \
        "$script_dir/router.conf.in" > "$state/main.conf.new"
    mv -- "$state/main.conf.new" "$state/main.conf"
    printf '%s\n' "$$" > "$state/supervisor.pid"
    printf '%s gateway=%s\n' "$(date -u +%FT%TZ)" "$gateway"
    "$binary" -c "$state/main.conf" -d "$state/config.d" 9>&- &
    child=$!
    printf '%s\n' "$child" > "$state/router.pid"
    trap 'kill -TERM "$child" 2>/dev/null || true' TERM INT
    local result=0
    wait "$child" || result=$?
    # A trapped signal can interrupt wait before the child has released its socket.
    if kill -0 "$child" 2>/dev/null; then wait "$child" || true; fi
    : > "$state/router.pid"
    : > "$state/supervisor.pid"
    return "$result"
}

start_router() {
    local pid
    if pid="$(router_pid)"; then echo "Already running: PID $pid"; return; fi
    verify_binary
    # Retain one previous run's log; runtime files never enter either checkout.
    if [[ -f "$state/router.log" ]]; then mv -f -- "$state/router.log" "$state/router.previous.log"; fi
    nohup bash "$script" run </dev/null >"$state/router.log" 2>&1 8>&- &
    local supervisor=$!
    for ((attempt=0; attempt<50; attempt++)); do
        if pid="$(router_pid)" && ss -H -ulnp 'sport = :14550' | grep -q "pid=$pid,"; then
            echo "Started mavlink-router v4: PID $pid, supervisor $supervisor"
            echo "Configuration: $state/main.conf"
            return
        fi
        if ! kill -0 "$supervisor" 2>/dev/null; then break; fi
        sleep 0.1
    done
    echo "Router failed to start; see $state/router.log" >&2
    tail -n 20 "$state/router.log" >&2
    return 1
}

stop_router() {
    local pid
    if ! pid="$(router_pid)"; then echo 'Router is stopped.'; return; fi
    kill -TERM "$pid"
    for ((attempt=0; attempt<50; attempt++)); do
        if ! router_pid >/dev/null && flock -n "$state/instance.lock" true; then
            echo "Stopped router PID $pid; supervisor lock released."
            return
        fi
        sleep 0.1
    done
    echo 'Graceful stop timed out; inspect the process before further action.' >&2
    return 1
}

case "${1:-status}" in
    run) run_router ;;
    status)
        if pid="$(router_pid)"; then
            echo "Running: PID $pid"
            ps -p "$pid" -o pid,args
            ss -H -uapn | grep "pid=$pid," || true
            echo "Configuration: $state/main.conf"
            echo "Log: $state/router.log"
        else echo 'Router is stopped.'; exit 3; fi
        ;;
    start|stop|restart)
        exec 8>"$state/control.lock"
        flock -w 10 8 || { echo 'Another lifecycle operation is in progress.' >&2; exit 1; }
        case "$1" in
            start) start_router ;;
            stop) stop_router ;;
            restart) stop_router; start_router ;;
        esac
        ;;
    *) echo "Usage: bash $script {start|stop|status|restart}" >&2; exit 2 ;;
esac
