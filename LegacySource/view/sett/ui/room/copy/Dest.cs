using System;
using snake2d.util.datatypes;
using snake2d.util.map;

namespace view.sett.ui.room.copy
{
    internal sealed class Dest : MAP_BOOLEAN, BODY_HOLDER
    {
        private readonly Source source;
        private int cx, cy;
        private Coo sourceCoo = new Coo();
        private int rot = 0;
        private Rec body = new Rec();

        public Dest(Source source)
        {
            this.source = source;
        }

        public void Init(int cx, int cy, int rot)
        {
            body.Clear();
            this.rot = rot;
            this.cx = cx;
            this.cy = cy;

            int width = source.Area().Width();
            int height = source.Area().Height();

            for (int i = 0; i < this.rot; i++)
            {
                int ox = width;
                width = height;
                height = ox;
            }
            body.SetDim(width + 1, height + 1);
            body.MoveC(cx, cy);
        }

        public bool Is(int tile)
        {
            return Is(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
        }

        public bool Is(int tx, int ty)
        {
            return source.Is(Transform(tx, ty));
        }

        public bool SourceIs(int tx, int ty)
        {
            return source.Is(tx, ty);
        }

        public bool Blocking(int tx, int ty)
        {
            return SETT.PLACA().SolidityWill.Is(Transform(tx, ty));
        }

        public COORDINATE Transform(int tx, int ty)
        {
            int dx = tx - cx;
            int dy = ty - cy;

            for (int i = 0; i < rot; i++)
            {
                int ox = dx;
                dx = dy;
                dy = -ox;
            }

            dx = source.Area().CX() + dx;
            dy = source.Area().CY() + dy;

            sourceCoo.Set(dx, dy);
            return sourceCoo;
        }

        public RECTANGLE Body()
        {
            return body;
        }

        public int Rot()
        {
            return rot;
        }
    }
}