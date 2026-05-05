# Logistics Network Implementation Checklist

> For Hermes/coding agents: use `subagent-driven-development` for implementation. Dispatch focused subagents for reconnaissance, implementation, and review. Do not make one agent scan the whole game install repeatedly.
>
> See also: `docs/LOGISTICS_NETWORK_NEXT_STEPS.md` for the compact post-review milestone plan.

## Goal

Implement the full redesign from legacy `AutoForge` into a general vanilla-workstation logistics network.

## Status legend

Use this file as a live checklist.

- `[ ]` not started
- `[~]` in progress
- `[x]` done
- `[!]` blocked / needs operator decision

## Operator decision gates

Stop and ask the operator at these points unless they already answered in the current session.

### Decision A: Mod name

Default recommendation: `LogisticsNetwork` internally, player-facing name `Wasteland Logistics`.

Operator choices:

- `LogisticsNetwork` / `Wasteland Logistics`
- `Conduit Logistics`
- `Survivor Logistics Network`
- another name

### Decision B: MVP automation depth

Default recommendation: start with network-supplied ingredients + output extraction, then direct queue automation once API is verified.

Operator choices:

- MVP only supplies inputs and extracts outputs; player manually queues recipes.
- MVP directly selects/starts recipes from connector config.
- MVP attempts full recipe-pattern autocrafting immediately.

### Decision C: UI strategy

Default recommendation: connector-driven minimal configuration first.

Operator choices:

- no custom UI; use simple block roles/hardcoded behavior first
- connector UI only
- patch vanilla workstation UI with network tab
- full storage/crafting terminal UI

### Decision D: Power/progression

Default recommendation: no power requirement in first playable prototype.

Operator choices:

- no power requirement for now
- require vanilla electricity
- require a network controller block
- require fuel/maintenance item

### Decision E: Multiplayer priority

Default recommendation: single-player first, document sync assumptions.

Operator choices:

- single-player/local only for MVP
- server-safe dedicated multiplayer support before gameplay expansion
- defer multiplayer until after routing/crafting works

### Decision F: Storage network scope

Default recommendation: chest/container routing first, then workstation crafting.

Operator choices:

- storage sorting first
- workstation automation first
- both together in a thin vertical slice

## Phase 0: Repo orientation and safety

- [x] Preserve original AutoForge prototype under `mods/AutoForge/`.
- [x] Push initial prototype import to GitHub main.
- [x] Seed in-repo vanilla 7DTD dataset under `db/datasets/7dtd-vanilla/`.
- [x] Push dataset seed to GitHub main.
- [x] Create forward-looking redesign docs.
- [ ] Keep `mods/AutoForge/` untouched except for legacy notes unless the operator asks for migration edits.
- [ ] Never commit compiled DLLs/PDBs unless the operator explicitly wants release artifacts tracked.
- [ ] Never commit credentials or local GitHub auth files.

Verification:

```bash
git -C /mnt/c/Users/Administrator/source/repos/7d2dmods status --short
```

Expected: clean or only intentional doc/source changes.

## Phase 1: Expand the in-repo vanilla data lookup

Purpose: prevent future agents from repeatedly scraping XML/wiki.

- [x] Create root-level `tools/extract_vanilla_data.py` or equivalent script. Root-level means `/mnt/c/Users/Administrator/source/repos/7d2dmods/tools/`, not inside `db/` or a mod folder.
- [x] Script reads vanilla XML from `/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/`.
- [x] Parse `items.xml` into `db/datasets/7dtd-vanilla/derived/items.json`.
- [x] Parse `blocks.xml` into `db/datasets/7dtd-vanilla/derived/blocks.json`.
- [x] Parse `recipes.xml` into `db/datasets/7dtd-vanilla/derived/recipes.json`.
- [x] Parse `Localization.txt` into `db/datasets/7dtd-vanilla/derived/localization.json` or join display names into derived files.
- [x] Add provenance info for every derived file.
- [x] Normalize all ids to canonical internal ids, not display names.
- [x] Include display names as a separate field from localization.
- [x] Include recipe craft area/workstation requirement.
- [x] Include recipe ingredients and counts.
- [x] Include stack sizes where available.
- [x] Include block/item model/icon references where available.
- [ ] Include unlock/progression fields if found in vanilla XML.
- [x] Add a simple query helper script for agents, e.g. `tools/query_vanilla_data.py`.
- [x] Update `docs/DATA_LOOKUP_GUIDE.md` with query examples.
- [ ] Commit dataset expansion separately.

