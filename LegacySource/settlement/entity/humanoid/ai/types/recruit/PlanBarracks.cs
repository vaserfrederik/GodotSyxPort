using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.military.training.barracks;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sprite.text;

namespace settlement.entity.humanoid.ai.types.recruit
{
    internal class PlanBarracks : AIPLAN.PLANRES
    {
        private readonly AIModule_Recruit module;

        public PlanBarracks(AIModule_Recruit module) : base("recBarrack")
        {
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
                return null;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer inits = new Res()
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
                return walkLast.set(a, d);
            }
        };

        private readonly Resumer walkLast = new Res()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                COORDINATE man = blue(a).faceCoo(d.planTile.x(), d.planTile.y());
                DIR dir = DIR.get(d.planTile, man);
                AISTATE s = AI.STATES().WALK2.edge(a, d, dir);
                a.speed.setDirCurrent(dir);
                return AI.SUBS().DUMMY.activate(a, d, s);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (job(a, d) == null)
                    return null;
                a.speed.magnitudeTargetSet(0);
                a.speed.magnitudeInit(0);
                return fight.set(a, d);
            }
        };

        private readonly Resumer fight = new Res()
        {
            private readonly AISUB.Simple sub = new AISUB.Simple("Barracksfight")
            {
                protected override AISTATE resume(Humanoid a, AIManager d)
                {
                    d.subByte++;
                    if (job(a, d) == null)
                        return null;
                    if (d.subByte == 1)
                    {
                        return AI.STATES().anima.sword_out.activate(a, d);
                    }
                    else if (d.subByte == 2)
                    {
                        job(a, d).jobSound().rnd(a);
                        return AI.STATES().anima.sword_in.activate(a, d);
                    }

                    return null;
                }
            };

            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                SETT_JOB j = job(a, d);
                j.jobStartPerforming();
                return sub.activate(a, d, AI.STATES().anima.sword.activate(a, d, 5 + RND.rFloat(5)));
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (job(a, d) == null)
                    return null;
                if (!module.planShouldContinue(a, d))
                {
                    can(a, d);
                    return null;
                }

                return sub.activate(a, d, AI.STATES().anima.sword.activate(a, d, 5 + RND.rFloat(5)));
            }
        };

        public override double poll(Humanoid a, AIManager d, HPollData e)
        {
            if (e.type == HPoll.WORKING)
                return 1.0;
            return base.poll(a, d, e);
        }

        private ROOM_BARRACKS blue(Humanoid a)
        {
            RoomInstance w = STATS.WORK().EMPLOYED.get(a);
            if (w != null && w.blueprintI() is ROOM_BARRACKS)
                return (ROOM_BARRACKS)w.blueprintI();
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