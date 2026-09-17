using System;
using System.IO;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sprite;
using util.spritecomposer;

namespace init.sprite.UI
{
    public sealed class UIPanels
    {
        public readonly UIPanel thin;
        public readonly UIPanel butt;
        public readonly UIPanel big;
        public TILE_SHEET panelClose;
        public readonly TitleBox[] titleBoxes;

        public UIPanels() : base()
        {
            new ComposerThings.IInit(PATHS.SPRITE_UI().get("Panels"), 504, 84)
            {
                protected override void init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.full.init(0, 0, 4, 1, 3, 3, d.s24);
                    base.init(c, s, d);
                }
            };

            thin = new UIPanel(0, 5, 3);
            butt = new UIPanel(1, 3, 1);
            big = new UIPanel(2, 12, 9);

            titleBoxes = new TitleBox[3];

            {
                TILE_SHEET s = new ITileSheet(PATHS.SPRITE_UI().get("TitleBox"), 312, 140)
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.full.init(0, 0, 1, 1, 3, 1, d.s24);
                        s.full.paste(true);
                        return d.s24.saveGui();
                    }
                }.get();
                titleBoxes[0] = new TitleBoxN(24, s);

                panelClose = new ITileSheet()
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.full.init(s.full.body().x2(), 0, 1, 1, 2, 1, d.s24);
                        s.full.paste(true);
                        return d.s24.saveGui();
                    }
                }.get();

                s = new ITileSheet()
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.full.init(0, s.full.body().y2(), 1, 1, 3, 1, d.s32);
                        s.full.paste(true);
                        return d.s32.saveGui();
                    }
                }.get();
                titleBoxes[1] = new TitleBoxN(32, s);

                final TILE_SHEET ss = new ITileSheet()
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.full.init(0, s.full.body().y2(), 1, 1, 6, 2, d.s24);
                        s.full.paste(true);
                        return d.s24.saveGui();
                    }
                }.get();

                titleBoxes[2] = new TitleBox(48)
                {
                    public override void render(SPRITE_RENDERER r, int x1, int y1, int width)
                    {
                        renderP(r, 0, x1 - height, y1);
                        for (int w = 0; w + height < width; w++)
                        {
                            renderP(r, 1, x1 + w, y1);
                        }
                        renderP(r, 1, x1 + width - height, y1);
                        renderP(r, 2, x1 + width, y1);
                    }

                    private void renderP(SPRITE_RENDERER r, int t, int x1, int y1)
                    {
                        ss.render(r, t * 2, x1, y1);
                        ss.render(r, t * 2 + 1, x1 + 24, y1);
                        ss.render(r, t * 2 + 6, x1, y1 + 24);
                        ss.render(r, t * 2 + 7, x1 + 24, y1 + 24);
                    }
                };
            }
        }

        public TitleBox titleBox(int height)
        {
            foreach (TitleBox b in titleBoxes)
            {
                if (height <= b.height - 8)
                    return b;
            }
            return titleBoxes[titleBoxes.Length - 1];
        }

        public abstract class TitleBox
        {
            public int height;

            protected TitleBox(int height)
            {
                this.height = height;
            }

            public abstract void render(SPRITE_RENDERER r, int x1, int y1, int width);
            public void renderCY(SPRITE_RENDERER r, int x1, int cy, int width)
            {
                render(r, x1, cy - height / 2, width);
            }
        }

        public class TitleBoxN : TitleBox
        {
            private readonly TILE_SHEET sheet;

            private TitleBoxN(int height, TILE_SHEET sheet) : base(height)
            {
                this.sheet = sheet;
            }

            public override void render(SPRITE_RENDERER r, int x1, int y1, int width)
            {
                sheet.render(r, 0, x1 - height, y1);
                for (int w = 0; w + height < width; w++)
                {
                    sheet.render(r, 1, x1 + w, y1);
                }
                sheet.render(r, 1, x1 + width - height, y1);
                sheet.render(r, 2, x1 + width, y1);
            }
        }

        public class UIPanel
        {
            public readonly int margin;
            private readonly int min;
            public readonly int tMid;
            private readonly TILE_SHEET sheet;
            public const int dim = 24;

            private readonly static int[] toBox = Alloc.ii(16);
            static
            {
                toBox[DIR.N.mask() | DIR.W.mask()] = 0;
                toBox[DIR.N.mask()] = 1;
                toBox[DIR.N.mask() | DIR.E.mask()] = 2;
                toBox[DIR.W.mask()] = 3;
                toBox[0] = 4;
                toBox[DIR.E.mask()] = 5;
                toBox[DIR.W.mask() | DIR.S.mask()] = 6;
                toBox[DIR.S.mask()] = 7;
            }

            public UIPanel(int variation, int margin, int tMid)
            {
                this.margin = margin;
                this.min = margin * 2 + tMid;
                this.tMid = tMid;
                this.sheet = new ITileSheet(PATHS.SPRITE_UI().get("Panels"), 504, 84)
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.full.init(variation, 0, 1, 1, 1, 1, d.s24);
                        s.full.paste(true);
                        return d.s24.saveGui();
                    }
                }.get();
            }

            public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2, int margin)
            {
                render(r, X1, X2, Y1, Y2, margin, -1);
            }

            public void render(SPRITE_RENDERER r, RECTANGLE body, int margin)
            {
                render(r, body.x1(), body.x2(), body.y1(), body.y2(), margin);
            }

            public void render(SPRITE_RENDERER r, RECTANGLE body, int margin, DIR d1, DIR d2)
            {
                render(r, body.x1(), body.x2(), body.y1(), body.y2(), margin, d1.mask() | d2.mask());
            }

            public void renderVertical(SPRITE_RENDERER r, int x1, int y1, int height)
            {
                int y2 = y1 + height;
                while (y1 < y2)
                {
                    if (y1 + dim > y2)
                        y1 = y2 - dim;
                    render(r, x1, y1, DIR.W, DIR.W, -1);
                    y1 += dim;
                }
            }

            public void renderHorizontal(SPRITE_RENDERER r, int x1, int x2, int y1)
            {
                while (x1 <= x2 - dim)
                {
                    render(r, x1, y1, DIR.N, DIR.N, -1);
                    x1 += dim;
                }
                render(r, x2 - dim, y1, DIR.N, DIR.N, -1);
            }

            private void render(SPRITE_RENDERER r, int x, int y, DIR d1, DIR d2, int dirMask)
            {
                int m = (d1.mask() & dirMask) | (d2.mask() & dirMask);
                int i = toBox[m & 0x0F];
                sheet.render(r, i, x, y);
            }
        }
    }
}