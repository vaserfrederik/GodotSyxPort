using System;
using System.Collections.Generic;
using System.Linq;
using game.GAME;
using game.battle.div;
using game.boosting;
using game.faction.diplomacy;
using game.faction.npc;
using game.faction.npc.stockpile;
using init.race;
using init.resources;
using init.sprite.UI;
using init.trade;
using settlement.stats;
using settlement.stats.equip;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.info;
using util.text;
using world.map.regions;
using world.region;
using world.region.building;

namespace game.faction
{
    public static class FWorth
    {
        private static readonly string ¤¤resD = "Worth of stored resources";
        private static readonly string ¤¤popD = "Worth of population";
        private static readonly string ¤¤regionD = "Worth of your regions";
        private static readonly string ¤¤vassals = "Vassals";
        private static readonly string ¤¤vassalsD = "Worth of your vassals";
        private static readonly string ¤¤slaves = "Slaves";
        private static readonly string ¤¤slavesD = "Worth of your slaves";

        static FWorth()
        {
            D.ts(typeof(FWorth));
        }

        public readonly WINT resources = new WINT(Dic.¤¤Resources, ¤¤resD, UI.icons().s.storage)
        {
            protected override int pget(Faction f)
            {
                double cache = 0;
                for (int ri = 0; ri < RESOURCES.ALL().size(); ri++)
                {
                    RESOURCE res = RESOURCES.ALL().get(ri);
                    cache += worthResource(TR.get(res), f.res().getAvailable(TR.get(res)));
                }
                for (int ei = 0; ei < STATS.EQUIP().BATTLE_ALL().size(); ei++)
                {
                    EquipBattle e = STATS.EQUIP().BATTLE_ALL().get(ei);
                    int am = 0;
                    for (int di = 0; di < GAME.ARMIES().player().divisions().size(); di++)
                    {
                        Div d = GAME.ARMIES().player().divisions().get(di);
                        am += d.info.equipI(e) * d.menNrOf();
                    }
                    if (am > f.res().getAvailable(TR.get(e.resource)))
                        am = f.res().getAvailable(TR.get(e.resource));
                    cache -= worthResource(TR.get(e.resource), am);
                }
                return (int)cache;
            }
        };

        public static double worthResource(TRADABLE res, int amount)
        {
            return amount * FACTIONS.PRICE().get(res);
        }

        public readonly WINT population = new WINT(Dic.¤¤Population, ¤¤popD, UI.icons().s.human)
        {
            protected override int pget(Faction f)
            {
                double cache = 0;
                for (int ri = 0; ri < RD.RACES().all.size(); ri++)
                {
                    Race res = RD.RACES().all.get(ri).race;
                    cache += f.citizens(res) * NPCStockpile.AVERAGE_PRICE * 5.0 / res.population().max;
                }
                return (int)cache;
            }
        };

        public readonly WINT slaves = new WINT(¤¤slaves, ¤¤slavesD, UI.icons().s.slave)
        {
            protected override int pget(Faction f)
            {
                double cache = 0;
                for (int ri = 0; ri < RACES.all().size(); ri++)
                {
                    Race res = RACES.all().get(ri);
                    cache += FACTIONS.PRICE().get(TR.get(res)) * 0.25 * f.seller(TR.get(res)).removeMax();
                }
                return (int)cache;
            }
        };

        public static double pop(Race race, int amount)
        {
            return amount * FACTIONS.PRICE().get(TR.get(race)) * 0.25;
        }

        public readonly WINT regions = new WINT(Dic.¤¤Regions, ¤¤regionD, UI.icons().s.world)
        {
            protected override int pget(Faction f)
            {
                double cache = 0;
                for (int i = 0; i < f.realm().regions(); i++)
                {
                    Region reg = f.realm().region(i);
                    if (reg.capitol())
                        continue;
                    cache += region(reg);
                }
                return (int)cache;
            }
        };

        public static double region(Region reg)
        {
            double v = -1;
            for (int bi = 0; bi < RD.BUILDINGS().all.size(); bi++)
            {
                RDBuilding b = RD.BUILDINGS().all.get(bi);
                double a = BUtil.value(b.baseFactors, reg);
                v = Math.Max(a, v);
            }
            v *= NPCStockpile.AVERAGE_PRICE;

            v += (RegionInfo.vFer().getAi(reg) + RegionInfo.vArea().getAi(reg)) * FACTIONS.PRICE().edible();
            return v * 0.25 * RD.RACES().population.get(reg);
        }

        public readonly WINT worthVassals = new WINT(¤¤vassals, ¤¤vassalsD, UI.icons().s.noble)
        {
            protected override int pget(Faction f)
            {
                double c = 0;
                foreach (Faction f2 in DIP.VASSAL().all(f))
                {
                    c += vassal(f2);
                }
                return (int)c;
            }
        };

        public static double vassal(Faction fa)
        {
            if (fa is FactionNPC)
            {
                FactionNPC f = (FactionNPC)fa;
                DIP.TMP2().setFactionAndClear(f);
                return Math.Ceiling(DIP.TMP().npc.offerableWorth() * 0.05);
            }
            return 0;
        }

        public readonly WINT cash = new WINT(Dic.¤¤Currs, Dic.¤¤Currs, UI.icons().s.money)
        {
            protected override int pget(Faction f)
            {
                return (int)f.credits().getD();
            }
        };

        public readonly LIST<WINT> raider = new ArrayList<WINT>(cash, resources, slaves);
        public readonly LIST<WINT> faction = new ArrayList<WINT>(cash, resources, population, slaves, regions, worthVassals);

        public double raider()
        {
            return get(raider, FACTIONS.player());
        }

        public double faction()
        {
            return get(faction, FACTIONS.player());
        }

        public double raider(Faction f)
        {
            return get(raider, f);
        }

        public double faction(Faction f)
        {
            return get(faction, f);
        }

        private double get(LIST<WINT> li, Faction f)
        {
            double am = 0;
            foreach (WINT d in li)
                am += d.get(f);
            return am;
        }

        public abstract class WINT
        {
            private readonly int[] upI = Alloc.ii(FACTIONS.MAX());
            private readonly int[] cache = Alloc.ii(FACTIONS.MAX());
            public readonly INFO info;
            public SPRITE icon;

            public WINT(CharSequence name, CharSequence desc, SPRITE icon)
            {
                this.info = new INFO(name, desc);
                this.icon = icon;
                Array.Fill(upI, -1);
            }

            public int player()
            {
                return get(FACTIONS.player());
            }

            public int get(Faction f)
            {
                if (f == null)
                    return 0;
                if (upI[f.index()] != GAME.updateI())
                {
                    upI[f.index()] = GAME.updateI();
                    cache[f.index()] = pget(f);
                }
                return cache[f.index()];
            }

            protected abstract int pget(Faction f);
        }
    }
}