"""Calculate only manually supplied KPI inputs; never contacts an analytics service."""
from __future__ import annotations
import argparse
import csv
import json
import math
from pathlib import Path


def calculate(row):
    """Return unavailable for missing inputs; preserve valid zeros."""
    value = row.get("numerator_value", "").strip()
    if not value:
        return {"metric": row["metric"], "value": None, "status": "NOT_MEASURED"}
    numerator = float(value)
    if not math.isfinite(numerator) or numerator < 0:
        raise ValueError(f"Invalid numerator for {row['metric']}")
    kind = row["kind"]
    if kind in ("count", "currency"):
        result = numerator
    else:
        denominator_text = row.get("denominator_value", "").strip()
        if not denominator_text:
            return {"metric": row["metric"], "value": None, "status": "DENOMINATOR_MISSING"}
        denominator = float(denominator_text)
        if not math.isfinite(denominator) or denominator < 0:
            raise ValueError(f"Invalid denominator for {row['metric']}")
        if denominator == 0:
            return {"metric": row["metric"], "value": None, "status": "DENOMINATOR_ZERO"}
        result = numerator / denominator
        if kind == "currency_per_1000":
            result *= 1000
        elif kind not in ("ratio", "currency_per_install"):
            raise ValueError(f"Unknown calculation kind: {kind}")
    return {"metric": row["metric"], "value": result,
            "status": "DERIVED_FROM_MANUAL_INPUTS",
            "period_start_UTC": row.get("period_start_UTC", ""),
            "period_end_UTC": row.get("period_end_UTC", ""),
            "segment": row.get("segment", "")}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, default=Path(__file__).with_name("KPI_DASHBOARD.csv"))
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    if args.input.resolve() == args.output.resolve():
        raise ValueError("Output must not overwrite the CSV input")
    with args.input.open(encoding="utf-8-sig", newline="") as source:
        rows = list(csv.DictReader(source))
    result = {"source": str(args.input), "connectedToLiveService": False,
              "results": [calculate(row) for row in rows]}
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")


if __name__ == "__main__":
    main()
