using System;
using System.Collections.Generic;
using System.Linq;

namespace Init.Resources
{
    public class ResGroup<T> where T : ResG
    {
        public readonly RBIT Mask;
        private readonly List<T> All;
        private readonly List<RESOURCE> Resources;
        private int[] IndexMap;
        public readonly string Key;
        public readonly RMAPS<T> Map;

        public ResGroup(string key, List<T> resses)
        {
            this.Key = key;
            All = new List<T>(resses);
            if (All.Count == 0)
                throw new Errors.DataError($"not enough {key} resources have been declared");
            List<RESOURCE> ress = new List<RESOURCE>(resses.Count);
            IndexMap = new int[RESOURCES.ALL().Count];
            Array.Fill(IndexMap, -1);
            RBITImp m = new RBITImp();
            foreach (T r in resses)
            {
                if (m.Has(r.Resource.Bit))
                    throw new Errors.DataError($"Several {key} is mapping to the same resource, and that doesn't work");
                ress.Add(r.Resource);
                m.Or(r.Resource.Bit);
                IndexMap[r.Resource.Index()] = r.Index();
            }
            this.Resources = ress;
            Mask = m;
            Map = new RMAPS<T>(key, resses);
        }

        public List<RESOURCE> Res()
        {
            return Resources;
        }

        public bool Is(RESOURCE res)
        {
            return Mask.Has(res);
        }

        public List<T> All()
        {
            return All;
        }

        public T Get(RESOURCE res)
        {
            if (Mask.Has(res.Bit))
            {
                return All[IndexMap[res.BIndex()]];
            }
            return null;
        }

        public RESOURCE[] MakeArray()
        {
            RESOURCE[] rr = new RESOURCE[All.Count];
            foreach (ResG e in All())
                rr[e.Index()] = e.Resource;
            return rr;
        }
    }
}