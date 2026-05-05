#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

from vanilla_data import (
    DATASET_ROOT,
    DEFAULT_CONFIG_DIR,
    build_dataset,
    load_dataset_from_derived,
    print_record_summary,
    query_dataset,
)


ALIASES = {
    "item": "items",
    "items": "items",
    "block": "blocks",
    "blocks": "blocks",
    "workstation": "workstations",
    "workstations": "workstations",
    "recipe": "recipes",
    "recipes": "recipes",
    "localization": "localization",
    "loc": "localization",
}


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Query the tracked vanilla 7DTD dataset.")
    parser.add_argument("entity", help="Entity to query: item, block, workstation, recipe, or localization.")
    parser.add_argument("needle", help="Search text to match against ids, display names, and selected fields.")
    parser.add_argument(
        "--dataset-root",
        type=Path,
        default=DATASET_ROOT,
        help="Dataset root directory that contains derived/.",
    )
    parser.add_argument(
        "--source-dir",
        type=Path,
        default=DEFAULT_CONFIG_DIR,
        help="Optional vanilla Data/Config directory to extract from if derived data is unavailable.",
    )
    parser.add_argument("--limit", type=int, default=20, help="Maximum number of matches to print.")
    parser.add_argument("--json", action="store_true", help="Emit JSON instead of formatted text.")
    return parser.parse_args()


def load_query_dataset(dataset_root: Path, source_dir: Path) -> dict[str, list[dict[str, object]]]:
    derived_root = dataset_root / "derived"
    if derived_root.exists() and any((derived_root / name).exists() for name in ("items.json", "blocks.json", "recipes.json")):
        return load_dataset_from_derived(dataset_root)
    if source_dir.exists():
        return build_dataset(source_dir)
    return load_dataset_from_derived(dataset_root)


def main() -> int:
    args = parse_args()
    entity = ALIASES.get(args.entity.lower())
    if entity is None:
        print(f"Unknown entity: {args.entity}", file=sys.stderr)
        return 2

    dataset = load_query_dataset(args.dataset_root, args.source_dir)
    matches = query_dataset(dataset, entity, args.needle, limit=args.limit)

    if args.json:
        print(json.dumps(matches, indent=2, ensure_ascii=False, sort_keys=True))
        return 0

    if not matches:
        print(f"No {entity} matches for {args.needle!r}")
        return 0

    print(f"{len(matches)} {entity} match(es) for {args.needle!r}")
    for record in matches:
        print(print_record_summary(entity, record))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
