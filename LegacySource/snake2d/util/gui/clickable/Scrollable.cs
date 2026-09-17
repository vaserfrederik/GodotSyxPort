using System;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;

namespace snake2d.util.gui.clickable
{
    public abstract class Scrollable
    {
        private readonly GuiSection section = new GuiSection
        {
            render = (r, ds) =>
            {
                if (nrOfElements != nrOFEntries())
                {
                    move(itemTop);
                }

                base.render(r, ds);
            },
            hover = mCoo =>
            {
                bool ret = base.hover(mCoo);
                if (hoveredIs())
                {
                    double dw = MButt.clearWheelSpin();
                    if (nrOfElements != nrOFEntries() || (hoveredIs() && dw != 0))
                    {
                        move(itemTop + (int)(-dw));
                    }
                }
                return ret;
            }
        };

        private readonly GuiSection elements = new GuiSection();
        private int itemTop = -1;
        private int nrOfElements = 0;
        private readonly int elementsY;
        private readonly int offY1;
        private readonly int width;
        private readonly ScrollRow[] rows;

        public Scrollable(RENDEROBJ title, params ScrollRow[] rows)
        {
            int h = 0;
            int w = 0;
            foreach (ScrollRow s in rows)
            {
                h += s.body().height();
                if (s.body().width() > w)
                    w = s.body().width();
            }
            this.width = w;
            this.rows = rows;
            int height = h;

            if (width <= 0 || height <= 0)
                throw new RuntimeException(width + " " + height + " " + rows.Length);

            elements.body().setDim(width, height);
            section.add(this.elements);

            if (title != null)
            {
                title.body().centerX(section);
                title.body().moveY2(section.body().y1());
                section.add(title);
                offY1 = title.body().height();
            }
            else
                offY1 = 0;

            nrOfElements = -1;
            itemTop = 0;
        }

        private void move(int first)
        {
            if (first >= nrOFEntries() - elementsY)
                first = nrOFEntries() - elementsY;

            if (first < 0)
                first = 0;

            if (itemTop == first && nrOfElements == nrOFEntries())
                return;

            nrOfElements = nrOFEntries();
            itemTop = first;
            elements.clear();

            int real = itemTop;
            int virtual = 0;

            int y1 = elements.body().y1();

            while (real < nrOFEntries() && virtual < elementsY)
            {
                RENDEROBJ r = getElement(virtual, real);
                real++;
                if (r != null)
                {
                    r.body().moveX1(elements.body().x1());
                    r.body().moveY1(y1);
                    elements.add(r);
                    y1 += r.body().height();
                    virtual++;
                }
            }
            elements.body().moveX1(section.body().x1());
            elements.body().moveY1(section.body().y1() + offY1);
        }

        public abstract int nrOFEntries();

        public RENDEROBJ getElement(int virtual, int real)
        {
            rows[virtual].init(real);
            return rows[virtual];
        }

        public GuiSection getView()
        {
            return section;
        }

        public interface ScrollRow : RENDEROBJ
        {
            void init(int index);

            public class ScrollRowImp : GuiSection, ScrollRow
            {
                public void init(int index)
                {
                    // TODO Auto-generated method stub
                }
            }
        }

        public int min()
        {
            return 0;
        }

        public int max()
        {
            return nrOFEntries() - rows.Length;
        }

        public int get()
        {
            return itemTop;
        }

        public void set(int t)
        {
            move(t);
            nrOfElements = nrOFEntries();
        }
    }
}