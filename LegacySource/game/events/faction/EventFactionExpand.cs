using System;
using System.Collections.Generic;
using System.IO;
using game.events.EVENTS;
using game.faction.FACTIONS;
using game.faction.Faction;
using game.faction.diplomacy.DIP;
using game.faction.npc.FactionNPC;
using game.time.TIME;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using world.WORLD;
using world.army.AD;
using world.entity.army.WArmy;
using world.map.pathing.WRegFinder.RegDist;
using world.map.pathing.WRegFinder.Treaty;
using world.map.pathing.WRegSel;
using world.map.regions.Region;
using world.region.RD;

namespace game.events.faction
{
    public class EventFactionExpand : EventResource
    {
        private static readonly double dtime = TIME.secondsPerDay() * 2;
        private double timer = dtime;
        private int nextFaction = RND.rInt(FACTIONS.MAX());

        public EventFactionExpand() : base("FACTION_EXPAND")
        {
        }

        protected override void update(double ds)
        {
            timer -= ds * CLAMP.d(FACTIONS.player().realm().regions() / 8.0, 0, 1);
            if (timer > 0)
                return;

            Faction f = FACTIONS.getByIndex(nextFaction);
            if (f != null && f.isActive() && f is FactionNPC)
            {
                trigger((FactionNPC)f);
            }
            clear();
        }

        public bool trigger(FactionNPC f)
        {
            if (DIP.WAR().all(f).Count > 0)
                return false;

            if (f.sanctified)
                return false;

            Region best = null;
            double bv = 0;

            foreach (RegDist d in WORLD.PATH().regFinder.all(f, Treaty.FACTION_BORDERS, WRegSel.FACTION(null)))
            {
                if (FACTIONS.player().realm().regions() == 1 && RD.DIST().reachable(d.reg))
                    continue;
                double v = RD.OWNER().prevOwner(d.reg) == f ? 5.0 : 1.0;
                v /= d.distance;
                if (v > bv)
                {
                    bv = v;
                    best = d.reg;
                }
            }

            if (best == null)
                return false;

            foreach (WArmy a in f.armies().all())
            {
                if (AD.power().get(a) > RD.MILITARY().power.getD(best) * 1.5)
                {
                    a.besiege(best);
                    return true;
                }
            }

            return false;
        }

        protected override void save(FilePutter file)
        {
            file.d(timer);
            file.i(nextFaction);
        }

        protected override void load(FileGetter file)
        {
            timer = file.d();
            nextFaction = file.i();
        }

        protected override void clear()
        {
            timer = RND.rFloat() * dtime;
            nextFaction = RND.rInt(FACTIONS.MAX());
        }
    }
}