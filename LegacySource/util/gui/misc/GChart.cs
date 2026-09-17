using System;
using System.Collections.Generic;
using snake2d;
using util.colors;
using util.info;
using util.statistics;
using util.text;
using snake2d.util.gui.Hoverable;
using snake2d.util.gui;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace util.gui.misc
{
    public class GChart : HoverableAbs
    {
        private const int max = 16;
        private const int M = 1;

        private double[] dividers = new double[max];
        private readonly ArrayList<HISTORY> entries = new ArrayList<HISTORY>(max);

        private COLOR[] colors = new COLOR[max];
        private CharSequence[] lables = new CharSequence[max];
        private int steps;
        private int hoverI = -1;
        private double smallestCycle;
        private TIMECYCLE smallest;
        private CharSequence title;
        private static readonly GText tText = new GText(UI.FONT().M, 128);
        private bool legend = false;

        public GChart()
        {
            body().setDim(300, 100);
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
        {
            if (entries.size() == 0)
                return;

            steps = 0;
            foreach (HISTORY h in entries)
                if (h.historyRecords() > steps)
                    steps = h.historyRecords();
            if (steps == 0)
                return;

            Font f = UI.FONT().M;
            int y1 = body.y1();
            if (title != null)
                y1 += f.height();

            int height = body.y2() - y1;
            if (legend)
                height -= UI.FONT().M.height();

            renderStaples(r, body.x1(), y1, body.width(), height);

            if (legend)
            {
                tText.clear().add(-steps);
                tText.add(' ');
                tText.add(smallest.cycleNames());
                UI.FONT().M.renderC(r, body.cX(), y1 + height, tText);
            }

            if (title != null)
            {
                tText.clear().set(title);
                tText.lablify();
                int w = tText.width();
                int x1 = body.cX() - w / 2;
                tText.render(r, x1, body.y1());

                tText.clear();

                int am = 0;

                for (int i = 0; i < entries.size(); i++)
                    am += entries.get(i).getD() / dividers[i];
                GFORMAT.i(tText, am);
                tText.render(r, x1 + w + 10, body.y1());
            }
        }

        private void renderStaples(SPRITE_RENDERER r, int x1, int y1, int width, int height)
        {
            int dx = (width - (steps - 1) * M) / steps;

            x1 = x1 + (width - (dx + M) * steps) / 2;

            height -= C.SG * 10;

            if (dx <= 0)
                return;

            double biggestValue = 0;
            smallestCycle = Double.MaxValue;
            smallest = null;
            foreach (HISTORY e in entries)
            {
                if (e.time().cycleSeconds() < smallestCycle)
                {
                    smallestCycle = e.time().cycleSeconds();
                    smallest = e.time();
                }
                for (int i = 0; i < e.historyRecords(); i++)
                    if (e.getD(i) > biggestValue)
                        biggestValue = e.getD(i);
            }

            for (int i = steps - 1; i >= 0; i--)
            {
                int x = x1 + ((steps - 1) - i) * (dx + M);
                if (i == hoverI)
                    COLOR.WHITE15WHITE50.render(r, x, x + dx + M, y1, y1 + height);

                for (int ei = 0; ei < entries.size(); ei++)
                {
                    HISTORY e = entries.get(ei);
                    int hi = (int)(i * e.time().cycleSeconds() / smallestCycle);

                    if (hi >= e.historyRecords())
                        continue;

                    double v = e.getD(hi);
                    int h = (int)(height * v / biggestValue);

                    int y = y1 + height - h;

                    if (i != hoverI)
                        OPACITY.O66.bind();
                    ColorImp.TMP.interpolate(colors[ei], COLOR.BLACK, 0.5);
                    if (dx >= 3 && height > 3)
                    {
                        ColorImp.TMP.render(r, x, x + dx, y, y1 + height);
                        colors[ei].render(r, x + 1, x + dx - 1, y + 1, y1 + height - 1);
                    }
                    else if (dx > 0 && height > 0)
                    {
                        ColorImp.TMP.render(r, x, x + dx, y, y1 + height);
                    }

                    OPACITY.unbind();
                }
                x += (dx + M);

                if (i % 10 == 0)
                    COLOR.WHITE100.render(r, x - 2 * C.SG, x + C.SG * 1, y1 + height, y1 + height + C.SG * 10);
                else if (i % 5 == 0)
                    COLOR.WHITE100.render(r, x - 1 * C.SG, x, y1 + height, y1 + height + C.SG * 8);
                else
                    COLOR.WHITE100.render(r, x - 1 * C.SG, x, y1 + height + C.SG, y1 + height + C.SG * 5);
            }

            hoverI = -1;
        }

        public override bool hover(COORDINATE mCoo)
        {
            hoverI = -1;
            if (base.hover(mCoo))
            {
                steps = 0;
                foreach (HISTORY h in entries)
                    if (h.historyRecords() > steps)
                        steps = h.historyRecords();
                if (steps == 0)
                    return true;
                int dx = (body.width() - (steps - 1) * M) / steps;

                int x1 = body.x1() + (body.width() - (dx + M) * steps) / 2;
                if (mCoo.x() - x1 >= 0)
                {
                    int h = (mCoo.x() - x1) / (dx + M);
                    if (h < steps)
                        hoverI = (steps - 1) - h;
                }
                return true;
            }
            return false;
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            if (hoverI < 0)
                return;
            if (smallest == null)
                return;
            hoverInfo(hoverI, smallestCycle, (GBox)text, entries, colors);
        }

        private static CharSequence ¤¤current = "¤Current";

        static GChart()
        {
            D.ts(typeof(GChart));
        }

        protected void hoverInfo(int h, double secondSpan, GBox box, LIST<HISTORY> hs, COLOR[] colors)
        {
            if (h == 0)
            {
                box.add(box.text().add(¤¤current).add(' ').add(smallest.cycleName()));
            }
            else
            {
                box.add(box.text().add('-').add(h).add(' ').add(smallest.cycleNames()));
            }
            box.NL();
            for (int i = 0; i < hs.size(); i++)
            {
                int index = (int)(h * secondSpan / hs.get(i).time().cycleSeconds());
                hoverInfo(index, box, hs.get(i), colors[i], lables[i]);
            }
        }

        protected void hoverInfo(int back, GBox box, HISTORY hs, COLOR color, CharSequence label)
        {
            int index = back;
            if (index < hs.historyRecords())
            {
                if (label != null)
                    box.add(box.text().color(color).add(label));
                box.add(box.text().color(color).add(hs.getD(index)));
            }
            box.NL();
        }

        public void clear()
        {
            entries.clear();
        }

        public void add(HISTORY entry, double divider, COLOR color, CharSequence label)
        {
            dividers[entries.size()] = divider;
            colors[entries.size()] = color;
            lables[entries.size()] = label;
            entries.add(entry);
        }

        public void add(HISTORY entry)
        {
            add(entry, 1, COLOR.WHITE, "");
        }

        public void title(CharSequence title)
        {
            this.title = title;
        }

        public GChart legend()
        {
            legend = true;
            return this;
        }

        public SPRITE sprite(HISTORY h)
        {
            clear();
            add(h);
            return sprite;
        }

        public readonly SPRITE sprite = new SPRITE()
        {
            public override int width()
            {
                return body().width();
            }

            public override int height()
            {
                return body().height();
            }

            public override void renderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
            {
                // TODO Auto-generated method stub
            }

            public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                body().moveX1Y1(X1, Y1);
                GChart.this.render(r, 0, false);
            }
        };
    }
}