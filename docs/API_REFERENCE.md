# 7 Days to Die Modding API Reference Notes

This file records verified or suspected 7DTD C#/XML API facts for the logistics network mod.

Important: future agents must update this file whenever they verify an API field, method, class, or limitation. The original AutoForge implementation had recipe processing stubbed because workstation crafting API field names were not verified. Do not repeat that mistake.

## Confidence levels

Use these labels:

- `Verified`: confirmed by source code, build, reflection, or in-game test.
- `Prototype`: used in AutoForge prototype but not fully validated.
- `Suspected`: plausible but must be verified before relying on it.
- `Unknown`: known gap.

## Prototype facts from AutoForge

### Harmony entry point

Status: Prototype

The original mod used a C# entry point that registers Harmony patches and hooks game update logic.

Reference files:

- `mods/AutoForge/Source/AutoForgeMod.cs`
- `mods/AutoForge/Source/Patches/HarmonyPatches.cs`

Future action:

- Verify the exact current 7DTD V2.6 mod entry point pattern.
- Copy only what still builds.

### Tick loop

Status: Prototype

The original mod ran `AutoForgeTick.RunAll()` every 2 seconds via game update.

Reference:

- `mods/AutoForge/Source/AutoForgeTick.cs`

Future action:

- Rename/generalize to a network tick manager.
- Throttle operations and avoid expensive full scans every tick.

### Placed block registry

Status: Prototype

The original mod used a static `HashSet<Vector3i>` to track placed Auto Forge positions.

Reference:

- `mods/AutoForge/Source/AutoForgeRegistry.cs`
- `mods/AutoForge/Source/Blocks/AutoForgeBlock.cs`

Future action:

- Replace with registry of logistics blocks/connectors/networks.
- Consider invalidation on block place/remove.

### Conduit BFS scan

Status: Prototype/useful

The original mod used BFS/flood-fill with a max step limit to scan connected conduit blocks and locate connected loot containers.

Reference:

- `mods/AutoForge/Source/Network/ConduitNetwork.cs`

Future action:

- Generalize to `NetworkScanner` that finds conduits, connectors, storage endpoints, and workstation endpoints.

## Build/reference notes

Status: Prototype/Verified from `mods/AutoForge/Source/AutoForge.csproj`

The original project targeted `.NET Framework 4.8` via `net48` and referenced these assemblies:

- `Assembly-CSharp.dll` from `7DaysToDie_Data/Managed/`
- `Assembly-CSharp-firstpass.dll` from `7DaysToDie_Data/Managed/`
- `UnityEngine.dll` from `7DaysToDie_Data/Managed/`
- `UnityEngine.CoreModule.dll` from `7DaysToDie_Data/Managed/`
- `0Harmony.dll` from `Mods/0_TFP_Harmony/`
- `LogLibrary.dll` from `7DaysToDie_Data/Managed/`
- NuGet package: `Microsoft.NETFramework.ReferenceAssemblies` version `1.0.3`

Reference file:

- `mods/AutoForge/Source/AutoForge.csproj`

When creating `mods/LogisticsNetwork/Source/LogisticsNetwork.csproj`, start from these references and update only assembly/mod names and relative paths.

For WSL agents, run Windows build scripts through `cmd.exe`, for example:

```bash
cmd.exe /c "cd /d C:\Users\Administrator\source\repos\7d2dmods\mods\LogisticsNetwork && build.bat"
```

A future `build.sh` wrapper is acceptable, but do not require it for MVP if `build.bat` already works.

## XML/mod config notes

### Block definitions

Status: Prototype

Reference:

- `mods/AutoForge/Config/blocks.xml`

Known prototype ids:

- `autoForgeWorkstation` — legacy, should not be central going forward.
- `autoForgeConduit` — useful starting point for new conduit.

Future ids should avoid AutoForge naming, e.g.:

- `logisticsConduit`
- `logisticsConnector`
- `logisticsImporter`
- `logisticsExporter`
- `logisticsFilterModule`

### Recipes

Status: Prototype

Reference:

- `mods/AutoForge/Config/recipes.xml`

Use vanilla item ids from the dataset wherever possible.

### Localization

Status: Prototype

Reference:

- `mods/AutoForge/Config/localization.txt`

Keep player-facing names clear. Include descriptions explaining block behavior.

## LogisticsNetwork: passive scanner bootstrap