Verification:

```bash
python3 tools/extract_vanilla_data.py --check
python3 tools/query_vanilla_data.py recipe forgedIron
python3 tools/query_vanilla_data.py workstation forge
```

If Python script is not available yet, create it before using these commands.

## Phase 2: Create the new mod skeleton

Do not rewrite AutoForge in place unless the operator asks. Make a clean sibling mod.

- [x] Confirm operator decision A: mod name.
- [x] Create `mods/LogisticsNetwork/` or chosen name.
- [x] Create `mods/LogisticsNetwork/ModInfo.xml`.
- [x] Create `mods/LogisticsNetwork/build.bat` based on AutoForge build script.
- [x] Create `mods/LogisticsNetwork/README.md` explaining MVP.
- [x] Create `mods/LogisticsNetwork/Source/LogisticsNetwork.csproj`.
- [x] Reference the same required 7DTD assemblies used by AutoForge.
- [x] Create `Source/LogisticsNetworkMod.cs` entry point.
- [x] Register Harmony patches.
- [ ] Add tick scheduling similar to AutoForge but renamed/generalized.
- [x] Add minimal logging helper.
- [x] Add empty `Config/blocks.xml`, `recipes.xml`, `localization.txt` scaffolds.
- [x] Build once and document any missing references.
- [ ] Commit skeleton separately.

Notes:

- The skeleton build verified successfully with `cmd.exe /c "cd /d C:\Users\Administrator\source\repos\7d2dmods\mods\LogisticsNetwork && build.bat"`.
- The new project uses absolute HintPath references to the installed game on this machine.

Verification:

```bat
cd C:\Users\Administrator\source\repos\7d2dmods\mods\LogisticsNetwork
build.bat
```

From WSL/Hermes, prefer the Windows command wrapper:

```bash
cmd.exe /c "cd /d C:\Users\Administrator\source\repos\7d2dmods\mods\LogisticsNetwork && build.bat"
```

Expected: DLL builds or failure is documented in `docs/API_REFERENCE.md` / issue notes.

## Phase 3: Port and generalize conduit blocks

- [x] Copy useful conduit block XML from `mods/AutoForge/Config/blocks.xml` into new mod config.
- [x] Rename internal ids away from `autoForgeConduit`.
- [x] Suggested id: `logisticsConduit`.
- [x] Copy/refactor `ConduitBlock.cs` into new mod source.
- [x] Rename namespace/classes away from AutoForge.
- [x] Preserve vanilla prefab approach initially.
- [x] Add conduit recipe to `Config/recipes.xml`.
- [x] Add localization entries.
- [x] Verify conduit appears/crafts/places in game.
- [ ] Commit conduit milestone.

Verification:

- Game loads without XML errors.
- Conduit item appears with expected name.
- Conduit block can be placed and removed.
- Log file shows no missing class errors.

## Phase 4: Implement network scanner

- [x] Copy/refactor `ConduitNetwork.cs` into `Network/NetworkScanner.cs`.
- [x] Generalize scanner to start from any logistics block, not only AutoForge workstation.
- [x] Represent discovered network as `NetworkGraph`.
- [x] Track conduit positions.
- [ ] Track connector positions.
- [x] Track adjacent storage endpoints.
- [x] Track adjacent workstation endpoints.
- [x] Add max scan depth constant.
- [x] Add loop/visited protection.
- [x] Add debug logging summary: conduits, storage endpoints, workstation endpoints.
- [ ] Add tests if a test harness can be created outside game runtime.
- [x] Otherwise add a deterministic debug command/log path for in-game verification.
- [ ] Commit scanner milestone.

