using System;
using settlement.main;
using snake2d.util.file;
using snake2d.util.map;

namespace settlement.path.components
{
    internal class SComp0Map : MAP_OBJECTE<SComp0>
    {
        private readonly SComp0Factory factory;
        private readonly int[] ids;

        public SComp0Map(SComp0Factory factory)
        {
            this.factory = factory;
            ids = Alloc.ii(SETT.TAREA);
        }

        public void Clear()
        {
            Array.Fill(ids, factory.NONE.index());
        }

        public override SComp0 Get(int tile)
        {
            SComp0 c = factory.Get(ids[tile]);
            if (c == factory.NONE)
                return null;
            return factory.Get(ids[tile]);
        }

        public override SComp0 Get(int tx, int ty)
        {
            if (SETT.IN_BOUNDS(tx, ty))
                return Get(tx + ty * SETT.TWIDTH);
            return null;
        }

        public override void Set(int tile, SComp0 obj)
        {
            if (obj == null)
                ids[tile] = factory.NONE.index();
            else
                ids[tile] = obj.index();
        }

        public override void Set(int tx, int ty, SComp0 obj)
        {
            if (IN_BOUNDS(tx, ty))
            {
                Set(tx + ty * SETT.TWIDTH, obj);
            }
        }
    }
}