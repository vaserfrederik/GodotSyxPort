using System;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.map;
using snake2d.util.sets;

namespace view.sett.ui.room.copy
{
    internal class Source : MAP_BOOLEANE
    {
        private readonly Bitmap1D check = new Bitmap1D(SETT.TAREA, false);
        private readonly Rec rec = new Rec();

        public void Init()
        {
            rec.Clear();
            check.Clear();
        }

        public override bool Is(int tile)
        {
            return check.Get(tile);
        }

        public override bool Is(int tx, int ty)
        {
            return Is(tx + ty * SETT.TWIDTH);
        }

        public override MAP_BOOLEANE Set(int tile, bool value)
        {
            check.Set(tile, value);
            rec.Unify(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            return this;
        }

        public override MAP_BOOLEANE Set(int tx, int ty, bool value)
        {
            if (SETT.IN_BOUNDS(tx, ty))
            {
                Set(tx + ty * SETT.TWIDTH, value);
            }
            return this;
        }

        public RECTANGLE Area()
        {
            return rec;
        }
    }
}