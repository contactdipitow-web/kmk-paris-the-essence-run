#!/usr/bin/env python3
from __future__ import annotations
import base64
import io
import tarfile
from pathlib import Path

root = Path(__file__).resolve().parents[1]
chunk_dir = root / "Tools" / ".bootstrap_chunks"
payload = "".join(path.read_text(encoding="utf-8").strip() for path in sorted(chunk_dir.glob("chunk*.txt")))
if len(payload) != 47528:
    raise RuntimeError(f"Unexpected bootstrap payload length: {len(payload)}")
data = base64.b64decode(payload)
with tarfile.open(fileobj=io.BytesIO(data), mode="r:gz") as archive:
    root_resolved = root.resolve()
    for member in archive.getmembers():
        destination = (root / member.name).resolve()
        if destination != root_resolved and root_resolved not in destination.parents:
            raise RuntimeError(f"Unsafe archive path: {member.name}")
    archive.extractall(root, filter="data")
print("KMK Unity V2 project extracted")
