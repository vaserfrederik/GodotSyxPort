using System;
using System.Collections.Generic;
using game;
using game.faction.player;
using init.trade;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.Hoverable;
using snake2d.util.misc;
using util.colors;
using util.data.INT;
using util.gui.misc;
using util.gui.table;
using util.info;

namespace view.ui.economy
{
    public class MainChart : GuiSection
    {
        private IntImp hi;
        private readonly int w;
        private int am = GAME.player().credits().creditsH().historyRecords();

        private int loCredits;
        private double maxin, maxout;

        public MainChart(int height, IntImp hi, int sw) : base(height)
        {
            this.hi = hi;
            this.w = sw;
            addRelBody(4, DIR.S, amount());
            addRelBody(4, DIR.S, new Profits());
            addRelBody(8, DIR.S, new Losses());
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            loCredits = int.MaxValue;
            maxin = 0;
            maxout = 0;
            for (int i = 0; i < am; i++)
            {
                loCredits = Math.Min(loCredits, (int)GAME.player().credits().creditsH().get(i));
                int m = 0;
                int o = 0;

                foreach (CredHistory h in GAME.player().credits().all())
                {
                    m += h.IN.get(i);
                    o += h.OUT.get(i);
                }

                maxin = Math.Max(maxin, m);
                maxout = Math.Max(o, maxout);
            }
            if (loCredits > 1)
                loCredits--;
            base.render(r, ds);
        }

        private GStaples amount()
        {
            GStaples s = new GStaples(am)
            {
                Hover = (box, stapleI) =>
                {
                    // TODO Auto-generated method stub
                },
                Render = (r, ds, isHovered) =>
                {
                    if (hi.get() >= 0)
                    {
                        SetHovered(hi.get());
                    }
                    base.render(r, ds, hoveredIs());
                },
                HoverCheck = (mCoo) =>
                {
                    if (base.hover(mCoo))
                    {
                        hi.set(hoverI());
                        return true;
                    }
                    return false;
                },
                GetValue = (stapleI) =>
                {
                    return CLAMP.d(GAME.player().credits().creditsH().get(am - stapleI - 1), 0, int.MaxValue);
                },
                SetColor = (c, stapleI, value) =>
                {
                    c.set(COLOR.YELLOW100).saturateSelf(0.5);
                }
            };
            s.normalize(true);
            s.body().setWidth(w * am);
            s.body().setHeight(78);
            return s;
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            if (hi.get() >= 0)
            {
                GBox b = (GBox)text;
                int si = am - hi.get() - 1;

                {
                    int ri = 0;
                    foreach (TRADABLE res in TR.ALL())
                    {
                        int a = GAME.player().trade.inExported.history(res).get(si) - GAME.player().trade.outImported.history(res).get(si);
                        if (a != 0)
                        {
                            b.add(res.icon());
                            b.add(GFORMAT.iIncr(b.text(), a));
                            b.space();
                            ri++;
                            if (ri >= 4)
                            {
                                ri = 0;
                                b.NL(8);
                            }
                        }
                    }
                }
            }
            else
                base.hoverInfoGet(text);
        }

        private sealed class Profits : HOVERABLE.HoverableAbs
        {
            public Profits() : base()
            {
                body.setWidth(w * am);
                body().setHeight(112);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                GCOLOR.UI().border().render(r, body(), 1);

                for (int x = 0; x < am; x++)
                {
                    int x1 = body().x1() + w * x;

                    if (x != hi.get())
                    {
                        GCOLOR.UI().bg().render(r, x1, x1 + w, body().y1(), body().y2());
                    }

                    if (maxin == 0)
                        continue;

                    int si = am - x - 1;

                    int y2 = body().y2();
                    foreach (CredHistory h in GAME.player().credits().all())
                    {
                        double d = h.IN.get(si) / maxin;
                        int hig = (int)Math.Ceiling(body().height() * d);
                        ColorImp.TMP.set(COLOR.UNIQUE.getC(h.type.ordinal()));
                        if (x == hi.get())
                        {
                            ColorImp.TMP.shadeSelf(1.5);
                        }
                        else
                        {
                            ColorImp.TMP.shadeSelf(0.5);
                        }
                        ColorImp.TMP.render(r, x1, x1 + w, y2 - hig, y2);

                        if (hig > 1)
                            COLOR.UNIQUE.getC(h.type.ordinal()).render(r, x1 + 1, x1 + w - 1, y2 - hig + 1, y2);
                        if (hig > 0)
                            hig--;
                        y2 -= hig;
                    }
                }
            }

            public override bool hover(COORDINATE mCoo)
            {
                if (base.hover(mCoo))
                {
                    int ii = ((mCoo.x() - body().x1()) / w);
                    if (ii < am)
                        hi.set(ii);
                    return true;
                }
                return false;
            }
        }

        private sealed class Losses : HOVERABLE.HoverableAbs
        {
            public Losses() : base()
            {
                body.setWidth(w * am);
                body().setHeight(112);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                GCOLOR.UI().border().render(r, body(), 1);

                for (int x = 0; x < am; x++)
                {
                    int x1 = body().x1() + w * x;

                    if (x != hi.get())
                    {
                        GCOLOR.UI().bg().render(r, x1, x1 + w, body().y1(), body().y2());
                    }

                    if (maxout == 0)
                        continue;

                    int si = am - x - 1;

                    int y1 = body().y1();
                    foreach (CredHistory h in GAME.player().credits().all())
                    {
                        double d = h.OUT.get(si) / maxout;
                        int hig = (int)Math.Ceiling(body().height() * d);
                        ColorImp.TMP.set(COLOR.UNIQUE.getC(h.type.ordinal()));
                        if (x == hi.get())
                        {
                            ColorImp.TMP.shadeSelf(1.5);
                        }
                        else
                        {
                            ColorImp.TMP.shadeSelf(0.5);
                        }
                        ColorImp.TMP.render(r, x1, x1 + w, y1, y1 + hig);

                        if (hig > 1)
                            COLOR.UNIQUE.getC(h.type.ordinal()).render(r, x1 + 1, x1 + w - 1, y1 - 1, y1 + hig);
                        if (hig > 0)
                            hig--;
                        y1 += hig;
                    }
                }
            }

            public override bool hover(COORDINATE mCoo)
            {
                if (base.hover(mCoo))
                {
                    int ii = ((mCoo.x() - body().x1()) / w);
                    if (ii < am)
                        hi.set(ii);
                    return true;
                }
                return false;
            }
        }
    }
}