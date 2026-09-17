using settlement.room.food.hunter;
using static settlement.main.SETT;
using settlement.main;
using settlement.room.main.util;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.food.hunter
{
    public class Tile
    {
        public readonly Coo coo = new Coo();

        public readonly RoomBits reserved = new RoomBits(coo, new Bits(0b0000_0001));
        public readonly RoomBits cadaver = new RoomBits(coo, new Bits(0b0000_0010));
        private static readonly Bits gore = new Bits(0b0111_0000);

        public Tile(ROOM_HUNTER h)
        {
        }

        public Tile Init(int tx, int ty, HunterInstance ins)
        {
            if (!ins.Is(tx, ty))
                return null;
            if (ROOMS().fData.tile.Is(tx, ty, ins.BlueprintI().constructor.ww))
            {
                this.coo.Set(tx, ty);
                return this;
            }
            return null;
        }

        public void Reset(HunterInstance ins, COORDINATE c)
        {
            int d = ROOMS().data.Get(c);
            ROOMS().data.Set(ins, c, gore.Set(d, 0));
        }

        public void Gore(HunterInstance ins, COORDINATE c)
        {
            int d = ROOMS().data.Get(c);
            ROOMS().data.Set(ins, c, gore.Inc(d, 1));
        }
    }
}