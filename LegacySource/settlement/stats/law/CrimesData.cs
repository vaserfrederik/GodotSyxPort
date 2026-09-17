using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Settlement.Stats.Law
{
    using Game.Boosting;
    using Game.Time;
    using Init.Race;
    using Init.Type;
    using Settlement.Stats;
    using Snake2D.Util.File;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Rnd;
    using Util.Statistics;
    using Util.Updating;

    class CrimesData : SAVABLE
    {
        public readonly HistoryObject<HCLASS_RACE> crimesComitted = new HistoryObject<HCLASS_RACE>(STATS.DAYS_SAVED, TIME.Days(), false, HCLASS_RACE.MAP());
        public readonly HistoryObject<HCLASS_RACE> criminalsCaught = new HistoryObject<HCLASS_RACE>(STATS.DAYS_SAVED, TIME.Days(), false, HCLASS_RACE.MAP());
        public readonly HistoryObject<HCLASS_RACE> lawHistory = new HistoryObject<HCLASS_RACE>(STATS.DAYS_SAVED, TIME.Days(), false, HCLASS_RACE.MAP());
        private readonly int[] criminalsTypes = Alloc.Ii(HCLASS_RACE.ALL().Count);

        public readonly IUpdater upCrime;
        public double escapedPrisoners;
        public readonly Curfew curfew = new Curfew();
        public readonly IUpdater up;
        public readonly IUpdater upPunishment;

        public CrimesData(StatsInit init)
        {
            init.savers["LAW_CRIMES_DATA"] = this;

            upCrime = new IUpdater(HCLASS_RACE.ALL().Count * CRIMES.ALL().Count, TIME.SecondsPerDay())
            {
                protected override void Update(int i, double timeSinceLast)
                {
                    HCLASS_RACE cl = HCLASS_RACE.ALL()[i / CRIMES.ALL().Count];
                    CRIME c = CRIMES.ALL()[i % CRIMES.ALL().Count];
                    c.Stat().Update(cl, timeSinceLast);
                }
            };

            upPunishment = new IUpdater(HCLASS_RACE.ALL().Count * CRIME_PUNISHMENTS.ALL().Count, TIME.SecondsPerDay())
            {
                protected override void Update(int i, double timeSinceLast)
                {
                    HCLASS_RACE cl = HCLASS_RACE.ALL()[i / CRIME_PUNISHMENTS.ALL().Count];
                    PUNISHMENT c = CRIME_PUNISHMENTS.ALL()[i % CRIME_PUNISHMENTS.ALL().Count];
                    c.Stat().DecRate(cl);
                }
            };

            up = new IUpdater(HCLASS_RACE.ALL().Count, TIME.SecondsPerDay())
            {
                protected override void Update(int cli, double ds)
                {
                    if (cli == 0)
                    {
                        escapedPrisoners -= 0.5 + STATS.POP().POP.Data().Get(null) * 0.01;
                        escapedPrisoners = CLAMP.d(escapedPrisoners, 0, STATS.POP().POP.Data().Get(null));
                    }

                    lawHistory.Set(HCLASS_RACE.ALL()[cli], (int)(10000 * BOOSTABLES.CIVICS().LAW.Get(HCLASS_RACE.ALL()[cli])));
                }
            };

            init.upers.Add(new StatUpdatable()
            {
                public void Update(double ds)
                {
                    upCrime.Update(ds);
                    up.Update(ds);
                    upPunishment.Update(ds);
                    curfew.Update(ds);
                }
            });

            init.addable.Add(new Addable()
            {
                public void RemovePrivate(Induvidual i)
                {
                    if (i.HType() == HTYPES.PRISONER())
                    {
                        STATS.LAW().crimes[STATS.LAW().prisonerType.Get(i).Index()].criminals[i.Race().Index]--;
                        Count(i, -1);
                        STATS.LAW().crimes[STATS.LAW().prisonerType.Get(i).Index()].criminalsTot--;
                    }
                }

                public void AddPrivate(Induvidual i)
                {
                    if (i.HType() == HTYPES.PRISONER() && STATS.LAW().prisonerType.Get(i) != null)
                    {
                        STATS.LAW().crimes[STATS.LAW().prisonerType.Get(i).Index()].criminals[i.Race().Index]++;
                        Count(i, 1);
                        STATS.LAW().crimes[STATS.LAW().prisonerType.Get(i).Index()].criminalsTot++;
                    }
                    else if (STATS.LAW().prisonerType.Get(i) == null || STATS.LAW().prisonerType.Get(i).Cl != i.Clas())
                    {
                        long m = 0;
                        foreach (CRIME c in CRIMES.All(i.Clas()))
                        {
                            if (c.IsCriminal())
                                m += (long)(1024 * c.Tyrrany(i.Clas(), i.Race()));
                        }
                        m *= RND.rFloat();
                        foreach (CRIME c in CRIMES.All(i.Clas()))
                        {
                            if (c.IsCriminal())
                                m -= (long)(1024 * c.Tyrrany(i.Clas(), i.Race()));
                            if (m <= 0)
                            {
                                STATS.LAW().prisonerType.Set(i, c);
                                break;
                            }
                        }
                    }
                }
            });
        }

        void Count(Induvidual i, int delta)
        {
            if (i.HType() == HTYPES.PRISONER())
            {
                HCLASS_RACE cl = HCLASS_RACE.ClP(i.Race(), STATS.LAW().prisonerType.Get(i).Cl);
                criminalsTypes[cl.Index] += delta;
                cl = HCLASS_RACE.ClP(null, STATS.LAW().prisonerType.Get(i).Cl);
                criminalsTypes[cl.Index] += delta;
            }
        }

        int Criminals(HCLASS cl, Race race)
        {
            HCLASS_RACE cll = HCLASS_RACE.ClP(race, cl);
            return criminalsTypes[cll.Index()];
        }

        public void Save(FilePutter file)
        {
            crimesComitted.Save(file);
            criminalsCaught.Save(file);
            lawHistory.Save(file);
            upCrime.Save(file);
            file.D(escapedPrisoners);
            curfew.Saver.Save(file);
            up.Save(file);
            upPunishment.Save(file);
        }

        public void Load(FileGetter file) throw IOException
        {
            crimesComitted.Load(file);
            criminalsCaught.Load(file);
            lawHistory.Load(file);
            upCrime.Load(file);
            escapedPrisoners = file.D();
            curfew.Saver.Load(file);
            up.Load(file);
            upPunishment.Load(file);
        }

        public void Clear()
        {
            crimesComitted.Clear();
            criminalsCaught.Clear();
            lawHistory.Clear();
            upCrime.Clear();
            escapedPrisoners = 0;
            curfew.Saver.Clear();
            up.Clear();
            upPunishment.Clear();
            Array.Fill(criminalsTypes, 0);
        }

        public double History(HCLASS_RACE ra, int days)
        {
            return lawHistory.History(ra).Get(days) / 10000.0;
        }
    }
}