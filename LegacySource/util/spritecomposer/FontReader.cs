using System;
using System.IO;
using Snake2D.Errors;
using Snake2D.Util.File;
using Snake2D.Util.Sprite.Text;

namespace Util.SpriteComposer
{
    public class FontReader
    {
        private const int GREEN = 0x00FF00FF;
        private readonly int glyphs;

        public FontReader(string charset)
        {
            glyphs = charset.Length;
            Font.SetCharset(charset);
        }

        public Font Get(int x1, int y1, SnakeImage source, Path path, int trail)
        {
            int x = x1 + 1;
            int y = y1;

            if (source.Rgb.Get(x, y) != GREEN)
                throw new DataError("error with font. Expecting full green at pixel: " + x + "," + y, path);

            int height = 0;

            while (true)
            {
                if (y >= source.Height - 1)
                {
                    throw new DataError("unable to find height of font. Make sure surrounding edges are full green!", path);
                }
                y++;
                int col = source.Rgb.Get(x, y);
                if (col == GREEN)
                {
                    height = y - y1 - 1;
                    break;
                }
            }

            FontGlyph[] ggs = new FontGlyph[glyphs];
            for (int i = 0; i < ggs.Length; i++)
                ggs[i] = new FontGlyph();

            for (int i = 0; i < glyphs; i++)
            {
                ggs[i].Ty1 = (short)(y1 + 1);
                ggs[i].Tx1 = (short)x;
                ggs[i].Width = GetWidth(x, y1 + 1, source, path.ToString(), i);
                x += ggs[i].Width + 1;
            }

            return new Font(ggs, height, 1.0, trail);
        }

        private byte GetWidth(int x1, int y1, SnakeImage source, string path, int glyph)
        {
            int w = 0;

            while (source.Rgb.Get(x1, y1) != GREEN)
            {
                if (w > 100)
                    throw new DataError("unable to find width of glyph " + glyph + ". At pixel: " + x1 + "," + y1, path);
                if (x1 >= source.Width)
                    throw new DataError("unable2 to find width of glyph " + glyph + ". At pixel: " + x1 + "," + y1, path);
                w++;
                x1++;
            }

            return (byte)w;
        }
    }
}