# Repository Map

## Current content

### `mods/LogisticsNetwork/`

Active development directory for the redesigned mod.

Player-facing name: `Wasteland Logistics`.

Purpose:

- logistics conduits
- logistics connectors
- storage endpoints
- vanilla workstation endpoints
- future importer/exporter/filter behavior
- future recipe/pattern automation after API verification

Important files:

- `README.md` — active mod overview and build instructions.
- `ModInfo.xml` — active mod metadata loaded by the game.
- `build.bat` — Windows build helper.
- `Config/blocks.xml` — active block definitions.
- `Config/recipes.xml` — active recipes.
- `Config/localization.txt` — active names/descriptions.
- `Source/LogisticsNetwork.csproj` — C# project file.
- `Source/LogisticsNetworkMod.cs` — mod entry point.
- `Source/Blocks/LogisticsConduitBlock.cs` — passive network conduit.
- `Source/Blocks/LogisticsConnectorBlock.cs` — MVP connector block.
- `Source/Network/NetworkRegistry.cs` — placed logistics block registry.
- `Source/Network/NetworkScanner.cs` — BFS/scanner/bootstrap logic.
- `Source/Network/NetworkGraph.cs` — graph snapshot model.
- `Source/Network/NetworkEndpoint.cs` — endpoint abstraction.
- `Source/Network/StorageEndpoint.cs` — passive storage endpoint snapshot.
- `Source/Tick/LogisticsNetworkTick.cs` — throttled scan tick.

### Legacy AutoForge prototype

The old AutoForge prototype created a custom `Auto Forge` workstation and a forge-specific conduit. That concept is retired as the active gameplay direction.

If a `mods/AutoForge/` folder exists in a local checkout or game install, treat it as legacy only. Do not install it alongside `mods/LogisticsNetwork/` unless intentionally testing the old prototype, because it exposes the unwanted `Auto Forge` creative-menu item.

The useful ideas from AutoForge have already been ported or documented:

- Harmony entry point pattern
- throttled tick loop
- placed-block registry
- conduit block shell
- BFS network scanning
- vanilla prefab usage

Use git history for old AutoForge source if needed; do not continue development there.

### `docs/`

Agent-facing documentation and future design notes.

Important files:

- `AGENT_START_HERE.md` — first read for fresh agents.
- `REDESIGN_SPEC.md` — target product and architecture.
- `IMPLEMENTATION_CHECKLIST.md` — long implementation plan with operator decision gates.
- `LOGISTICS_NETWORK_NEXT_STEPS.md` — compact current roadmap and review notes.
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
- `derived/blocks.json`
- `derived/recipes.json`
- `derived/localization.json`

### `tools/`

Root-level extraction/query/validation scripts that help agents work with game data and repo checks. Runtime game code should not depend on these Python tools.

Important files:

- `tools/extract_vanilla_data.py`
- `tools/query_vanilla_data.py`
- `tools/validate_dataset.py`
- `tools/vanilla_data.py`

Do not commit one-off scratch scripts unless they are cleaned up and documented.

## What should not be committed

- compiled mod DLLs/PDBs unless explicitly requested as release artifacts
- `Source/obj/`
- scratch inspection scripts
- credentials or local auth files
- active development under `mods/AutoForge/`

## Suggested next steps

1. Keep `mods/LogisticsNetwork/` as the only active mod.
2. Continue scanner/bootstrap hardening and endpoint verification.
3. Implement storage routing before risky workstation recipe queue automation.
4. Use connectors to expose vanilla workstations instead of adding replacement workstation blocks.
5. Update docs as API facts are verified.
