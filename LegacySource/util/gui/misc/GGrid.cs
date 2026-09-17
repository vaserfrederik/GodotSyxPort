using System;
using System.Collections.Generic;
using init.constant;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;

namespace util.gui.misc
{
    public class GGrid
    {
        private readonly ArrayList<RENDEROBJ> row;
        private int maxHeight = 0;

        private int startX;
        private int startY;
        private readonly int elements;
        private int width;
        private int marginY = 0;
        private int count;

        private DIR align = DIR.NW;
        public readonly GuiSection section;

        public GGrid(GuiSection section, int width, int elements, int x1, int y1)
            : this(section, width, elements, x1, y1, 0)
        {
        }

        public GGrid(GuiSection section, int elements)
            : this(section, section.body().width(), elements, section.body().x1(), section.body().y1(), 0)
        {
        }

        public GGrid(GuiSection section, int elements, int y1)
            : this(section, section.body().width(), elements, section.body().x1(), y1, 0)
        {
        }

        public GGrid(GuiSection section, int width, int elements, int x1, int y1, int marginX)
        {
            this.startX = x1 + marginX;
            this.startY = y1;
            this.width = width - marginX * 2;
            this.elements = elements;
            this.section = section;
            this.row = new ArrayList<RENDEROBJ>(elements);
        }

        public GGrid SetStartY(int y1)
        {
            this.startY = y1;
            return this;
        }

        public GGrid SetAlignment(DIR align)
        {
            this.align = align;
            return this;
        }

        public void SetTile(RENDEROBJ o)
        {
            o.body().moveY2(startY - 2 * C.SG);
            o.body().moveCX(startX + width / 2);
            section.add(o);
        }

        public void Centered(RENDEROBJ o)
        {
            NL(8);
            o.body().moveY1(startY + 4 * C.SG);
            o.body().moveCX(startX + width / 2);
            section.add(o);
            count = 0;
            row.clear();
            maxHeight = 0;
            this.startY = section.getLastY2();
        }

        public void Add(RENDEROBJ o)
        {
            if (o.body().height() > maxHeight)
            {
                maxHeight = o.body().height();
                int x = 0;
                foreach (RENDEROBJ r in row)
                {
                    if (r != null)
                    {
                        Align(x++, r);
                    }
                }
            }

            Align(count, o);
            row.add(o);
            section.add(o);

            count++;
            if (count == row.max())
                NL();
        }

        private void Align(int row, RENDEROBJ o)
        {
            int dw = width / elements;
            int x1 = startX + (row % elements) * dw;
            int y1 = startY;
            int dx = (int)Math.Ceiling((dw - o.body().width()) / 2.0);
            int dy = (maxHeight - o.body().height()) / 2;

            int cx = x1 + dw / 2;
            int cy = y1 + maxHeight / 2;

            cx += align.x() * dx;
            cy += align.y() * dy;

            o.body().moveC(cx, cy);
        }

        public void Skip()
        {
            if (count == 0)
                return;
            count++;
            if (count == row.max())
                NL();
        }

        public void Add(SPRITE s)
        {
            Add(new RENDEROBJ.Sprite(s));
        }

        public int Sx(int i)
        {
            int dw = width / elements;
            return startX + (i % elements) * dw;
        }

        public void NL()
        {
            startY += maxHeight + marginY;
            count = 0;
            row.clear();
            maxHeight = 0;
        }

        public void NL(int margin)
        {
            startY += maxHeight + margin;
            count = 0;
            row.clear();
            maxHeight = 0;
        }

        public GGrid WidthSet(int width)
        {
            this.width = width;
            return this;
        }

        public GGrid SetMarginY(int y)
        {
            this.marginY = y;
            return this;
        }

        public GGrid IncStartX(int x)
        {
            this.startX += x;
            return this;
        }
    }
}