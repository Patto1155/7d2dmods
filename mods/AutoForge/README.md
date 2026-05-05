# AutoForge

Automated crafting workstation mod for 7 Days to Die 1.x.

## Install Location

```
7 Days To Die\Mods\AutoForge\
```

## Build

Requires .NET SDK 4.8 and the game installed at the default Steam path.

```
cd Source
dotnet build AutoForge.csproj -c Release
```

Or from the mod root:

```
build.bat
```

The DLL lands at `Mods\AutoForge\AutoForge.dll`.

## Testing In-Game

1. Start a new creative game or load a save.
2. Open the creative menu (U) and search "AutoForge" — you'll find the workstation and conduit blocks.
3. Place the workstation, run conduit blocks to a storage crate.
4. Open the workstation and queue any workbench recipe that you have ingredients for in the connected crates.
5. Every 2 seconds the mod ticks: it will pull ingredients from crates, complete the craft, and push output back to the first crate with space.

## Known Phase 1 Limitations

- Single-player only. Multiplayer clients will not sync (NetPackages not implemented yet).
- No custom UI. Uses the standard workbench window — the auto-pull happens silently in the background.
- No power requirement. The workstation runs indefinitely without electricity.
- Conduit model falls back to a simple shape if the pipe prefab path does not resolve in your install.
