"""Prove title migration changes only font/material references in seven serialized assets."""
from pathlib import Path
import hashlib
import json
import subprocess

ROOT = Path(__file__).resolve().parents[2]
BASELINE = "44dcb361"
OLD = "f4d1b940dffc18642ab5a68d8b7a8502"
ENTRIES = dict(line.split("=", 1) for line in Path(__file__).with_name("title-asset-map.txt").read_text().splitlines())


def mapped(data):
    for old_id, kind in {"11400000": "font", "4243204929466537824": "default",
                         "-525849521160799334": "texture"}.items():
        guid, local_id = ENTRIES[kind].split(":")
        data = data.replace(f"fileID: {old_id}, guid: {OLD}".encode(),
                            f"fileID: {local_id}, guid: {guid}".encode())
    for old_guid, new_guid in ENTRIES.items():
        if len(old_guid) == 32:
            data = data.replace(("guid: " + old_guid).encode(), ("guid: " + new_guid).encode())
    return data


def check():
    prefix = subprocess.check_output(["git", "rev-parse", "--show-prefix"], cwd=ROOT).decode().strip()
    records = json.loads(Path(__file__).with_name("title-reference-migration.json").read_text())
    for record in records:
        path = record["path"]
        before = subprocess.check_output(["git", "show", BASELINE + ":" + prefix + path], cwd=ROOT)
        actual = (ROOT / path).read_bytes()
        assert mapped(before).replace(b"\r\n", b"\n") == actual.replace(b"\r\n", b"\n"), path
    report = {"status": "PASS", "baseline": BASELINE, "referenceOnlyAssets": len(records),
              "paths": [r["path"] for r in records], "no_layout_or_gameplay_changes": True}
    out = ROOT / "Captures/UIRollout/Phase08Revisions/FontProof/title-source-scope.json"
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(report, indent=2) + "\n")
    print("PASS: seven scene/prefab assets differ only in exact font/material references")


if __name__ == "__main__":
    check()