Status: Suspected (depends on reflection matching current `World` / `TileEntity` shapes)

When the conduit/connector registry has no entries, `NetworkScanner` can attempt a **throttled** reflection pass to find parameterless `World.GetTileEntities()` and enumerate its results. It then looks for `logisticsConduit` / `logisticsConnector` blocks in tiles adjacent to discovered tile-entity positions and registers them.

**Unverified assumptions** (must be re-checked when changing game versions):

- `World` exposes a parameterless instance method named `GetTileEntities` that returns an enumerable.
- Enumerated entries are `TileEntity` values or objects with a `Value` property assignable to `TileEntity`.
- A usable block position is available on tile entities via a `Vector3i` property/field named one of: `BlockPosition`, `blockPos`, `blockPosition`, `Position`, `pos`.

**Behavioral notes:**

- If this reflection path fails, bootstrap yields no seeds; the mod retries on a short timer and backs off to 30s after several empty attempts to avoid spamming reflection while no logistics blocks exist.
- `NetworkRegistry` prunes saved positions when the world block at that cell is no longer a `LogisticsConduitBlock` / `LogisticsConnectorBlock` instance (guards against stale registry entries).
- BFS uses a max depth; when the frontier hits the cap, `NetworkGraph.TruncatedByDepthLimit` is set and logs include `truncatedDepth=Y`.

## Vanilla data source paths

Status: Verified path availability from local machine

Use game XML as source of truth:

- `/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/blocks.xml`
- `/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/items.xml`
- `/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/recipes.xml`
- `/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/Localization.txt`

Do not rely on Fandom as primary source.

## Classes and concepts to verify

### BlockWorkstation

Status: Prototype/Suspected

Original `AutoForgeBlock` extended `BlockWorkstation`.

Questions to verify:

- Is subclassing needed for new design? Prefer avoiding replacement vanilla workstation blocks.
- Can adjacent connectors identify vanilla workstation blocks without subclassing them?
- What tile entity class backs each vanilla workstation?

### TileEntityLootContainer

Status: Prototype

Original network scan returned connected `TileEntityLootContainer` storage crates.

Questions to verify:

- Which vanilla storage containers use this class?
- How to safely read item slots?
- How to safely insert/extract item stacks?
- Which method marks the tile entity dirty/modified?
- What sync call is needed in multiplayer?

### TileEntityWorkstation

Status: Suspected/Unknown

Likely relevant for forge/workbench/campfire/etc.

Questions to verify:

- Exact class name(s) in 7DTD V2.6.
- How each station stores input slots.
- How each station stores output slots.
- How fuel/tool slots are represented.
- How crafting queue is represented.
- How to start/enqueue a recipe programmatically.
- How to detect completed output.
- How to avoid touching fuel/tools/inputs during output extraction.

### ItemStack / ItemValue / inventory classes

Status: Suspected/Unknown

Questions to verify:

- Exact constructors/factory methods for item stacks.
- How item ids map to `ItemValue`.
- How stack counts are stored.
- How max stack sizes are checked.
- How partial stack insertion works.
- How to clone/copy stacks safely.

### SetModified / sync

Status: Suspected/Unknown

Questions to verify:

- Exact method to call after mutating a tile entity.
- Whether local single-player and server use same path.
- Whether clients require NetPackage updates for connector config.

## Runtime verification checklist for API facts

When verifying any API fact:

- [ ] Record exact game version.
- [ ] Record exact class/method/field name.
- [ ] Record how it was verified: build, reflection, in-game log, or source reference.
- [ ] Record file where the fact is used.
- [ ] Add a small code comment near risky API usage.
- [ ] Add an entry here with confidence `Verified`.

## Reflection/scouting strategy

If source field names are unclear:

1. Use assembly inspection/reflection if possible.
2. Write a small temporary inspector script only outside committed source unless it is broadly useful.
3. Log runtime type names from adjacent tile entities when connectors are placed.
4. Update this doc with verified names.
5. Delete scratch scripts or add them under `tools/` if reusable.

## Known risk areas

- Item duplication/loss from incorrect partial stack handling.
- Accidentally pulling fuel/tools/input ingredients as if they are outputs.
- Workstation queue API differences between station types.
- Client/server desync in multiplayer.
- Chunk unload/reload invalidating cached endpoints.
- XML class name mismatch causing game load failure.
