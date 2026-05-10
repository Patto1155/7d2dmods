# Logistics Network Next Steps

> Compact plan for future agents. Read this after `docs/IMPLEMENTATION_CHECKLIST.md` and `docs/REDESIGN_SPEC.md`.

## Current state

Completed locally and pushed:

- `mods/LogisticsNetwork/` skeleton
- conduit block
- connector block plus **importer**, **exporter**, and **filter** role blocks (XML/recipes/localization; scan + passive routing; filter block is policy placeholder until wired)
- registry pruning + throttled reflection bootstrap for empty-registry recovery
- depth-limit truncation flag + topology hash in scan logs
- registry + passive network scanner
- passive `NetworkEndpoint` / `StorageEndpoint` resolution logs for scanned storage tiles; `WorkstationEndpoint` + `WorkstationOutputProbe` logs for workstation tiles; `NetworkConnectorSnapshot` logs for connector adjacency
- **Passive routing:** `ItemRoutingService` pairs importer→exporter connectors (deterministic priority + coordinates); route-plan `filterMode` mirrors `LogisticsNetworkFeatures.ItemTransferFilterMode` / ids (same semantics as live transfer filters).
- **Experimental live storage→storage transfer** (default **off**): `LogisticsNetworkFeatures.EnableLiveStorageTransfer` — at most one item per tick; importer adjacent to source chest, exporter adjacent to dest chest; placement empty → `TryStackItem` (tuple interpreted + logged on failure) → `AddItem`; richer `skip:*` reasons when placement fails.
- **Experimental workstation output extraction** (default **off**): `LogisticsNetworkFeatures.EnableLiveWorkstationOutputExtraction` — pulls one unit per tick from `TileEntityWorkstation.Output` (workbench / campfire / cement mixer / chemistry) into a destination chest; reuses the storage placement contract; pauses while a player has the station UI open (`skip:workstation_user_accessing`); rejects `TileEntityForge` until its single-stack output is verified separately. Inputs / fuel / tools are never read or written.
- 2-second tick loop
- basic docs/checklist updates

**Inventory icons:** addressed for MVP via `Config/items.xml` + block `CustomIcon` (Phase 3b largely done; custom art still optional).

The current implementation is intentionally passive/local-first and should not claim inventory automation or multiplayer support yet unless verified in-game.

## Immediate next milestones

1. **Validate experimental storage transfer in SP** (then dedicated server): enable live transfer, whitelist a test item, confirm logs and no dup/loss across save/reload.

1b. **Validate experimental workstation output extraction in SP**: enable `EnableLiveWorkstationOutputExtraction`, place importer beside workbench/campfire/cement mixer/chemistry, finish a craft, confirm only `Output[]` is moved, inputs/fuel/tools/queue are intact, and that opening the station UI pauses extraction with `skip:workstation_user_accessing`. Check save/reload for dup/loss. Forge is intentionally skipped at this slice.

2. Harden the scanner/bootstrap path (ongoing)
   - prune or verify registry entries against world state
   - keep scan output stable and easy to compare in logs

3. Storage endpoint support (partial — live chest→chest exists behind flag; overflow routing & multi-route reliability still open)

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
- Connectors exist as first-class scanned nodes; **importer** / **exporter** / **filter** blocks are implemented as distinct block types with passive routing roles (`NetworkConnectorSnapshot.Role`). Live moves only run for storage↔storage pairs with importer at source connector and exporter at destination connector.

## Phase 19 snapshot (not acceptance)

Full checklist: `docs/IMPLEMENTATION_CHECKLIST.md` Phase 19. Rough progress: scaffold + passive scan/logs are strong; **reliable** sorting, workstation I/O, recipe-driven behavior, MP verification, and dup/loss sign-off remain ahead. Treat percentages in chat as informal unless tied to checklist rows.

## Guardrails for future work

- Keep the mod passive/local-first until a feature is explicitly verified.
- Do not claim multiplayer support until tested.
- Do not jump to a full custom UI early.
- Use the in-repo vanilla dataset/docs before searching the web again.
- Treat `mods/AutoForge/` as legacy reference, not the active design target.
