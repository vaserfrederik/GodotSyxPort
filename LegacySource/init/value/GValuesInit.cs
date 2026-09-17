using System;
using System.Collections.Generic;
using game.GAME;
using game.boosting;
using game.faction;
using game.faction.FResources;
using game.faction.diplomacy;
using game.faction.royalty;
using game.faction.royalty.opinion;
using init.sprite.UI;
using init.type;
using settlement.stats;
using util.data;
using util.text;
using world.map.regions;

namespace init.value
{
    class GValuesInit
    {
        private static string ¤¤roySucc = "Succession Order";
        private static string ¤¤regFactionNo = "Free Lands";

        static GValuesInit()
        {
            D.ts(typeof(GValuesInit));
        }

        public static void init()
        {
            faction();
            royalty();
            region();
        }

        private static void royalty()
        {
            GValueCat<Royalty> V = GVALUES.ROYALTY;

            foreach (Value<Faction> v in GVALUES.FACTION.map().allSorted())
            {
                DOUBLE_O<Royalty> va = new DOUBLE_O<Royalty>()
                {
                    getD = (Royalty t) => v.d.getD(t.court.faction)
                };
                Value<Royalty> roy = new Value<Royalty>("FACTION_" + v.key, v.icon, v.name, va, v.percentage, v.isBool);
                V.push(roy);
            }

            foreach (Value<Induvidual> v in GVALUES.INDU.map().allSorted())
            {
                DOUBLE_O<Royalty> va = new DOUBLE_O<Royalty>()
                {
                    getD = (Royalty t) => v.d.getD(t.induvidual)
                };
                Value<Royalty> roy = new Value<Royalty>("INDUVIDUAL_" + v.key, v.icon, v.name, va, v.percentage, v.isBool);
                V.push(roy);
            }

            for (int i = 0; i < NPCCourt.MAX; i++)
            {
                int k = i;
                V.push("SUCCESSION_ORDER_" + i, ¤¤roySucc, UI.icons().s.noble, new BOOLEANO<Royalty>()
                {
                    isValue = (Royalty t) => t.successionI() == k
                });
            }
        }

        public static void region()
        {
            GValueCat<Region> V = GVALUES.REGION;

            foreach (Value<Faction> v in GVALUES.FACTION.map().allSorted())
            {
                DOUBLE_O<Region> va = new DOUBLE_O<Region>()
                {
                    getD = (Region t) => t.faction() == null ? 0 : v.d.getD(t.faction())
                };
                Value<Region> roy = new Value<Region>("FACTION_" + v.key, v.icon, v.name, va, v.percentage, v.isBool);
                V.push(roy);
            }

            V.push("FACTION_NONE", ¤¤regFactionNo, UI.icons().s.flag, new BOOLEANO<Region>()
            {
                isValue = (Region t) => t.faction() == null
            });

            V.push("HAS_BOOST_PERM", "Has Boost", UI.icons().s.question, new BOOLEANO<Region>()
            {
                isValue = (Region t) => GAME.BOOST().regions.any(t)
            });

            V.push("IS_CAPITAL", Dic.¤¤Capitol, UI.icons().s.question, new BOOLEANO<Region>()
            {
                isValue = (Region t) => t.capitol()
            });

            {
                DOUBLE_O<Region> v = new DOUBLE_O<Region>()
                {
                    getD = (Region t) => RegionInfo.vFer().get(t)
                };
                GVALUES.REGION.push("PROP_FERTILTIY", Dic.¤¤Fertility, UI.icons().s.sprout, v);
            }
            {
                DOUBLE_O<Region> v = new DOUBLE_O<Region>()
                {
                    getD = (Region t) => RegionInfo.vArea().get(t)
                };
                GVALUES.REGION.push("PROP_AREA", Dic.¤¤Area, UI.icons().s.expand, v);
            }
            foreach (TERRAIN t in TERRAINS.ALL())
            {
                DOUBLE_O<Region> v = new DOUBLE_O<Region>()
                {
                    getD = (Region r) => RegionInfo.vTerrain(t).get(r)
                };
                GVALUES.REGION.push("PROP_TERRAIN_" + t.key, t.name, t.icon(), v);
            }

            {
                DOUBLE_O<Region> v = new DOUBLE_O<Region>()
                {
                    getD = (Region t) => Math.Max(RegionInfo.vTerrain(TERRAINS.WET()).get(t), RegionInfo.vTerrain(TERRAINS.OCEAN()).get(t))
                };

                GVALUES.REGION.push("PROP_TERRAIN_WATER", Dic.¤¤Water, UI.icons().s.drop, v);
            }
        }

