using System;
using System.Collections.Generic;
using game.faction;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.main;
using settlement.room.main;
using settlement.room.main.employment;
using settlement.room.main.throne;
using settlement.stats.colls;
using settlement.stats.standing;
using settlement.stats.stat;
using snake2d;
using util.data;
using util.text;

namespace settlement.stats
{
    internal class SValues
    {
        private static readonly CharSequence ¤¤player = "Chosen Race";
        private static readonly CharSequence ¤¤playerIs = "Faction is:";

        private static readonly CharSequence ¤¤oddjobs = "Odd jobs exist";

        static SValues()
        {
            D.ts(typeof(SValues));
        }

        public SValues()
        {
            foreach (STAT s in STATS.all())
            {
                if (s.key() != null && s.indu() != null)
                {
                    GVALUES.INDU.push(s.key() + "_F", s.info().name, s.info().icon == null ? UI.icons().s.question : s.info().icon, s.indu());
                    GVALUES.INDU.pushI(s.key() + "_I", s.info().name, s.info().icon == null ? UI.icons().s.question : s.info().icon, s.indu());
                }
                if (s.key() != null)
                {
                    string k = s.key();
                    if (s.info().isInt())
                    {
                        GVALUES.FACTION.push(k, s.info().name, s.info().icon == null ? UI.icons().s.question : s.info().icon, new DOUBLE_O<Faction>()
                        {
                            public double getD(Faction t) => s.data(HCLASSES.CITIZEN()).getD(null) * s.dataDivider()
                        }, false);
                    }
                    else
                    {
                        GVALUES.FACTION.push(k, s.info().name, s.info().icon == null ? UI.icons().s.question : s.info().icon, new DOUBLE_O<Faction>()
                        {
                            public double getD(Faction t) => s.data(HCLASSES.CITIZEN()).getD(null)
                        }, true);
                    }
                }
            }

            SPRITE[] ri = new SPRITE[RACES.all().size()];
            foreach (Race race in RACES.all())
            {
                ri[race.index] = new SPRITE.Imp(Icon.L)
                {
                    public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) => race.appearance().iconBig.render(r, X1, X2, Y1, Y2)
                };
            }

            GVALUES.INDU.push("RACE_IS_PLAYER", ¤¤player, UI.icons().s.human, new BOOLEANO<Induvidual>()
            {
                public bool is(Induvidual t) => t.race() == FACTIONS.player().race()
            });

            foreach (Race race in RACES.all())
            {
                GVALUES.INDU.push("RACE_" + race.key, race.info.name, ri[race.index], new BOOLEANO<Induvidual>()
                {
                    public bool is(Induvidual t) => t.race() == race
                });
            }

            foreach (HTYPE t in HTYPES.ALL())
            {
                GVALUES.INDU.push("TYPE_" + t.key, t.name, UI.icons().s.human, new BOOLEANO<Induvidual>()
                {
                    public bool is(Induvidual i) => i.hType() == t
                });
            }

            foreach (HCLASS t in HCLASSES.ALL())
            {
                GVALUES.INDU.push("CLASS_" + t.key, t.name, UI.icons().s.human, new BOOLEANO<Induvidual>()
                {
                    public bool is(Induvidual i) => i.clas() == t
                });
            }

            GVALUES.FACTION.push("POPULATION", Dic.¤¤Population, UI.icons().s.human, new DOUBLE_O<Faction>()
            {
                public double getD(Faction o) => POP.tot(null, null)
            }, false);

            GVALUES.FACTION.push("CREDITS", Dic.¤¤Currs, UI.icons().s.money, new DOUBLE_O<Faction>()
            {
                public double getD(Faction o) => (int)o.credits().getD()
            }, false);

            foreach (HCLASS cl in HCLASSES.ALL())
            {
                string k = "POPULATION_" + cl.key;
                GVALUES.FACTION.push(k, cl.names, cl.icon(), new DOUBLE_O<Faction>()
                {
                    public double getD(Faction o) => POP.tot(cl, null)
                }, false);
            }

            GVALUES.FACTION.push("WORKFORCE", Dic.¤¤Employees, UI.icons().s.hammer, new DOUBLE_O<Faction>()
            {
                public double getD(Faction o) => STATS.WORK().workforce()
            }, false);

