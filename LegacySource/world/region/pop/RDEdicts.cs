using System;
using System.Collections.Generic;
using game.boosting;
using game.faction;
using game.time;
using init.sprite.UI;
using init.type;
using settlement.stats;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.data.INT_O;
using util.info;
using util.text;
using world.map.regions;
using world.region;
using static util.text.D;
using static util.text.Dic;

namespace world.region.pop
{
    public class RDEdicts
    {
        private static string ¤¤Distant = "¤Distant";

        private static string ¤¤Prosecute = "¤Persecution";
        private static string ¤¤ProsecuteD = "¤Persecuting a species severely diminishes growth and decreases happiness.";

        private static string ¤¤Exile = "¤Exile";
        private static string ¤¤ExileD = "¤Forbid this species from immigrating and sends off any citizens to neighbouring regions where they are still welcome.";

        private static string ¤¤Massacre = "¤Massacre";
        private static string ¤¤MassacreD = "¤Commit genocide and instantly rid yourself of this species. Will cause an outrage of course, make sure you have enough military presence to handle an eventual uprising.";

        private static double dtime = 1.0 / (TIME.secondsPerDay() * 2 * 16);

        static RDEdicts()
        {
            ts(typeof(RDEdicts));
        }

        public readonly LIST<RDRaceEdict> all;
        public readonly RDRaceEdict sanction;
        public readonly RDRaceEdict exile;
        public readonly RDRaceEdict massacre;

        public RDEdicts(LIST<RDRace> races, RDInit init)
        {
            sanction = new RDRaceEdict("SANCTION", init, new INFO(¤¤Prosecute, ¤¤ProsecuteD), UI.icons().m.descrimination, races, 0.25, 0.5);
            exile = new RDRaceEdict("EXILE", init, new INFO(¤¤Exile, ¤¤ExileD), UI.icons().m.exit, races, 1.0, 0.6);
            massacre = new RDRaceEdict("MASSACRE", init, new INFO(¤¤Massacre, ¤¤MassacreD), UI.icons().m.skull, races, 1.0, 1.0);
            this.all = new ArrayList<RDRaceEdict>(sanction, exile, massacre);
            foreach (RDRace r in races)
                init.upers.add(new Up(r));

            new RD.RDOwnerChanger()
            {
                public void change(Region reg, Faction oldOwner, Faction newOwner)
                {
                    if (newOwner == FACTIONS.player())
                    {
                        foreach (RDRace r in RD.RACES().all)
                        {
                            foreach (RDRaceEdict e in all)
                                e.toggled(r).set(reg, 0);
                        }
                    }
                }
            };

            foreach (RDRace r in races)
            {
                BSourceInfo ss = new BSourceInfo(STATS.MULTIPLIERS().PROSECUTION.name + " (" + Dic.¤¤Capitol + ")", UI.icons().m.descrimination);
                new RBooster(ss, 1, 0.25, true)
                {
                    public double get(Region t)
                    {
                        return STATS.MULTIPLIERS().PROSECUTION.value(HCLASSES.CITIZEN(), r.race, 0);
                    }
                }.add(r.loyalty.target);
            }
        }

        private class Up : RDUpdatable
        {
            private readonly RDRace race;

            public Up(RDRace r)
            {
                this.race = r;
            }

            public void update(Region reg, double ds)
            {
                if (reg.faction() != null && reg.capitol())
                {
                    foreach (RDRaceEdict e in all)
                    {
                        int am = 0;
                        for (int ri = 0; ri < reg.faction().realm().regions(); ri++)
                        {
                            Region r = reg.faction().realm().region(ri);
                            am += e.toggled(race).get(r);
                        }

                        if (am > 0)
                        {
                            e.realm(race).incFraction(reg.faction(), am * 0.5 * ds * TIME.secondsPerDayI() * e.realm(race).max(null));
                        }
                        else
                        {
                            e.realm(race).incFraction(reg.faction(), -ds * dtime * e.realm(race).max(null));
                        }
                    }
                }
            }

            public void init(Region reg)
            {
                if (reg.faction() == FACTIONS.player())
                {
                    foreach (RDRaceEdict e in all)
                    {
                        e.toggled(race).set(reg, 0);
                        e.realm(race).setD(reg.faction(), 0);
                    }
                }
                else if (reg.faction() != null && reg.capitol())
                {
                    foreach (RDRaceEdict e in all)
                    {
                        e.realm(race).setD(reg.faction(), 0);
                        for (int ri = 0; ri < reg.faction().realm().regions(); ri++)
                        {
                            Region r = reg.faction().realm().region(ri);
                            if (e.toggled(race).get(r) == 1)
                            {
                                e.realm(race).setD(reg.faction(), 1.0);
                            }
                        }
                    }
                }
            }
        }

        public class RDRaceEdict
        {
            public readonly LIST<INT_OE<Region>> toggled;
            public readonly LIST<INT_OE<Faction>> realm;
            public readonly INFO info;
            public readonly SPRITE icon;
            public readonly BoostSpecs boosts;

            private RDRaceEdict(string key, RDInit init, INFO info, SPRITE icon, LIST<RDRace> races, double loyalty, double growth)
            {
                this.info = info;

                ArrayList<INT_OE<Region>> toggleds = new ArrayList<INT_OE<Region>>(races.size());
                ArrayList<INT_OE<Faction>> realms = new ArrayList<INT_OE<Faction>>(races.size());

                boosts = new BoostSpecs(info.name, icon, true);

                foreach (RDRace r in races)
                {
                    INT_OE<Region> toggled = init.count.new DataBit(key + "_RACE_TOGGLED" + r.race.key);
                    INT_OE<Faction> realm = init.rCount.new DataByte(key + "_RACE_REALM" + r.race.key);

                    boosts.push(new RBooster(new BSourceInfo(info.name, icon), 1, 1.0 - loyalty, true)
                    {
                        public double get(Region t)
                        {
                            return toggled.get(t);
                        }
                    }, r.loyalty.target);

                    boosts.push(new BoosterImp(new BSourceInfo(¤¤Distant + ": " + info.name, icon), 1, 1.0 - loyalty, true)
                    {
                        public double vGet(Region t)
                        {
                            if (t.faction() != null && realm.get(t.faction()) > 0)
                                return CLAMP.d(realm.getD(t.faction()), 0, 1);
                            return 0;
                        }

                        public double vGet(Faction f)
                        {
                            return CLAMP.d(realm.getD(f), 0, 1);
                        }
                    }, r.loyalty.target);

                    boosts.push(new RBooster(new BSourceInfo(info.name, icon), 1, 1.0 - growth, true)
                    {
                        public double get(Region t)
                        {
                            return toggled.get(t);
                        }
                    }, r.pop.dtarget);

                    toggleds.add(toggled);
                    realms.add(realm);
                }

                this.toggled = toggleds;
                this.realm = realms;

                this.icon = icon;
            }

            public INT_OE<Region> toggled(RDRace r)
            {
                return toggled.get(r.index());
            }

            public INT_OE<Faction> realm(RDRace r)
            {
                return realm.get(r.index());
            }
        }
    }
}