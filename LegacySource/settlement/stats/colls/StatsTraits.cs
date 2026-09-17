using System;
using System.Collections.Generic;
using System.Linq;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.info;
using util.text;

public sealed class StatsTraits
{
    private readonly ArrayList<StatTrait> all;
    public readonly INFO info;

    public StatsTraits(StatsInit init)
    {
        D.t(this);
        info = new INFO(
            D.g("Trait"),
            D.g("Traits"),
            D.g("TraitDesc", "Traits can manifest themselves in individual subjects and in turn change properties of said person."), null);

        all = new ArrayList<StatTrait>(TRAITS.ALL().Count);

        foreach (TRAIT t in TRAITS.ALL())
        {
            new StatTrait(init, all, t);
        }

        init.onConstruct.Add(new StatInitable
        {
            public void init(Induvidual h)
            {
                TRAITS.init(h);
            }
        });
    }

    public LIST<StatTrait> all()
    {
        return all;
    }

    public StatTrait stat(TRAIT t)
    {
        return all.get(t.index());
    }

    public sealed class StatTrait : DOUBLE_OE<Induvidual>
    {
        public readonly TRAIT trait;
        private readonly Data data;
        private readonly double di = 1.0 / 0x0F;

        StatTrait(StatsInit init, LISTE<StatTrait> all, TRAIT trait)
        {
            all.add(this);
            this.trait = trait;
            this.data = new Data(init, "TRAIT_" + trait.key());
        }

        public int get(HCLASS c, Race r)
        {
            int ci = c == null ? HCLASSES.ALL().Count : c.index();
            int ri = r == null ? RACES.all().Count : r.index;
            return data.gdata[ci][ri];
        }

        public double getD(HCLASS c, Race r)
        {
            double p = STATS.POP().POP.data(c).get(r);
            double v = get(c, r) * di;
            if (p == 0)
                return CLAMP.d(v, 0, 1);
            return CLAMP.d(v / p, 0, 1);
        }

        public double getD(Div div)
        {
            return data.ddata[div.index()] * di;
        }

        public override double getD(Induvidual t)
        {
            return data.indu.get(t) * di;
        }

        public override DOUBLE_OE<Induvidual> setD(Induvidual t, double d)
        {
            data.removePrivate(t);
            data.indu.set(t, (int)(d * 0x0F));
            data.addPrivate(t);
            return data.indu;
        }

        private class Data : Addable
        {
            private readonly INT_OE<Induvidual> indu;
            private int[][] gdata = Alloc.i2(HCLASSES.ALL().Count + 1, RACES.all().Count + 1);
            private int[] ddata = Alloc.ii(Armies.DIVISIONS);

            public Data(StatsInit init, string key)
            {
                indu = init.count.new DataNibble("TRAIT_" + key);
                init.copier.Add(new INT_OE<Induvidual>
                {
                    public int get(Induvidual t)
                    {
                        return indu.get(t);
                    }

                    public int min(Induvidual t)
                    {
                        return 0;
                    }

                    public int max(Induvidual t)
                    {
                        return 0x0F;
                    }

                    public void set(Induvidual t, int i)
                    {
                        removeH(t);
                        indu.set(t, i);
                        addH(t);
                    }
                });
                init.addable.Add(this);
            }

            public override void addPrivate(Induvidual i)
            {
                if (i.player())
                {
                    gdata[i.clas().index()][RACES.all().Count] += indu.get(i);
                    gdata[HCLASSES.ALL().Count][RACES.all().Count] += indu.get(i);
                }
                gdata[i.clas().index()][i.race().index] += indu.get(i);
            }

            public override void removePrivate(Induvidual i)
            {
                if (i.player())
                {
                    gdata[i.clas().index()][RACES.all().Count] -= indu.get(i);
                    gdata[HCLASSES.ALL().Count][RACES.all().Count] -= indu.get(i);
                }
                gdata[i.clas().index()][i.race().index] -= indu.get(i);
            }
        }
    }
}