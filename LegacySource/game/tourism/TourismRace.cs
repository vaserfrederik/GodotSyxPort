using System;
using System.Collections.Generic;
using System.Linq;
using init.paths;
using init.race;
using settlement.main;
using settlement.room.main;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.tourism
{
    public class TourismRace
    {
        private static readonly KeyMap<Text> cache = new KeyMap<Text>();
        private static readonly Text DUMMY = new Text(null);

        public readonly double occurence;
        public readonly double credits;
        readonly Text data;
        public readonly LIST<RoomBlueprintIns<?>> attractions;

        public TourismRace(Json json, Race race)
        {
            if (json.Has("TOURIST"))
            {
                json = json.Json("TOURIST");
                occurence = json.d("OCCURENCE", 0, 100000);
                credits = json.d("CREDITS", 0, 100000);
                string d = json.value("TOURIST_TEXT_FILE");
                if (!cache.ContainsKey(d))
                {
                    cache.Put(d, new Text(new Json(PATHS.TEXT().getFolder("race").getFolder("tourist").gets(d))));
                }
                data = cache.Get(d);
            }
            else
            {
                data = DUMMY;
                occurence = 0;
                credits = 0;
            }

            ArrayListGrower<RoomBlueprintImp> li = new ArrayListGrower<RoomBlueprintImp>();
            foreach (RoomBlueprintImp b in SETT.ROOMS().bonus.all)
            {
                if (b.employment() != null && race.pref().getWork(b.employment()) > 0)
                {
                    li.Add(b);
                }
            }

            li.Sort((o1, o2) =>
            {
                double v = race.pref().getWork(o1.employment()) - race.pref().getWork(o2.employment());
                if (v < 0)
                    return 1;
                else if (v > 0)
                    return -1;
                return 0;
            });

            int am = Math.Min(li.size(), 5);
            ArrayList<RoomBlueprintIns<?>> res = new ArrayList<RoomBlueprintIns<?>>(am);
            foreach (RoomBlueprint b in li)
            {
                res.Add((RoomBlueprintIns<?>)b);
                if (!res.hasRoom())
                    break;
            }

            this.attractions = res;
        }

        public RoomBlueprintIns<?> getAttraction(long ran)
        {
            return attractions.Get((int)ran);
        }
    }
}