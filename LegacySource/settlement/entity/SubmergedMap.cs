using System;
using settlement.main;
using settlement.room.main;
using settlement.room.service.hygine.bath;
using settlement.room.water.pool;
using snake2d.util.map;

namespace settlement.entity
{
    public class SubmergedMap : MAP_BOOLEAN
    {
        public override bool is(int tx, int ty)
        {
            if (ROOM_BATH.isPool(tx, ty))
                return true;

            if (SETT.TERRAIN().WATER.ice.is(tx, ty))
                return false;
            if (SETT.TERRAIN().WATER.open.is(tx, ty))
                return true;
            Room r = SETT.ROOMS().map.get(tx, ty);
            if (r != null && r.blueprint() is ROOM_POOL)
                return true;
            return false;
        }

        public override bool is(int tile)
        {
            throw new RuntimeException();
        }
    }
}