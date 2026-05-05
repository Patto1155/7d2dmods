# Repository Map

## Current content

### `mods/AutoForge/`

Legacy prototype reference for the original mod.

This folder is preserved so agents can reuse working patterns, but it should not be treated as the final product architecture.

Important files:

- `README.md` — original mod handover / usage notes.
- `ModInfo.xml` — prototype mod metadata loaded by the game.
- `build.bat` — local build helper.
- `Source/AutoForge.csproj` — .NET 4.8 project file.
- `Source/AutoForgeMod.cs` — prototype entry point.
- `Source/AutoForgeRegistry.cs` — registry for placed Auto Forge blocks.
- `Source/AutoForgeTick.cs` — tick/update logic shell.
- `Source/Blocks/AutoForgeBlock.cs` — legacy custom workstation behavior; should be retired as centerpiece.
- `Source/Blocks/ConduitBlock.cs` — useful conduit behavior shell.
- `Source/Network/ConduitNetwork.cs` — useful connected-network BFS search.
- `Source/Patches/HarmonyPatches.cs` — patch placeholder.
- `Config/blocks.xml` — prototype block definitions.
- `Config/recipes.xml` — prototype crafting recipes.
- `Config/localization.txt` — prototype names and descriptions.
- `Config/XUi/windows.xml` — prototype UI hooks/notes.

### `mods/LogisticsNetwork/`

Planned active development directory for the redesigned mod.

This directory may not exist yet. Create it when implementation begins unless the operator chooses a different mod/internal name.

Target purpose:

- conduits
- connectors
- importer/exporter/filter blocks
- vanilla workstation endpoints
- storage routing
- recipe/pattern automation

### `docs/`

Agent-facing documentation and future design notes.

Important files:

- `AGENT_START_HERE.md` — first read for fresh agents.
- `REDESIGN_SPEC.md` — target product and architecture.
- `IMPLEMENTATION_CHECKLIST.md` — long implementation plan with operator decision gates.
- `API_REFERENCE.md` — verified/suspected 7DTD C# and XML API facts.
- `DATA_LOOKUP_GUIDE.md` — how to use the in-repo vanilla dataset.
- `LESSONS_LEARNED.md` — historical notes from the initial prototype.
- `REPO_MAP.md` — this file.

### `db/datasets/7dtd-vanilla/`

Machine-readable seed dataset for vanilla game data.

Purpose:

- avoid repeated searching of game XML/wiki
- provide internal ids/display names/recipe data/workstation data
- preserve provenance for future extraction

Important files:

- `README.md`
- `schema.json`
- `query_examples.md`
- `raw/provenance.md`
- `derived/workstations.json`
- `derived/items.json`
- `derived/recipes.json`

### Future root `tools/`

Planned root-level location: `tools/` at the repository root, not inside `db/` or a mod folder.

Purpose: reusable extraction/query/validation scripts that help agents work with game data and repo checks. Runtime game code should not depend on these Python tools.

Suggested future files:

- `tools/extract_vanilla_data.py`
- `tools/query_vanilla_data.py`
- `tools/validate_dataset.py`

Do not commit one-off scratch scripts unless they are cleaned up and documented.

## What was intentionally not copied

- `AutoForge.dll`
- `AutoForge.pdb`
- `Source/obj/`
- scratch inspection scripts like `inspect2.py`, `inspect3.py`, `inspect_sig.py`
- credentials or local auth files

## Suggested next steps

1. Confirm operator decisions in `docs/IMPLEMENTATION_CHECKLIST.md`.
2. Expand `db/datasets/7dtd-vanilla/` with repeatable extraction scripts.
3. Create `mods/LogisticsNetwork/` as a clean sibling to legacy AutoForge.
4. Port conduit/network scanning concepts.
5. Implement storage routing before risky workstation recipe queue automation.
6. Update docs as API facts are verified.
