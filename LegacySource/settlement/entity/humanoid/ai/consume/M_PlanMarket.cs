using System;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.service.market;
using settlement.stats.equip;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.consume
{
    final class M_PlanMarket : SPlanAbs<ROOM_MARKET>
    {
        private readonly AIModule_Shop mm;

        public M_PlanMarket(AIModule_Shop m) : base("Market", SETT.ROOMS().MARKET, false)
        {
            this.mm = m;
        }

        protected override AISubActivation arrive(Humanoid a, AIManager d)
        {
            return first.set(a, d);
        }

        final Resumer first = new Resumer("")
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                get(a, d).findableReserveCancel();
                d.planByte1 = (byte)(5 + RND.rInt(5));
                return AI.SUBS().STAND.activateRndDir(a, d, 5);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return shop.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        final Resumer shop = new Resumer("")
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().STAND.activateRndDir(a, d, 5);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                ROOM_MARKET m = blue(d);
                if (m == null || !m.is(a.tc()))
                    return d.resumeOtherPlan(a, mm.ground);

                bool bought = false;

                foreach (WearableResource e in RACES.res().all(a.indu().popCL()))
                {
                    int needed = e.needed(a.indu());
                    if (needed > 0)
                    {
                        int am = m.buy(RACES.res().get(e.resource(a.indu())), needed, a.tc().x(), a.tc().y());
                        if (am > 0)
                        {
                            bought = true;
                            e.wearOut(a.indu());
                            e.inc(a.indu(), am);
                        }
                    }
                }

                d.planByte1--;
                if (bought || d.planByte1 > 0)
                {
                    return walk.set(a, d);
                }
                return d.resumeOtherPlan(a, mm.ground);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        final Resumer walk = new Resumer("")
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                ROOM_MARKET m = blue(d);
                if (m.is(a.tc()) && d.planByte1 > 0)
                {
                    AISubActivation s = AI.SUBS().walkTo.room(a, d, m.getter.get(a.tc()));
                    if (s != null)
                    {
                        return s;
                    }
                }
                return d.resumeOtherPlan(a, mm.ground);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return shop.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };
    }
}