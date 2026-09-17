using System;
using System.Collections.Generic;
using world.region;
using settlement.room.industry.module;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;
using util.data;
using world;
using world.map.regions;

public class RDProspects
{
    private readonly List<II> all = new List<II>(IndustryRegion.ALL().Count);
    private bool init = false;

    public RDProspects(RDInit init)
    {
        foreach (IndustryRegion ii in IndustryRegion.ALL())
        {
            all.Add(new II(init, ii));
        }
    }

    public double Get(Industry ins, Region reg)
    {
        return Get(ins.Reg(), reg);
    }

    public double Get(IndustryRegion ins, Region reg)
    {
        if (reg == null)
            return 0;
        return all[ins.Index].Data.Get(reg);
    }

    private static bool log = false;

    public void Tt()
    {
        init = false;
        Init();
    }

    private void Init()
    {
        if (init)
            return;
        init = true;
        foreach (II ii in all)
        {
            double am = 0;
            foreach (Region reg in WORLD.REGIONS().Active())
            {
                am += ii.Data.Get(reg);
            }
            if (log)
                LOG.Ln(ii.Reg.Ins.Blue.Key + " " + ii.Reg.Rarity + " " + am + " " + am / WORLD.REGIONS().Active().Count);
            am /= WORLD.REGIONS().Active().Count;
            ii.Average = 1.0 / am;
        }

        foreach (II ii in all)
        {
            double min = int.MaxValue;
            double max = 0;
            double am = 0;
            foreach (Region reg in WORLD.REGIONS().Active())
            {
                double a = GetAi(ii.Reg, reg);
                am += a;
                min = Math.Min(a, min);
                max = Math.Max(max, a);
            }

            am /= WORLD.REGIONS().Active().Count;
            if (log)
                LOG.Ln(ii.Reg.Ins.Blue.Key + " " + am + " " + min + " <-> " + max);
        }
    }

    public double GetAi(IndustryRegion ins, Region r)
    {
        if (ins == null)
            return 1;
        Init();
        return 0.8 + 0.2 * all[ins.Index].Data.Get(r) * all[ins.Index].Average;
    }

    private class II
    {
        private readonly INT_OE<Region> Data;
        private readonly IndustryRegion Reg;
        private double Average;

        public II(RDInit init, IndustryRegion reg)
        {
            Data = init.Count.NewDataCrumb("PROSPECT_" + reg.Ins.Blue.Key);
            this.Reg = reg;
        }
    }

    void Generate()
    {
        foreach (Region r in WORLD.REGIONS().All())
        {
            foreach (II ii in all)
            {
                ii.Data.Set(r, 0);
                ii.Average = 0;
            }
        }

        int regs = WORLD.REGIONS().Active().Count;

        double amPerRegion = 2;
        double tot = regs * amPerRegion;
        double rareTot = 0;
        foreach (IndustryRegion ii in IndustryRegion.ALL())
        {
            rareTot += ii.Rarity;
        }

        int[] toAssign = Alloc.Ii(IndustryRegion.ALL().Count);
        int[] assigned = Alloc.Ii(WREGIONS.MAX);

        for (int i = 3; i > 0; i--)
        {
            foreach (IndustryRegion ii in IndustryRegion.ALL())
            {
                toAssign[ii.Index] = (int)Math.Ceiling(tot * ii.Rarity / rareTot);
            }

            while (Assign(toAssign, assigned, i))
                ;
        }

        foreach (II ii in all)
        {
            double am = 0;
            foreach (Region reg in WORLD.REGIONS().Active())
            {
                am += ii.Data.Get(reg);
            }

            am /= WORLD.REGIONS().Active().Count;
            ii.Average = am;
        }

        init = false;
        Init();
    }

    private bool Assign(int[] toAssign, int[] assigned, int value)
    {
        bool a = false;
        foreach (IndustryRegion ii in IndustryRegion.ALL())
        {
            if (toAssign[ii.Index] > 0)
            {
                Region best = null;
                double bv = double.NegativeInfinity;

                foreach (Region reg in WORLD.REGIONS().Active())
                {
                    if (reg != WORLD.REGIONS().Player && all[ii.Index].Data.Get(reg) == 0)
                    {
                        double v = ii.Occurence(reg) / (1.0 + assigned[reg.Index()]);
                        if (v > bv)
                        {
                            bv = v;
                            best = reg;
                        }
                    }
                }

                if (best != null)
                {
                    all[ii.Index].Data.Set(best, value);
                    toAssign[ii.Index]--;
                    assigned[best.Index()]++;
                    a = true;
                }
            }
        }
        return a;
    }
}