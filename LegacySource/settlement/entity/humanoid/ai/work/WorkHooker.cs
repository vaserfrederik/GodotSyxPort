using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.misc.job;
using settlement.room.service.pleasure;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.text;

namespace settlement.entity.humanoid.ai.work
{
    internal sealed class WorkHooker : PlanBlueprint
    {
        private readonly ROOM_PLEASURE b;
        private static readonly CharSequence ¤¤waiting = "¤Waiting for business";

        static WorkHooker()
        {
            D.ts(typeof(WorkHooker));
        }

        protected WorkHooker(ROOM_PLEASURE b, AIModule_Work module, PlanBlueprint[] map) : base(module, b, map)
        {
            this.b = b;
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            JOB_MANAGER jm = ((JOBMANAGER_HASER)work(a)).getWork();

            SETT_JOB j = jm.getReservableJob(a.tc());
            if (j == null)
            {
                return null;
            }
            d.planTile.set(j.jobCoo());
            AISubActivation s = walk.set(a, d);
            if (s != null)
            {
                j = jm.getJob(d.planTile);
                j.jobReserve(null);
            }
            return s;
        }

        private readonly Resumer walk = new Resumer(blueprint.employment().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                STATS.WORK().proximityStart(a);
                return AI.SUBS().walkTo.cooFull(a, d, d.planTile);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                STATS.WORK().proximityEnd(a);
                return init.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return hasEmployment(a, d) && jobIsReservedAndReserve(a, d, null);
            }

            public override void can(Humanoid a, AIManager d)
            {
                jobCancel(a, d, null);
            }
        };

        private readonly Resumer init = new Resumer(blueprint.employment().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                a.speed.setDirCurrent(a.speed.dir().perpendicular());
                d.planByte1 = (byte)a.speed.dir().id();
                return AI.SUBS().STAND.activateTime(a, d, 1);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (!module.moduleCanContinue(a, d) || !hasEmployment(a, d) || STATS.WORK().WORK_TIME.indu().getD(a.indu()) == 1)
                {
                    can(a, d);
                    return null;
                }

                if (b.workerReadyShouldUndress(d.planTile.x(), d.planTile.y()))
                {
                    STATS.POP().NAKED.set(a.indu(), 1);
                }
                else
                {
                    STATS.POP().NAKED.set(a.indu(), 0);
                }

                if (RND.oneIn(10))
                {
                    a.speed.setDirCurrent(DIR.ALL.get(d.planByte1).next(-1).next(RND.rInt(3)));
                }

                return AI.SUBS().STAND.activateTime(a, d, 4);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return walk.con(a, d);
            }

            public override void can(Humanoid a, AIManager d)
            {
                SETT_JOB j = jobGet(a, d);
                if (j != null)
                    j.jobPerform(a, null, 0);
                STATS.POP().NAKED.set(a.indu(), 0);
            }

            protected override void name(Humanoid a, AIManager d, Str string)
            {
                if (b.workerReadyShouldUndress(d.planTile.x(), d.planTile.y()))
                    base.name(a, d, string);
                else
                    string.add(¤¤waiting);
            }
        };
    }
}