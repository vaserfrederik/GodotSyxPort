using System;
using System.Collections.Generic;
using settlement.stats.disease;
using init.type;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util.file;
using snake2d.util.rnd;
using util.data.INT_O;

namespace Settlement.Stats.Disease
{
    class Data
    {
        public readonly STATData infected;
        public readonly STATData incubating;
        private readonly INT_OE<Induvidual> index;
        public readonly INT_OE<Induvidual> count;
        private readonly INT_OE<Induvidual> status;
        public readonly INT_OE<Induvidual> die;
        private int[][] active;
        private readonly Addable adder;

        public Data(StatsInit init)
        {
            infected = new SS(init, "INFECTED", "D_INFECTED", DiseaseStatus.ISICK);
            incubating = new SS(init, "INCUBATE", "D_INCUBATE", DiseaseStatus.INCUBATING);

            incubating.info().setMatters(false, true);

            index = init.count.new DataByte("DIS_INDEX");
            count = init.count.new DataNibble("DIS_COUNT");
            die = init.count.new DataBit("DIS_DIE");
            adder = new Addable
            {
                addPrivate = (i) =>
                {
                    if (i.player && Get(i) != null && Status(i).active)
                    {
                        active[HCLASS_RACE.clP(i).index][Get(i).index()]++;
                        active[HCLASS_RACE.clP(i.clas()).index][Get(i).index()]++;
                    }
                },
                removePrivate = (i) =>
                {
                    if (i.player && Get(i) != null && Status(i).active)
                    {
                        active[HCLASS_RACE.clP(i).index][Get(i).index()]--;
                        active[HCLASS_RACE.clP(i.clas()).index][Get(i).index()]--;

                        active[HCLASS_RACE.clP(i).index][Get(i).index()] &= int.MaxValue;
                        active[HCLASS_RACE.clP(i.clas()).index][Get(i).index()] &= int.MaxValue;
                    }
                }
            };
            init.addable.add(adder);

            status = init.count.new DataNibble("DIST_STATUS");
        }

        private class SS : STATData
        {
            public SS(StatsInit init, string key, string dkey, DiseaseStatus status) : base(key, key, init, new INT_OE<Induvidual>
            {
                get = (t) => Data.this.Get(t) != null && Status(t) == status ? 1 : 0,
                min = (t) => 0,
                max = (t) => 1,
                set = (t, i) => { }
            })
            { }
        }

        public DISEASE Get(Induvidual t)
        {
            int inValue = index.get(t) - 1;
            if (inValue < 0)
                return null;
            return DISEASES.all().get(inValue);
        }

        public DiseaseStatus Status(Induvidual t)
        {
            return DiseaseStatus.ALL.getC(status.get(t));
        }

        public int Cases(HCLASS_RACE pop, DISEASE d)
        {
            return active[pop.index][d.index()];
        }

        public void Set(Induvidual t, DISEASE d, DiseaseStatus status)
        {
            infected.removeH(t);
            incubating.removeH(t);
            adder.removeH(t);

            int i = 0;
            if (d != null)
            {
                i = d.index() + 1;
                double f = d.fatalityRate;
                if (t.hType().parent() != t.hType())
                    f *= 1 + RND.rFloat(5);
                die.set(t, RND.rFloat() < f ? 1 : 0);
            }
            else
            {
                status = DiseaseStatus.NONE;
            }

            index.set(t, i);
            this.status.set(t, (int)status);
            count.set(t, 0);

            infected.addH(t);
            incubating.addH(t);
            adder.addH(t);
        }
    }
}