            foreach (Race r in RACES.all())
            {
                string k = "POPULATION_" + r.key + "_";
                GVALUES.FACTION.push(k + "_F", r.info.names, ri[r.index], new DOUBLE_O<Faction>()
                {
                    public double getD(Faction o)
                    {
                        double div = POP.tot(null, null);
                        return div == 0 ? 0 : POP.tot(null, r) / div;
                    }
                }, true);

                GVALUES.FACTION.push(k + "_I", r.info.names, ri[r.index], new DOUBLE_O<Faction>()
                {
                    public double getD(Faction o) => POP.tot(null, r)
                }, false);

                GVALUES.FACTION.push("FACTION_IS_" + r.key, ¤¤playerIs + " " + r.info.names, ri[r.index], new BOOLEANO<Faction>()
                {
                    public bool is(Faction t) => t.race() == r
                });

                foreach (HCLASS cl in HCLASSES.ALL())
                {
                    if (!cl.player) continue;
                    GVALUES.FACTION.push(k + cl.key + "_F", cl.names + ": " + r.info.names, ri[r.index], new DOUBLE_O<Faction>()
                    {
                        public double getD(Faction o)
                        {
                            double div = POP.tot(null, null);
                            return div == 0 ? 0 : POP.tot(cl, r) / div;
                        }
                    }, true);

                    GVALUES.FACTION.push(k + cl.key + "_I", cl.names + ": " + r.info.names, ri[r.index], new DOUBLE_O<Faction>()
                    {
                        public double getD(Faction o) => POP.tot(cl, r)
                    }, false);
                }

                foreach (HTYPE t in HTYPES.ALL())
                {
                    GVALUES.FACTION.push(k + t.key + "_F", t.name + ": " + r.info.names, ri[r.index], new DOUBLE_O<Faction>()
                    {
                        public double getD(Faction o)
                        {
                            double div = POP.tot(null, null);
                            return div == 0 ? 0 : POP.tot(t, r) / div;
                        }
                    }, true);

                    GVALUES.FACTION.push(k + t.key + "_I", t.name + ": " + r.info.names, ri[r.index], new DOUBLE_O<Faction>()
                    {
                        public double getD(Faction o) => POP.tot(t, r)
                    }, false);
                }
            }

            foreach (StatReligion r in STATS.RELIGION().ALL)
            {
                GVALUES.FACTION.push(STATS.RELIGION().key + "_" + r.religion.key + "_F", r.religion.info.name, r.religion.icon, new DOUBLE_O<Faction>()
                {
                    public double getD(Faction t) => r.followers.data(HCLASSES.CITIZEN()).getD(null)
                }, true);

                GVALUES.FACTION.push(STATS.RELIGION().key + "_" + r.religion.key + "_I", r.religion.info.name, r.religion.icon, new DOUBLE_O<Faction>()
                {
                    public double getD(Faction t) => r.followers.data(HCLASSES.CITIZEN()).getD(null)
                }, false);

                GVALUES.INDU.push(STATS.RELIGION().key + "_" + r.religion.key, r.religion.info.name, r.religion.icon, new DOUBLE_O<Induvidual>()
                {
                    public double getD(Induvidual t) => STATS.RELIGION().getter.get(t) == r ? 1 : 0
                }, false, true);
            }

            foreach (HCLASS cl in HCLASSES.ALL())
            {
                if (cl.player)
                {
                    GVALUES.FACTION.push("LOYALTY_" + cl.key, Dic.¤¤Happiness + ": " + cl.names, UI.icons().s.heart, new DOUBLE_O<Faction>()
                    {
                        public double getD(Faction t) => STANDINGS.get(cl).current()
                    }, true);
                }
            }

            GVALUES.FACTION.push("PLAYER_HAS_ODDJOBS", ¤¤oddjobs, UI.icons().s.hammer, new BOOLEANO<Faction>()
            {
                public bool is(Faction t) => SETT.PATH().finders.job.hasAnyJobs(THRONE.coo().x(), THRONE.coo().y())
            });

            foreach (RoomEmploymentSimple e in SETT.ROOMS().employment.ALLS())
            {
                GVALUES.INDU.push("WORK_" + e.blueprint().key, e.title, e.blueprint().icon, new BOOLEANO<Induvidual>()
                {
                    public bool is(Induvidual i)
                    {
                        RoomInstance ins = STATS.WORK().EMPLOYED.get(i);
                        return ins != null && ins.blueprint().employment() == e;
                    }
                });
            }
        }
    }
}