# Checkpoint A88 — original RoomSprite furniture rendering foundation

This checkpoint removes the data loss that prevented built furniture from
selecting the same source sprite as its Java `FurnisherItem`.

## Ported in this checkpoint

- Furniture placements now retain `group`, `variant` and `rotation` from the
  placer through construction, room runtime state and save/load (save v37).
- Added a runtime `RoomSpriteCombo`/`RoomSprite1x1` atlas reader for original
  `sprite/game` sheets. It reads the raw left-half colour sheet using the source
  `ComposerSources.house` and `full2` coordinates instead of slicing it as a
  conventional grid.
- Ported the hunter constructor's visual composition:
  `TABLE_COMBO`, `STORAGE_1X1` and the `TOP`/`ANIMAL` nicknack layer.
- Table adjacency uses the original N/E/S/W four-bit combo mask, including
  corners and ends. Larger E/Q variants and R rotations retain their selected
  geometry when construction finishes and after loading a save.
- Added the four source atlases required by `settlement.room.food.hunter.Constructor`:
  `COMBO_TABLES.png`, `STORAGE.png`, `TOP.png`, and `ANIMAL.png`.
- Rooms whose Java `RoomSprite` mapping is not ported yet deliberately keep the
  existing visible box renderer; they are not hidden by the new path.

## Still required

- Extend the sprite metadata/catalog to every Java room constructor and remaining
  `RoomSpriteBoxN`, `RoomSpriteRot`, `RoomSpriteXxX`, 2x2 and 3x3 sheet types.
- Port the source dashed/availability overlays used by `renderPlaceholder`; A86's
  valid/invalid footprint preview remains active until that overlay atlas is wired.
- Port shadow-half rendering, degradation overlays, candles and animated frames.

No Godot or C# compilation was run for this checkpoint.
