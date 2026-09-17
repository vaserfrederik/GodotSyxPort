using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Settlement.Entry
{
    public class PeopleSpawner : SAVABLE
    {
        private int rspot = RND.rInt();
        private double time;
        private int ri;
        private int ti;

        private readonly IM[] onTheirWay;
        private readonly IM onTotal;

        public PeopleSpawner()
        {
            IDebugPanelSett.Add("Spawn immigrants", new ACTION()
            {
                public void Exe()
                {
                    Add(FACTIONS.Player().Race(), HTYPES.SUBJECT(), 100);
                }
            });
            onTheirWay = new IM[RACES.All().Count];
            for (int i = 0; i < onTheirWay.Length; i++)
            {
                onTheirWay[i] = new IM();
            }
        }

        public void Save(FilePutter file)
        {
            RACES.Map().Saver().Save(onTheirWay, file);
            onTotal.Save(file);
            file.i(rspot);
            file.i(ri);
            file.i(ti);
            file.d(time);
        }

        public void Load(FileGetter file)
        {
            RACES.Map().Loader().Load(onTheirWay, file);
            onTotal.Load(file);
            rspot = file.i();
            ri = file.i();
            ti = file.i();
            time = file.d();
        }

        public void Clear()
        {
            foreach (IM isItem in onTheirWay)
                isItem.Clear();
            onTotal.Clear();
        }

        public int OnTheirWay(Race race, HTYPE type)
        {
            IM ii = race == null ? onTotal : onTheirWay[race.Index()];
            return ii.Get(type);
        }

        public void Add(Race race, HTYPE type, int amount)
        {
            if (amount > short.MaxValue)
                throw new RuntimeException();
            if (amount < 0)
                return;
            onTotal.Inc(type, amount);
            onTheirWay[race.Index()].Inc(type, amount);
        }

        private void Update(double ds)
        {
            if (onTotal.Tot() == 0)
                return;

            if (SETT.Entry().Points.Reachable().Count == 0)
                return;

            time += ds;
            int rr = RACES.All().Count;
            while (time > 0 && rr-- > 0)
            {
                ri %= RACES.All().Count;
                IM aa = onTheirWay[ri];
                if (aa.Tot() > 0)
                {
                    for (int i = 0; i < HTYPES.All().Count; i++)
                    {
                        ti %= HTYPES.All().Count;
                        HTYPE tt = HTYPES.All()[ti];
                        if (aa.Get(tt) > 0)
                        {
                            COORDINATE c = SETT.Entry().Points.RandomReachable(rspot);
                            if (c == null)
                            {
                                return;
                            }
                            Spawn(c, RACES.All()[ri], HTYPES.All()[ti]);
                            time--;
                            if (time < 0)
                                return;
                        }
                        else
                        {
                            ti++;
                        }
                    }
                }
                rspot = RND.rInt();
                ti = 0;
                ri++;
            }
        }

        private bool Spawn(COORDINATE spot, Race r, HTYPE t)
        {
            DIR d = DIR.Get(SETT.TWIDTH / 2, SETT.THEIGHT / 2, spot.X, spot.Y).Next(2);
            if (!d.IsOrtho())
                d = d.Next((int)(1 * RND.rSign()));

            int tx = spot.X + d.X * RND.rInt(6);
            int ty = spot.Y + d.Y * RND.rInt(6);
            for (int dd = 0; dd <= 6; dd++)
            {
                if (SETT.PATH().Connectivity.Is(tx, ty))
                {
                    Humanoid h = SETT.HUMANOIDS().Create(r, tx, ty, t, CAUSE_ARRIVES.IMMIGRATED());
                    Init(h);

                    if (t == HTYPES.PRISONER())
                    {
                        STATS.LAW().PrisonerType.Set(h.Indu(), CRIMES.WAR());
                    }

                    onTheirWay[r.Index].Inc(t, -1);
                    onTotal.Inc(t, -1);

                    return true;
                }
                tx -= d.X;
                ty -= d.Y;
            }
            return false;
        }

        private void Init(Humanoid h)
        {
            if (h == null || !h.Indu().Clas().Player)
            {
                return;
            }
            STATS.Arrive(h);
        }

        private class IM : SAVABLE, INT_OE<HTYPE>
        {
            private int tot;
            private readonly int[] pam;

            public IM()
            {
                pam = Alloc.ii(HTYPES.All().Count);
            }

            public void Save(FilePutter file)
            {
                HTYPES.Map().Saver().Save(pam, file);
            }

            public void Load(FileGetter file)
            {
                HTYPES.Map().Loader().Load(pam, file, 0);
                tot = 0;
                foreach (int i in pam)
                    tot += i;
            }

            public void Clear()
            {
                tot = 0;
                Array.Fill(pam, 0);
            }

            public int Get(HTYPE t)
            {
                if (t == null)
                    return tot;
                return pam[t.Index()];
            }

            public int Min(HTYPE t)
            {
                return 0;
            }

            public int Max(HTYPE t)
            {
                return int.MaxValue;
            }

            public void Set(HTYPE t, int i)
            {
                tot -= pam[t.Index()];
                pam[t.Index()] = i;
                tot += pam[t.Index()];
            }

            public int Tot()
            {
                return tot;
            }
        }
    }
}