        public static void faction()
        {
            GValueCat<Faction> V = GVALUES.FACTION;

            foreach (Boostable b in BOOSTING.ALL())
            {
                V.push("BOOST_" + b.key, b.name, b.icon, new DOUBLE_O<Faction>()
                {
                    getD = (Faction t) => b.get(t)
                }, false);
            }

            V.push("HAS_BOOST_PERM", "Has Boost", UI.icons().s.question, new BOOLEANO<Faction>()
            {
                isValue = (Faction t) => GAME.BOOST().factions.any(t)
            });

            V.push("IS_PLAYER", "Is Player", UI.icons().s.question, new BOOLEANO<Faction>()
            {
                isValue = (Faction t) => t == FACTIONS.player()
            });

            dip(DIP.ALLY());
            dip(DIP.NEUTRAL());
            dip(DIP.OVERLORD());
            dip(DIP.PACT());
            dip(DIP.TRADE());
            dip(DIP.VASSAL());
            dip(DIP.WAR());

            V.push("STACE_IS_TRADING", "Trading", UI.icons().s.question, new BOOLEANO<Faction>()
            {
                isValue = (Faction t) =>
                {
                    if (t is FactionNPC)
                    {
                        return DIP.get((FactionNPC)t).trades;
                    }
                    return false;
                }
            });

            V.push("STACE_IS_ALLY", "Trading", UI.icons().s.question, new BOOLEANO<Faction>()
            {
                isValue = (Faction t) =>
                {
                    if (t is FactionNPC)
                    {
                        return DIP.get((FactionNPC)t).ally;
                    }
                    return false;
                }
            });

            V.push("OPINION_ABS", "Opinion", UI.icons().s.question, new DOUBLE_O<Faction>()
            {
                getD = (Faction t) =>
                {
                    if (t is FactionNPC)
                    {
                        Royalty roy = ((FactionNPC)t).court().king().roy();
                        return ROPINION.BOOST().get(roy);
                    }
                    return 0;
                }
            });

            V.push("OPINION_REL", "Opinion", UI.icons().s.question, new DOUBLE_O<Faction>()
            {
                getD = (Faction t) =>
                {
                    if (t is FactionNPC)
                    {
                        return ROPINION.get((FactionNPC)t);
                    }
                    return 0;
                }
            });

            foreach (RTYPE t in RTYPE.all)
            {
                V.push("RESOURCE_ALL_PRODUCED_" + t.name(), "resources " + t.name(), UI.icons().s.storage, new INT_O<Faction>()
                {
                    get = (Faction f) =>
                    {
                        int am = Math.Max(f.res().in(t).total().get(), f.res().in(t).total().get(1));
                        am -= Math.Max(f.res().out(t).total().get(), f.res().out(t).total().get(1));
                        return am;
                    },
                    min = (Faction t) => -int.MaxValue,
                    max = (Faction t) => int.MaxValue
                });
            }
        }

        private static void dip(DipStance dip)
        {
            GVALUES.FACTION.push("STANCE_" + dip.key(), dip.name, dip.icon, new BOOLEANO<Faction>()
            {
                isValue = (Faction t) =>
                {
                    if (t is FactionNPC)
                    {
                        return dip.is((FactionNPC)t);
                    }
                    return false;
                }
            });
        }
    }
}