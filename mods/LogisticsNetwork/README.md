# Wasteland Logistics

Passive logistics-network milestone mod for 7 Days to Die.

Internal assembly name: `LogisticsNetwork`
Player-facing name: `Wasteland Logistics`

This is the active replacement for the old AutoForge prototype. It uses vanilla workstations plus logistics blocks instead of adding a custom Auto Forge workstation. It currently provides:

- mod metadata
- Harmony entry point
- passive 2-second network scan tick
- vanilla-prefab logistics conduit block
- logistics connector block (registry + scan; no routing UI or item IO yet)
- reusable block registry, bootstrap retry with throttling, registry pruning against world blocks
- BFS network scanner with depth-limit truncation reporting
- passive `StorageEndpoint` / `NetworkEndpoint` snapshots for scanned storage positions (metadata + slot count log lines; no item movement)
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

- The old AutoForge prototype is retired; do not install it alongside this mod unless intentionally testing legacy behavior.
- Harmony and 7DTD managed references follow the verified prototype project pattern.
- Future gameplay work should stay passive until the logistics/network design is verified.
