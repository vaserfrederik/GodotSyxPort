using System;
using settlement.entity.humanoid.ai.types.recruit;
using init.constant;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.military.training.archery;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.sprite.text;

namespace settlement.entity.humanoid.ai.types.recruit
{
    public sealed class PlanRange : AIPLAN.PLANRES
    {
        private readonly AIModule_Recruit module;

        public PlanRange(AIModule_Recruit module)
        {
            base("recRange");
            this.module = module;
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return inits.set(a, d);
        }

        private readonly Resumer done = new Res()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (job(a, d) != null)
                    job(a, d).jobReserveCancel(null);
                return AI.SUBS().STAND.activate(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                a.HTypeSet(HTYPES.SUBJECT(), null, null);
                return d.resumeOtherPlan(a, AI.plans().NOP);
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        public readonly Resumer inits = new Res()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                JOBMANAGER_HASER w = work(a);

                if (w == null)
                    return done.set(a, d);

                SETT_JOB j = w.getWork().getReservableJob(a.tc());

                if (j == null)
                    return done.set(a, d);

                d.planTile.set(j.jobCoo());
                AISubActivation s = AI.SUBS().walkTo.cooFull(a, d, j.jobCoo());

                if (s != null)
                {
                    j = w.getWork().getJob(d.planTile);
                    j.jobReserve(null);
                    return s;
                }

                return done.set(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (job(a, d) == null)
                    return null;
                DIR dir = blue(a).faceCoo(d.planTile.x(), d.planTile.y());
                a.speed.setDirCurrent(dir);
                a.speed.magnitudeTargetSet(0);
                a.speed.magnitudeInit(0);
                return work.set(a, d);
            }
        };

        public override double poll(Humanoid a, AIManager d, HPollData e)
        {
            if (e.type == HPoll.WORKING)
                return 1.0;
            return base.poll(a, d, e);
        }

        public readonly Resumer work = new Res()
        {
            private readonly AISUB sub = new AISUB.Simple("BarracksRange")
            {
                protected override AISTATE resume(Humanoid a, AIManager d)
                {
                    d.subByte++;
                    if (d.subByte == 1)
                        return AI.STATES().STAND.activate(a, d, 10);
                    if (d.subByte == 2)
                        return AI.STATES().anima.archer1.activate(a, d, 3);
                    if (d.subByte == 3)
                    {
                        if (blue(a) != null)
                        {
                            DIR dir = blue(a).faceCoo(d.planTile.x(), d.planTile.y());
                            blue(a).fireArrow(a.tc().x(), a.tc().y(), a.body().cX() + dir.x() * C.TILE_SIZEH, a.body().cY() + dir.y() * C.TILE_SIZEH);
                        }
                        return AI.STATES().anima.archer2.activate(a, d, 3);
                    }
                    return null;
                }
            };

            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (!module.planShouldContinue(a, d))
                {
                    can(a, d);
                    return null;
                }
                return sub.activate(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (job(a, d) == null)
                    return null;
                return set(a, d);
            }
        };

        private ROOM_ARCHERY blue(Humanoid a)
        {
            RoomInstance w = STATS.WORK().EMPLOYED.get(a);
            if (w != null && w.blueprintI() is ROOM_ARCHERY)
                return (ROOM_ARCHERY)w.blueprintI();
            return null;
        }

        private JOBMANAGER_HASER work(Humanoid a)
        {
            if (blue(a) != null)
                return (JOBMANAGER_HASER)STATS.WORK().EMPLOYED.get(a);
            return null;
        }

        private SETT_JOB job(Humanoid a, AIManager d)
        {
            JOBMANAGER_HASER w = work(a);
            if (w != null)
            {
                SETT_JOB j = w.getWork().getJob(d.planTile);
                if (j == null || !j.jobReservedIs(null))
                    return null;

                if (!module.planShouldContinue(a, d))
                {
                    j.jobReserveCancel(null);
                    return null;
                }

                return j;
            }
            return null;
        }

        private abstract class Res : Resumer
        {
            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                SETT_JOB j = job(a, d);
                if (j != null)
                    j.jobReserveCancel(null);
            }

            protected override void name(Humanoid a, AIManager d, Str string)
            {
                JOBMANAGER_HASER bb = work(a);
                if (bb == null)
                    return;
                string.add(STATS.WORK().EMPLOYED.get(a).blueprintI().employment().verb);
            }
        }
    }
}