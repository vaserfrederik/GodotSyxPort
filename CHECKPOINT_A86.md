# Checkpoint A86 — source-faithful terrain masks and room placer behaviour

This checkpoint starts replacing A85 approximations with direct ports of the
corresponding Java paths.

## Ported in this checkpoint

- `TWater.Sprites.stencil` / `ComposerSources.house`: settlement water now uses
  the four original 16-mask houses from `Water.png`. The former bilinear
  `SmoothFlag` shore approximation is no longer used.
- `GeneratorLake` / `SettlementGrid.Tile.getDirs`: mapped lakes are stamped at
  the source centre/edge/corner positions of the selected 3x3 world footprint.
- Corrected the world-site fertility assignment (`averageFertility`, not
  `averageMoisture`).
- `PlacableFixedTool`: furniture is continuously previewed under the cursor,
  centred on the cursor, with E/Q size selection and R rotation.
- Invalid furniture cells stay visible in red instead of disappearing outside
  the room.
- `SStats`: room stat names are read from the source room text files and the
  selected item's contribution is shown alongside current totals.
- Room placement validation messages used by this flow are now Russian.

## Still required

- Port `GeneratorMountain`, `GeneratorRiver`, `GeneratorRiverSmall`,
  `GeneratorOcean` and their `Polymap`/flooder behaviour without noise-based
  replacement paths.
- Port the complete `TWater` animation/corner/deep-water pass and chunk it at
  the original 16-pixel tile resolution.
- Port `RoomSprite*` and the game sprite-sheet composer for built and previewed
  furnishing art. The current repository does not yet contain
  `Data/Original/assets/sprite/game`, although those atlases exist in the
  supplied `data.zip`.
- Replace the remaining hard-coded `FurnisherStatRuntime` switch with the Java
  `FurnisherStat` class behaviours and add a complete Russian translation
  overlay for source text.

No Godot or C# compilation was run for this checkpoint.
