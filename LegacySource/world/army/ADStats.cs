using System;
using System.Collections.Generic;
using game.boosting;
using game.faction;
using game.time;
using init.sprite.UI;
using init.value;
using settlement.stats;
using snake2d.util.misc;
using util.data;
using util.info;
using util.text;
using world.army.ADInit;
using world.battle;
using world.entity.army;
using world.map.regions;

namespace world.army
{
    public sealed class ADStats
    {
        private static readonly string ¤¤Wins = "Victories";
        private static readonly string ¤¤Defeats = "Defeats";
        private static readonly string ¤¤kills = "Enemies Killed";
        private static readonly string ¤¤losses = "Casualties";
        private static readonly string ¤¤SiegeWon = "Sieges Won";
        private static readonly string ¤¤WinsD = "Total amount of victories.";
        private static readonly string ¤¤DefeatsD = "Total amount of defeats.";
        private static readonly string ¤¤killsD = "Total amount of enemies killed.";
        private static readonly string ¤¤lossesD = "Total amount of casualties sustained.";
        private static readonly string ¤¤SiegeWonD = "Total amount of sieges won.";
        private static readonly string ¤¤reputation = "Reputation";
        private static readonly string ¤¤reputationD = "Based on previous and recent victories and defeats. Affects morale on the battlefield.";
        private static readonly string ¤¤mercy = "Mercy";
        private static readonly string ¤¤cruelty = "Cruelty";

        static ADStats()
        {
            D.ts(typeof(ADStats));
        }

        public readonly ADStat wins;
        public readonly ADStat defeats;
        public readonly ADStat kills;
        public readonly ADStat losses;
        public readonly ADStat siegeWon;

        private readonly DOUBLE_OE<WArmy> scoreA;
        private readonly DOUBLE_OE<Faction> scoreF;
        private readonly DOUBLE_OE<Faction> mercy;
        private readonly DOUBLE_O<Faction> cruelty;

