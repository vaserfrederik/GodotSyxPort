using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace World.Army
{
    public sealed class ADArmies
    {
        private readonly List<short> armies;
        private readonly int factionI;
        private readonly List list = new List();
        private readonly long[] data;

        public ADArmies(int factionI, int max)
        {
            this.factionI = factionI;
            armies = new List<short>(max);
            data = new long[AD.iinit().dataT.longCount()];
        }

        public readonly SAVABLE saver = new SAVABLE
        {
            Save = f =>
            {
                armies.Save(f);
                AD.iinit().dataT.saver().Save(faction(), f);
            },
            Load = f =>
            {
                armies.Load(f);
                AD.iinit().dataT.loader().Load(faction(), f);
            },
            Clear = () =>
            {
                armies.Clear();
                Array.Fill(data, 0);
            }
        };

        public Faction faction()
        {
            if (factionI == -1)
                return null;
            return FACTIONS.getByIndex(factionI);
        }

        public LIST<WArmy> all()
        {
            return list;
        }

        public bool canCreate()
        {
            return WORLD.ENTITIES().armies.canCreate() && armies.HasRoom();
        }

        public void disbandAll()
        {
            while (all().size() > 0)
            {
                all().get(0).disband();
            }
        }

        private sealed class List : LIST<WArmy>, IEnumerator<WArmy>
        {
            private int ii;

            public IEnumerator<WArmy> GetEnumerator()
            {
                ii = 0;
                return this;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public WArmy get(int index)
            {
                WArmy a = WORLD.ENTITIES().armies.get(armies[index]);
                if (a == null)
                    throw new Exception(index + " " + armies[index] + " " + faction());
                return a;
            }

            public bool contains(int i)
            {
                return i >= 0 && i < armies.Count;
            }

            public bool contains(WArmy object)
            {
                for (int i = 0; i < armies.Count; i++)
                {
                    if (get(i) == object)
                        return true;
                }
                return false;
            }

            public int size()
            {
                return armies.Count;
            }

            public bool isEmpty()
            {
                return armies.Count == 0;
            }

            public bool hasNext()
            {
                return ii < armies.Count;
            }

            public WArmy next()
            {
                WArmy r = get(ii);
                ii++;
                return r;
            }

            public void Reset()
            {
                ii = 0;
            }

            public void Dispose()
            {
                // No specific dispose logic required
            }
        }
    }
}