Verification:

- Place small network in game.
- Logs show correct count of conduits and endpoints.
- Remove a conduit and verify network updates.
- Place a loop and verify no infinite scan.

## Phase 5: Connector/importer/exporter/filter block design

- [ ] Confirm operator decision C: UI strategy.
- [ ] Add `logisticsConnector` block XML.
- [ ] Add `logisticsImporter` block XML if using separate role blocks.
- [ ] Add `logisticsExporter` block XML if using separate role blocks.
- [ ] Add `logisticsFilter` item/block XML if using filter modules.
- [ ] Add localization for all blocks/items.
- [ ] Add recipes that fit 7DTD progression.
- [ ] Implement `ConnectorBlock.cs` shell.
- [ ] Implement role detection: connector/importer/exporter/filter.
- [ ] Implement adjacent tile entity discovery.
- [ ] Log adjacent block/entity type for verification.
- [ ] Commit block design milestone.

MVP recommendation:

- Use separate simple blocks for importer/exporter first.
- Add complex configurable filter UI later.

Verification:

- All blocks/items appear in creative menu/crafting if configured.
- Placing a connector beside a chest logs it as storage endpoint.
- Placing a connector beside forge/workbench logs it as workstation endpoint.

## Phase 6: Storage endpoint abstraction

- [ ] Create `NetworkEndpoint` base/interface.
- [ ] Create `StorageEndpoint` wrapper around vanilla loot containers/storage tile entities.
- [ ] Implement read inventory slots.
- [ ] Implement can-insert check.
- [ ] Implement insert stack or partial stack.
- [ ] Implement can-extract check.
- [ ] Implement extract stack or partial stack.
- [ ] Mark tile entity modified after mutations.
- [ ] Add logs for item id/count moved.
- [ ] Add safeguards against null tile entities and unloaded chunks.
- [ ] Commit storage endpoint milestone.

Verification:

- Importer can pull one whitelisted test item from adjacent chest.
- Exporter can push one whitelisted test item into adjacent chest.
- Counts are correct after reload.
- No duplication.
- No deletion.

## Phase 7: Basic item routing

- [ ] Create `ItemRoutingService`.
- [ ] Define route request: item id, count, source endpoint, destination constraints.
- [ ] Implement scan of available sources.
- [ ] Implement scan of valid destinations.
- [ ] Implement priority convention; default higher number wins.
- [ ] Implement overflow fallback.
- [ ] Implement no-route behavior with clear logs.
- [ ] Add simple whitelist/blacklist filter rule object.
- [ ] Support `pull all matching` for importer.
- [ ] Support `keep stock N` for exporter if easy; otherwise defer.
- [ ] Commit routing milestone.

Verification:

- Put item in input chest.
- Network moves it to correct filtered output chest.
- Full preferred chest causes overflow to secondary chest.
- Blacklisted item is not moved.

## Phase 8: Workstation endpoint discovery

- [ ] Confirm operator decision F: storage first vs workstation first.
- [ ] Identify vanilla workstation tile entity classes in game assemblies or runtime logs.
- [ ] Update `docs/API_REFERENCE.md` with verified class names and fields.
- [ ] Implement `WorkstationEndpoint` wrapper.
- [ ] Detect campfire.
- [ ] Detect forge.
- [ ] Detect workbench.
- [ ] Detect cement mixer.
- [ ] Detect chemistry station.
- [ ] Expose station type/crafting area.
- [ ] Expose inventory slots if API allows.
- [ ] Expose output slots if API allows.
- [ ] Expose fuel/input/tool slots if API allows.
- [ ] Commit workstation discovery milestone.

Verification:

- Connector touching each vanilla station logs correct station type.
- No station is mistaken for a normal chest.
- Removing station removes endpoint from network scan.

## Phase 9: Workstation output extraction

This is the safest first workstation automation feature.

