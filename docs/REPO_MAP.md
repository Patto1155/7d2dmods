# Repository Map

## Current content

### `mods/AutoForge/`
The preserved source tree for the current mod.

Important files:
- `README.md` — original mod handover / usage notes
- `ModInfo.xml` — mod metadata loaded by the game
- `build.bat` — local build helper
- `Source/AutoForge.csproj` — .NET 4.8 project file
- `Source/AutoForgeMod.cs` — entry point
- `Source/AutoForgeRegistry.cs` — registry for placed forges
- `Source/AutoForgeTick.cs` — tick/update logic
- `Source/Blocks/AutoForgeBlock.cs` — workstation block behavior
- `Source/Blocks/ConduitBlock.cs` — conduit block behavior
- `Source/Network/ConduitNetwork.cs` — connected-network search
- `Source/Patches/HarmonyPatches.cs` — patch placeholder
- `Config/blocks.xml` — block definitions
- `Config/recipes.xml` — crafting recipes
- `Config/localization.txt` — names and descriptions
- `Config/XUi/windows.xml` — UI hooks / notes

### `docs/`
Agent-facing documentation and future design notes.

## What was intentionally not copied

- `AutoForge.dll`
- `AutoForge.pdb`
- `Source/obj/`
- scratch scripts like `inspect2.py`, `inspect3.py`, `inspect_sig.py`

## Suggested next steps

1. Keep code changes inside `mods/AutoForge/`.
2. Record redesign ideas in `docs/LESSONS_LEARNED.md`.
3. If a new mod is added later, create a sibling folder under `mods/` and add a short doc entry here.

