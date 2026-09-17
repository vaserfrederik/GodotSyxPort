using System;
using System.Collections.Generic;

namespace util.gui.table
{
    public class GRows
    {
        private GuiSection s = null;
        private readonly LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();
        private int ii = 0;
        private readonly int max;
        private int pad = 0;
        private int minDist = 0;

        public GRows(int rowSize)
        {
            this.max = rowSize;
        }

        public GRows SetPad(int pad)
        {
            this.pad = pad;
            return this;
        }

        public GRows SetMin(int min)
        {
            this.minDist = min;
            return this;
        }

        public void Add(RENDEROBJ obj)
        {
            if (ii % max == 0)
            {
                s = new GuiSection();
                rows.Add(s);
                ii = 0;
            }
            int p = minDist - obj.Body().Width();
            if (pad > p)
                p = pad;
            s.AddRight(p, obj);

            ii++;
        }

        public void Nl()
        {
            if (s == null || s.Elements().Count == 0)
                return;
            s = new GuiSection();
            rows.Add(s);
            ii = 0;
        }

        public int Height()
        {
            int h = 0;
            foreach (RENDEROBJ o in rows)
                h += o.Body().Height();
            return h;
        }

        public LIST<RENDEROBJ> Rows()
        {
            foreach (RENDEROBJ rr in rows)
            {
                GuiSection s = (GuiSection)rr;
                if (s.GetLast().Width() < minDist)
                {
                    s.Body().IncrW(minDist - s.GetLast().Width());
                }
            }
            return rows;
        }

        public LIST<RENDEROBJ> RowsCentered(int width)
        {
            foreach (RENDEROBJ rr in Rows())
            {
                GuiSection s = (GuiSection)rr;
                if (s.Body().Width() < width)
                    s.Pad((width - s.Body().Width()) / 2, 0);
            }
            return rows;
        }
    }
}