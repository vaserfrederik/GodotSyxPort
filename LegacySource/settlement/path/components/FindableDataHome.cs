using System.Collections.Generic;
using init.type;
using snake2d.util.sets;

namespace settlement.path.components
{
    public sealed class FindableDataHome
    {
        private readonly ArrayList<FindableData> all;

        public FindableDataHome()
        {
            all = new ArrayList<FindableData>(HGROUP.All().Count);
            foreach (HGROUP t in HGROUP.All())
            {
                all.Add(new FindableData("home " + t.Name));
            }
        }

        public void Add(SComponent c, HTypeBits t)
        {
            for (int ti = 0; ti < HGROUP.All().Count; ti++)
            {
                if (t.Is(ti))
                {
                    all[ti].Add(c);
                }
            }
        }

        public void Remove(SComponent c, HGROUP t)
        {
            all[t.Index()].Remove(c);
        }

        public bool Has(SComponent c, HGROUP t)
        {
            return all[t.Index()].Get(c) > 0;
        }

        public void ReportPresence(int tx, int ty, HTypeBits t)
        {
            for (int ti = 0; ti < HGROUP.All().Count; ti++)
            {
                if (t.Is(ti))
                {
                    all[ti].ReportPresence(tx, ty);
                }
            }
        }

        public void ReportAbsence(int tx, int ty, HTypeBits t)
        {
            for (int ti = 0; ti < HGROUP.All().Count; ti++)
            {
                if (t.Is(ti))
                {
                    all[ti].ReportAbsence(tx, ty);
                }
            }
        }

        public FindableData Get(HGROUP t)
        {
            return all[t.Index()];
        }
    }
}