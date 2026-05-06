# Agent Start Here

This repo is intentionally organized so a coding agent can start quickly without repeatedly scanning the whole 7 Days to Die install, wiki pages, or old scratch files.

## Mission

Build a general 7 Days to Die logistics and automation mod inspired by block-network storage systems such as Refined Storage, but designed for 7DTD.

The old `AutoForge` prototype is retired as active gameplay. The current mod should not center on a custom `Auto Forge` workstation; it should connect vanilla workstations through Wasteland Logistics conduits/connectors.

## Read order for future agents

Read these first, in this order:

1. `docs/REDESIGN_SPEC.md` — product and architecture target.
2. `docs/IMPLEMENTATION_CHECKLIST.md` — long task checklist and operator decision gates.
3. `docs/LOGISTICS_NETWORK_NEXT_STEPS.md` — compact current roadmap and recent review notes.
4. `docs/API_REFERENCE.md` — known 7DTD/Harmony/C# API notes and places where the API still needs verification.
5. `docs/DATA_LOOKUP_GUIDE.md` — how to use the in-repo vanilla game data instead of searching every time.
6. `docs/REPO_MAP.md` — where things live.
7. `docs/LESSONS_LEARNED.md` — why the redesign exists and what to avoid.
8. `mods/LogisticsNetwork/README.md` — active mod overview and build instructions.

## Current design decisions

These are already decided unless the operator says otherwise:

- Scrap the custom `autoForgeWorkstation` as the centerpiece.
- Keep conduits as the core world-network block concept.
- Use vanilla 7DTD workstations: forge, workbench, campfire, cement mixer, chemistry station, and later other compatible vanilla blocks.
- Prefer connector/import/export/filter blocks over replacing vanilla workstation classes.
- Store discovered game data in `db/datasets/7dtd-vanilla/` so future agents can query it directly.
- Use vanilla XML from the installed game as source of truth, not Fandom/wiki pages.
- Start single-player/local-first unless the operator chooses to prioritize multiplayer sync.

## Do not waste time on these

- Do not browse Fandom first; it may be blocked and can be stale.
- Do not scan the entire Steam directory unless a targeted path fails.
- Do not inspect compiled artifacts unless debugging a build result.
- Do not preserve or commit tokens, credentials, or local auth files.
- Do not build a huge custom UI as the first implementation step.
- Do not implement belts/inserters unless the operator explicitly pivots the design.

## Useful local paths

Repo:
- `/mnt/c/Users/Administrator/source/repos/7d2dmods`

Active mod install target:
- `/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/LogisticsNetwork/`

If an old `Mods/AutoForge/` install exists, remove/disable it before testing Wasteland Logistics so the retired Auto Forge workstation does not appear in the creative menu.

Vanilla game config source of truth:
- `/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/`

Known vanilla config files:
- `blocks.xml`
- `items.xml`
- `recipes.xml`
- `Localization.txt`

## GitHub auth note

In this Hermes environment, GitHub auth may be in the user's normal WSL home rather than Hermes' profile home. For pushes, use:

```bash
HOME=/home/dministrator git -C /mnt/c/Users/Administrator/source/repos/7d2dmods push
```
