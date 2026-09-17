using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.misc.util;
using settlement.room.service.pleasure;
using settlement.stats;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.service
{
    sealed class M_PlanBrothel : MPlan<ROOM_PLEASURE>
    {
        public M_PlanBrothel() : base("Brothel", SETT.ROOMS().BROTHELS, false)
        {
        }

        protected override AISubActivation arrive(Humanoid a, AIManager d)
        {
            return wait.Set(a, d);
        }

        private readonly Resumer wait = new Resumer("")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                d.planByte2 = (byte)(25 + RND.rInt(10));
                d.planByte1 = 0;
                Get(a, d).StartUsing();
                return Res(a, d);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                d.planByte2--;

                if (Blue(d).ClientShouldUndress(d.planTile.X(), d.planTile.Y()) || d.planByte2 <= 0)
                {
                    STATS.POP().NAKED.Set(a.indu(), 1);
                    if (d.planByte1 == 1)
                    {
                        Blue(d).ClientUndress(d.planTile.X(), d.planTile.Y());
                        return second.Set(a, d);
                    }
                    d.planByte1 = 1;
                }

                return AI.SUBS().STAND.ActivateRndDir(a, d, 3);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                FINDABLE s = Get(a, d);
                return s != null && s.FindableReservedIs();
            }

            public override void Can(Humanoid a, AIManager d)
            {
                STATS.POP().NAKED.Set(a.indu(), 0);
                FSERVICE s = Get(a, d);
                if (s != null && s.FindableReservedIs())
                    s.Consume();
            }
        };

        private readonly Resumer second = new Resumer("")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                d.planByte2 += 5 + RND.rInt(5);
                return Res(a, d);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                d.planByte2--;
                if (d.planByte2 < 0)
                {
                    wait.Can(a, d);
                    return null;
                }

                return AI.SUBS().STAND.ActivateRndDir(a, d, 3);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return wait.Con(a, d);
            }

            public override void Can(Humanoid a, AIManager d)
            {
                wait.Can(a, d);
            }
        };
    }
}