using System;
using System.Collections.Generic;
using game;
using game.boosting;
using game.faction;
using game.faction.npc;
using game.faction.royalty.opinion;
using game.time;
using settlement.main;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util;
using world;
using world.army;
using world.entity.army;
using world.map.pathing;
using world.map.regions;

namespace game.faction.diplomacy
{
    public sealed class DipWarPlayer
    {
        private readonly Bitmap1D bPotential;
        private readonly Bitmap1D bWilling;
        private readonly Bitmap1D bProxy;

        private readonly ArrayList<FactionNPC> potential;
        private readonly ArrayList<FactionNPC> willing;
        private readonly ArrayList<FactionNPC> proxy;
        private readonly Distress[] distresses;
        private int upI = -120;

        private double pPow;
        private double coalitionPow;

        public readonly Str warName;
        public readonly Str teamName;

        public DipWarPlayer()
        {
            bPotential = new Bitmap1D(FACTIONS.MAX(), false);
            bWilling = new Bitmap1D(FACTIONS.MAX(), false);
            bProxy = new Bitmap1D(FACTIONS.MAX(), false);

            potential = new ArrayList<FactionNPC>(FACTIONS.MAX());
            willing = new ArrayList<FactionNPC>(FACTIONS.MAX());
            proxy = new ArrayList<FactionNPC>(FACTIONS.MAX());
            distresses = new Distress[FACTIONS.MAX()];

            for (int i = 0; i < distresses.Length; i++)
            {
                distresses[i] = new Distress();
            }

            new DIP.DipActivityListener()
            {
                public override void change(Faction faction, Faction other, DipStance old, DipStance nn)
                {
                    upI = -120;

                    if (nn == DIP.WAR())
                    {
                        if (DIP.WAR().all(FACTIONS.player()).size() == 1)
                        {
                            if (faction == FACTIONS.player())
                            {
                                warName.clear().add(other.race().kingMessage().WAR_CAUSE_AGRESSION.rnd()).insert(0, other.name);
                                teamName.clear().add(other.race().kingMessage().COALITION_NAME.rnd());
                            }
                            else if (other == FACTIONS.player())
                            {
                                teamName.clear().add(other.race().kingMessage().COALITION_NAME.rnd());
                                warName.clear().add(other.race().kingMessage().WAR_CAUSE_DEFEND.rnd()).insert(0, other.name);
                            }
                        }
                    }
                }
            };
        }

        private void init()
        {
            if (Math.Abs(upI - GAME.updateI()) < 120)
                return;

            upI = GAME.updateI();

            {
                bPotential.clear();
                potential.clearSloppy();
                foreach (FactionNPC ff in FACTIONS.NPCs())
                {
                    if (validEnemy(ff))
                    {
                        bPotential.set(ff.index(), true);
                        potential.add(ff);
                    }
                }
            }

            {
                pPow = FACTIONS.player().offensivePower();
                coalitionPow = 0;

                willing.clearSloppy();

                foreach (FactionNPC ff in FACTIONS.NPCs())
                {
                    if (validEnemy(ff))
                    {
                        coalitionPow += AD.power().get(ff);
                        willing.add(ff);
                        bPotential.set(ff.index(), true);
                        potential.add(ff);
                    }
                    else if (DIP.WAR().is(ff))
                    {
                        coalitionPow += AD.power().get(ff);
                    }
                    else if (DIP.ALLY().is(ff))
                    {
                        pPow += AD.power().get(ff);
                    }
                }

                bool recalc = true;
                while (recalc)
                {
                    recalc = false;
                    double advantage = coalitionPow / pPow - 1;
                    if (advantage < 0)
                    {
                        willing.clearSloppy();
                        break;
                    }
                    for (int i = willing.size() - 1; i >= 0; i--)
                    {
                        FactionNPC f = willing.get(i);
                        if (ROPINION.trust().get(f) > advantage + distress(f))
                        {
                            double pow = f.offensivePower();
                            coalitionPow -= pow;
                            if (DIP.get(f).ally)
                            {
                                pPow += pow;
                            }
                            recalc = true;
                            willing.remove(i);
                        }
                    }
                }

                bWilling.clear();
                foreach (FactionNPC f in willing)
                {
                    bWilling.set(f.index(), true);
                }
            }

            {
                LIST<RegDist> proxies = WORLD.PATH().regFinder.all(FACTIONS.player(), treaty, WRegSel.DUMMY());
                bProxy.clear();
                proxy.clearSloppy();
                foreach (RegDist r in proxies)
                {
                    if (r.reg.faction() != null && !bProxy.get(r.reg.faction().index()) && r.reg.faction() is FactionNPC f)
                    {
                        if (validProxy(f))
                        {
                            proxy.add(f);
                        }
                    }
                }
            }
        }

