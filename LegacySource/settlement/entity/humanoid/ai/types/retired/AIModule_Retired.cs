using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Entity.Humanoid.AI.Types.Retired
{
    public sealed class AIModule_Retired : AIModule
    {
        private readonly Plan plan = new Plan(this);

        private static readonly string ¤¤name = "Retire";
        private static readonly string ¤¤desc = "Spend time at a retirement facility.";

        static AIModule_Retired()
        {
            D.ts(typeof(AIModule_Retired));
        }

        public AIModule_Retired()
            : base(UI.icons().s.clock, ¤¤name, ¤¤desc)
        {
        }

        public override AiPlanActivation GetPlan(Humanoid a, AIManager d)
        {
            foreach (ROOM_RESTHOME h in a.Race().Pref().RestHomes)
            {
                if (h.Emp.Employ(a))
                {
                    AI.Modules().Work.SwapInstance(a);
                    return plan.Activate(a, d);
                }
            }

            return null;
        }

        protected override void Init(Humanoid a, AIManager d, HTYPE prev, HTYPE current)
        {
            foreach (ROOM_RESTHOME h in a.Race().Pref().RestHomes)
            {
                if (h.Emp.Employ(a))
                    return;
            }
        }

        public override int GetPriority(Humanoid a, AIManager d)
        {
            if (STATS.WORK().Employed.Get(a) == null)
            {
                foreach (ROOM_RESTHOME h in a.Race().Pref().RestHomes)
                {
                    if (h.Emp.Employable() > 0)
                        return 4;
                }
                return 0;
            }

            return STATS.WORK().WorkTime.Indu().GetD(a.Indu()) < 1 ? 4 : 0;
        }

        protected override void Update(Humanoid a, AIManager d, bool newDay, int byteDelta, int updateOfDay)
        {
        }

        private sealed class Plan : AIPLAN.PLANRES
        {
            private readonly AIModule_Retired module;

            public Plan(AIModule_Retired module)
                : base("Retired")
            {
                this.module = module;
            }

            protected override AISubActivation Init(Humanoid a, AIManager d)
            {
                d.PlanByte1 = (byte)((ROOM_RESTHOME)STATS.WORK().Employed.Get(a).Blueprint()).TypeIndex();
                return walk.Set(a, d);
            }

            private ROOM_RESTHOME Blue(AIManager d)
            {
                return SETT.ROOMS().RestHomes.Get(d.PlanByte1);
            }

            private JOBMANAGER_HASER Jobs(Humanoid a)
            {
                RoomInstance ins = STATS.WORK().Employed.Get(a);
                if (ins == null)
                    return null;
                if (ins.BlueprintI() is ROOM_RESTHOME)
                {
                    return (JOBMANAGER_HASER)ins;
                }
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

            private readonly Resumer walk = new Resumer
            {
                SetAction = (a, d) =>
                {
                    JOBMANAGER_HASER jobs = Jobs(a);
                    if (jobs == null)
                        return null;

                    SETT_JOB j = jobs.GetWork().GetReservableJob(a.Tc());
                    if (j == null)
                        return null;

                    j.JobReserve(null);
                    d.PlanTile.Set(j.JobCoo());
                    AISubActivation s = null;
                    if (SETT.PATH().Solidity.Is(d.PlanTile))
                    {
                        s = AI.SUBS().WalkTo.Coo(a, d, d.PlanTile);
                    }
                    else
                        s = AI.SUBS().WalkTo.CooFull(a, d, d.PlanTile);
                    if (s == null)
                    {
                        Cancel(a, d);
                    }
                    return s;
                },
                Res = (a, d) =>
                {
                    if (!Conn(a, d))
                    {
                        Cancel(a, d);
                        return null;
                    }
                    return work.Set(a, d);
                },
                Con = (a, d) => true,
                Can = (a, d) => { }
            };

            private readonly Resumer work = new Resumer
            {
                SetAction = (a, d) =>
                {
                    d.PlanByte2 = (byte)(10 + RND.rInt(20));
                    job(a, d).JobStartPerforming();
                    return Res(a, d);
                },
                Res = (a, d) =>
                {
                    if (!Conn(a, d))
                    {
                        Cancel(a, d);
                        return null;
                    }

                    d.PlanByte2--;
                    if (d.PlanByte2 <= 0)
                    {
                        Cancel(a, d);
                        return walk.Set(a, d);
                    }

                    if (Blue(d).Cards(d.PlanTile))
                    {
                        if (RND.OneIn(8))
                            return AI.SUBS().DUMMY.Activate(a, d, AI.STATES().Anima.Fist.Activate(a, d, 1 + RND.rFloat() * 2));
                        return AI.SUBS().STAND.ActivateTime(a, d, 6 + RND.rInt(3));
                    }
                    else if (Blue(d).Dance(d.PlanTile))
                    {
                        if (RND.OneIn(5))
                            return AI.SUBS().STAND.ActivateRndDir(a, d, 5 + RND.rInt(3));
                        a.Speed.SetDirCurrent(a.Speed.Dir().Next(-1 + RND.rInt(3)));
                        return AI.SUBS().DUMMY.Activate(a, d, AI.STATES().AnimaArr.Dance().Activate(a, d, 1 + RND.rFloat() * 10));
                    }
                    else if (Blue(d).SitDir(d.PlanTile) != null)
                    {
                        a.Speed.SetDirCurrent(Blue(d).SitDir(d.PlanTile));
                        if (RND.OneIn(5))
                        {
                            return AI.SUBS().DUMMY.Activate(a, d, AI.STATES().AnimaArr.Speak().Activate(a, d, 2 + RND.rFloat() * 5));
                        }

                        return AI.SUBS().STAND.ActivateTime(a, d, 5);
                    }

                    int dx = a.Tc().X + a.Speed.Dir().X;
                    int dy = a.Tc().Y + a.Speed.Dir().Y;
                    if (SETT.ENTITIES().HasAtTile(dx, dy))
                    {
                        if (RND.rBoolean())
                            return AI.SUBS().DUMMY.Activate(a, d, AI.STATES().AnimaArr.Speak().Activate(a, d, 2 + RND.rFloat() * 5));
                        return AI.SUBS().STAND.ActivateTime(a, d, 5);
                    }
                    else if (RND.rBoolean())
                    {
                        a.Speed.SetDirCurrent(a.Speed.Dir().Next(-1 + RND.rInt(3)));
                    }
                    return AI.SUBS().STAND.ActivateTime(a, d, 5);

                },
                Con = (a, d) => true,
                Can = (a, d) => { }
            };

            protected override void Cancel(Humanoid a, AIManager d)
            {
                SETT_JOB j = Job(a, d);
                if (j != null)
                    j.JobReserveCancel(null);
            }

            private bool Conn(Humanoid a, AIManager d)
            {
                return Job(a, d) != null && module.ModuleCanContinue(a, d) && module.GetPriority(a, d) > 0;
            }

            public override double Poll(Humanoid a, AIManager d, HPollData e)
            {
                if (e.Type == HPoll.WORKING)
                    return 1.0;
                return base.Poll(a, d, e);
            }

            protected override void Name(Humanoid a, AIManager d, Str str)
            {
                str.Add(Blue(d).Employment().Verb);
            }
        }
    }
}