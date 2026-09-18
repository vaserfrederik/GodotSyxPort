# Checkpoint A93 — source build-menu cascade and fixed houses

This checkpoint replaces two generic C# behaviours with the corresponding
Java branches used by `BuildMain` and `UIRoomPlacer`.

## Build-menu behaviour

- Clicking a main room category now opens only its first column.
- The second room column remains closed until the pointer enters a subcategory,
  matching `BuildMain.BExp.hover` and `Inter.exp`.
- Changing the main category closes the previously expanded subcategory.
- Room families use the registration/prefix order from `ROOMS.java`; translated
  names are no longer alphabetically resorted.
- Law and logistics entries follow their source registration order.

## Fixed house placement

- `_HOME` and `_HOME_CHAMBER` now take the `Furnisher.usesArea() == false`
  placement branch instead of opening the ordinary room-area editor.
- `HomeContructor`'s procedural apartment, house and longhouse footprints are
  extracted into the runtime layout table: 9, 15 and 30 size variants.
- `E`/`Q` change the actual repeated source footprint, `R` rotates it, the cursor
  previews the complete building, and a left click creates its construction
  project directly as in `PlacableFixed`.
- The fixed-house panel hides area/wall/door editing controls and shows the three
  original dwelling types with Russian labels.
- The longhouse dimension limit now admits the source maximum of 30 five-tile
  modules without increasing the limit used by hand-drawn rooms.

## Source correspondence

- `view.sett.ui.bottom.BuildMain`
- `view.sett.ui.bottom.Inter`
- `view.sett.ui.room.construction.UIRoomPlacer.init`
- `settlement.room.main.category.RoomCategories`
- `settlement.room.main.ROOMS`
- `settlement.room.home.house.HomeContructor`

The terrain-job and construction-action contents of the fifth and sixth main
menus still need their individual Java placers; this checkpoint does not map
missing jobs to unrelated generic tools.

No Godot or C# compilation was run for this checkpoint.
