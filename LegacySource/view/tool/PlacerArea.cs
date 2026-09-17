using System;
using snake2d.util.datatypes;
using snake2d.util.map;
using snake2d.util.sets;

namespace view.tool
{
    final class PlacerArea : AREA
    {
        static PlacerArea self = new PlacerArea();

        private readonly Bitmap1D map = new Bitmap1D(SETT.TAREA, false);
        private readonly Rec bounds = new Rec();
        private int area = 0;

        private PlacerArea()
        {
        }

        public override bool is(int tile)
        {
            return map.get(tile);
        }

        public override bool is(int tx, int ty)
        {
            if (SETT.IN_BOUNDS(tx, ty))
                return is(tx + ty * SETT.TWIDTH);
            return false;
        }

        final MAP_SETTER set = new MAP_SETTER()
        {
            public MAP_SETTER set(int tx, int ty)
            {
                if (SETT.IN_BOUNDS(tx, ty))
                {
                    bounds.unify(tx, ty);
                    int i = tx + ty * SETT.TWIDTH;
                    if (!map.get(i))
                    {
                        area++;
                        map.set(i, true);
                    }
                }
                return this;
            }

            public MAP_SETTER set(int tile)
            {
                throw new RuntimeException();
            }
        };

        void clear()
        {
            if (area > 0)
            {
                foreach (COORDINATE c in bounds)
                {
                    map.set(c.x() + c.y() * SETT.TWIDTH, false);
                }
                bounds.set(SETT.TWIDTH, 0, SETT.THEIGHT, 0);
                area = 0;
            }
        }

        public override RECTANGLE body()
        {
            return bounds;
        }

        public override int area()
        {
            return area;
        }
    }
}