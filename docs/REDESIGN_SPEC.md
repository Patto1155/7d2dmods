# 7d2dmods Logistics Network Redesign Spec

> For coding agents: this is the target architecture. Use `docs/IMPLEMENTATION_CHECKLIST.md` for implementation order.

## Goal

Transform the original AutoForge prototype into a general 7 Days to Die logistics network that connects vanilla workstations and storage through conduit blocks.

The player should be able to place conduits and small connector blocks, link vanilla workstations/storage, choose what should be crafted or routed, and let the network move ingredients and outputs automatically.

## Non-goals for the first major implementation

Do not start by building:

- A full custom storage-terminal UI.
- Belts, inserters, item entities travelling in the world, or Factorio-style logistics.
- Multiplayer sync as the first milestone unless the operator explicitly chooses that.
- A total rewrite of vanilla workstation UI.
- Custom meshes/textures as a blocker.

## Core fantasy

The player builds a base with normal 7DTD stations:

- campfire
- forge
- workbench
- cement mixer
- chemistry station
- storage crates

Then the player places conduits between them. The network can:

- pull matching ingredients from storage
- feed those ingredients into vanilla stations
- pull crafted outputs out of stations
- route outputs into storage
- optionally keep stock levels, e.g. keep 500 forged iron or 100 concrete mix available

## Existing prototype to preserve as reference

The original AutoForge mod contains useful ideas:

- Harmony entry point.
- Game update tick running every 2 seconds.
- Static registry of placed blocks.
- Conduit block definition.
- BFS flood-fill network scan.
- Vanilla prefab usage to avoid asset work.

But it also contains concepts to replace:

- `autoForgeWorkstation` should become legacy only.
- Forge-specific naming and class names should be refactored.
- Recipe processing was stubbed because the workstation API was not verified.
- Single-purpose workstation scanning should become endpoint scanning.

## New conceptual model

### Network

A network is a connected set of logistics blocks discovered by BFS/flood-fill from conduits/connectors.

A network has:

- conduit blocks
- storage endpoints
- workstation endpoints
- importer/exporter/filter rules
- optional future controller or power requirement

### Conduit

Conduits are passive connection blocks.

Responsibilities:

- connect to adjacent conduits/connectors
- allow network discovery
- expose minimal player interaction, if any
- keep block model simple and vanilla-prefab based initially

Conduits should not contain complex recipe logic.

### Connector

A connector is a small block/module touching exactly one external block when possible.

Responsibilities:

- detect adjacent vanilla containers/workstations
- register that external block as a network endpoint
- provide a place for configuration without replacing vanilla station UI

Possible names:

- Network Connector ********LET'S GO WITH THIS - patrick
- Workstation Connector
- Storage Connector
- Logistics Port

### Importer

An importer pulls items from an adjacent endpoint into the network.

Examples:

- pull completed outputs from a forge
- pull loot/crate contents into central storage
- pull products from a chemistry station

Behavior:

- respects whitelist/blacklist filters
- never pulls fuel/tools/input-only slots unless configured
- prefers completed-output slots for workstations
- marks changed tile entities as modified for save/sync

### Exporter

An exporter pushes items from the network into an adjacent endpoint.

Examples:

- feed ingredients into a workbench
- keep a forge stocked with clay/iron/wood if that model is supported
- keep a chest stocked with medical supplies

Behavior:

- respects whitelist/blacklist filters
- respects max stock thresholds
- avoids filling output slots or inappropriate slots
- does not blindly dump unrelated items into workstations

### Filter

A filter is a rule source that constrains item movement.

Possible implementations, from simplest to most advanced:

1. XML-defined filter blocks with hardcoded roles.
2. Player config via held item + interact action.
3. A simple item installed in connector storage slots.
4. Custom UI panel listing whitelist/blacklist entries.
5. Full pattern grid / recipe terminal.

Recommended MVP: use simple connector behavior and minimal filter configuration first. Avoid custom UI until API is verified.

### Workstation endpoint

A workstation endpoint is a vanilla workstation detected through an adjacent connector/conduit.

The mod should try to operate on vanilla stations rather than introducing replacement station blocks.

Responsibilities:

- identify station type
- inspect active/queued recipe where possible
- determine required ingredients
- insert ingredients if missing
- extract completed outputs
- avoid corrupting station state

### Recipe pattern

A recipe pattern is a stored instruction saying:

- output item id
- workstation/craft area required
- ingredient item ids and counts
- batch count or keep-stock target

Patterns should eventually come from vanilla `recipes.xml` and localization data.

MVP can start with a small hardcoded or JSON-backed pattern set while the API is verified.

