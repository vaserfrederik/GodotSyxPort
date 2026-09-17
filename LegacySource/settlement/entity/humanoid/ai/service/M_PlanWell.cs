using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.misc.util;
using settlement.room.service.hygine.well;
using settlement.stats;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.service
{
    final class M_PlanWell : MPlan<ROOM_WELL>
    {
        public M_PlanWell() : base("Well", SETT.ROOMS().WELLS, true)
        {
        }

        protected override AISubActivation arrive(Humanoid a, AIManager d)
        {
            return first.set(a, d);
        }

        final Resumer first = new Resumer("1")
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                STATS.POP().NAKED.set(a.indu(), 1);
                blue(d).service().service(d.planTile.x(), d.planTile.y()).startUsing();
                d.planByte1 = (byte)(1 + RND.rInt(8));
                return AI.SUBS().STAND.activate(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (d.planByte1 <= 0)
                {
                    STATS.NEEDS().EXPOSURE.fix(a.indu());
                    STATS.NEEDS().DIRTINESS.set(a.indu(), 0);
                    if (STATS.NEEDS().EXPOSURE.COUNT.indu().get(a.indu()) == 0)
                    {
                        can(a, d);
                        return null;
                    }
                    d.planByte1 = (byte)(1 + RND.rInt(8));
                }

                d.planByte1--;

                if ((d.planByte1 & 1) == 1)
                {
                    return AI.SUBS().STAND.activateTime(a, d, 1 + RND.rInt(5));
                }
                else
                {
                    return AI.SUBS().single.activate(a, d, AI.STATES().anima.box, 1 + RND.rInt(5));
                }
            }

            public override bool con(Humanoid a, AIManager d)
            {
                FSERVICE s = blue(d).service().service(d.planTile.x(), d.planTile.y());
                return s != null && s.findableReservedIs();
            }

            public override void can(Humanoid a, AIManager d)
            {
                FSERVICE s = blue(d).service().service(d.planTile.x(), d.planTile.y());
                if (s != null && s.findableReservedIs())
                    s.consume();
                STATS.POP().NAKED.set(a.indu(), 0);
            }
        };
    }
}