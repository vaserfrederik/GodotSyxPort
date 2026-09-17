# Checkpoint A87 — source Polymap mountains and river routing

This checkpoint removes the remaining threshold-noise mountain shape and the
hand-bent straight river branches from settlement generation.

## Ported in this checkpoint

- `snake2d.util.rnd.Polymap(width,height)`: deterministic polygon ids and the
  original east/south `isEdge` test are now shared by mountains and rivers.
- `GeneratorMountain`: the selected 3x3 world footprint is processed with the
  source `SettlementGrid.Tile.getDirs()` ownership order, `MOUNTAIN_SIZE*100`
  stamps, eight-tile coarse pass, polygon refinement, arrival corridors and
  water-connection corridors.
- Strategic mountain height is retained in `SettlementWorldTileSample`, so a
  massif interior is distinguished from a height-one world edge.
- `GeneratorRiver` / `GeneratorRiverSmall`: every connected direction pair is
  routed inside its corresponding world-tile quadrant. The former invented
  centre hub and sinusoidal/noise bend are gone.
- River entry points are selected from Polymap edges and A* uses the source
  costs: outside quadrant 50, mountain 20, non-polygon-edge 2, existing shallow
  water 0.5, ordinary polygon edge 1.
- River paths are widened with the source fertility radius and elevation cost;
  lakes are applied only after both river passes, matching `Generator.java`.

## Data limitation retained explicitly

The C# strategic world currently stores mountain occupancy and height, but not
the Java `WorldMountain` four-corner join nibble. Height-greater-than-one points
therefore follow the exact source rule; height-one boundary points are rebuilt
from adjacent mountain occupancy. Porting the join nibble into strategic-world
saves will remove this final boundary-data approximation.

## Still required

- Port the complete `GeneratorOcean` and `GeneratorWaterFin` flood/height passes.
- Persist the strategic `WorldMountain` join nibble and consume it directly in
  `AreaTileMountain.is`/`borders`.
- Port `RoomSprite*` plus the original `sprite/game` atlases for finished room
  furniture, and replace the remaining hard-coded room-stat calculations.

No Godot or C# compilation was run for this checkpoint.
