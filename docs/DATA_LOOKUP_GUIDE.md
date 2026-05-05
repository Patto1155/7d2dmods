# Vanilla Data Lookup Guide

This repo includes a seed machine-readable dataset so future agents do not waste time repeatedly searching vanilla game XML or wiki pages.

Dataset root:

```text
db/datasets/7dtd-vanilla/
```

## Current files

```text
db/datasets/7dtd-vanilla/
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

## Source of truth

Use the installed game XML, not Fandom/wiki pages, whenever possible:

```text
/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/blocks.xml
/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/items.xml
/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/recipes.xml
/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/Localization.txt
```

## How future agents should use the dataset

Before opening large XML files, use the helper scripts or the derived JSON first.

Preferred commands:

```bash
python3 tools/validate_dataset.py
python3 tools/extract_vanilla_data.py --check
python3 tools/query_vanilla_data.py item resourceForgedIron
python3 tools/query_vanilla_data.py workstation forge
python3 tools/query_vanilla_data.py recipe workbench
```

Direct file lookups still work when you need a one-off inspection:

- Need vanilla workstation ids? Read `derived/workstations.json`.
- Need starter item ids? Read `derived/items.json`.
- Need recipe structure? Read `derived/recipes.json` and `schema.json`.
- Need localization or provenance details? Read `derived/localization.json` and `raw/provenance.md`.

Only return to the raw game XML when the needed field is missing or suspected stale.

## Desired mature dataset shape

The seed dataset is intentionally small. It should grow toward:

```text
derived/
  blocks.json
  items.json
  recipes.json
  workstations.json
  localization.json
  crafting_areas.json
  unlocks.json
  icons_models.json
```

Every derived record should preserve:

- internal id
- display name, if localized
- source file
- source XML path/context where feasible
- mod/game version context
- notes/confidence if field is inferred

## Required normalization rules

- Use internal ids as primary keys.
- Store display names separately.
- Do not use display names as recipe ingredient ids.
- Preserve counts as numbers.
- Preserve craft area/workstation requirements.
- Preserve unknown fields as null or omit them; do not invent values.
- Add provenance whenever a derived field is inferred.

## Query examples for agents

Read a file directly:

```bash
python3 - <<'PY'
import json
from pathlib import Path
p = Path('/mnt/c/Users/Administrator/source/repos/7d2dmods/db/datasets/7dtd-vanilla/derived/workstations.json')
data = json.loads(p.read_text())
for row in data:
    print(row.get('id'), row.get('display_name'))
PY
```

Find recipes for a craft area once full extraction exists:

```bash
python3 - <<'PY'
import json
from pathlib import Path
p = Path('/mnt/c/Users/Administrator/source/repos/7d2dmods/db/datasets/7dtd-vanilla/derived/recipes.json')
recipes = json.loads(p.read_text())
for r in recipes:
    if r.get('craft_area') == 'workbench':
        print(r.get('id'), '->', r.get('output'))
PY
```

Search for an item by display/internal id:

```bash
python3 - <<'PY'
import json
from pathlib import Path
needle = 'forged'.lower()
p = Path('/mnt/c/Users/Administrator/source/repos/7d2dmods/db/datasets/7dtd-vanilla/derived/items.json')
items = json.loads(p.read_text())
for item in items:
    text = (item.get('id','') + ' ' + item.get('display_name','')).lower()
    if needle in text:
        print(item)
PY
```

## Dataset expansion checklist

- [ ] Write repeatable extraction script under root-level `tools/` (`/mnt/c/Users/Administrator/source/repos/7d2dmods/tools/`).
- [ ] Parse XML deterministically.
- [ ] Join localization display names.
- [ ] Generate stable sorted JSON for readable diffs.
- [ ] Include extraction timestamp/game version in metadata.
- [ ] Validate output against `schema.json` or a Python validator.
- [ ] Update this guide when adding new derived files.
- [ ] Commit data updates separately from gameplay code when possible.

## Why this matters

The user explicitly wants this repo to become an agent-friendly knowledge base. Future coding models should not spend dozens of tool calls rediscovering:

- item ids
- block ids
- recipe ingredients
- workstation requirements
- localization keys
- prefab/icon/model references
- which files contain the truth

If you discover a useful lookup, record it here or in the dataset.
