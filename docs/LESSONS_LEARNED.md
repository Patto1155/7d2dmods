# Lessons Learned

This file captures what the first AutoForge implementation taught us so future agents do not repeat the same mistakes.

## Good things to preserve

- Vanilla prefabs kept the asset burden low.
- A small source tree made the first pass understandable.
- Harmony entry point plus a throttled tick loop is a reasonable starting pattern.
- Static registries and BFS network scans are useful for a first prototype.
- Conduits are a good 3D/block-network concept for 7DTD.
- The prototype proved that a mod can define new placeable blocks/items with XML plus C# classes.

## Pain points from the first prototype

- The custom `Auto Forge` workstation was too narrow and should not be the final design center.
- Recipe processing was stubbed because workstation crafting API field names were not verified.
- Forge-specific naming made the design feel smaller than the intended logistics system.
- Multiplayer sync was not implemented.
- No custom UI meant recipe selection/configuration remained unsolved.
- Workstation input/output/fuel/tool slot handling is risky without exact API verification.
- Broad filesystem/wiki searches waste time; local XML and in-repo datasets are better.

## Design lessons

- Keep vanilla workstations. Do not force players to replace them with custom versions.
- Put automation behavior in conduits/connectors/importers/exporters instead of inside a fake station.
- Implement output extraction before direct recipe queue automation; it is safer.
- Implement storage sorting before complex autocrafting if the operator is okay with that order.
- Treat Refined Storage as inspiration, not a direct clone.
- Avoid Factorio-style belts/inserters unless the operator explicitly pivots.
- Use simple block roles before building a custom UI.
- Add custom UI only after stable item movement and API verification.

## Documentation lessons

- Future agents need a direct read order.
- Future agents need exact paths to vanilla XML and derived datasets.
- API facts must be recorded as soon as they are verified.
- Operator decisions should be visible in a checklist instead of hidden in chat history.
- The repo should become the shared memory for coding models.

## Data lessons

- Use vanilla game XML as the source of truth.
- Fandom/wiki pages can be blocked or stale.
- Store normalized internal ids, not only display names.
- Preserve provenance for every derived dataset.
- Add repeatable extraction scripts instead of manual one-off data dumps.

## Implementation caution list

- Never move items without verifying exact source/destination slot semantics.
- Never claim multiplayer support without dedicated server/client testing.
- Never leave logs as the only proof of no duplication/loss; verify actual inventory counts.
- Never assume all workstations use identical slot layouts.
- Never let a keep-stock job enqueue infinite crafts without limits.
- Never scan huge networks every tick without throttling/caching.

## Future redesign notes

The target mod should become a general logistics layer:

- conduits connect networks
- connectors expose adjacent vanilla blocks as endpoints
- importers pull from endpoints into the network
- exporters push from the network into endpoints
- filters constrain what moves
- patterns/recipes define craft requests
- storage routing handles sorting and overflow
- workstation automation feeds ingredients and extracts outputs

See:

- `docs/REDESIGN_SPEC.md`
- `docs/IMPLEMENTATION_CHECKLIST.md`
- `docs/API_REFERENCE.md`
- `docs/DATA_LOOKUP_GUIDE.md`
