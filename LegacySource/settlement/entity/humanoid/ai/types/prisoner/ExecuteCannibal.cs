using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.food.cannibal;
using snake2d.util;
using util.text;

namespace settlement.entity.humanoid.ai.types.prisoner
{
    class ExecuteCannibal : AIPLAN.PLANRES
    {
        private static string ¤¤verb = "Report to the cannibal for immediate pardon.";
        static
        {
            D.ts(typeof(ExecuteCannibal));
        }

        private readonly ROOM_CANNIBAL b = SETT.ROOMS().CANNIBAL;

        public ExecuteCannibal() : base("PUNISH_CANNIBAL")
        {
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            if (!b.punishEnabled().is(a.race()))
                return null;

            Cage c = b.getPrisonerCage();

            if (c == null)
                return null;

            d.planTile.set(c.coo());

            AISubActivation s = AI.SUBS().walkTo.cooFull(a, d, c.coo());
            if (s == null)
                return null;

            b.cage(d.planTile.x(), d.planTile.y()).prisonerReserve();
            walk.set(a, d);
            return s;
        }

        private readonly Resumer walk = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                d.planByte1 = -1;
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                Cage c = b.cage(d.planTile.x(), d.planTile.y());
                if (c == null || !c.prisonerOk())
                {
                    can(a, d);
                    return null;
                }
                if (d.planByte1 == -1)
                {
                    d.planByte1 = (byte)(TIME.days().bitsSinceStart() & 0x0F);
                    c.prisonerArrive();
                }
                else
                {
                    if (MATH.distance(d.planByte1, TIME.days().bitsSinceStart() & 0x0F, 0x0F) >= 2)
                    {
                        can(a, d);
                        return null;
                    }
                }

                return AI.SUBS().STAND.activateRndDir(a, d, 10);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                Cage c = b.cage(d.planTile.x(), d.planTile.y());
                if (c != null)
                    c.prisonerCancel();
            }
        };
    }
}