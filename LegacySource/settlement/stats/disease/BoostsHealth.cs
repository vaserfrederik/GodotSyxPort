using System;
using game.battle.div;
using game.boosting;
using game.faction.player;
using init.sprite.UI;
using init.type;
using settlement.stats;
using snake2d.util.misc;
using util.text;

namespace settlement.stats.disease
{
    class BoostsHealth
    {
        private static readonly CharSequence ¤¤entries = "New arrivals";

        static BoostsHealth()
        {
            D.ts(typeof(BoostsHealth));
        }

        public BoostsHealth()
        {
            {
                BSourceInfo s = new BSourceInfo(Dic.¤¤Population, UI.icons().s.human);
                BValue v = new BValue.BValueFaction(BOOSTABLES.PHYSICS().HEALTH)
                {
                    public override double vGet(Player f)
                    {
                        double d = 1.0 - 5.0 / (1 + POP.tot(null, null));
                        d = CLAMP.d(d, 0, 1);
                        return d;
                    }
                };
                new BoosterValue(v, s, 100, 0, true).add(BOOSTABLES.PHYSICS().HEALTH);
            }

            {
                BSourceInfo s = new BSourceInfo(¤¤entries, UI.icons().s.arrow_right);
                BValue v = new BValue.BValueFaction(BOOSTABLES.PHYSICS().HEALTH)
                {
                    public override double vGet(Player f)
                    {
                        return CLAMP.d(STATS.POP().COUNT.newEntries(), 0, 1);
                    }
                };
                new BoosterValue(v, s, 1, 0.5, true).add(BOOSTABLES.PHYSICS().HEALTH);
            }

            {
                BSourceInfo s = new BSourceInfo(HTYPES.CHILD().names, HTYPES.CHILD().icon);
                BValue v = new BValue.BValueInduOnly()
                {
                    public override double vGet(Div div)
                    {
                        return 0;
                    }

                    public override double vGet(Induvidual indu)
                    {
                        if (indu.hType().parent() != indu.hType())
                        {
                            return 1;
                        }
                        return 0;
                    }
                };
                new BoosterValue(v, s, 1, 0.25, true).add(BOOSTABLES.PHYSICS().HEALTH);
            }
        }
    }
}