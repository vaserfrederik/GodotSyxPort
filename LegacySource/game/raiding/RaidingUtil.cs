using System;
using System.Collections.Generic;
using game.GAME;
using game.boosting;
using game.faction;
using game.raiding.RaidingMap;
using init.constant;
using init.type;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.sets;
using world;
using world.army;
using world.entity.army;
using world.map.regions;
using world.region;

namespace game.raiding
{
    public sealed class RaidingUtil
    {
        private readonly ArrayList<Raider> active;
        private int cp = -1;
        private int weakest = -1;

        public RaidingUtil(int AMOUNT)
        {
            active = new ArrayList<Raider>(AMOUNT);
        }

        public int playerPow()
        {
            if (FACTIONS.player().capitolRegion() == null)
                return 0;

            return defences(FACTIONS.player().capitolRegion());
        }

        public int defences(Region reg)
        {
            double p = RD.MILITARY().power.getD(reg);

            if (reg == FACTIONS.player().capitolRegion() && GAME.raiders().entry.get(FACTIONS.player().capitolRegion()).points() > 0)
            {
                p *= 0.25;
                foreach (WArmy a in WORLD.ENTITIES().armies.fill(reg))
                    if (a.faction() == reg.faction())
                        p += AD.power().get(a);
            }
            else
            {
                foreach (WArmy a in FACTIONS.player().armies().all())
                    if (a.region() != null && a.region().faction() == FACTIONS.player())
                        p += AD.power().get(a);
            }
            return (int)p;
        }

        public int ransomCurrent()
        {
            return (int)((POP.tot(null, null) * Config.sett().POP_RAIDER_WORTH + FACTIONS.player().credits().getD() / Config.sett().POP_RAIDER_WORTH) / BOOSTABLES.CIVICS().RAID_SECURITY.get(HCLASS_RACE.clP()));
        }

        public COORDINATE attackSpot(Raider raider)
        {
            double prob = 0;

            foreach (RaidRegion r in GAME.raiders().entry.entryRegions())
            {
                if (defences(r.r()) < raider.army.power)
                    prob += r.r().capitol() ? 10 : 1;
            }
            prob *= RND.rFloat();
            foreach (RaidRegion r in GAME.raiders().entry.entryRegions())
            {
                if (defences(r.r()) < raider.army.power)
                {
                    prob -= r.r().capitol() ? 10 : 1;
                    if (prob <= 0)
                    {
                        prob = 0;

                        foreach (RaidEntryPoint e in GAME.raiders().entry.entrySpots())
                        {
                            if (r.r().is(e.c()))
                                prob++;
                        }
                        prob *= RND.rFloat();
                        foreach (RaidEntryPoint e in GAME.raiders().entry.entrySpots())
                        {
                            if (r.r().is(e.c()))
                            {
                                prob--;
                                if (prob <= 0)
                                    return e.c();
                            }
                        }
                    }
                }
            }

            Coo.TMP.set(FACTIONS.player().capitolRegion().cx(), FACTIONS.player().capitolRegion().cy());
            return Coo.TMP;
        }

        public Region weakestRegion()
        {
            cache();
            if (weakest == -1)
                return null;
            return WORLD.REGIONS().all().get(cp);
        }

        public int weakestRegionPow()
        {
            Region reg = weakestRegion();
            if (reg == null)
                return 0;
            return defences(reg);
        }

        public LIST<Raider> active()
        {
            cache();
            return active;
        }

        public bool validCoo(COORDINATE c, Raider raider)
        {
            Region reg = WORLD.REGIONS().map.get(c);
            if (reg == null || reg.faction() != FACTIONS.player())
                return false;
            return defences(reg) <= raider.army.power;
        }

        private void cache()
        {
            if (Math.Abs(cp - GAME.updateI()) < 128)
                return;

            if (cp == GAME.updateI())
                return;
            cp = GAME.updateI();
            weakest = -1;
            double power = double.MaxValue * 0.5;

            foreach (RaidRegion reg in GAME.raiders().entry.entryRegions())
            {
                if (reg.r().capitol())
                    continue;

                double pow = defences(reg.r());
                if (weakest == -1 || pow < power)
                {
                    weakest = reg.r().index();
                    power = pow;
                }
            }

            active.clearSloppy();

            foreach (Raider r in GAME.raiders().ALL())
            {
                if (!r.defeated && !r.isScared() && r.hasInterrest())
                    active.add(r);
            }
        }

        public void clear()
        {
            cp = -1;
        }
    }
}