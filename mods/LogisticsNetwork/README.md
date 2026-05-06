# Wasteland Logistics

Passive logistics-network milestone mod for 7 Days to Die.

Internal assembly name: `LogisticsNetwork`
Player-facing name: `Wasteland Logistics`

This is a clean sibling of the legacy AutoForge prototype. It currently provides:

- mod metadata
- Harmony entry point
- passive 2-second network scan tick
- vanilla-prefab logistics conduit block
- logistics connector block (registry + scan; no routing UI or item IO yet)
- reusable block registry, bootstrap retry with throttling, registry pruning against world blocks
- BFS network scanner with depth-limit truncation reporting
- minimal logging helper
- config scaffolds for future routing, recipes, localization, and UI hooks

It does not yet move items, automate workstations, or replace vanilla workstation UIs. Multiplayer behavior is not verified.

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

- This skeleton intentionally avoids touching `mods/AutoForge/` gameplay code.
- Harmony and 7DTD managed references are copied from the verified AutoForge project pattern.
- Future gameplay work should stay passive until the logistics/network design is verified.
