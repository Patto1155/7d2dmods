# Logistics Network Next Steps

> Compact plan for future agents. Read this after `docs/IMPLEMENTATION_CHECKLIST.md` and `docs/REDESIGN_SPEC.md`.

## Current state

Completed locally and pushed:

- `mods/LogisticsNetwork/` skeleton
- conduit block
- connector block (scan/registry shell; same passive behavior as conduits)
- storage endpoint metadata wrapper (`StorageEndpoint` / `NetworkEndpoint`) with chunk/null guards and slot-count logging (no item IO)
- registry pruning + throttled reflection bootstrap for empty-registry recovery
- depth-limit truncation flag + topology hash in scan logs
- registry + passive network scanner
- 2-second tick loop
- basic docs/checklist updates

The current implementation is intentionally passive/local-first and should not claim inventory automation or multiplayer support yet unless verified in-game.

## Immediate next milestones

1. Harden the scanner/bootstrap path
   - avoid one-shot bootstrap failure if the first scan happens before the world is ready
   - prune or verify registry entries against world state
   - make graph snapshots reflect real topology changes, not just counts
   - keep scan output stable and easy to compare in logs

2. Add connector abstraction
   - define the endpoint layer that touches vanilla blocks
   - separate conduits from connectors conceptually
   - keep the MVP simple: one block role first, not a large UI system

3. Storage endpoint support
   - done (passive): resolve `TileEntityLootContainer`, log type + slot count, guard unloaded chunks / null TE
   - next: read and mutate storage inventories safely once insert/extract APIs are verified
   - add insertion/extraction helpers

4. Workstation endpoint support
   - identify vanilla workstations through the network
   - inspect output slots first
   - only then consider ingredient feeding

5. Output extraction
   - pull completed items from stations
   - route to storage
   - verify no input/fuel/tool corruption

6. Ingredient feeding
   - push only required ingredients into stations
   - respect slot semantics and station-specific behavior
   - do not start with full autocrafting unless the API is verified

7. Recipe/pattern config
   - start with a small hardcoded or JSON-backed pattern set
   - keep recipe definitions agent-friendly and easy to inspect

8. Minimal UI, only if needed
   - prefer connector-driven configuration first
   - avoid a large custom UI until the core item movement is stable

9. Balance, localization, and hardening
   - tune counts and defaults
   - clean up names/text
   - document any API assumptions that remain unverified

## Review notes from the conduit/scanner milestone

- The scanner still uses reflection-based `World.GetTileEntities` discovery when the registry is empty; method signature drift across game versions remains an **unverified assumption** (throttled retries reduce permanent failure).
- Bootstrap retries on a timer when no registry seeds exist; repeated empty runs slow to 30s after several attempts to reduce reflection churn when no logistics blocks exist.
- A fixed scan-depth cap can truncate large networks; truncation is surfaced via `truncatedDepth=Y` on the graph summary when hit.
- Registry entries are pruned against live world blocks to reduce phantom nodes when removal events were missed.
- Tick snapshots hash sorted conduit/connector/storage/workstation positions so topology changes log even when counts stay the same.
- Connectors exist as a first-class scanned node (`logisticsConnector`); importer/exporter/filter roles remain future work.

## Guardrails for future work

- Keep the mod passive/local-first until a feature is explicitly verified.
- Do not claim multiplayer support until tested.
- Do not jump to a full custom UI early.
- Use the in-repo vanilla dataset/docs before searching the web again.
- Treat `mods/AutoForge/` as legacy reference, not the active design target.
