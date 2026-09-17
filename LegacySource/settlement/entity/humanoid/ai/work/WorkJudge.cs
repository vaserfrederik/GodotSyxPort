using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.law.court;
using snake2d.util.rnd;
using static snake2d.util.rnd.RND;

namespace settlement.entity.humanoid.ai.work
{
    internal sealed class WorkJudge : PlanBlueprint
    {
        private readonly ROOM_COURT b = SETT.ROOMS().COURT;

        protected WorkJudge(AIModule_Work module, PlanBlueprint[] map) : base(module, SETT.ROOMS().COURT, map)
        {
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            CourtStation s = b.WorkReserve(Work(a));
            if (s == null)
                return null;
            d.PlanTile.Set(s.CooJudge());
            return Walk.Set(a, d);
        }

        private readonly Resumer walk = new Resumer(Blueprint.Employment().Verb)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                AISubActivation s = AI.SUBS().WalkTo.CooFull(a, d, d.PlanTile);
                if (s == null)
                {
                    Can(a, d);
                    return null;
                }
                return s;
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                return Init.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                CourtStation s = b.ExecutionSpot(d.PlanTile);
                return HasEmployment(a, d) && s != null && s.WorkReserved();
            }

            public override void Can(Humanoid a, AIManager d)
            {
                CourtStation s = b.ExecutionSpot(d.PlanTile);
                if (s != null)
                    s.WorkCancel();
            }
        };

        private readonly Resumer init = new Resumer(Blueprint.Employment().Verb)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                CourtStation s = b.ExecutionSpot(d.PlanTile);
                a.Speed.SetDirCurrent(s.JundgeDir());
                d.PlanByte1 = 20;
                s.WorkUse();
                return AI.SUBS().STAND.ActivateTime(a, d, 1 + rInt(5));
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                CourtStation s = b.ExecutionSpot(d.PlanTile);

                if (s == null || d.PlanByte1-- <= 0 || !s.WorkReserved())
                {
                    Can(a, d);
                    return null;
                }

                if (OneIn(5))
                {
                    return AI.SUBS().Single.Activate(a, d, AI.STATES().Anima.Fist.Activate(a, d));
                }
                return AI.SUBS().STAND.ActivateTime(a, d, 1 + rInt(5));
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
                walk.Can(a, d);
            }
        };
    }
}