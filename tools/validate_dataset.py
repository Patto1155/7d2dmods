#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

from vanilla_data import DATASET_ROOT, load_dataset_from_derived, validate_dataset


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate the tracked vanilla 7DTD dataset.")
    parser.add_argument(
        "--dataset-root",
        type=Path,
        default=DATASET_ROOT,
        help="Dataset root directory that contains derived/.",
    )
    parser.add_argument("--json", action="store_true", help="Emit validation results as JSON.")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    dataset = load_dataset_from_derived(args.dataset_root)
    issues = validate_dataset(dataset)

    if args.json:
        import json

        print(json.dumps({"ok": not issues, "issues": issues}, indent=2, ensure_ascii=False, sort_keys=True))
    else:
        if issues:
            print("Dataset validation failed:", file=sys.stderr)
            for issue in issues:
                print(f"- {issue}", file=sys.stderr)
        else:
            print(f"Dataset validation passed for {args.dataset_root / 'derived'}")
            for entity, records in dataset.items():
                print(f"- {entity}: {len(records)} records")

    return 0 if not issues else 1


if __name__ == "__main__":
    raise SystemExit(main())
