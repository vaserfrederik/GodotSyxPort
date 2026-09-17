using System;
using System.Collections.Generic;
using settlement.stats.disease;
using game.boosting;
using game.time;
using init.type;
using settlement.entity.humanoid;
using settlement.stats;
using settlement.stats.StatsInit;
using snake2d.util.rnd;
using view.main;

namespace settlement.stats.disease
{
    final class Updater : StatUpdatableI
    {
        private readonly Data data;

        public Updater(Data data, StatsInit init)
        {
            this.data = data;
            init.updatable.Add(this);
        }

        public void Update16(Humanoid h, int updateR, bool day, int updateI)
        {
            Induvidual i = h.indu();
            if (VIEW.b().IsActive())
                return;
            DiseaseStatus st = data.status(i);

            switch (st)
            {
                case DiseaseStatus.IIMMUNE:
                    if (!day)
                        return;
                    if (data.count.isMax(i))
                    {
                        data.set(i, null, null);
                    }
                    else
                    {
                        data.count.inc(i, 1);
                    }
                    regular(h);
                    break;
                case DiseaseStatus.INCUBATING:
                    DISEASE d = data.get(i);
                    if (d == null || ((((STATS.RAN().get(i, 11, 16) + TIME.seasons().bitsSinceStart()) >> 7) & 0b11) == 0 && STATS.LAW().getCurfew().is()))
                    {
                        data.set(i, null, null);
                    }
                    else if (RND.oneIn(d.incubationDays * 16))
                    {
                        data.set(i, data.get(i), DiseaseStatus.ISICK);
                    }
                    break;
                case DiseaseStatus.ISICK:
                    if (!day)
                        return;
                    data.count.inc(i, 1);
                    break;
                case DiseaseStatus.NONE:
                    if (day)
                        regular(h);
                    break;
                default:
                    break;
            }
        }

        private void regular(Humanoid h)
        {
            Induvidual i = h.indu();
            if (!i.hType().parentClass().player)
                return;
            DiseaseStatus st = data.status(i);
            if (shouldGetSickDay(h.indu()))
            {
                DISEASE d2 = DISEASES.randomRegular();
                if (d2 == null || (data.get(i) == d2 && st == DiseaseStatus.IIMMUNE))
                {

                }
                else
                {
                    data.set(i, d2, DiseaseStatus.ISICK);
                }
            }
        }

        public bool isDone(Humanoid i, double treatment)
        {
            return time(i, treatment) <= 0;
        }

        public double time(Humanoid h, double treatment)
        {
            Induvidual i = h.indu();
            DiseaseStatus st = data.status(i);
            if (!st.active)
                return 0;
            if (data.get(i) == null)
                return 0;
            return data.get(i).length * (1 - treatment) - (data.count.get(i) + h.partOfDay());
        }

        public static bool shouldGetSickDay(Induvidual a)
        {
            double chance = DISEASES.regularDays() * (1 + Math.Max(BOOSTABLES.PHYSICS().HEALTH.get(a), 0));

            if (RND.oneIn((int)Math.Ceiling(chance)))
            {
                return true;
            }
            return false;
        }
    }
}