using System;
using System.IO;
using game.events.EVENTS;
using game.faction.FACTIONS;
using game.time.TIME;
using snake2d.util.file;
using snake2d.util.rnd;
using world.WORLD;
using world.map.regions;

namespace game.events.faction
{
    public class EventFactionPopup : EventResource
    {
        private const double dtime = TIME.SecondsPerDay() * 16;
        private double timer = dtime;
        private int nextRegion;

        public EventFactionPopup() : base("FACTION_POPUP")
        {
        }

        protected override void update(double ds)
        {
            timer -= ds;
            if (timer > 0)
                return;

            Region r = WORLD.REGIONS().active().getC(nextRegion);
            if (FACTIONS.active().size() > FACTIONS.MAX() - 16 || r == null || r.faction() != null)
            {
                clear();
                return;
            }
            FACTIONS.activateNext(r, null, true);

            clear();
        }

        protected override void save(FilePutter file)
        {
            file.d(timer);
            file.i(nextRegion);
        }

        protected override void load(FileGetter file)
        {
            timer = file.d();
            nextRegion = file.i();
        }

        protected override void clear()
        {
            timer = RND.rFloat() * dtime;
            nextRegion = RND.rInt();
        }
    }
}