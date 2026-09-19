# Checkpoint A101 — GeneratorLake, housing placement and preview performance

## Lake generation

- Replaced direct independent lake circles with the two-stage `GeneratorLake` Polymap pass.
- Lake stamps first select coarse shared polygons through `tx/4, ty/4`, then refine those selections into full-resolution settlement polygons.
- Sparse shoreline fragments and `LAKE_ISLANDS` now operate on complete Polymap regions as in Java.

## Housing construction

- Opening another BuildMain room category deactivates and clears the previous unfinished room tool.
- Rebuilt submenu controls are detached immediately, so queued controls from the previous category cannot capture the Housing click and reopen Woodcutter.
- `_HOME` and `_HOME_CHAMBER` retain their fixed-item placement path and now show the compact placement panel instead of area-room shape/stat controls.

## Performance

- Furniture previews are rebuilt only when the cursor cell, room, item group, size, or rotation changes.
- The previous path rebuilt and sorted the complete room preview and recreated furniture nodes every rendered frame; this was especially expensive for large drafts.

## Verification

- Changes were reviewed statically against `GeneratorLake`, `BuildMain`, `UIRoomPlacer`, `Config`, and `PlacableFixedTool` from the Java source archive.
- The project was not compiled, per project workflow.
