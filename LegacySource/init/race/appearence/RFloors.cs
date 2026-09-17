using System.Collections.Generic;
using settlement.main;
using settlement.room.main;
using settlement.tilemap.floor.Floors;
using snake2d.util.file;
using snake2d.util.sets;

namespace init.race.appearence
{
    public sealed class RFloors
    {
        private readonly LL[] overrideFloors;

        public RFloors(Json json)
        {
            overrideFloors = new LL[SETT.ROOMS().AMOUNT_OF_BLUEPRINTS];

            SETT.ROOMS().collection.new KJson("ROOM_FLOOR_OVERRIDE", json)
            {
                protected override void process(RoomBlueprint s, Json j, string key, bool isWeak)
                {
                    if (overrideFloors[s.index()] == null)
                        overrideFloors[s.index()] = new LL();
                    foreach (Floor f in SETT.FLOOR().map.readMany(key, j))
                    {
                        overrideFloors[s.index()].add(f);
                    }
                }
            };
        }

        public Floor get(RoomBlueprint b, int i, Floor backup)
        {
            LL li = overrideFloors[b.index()];
            if (li == null)
                return backup;
            if (i >= li.size())
                return backup;
            return li.get(i);
        }

        private class LL : ArrayListGrower<Floor>
        {
            private static readonly long serialVersionUID = 1L;
        }
    }
}