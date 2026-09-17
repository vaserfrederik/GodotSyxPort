using settlement.main;
using settlement.room.main;
using snake2d.util.map;

namespace settlement.thing.pointlight
{
    public sealed class LOS_MAP : MAP_OBJECT<LOS>
    {
        public LOS_MAP()
        {
            // TODO Auto-generated constructor stub
        }

        public override LOS Get(int tile)
        {
            return Get(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
        }

        public override LOS Get(int tx, int ty)
        {
            RoomBlueprint p = SETT.ROOMS().Map.Blueprint.Get(tx, ty);
            if (p != null)
            {
                return p.LOS(tx, ty);
            }

            return SETT.TILE_MAP().LOS(tx, ty);
        }
    }
}