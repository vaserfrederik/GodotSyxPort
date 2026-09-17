using System;
using System.Collections.Generic;
using game;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util.color;
using util.statistics;
using util.text;
using view.sett;
using view.tool;

namespace settlement.stats.disease
{
    public class StatsDisease : StatCollection
    {
        private readonly Data data;
        private readonly Updater updater;
        private readonly Epidemic epidemic;
        public readonly HistoryInt healthHistory = new HistoryInt(STATS.DAYS_SAVED, TIME.days(), true)
        {
            public override double getD(int fromZero)
            {
                return get(fromZero) / 1024.0;
            }

            public override int get(int fromZero)
            {
                if (fromZero == 0)
                    return (int)(BOOSTABLES.PHYSICS().HEALTH.get(HCLASS_RACE.clP()) * 1024);
                return base.get(fromZero);
            }
        };

        private static readonly CharSequence ¤¤name = "Disease";
        private static readonly CharSequence ¤¤desc = "Disease stats.";
        public static readonly CharSequence ¤¤low = "The poor health in our settlement is a serious cause for concern. Lots of people are sick, and if not improved, there will be serious outbreaks.";
        public static readonly CharSequence ¤¤high = "Health in your settlement is good. There is no risk of outbreaks, but health can always be improved additionally to have less sick people to take care of.";

        static StatsDisease()
        {
            D.ts(typeof(StatsDisease));
        }

        public StatsDisease(StatsInit init) : base(init, "DISEASE", ¤¤name, ¤¤desc)
        {
            data = new Data(init);
            updater = new Updater(data, init);
            epidemic = new Epidemic(init);
            new BoostsHealth();

            init.upers.Add(new StatUpdatable()
            {
                public void update(double ds)
                {
                    healthHistory.set((int)(BOOSTABLES.PHYSICS().HEALTH.get(HCLASS_RACE.clP()) * 1024));
                }
            });

            init.savers.Put("D_HEALTH_HISTORY", healthHistory);

            IDebugPanelSett.Add(new PlacableSimple("Disease Infect")
            {
                public void place(int x, int y)
                {
                    foreach (ENTITY e in SETT.ENTITIES().getAtPointL(x, y))
                    {
                        if (e is Humanoid)
                        {
                            Humanoid a = (Humanoid)e;
                            infect(a.indu(), DISEASES.randomRegular());
                        }
                    }
                }

                public CharSequence isPlacable(int x, int y)
                {
                    return SETT.ENTITIES().getAtPoint(x, y) != null ? null : E;
                }
            });

            IDebugPanelSett.Add(new PlacableSimple("Disease Cure")
            {
                public void place(int x, int y)
                {
                    foreach (ENTITY e in SETT.ENTITIES().getAtPointL(x, y))
                    {
                        if (e is Humanoid)
                        {
                            Humanoid a = (Humanoid)e;
                            cure(a.indu(), false);
                        }
                    }
                }

                public CharSequence isPlacable(int x, int y)
                {
                    return SETT.ENTITIES().getAtPoint(x, y) != null ? null : E;
                }
            });
        }

        public int cases(HCLASS_RACE pop, DISEASE d)
        {
            return data.cases(pop, d);
        }

        public bool shouldHospital(Humanoid i)
        {
            if (!STATS.SERVICE().hospital.accessRequest(i))
                return false;
            if (shouldDie(i))
                return true;
            if (updater.time(i, 0) > 1.0)
                return true;
            return false;
        }

        public bool shouldDie(Humanoid i)
        {
            DISEASE d = data.get(i.indu());
            if (d == null)
                return false;
            return data.die.get(i.indu()) == 1;
        }

        public bool diseaseIsDone(Humanoid i, double treatment)
        {
            return updater.isDone(i, treatment);
        }

        public double diseaseTime(Humanoid i, double treatment)
        {
            return updater.time(i, treatment);
        }

        public STAT sick()
        {
            return data.infected;
        }

        public STAT incubating()
        {
            return data.incubating;
        }

        public DISEASE get(Induvidual i)
        {
            return data.get(i);
        }

        public DISEASE currentEpidemic()
        {
            return epidemic.current;
        }

        public DiseaseStatus status(Induvidual i)
        {
            return data.status(i);
        }

        public COLOR color(Induvidual i)
        {
            if (get(i) != null && status(i).active)
            {
                return get(i).color;
            }
            return null;
        }

        public void infect(Induvidual a, DISEASE d)
        {
            data.set(a, d, DiseaseStatus.ISICK);
        }

        public void incubate(Induvidual a, DISEASE d)
        {
            data.set(a, d, DiseaseStatus.INCUBATING);
        }

        public void cure(Induvidual a, bool hospital)
        {
            if (get(a) != null && status(a) != DiseaseStatus.IIMMUNE)
            {
                if (hospital)
                    GAME.count().CURED.inc(1);
                data.set(a, get(a), DiseaseStatus.IIMMUNE);
            }
        }

        public bool outbreak(double d, DISEASE currentStrain)
        {
            return epidemic.outbreak(d, currentStrain);
        }
    }
}