- [ ] Implement output-slot detection for one station type first, preferably workbench or campfire.
- [ ] Add filter rule: crafted-products-only.
- [ ] Importer touching station extracts only output items.
- [ ] Do not remove inputs/fuel/tools.
- [ ] Move extracted items into connected storage.
- [ ] Mark station and destination storage modified.
- [ ] Log exact slot/item/count moved.
- [ ] Repeat for forge if slot layout differs.
- [ ] Repeat for workbench/cement mixer/chemistry station.
- [ ] Commit output extraction milestone.

Verification:

- Manually queue/craft item in vanilla station.
- Let output finish.
- Network pulls output into storage.
- Inputs/fuel/tools remain untouched.
- No duplication/deletion across save/reload.

## Phase 10: Workstation ingredient feeding

- [ ] Confirm operator decision B: feed-only vs direct recipe start.
- [ ] For one station type, identify input slots.
- [ ] Given a configured recipe/pattern, find required ingredients.
- [ ] Check network storage for ingredients before moving anything.
- [ ] Move only required ingredients into station.
- [ ] Avoid overfilling input slots.
- [ ] Avoid moving ingredients if station queue cannot use them.
- [ ] Add missing-ingredient logs.
- [ ] Repeat for other station types after one works safely.
- [ ] Commit ingredient feeding milestone.

Verification:

- Storage contains exact ingredients.
- Station receives only required ingredients.
- Missing ingredient prevents partial destructive moves unless explicitly allowed.
- Output extraction still works.

## Phase 11: Recipe/pattern selection MVP

- [ ] Decide MVP pattern storage approach.
- [ ] Option 1: JSON config file patterns.
- [ ] Option 2: connector block role hardcoded to one recipe for testing.
- [ ] Option 3: filter item encodes target output.
- [ ] Option 4: custom UI.
- [ ] Implement minimal `RecipePattern` class.
- [ ] Load pattern from data or config.
- [ ] Match pattern to vanilla recipe from `db/datasets/7dtd-vanilla/derived/recipes.json` during dev tooling, or duplicated runtime-safe config if needed.
- [ ] Validate ingredients and craft area.
- [ ] Log pattern loaded.
- [ ] Attach pattern to connector/workstation endpoint.
- [ ] Commit pattern milestone.

Recommended MVP:

- Use JSON/config or a simple debug mapping first.
- Do not block on custom UI.

Verification:

- Pattern for one workbench recipe loads.
- Network can report missing/available ingredients.
- Pattern can be changed without recompiling if using config.

## Phase 12: Direct crafting queue automation

Only start after API verification.

- [ ] Verify how vanilla workstation queues recipes in C#.
- [ ] Update `docs/API_REFERENCE.md` with exact classes/fields/methods.
- [ ] Implement check for station idle/queue room.
- [ ] Implement start recipe or enqueue recipe call.
- [ ] Ensure server/local authority is correct.
- [ ] Handle failure gracefully.
- [ ] Avoid starting infinite crafts without stock/target limits.
- [ ] Commit queue automation milestone.

Verification:

- Network starts one recipe from configured pattern.
- Craft consumes correct ingredients.
- Craft appears in station queue if UI visible.
- Completed output is extracted.
- No infinite runaway queue unless configured.

## Phase 13: Keep-stock jobs

- [ ] Implement job target: keep item id at count N in network storage.
- [ ] Count existing item across connected storage.
- [ ] If count below target, request craft job.
- [ ] If ingredients missing, report missing list.
- [ ] Prevent duplicate jobs for same target.
- [ ] Add per-network job state.
- [ ] Add cooldown/tick throttling.
- [ ] Commit keep-stock milestone.

Verification:

- Set keep 100 forged iron.
- If storage has 90 and ingredients exist, craft 10 or nearest batch amount.
- If storage has 100+, do nothing.
- Restart/reload does not duplicate jobs unexpectedly.

## Phase 14: UI and configuration upgrade path

Only after core movement is stable.

- [ ] Confirm operator decision C again before UI work.
- [ ] Investigate XUi patterns from vanilla and AutoForge placeholder.
- [ ] Add connector UI with role, filters, priority, target item/count.
- [ ] Add storage endpoint UI if needed.
- [ ] Add pattern selection UI if feasible.
- [ ] Add missing ingredient display.
- [ ] Add network status display.
- [ ] Commit UI milestone.

