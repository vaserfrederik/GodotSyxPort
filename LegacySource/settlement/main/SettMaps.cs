using System;
using settlement.misc.util;
using settlement.room.main;
using snake2d.util.map;

namespace settlement.main
{
    public sealed class SettMaps
    {
        public SettMaps()
        {
        }

        public readonly MAP_OBJECT<TILE_STORAGE> STORAGE = new MAP_OBJECT<TILE_STORAGE>
        {
            Get = (tx, ty) =>
            {
                Room r = SETT.ROOMS().map.Get(tx, ty);
                if (r != null)
                {
                    return r.storage(tx, ty);
                }
                return null;
            },

            GetTile = tile =>
            {
                int x = tile % SETT.TWIDTH;
                int y = tile / SETT.TWIDTH;
                return Get(x, y);
            }
        };
    }
}