#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

from vanilla_data import (
    DEFAULT_CONFIG_DIR,
    DATASET_ROOT,
    build_dataset,
    validate_dataset,
    write_dataset,
)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Extract vanilla 7 Days to Die data into the tracked dataset.")
    parser.add_argument(
        "--source-dir",
        type=Path,
        default=DEFAULT_CONFIG_DIR,
        help="Vanilla 7DTD Data/Config directory (defaults to the canonical Steam install path).",
    )
    parser.add_argument(
        "--dataset-root",
        type=Path,
        default=DATASET_ROOT,
        help="Dataset root directory that contains derived/ and raw/.",
    )
    parser.add_argument(
        "--check",
        action="store_true",
        help="Do not write files; validate the current derived dataset when the source is unavailable, or compare a fresh extraction in-memory when the source exists.",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    config_dir = args.source_dir
    source_exists = config_dir.exists()

    if not source_exists and not args.check:
        print(f"Source directory not found: {config_dir}", file=sys.stderr)
        print("Re-run with --check to validate the already-tracked derived dataset.", file=sys.stderr)
        return 2

    if source_exists:
        dataset = build_dataset(config_dir)
        issues = validate_dataset(dataset)
        if issues:
            print("Validation issues detected:", file=sys.stderr)
            for issue in issues:
                print(f"- {issue}", file=sys.stderr)
            if not args.check:
                return 1
        if args.check:
            print(f"Checked source extraction from {config_dir}")
            for entity, records in dataset.items():
                print(f"- {entity}: {len(records)} records")
            return 0 if not issues else 1
        write_dataset(dataset)
        print(f"Wrote dataset to {args.dataset_root / 'derived'}")
        for entity, records in dataset.items():
            print(f"- {entity}: {len(records)} records")
        return 0

    # Source is missing, but the user asked for a check on the tracked dataset.
    from vanilla_data import load_dataset_from_derived

    dataset = load_dataset_from_derived(args.dataset_root)
    issues = validate_dataset(dataset)
    if issues:
        print("Derived dataset validation issues detected:", file=sys.stderr)
        for issue in issues:
            print(f"- {issue}", file=sys.stderr)
        return 1

    print(f"Validated derived dataset at {args.dataset_root / 'derived'}")
    for entity, records in dataset.items():
        print(f"- {entity}: {len(records)} records")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
