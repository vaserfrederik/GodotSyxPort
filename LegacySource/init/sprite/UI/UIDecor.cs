using System;
using System.IO;

namespace Init.Sprite.UI
{
    using Init.Constant;
    using Init.Paths;
    using Snake2D;
    using Snake2D.Util.Color;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Gui;
    using Snake2D.Util.Gui.Renderable;
    using Snake2D.Util.Sprite;
    using Util.Colors;
    using Util.Gui.Misc;
    using Util.SpriteComposer;
    using Util.SpriteComposer.ComposerThings;

    public class UIDecor
    {
        public readonly Sprite TopDecor = new Sprite
        {
            Width = 4 * 32,
            Height = 32,

            RenderTextured = (TextureCoords texture, int X1, int X2, int Y1, int Y2) =>
            {
                // TODO Auto-generated method stub
            },

            Render = (SpriteRenderer r, int X1, int X2, int Y1, int Y2) =>
            {
                GColor.T().H1.Bind();
                for (int i = 0; i < 4; i++)
                    Sheet2.Render(r, i, X1 + 32 * i, Y1);
                Color.Unbind();
            }
        };

        private readonly TileSheet _borderTop = new TileSheet(new ComposerSources
        {
            Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
            {
                s.Full.Init(0, s.Full.Body().Y2(), 1, 1, 10, 1, d.S32);
                s.Full.SetSkip(4, 0);
                s.Full.Paste(true);
                return d.S32.SaveGui();
            }
        });

        private readonly TileSheet _borderBottom = new TileSheet(new ComposerSources
        {
            Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
            {
                s.Full.SetSkip(4, 4);
                s.Full.Paste(true);
                return d.S32.SaveGui();
            }
        });

        private readonly TileSheet _leftRight = new TileSheet(new ComposerSources
        {
            Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
            {
                s.Full.SetSkip(2, 8);
                s.Full.Paste(true);
                return d.S32.SaveGui();
            }
        });

        public readonly TileSheet Slider = new TileSheet(new ComposerSources
        {
            Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
            {
                s.Full.Init(0, s.Full.Body().Y2(), 1, 1, 4, 1, d.S24);
                s.Full.Paste(true);
                s.Full.PasteRotated(1, true);
                return d.S24.SaveGui();
            }
        });

        public readonly Sprite Mouse = ISprite.Gui(new ISpriteData
        {
            Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
            {
                s.Full.Init(0, s.Full.Body().Y2(), 1, 1, 1, 1, d.S24);
                s.Full.SetSkip(1, 0).Paste(true);
                return d.S24.SaveSprite();
            }
        });

        public readonly Sprite MouseHov = ISprite.Gui(new ISpriteData
        {
            Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
            {
                s.Full.Init(s.Full.Body().X2(), s.Full.Body().Y1(), 1, 1, 1, 1, d.S24);
                s.Full.SetSkip(1, 0).Paste(true);
                return d.S24.SaveSprite();
            }
        });

        public readonly Sprite Up = new TileSprite(32, 16, C.SG)
        {
            Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
            {
                s.Full.Init(s.Full.Body().X2(), s.Full.Body().Y1(), 1, 1, 4, 1, d.S16);
                s.Full.SetSkip(2, 0).Paste(true);
                return d.S16.SaveGui();
            }
        };

        public readonly Sprite Down = new TileSprite(32, 16, C.SG)
        {
            Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
            {
                s.Full.SetSkip(2, 2).Paste(true);
                return d.S16.SaveGui();
            }
        };

        private readonly TileSheet Sheet2 = new TileSheet(PATHS.SPRITE_UI().Get("Decor"), 664, 160)
        {
            Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
            {
                s.Full.Init(0, 0, 1, 1, 4, 1, d.S32);
                s.Full.Paste(true);
                return d.S32.SaveGui();
            }
        };

        public Sprite BorderTop(int width)
        {
            return new Adaptive(width, _borderTop);
        }

        public Sprite BorderBottom(int width)
        {
            return new Adaptive(width, _borderBottom);
        }

        public Sprite BorderTop(int width, Color color)
        {
            return new Adaptive(width, _borderTop, color);
        }

        public Sprite BorderBottom(int width, Color color)
        {
            return new Adaptive(width, _borderBottom, color);
        }

        public RenderObj Decorate(CharSequence s)
        {
            return Decorate(s, GColor.T().H1);
        }

        public RenderObj Decorate(CharSequence s, Color color)
        {
            var gui = new Gui();
            gui.Add(new RenderObj(new Text(UI.FONT().H1, s).ToUpper()).SetColor(color));
            return gui;
        }

        public RenderObj GetFancyFrame(int width, int height, CharSequence title)
        {
            var gui = new Gui();
            gui.Add(new RenderObj(new Text(UI.FONT().H1, title).ToUpper()).SetColor(GColor.T().H1));
            gui.Add(new RenderObj(TopDecor));
            return gui;
        }

        private class Frame : Sprite
        {
            private readonly Sprite top;
            private readonly Sprite bottom;
            private readonly int width;
            private readonly int height;

            public Frame(int width, int height) : this(width, height, GColor.T().H1) { }

            public Frame(int width, int height, Color color)
            {
                this.width = width;
                this.height = height + 64;
                top = BorderTop(width, color);
                bottom = BorderBottom(width, color);
            }

            public override int Width => width;

            public override int Height => height;

            public override void Render(SpriteRenderer r, int X1, int X2, int Y1, int Y2)
            {
                top.Render(r, X1, Y1);
                bottom.Render(r, X1, Y2 - 32);
            }

            public override void RenderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
            {
                // TODO Auto-generated method stub
            }
        }

        private class Adaptive : Sprite
        {
            private const int Size = 32;
            private readonly int width;
            private readonly TileSheet sheet;
            private readonly Color color;

            public Adaptive(int width, TileSheet sheet) : this(width, sheet, GColor.T().H1) { }

            public Adaptive(int width, TileSheet sheet, Color color)
            {
                this.width = width;
                this.sheet = sheet;
                this.color = color;
            }

            public override int Width => width;

            public override int Height => Size;

            public override void Render(SpriteRenderer r, int X1, int X2, int Y1, int Y2)
            {
                color.Bind();
                int w = (X2 - X1) - 3 * Size;
                if (w < 0)
                    w = 0;
                w /= 2;
                int dw = w % Size;
                w /= Size;

                sheet.Render(r, 0, X1, Y1);
                X1 += Size;
                for (int i = 0; i < w; i++)
                {
                    sheet.Render(r, 1, X1, Y1);
                    X1 += Size;
                }
                if (dw != 0)
                {
                    sheet.Render(r, 1, X1 - (Size - dw), Y1);
                    X1 += dw;
                }
                sheet.Render(r, 2, X1, Y1);
                X1 += Size;
                for (int i = 0; i < w; i++)
                {
                    sheet.Render(r, 1, X1, Y1);
                    X1 += Size;
                }
                if (dw != 0)
                {
                    sheet.Render(r, 1, X1 - (Size - dw), Y1);
                    X1 += dw;
                }
                sheet.Render(r, 3, X1, Y1);
                Color.Unbind();
            }

            public override void RenderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
            {
                // TODO Auto-generated method stub
            }
        }

        public UIDecor() { }
    }
}