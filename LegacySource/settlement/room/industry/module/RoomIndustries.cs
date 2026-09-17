using System.Collections.Generic;
using settlement.room.main;
using snake2d.util.sets;

namespace settlement.room.industry.module
{
    public class RoomIndustries
    {
        public readonly LIST<Industry> all;

        public RoomIndustries(ROOMS rooms)
        {
            int am = 0;
            foreach (RoomBlueprint b in rooms.all())
            {
                if (b is INDUSTRY_HASER)
                {
                    INDUSTRY_HASER h = (INDUSTRY_HASER)b;
                    am += h.industries().size();
                }
            }

            ArrayList<Industry> hh = new ArrayList<Industry>(am);
            foreach (RoomBlueprint b in rooms.all())
            {
                if (b is INDUSTRY_HASER)
                {
                    INDUSTRY_HASER h = (INDUSTRY_HASER)b;
                    foreach (Industry i in h.industries())
                        hh.add(i);
                }
            }
            all = new ArrayList<Industry>(hh);
        }
    }
}