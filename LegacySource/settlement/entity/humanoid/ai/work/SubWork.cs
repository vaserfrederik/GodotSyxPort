using System;
using System.Text;

namespace Settlement.Entity.Humanoid.AI.Work
{
    public static class SubWork
    {
        private const string sworking = "working";

        public abstract class SubWorkTool : AISUB.Simple
        {
            public SubWorkTool(string key) : base(key) { }

            public override AISubActivation Activate(Humanoid a, AIManager d)
            {
                throw new RuntimeException();
            }

            public AISubActivation Activate(Humanoid a, AIManager d, SETT_JOB j)
            {
                AISubActivation k = base.Activate(a, d, AI.STATES().anima.toolSlam.Activate(a, d));
                int iters = (int)(j.jobPerformTime(a) / (AI.STATES().anima.toolSlam.time + AI.STATES().anima.toolBack.time));
                if (j.jobPerformTime(a) - iters > 0)
                    iters++;
                iters *= 2;
                iters &= ~1;
                if (iters > 0x0FF)
                {
                    //GAME.Notify("bah " + iters + " " + j.jobPerformTime(a) + " " + j);
                    iters = 0x0FF;
                }
                if (iters <= 0)
                {
                    GAME.Notify("bah " + iters + " " + j.jobPerformTime(a) + " " + j.jobCoo());
                    d.subByte = 2;
                }

                d.subByte = (byte)iters;

                return k;
            }

            protected override AISTATE Resume(Humanoid a, AIManager d)
            {
                d.subByte--;

                int s = d.subByte & 0x0FF;
                if (s == 0)
                    return null;
                if ((s & 1) == 1)
                {
                    SETT_JOB j = GetJob(a, d);
                    if (j == null)
                        return null;
                    if (j.jobSound() != null)
                    {
                        if (a == null)
                        {
                            Console.Error.WriteLine("a");
                        }
                        else if (a.physics == null)
                            Console.Error.WriteLine("phy");
                        else if (a.physics.body() == null)
                            Console.Error.WriteLine("2");
                        j.jobSound().Rnd(a);
                    }
                    return AI.STATES().anima.toolBack.Activate(a, d);
                }
                else
                {
                    return AI.STATES().anima.toolSlam.Activate(a, d);
                }
            }

            protected override string Name(Humanoid a, AIManager d)
            {
                if (GetJob(a, d) == null)
                {
                    return sworking;
                }
                return GetJob(a, d).jobName();
            }

            protected abstract SETT_JOB GetJob(Humanoid a, AIManager d);
        }

        public abstract class SubWorkHands : AISUB.Simple
        {
            private readonly Animation ani = AI.STATES().anima.box;

            public SubWorkHands(string key) : base(key) { }

            public override AISubActivation Activate(Humanoid a, AIManager d)
            {
                throw new RuntimeException();
            }

            public AISubActivation Activate(Humanoid a, AIManager d, SETT_JOB j)
            {
                AISubActivation k = base.Activate(a, d, ani.Resume(a, d, 5));
                int iters = (int)Math.Ceiling(j.jobPerformTime(a) / 5.0);
                iters--;
                d.subByte = (byte)iters;
                if (d.subByte < 0)
                    throw new RuntimeException("" + d.subByte + " " + j);
                return k;
            }

            protected override AISTATE Resume(Humanoid a, AIManager d)
            {
                d.subByte--;

                if (d.subByte < 0)
                {
                    return null;
                }
                SETT_JOB j = GetJob(a, d);
                if (j == null)
                    return null;
                if (j.jobSound() != null)
                    j.jobSound().Rnd(a);
                if (d.subByte == 0)
                {
                    double t = j.jobPerformTime(a) % 5;
                    if (t == 0)
                        return ani.Resume(a, d, 5);
                    return ani.Resume(a, d, t);
                }

                return ani.Resume(a, d, 5.0);
            }

            protected override string Name(Humanoid a, AIManager d)
            {
                if (GetJob(a, d) == null)
                {
                    return sworking;
                }
                return GetJob(a, d).jobName();
            }

            protected abstract SETT_JOB GetJob(Humanoid a, AIManager d);
        }

        public abstract class SubWorkThink : AISUB.Simple
        {
            public SubWorkThink(string key) : base(key) { }

            public override AISubActivation Activate(Humanoid a, AIManager d)
            {
                throw new RuntimeException();
            }

            public AISubActivation Activate(Humanoid a, AIManager d, SETT_JOB j)
            {
                AISubActivation k = base.Activate(a, d, Resume(a, d, 5));
                int iters = (int)Math.Ceiling(j.jobPerformTime(a) / 5.0);
                iters--;
                d.subByte = (byte)iters;
                if (d.subByte < 0)
                    throw new RuntimeException("" + d.subByte);
                return k;
            }

            protected override AISTATE Resume(Humanoid a, AIManager d)
            {
                d.subByte--;

                if (d.subByte < 0)
                {
                    return null;
                }
                SETT_JOB j = GetJob(a, d);
                if (j == null)
                    return null;
                if (j.jobSound() != null)
                    j.jobSound().Rnd(a);
                if (d.subByte == 0)
                {
                    double t = j.jobPerformTime(a) % 5;
                    if (t == 0)
                        return Resume(a, d, 5);
                    return Resume(a, d, t);
                }

                return Resume(a, d, 5.0);
            }

            private AISTATE Resume(Humanoid a, AIManager d, double time)
            {
                if (RND.oneIn(8))
                {
                    a.speed.setDirCurrent(a.speed.dir().next(-1 + RND.rInt(3)));
                    return AI.STATES().anima.stand.Activate(a, d, time);
                }
                a.speed.setDirCurrent(DIR.get(a.tc(), GetJob(a, d).jobCoo()));
                if (RND.rBoolean())
                    return AI.STATES().anima.fistRight.Resume(a, d, time);
                else
                    return AI.STATES().anima.stand.Activate(a, d, time);
            }

            protected override string Name(Humanoid a, AIManager d)
            {
                if (GetJob(a, d) == null)
                {
                    return sworking;
                }
                return GetJob(a, d).jobName();
            }

            protected abstract SETT_JOB GetJob(Humanoid a, AIManager d);
        }
    }
}