Verification:

- UI opens reliably.
- Config persists across save/reload.
- Config sync assumptions are documented.
- Misclicks do not destroy items/config.

## Phase 15: Balance and recipes

- [ ] Decide progression tier for conduits.
- [ ] Decide progression tier for importer/exporter.
- [ ] Decide whether advanced crafting automation requires schematics/perks.
- [ ] Add recipes to mod XML.
- [ ] Add localization descriptions explaining behavior.
- [ ] Test early-game affordability.
- [ ] Test late-game scaling.
- [ ] Commit balance milestone.

Possible initial recipe direction:

- Conduit: forged iron + scrap polymer, outputs several blocks.
- Connector: forged iron + mechanical parts + electric parts.
- Importer/exporter: connector + more electric/mechanical parts.
- Filter module: paper/plastic/electrical part or data-themed component.

## Phase 16: Multiplayer hardening

Do not claim multiplayer support until this is complete.

- [ ] Confirm operator decision E.
- [ ] Verify all item movement occurs server-side.
- [ ] Verify tile entity modifications replicate to clients.
- [ ] Add NetPackage or equivalent for connector config if needed.
- [ ] Test on local dedicated server.
- [ ] Test two clients interacting with same network.
- [ ] Test chunk unload/reload.
- [ ] Test land claim/protection interactions.
- [ ] Commit multiplayer milestone.

Verification:

- No client/server desync.
- No item duplication under simultaneous access.
- No item loss under disconnect/reconnect.

## Phase 17: Performance hardening

- [ ] Avoid full network scans every tick for every block if possible.
- [ ] Cache network graph with invalidation on block place/remove.
- [ ] Throttle item movement per network.
- [ ] Add max operations per tick.
- [ ] Add debug performance logs behind a flag.
- [ ] Test large conduit network.
- [ ] Commit performance milestone.

Verification:

- Large network does not noticeably stutter.
- Logs show bounded tick time.
- Chunk unload/reload does not leak stale endpoints.

## Phase 18: Docs and agent handoff completeness

- [ ] Update `docs/API_REFERENCE.md` with every verified API fact.
- [ ] Update `docs/DATA_LOOKUP_GUIDE.md` with every dataset expansion.
- [ ] Update `docs/LESSONS_LEARNED.md` after major debugging sessions.
- [ ] Update `docs/REPO_MAP.md` when new directories/files are added.
- [ ] Add build/run/test instructions to new mod README.
- [ ] Add troubleshooting section for XML load errors.
- [ ] Add troubleshooting section for missing DLL/reference errors.
- [ ] Add troubleshooting section for item duplication/loss risk.
- [ ] Commit docs milestone.

## Phase 19: Final acceptance criteria

The redesign is “implemented” when:

- [ ] The custom AutoForge workstation is no longer required for automation gameplay.
- [ ] Conduits connect multiple vanilla workstations and storage containers.
- [ ] At least one storage sorting route works reliably.
- [ ] At least one vanilla workstation can have outputs extracted automatically.
- [ ] At least one vanilla workstation can receive required ingredients automatically.
- [ ] At least one recipe/pattern can drive workstation automation or feed behavior.
- [ ] No known item duplication/loss in tested flows.
- [ ] All implementation docs are current enough for a fresh coding model to continue.
- [ ] The vanilla data lookup has enough recipes/items/workstations for tested features.
- [ ] Build instructions work on the operator's Windows setup.

## Commit strategy

Use small commits:

```bash
git add <specific files>
git commit -m "docs: ..."
git commit -m "feat: add logistics conduit block"
git commit -m "feat: add network scanner"
git commit -m "feat: add storage endpoint routing"
git commit -m "feat: add workstation output extraction"
```

Push with:

```bash
HOME=/home/dministrator git -C /mnt/c/Users/Administrator/source/repos/7d2dmods push
```
