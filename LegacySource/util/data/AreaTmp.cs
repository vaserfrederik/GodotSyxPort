using System;
using snake2d.util.datatypes;
using snake2d.util.map;
using snake2d.util.sets;

namespace util.data
{
    public class AreaTmp : AREA, MAP_SETTER, MAP_CLEARER
    {
        private readonly Bitmap1D data;
        private readonly Rec body;
        private int area;

        public AreaTmp()
        {
            data = new Bitmap1D(SETT.TAREA, false);
            body = new Rec();
            area = 0;
        }

        public RECTANGLE Body()
        {
            return body;
        }

        public bool Is(int tile)
        {
            return data.Get(tile);
        }

        public bool Is(int tx, int ty)
        {
            if (SETT.IN_BOUNDS(tx, ty))
                return data.Get(tx + ty * SETT.TWIDTH);
            return false;
        }

        public int Area()
        {
            return area;
        }

        public void Clear()
        {
            if (area > 0)
                data.Clear();
            area = 0;
            body.SetDim(0, 0).MoveX1Y1(-1, -1);
        }

        public MAP_SETTER Set(int tile)
        {
            throw new Exception();
        }

        public MAP_SETTER Set(int tx, int ty)
        {
            if (IN_BOUNDS(tx, ty))
            {
                if (area == 0)
                {
                    body.SetDim(1).MoveX1Y1(tx, ty);
                }
                else
                {
                    body.Unify(tx, ty);
                }
                area++;
                data.Set(tx + ty * SETT.TWIDTH, true);
            }
            return this;
        }

        public MAP_CLEARER Clear(int tile)
        {
            throw new Exception();
        }

        public MAP_CLEARER Clear(int tx, int ty)
        {
            if (IN_BOUNDS(tx, ty))
            {
                int i = tx + ty * SETT.TWIDTH;
                if (data.Get(i))
                {
                    area--;
                    data.Set(tx + ty * SETT.TWIDTH, false);
                }
            }
            return this;
        }
    }
}