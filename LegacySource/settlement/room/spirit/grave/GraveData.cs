using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Spirit.Grave
{
    public class GraveData
    {
        private const double disturbanceRemove = 0.01;

        public interface GRAVE_DATA_HOLDER
        {
            GraveData GraveData();
        }

        public RoomBlueprintIns<?> Blueprint()
        {
            return p;
        }

        public class GraveDataClass
        {
            private readonly double[] burr = new double[RACES.All().Count];
            private readonly double[] fails = new double[RACES.All().Count];
            private readonly HCLASS cl;

            public GraveDataClass(HCLASS cl)
            {
                this.cl = cl;
            }

            public readonly SAVABLE Saver = new SAVABLE
            {
                Save = (FilePutter file) =>
                {
                    file.Ds(burr);
                    file.Ds(fails);
                },
                Load = (FileGetter file) =>
                {
                    file.Ds(burr);
                    file.Ds(fails);
                },
                Clear = () =>
                {
                    Array.Fill(burr, 0);
                    Array.Fill(fails, 0);
                }
            };

            public DOUBLE_O<Race> Burried = new DOUBLE_O<Race>
            {
                GetD = (Race t) =>
                {
                    if (t == null)
                    {
                        double m = 0;
                        foreach (Race r in RACES.All())
                        {
                            m += burr[r.Index];
                        }
                        if (m == 0)
                            return 0;
                        return m;
                    }
                    return burr[t.Index];
                },
                Info = new INFO(¤¤Burried, ¤¤BurriedD)
            };

            public DOUBLE_O<Race> Failed = new DOUBLE_O<Race>
            {
                GetD = (Race t) =>
                {
                    if (t == null)
                    {
                        double m = 0;
                        foreach (Race r in RACES.All())
                        {
                            m += fails[r.Index];
                        }
                        if (m == 0)
                            return 0;
                        return m;
                    }
                    return fails[t.Index];
                },
                Info = new INFO(¤¤Corpses, ¤¤CorpsesD)
            };

            public DOUBLE_O<Race> Value = new DOUBLE_O<Race>
            {
                GetD = (Race t) =>
                {
                    if (t == null)
                    {
                        double m = 0;
                        foreach (Race r in RACES.All())
                        {
                            m += burr[r.Index] * STATS.POP().POP.Data(cl).Get(r);
                        }
                        if (m == 0)
                            return 0;
                        return m / STATS.POP().POP.Data(cl).Get(null);
                    }

                    double tot = burr[t.Index] + fails[t.Index];
                    double res = 1;
                    if (tot != 0)
                        res = 1.0 - fails[t.Index] / tot;
                    res *= 1.0 - 0.5 * blueprint().DegradeAverage();
                    res *= 0.5 + 0.5 * respect.GetD(null);

                    return CLAMP.D(res, 0, 1);
                },
                Info = new INFO(¤¤Value, ¤¤ValueD)
            };

            public void Fail(Corpse c, int delta)
            {
                fails[c.Indu().Race().Index] += delta;
            }

            public void Bury(Corpse c)
            {
                burr[c.Indu().Race().Index]++;
            }

            public void Update()
            {
                foreach (Race r in RACES.All())
                {
                    fails[r.Index] -= Math.Max(fails[r.Index] * 0.10, 0.10);
                    if (fails[r.Index] < 0)
                        fails[r.Index] = 0;

                    burr[r.Index] -= Math.Max(burr[r.Index] * 0.10, 0.10);
                    if (burr[r.Index] < 0)
                        burr[r.Index] = 0;
                }
            }
        }

        public DOUBLE Disturbance = new DOUBLE
        {
            GetD = () =>
            {
                double p = STATS.POP().POP.Data(null).Get(null);
                if (p == 0)
                    return dist > 0 ? 1 : 0;
                return CLAMP.D(100.0 * dist / p, 0, 1);
            },
            Info = new INFO(¤¤Disturbance, ¤¤DisturbanceD)
        };

        public GRAVE_JOB RequestAccessTile()
        {
            if (avai == 0 || p.InstancesSize() == 0)
                return null;

            int i = RND.RInt(p.InstancesSize());

            for (int k = 0; k < p.InstancesSize(); k++)
            {
                int ii = (i + k) % p.InstancesSize();
                GraveInstance ins = (GraveInstance)p.GetInstance(ii);
                if (ins.Active() && ins.Available() > 0)
                {
                    foreach (COORDINATE c in ins.Body())
                    {
                        if (ins.Is(c))
                        {
                            GRAVE_JOB g = grave.Job(c.X, c.Y);
                            if (g != null && g.JobReserveCanBe())
                            {
                                return g;
                            }
                        }
                    }
                    GAME.Notify("nono");
                }
            }
            GAME.Notify("nonono");
            return null;
        }

        public void Update(double ds)
        {
            {
                double d = dist * disturbanceRemove;
                if (d < disturbanceRemove)
                    d = disturbanceRemove;
                dist -= d * ds;
                if (dist < 0)
                    dist = 0;
            }
            upD += ds;
            if (upD > TIME.SecondsPerDay())
            {
                upD -= TIME.SecondsPerDay();
                foreach (HCLASS c in HCLASSES.All())
                {
                    classes[c.Index].Update();
                }
            }
        }

        public StandingDef StandingDef()
        {
            return standingDef;
        }

        public GraveInfo Info(Room r, int i)
        {
            GraveInfo info = GraveInfo.Get((GraveInstance)r, i);
            if (info.HasBody())
                return info;
            return null;
        }
    }
}