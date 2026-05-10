# Wasteland Logistics

Passive logistics-network milestone mod for 7 Days to Die.

Internal assembly name: `LogisticsNetwork`
Player-facing name: `Wasteland Logistics`

This is the active replacement for the old AutoForge prototype. It uses vanilla workstations plus logistics blocks instead of adding a custom Auto Forge workstation. It currently provides:

- mod metadata
- Harmony entry point
- passive 2-second network scan tick
- vanilla-prefab logistics conduit block
- logistics connector, importer, exporter, and filter blocks (registry + scan; passive routing logs + optional experimental storage transfer + optional workstation output extraction)
- passive storage endpoint diagnostics (`slotsUsed`, `canInsert`, `canExtract`) for routing readiness logs
- passive routing intent diagnostics (`ItemRoutingService`) that classify connector/importer/exporter/filter nodes (live moves are opt-in; see below)
- passive route pairing logs (`routePlan src->dst`) to verify deterministic source/destination matching before live transfer
- default passive route priorities (higher wins) applied before route pairing to mirror planned routing behavior
- passive overflow diagnostics (`overflowSources` / `overflowDestinations`) and filter-mode markers in route plan logs (`filterMode` matches `LogisticsNetworkFeatures.ItemTransferFilterMode` / ids — same semantics as live transfer when using the global dev filter)
- passive routing options are now surfaced in logs (`pullAllMatching`, `keepStockTarget`) for importer/exporter behavior planning
- reusable block registry, bootstrap retry with throttling, registry pruning against world blocks
- BFS network scanner with depth-limit truncation reporting
- minimal logging helper
- config scaffolds for future routing, recipes, localization, and UI hooks

**Inventory icons:** conduit/connector/importer/exporter/filter use vanilla icon references via `Config/items.xml` and block `CustomIcon` properties. Custom art is still optional future polish.

**Experimental item transfer (opt-in):** set `LogisticsNetworkFeatures.EnableLiveStorageTransfer = true` in `Source/Network/LogisticsNetworkFeatures.cs` and rebuild. Optional global filter: set `ItemTransferFilterMode` to `Whitelist` or `Blacklist` and populate `ItemTransferFilterIds` with internal item names (`ItemClass.Name`, e.g. `resourceWood`). The mover scans the source chest for the **first slot** whose item passes the rule (pull-all-matching style when filtering). Destination placement order: **empty slot** → **TryStackItem** (merge; uses return tuple; failed placements log counts + tuple flags) → **AddItem**. Per-block `logisticsFilter` UI is not wired yet — filter lists are code-only. Keep-stock is still skipped when `keepStockTarget > 0`. Mutation is skipped on `World.IsRemote()` when `RespectWorldIsRemote` is true. **Dedicated multiplayer is not verified.** Transfer logs include `placement=empty|stack|additem`. Failed transfers log `skip:*` with detail (e.g. `snapshot_failed side=source`, `dest_placement_failed … additem_false`).

**Experimental workstation output extraction (opt-in):** set `LogisticsNetworkFeatures.EnableLiveWorkstationOutputExtraction = true` and rebuild. With an **importer** adjacent to a vanilla workstation (workbench / campfire / cement mixer / chemistry station) and an **exporter** adjacent to a destination chest, the network moves at most one unit per tick from `TileEntityWorkstation.Output` into the chest. Inputs, fuel, tools, and the recipe queue are never read or written. The same global `ItemTransferFilterMode` / `ItemTransferFilterIds` rule applies. Extraction pauses with `skip:workstation_user_accessing` while a player has the station UI open. `TileEntityForge` is intentionally rejected (`skip:source_not_workstation type=TileEntityForge`) until its single-stack output reservoir is handled in a separate slice. Successful extracts log `workstation outputExtract OK graph=… stationType=… item=… outSlot=… countBefore=… countAfter=…`.

Crafting-queue automation and custom workstation UIs are not implemented. Multiplayer behavior beyond the remote-world gate is not verified.

## Install Location

```
7 Days To Die\Mods\LogisticsNetwork\
```

## Build

From the mod root:

```
build.bat
```

Or directly:

```
dotnet build Source\LogisticsNetwork.csproj -c Release
```

The DLL is emitted to `Mods\LogisticsNetwork\LogisticsNetwork.dll`.

## Notes

- The old AutoForge prototype is retired; do not install it alongside this mod unless intentionally testing legacy behavior.
- Harmony and 7DTD managed references follow the verified prototype project pattern.
- Treat `EnableLiveStorageTransfer` as experimental; validate saves and multiplayer yourself before relying on it.

## Troubleshooting

- **Game fails to load / XML errors:** check `output_log.txt` for block/item typos; verify `Config/blocks.xml` class names match types in `LogisticsNetwork.dll` and that the DLL is beside `ModInfo.xml`.
- **Build fails / missing references:** open `Source/LogisticsNetwork.csproj` and point `HintPath` entries at your install (`7DaysToDie_Data/Managed`, `Mods/0_TFP_Harmony/0Harmony.dll`). Build from the mod folder: `dotnet build Source\LogisticsNetwork.csproj -c Release`.
- **No transfer when enabled:** live moves require an **importer** adjacent to the source chest and an **exporter** adjacent to the destination chest on the same passive route pair; plain connectors alone do not move items. Confirm `EnableLiveStorageTransfer` is true in the built DLL, chunks loaded, and not blocked by `RespectWorldIsRemote` on a multiplayer client.
- **Possible dup/loss:** experimental path mutates loot containers; test in SP before relying on it; treat blacklist/whitelist mistakes as high risk until you have a reproducible safe workflow.
