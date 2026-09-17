using System;
using System.IO;

namespace Util.SpriteComposer
{
    public class ComposerFonter
    {
        private readonly ComposerUtil c;
        private readonly int TRANS = 0x00000000;
        private readonly int GREEN = 0x00FF00FF;

        public ComposerFonter(ComposerUtil c)
        {
            this.c = c;
        }

        public Font Save(int x1, int y1, int trail)
        {
            FilePutter p = Resources.P;

            Source s = new Source(c.Source);
            Tile dest = GetDest(s.Height);
            Dest d = new Dest(s, dest);

            FontGlyph[] ggs = new FontGlyph[Font.Glyps];
            for (int i = 0; i < ggs.Length; i++)
                ggs[i] = new FontGlyph();

            int maxX = dest.StartX + dest.TilesX * dest.Size;

            int hh = 0;

            for (int i = 0; i < Font.Glyps; i++)
            {
                s.Set(i);

                int width = s.Width;

                if ((d.x1 + width) >= maxX)
                {
                    d.x1 = dest.StartX;
                    d.y1 += dest.Size;
                }

                ggs[i].Width = (short)s.Width;
                ggs[i].Ty1 = (short)d.y1;
                ggs[i].Tx1 = (short)d.x1;

                SetDescent(ggs[i], s);
                if (char.IsLetter(Font.Charset[i]))
                    SetCorners(ggs[i], s);

                SetTrail(ggs[i], s);

                hh = Math.Max(hh, s.hh);

                c.Copy(s);
                c.Paste(d);
                d.x1 += width;
            }

            foreach (FontGlyph g in ggs)
            {
                g.Descent -= s.Height - hh;
            }

            int tStart = dest.x1() / dest.Size;
            int tEnd = d.x1() / dest.Size + dest.TilesX * (d.y1 - dest.Y1()) / dest.Size;
            tEnd += d.x1() % dest.Size != 0 ? 1 : 0;
            dest.Skip(tEnd - tStart);

            p.Mark("font");
            p.I(s.Height);
            p.I(d.d.Size);
            for (int i = 0; i < Font.Glyps; i++)
            {
                ggs[i].Save(p);
            }

            return new Font(ggs, s.Height(), 1.0f, trail);
        }

        private Tile GetDest(int h)
        {
            if (h <= Resources.Dests.S16.Size)
                return Resources.Dests.S16;
            if (h <= Resources.Dests.S24.Size)
                return Resources.Dests.S24;
            if (h <= Resources.Dests.S32.Size)
                return Resources.Dests.S32;
            throw new Errors.DataError("Unable to create font. Font height too big: " + h, c.SourcePath);
        }

        private void SetDescent(FontGlyph g, Source d)
        {
            int dd = 0;
            for (int y = 0; y < d.Height; y++)
            {
                if (d.im.rgb.Get(d.x1 - 1, y + d.y1) != GREEN)
                    dd++;
            }

            g.Descent = (short)dd;
        }

        private void SetTrail(FontGlyph g, Source s)
        {
            for (int y = 0; y < s.Height; y++)
            {
                int rgb = s.im.rgb.Get(s.x1 + s.width - 1, y + s.y1);
                rgb &= 0x0FF;
                if (rgb > 32)
                {
                    g.Trail = (short)Math.Max(g.Trail, 1);
                    return;
                }
            }
        }