        public ADStats(ADInit init)
        {
            scoreA = init.dataA.new DataDouble("STATS_MORALE", new INFO(¤¤reputation, ¤¤reputationD));
            scoreF = init.dataT.new DataDouble("STATS_MORALE", new INFO(¤¤reputation, ¤¤reputationD));
            mercy = init.dataT.new DataDouble("BATTLE_MERCY", new INFO(¤¤mercy, ¤¤mercy))
            {
                public DOUBLE_OE<Faction> setD(Faction t, double d)
                {
                    d = CLAMP.d(d, -1, 1);
                    return base.setD(t, d);
                }
            };

            GVALUES.FACTION.push("BATTLES_MERCY", mercy.info().name, UI.icons().s.heart, mercy);

            cruelty = new DOUBLE_O<Faction>()
            {
                INFO info = new INFO(¤¤cruelty, ¤¤cruelty);

                public double getD(Faction t)
                {
                    return CLAMP.d(-mercy.getD(t), 0, 1);
                }

                public INFO info()
                {
                    return info;
                }
            };

            GVALUES.FACTION.push("BATTLES_REPUTATION", ¤¤reputation, UI.icons().s.arrowUp, scoreF);

            wins = new ADStat(init, "BATTLES_WON", ¤¤Wins, ¤¤WinsD);
            defeats = new ADStat(init, "BATTLES_LOST", ¤¤Defeats, ¤¤DefeatsD);
            kills = new ADStat(init, "BATTLES_ENEMIES_KILLED", ¤¤kills, ¤¤killsD);
            losses = new ADStat(init, "BATTLES_CASUALTIES", ¤¤losses, ¤¤lossesD);
            siegeWon = new ADStat(init, "BATTLES_SIEGES_WON", ¤¤SiegeWon, ¤¤SiegeWonD);

            AD.moraleFactors().add(new BoosterAbs<WArmy>(new BSourceInfo(¤¤reputation, UI.icons().s.crown), false)
            {
                public double to()
                {
                    return 1;
                }

                protected double pget(WArmy o)
                {
                    return rep().getD(o);
                }

                public double from()
                {
                    return 0;
                }

                public double getValue(double input)
                {
                    return input;
                }
            });

            AD.moraleFactors().add(new BoosterAbs<WArmy>(new BSourceInfo(¤¤reputation, Dic.¤¤global, UI.icons().s.crown), false)
            {
                public double to()
                {
                    return 1;
                }

                protected double pget(WArmy o)
                {
                    return repF().getD(o.faction());
                }

                public double getValue(double input)
                {
                    return input;
                }

                public double from()
                {
                    return 0;
                }
            });

            init.updaters.add(new Updater()
            {
                public void update(Faction f, double ds)
                {
                    double d = scoreF.getD(f);
                    if (d < 0)
                    {
                        d += ds / (TIME.secondsPerDay() * 10.0);
                        d = CLAMP.d(d, d, 0);
                    }
                    else if (d > 1)
                    {
                        d -= ds / (TIME.secondsPerDay() * 20);
                        d = CLAMP.d(d, 0, d);
                    }
                    scoreF.setD(f, d);
                }

                public void update(WArmy a, double ds)
                {
                    double d = scoreA.getD(a);
                    if (d < 0)
                    {
                        d += ds / (TIME.secondsPerDay() * 5.0);
                        d = CLAMP.d(d, d, 0);
                    }
                    else if (d > 1)
                    {
                        d -= ds / (TIME.secondsPerDay() * 10);
                        d = CLAMP.d(d, 0, d);
                    }
                    scoreA.setD(a, d);
                }
            });

            new BattleListener()
            {
                public void siege(Faction attacker, Region reg)
                {
                    siegeWon.f.inc(attacker, 1);
                }

                public void siege(WArmy attacker, Region reg)
                {
                    siegeWon.a.inc(attacker, 1);
                }

                public void battle(WArmy a, bool victory, int losses, int kills, Faction againsts)
                {
                    ADStats.this.kills.a.inc(a, kills);
                    ADStats.this.losses.a.inc(a, losses);
                    if (victory)
                    {
                        double d = 0.25 * (double)kills / (AD.men(null).get(a) + 1);
                        double s = scoreA.getD(a);
                        s += d;
                        s = CLAMP.d(s, 0, 1);
                        scoreA.setD(a, s);
                        if (kills > 0)
                            ADStats.this.wins.a.inc(a, 1);
                    }
                    else
                    {
                        double d = (double)losses / (AD.men(null).get(a) + 1);
                        double s = scoreA.getD(a);
                        s -= d;
                        s = CLAMP.d(s, 0, 1);
                        scoreA.setD(a, s);
                        ADStats.this.defeats.a.inc(a, 1);
                    }
                }

                public void battle(Faction a, bool victory, int losses, int kills, Faction againsts)
                {
                    report(a, victory, losses, kills);
                }
            };
        }

        public void report(Faction a, bool victory, int losses, int kills)
        {
            ADStats.this.kills.f.inc(a, kills);
            ADStats.this.losses.f.inc(a, losses);
            double men = 1 + AD.men(null).faction(a) + losses;
            if (a == FACTIONS.player())
                men += STATS.BATTLE().DIV.stat().data().get(null);

            if (victory)
            {
                double d = 0.25 * (double)kills / men;
                double s = scoreF.getD(a);
                s += d;
                s = CLAMP.d(s, -1, 1);
                scoreF.setD(a, s);
                ADStats.this.wins.f.inc(a, 1);
            }
            else
            {
                double d = (double)losses / men;
                double s = scoreF.getD(a);
                s -= d;
                s = CLAMP.d(s, -1, 1);
                scoreF.setD(a, s);
                ADStats.this.defeats.f.inc(a, 1);
            }
        }

        public DOUBLE_OE<WArmy> rep()
        {
            return scoreA;
        }

        public DOUBLE_OE<Faction> repF()
        {
            return scoreF;
        }

        public DOUBLE_OE<Faction> mercy()
        {
            return mercy;
        }

        public DOUBLE_O<Faction> cruelty()
        {
            return cruelty;
        }

        public class ADStat : INFO
        {
            private INT_OE<WArmy> a;
            private INT_OE<Faction> f;

            public ADStat(ADInit init, string key, string name, string desc) : base(name, desc)
            {
                this.a = init.dataA.new DataInt(key, null, 10000);
                f = init.dataT.new DataInt(key);
                GVALUES.FACTION.pushI(key, name, UI.icons().s.sword, f);
            }

            public INT_O<WArmy> a()
            {
                return a;
            }

            public INT_O<Faction> f()
            {
                return f;
            }
        }
    }
}