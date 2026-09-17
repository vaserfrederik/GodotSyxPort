using System;
using game.battle.div;
using game.boosting;
using game.faction.npc;
using game.faction.player;
using init.sprite;
using init.sprite.UI;
using init.type;
using settlement.stats;
using snake2d.util.misc;
using snake2d.util.sprite;
using util.text;
using world.map.regions;

namespace settlement.stats.law
{
    class StatLawBoosts
    {
        private static string ¤¤popSize = "Population Size";
        private static string ¤¤escapes = "Escapes";
        private static string ¤¤punishment = "Punishment";

        static StatLawBoosts()
        {
            D.ts(typeof(StatLawBoosts));
        }

        public StatLawBoosts(StatsLaw l)
        {
            lawfulness();

            Value v = new Value
            {
                vGet = (HCLASS_RACE t) => l.escapees() / 10.0
            };
            new Boost(v, ¤¤escapes, SPRITES.icons().m.chainsFree, 1, 0.25, true).add(BOOSTABLES.CIVICS().LAW);

            v = new Value
            {
                vGet = (HCLASS_RACE t) => l.guards.data(t.cl).getD(t.race)
            };
            new Boost(v, l.guards.info().name, l.guards.info().icon, 0, 1, true).add(BOOSTABLES.CIVICS().LAW);

            v = new Value
            {
                vGet = (HCLASS_RACE cl) => l.lawMultiplier(cl.cl, cl.race) / 100.0
            };
            new Boost(v, ¤¤punishment, SPRITES.icons().m.law, 0, 100, false).add(BOOSTABLES.CIVICS().LAW);

            v = new Value
            {
                vGet = (HCLASS_RACE t) => 1.0 / 100.0 + BOOSTABLES.CIVICS().LAW.get(t) / 99.0
            };
            new Boost(v, BOOSTABLES.CIVICS().LAW.name, BOOSTABLES.CIVICS().LAW.icon, 0, 100, true).addRet(BOOSTABLES.BEHAVIOUR().LOYALTY).add(BOOSTABLES.BEHAVIOUR().SUBMISSION);

            v = new Value
            {
                vGet = (HCLASS_RACE cl) => STATS.LAW().tyrrany(cl.cl, cl.race)
            };
            new Boost(v, StatsLaw.¤¤tyranny, SPRITES.icons().m.law, 1, 0, true).addRet(BOOSTABLES.BEHAVIOUR().HAPPI).add(BOOSTABLES.BEHAVIOUR().HAPPI_SLAVES);
        }

        private void lawfulness()
        {
            Value v = new Value
            {
                vGet = (HCLASS_RACE t) =>
                {
                    double d = 1000.0 / STATS.POP().POP.data().get(null);
                    d = Math.Pow(d, 1.5);
                    d = 1.0 - d;
                    d = CLAMP.d(d, 0, 1);
                    return d;
                }
            };
            new Boost(v, ¤¤popSize, UI.icons().s.citizen, 10, 0, true).add(BOOSTABLES.BEHAVIOUR().LAWFULNESS);

            v = new Value
            {
                vGet = (HCLASS_RACE t) =>
                {
                    Boostable bo = BOOSTABLES.BEHAVIOUR().HAPPI;

                    if (t.cl == HCLASSES.SLAVE())
                        bo = BOOSTABLES.BEHAVIOUR().HAPPI_SLAVES;

                    double d = bo.get(t);

                    if (d < 1)
                        return d / 100.0;

                    d -= 1;
                    d *= 99;
                    d += 1;

                    return d / 100.0;
                }
            };
            new Boost(v, BOOSTABLES.BEHAVIOUR().HAPPI.name, BOOSTABLES.BEHAVIOUR().HAPPI.icon, 0, 100, true).add(BOOSTABLES.BEHAVIOUR().LAWFULNESS);

            v = new Value
            {
                vGet = (HCLASS_RACE t) => BOOSTABLES.CIVICS().LAW.get(t) * 2.0 / 100.0
            };
            new Boost(v, BOOSTABLES.CIVICS().LAW.name, BOOSTABLES.CIVICS().LAW.icon, 1, 100, true).add(BOOSTABLES.BEHAVIOUR().LAWFULNESS);
        }

        private abstract class Value : BValue
        {
            public abstract double vGet(HCLASS_RACE t);

            public double vGet(Induvidual indu)
            {
                return vGet(indu.popCL());
            }

            public double vGet(Region reg)
            {
                return 0;
            }

            public double vGet(Div div)
            {
                return 0;
            }

            public double vGet(Player f)
            {
                return vGet(HCLASS_RACE.clP());
            }

            public double vGet(FactionNPC f)
            {
                return 0;
            }
        }

        private class Boost : BoosterValue
        {
            public Boost(Value v, string name, SPRITE icon, double from, double to, bool isMul)
                : base(v, new BSourceInfo(name, icon), from, to, isMul)
            {
            }
        }
    }
}