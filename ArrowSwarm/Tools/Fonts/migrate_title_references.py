"""Replace only serialized font/material references; never re-save whole Unity scenes."""
from pathlib import Path
import hashlib
import json
import re

ROOT = Path(__file__).resolve().parents[2]
OLD = "f4d1b940dffc18642ab5a68d8b7a8502"
EMBEDDED = {"11400000": "font", "4243204929466537824": "default",
            "-525849521160799334": "texture"}


def migrate():
    entries = dict(line.split("=", 1) for line in Path(__file__).with_name("title-asset-map.txt").read_text().splitlines())
    changes = []
    for folder in ("Scenes", "Prefabs"):
        for path in sorted((ROOT / "Assets/_Project" / folder).rglob("*")):
            if path.suffix not in (".unity", ".prefab"):
                continue
            before = path.read_bytes()
            after = before
            counts = {}
            for old_id, kind in EMBEDDED.items():
                guid, local_id = entries[kind].split(":")
                old = f"fileID: {old_id}, guid: {OLD}".encode()
                new = f"fileID: {local_id}, guid: {guid}".encode()
                counts[kind] = after.count(old)
                after = after.replace(old, new)
            for old_guid, new_guid in entries.items():
                if len(old_guid) != 32:
                    continue
                old = ("guid: " + old_guid).encode()
                counts[old_guid] = after.count(old)
                after = after.replace(old, ("guid: " + new_guid).encode())
            if after == before:
                continue
            assert OLD.encode() not in after, "Unrecognized old font subasset reference"
            path.write_bytes(after)
            changes.append({"path": str(path.relative_to(ROOT)).replace("\\", "/"),
                            "before_sha256": hashlib.sha256(before).hexdigest(),
                            "after_sha256": hashlib.sha256(after).hexdigest(),
                            "reference_counts": {k: v for k, v in counts.items() if v}})
    Path(__file__).with_name("title-reference-migration.json").write_text(json.dumps(changes, indent=2) + "\n")
    print("Rebound font/material references in", len(changes), "scene/prefab assets only")


if __name__ == "__main__":
    migrate()