        private void SetCorners(FontGlyph g, Source d)
        {
            {
                int off = d.width / 2;
                outer:
                for (int x = 0; x < d.width; x++)
                {
                    for (int y = 0; y < d.height / 2 - g.Descent; y++)
                    {
                        if (d.im.rgb.Get(x + d.x1, y + d.y1) != TRANS)
                        {
                            off = x;
                            break outer;
                        }
                    }
                }
                g.Nw = (byte)CLAMP.I(off - 1, 0, off);
            }

            {
                int off = d.width / 2;
                outer:
                for (int x = 0; x < d.width; x++)
                {
                    for (int y = 0; y < d.height / 2 - g.Descent; y++)
                    {
                        if (d.im.rgb.Get(d.x1 + d.width - 1 - x, y + d.y1) != TRANS)
                        {
                            off = x;
                            break outer;
                        }
                    }
                }
                g.Ne = (byte)CLAMP.I(off - 1, 0, off);
            }

            {
                int off = d.width / 2;
                outer:
                for (int x = 0; x < d.width; x++)
                {
                    for (int y = 0; y < d.height / 2; y++)
                    {
                        int y1 = y + d.height / 2 + d.y1;
                        y1 -= g.Descent;
                        if (y1 < d.y1)
                            break;
                        if (d.im.rgb.Get(x + d.x1, y1) != TRANS)
                        {
                            off = x;
                            break outer;
                        }
                    }
                }
                g.Sw = (byte)CLAMP.I(off - 1, 0, off);
            }

            {
                int off = d.width / 2;
                outer:
                for (int x = 0; x < d.width; x++)
                {
                    for (int y = 0; y < d.height / 2; y++)
                    {
                        int y1 = y + d.height / 2 + d.y1;
                        y1 -= g.Descent;
                        if (y1 < d.y1)
                            break;
                        if (d.im.rgb.Get(d.x1 + d.width - 1 - x, y1) != TRANS)
                        {
                            off = x;
                            break outer;
                        }
                    }
                }
                g.Se = (byte)CLAMP.I(off - 1, 0, off);
            }
        }

        public static Font Get(int trail)
        {
            FileGetter g = Resources.G;
            g.Check("font");
            int h = g.I();
            int dy = Optimizer.Get(g.I()).StartY;

            FontGlyph[] ggs = new FontGlyph[Font.Glyps];
            for (int i = 0; i < ggs.Length; i++)
                ggs[i] = new FontGlyph();
            for (int i = 0; i < Font.Glyps; i++)
            {
                ggs[i].Load(g);
                ggs[i].Ty1 += dy;
            }

            return new Font(ggs, h, 1.0f, trail);
        }

        private class Source : ComposerSources.Source
        {
            private readonly Snake _snake;
            private readonly int _height;

            public Source(Snake snake, int height)
            {
                _snake = snake;
                _height = height;
            }

            public int Height => _height;

            public int Width
            {
                get
                {
                    int width = _height;
                    for (int x = 0; x < _height; x++)
                    {
                        if (_snake.GetPixel(x, 0) != 0)
                        {
                            width = x;
                            break;
                        }
                    }
                    return width;
                }
            }

            public int Hh
            {
                get
                {
                    int hh = _height;
                    for (int y = 0; y < _height; y++)
                    {
                        if (_snake.GetPixel(0, y) != 0)
                        {
                            hh = y;
                            break;
                        }
                    }
                    return hh;
                }
            }
        }

        private class Dest : ComposerDests.Dest
        {
            private int x1, y1;
            private readonly Tile _d;
            private readonly Source _s;

            public Dest(Source s, Tile d)
            {
                _s = s;
                _d = d;
                x1 = d.X1;
                y1 = d.Y1;
            }

            public int X1 => x1;

            public int Y1 => y1;

            public int Width => _s.Width;

            public int Height => _s.Height;

            public void Jump(int i)
            {
                _d.Jump(i);
            }

            public void Dispose()
            {
                // TODO Auto-generated method stub
            }

            public int Size => _d.Size;

            public void DiffuseSet(int x, int y, int c)
            {
                _d.DiffuseSet(x, y, c);
            }

            public int DiffuseGet(int x, int y)
            {
                return _d.DiffuseGet(x, y);
            }

            public void NormalSet(int x, int y, int c)
            {
                _d.NormalSet(x, y, c);
            }

            public int NormalGet(int x, int y)
            {
                return _d.NormalGet(x, y);
            }

            public int DestWidth => _d.DestWidth;
        }
    }
}