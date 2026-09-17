using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.misc.job;
using settlement.room.main;
using settlement.stats;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.work
{
    internal sealed class WorkOrator : PlanBlueprint
    {
        private readonly Animation[] anima;

        private WorkOrator(AIModule_Work module, RoomBlueprintIns blueprint, PlanBlueprint[] map, Animation[] anima)
            : base(module, blueprint, map)
        {
            this.anima = anima;
        }

        internal static WorkOrator GetSpeaker(AIModule_Work module, RoomBlueprintIns blueprint, PlanBlueprint[] map)
        {
            var anima = new Animation[]
            {
                AI.STATES().anima.carry,
                AI.STATES().anima.fist,
                AI.STATES().anima.grab,
                AI.STATES().anima.fistRight,
                AI.STATES().anima.fistRight,
                AI.STATES().anima.fistRight,
            };
            return new WorkOrator(module, blueprint, map, anima);
        }

        internal static WorkOrator GetDancer(AIModule_Work module, RoomBlueprintIns blueprint, PlanBlueprint[] map)
        {
            var anima = new Animation[]
            {
                AI.STATES().anima.carry,
                AI.STATES().anima.fist,
                AI.STATES().anima.grab,
                AI.STATES().anima.fistRight,
                AI.STATES().anima.fistLeft,
                AI.STATES().anima.dance,
                AI.STATES().anima.dance,
                AI.STATES().anima.dance,
                AI.STATES().anima.danceE,
                AI.STATES().anima.danceE,
                AI.STATES().anima.danceE,
            };
            return new WorkOrator(module, blueprint, map, anima);
        }

        internal static WorkOrator GetLecture(AIModule_Work module, RoomBlueprintIns blueprint, PlanBlueprint[] map)
        {
            var anima = new Animation[]
            {
                AI.STATES().anima.box,
                AI.STATES().anima.fist,
                AI.STATES().anima.grab,
                AI.STATES().anima.wave,
            };
            return new WorkOrator(module, blueprint, map, anima);
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            if (!module.ModuleCanContinue(a, d) || !HasEmployment(a, d))
                return null;
            if (STATS.WORK().WORK_TIME.indu().GetD(a.indu()) == 1)
                return null;

            JOB_MANAGER jm = ((JOBMANAGER_HASER)Work(a)).GetWork();
            SETT_JOB j = jm.GetReservableJob(null);
            if (j == null)
            {
                return null;
            }
            j.JobReserve(null);
            d.planTile.Set(j.JobCoo());
            return walkToJob.Set(a, d);
        }

        protected override bool ShouldContinue(Humanoid a, AIManager d)
        {
            return JobIsReservedAndReserve(a, d, null) && base.ShouldContinue(a, d);
        }

        protected override void Cancel(Humanoid a, AIManager d)
        {
            JobCancel(a, d, null);
            base.Cancel(a, d);
        }

        private readonly Resumer walkToJob = new Resumer(blueprint.employment().verb)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                AISubActivation s = AI.SUBS().walkTo.cooFull(a, d, d.planTile);
                if (s == null)
                {
                    Cancel(a, d);
                }
                return s;
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                return work.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer work = new Resumer(blueprint.employment().verb)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                SETT_JOB j = ((JOBMANAGER_HASER)Work(a)).GetWork().GetJob(d.planTile);
                j.JobStartPerforming();
                return Res(a, d);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (STATS.WORK().WORK_TIME.indu().GetD(a.indu()) == 1 || !AI.modules().work.ModuleCanContinue(a, d))
                {
                    SETT_JOB j = ((JOBMANAGER_HASER)Work(a)).GetWork().GetJob(d.planTile);
                    j.JobPerform(a, null, 0);
                    return null;
                }

                if (RND.oneIn(4))
                {
                    a.speed.setDirCurrent(a.speed.dir().next(-1 + RND.rInt(3)));
                }

                if (RND.oneIn(2))
                {
                    return AI.SUBS().single.activate(a, d, anima[RND.rInt(anima.Length)], 2 + RND.rInt(3));
                }
                else
                {
                    return AI.SUBS().STAND.activateTime(a, d, 3 + RND.rInt(4));
                }
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
            }
        };
    }
}