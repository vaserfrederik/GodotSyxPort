# Checkpoint A90 — source fishery furniture sprites

This checkpoint ports the fishery (`food/fish`) furnishing art and construction
composition from `settlement.room.food.fish.Constructor`.

## Ported in this checkpoint

- Exact Java `ss`, `cc`, `ms`, `ww`, `m1`, `m2` and `ml` tile-role matrices for
  both furnishing groups and all source variants.
- `STORAGE_BOTTOM_1X1`, `STORAGE_TOP_1X1`, `CANDLE_1X1`, `MISC_1X1`,
  `MISC_TOP_1X1`, `WORKTABLE_1X1`, `AUX_EDGE_1X1`, `AUX_MID_1X1` and
  `AUX_BIG_2X2` source frame selection.
- Original `NATURE.png`, `TABLES.png`, `2xROOF.png` and `HUT.png` atlases.
- `RoomSprite1xN` direction lookup and connected construction masks.
- `RoomSpriteXxX` 2x2 cell extraction and connected construction masks.
- Furnishing upgrade level now travels with built and preview visual metadata;
  changing a room upgrade refreshes its completed source furniture.

## Source correspondence

The renderer uses the frame rows declared in `FISHERY_NORMAL.txt`. The source
footprints are still read from `furnisher_layouts.tsv`, while the per-cell sprite
roles are the matrices from the Java constructor rather than inferred generic art.

## Still required

- Port source shadow halves and degradation layers.
- Port the live candle/resource-on-table state used by `renderAbove`.
- Port the remaining room constructors and their `RoomSprite` subclasses.

No Godot or C# compilation was run for this checkpoint.
