# Checkpoint A92 — selected world site terrain input

This checkpoint fixes the source-data boundary between the selected world-map
site and settlement generation.  The chosen capital centre was preserved, but
the C# generators only retained its 3x3 footprint and clamped every lookup at
that footprint's edge.  The Java generators continue reading adjacent world
tiles there, so boundary rivers, lakes, mountains and moisture were severed.

## Ported in this checkpoint

- `FromWorldSite` now retains the selected 3x3 `CapitolArea` plus the one-tile
  source halo required by its generators (5x5 world samples in total).
- Mountain area points, river/lake joins, ocean joins and generated roads read
  the real adjacent world tile instead of treating the 3x3 edge as empty.
- `GeneratorFertilityInit.getBase` interpolation can read neighbouring moisture
  outside the selected 3x3 area instead of clamping back to the edge tile.
- Settlement climate now comes from the exact capital centre, matching
  `CapitolArea.init`, rather than the majority climate of nine tiles.
- The invented strategic-height/local-height blend was removed.  The source
  uses the settlement-local `GeneratorUtil.height`; world elevation affects
  topology through the original mountain and water generators.

## Source correspondence

- `settlement.main.CapitolArea.init`
- `settlement.main.SettlementGrid`
- `settlement.tilemap.generator.GeneratorFertilityInit.getBase`
- `settlement.tilemap.generator.GeneratorMountain`
- `settlement.tilemap.generator.GeneratorLake`
- `settlement.tilemap.generator.GeneratorRiver`
- `settlement.tilemap.generator.GeneratorOcean`
- `settlement.tilemap.generator.GeneratorRoads`

No Godot or C# compilation was run for this checkpoint.
