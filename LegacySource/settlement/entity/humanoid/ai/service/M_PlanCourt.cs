using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.misc.util;
using settlement.room.law.court;
using snake2d.util.rnd;
using snake2d.util.sets;

namespace settlement.entity.humanoid.ai.service
{
    final class M_PlanCourt : MPlan<ROOM_COURT>
    {
        public M_PlanCourt() : base("Court", new ArrayList<ROOM_COURT>(SETT.ROOMS().COURT), true)
        {
        }

        protected override AISubActivation arrive(Humanoid a, AIManager d)
        {
            return first.Set(a, d);
        }

        private readonly Resumer first = new Resumer("")
        {
            public AISubActivation SetAction(Humanoid a, AIManager d)
            {
                get(a, d).StartUsing();
                d.planByte1 = (byte)(4 + RND.rInt(8));
                return AI.SUBS().STAND.Activate(a, d);
            }

            public AISubActivation Res(Humanoid a, AIManager d)
            {
                d.planByte1--;
                if (d.planByte1 >= 0)
                {
                    return AI.SUBS().STAND.ActivateRndDir(a, d);
                }

                FSERVICE s = get(a, d);
                if (s != null)
                    s.Consume();
                return null;
            }

            public bool Con(Humanoid a, AIManager d)
            {
                FINDABLE s = get(a, d);
                return s != null && s.FindableReservedIs();
            }

            public void Can(Humanoid a, AIManager d)
            {
                FINDABLE s = get(a, d);
                if (s != null)
                    s.FindableReserveCancel();
            }
        };
    }
}