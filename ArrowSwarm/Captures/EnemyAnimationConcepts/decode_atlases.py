"""Inspect alpha components to decode sprite rectangles without changing source art."""
from collections import defaultdict
from pathlib import Path
import hashlib
import json

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parent
ATLASES = ["01-kristal-hayalet.png", "02-yaramaz-vizildayan.png", "03-jole-kristal.png"]


def alpha_components(mask):
    """Return eight-connected component pixel counts and half-open bounding boxes."""
    parents, runs = [], []

    def find(item):
        while parents[item] != item:
            parents[item] = parents[parents[item]]
            item = parents[item]
        return item

    previous = []
    for y, row in enumerate(mask):
        padded = np.concatenate(([False], row, [False])).astype(np.int8)
        transitions = np.flatnonzero(np.diff(padded))
        current = []
        for start, end in zip(transitions[::2], transitions[1::2]):
            identity = len(parents)
            parents.append(identity)
            record = (int(start), int(end), y, identity)
            current.append(record)
            runs.append(record)
            for old_start, old_end, _, old_id in previous:
                if old_end < start:
                    continue
                if old_start > end:
                    break
                first, second = find(identity), find(old_id)
                if first != second:
                    parents[max(first, second)] = min(first, second)
        previous = current

    groups = defaultdict(lambda: {"area": 0, "box": [mask.shape[1], mask.shape[0], 0, 0]})
    for start, end, y, identity in runs:
        component = groups[find(identity)]
        component["area"] += end - start
        box = component["box"]
        box[0], box[1] = min(box[0], start), min(box[1], y)
        box[2], box[3] = max(box[2], end), max(box[3], y + 1)
    return sorted(groups.values(), key=lambda value: value["area"], reverse=True)


def overlap(first, second):
    return max(0, min(first[2], second[2]) - max(first[0], second[0])) * max(
        0, min(first[3], second[3]) - max(first[1], second[1]))


def ordered(components):
    """Group the sixteen independent bodies into four rows, then sort each row."""
    by_y = sorted(components, key=lambda item: (item["box"][1] + item["box"][3]) / 2)
    return [component for row in range(4) for component in sorted(
        by_y[row * 4:row * 4 + 4], key=lambda item: item["box"][0])]


def decode(filename):
    path = ROOT / filename
    fingerprint = hashlib.sha256(path.read_bytes()).hexdigest()
    image = Image.open(path).convert("RGBA")
    alpha = np.asarray(image)[..., 3]
    core_components = alpha_components(alpha >= 16)
    cores = ordered([item for item in core_components if item["area"] >= 10000])
    assert len(cores) == 16, f"Expected sixteen separate main bodies: {filename}"

    # Faint antialiasing can connect bodies across a gutter. Choose the lowest
    # alpha threshold that retains sixteen independent main components, without
    # recoloring, masking, cropping, or writing any source image pixels.
    for threshold in (1, 2, 4, 8, 16):
        components = alpha_components(alpha >= threshold)
        bodies = [item for item in components if item["area"] >= 10000]
        if len(bodies) == 16:
            bodies = ordered(bodies)
            disjoint = all(not overlap(bodies[first]["box"], bodies[second]["box"])
                           for first in range(16) for second in range(first + 1, 16))
            if disjoint and all(overlap(core["box"], body["box"]) ==
                   (core["box"][2] - core["box"][0]) * (core["box"][3] - core["box"][1])
                   for core, body in zip(cores, bodies)):
                break
    else:
        raise AssertionError(f"Could not isolate sprite bodies: {filename}")

    # Genuine detached tail bubbles have substantial, compact round components.
    # Associate these core components with the nearest main body rectangle.
    bubbles = [[] for _ in range(16)]
    for item in core_components:
        left, top, right, bottom = item["box"]
        density = item["area"] / ((right - left) * (bottom - top))
        if not 150 <= item["area"] < 10000 or density < .55:
            continue
        x, y = (left + right) / 2, (top + bottom) / 2

        def distance(core):
            x0, y0, x1, y1 = core["box"]
            return max(x0 - x, 0, x - x1) ** 2 + max(y0 - y, 0, y - y1) ** 2

        nearest = min(range(16), key=lambda index: distance(cores[index]))
        assert distance(cores[nearest]) <= 64 ** 2, "Detached shape too far from its owner"
        bubbles[nearest].append(item["box"])

    rects = []
    for body, attached in zip(bodies, bubbles):
        boxes = [body["box"], *attached]
        x0, y0 = min(box[0] for box in boxes), min(box[1] for box in boxes)
        x1, y1 = max(box[2] for box in boxes), max(box[3] for box in boxes)
        rects.append({"x": x0, "y": y0, "w": x1 - x0, "h": y1 - y0})
    boxes = [[r["x"], r["y"], r["x"] + r["w"], r["y"] + r["h"]] for r in rects]
    assert all(not overlap(boxes[first], boxes[second]) for first in range(16)
               for second in range(first + 1, 16)), f"Sprite rectangles overlap: {filename}"
    assert hashlib.sha256(path.read_bytes()).hexdigest() == fingerprint, "Source art changed"
    return {
        "atlas_width": image.width, "atlas_height": image.height,
        "max_width": max(rect["w"] for rect in rects),
        "max_height": max(rect["h"] for rect in rects),
        "alpha_threshold": threshold, "core_alpha_threshold": 16,
        "main_body_count": len(cores), "detached_tail_bubbles": sum(map(len, bubbles)),
        "tail_bubble_frames": [index for index, group in enumerate(bubbles) if group],
        "rectangles_overlap": False, "source_sha256": fingerprint,
        "frame_order": "left-to-right, top-to-bottom", "rects": rects,
    }


def main():
    result = {filename: decode(filename) for filename in ATLASES}
    destination = ROOT / "frame-rects.json"
    destination.write_text(json.dumps(result, indent=2), encoding="utf-8")
    for filename, metadata in result.items():
        print(f"{filename}: 16 bodies, alpha >= {metadata['alpha_threshold']}, "
              f"{metadata['detached_tail_bubbles']} detached bubbles, "
              f"max {metadata['max_width']}x{metadata['max_height']}, no rectangle overlap")
    print(f"PASS: {destination}; source images unchanged")


if __name__ == "__main__":
    main()
