using System;
using System.Collections.Generic;
using init.race;
using init.type;
using settlement.entity.humanoid;
using settlement.stats;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.data;
using util.keymap;

namespace settlement.stats.stat
{
    public class StatDecree : StatInfo
    {
        private readonly int def;

        private readonly RMapIntTwo<HCLASS, Race> levels = new RMapIntTwo<HCLASS, Race>(HCLASSES.MAP(), RACES.map())
        {
            public override void clear()
            {
                base.clear();
                setAll(def);
            }
        };

        private readonly ArrayList<INT_OE<Race>> tars;
        private readonly double ii;
        private readonly double iii;

        public StatDecree(string dkey, StatsInit i, int min, int max, int fractions, string name, int def)
            : base(name, "")
        {
            this.def = def;
            i.savers.put(dkey + "_DEC", levels);
            levels.setAll(def);
            tars = new ArrayList<INT_OE<Race>>(HCLASSES.ALL().size());

            final int m = max * fractions;
            ii = 1.0 / (m);
            iii = 1.0 / fractions;
            foreach (HCLASS c in HCLASSES.ALL())
            {
                tars.add(new INT_OE<Race>()
                {
                    public int min(Race t)
                    {
                        return min;
                    }

                    public int max(Race t)
                    {
                        return m;
                    }

                    public int get(Race t)
                    {
                        if (t == null)
                        {
                            int m = 0;
                            for (int ri = 0; ri < RACES.all().size(); ri++)
                            {
                                Race r = RACES.all().get(ri);
                                m = Math.Max(m, get(r));
                            }
                            return m;
                        }
                        return CLAMP.i(levels.get(c).get(t), min, m);
                    }

                    public void set(Race t, int i)
                    {
                        if (t == null)
                        {
                            for (int ri = 0; ri < RACES.all().size(); ri++)
                            {
                                Race r = RACES.all().get(ri);
                                set(r, i);
                            }
                            return;
                        }
                        levels.get(c).set(t, CLAMP.i(i, min, m));
                    }
                });
            }
        }

        public INT_OE<Race> getI(HCLASS c)
        {
            return tars.get(c.index());
        }

        public double getD(HCLASS c, Race race)
        {
            return tars.get(c.index()).get(race) * ii;
        }

        public double get(HCLASS c, Race race)
        {
            return tars.get(c.index()).get(race) * iii;
        }

        public int get(Humanoid h)
        {
            double d = tars.get(h.indu().clas().index()).get(h.indu().race()) * iii;
            int i = (int)d;
            if (RND.rFloat() < d - i)
                i++;
            return i;
        }
    }
}