using settlement.main;
using settlement.tilemap.floor.Floors;
using settlement.tilemap.terrain;
using snake2d.util.map;

namespace settlement.job
{
    sealed class JobGetter : MAP_OBJECT<Job>
    {
        public override Job get(int tile)
        {
            return get(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
        }

        public override Job get(int tx, int ty)
        {
            if (!SETT.IN_BOUNDS(tx, ty))
                return null;

            {
                Job j = SETT.JOBS().getter.get(tx, ty);
                if (j != null)
                    return j;
            }

            TerrainTile t = SETT.TERRAIN().get(tx, ty);
            if (t is TFortification.Tile)
            {
                return SETT.JOBS().build_fort.all.get(((TFortification.Tile)t).fort.index());
            }
            if (t is TBuilding.BuildingComponent)
            {
                JobBuildStructure tt = SETT.JOBS().build_structure.get(((TBuilding.BuildingComponent)t).building().structure.index());
                if (t is TBuilding.Ceiling || t is TBuilding.Ceiling.Opening)
                    return tt.ceiling;
                return tt.wall;
            }
            if (t is TFence.TFenceTile)
            {
                return SETT.JOBS().fences.get(((TFence.TFenceTile)t).fence.index());
            }
            if (t is TFortification.Stairs)
            {
                return SETT.JOBS().build_fort.build_stairs;
            }

            if (t == SETT.TERRAIN().MOUNTAIN)
            {
                return SETT.JOBS().clearss.caveFill;
            }

            Floor f = SETT.FLOOR().getter.get(tx, ty);
            if (f != null && f.isRoad)
            {
                return SETT.JOBS().roads.all.get(f.indexRoad());
            }
            if (t == SETT.TERRAIN().CAVE)
            {
                return SETT.JOBS().clearss.tunnel;
            }
            return null;
        }
    }
}