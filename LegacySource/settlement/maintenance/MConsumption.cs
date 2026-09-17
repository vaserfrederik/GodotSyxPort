using System;
using System.Collections.Generic;
using settlement.main;
using settlement.path;
using snake2d.util.datatypes;
using snake2d.util.map;

namespace settlement.maintenance
{
    final class MConsumption
    {
        private readonly AbsGrid grid = new AbsGrid(SETT.TWIDTH, SETT.THEIGHT, 32);
        private readonly QData[] datas = new QData[grid.all.size()];
        private readonly long[] ress = new long[RESOURCES.ALL().size()];
        private readonly MAINTENANCE m;

        private int upI = 0;

        private const int DD = 1024 * 16;
        private const double DDI = 1.0 / DD;

        MConsumption(MAINTENANCE m)
        {
            this.m = m;
            for (int i = 0; i < datas.Length; i++)
                datas[i] = new QData();

            new AvailabilityListener
            {
                protected override void changed(int tx, int ty, AVAILABILITY a, AVAILABILITY old, bool playerChange)
                {
                    if (m.disabled.is(tx, ty))
                        return;
                    int in = grid.map.get(tx, ty).index;
                    datas[in].changed = true;
                }
            };
        }

        void init()
        {
            for (int i = 0; i < grid.all.size(); i++)
            {
                update(i);
            }
        }

        void update()
        {
            if (upI >= grid.all.size())
                upI = 0;

            if (datas[upI].changed)
            {
                update(upI);
            }
            upI++;
        }

        void change(int tx, int ty)
        {
            int in = grid.map.get(tx, ty).index;
            datas[in].changed = true;
        }

        private void update(int i)
        {
            QData d = datas[i];
            d.changed = false;
            for (int r = 0; r < RESOURCES.ALL().size(); r++)
            {
                ress[r] -= (long)Math.Ceiling(d.amounts[r] * DD);
                d.amounts[r] = 0;
            }
            foreach (COORDINATE c in grid.get(i))
            {
                if (m.disabled.is(c))
                    continue;
                foreach (MType t in m.types)
                {
                    for (int r = 1; r < 5; r++)
                    {
                        double am = t.resRate(c.x(), c.y(), r);
                        if (am > 0)
                        {
                            d.amounts[t.res(c.x(), c.y(), r).index()] += am;
                        }
                    }
                }
            }
            for (int r = 0; r < RESOURCES.ALL().size(); r++)
            {
                ress[r] += (long)Math.Ceiling(d.amounts[r] * DD);
            }
        }

        private class QData
        {
            private double[] amounts = new double[RESOURCES.ALL().size()];
            private bool changed = false;
        }

        public double get(RESOURCE res)
        {
            return ress[res.index()] * DDI;
        }
    }
}