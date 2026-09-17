using System;
using System.Collections.Generic;

namespace Settlement.Entity.Humanoid.AI.Consume
{
    public class D_PlanDrunk : AIPLAN.PLANRES
    {
        private static string ¤¤drunk = "Intoxicated";
        private static string ¤¤sobering = "Sobering Up";

        static D_PlanDrunk()
        {
            D.ts(typeof(D_PlanDrunk));
        }

        public D_PlanDrunk() : base("serDrunk")
        {
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            return walkWeird.Set(a, d);
        }

        private readonly AISUB walk = new AISUB.Simple("DRUNK")
        {
            protected override AISTATE Resume(Humanoid a, AIManager d)
            {
                if (d.subByte == 0)
                {
                    a.Speed.TurnRandom();
                    AISTATE s = AI.STATES().WALK.Activate(a, d, 4 + RND.rInt(5));
                    a.Speed.MagnitudeTargetSet(0.2f);
                    a.Speed.SetDirCurrent(DIR.ALL.Rnd());
                    d.subByte = 1;
                    return s;
                }
                a.Speed.MagnitudeInit(0);
                a.Speed.MagnitudeTargetSet(0);
                return null;
            }

            public override bool Event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.Event == HEvent.COLLISION_TILE)
                {
                    return true;
                }
                return base.Event(a, d, e);
            }
        };

        private readonly Resumer walkWeird = new Resumer(¤¤drunk)
        {
            public override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                return walk.Activate(a, d);
            }

            public override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (RND.rBoolean())
                {
                    return SetAction(a, d);
                }
                else if (RND.OneIn(4))
                {
                    return sleep.Set(a, d);
                }
                else
                {
                    return drink.Set(a, d);
                }
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
                d.ResourceCarriedSet(null);
            }

            public override bool Event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.Event == HEvent.COLLISION_TILE)
                {
                    return true;
                }
                return base.Event(a, d, e);
            }
        };

        private readonly Resumer drink = new Resumer(¤¤drunk)
        {
            private readonly Animation[] animi = new Animation[]
            {
                AI.STATES().anima.grab,
                AI.STATES().anima.box,
                AI.STATES().anima.fist,
                AI.STATES().anima.work,
            };

            public override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                if (RND.rBoolean())
                    return AI.SUBS().STAND.ActivateTime(a, d, 1 + RND.rInt(5));
                return AI.SUBS().single.Activate(a, d, animi[RND.rInt(animi.Length)], RND.rFloat() * 4);
            }

            public override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (RND.OneIn(8))
                    return sleep.Set(a, d);
                return SetAction(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
                d.ResourceCarriedSet(null);
            }
        };

        private readonly Resumer sleep = new Resumer(¤¤sobering)
        {
            public override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().LAY.ActivateTime(a, d, 20 + RND.rInt(40));
            }

            public override AISubActivation Res(Humanoid a, AIManager d)
            {
                return null;
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
                d.ResourceCarriedSet(null);
            }
        };
    }
}