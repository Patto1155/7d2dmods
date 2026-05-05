# 7DTD Vanilla Data

This dataset is a machine-readable seed for future agents working on the 7 Days to Die logistics/automation mod.

## Purpose

- Fast lookup of vanilla workstations, items, recipes, and source files.
- Support mod design and implementation without repeatedly scraping game files.
- Preserve source/provenance so derived facts can be trusted or re-generated.
- Give coding agents a stable local database before they inspect large XML files.

## Current scope

Seed data currently includes:

- a starter set of workstation blocks
- a starter set of resource items
- representative recipe/source metadata
- schema, localization, and provenance notes

This dataset is intentionally conservative. Expand it from game XML as implementation needs grow.

## Primary source of truth

Installed vanilla config directory:

```text
D:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/
```

WSL path:

```text
/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/
```

Important source files:

- `blocks.xml`
- `items.xml`
- `recipes.xml`
- `Localization.txt`

Use game XML over wiki text whenever possible.

## Files

```text
README.md
schema.json
query_examples.md
raw/
  provenance.md
derived/
  blocks.json
  items.json
  recipes.json
  workstations.json
  localization.json
```

## How to use for coding

Before opening vanilla XML, check the derived JSON:

- Workstation ids and starter station notes: `derived/workstations.json`
- Item ids and starter item metadata: `derived/items.json`
- Recipe examples and shape: `derived/recipes.json`
- Expected fields: `schema.json`
- Query examples: `query_examples.md`

Recommended code-agent flow:

1. Read `docs/DATA_LOOKUP_GUIDE.md`.
2. Run `python3 tools/validate_dataset.py` if you want a quick integrity check.
3. Use `python3 tools/query_vanilla_data.py <entity> <needle>` to inspect the derived dataset.
4. Only inspect raw vanilla XML if the derived dataset is missing a field or looks stale.
5. If new data is extracted, run `python3 tools/extract_vanilla_data.py` and commit the dataset update separately from gameplay code.

## Mapping to mod implementation

The logistics mod needs this data for:

- identifying vanilla workstation block ids
- mapping recipes to required crafting areas/workstations
- resolving ingredient internal ids
- showing display names in docs/UI
- finding icons/models/prefabs later
- validating filters and recipe patterns

Expected runtime architecture:

- Development-time tools parse this dataset.
- Mod XML/C# may copy or generate only the data needed at runtime.
- Do not make the game depend on Python tooling at runtime.

## Expansion priorities

1. Full recipe extraction from `recipes.xml`.
2. Full item extraction from `items.xml`.
3. Full block/workstation extraction from `blocks.xml`.
4. Localization join from `Localization.txt`.
5. Icons/models/prefab references.
6. Unlock/progression requirements.
7. Query helper scripts under `tools/`.

## Notes

- Data is intentionally conservative and only includes fields we have verified or clearly marked.
- Expand from game XML, not wiki text, whenever possible.
- Preserve internal ids separately from display names.
- Keep generated JSON stable/sorted for readable diffs.