        public LIST<FactionNPC> potential()
        {
            init();
            return potential;
        }

        public LIST<FactionNPC> willing()
        {
            init();
            return willing;
        }

        public LIST<FactionNPC> proxy()
        {
            init();
            return proxy;
        }

        public bool potential(FactionNPC f)
        {
            init();
            return bPotential.get(f.index());
        }

        public bool willing(FactionNPC f)
        {
            init();
            return bWilling.get(f.index());
        }

        public bool proxy(FactionNPC f)
        {
            init();
            return bProxy.get(f.index());
        }

        public double coalitionPower()
        {
            init();
            return coalitionPow;
        }

        public double playerPower()
        {
            init();
            return pPow;
        }

        public double coalitionAdvantage()
        {
            return CLAMP.d((coalitionPower() / (playerPower() + coalitionPower() + 1)) * 0.5 - 0.25 + distress(FACTIONS.player()), 0, 1);
        }

        private bool validEnemy(FactionNPC f)
        {
            if (DIP.WAR().is(f))
                return false;

            if (ROPINION.trust().get(f) >= 1)
                return false;

            if (DIP.secondSinceStance(f) < TIME.secondsPerDay() / 2)
                return false;

            if (RD.DIST().factionCanAttackPlayerAllies(f))
                return true;

            foreach (WArmy a in FACTIONS.player().armies().all())
            {
                if (a.region() != null && a.region().faction() == f)
                    return true;
            }

            return false;
        }

        private readonly Treaty treaty = new Treaty()
        {
            public bool can(Region origin, Region prevReg, Region to, int tx, int ty, double dist)
            {
                if (prevReg == null)
                    return true;

                if (prevReg.faction() == null)
                    return false;

                if (prevReg.faction() == FACTIONS.player())
                    return true;

                if (prevReg.faction() is FactionNPC f)
                {
                    if (DIP.ALLY().is(f, prevReg.faction()))
                        return true;
                }

                return false;
            }
        };

        private bool validProxy(FactionNPC f)
        {
            if (DIP.WAR().is(f))
                return false;

            if (DIP.ALLY().is(f))
                return false;

            if (DIP.secondSinceStance(f) < TIME.secondsPerDay() / 2)
                return false;

            return true;
        }

        public double distress(Faction f)
        {
            Distress d = distresses[f.index];

            if (Math.Abs(d.upI - GAME.updateI()) < 120)
                return d.distress;

            d.upI = GAME.updateI();

            double threatened = 0;
            double valueTot = 0;
            double capitol = 0;

            foreach (Region reg in f.realm().regions())
            {
                valueTot += FWorth.region(reg);

                double def = Math.Max(AD.power().get(reg.defense()), 0);
                double att = 0;

                foreach (WArmy a in WORLD.ENTITIES().armies.fill(reg))
                {
                    if (a.faction() != null)
                    {
                        if (a.faction() == f || DIP.ALLY().is(f, a.faction()))
                        {
                            def += AD.power().get(a);
                        }
                        else if (DIP.WAR().is(a.faction(), f))
                        {
                            att = AD.power().get(a);
                        }
                    }
                }

                double am = (att + 1) / (def + 1) - 1;

                if (am > 0)
                {
                    if (reg.capitol())
                    {
                        capitol = reg.besieged() ? 1.0 : 0.2;
                    }
                    threatened += FWorth.region(reg);
                }
            }

            threatened += valueTot * capitol;

            d.distress = 2.0 * threatened / valueTot;
            return d.distress;
        }

        public void debug(Debugger d, FactionNPC f)
        {
            d.title(typeof(DipWarPlayer).Name);
            d.debug("potential").add(potential(f));
            d.debug("willing").add(validEnemy(f));
            d.debug("proxy").add(validProxy(f));
            d.debug("distress").add(distress(f));
        }

        public void debug(Debugger d)
        {
            d.title(typeof(DipWarPlayer).Name);

            d.debug("player Pow").add(playerPower());
            d.debug("enemy pow").add(coalitionPower());
            d.debug("distress player").add(distress(FACTIONS.player()));
            d.debug("col advantage").add(coalitionAdvantage());

            foreach (FactionNPC f in willing())
            {
                d.debug("willing").add(f.name).s().add(ROPINION.trust().get(f)).s().add(distress(f));
            }

            foreach (FactionNPC f in proxy())
            {
                d.debug("proxy").add(f.name).s().add(distress(f));
            }
        }

        private class Distress
        {
            public int upI = -120;
            public double distress;
        }
    }
}