"""Passive WSL packet observer for the separately authorized ingress test.

Requires root for AF_PACKET. Sends no packets. The marker is an exact, externally
generated harmless HEARTBEAT frame, supplied as base64 on the command line.
"""
import base64
import json
import socket
import struct
import sys
import time

marker = base64.b64decode(sys.argv[1], validate=True)
deadline = time.monotonic() + 60
finish_after = None
result = {"markerIngress": 0, "markerToPx4": 0, "qgcToPx4": 0,
          "px4Telemetry": 0, "ingressEndpoints": []}
with socket.socket(socket.AF_PACKET, socket.SOCK_RAW, socket.htons(0x0800)) as observer:
    observer.settimeout(1)
    print("READY", flush=True)
    while time.monotonic() < deadline:
        if finish_after is not None and time.monotonic() >= finish_after:
            break
        try:
            packet = observer.recv(65535)
        except socket.timeout:
            continue
        if len(packet) < 42 or packet[23] != 17:
            continue
        offset = 14 + (packet[14] & 15) * 4
        if len(packet) < offset + 8:
            continue
        source, destination, length = struct.unpack("!HHH", packet[offset:offset + 6])
        payload = packet[offset + 8:offset + length]
        source_ip = socket.inet_ntoa(packet[26:30])
        destination_ip = socket.inet_ntoa(packet[30:34])
        if source == 14551 and marker in payload:
            result["markerIngress"] += 1
            endpoint = f"{source_ip}:{source} -> {destination_ip}:{destination}"
            if endpoint not in result["ingressEndpoints"]:
                result["ingressEndpoints"].append(endpoint)
            finish_after = time.monotonic() + 3
        if destination == 18570:
            if marker in payload:
                result["markerToPx4"] += 1
            # QGC's already verified source identity; count as positive control.
            if len(payload) > 10 and payload[0] == 0xFD and payload[5:7] == bytes([255, 190]):
                result["qgcToPx4"] += 1
            elif len(payload) > 8 and payload[0] == 0xFE and payload[3:5] == bytes([255, 190]):
                result["qgcToPx4"] += 1
        if source == 18570:
            result["px4Telemetry"] += 1
result["passed"] = (result["markerIngress"] > 0 and result["markerToPx4"] == 0
                    and result["qgcToPx4"] > 0 and result["px4Telemetry"] > 0)
print(json.dumps(result, indent=2), flush=True)
sys.exit(0 if result["passed"] else 1)
