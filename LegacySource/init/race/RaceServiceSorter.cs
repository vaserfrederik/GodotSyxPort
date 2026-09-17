using System;
using System.Collections.Generic;
using System.Linq;

namespace Init.Race
{
    public sealed class RaceServiceSorter
    {
        public readonly IList<IList<StatGrave>> GRAVES;

        public RaceServiceSorter(Race race)
        {
            Sorter<StatGrave> serG = new Sorter<StatGrave>
            {
                Standing = t => t.Standing()
            };

            GRAVES = serG.Sort(STATS.BURIAL().Graves(), race);
        }

        private abstract class Sorter<T>
        {
            public IList<IList<T>> Sort(IList<T> all, Race race)
            {
                List<IList<T>> res = new List<IList<T>>(HCLASSES.ALL().Count);

                foreach (HCLASS dd in HCLASSES.ALL())
                {
                    HCLASS c = dd == HCLASSES.OTHER() ? HCLASSES.CITIZEN() : dd;

                    int am = 0;
                    foreach (T h in all)
                    {
                        if (Standing(h).Definition(race).Get(c).Max > 0)
                        {
                            am++;
                        }
                    }

                    T[] al = new T[am];
                    am = 0;
                    foreach (T h in all)
                    {
                        if (Standing(h).Definition(race).Get(c).Max > 0)
                        {
                            al[am++] = h;
                        }
                    }

                    Array.Sort(al, (o1, o2) =>
                    {
                        double d = Standing(o1).Definition(race).Get(c).Max - Standing(o2).Definition(race).Get(c).Max;
                        if (d < 0)
                            return 1;
                        if (d > 0)
                            return -1;
                        return 0;
                    });

                    res.Add(al);
                }

                return res;
            }

            public abstract StatStanding Standing(T t);
        }
    }
}