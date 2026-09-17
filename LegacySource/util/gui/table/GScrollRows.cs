using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data.INT;
using util.gui.slider;

namespace util.gui.table
{
    public class GScrollRows
    {
        private readonly RENDEROBJ[] rows;
        private readonly ArrayList<RENDEROBJ> current;
        private readonly GuiSection srows = new GuiSection();
        private readonly GuiSection section = new GuiSection
        {
            render = (r, ds) =>
            {
                if (hoveredIs())
                {
                    double d = MButt.clearWheelSpin();
                    if (d > 0)
                    {
                        first--;
                    }
                    else if (d < 0)
                    {
                        first++;
                    }
                }
                init();
                base.render(r, ds);
            }
        };
        private int first = 0;
        private int last;

        public GScrollRows(IEnumerable<RENDEROBJ> rows, int height)
        {
            this(convert(rows), height, 0);
        }

        public GScrollRows(IEnumerable<RENDEROBJ> rows, int height, int width)
        {
            this(convert(rows), height, width - GSliderVer.WIDTH());
        }

        public GScrollRows(IEnumerable<RENDEROBJ> rows, int height, int width, bool slide)
        {
            this(convert(rows), height, width - GSliderVer.WIDTH(), slide);
        }

        public GScrollRows(RENDEROBJ[] renrows, int height, int width)
        {
            this(renrows, height, width, true);
        }

        public GScrollRows(RENDEROBJ[] renrows, int height, int width, bool slide)
        {
            this.rows = renrows;
            this.current = new ArrayList<RENDEROBJ>(renrows.Length);
            section.body().setHeight(height);
            int w = width;
            foreach (RENDEROBJ r in rows)
            {
                if (r.body().width() > w)
                {
                    w = r.body().width();
                }
            }
            section.body().setWidth(w);
            if (slide)
            {
                GSliderVer slider = new GSliderVer(target, height);
                section.add(slider, section.body().x2(), section.body().y1());
            }
            srows.body().moveX1Y1(section.body());
            section.add(srows);
        }

        private static RENDEROBJ[] convert(IEnumerable<RENDEROBJ> rows)
        {
            int size = 0;
            foreach (RENDEROBJ r in rows)
            {
                size++;
            }
            RENDEROBJ[] rs = new RENDEROBJ[size];
            size = 0;
            foreach (RENDEROBJ r in rows)
            {
                rs[size++] = r;
            }
            return rs;
        }

        public void init()
        {
            current.clearSloppy();

            for (int i = 0; i < rows.Length; i++)
            {
                if (passesFilter(i, rows[i]))
                {
                    current.add(rows[i]);
                }
            }

            int h = 0;
            last = 0;
            for (int i = current.size - 1; i >= 0; i--)
            {
                h += current.get(i).body().height();
                if (h > section.body().height())
                {
                    last = i + 1;
                    break;
                }
            }

            first = CLAMP.i(first, 0, last);
            srows.clear();
            srows.body().moveX1Y1(section.body());

            for (int i = first; i < current.size; i++)
            {
                RENDEROBJ rr = current.get(i);
                if (srows.body().height() + rr.body().height() > section.body().height())
                {
                    break;
                }
                srows.add(rr, srows.body().x1(), srows.getLastY2());
            }
        }

        protected virtual bool passesFilter(int i, RENDEROBJ o)
        {
            return true;
        }

        public CLICKABLE view()
        {
            return section;
        }

        public INTE target = new INTE
        {
            min = () => 0,
            max = () => last,
            get = () => first,
            set = (t) =>
            {
                first = t;
                init();
            }
        };
    }
}