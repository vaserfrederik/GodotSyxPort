using System;
using System.Collections.Generic;
using static settlement.main.SETT.PATH;
using init.resources;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace settlement.path.components
{
    public static class FindableDataRes
    {
        private static readonly LinkedList<FindableDataRes> all = new LinkedList<FindableDataRes>();

        private readonly ArrayList<FindableData> datas;
        private readonly int index;
        public readonly string title;

        public FindableDataRes(string title)
        {
            this.title = title;
            datas = new ArrayList<FindableData>(RESOURCES.ALL().Count);
            for (int i = 0; i < RESOURCES.ALL().Count; i++)
            {
                datas.Add(new Res(RESOURCES.ALL()[i]));
            }
            index = all.Add(this);
        }

        public void Add(SComponent c, RESOURCE res)
        {
            datas[res.Index()].Add(c);
        }

        public bool Remove(SComponent c, RESOURCE res)
        {
            return datas[res.Index()].Remove(c);
        }

        public int Get(SComponent c, RESOURCE res)
        {
            return datas[res.Index()].Get(c);
        }

        public bool Overflow(SComponent c, RESOURCE res)
        {
            return datas[res.Index()].Overflow(c);
        }

        public int Get(SComponent c, int res)
        {
            return datas[res].Get(c);
        }

        public RBIT Bits(SComponent c)
        {
            return c.ress[index];
        }

        public RBIT Bits(int sx, int sy)
        {
            SComponent s = PATH().comps.zero.Get(sx, sy);
            if (s == null)
                return RBIT.NONE;
            while (s.SuperComp() != null)
                s = s.SuperComp();
            return s.ress[index];
        }

        public bool Has(SComponent c, RBIT mask)
        {
            return Bits(c).Has(mask);
        }

        public void ReportPresence(int tx, int ty, RESOURCE res)
        {
            datas[res.Index()].ReportPresence(tx, ty);
        }

        public void ReportAbsence(int tx, int ty, RESOURCE res)
        {
            datas[res.Index()].ReportAbsence(tx, ty);
        }

        private readonly DIR[] dirs = new DIR[] {
            DIR.C, DIR.N, DIR.E, DIR.S, DIR.W
        };

        public bool Has(int startX, int startY, RBIT mask)
        {
            foreach (DIR d in dirs)
            {
                SComponent s = PATH().comps.zero.Get(startX, startY, d);
                if (s == null)
                    continue;
                while (s.SuperComp() != null)
                    s = s.SuperComp();
                if (Has(s, mask))
                    return true;
            }
            return false;
        }

        public SCompPatherFinder Finder(RBITImp mask)
        {
            fetchmask = mask;
            return finder;
        }

        private RBITImp fetchmask;
        private readonly SCompPatherFinder finder = new SCompPatherFinder()
        {
            public bool IsInComponent(SComponent c, double distance)
            {
                return c.ress[index].Has(fetchmask);
            }
        };

        private class Res : FindableData
        {
            private readonly RESOURCE res;

            public Res(RESOURCE res) : base(res.Name)
            {
                this.res = res;
            }

            public override void Add(SComponent c)
            {
                base.Add(c);
                c.ress[index].Or(res);
            }

            public override bool Remove(SComponent c)
            {
                bool ret = base.Remove(c);
                if (Get(c) == 0)
                {
                    c.ress[index].Clear(res);
                }
                return ret;
            }
        }
    }
}