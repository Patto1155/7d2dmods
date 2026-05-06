# 7d2dmods

Private repo for 7 Days to Die mod work.

Start here if you are an agent:

1. `docs/AGENT_START_HERE.md`
2. `docs/REPO_MAP.md`
3. The active mod folder under `mods/LogisticsNetwork/`

Current active mod included in this repo:

- Wasteland Logistics (`mods/LogisticsNetwork/`) — a general logistics network for vanilla 7DTD workstations and storage.

Important direction:

- Do not build around the old Auto Forge workstation.
- Any remaining `mods/AutoForge/` folder is legacy prototype material only and should not be installed as an active mod.
- The target design is connector/conduit logistics for vanilla workstations: forge, workbench, campfire, cement mixer, chemistry station, and compatible storage blocks.

Repo layout:

- `mods/LogisticsNetwork/` — active source, config, and build files for Wasteland Logistics.
- `docs/` — agent-facing notes, repo map, implementation checklist, API notes, and design docs.
- `db/datasets/7dtd-vanilla/` — local vanilla data to avoid repeated XML/wiki searches.
- `tools/` — dataset/query/validation helpers.
