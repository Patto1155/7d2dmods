# Wasteland Logistics

Passive skeleton mod for 7 Days to Die.

Internal assembly name: `LogisticsNetwork`
Player-facing name: `Wasteland Logistics`

This is a clean sibling of the legacy AutoForge prototype. It currently provides:

- mod metadata
- Harmony entry point
- placeholder patch file
- minimal logging helper
- empty config scaffolds for future blocks, recipes, localization, and UI hooks

It does not yet move items, automate workstations, or replace vanilla workstation UIs.

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
