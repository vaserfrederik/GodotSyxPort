using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement.Entity.Humanoid.AI.Battle
{
    using static Settlement.Main.SETT;

    using Game;
    using Game.Battle.Div;
    using Init.Type;
    using Settlement.Entity.Humanoid;
    using Settlement.Entity.Humanoid.AI.Main;
    using Settlement.Main;
    using Settlement.Stats;
    using Snake2D.Util.Datatypes;
    using Snake2D.Util.Rnd;

    class PlanRout : AIPLAN.PLANRES
    {
        public PlanRout(string key) : base(key)
        {
            // TODO Auto-generated constructor stub
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            return run.Set(a, d);
        }

        private readonly Resumer run = new Resumer("Routing")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                STATS.BATTLE().ROUTING.Indu().Set(a.Indu(), 1);
                Div div = a.Division();
                if (div != null)
                {
                    GAME.ARMIES().Factors.ReportRout(div);
                    int di = RND.RInt(DIR.ALL.Size);
                    for (int i = 0; i < DIR.ALL.Size; i++)
                    {
                        DIR dir = DIR.ALL.Get(di + i);
                        if (!div.Status().Threat(dir) && !div.Status().Threat(dir.Next(1)) && !div.Status().Threat(dir.Next(-1)))
                        {
                            a.SetDivision(null);
                            a.Speed.Turn2(dir);
                            return AI.SUBS().WalkTo.Run_Around_Crazy(a, d, 5);
                        }
                    }
                    a.SetDivision(null);
                }

                a.Speed.Turn90().Turn90();
                a.Speed.TurnWithAngel(RND.RFloat0(20));
                return AI.SUBS().WalkTo.Run_Around_Crazy(a, d, 5);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (RND.OneIn(5))
                    return path.Set(a, d);
                else
                    return surrendered.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {

            }
        };

        private readonly Resumer path = new Resumer("Routing")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                if (PATH().Finders.Entity.FindExitNoEnemies(a, a.Physics.TileC().X(), a.Physics.TileC().Y(), d.Path, int.MaxValue))
                {
                    return AI.SUBS().WalkTo.PathRun(a, d);
                }
                return run.Set(a, d);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                AIManager.Dead = CAUSE_LEAVES.DESERTED();
                return AI.SUBS().STAND.Activate(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {

            }
        };

        private readonly Resumer surrendered = new Resumer("Surrendered")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                d.PlanByte1 = 0;
                return AI.SUBS().LAY.ActivateTime(a, d, 10);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                d.PlanByte1++;
                if (d.PlanByte1 >= 32)
                {
                    AIManager.Dead = CAUSE_LEAVES.DESERTED();
                    return AI.SUBS().LAY.ActivateTime(a, d, 10);
                }

                if (a.Indu().Army() == GAME.ARMIES().Player())
                {
                    if (GAME.ARMIES().Enemy().Men() == 0)
                    {
                        return path.Set(a, d);
                    }
                }
                return AI.SUBS().LAY.ActivateTime(a, d, 10);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {

            }

            public override double Poll(Humanoid a, AIManager d, HPollData e)
            {

                return 0;
            }

            public override bool Event(Humanoid a, AIManager d, HEventData e)
            {
                return false;
            }

        };

        public override bool Event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.Event == HEvent.COLLISION_TILE && !SETT.TILE_BOUNDS.HoldsPoint(e.Tx, e.Ty))
            {
                a.HelloMyNameIsInigoMontoyaYouKilledMyFatherPrepareToDie();
                return false;
            }

            return base.Event(a, d, e);
        }

        public override double Poll(Humanoid a, AIManager d, HPollData e)
        {
            if (e.Type == HPoll.DEFENCE_SKILL)
                return 0;
            return base.Poll(a, d, e);
        }

    }
}