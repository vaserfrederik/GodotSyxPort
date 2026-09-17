using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Init.Race;
using Init.Type;
using Settlement.Stats;
using Util.Gui.Misc;
using Util.Info;
using Util.Text;
using Util.Util.File;
using Util.Util.Misc;

namespace Settlement.Stats.Law
{
    public sealed class StatCrimePersecute : StatCrime
    {
        private static readonly CharSequence ¤¤recentPersecutions = "Recent Persecutions";
        private static readonly CharSequence ¤¤recentPersecutionsMul = "Multiplier from recent persecutions";

        static StatCrimePersecute()
        {
            D.ts(typeof(StatCrimePersecute));
        }

        private readonly double[] active;

        public StatCrimePersecute(StatsInit init, CRIME type, CrimesData data) : base(init, type, data)
        {
            active = new double[HCLASS_RACE.ALL().Count];
            init.savers.Add("LAW_CRIME_DATA_PERS " + type.cl, new SAVABLE()
            {
                public void Save(FilePutter file)
                {
                    HCLASS_RACE.MAP().saver().Save(active, file);
                }

                public void Load(FileGetter file)
                {
                    HCLASS_RACE.MAP().loader().Load(active, file, 0);
                }

                public void Clear()
                {
                    Array.Fill(active, 0);
                }
            });
        }

        public override void Catch(Race race)
        {
            active[HCLASS_RACE.clP(race, crime.cl).index]++;
            base.Catch(race);
        }

        public double Value(HCLASS cl, Race race)
        {
            return CLAMP.d(100 * active(cl, race) / (1 + POP.pop(cl, race)), 0, 1);
        }

        private double active(HCLASS cl, Race race)
        {
            if (race == null)
            {
                double pop = 0;
                double res = 0;
                foreach (Race r in RACES.all())
                {
                    double p = STATS.POP().POP.data(cl).Get(r);
                    pop += p;
                    res += p * Value(cl, r);
                }
                if (pop == 0)
                    return 0;
                else
                    return res / pop;
            }

            HCLASS_RACE cc = HCLASS_RACE.clP(race, cl);
            return active[cc.index];
        }

        public override double LawValue(HCLASS cl, Race race)
        {
            return base.LawValue(cl, race) * Value(cl, race);
        }

        public override double TyrranyValue(HCLASS cl, Race race)
        {
            return base.TyrranyValue(cl, race) * Value(cl, race);
        }

        protected override void Update(HCLASS_RACE rr, double ds)
        {
            active[rr.index] -= (0.1 + active[rr.index] * 0.05) * ds * TIME.secondsPerDayI();
            active[rr.index] = CLAMP.d(active[rr.index], 0, STATS.POP().POP.data().Get(null));
            base.Update(rr, ds);
        }

        public override void Hover(GUI_BOX box, HCLASS cl, Race race)
        {
            base.Hover(box, cl, race);
            GBox b = (GBox)box;
            {
                b.sep();
                b.textLL(¤¤recentPersecutions);
                b.tab(6);
                b.add(GFORMAT.f(b.text(), active(cl, race)), 1);
                b.NL();

                b.textLL(¤¤recentPersecutionsMul);
                b.NL();
                b.add(GFORMAT.perc(b.text(), Value(cl, race)));
            }
        }
    }
}