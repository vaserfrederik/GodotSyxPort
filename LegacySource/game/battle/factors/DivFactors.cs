using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.battle;
using game.battle.div;
using game.battle.thread.status;
using game.boosting;
using game.save;
using game.time;
using init.constant;
using settlement.entity.humanoid;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.updating;

namespace game.battle.factors
{
    public static class DivFactors
    {
        static readonly ArrayListGrower<DivFactor> all = new ArrayListGrower<DivFactor>();
        static DivFactors()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    all.Clear();
                }
            };
        }

        private readonly ArrayListGrower<DataDiv> datas = new ArrayListGrower<DataDiv>();

        readonly DataA supplies = new DataA();

        readonly DataDiv casulties = new DataDiv();
        readonly DataDiv routing = new DataDiv();
        readonly DataDiv projectiles = new DataDiv();
        readonly DataDiv weariness = new DataDiv();
        readonly DataDiv kills = new DataDiv();

        private readonly DataDiv morale = new DataDiv();
        private readonly DataDiv valueFactors = new DataDiv();
        private readonly ArrayListGrower<BoosterAbs<BOOSTABLE_O>> boosters = new ArrayListGrower<BoosterAbs<BOOSTABLE_O>>();
        private readonly double fMax;

        private readonly IUpdater updater = new IUpdater(Config.battle().DIVISIONS_PER_BATTLE, 1.0)
        {
            private double speed = Config.battle().MORALE_HOLDOUT / (120);

            private readonly ArrayListGrower<DataDiv> player = new ArrayListGrower<DataDiv>();
            {
                player.Add(casulties);
                player.Add(routing);
            }

            protected override void Update(int i, double ds)
            {
                Div div = GAME.ARMIES().division((short)i);
                if (div.men() == 0 || (GAME.ARMIES().enemy().men() == 0 && !div.army().player()))
                {
                    foreach (DataDiv d in datas)
                    {
                        if (d != kills)
                            d.SetD(div, 0);
                    }
                }
                else
                {
                    if (GAME.ARMIES().enemy().men() == 0)
                    {
                        {
                            double w = weariness.GetD(div);
                            w -= ds * speed;
                            w = CLAMP.d(w, 0, 10000000);
                            weariness.SetD(div, w);
                            if (weariness.GetD(div) == 0)
                            {
                                kills.SetD(div, 0);
                            }
                        }

                        foreach (DataDiv d in player)
                        {
                            double am = d.GetD(div) - 10.0 * ds * TIME.secondsPerDayI();
                            am = CLAMP.d(am, 0, 10000);
                            d.SetD(div, am);
                        }
                    }
                    else
                    {
                        DivStatus s = div.status();
                        double w = weariness.GetD(div);

                        double cc = (double)s.engagements() / div.men();
                        if (cc > 0)
                        {
                            w += ds * cc * speed;
                        }
                        w = CLAMP.d(w, 0, 10000000);
                        weariness.SetD(div, w);
                    }

                    double d = projectiles.GetD(div);
                    d -= div.men() * ds / 10.0;
                    d = CLAMP.d(d, 0, div.men() * 4);
                    projectiles.SetD(div, d);
                    Set(div, ds);
                }
            }
        };

        public DivFactors(Armies a)
        {
            new Init(this);

            GAME.saver().Add(new Savable("BATTLE_DIV_FACTORS")
            {
                protected override void Save(FilePutter file)
                {
                    foreach (DataDiv d in datas)
                        d.Save(file);
                    supplies.Save(file);
                }

                protected override void Load(FileGetter file)
                {
                    foreach (DataDiv d in datas)
                        d.Load(file);
                    supplies.Load(file);
                }

                protected override void LoadFail()
                {
                    foreach (DataDiv d in datas)
                    {
                        d.Clear();
                    }
                    supplies.Clear();
                }
            });

            foreach (DivFactor f in all)
            {
                foreach (BoostSpec s in f.specs.all())
                {
                    boosters.Add(s.booster);
                }
            }
            fMax = BUtil.max(boosters, typeof(Div), 1);
        }

        private void Set(Div div, double ds)
        {
            morale.SetD(div, BOOSTABLES.BATTLE().MORALE.Get(div) - 1);
            double m = BUtil.value(boosters, div) / fMax;
            valueFactors.SetD(div, m);
        }

        public void Init(Army a, double supplies)
        {
            foreach (DataDiv d in datas)
            {
                d.Clear(a);
            }
            this.supplies.SetD(a, supplies - 0.5);
            foreach (Div div in a.divisions())
            {
                morale.SetD(div, BOOSTABLES.BATTLE().MORALE.Get(div) - 1);
                double m = BUtil.value(boosters, div) / fMax;
                valueFactors.SetD(div, m);
            }
        }

        public void Init(Div div)
        {
            foreach (DataDiv d in datas)
                d.SetD(div, 0);
            morale.SetD(div, BOOSTABLES.BATTLE().MORALE.Get(div) - 1);
            double m = BUtil.value(boosters, div) / fMax;
            valueFactors.SetD(div, m);
        }

        public void Update(double ds)
        {
            updater.Update(ds);
        }

        public double Morale(Div div)
        {
            return morale.GetD(div) + 1;
        }

        public double Morale(Army asa)
        {
            int m = asa.men();
            if (m == 0)
                return 1;
            return morale.army.GetD(asa) / m + 1;
        }

        public double ValueCurrent(Div div)
        {
            return valueFactors.GetD(div);
        }

        public bool ShouldRun(Div div)
        {
            return projectiles.GetD(div) > (div.menNrOf() >> 1);
        }

        public LIST<DivFactor> All()
        {
            return all;
        }

        class DataDiv : DOUBLE_OE<Div>, SAVABLE
        {
            private readonly double[] data = new double[Config.battle().DIVISIONS_PER_BATTLE];
            private readonly long[] dataa = new long[2];

            public DataDiv()
            {
                datas.Add(this);
            }

            public double GetD(Div t)
            {
                return data[t.index()];
            }

            public void Save(FilePutter file)
            {
                file.ds(data);
            }

            public void Load(FileGetter file)
            {
                file.ds(data);
                Array.Fill(dataa, 0L);
                for (int ai = 0; ai < 2; ai++)
                {
                    for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
                    {
                        dataa[ai] += (int)(data[ai * Config.battle().DIVISIONS_PER_ARMY + di] * 100);
                    }
                }
            }

            public void Clear()
            {
                Array.Fill(data, 0);
                Array.Fill(dataa, 0L);
            }

            void Clear(Army a)
            {
                for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
                {
                    data[di + a.index() * Config.battle().DIVISIONS_PER_ARMY] = 0;
                }
            }

            public DOUBLE_OE<Div> SetD(Div t, double d)
            {
                data[t.index()] = d;
                return this;
            }
        }

        class DataA : DOUBLE_OE<Army>, SAVABLE
        {
            private readonly double[] dataa = new double[2];

            public double GetD(Army t)
            {
                return dataa[t.index()] / 100.0;
            }

            public void Save(FilePutter file)
            {
                file.ds(dataa);
            }

            public void Load(FileGetter file)
            {
                file.ds(dataa);
            }

            public void Clear()
            {
                Array.Fill(dataa, 0);
            }

            public DOUBLE_OE<Army> SetD(Army t, double d)
            {
                dataa[t.index()] = d;
                return this;
            }
        }

        public double Casulties(Army enemy)
        {
            return casulties.army.GetD(enemy);
        }

        public double Projectiles(Div div)
        {
            return projectiles.GetD(div);
        }

        public void ReportCasulty(Div division)
        {
            casulties.incD(division, 1);
        }

        public void ReportRout(Div division)
        {
            routing.incD(division, 1);
        }

        public double Casulties(Div div)
        {
            return casulties.GetD(div);
        }

        public void ReportProjectile(Div division)
        {
            projectiles.incD(division, 1);
        }

        public void ReportKill(Humanoid a)
        {
            Div d = a.division();
            if (d != null)
            {
                kills.incD(d, 1);
            }
        }

        public int Kills(Div div)
        {
            return (int)kills.GetD(div);
        }
    }
}