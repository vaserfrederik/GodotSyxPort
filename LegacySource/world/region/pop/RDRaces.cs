using System;
using System.Collections.Generic;
using game.boosting;
using game.faction;
using game.faction.npc;
using init.constant;
using init.race;
using init.sprite.UI;
using init.value;
using settlement.entity;
using settlement.tilemap.ground;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.info;
using util.text;
using view.main;
using world.map.regions;
using world.region;
using world.region.RD;

public class RDRaces
{
    public static readonly string ¤¤Loyalty = "¤Loyalty";
    public static readonly string ¤¤LoyaltyD = "¤Current Loyalty. Loyalty determines the chance of rebellion. Loyalty changes slowly based on the target. Increase loyalty by allocating admin points into loyalty boosting areas. Loyalty will also increase the longer a region has belonged to you. Loyalty is species specific.";
    private static readonly string ¤¤RegionCapacity = "¤Region Capacity";
    private static readonly string ¤¤RegionCapacityD = "¤Region Population capacity.";

    static RDRaces()
    {
        D.ts(typeof(RDRaces));
    }

    private readonly RDRace[] map = new RDRace[RACES.all().size()];
    public readonly LIST<RDRace> all;
    public readonly Boostable capacity;
    public readonly RDEdicts edicts;
    private readonly RDataE pop;
    public readonly RData population;
    public readonly Visuals visuals;

    private readonly double mCapacity = Config.world().POPULATION_CAPACITY_MAX;

    public readonly DoubleOCached<Region> popTarget = new DoubleOCached<Region>()
    {
        public override double getValue(Region t)
        {
            double cache = 0;
            for (int ri = 0; ri < all.size(); ri++)
            {
                cache += all.get(ri).pop.target(t);
            }
            return cache;
        }
    };

    public readonly DOUBLE_O<Region> loyaltyAll = new DOUBLE_O<Region>()
    {
        private readonly INFO info = new INFO(¤¤Loyalty, ¤¤LoyaltyD);

        public override double getD(Region t)
        {
            double d = 0;
            foreach (RDRace r in all)
            {
                d += r.pop.get(t) * r.loyalty.getD(t);
            }
            if (population.get(t) > 0)
                d /= population.get(t);
            return d;
        }

        public override INFO info()
        {
            return info;
        }
    };

    public INT_O<Region> capacityCurrent = new INT_O<Region>()
    {
        public override int get(Region t)
        {
            double cc = 0;
            for (int ri = 0; ri < RD.RACES().all.size(); ri++)
            {
                RDRace r = RD.RACES().all.get(ri);
                cc += r.pop.get(t) / r.pop.maxPopulation;
            }
            return (int)cc;
        }

        public override int min(Region t)
        {
            return 0;
        }

        public override int max(Region t)
        {
            return int.MaxValue;
        }
    };

    public RDRaces(RDInit init)
    {
        capacity = BOOSTING.push("POPULATION_CAPACITY", 1, ¤¤RegionCapacity, ¤¤RegionCapacityD, UI.icons().s.human, BoostableCat.ALL().WORLD, 0);
        GVALUES.REGION.pushI("POPULATION_CAPACITY_USED", ¤¤RegionCapacity, UI.icons().s.human, capacityCurrent);

        new RBooster(new BSourceInfo(Dic.¤¤Area, UI.icons().s.expand), 0, 32768, true)
        {
            public override double get(Region t)
            {
                double a = t.info.area();
                a = CLAMP.d(a, 50, int.MaxValue);
                return a / 32768.0;
            }
        }.add(capacity);

        new RBooster(new BSourceInfo(Ground.¤¤moisture, UI.icons().s.sprout), 0.2, 1, true)
        {
            public override double get(Region t)
            {
                return t.info.moisture();
            }
        }.add(capacity);

        pop = new RDataE("POPULATION", init.count.new DataInt("POPULATION"), init, Dic.¤¤Population);
        population = pop;

        GVALUES.REGION.pushI("POPULATION", Dic.¤¤Population, UI.icons().s.human, pop);
        GVALUES.REGION.pushI("POPULATION_TARGET", Dic.¤¤Population + ": " + Dic.¤¤Target, UI.icons().s.human, new INT_O<Region>()
        {
            public override int get(Region t)
            {
                return (int)popTarget.getD(t);
            }

            public override int min(Region t)
            {
                return 0;
            }

            public override int max(Region t)
            {
                return int.MaxValue;
            }
        });
        GVALUES.FACTION.pushI("POPULATION_KINGDOM", Dic.¤¤Population + ": " + Dic.¤¤Realm, UI.icons().s.human, new INT_O<Faction>()
        {
            public override int get(Faction t)
            {
                if (t == null)
                    return 0;
                return pop.faction().get(t);
            }

            public override int min(Faction t)
            {
                return 0;
            }

            public override int max(Faction t)
            {
                return int.MaxValue;
            }
        });
        GVALUES.REGION.pushI("POPULATION_KINGDOM", Dic.¤¤Population + ": " + Dic.¤¤Realm, UI.icons().s.human, new INT_O<Region>()
        {
            public override int get(Region t)
            {
                if (t.faction() == FACTIONS.player())
                    return 0;
                return pop.faction().get(t);
            }

            public override int min(Region t)
            {
                return 0;
            }

            public override int max(Region t)
            {
                return int.MaxValue;
            }
        });

        visuals = new Visuals(init);
    }

    public final class Visuals
    {
        private readonly INT_OE<Region> cRace;
        private readonly INT_OE<Region> cacheI;
        private readonly ArrayList<INT_OE<Region>> vVill = new ArrayList<INT_OE<Region>>(16);

        private Visuals(RDInit init)
        {
            if (all.size() > 255)
                throw new RuntimeException("too many races");
            cRace = init.count.new DataByte("VISUALS_RACE");
            cacheI = init.count.new DataNibble("VISUALS_RACEI");
            while (vVill.hasRoom())
                vVill.add(init.count.new DataByte("VISUALS_RACE?" + vVill.size()));
        }

        public Race cRace(Region reg)
        {
            cache(reg);
            return all.get(cRace.get(reg)).race;
        }

        public Race vRace(Region reg, int ran)
        {
            cache(reg);
            ran &= 0x0F;
            return all.get(vVill.get(ran).get(reg)).race;
        }

        private void cache(Region reg)
        {
            int ri = (0x0F - ((VIEW.RI() >> 6) & 0x0F));
            if (cacheI.get(reg) == ri)
                return;
            cacheI.set(reg, ri);
            RDRace biggest = null;
            int bb = -1;
            int vi = 0;
            for (int rri = 0; rri < all.size(); rri++)
            {
                RDRace r = all.get(rri);
                if (r.pop.get(reg) > bb)
                {
                    biggest = r;
                    bb = r.pop.get(reg);
                }
                if (population.get(reg) > 0)
                {
                    int vam = 16 * r.pop.get(reg) / population.get(reg);
                    for (int i = 0; i < vam && vi < 16; i++)
                    {
                        vVill.get(vi++).set(reg, r.index());
                    }
                }
            }

            cRace.set(reg, biggest.index());

            for (; vi < 16; vi++)
            {
                vVill.get(vi).set(reg, biggest.index());
            }
        }
    }
}