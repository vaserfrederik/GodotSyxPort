using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.misc.util;
using snake2d.util.rnd;
using util.text;
using System;

namespace settlement.entity.humanoid.ai.subject
{
    final class ActivityMourn : AIPLAN.PLANRES
    {
        private static string ¤¤verb = "Mourning old friend";

        static ActivityMourn()
        {
            D.ts(typeof(ActivityMourn));
        }

        public ActivityMourn() : base("SUBJECT_MOURN")
        {
            // TODO Auto-generated constructor stub
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return start.set(a, d);
        }

        private readonly Resumer start = new Resumer(¤¤verb)
        {
            public AISubActivation setAction(Humanoid a, AIManager d)
            {
                AISubActivation s = AI.SUBS().walkTo.service(a, d, ROOMS().graveServiceSpots, 500);
                return s;
            }

            public AISubActivation res(Humanoid a, AIManager d)
            {
                return mourn.set(a, d);
            }

            public bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public void can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer mourn = new Resumer(¤¤verb)
        {
            public AISubActivation setAction(Humanoid a, AIManager d)
            {
                d.planByte1 = (byte)(4 + RND.rInt(8));
                return AI.SUBS().STAND.activate(a, d);
            }

            public AISubActivation res(Humanoid a, AIManager d)
            {
                d.planByte1--;
                if (d.planByte1 >= 0)
                {
                    return AI.SUBS().STAND.activateRndDir(a, d);
                }

                FSERVICE s = ROOMS().graveServiceSpots.get(d.path.destX(), d.path.destY());
                if (s != null)
                    s.consume();
                return null;
            }

            public bool con(Humanoid a, AIManager d)
            {
                return ROOMS().graveServiceSpots.get(d.path.destX(), d.path.destY()) != null;
            }

            public void can(Humanoid a, AIManager d)
            {
                FSERVICE s = ROOMS().graveServiceSpots.get(d.path.destX(), d.path.destY());
                if (s != null)
                    s.findableReserveCancel();
            }
        };
    }
}