using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.light;
using snake2d.util.misc;
using snake2d.util.sprite.text;

namespace cutscene
{
    internal sealed class Text
    {
        private readonly IEnumerable<CharSequence> rows;
        private readonly Font f = UI.FONT().M;
        private readonly int bheight;
        private readonly AmbientLight light = new AmbientLight();

        public Text(int height, CharSequence tt)
        {
            rows = f.GetRows(tt, 500);
            this.bheight = height;
        }

        public void Render(SPRITE_RENDERER r, int x1, int y11, double d, double blacken)
        {
            double height = f.Height() + 12;

            int virtualRows = (int)Math.Ceiling(bheight / height);
            int realRows = CLAMP.i(rows.Count - 8, 0, rows.Count);

            double y1 = (y11 + bheight) - height * 2 - d * height * (virtualRows + realRows);

            foreach (var row in rows)
            {
                if (y1 > y11 && y1 + height <= y11 + bheight)
                {
                    f.Render(r, row, x1, (int)y1 + 3);

                    double op = Math.Min(Math.Abs(y1 - y11), Math.Abs(y1 + height - (y11 + bheight))) / (height * 2);
                    op *= blacken;
                    if (op < 1)
                    {
                        light.Set(AmbientLight.Strongmoonlight, op);
                        light.Register(x1, x1 + 500, (int)y1, (int)(y1 + height));
                    }
                    else
                    {
                        AmbientLight.Strongmoonlight.Register(x1, x1 + 500, (int)y1, (int)(y1 + height));
                    }
                }
                y1 += height;
            }
        }
    }
}