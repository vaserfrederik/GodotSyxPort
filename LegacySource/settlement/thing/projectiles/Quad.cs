using System.Collections.Generic;
using settlement.main;
using settlement.thing.projectiles;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace settlement.thing.projectiles
{
    internal sealed class Quad
    {
        private int first = -1;
        private readonly int qx, qy;

        public Quad(int qx, int qy)
        {
            this.qx = qx;
            this.qy = qy;
        }

        void Add(int index)
        {
            SETT.PROJS().data.NextSet(index, first);
            first = index;
        }

        void Remove(int e)
        {
            int first = this.first;
            this.first = -1;

            while (first != -1)
            {
                int ee = first;
                first = SETT.PROJS().data.Next(first);
                if (ee != e)
                {
                    SETT.PROJS().data.NextSet(ee, this.first);
                    this.first = ee;
                }
            }

            SETT.PROJS().data.NextSet(e, -1);
        }

        bool Contains(int e)
        {
            int di = first;
            while (di != -1)
            {
                if (di == e)
                    return true;
                di = SETT.PROJS().data.Next(di);
            }
            return false;
        }

        void Clear()
        {
            first = -1;
        }

        void Fill(RECTANGLE bounds, ArrayListInt result)
        {
            int di = first;
            while (di != -1 && result.HasRoom())
            {
                Data data = SETT.PROJS().data.Data(di);
                if (bounds.HoldsPoint(data.X(), data.Y()))
                {
                    result.Add(di);
                }
                di = SETT.PROJS().data.Next(di);
            }
        }
    }
}