using System;
using System.Collections.Generic;
using System.IO;

namespace World.Region
{
    public sealed class Realm
    {
        private readonly ArrayListShortResize regions = new ArrayListShortResize(32, WREGIONS.MAX);
        private readonly List list = new List();
        private short capitolI = -1;
        private readonly short index;

        private double ferArea = 0;

        public Realm(int index)
        {
            this.index = (short)index;
        }

        private readonly SAVABLE saver = new SAVABLE()
        {
            Save = file =>
            {
                regions.Save(file);
                file.Write(capitolI);
                file.Write(ferArea);
            },
            Load = file =>
            {
                regions.Load(file);
                capitolI = file.ReadShort();
                ferArea = file.ReadDouble();
            },
            Clear = () =>
            {
                regions.Clear();
                capitolI = -1;
                ferArea = 0;
            }
        };

        public double FerArea()
        {
            return ferArea;
        }

        public LIST<Region> All()
        {
            return list;
        }

        public int Regions()
        {
            return regions.Size();
        }

        public Region Region(int i)
        {
            return WORLD.REGIONS().All().Get(regions.Get(i));
        }

        public Region Capitol()
        {
            if (capitolI == -1)
                return null;
            return WORLD.REGIONS().GetByIndex(capitolI);
        }

        public Faction Faction()
        {
            return FACTIONS.GetByIndex(index);
        }

        private class List : LIST<Region>, IEnumerator<Region>
        {
            private int ii;

            public IEnumerator<Region> GetEnumerator()
            {
                ii = 0;
                return this;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public Region Get(int index)
            {
                return WORLD.REGIONS().GetByIndex(regions.Get(index));
            }

            public bool Contains(int i)
            {
                return i >= 0 && i < regions.Size();
            }

            public bool Contains(Region obj)
            {
                for (int i = 0; i < regions.Size(); i++)
                {
                    if (Get(i) == obj)
                        return true;
                }
                return false;
            }

            public int Size()
            {
                return regions.Size();
            }

            public bool IsEmpty()
            {
                return regions.IsEmpty();
            }

            public bool MoveNext()
            {
                return ii < regions.Size();
            }

            public void Reset()
            {
                ii = 0;
            }

            public Region Current => Get(ii);

            object System.Collections.IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}