## In-game UX proposal

### MVP UX: connector-driven automation

1. Player places a vanilla workstation.
2. Player places a conduit/connector touching the station.
3. Player places storage crates connected by conduits.
4. Player configures one connector as input/exporter-to-workstation.
5. Player configures another connector or same connector as output/importer-from-workstation.
6. Player selects a desired recipe/pattern through the connector, not by replacing the vanilla UI.
7. Network pulls ingredients from storage, inserts them into the station, and extracts finished products.

This keeps the vanilla workstation UI intact and reduces risk.

### Later UX: workstation network tab

If UI patching proves stable, add a small network panel to the vanilla workstation window:

- selected network recipe
- missing ingredients
- auto-craft toggle
- output export toggle
- target stock amount
- connected storage count

This is nicer, but should not block MVP.

### Filter item idea

A filter item/module can be installed into a connector or used on a connector.

Modes:

- Pull crafted products only.
- Pull all except fuel/tools/inputs.
- Whitelist item ids.
- Blacklist item ids.
- Keep N in destination.
- Round-robin or priority routing.

For workstations, the most important first filter is:

- extract only completed outputs, leave all inputs/fuel/tools alone.

## Recipe automation behavior

Recommended first behavior:

1. Network sees a configured pattern for a station.
2. Network checks if station is idle or has room in queue.
3. Network checks connected storage for required ingredients.
4. Network reserves the ingredients in memory for this tick/job.
5. Network inserts only the ingredients required for the next craft/batch.
6. Network starts or allows the station recipe if API supports it.
7. Network watches output slots.
8. Network extracts completed outputs through importer behavior.

If the API does not allow starting the recipe directly:

- fallback to ingredient feed + output extraction
- document the limitation
- let the player manually queue the recipe while the network supplies inputs and extracts outputs

## Storage routing behavior

Storage endpoints should have routing rules:

- default storage: accepts anything
- filtered storage: accepts whitelist only
- priority: lower number or higher number wins, but document chosen convention
- overflow: if preferred destination full, route to next valid storage

Recommended convention:

- Higher priority number wins.
- `priority: 100` receives before `priority: 10`.
- unconfigured storage defaults to priority `0`.

## Power/progression options

Do not add power requirement in MVP unless the operator chooses it.

Possible progression gates:

- conduits crafted at workbench
- connector crafted at workbench
- importer/exporter crafted with forged iron, mechanical parts, electric parts
- advanced recipe/pattern blocks require robotics/electric progression later

## Multiplayer/server sync

Initial implementation can be single-player/local-first.

Before claiming multiplayer support, verify:

- server-authoritative item movement
- tile entity modification/sync calls
- client UI config synchronization
- NetPackage or equivalent state replication
- permissions/land-claim interactions

## Naming direction

Possible mod names:

- 7DTD Logistics Network
- Wasteland Logistics
- Survivor Logistics Network
- Conduit Logistics
- Wasteland Automation

Internal code should avoid `AutoForge` for new architecture.

Suggested active development path:

- `mods/LogisticsNetwork/`

The legacy prototype remains:

- `mods/AutoForge/`

## Recommended code architecture

Suggested source layout for the new mod:

```text
mods/LogisticsNetwork/
  ModInfo.xml
  build.bat
  README.md
  Config/
    blocks.xml
    recipes.xml
    localization.txt
    XUi/
  Source/
    LogisticsNetwork.csproj
    LogisticsNetworkMod.cs
    Blocks/
      ConduitBlock.cs
      ConnectorBlock.cs
      ImporterBlock.cs
      ExporterBlock.cs
      FilterBlock.cs
    Network/
      NetworkScanner.cs
      NetworkGraph.cs
      NetworkEndpoint.cs
      StorageEndpoint.cs
      WorkstationEndpoint.cs
      ItemRoutingService.cs
      CraftingJobService.cs
    Data/
      RecipePattern.cs
      VanillaDataLookup.cs
      FilterRule.cs
      RoutingRule.cs
    Patches/
      HarmonyPatches.cs
      WorkstationPatches.cs
    Util/
      Log.cs
      InventoryUtil.cs
      TileEntityUtil.cs
```

## Agent implementation rule

Every time a future agent learns a stable 7DTD API fact, it should update:

- `docs/API_REFERENCE.md`

Every time it expands vanilla game data, it should update:

- `db/datasets/7dtd-vanilla/`
- `docs/DATA_LOOKUP_GUIDE.md` if query workflow changes

Every time it completes a milestone, it should update:

- `docs/IMPLEMENTATION_CHECKLIST.md`
