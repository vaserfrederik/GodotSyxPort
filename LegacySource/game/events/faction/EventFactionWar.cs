using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using world.map.pathing;
using world.region;
using world.region.pop;

namespace game.events.faction
{
    public class EventFactionWar : EventResource
    {
        private const double dtime = TIME.secondsPerDay() * 16;
        private double timer = dtime;
        private int nextFaction;

        public EventFactionWar() : base("FACTION_WAR")
        {
        }

        protected override void update(double ds)
        {
            timer -= ds * CLAMP.d(FACTIONS.player().realm().regions() / 8.0, 0, 1);
            if (timer > 0)
                return;

            FactionNPC f = FACTIONS.NPCs().getC(nextFaction);

            if (f == null)
                return;

            clear();
            timer = 16;

            if (DIP.get(f).ally)
                return;
            if (f.sanctified)
                return;

            if (f != null && f.isActive() && f.capitolRegion() != null && DIP.WAR().all(f).Count == 0)
            {
                Faction enemy = null;
                double bestE = 0;

                foreach (RegDist d in WORLD.PATH().regFinder.all(f, Treaty.FACTION_BORDERS, WRegSel.CAPITOLS()))
                {
                    if (d.reg.faction() == null)
                        continue;
                    if (d.reg.faction() == f || d.reg.faction() == FACTIONS.player())
                        continue;
                    if (FACTIONS.player().realm().regions() < 3 && RD.DIST().reachable(d.reg))
                        continue;
                    FactionNPC ff = (FactionNPC)d.reg.faction();
                    if (DIP.get(ff).ally)
                        continue;
                    if (ff.sanctified)
                        continue;

                    double v = 0;

                    foreach (RDRace race in RD.RACES().all)
                    {
                        v += race.pop.get(d.reg) * race.race.pref().race(f.race());
                    }
                    v = 1.0 / v;
                    if (v > bestE)
                    {
                        enemy = d.reg.faction();
                        bestE = v;
                    }
                }

                if (enemy != null)
                {
                    DIP.WAR().set(f, enemy);
                    timer += TIME.secondsPerDay() * 20;
                }
                else
                {

                }
            }
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
            timer = 16;
        }

        protected override void clear()
        {
            timer = dtime;
            nextFaction = RND.rInt();
        }
    }
}