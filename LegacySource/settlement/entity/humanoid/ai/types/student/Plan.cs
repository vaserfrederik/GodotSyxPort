using System;
using System.Collections.Generic;
using System.Linq;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.types.student;
using settlement.misc.job;
using settlement.room.knowledge.university;
using settlement.room.main;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.types.student
{
    public class Plan : AIPLAN.PLANRES
    {
        private static readonly CharSequence ¤¤study = "¤studying";
        private readonly AIModule_Student module;

        static Plan()
        {
            D.ts(typeof(Plan));
        }

        public Plan(AIModule_Student module) : base("student")
        {
            this.module = module;
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            AI.Modules().Work.SwapInstance(a);
            return Walk.Set(a, d);
        }

        private ROOM_UNIVERSITY Uni(Humanoid a)
        {
            RoomInstance ins = STATS.WORK().EMPLOYED.Get(a);
            if (ins == null)
                return null;
            if (ins.BlueprintI() is ROOM_UNIVERSITY)
                return (ROOM_UNIVERSITY)ins.BlueprintI();
            return null;
        }

        private JOBMANAGER_HASER Jobs(Humanoid a)
        {
            RoomInstance ins = STATS.WORK().EMPLOYED.Get(a);
            if (ins == null)
                return null;
            if (ins.BlueprintI() is ROOM_UNIVERSITY)
                return (JOBMANAGER_HASER)ins;
            return null;
        }

        private SETT_JOB Job(Humanoid a, AIManager d)
        {
            JOBMANAGER_HASER jj = Jobs(a);
            if (jj == null)
                return null;
            SETT_JOB j = jj.GetWork().GetJob(d.PlanTile);
            if (j == null || !j.JobReservedIs(null))
                return null;
            return j;
        }

        public Resumer Walk = new Resumer(¤¤study)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                JOBMANAGER_HASER jobs = Jobs(a);
                if (jobs == null)
                    return null;

                SETT_JOB j = jobs.GetWork().GetReservableJob(a.Tc());

                if (j == null)
                    return null;
                j.JobReserve(null);
                d.PlanTile.Set(j.JobCoo());
                AISubActivation s = AI.SUBS().WalkTo.CooFull(a, d, d.PlanTile);
                if (s == null)
                {
                    Cancel(a, d);
                }
                return s;
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (!Conn(a, d))
                {
                    Cancel(a, d);
                    return null;
                }
                if (!Uni(a).IsLecturer(d.PlanTile))
                    return WalkLast.Set(a, d);
                return Lecture.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
            }
        };

        public Resumer WalkLast = new Resumer(¤¤study)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                DIR dir = Uni(a).SpotDir(d.PlanTile);

                AISTATE s = AI.STATES().Walk2.Edge(a, d, dir);
                a.Speed.SetDirCurrent(dir);
                return AI.SUBS().DUMMY.Activate(a, d, s);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (!Conn(a, d))
                {
                    Cancel(a, d);
                    return null;
                }

                a.Speed.MagnitudeTargetSet(0);
                a.Speed.MagnitudeInit(0);
                return Study.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
            }
        };

        public Resumer Study = new Resumer(¤¤study)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                return Res(a, d);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (!Conn(a, d))
                {
                    Cancel(a, d);
                    return null;
                }

                ROOM_UNIVERSITY u = Uni(a);
                if (!u.IsTime.Is())
                {
                    Cancel(a, d);
                    return null;
                }

                DIR dir = u.SpotDir(d.PlanTile);
                if (RND.OneIn(5))
                {
                    dir = dir.Next(-1 + RND.RInt(3));
                }
                a.Speed.SetDirCurrent(dir);
                return AI.SUBS().STAND.ActivateTime(a, d, 5);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
            }
        };

        public Resumer Lecture = new Resumer(¤¤study)
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                return Res(a, d);
            }

            private readonly Animation[] anima = new Animation[]
            {
                AI.STATES().anima.carry,
                AI.STATES().anima.fist,
                AI.STATES().anima.grab,
                AI.STATES().anima.fistRight,
                AI.STATES().anima.fistRight,
                AI.STATES().anima.fistRight,
            };

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (!Conn(a, d))
                {
                    Cancel(a, d);
                    return null;
                }

                ROOM_UNIVERSITY u = Uni(a);
                if (!u.IsTime.Is())
                {
                    Cancel(a, d);
                    return null;
                }

                if (RND.OneIn(4))
                {
                    a.Speed.SetDirCurrent(a.Speed.Dir().Next(-1 + RND.RInt(3)));
                }

                if (RND.OneIn(2))
                {
                    return AI.SUBS().Single.Activate(a, d, anima[RND.RInt(anima.Length)], 2 + RND.RInt(3));
                }
                else
                {
                    return AI.SUBS().STAND.ActivateTime(a, d, 3 + RND.RInt(4));
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

        protected override void Cancel(Humanoid a, AIManager d)
        {
            SETT_JOB j = Job(a, d);
            if (j != null)
                j.JobReserveCancel(null);
        }

        private bool Conn(Humanoid a, AIManager d)
        {
            return module.ModuleCanContinue(a, d) && AIModule_Student.ShouldContinue(a, d) && Job(a, d) != null;
        }

        public override double Poll(Humanoid a, AIManager d, HPollData e)
        {
            if (e.Type == HPoll.WORKING)
                return 1.0;
            return base.Poll(a, d, e);
        }
    }
}