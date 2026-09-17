using System;
using System.Collections.Generic;

namespace World.Entity.Haven
{
    using Game.Faction;
    using World;

    internal sealed class WHavenFactionData
    {
        internal readonly RaceData[] all;
        internal bool dirty;
        private readonly int fi;

        public WHavenFactionData(WHavens havens, int fi)
        {
            this.fi = fi;
            all = new RaceData[havens.types.size()];
            for (int i = 0; i < all.Length; i++)
                all[i] = new RaceData();
        }

        void Init()
        {
            if (!dirty)
                return;
            Faction f = FACTIONS.GetByIndex(fi);
            dirty = false;
            foreach (RaceData d in all)
                d.Clear();
            if (!f.IsActive())
                return;

            for (int i = 0; i < f.Realm().Regions(); i++)
            {
                foreach (WHaven h in WORLD.ENTITIES().Havens.Fill(f.Realm().Region(i)))
                {
                    all[h.Type().Index()].Add(h);
                }

            }
        }


        internal sealed class RaceData
        {

            internal int camps;
            internal int pop;
            internal double replenish;

            public void Clear()
            {
                camps = 0;
                pop = 0;
                replenish = 0;
            }
            public void Add(WHaven ii)
            {
                camps += 1;
                pop += ii.Pop();
                replenish += ii.Replenish();
            }
        }

    }
}