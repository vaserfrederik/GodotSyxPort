using System;
using System.Collections.Generic;

namespace Settlement.Room.Main.Furnisher
{
    public class FurnisherMinimapColor
    {
        private readonly float[][] bs;
        private readonly COLOR color;
        private readonly ColorImp imp = new ColorImp();

        public FurnisherMinimapColor(Json json)
        {
            if (!json.Has("MINI_COLOR"))
                color = COLOR.WHITE50;
            else
                color = new ColorImp(json, "MINI_COLOR");

            if (!json.Has("MINI_COLOR_PATTERN"))
            {
                bs = new float[][]
                {
                    new float[] { 1.0f },
                };
            }
            else
            {
                string[] ss = json.Texts("MINI_COLOR_PATTERN", 1, 32);
                int l = ss[0].Length;
                bs = new float[ss.Length][];

                for (int si = 0; si < ss.Length; si++)
                {
                    string s = ss[si];
                    if (l != s.Length)
                        json.Error("the pattern must have the same length of all its strings!", "MINI_COLOR_PATTERN");
                    bs[si] = new float[l];
                    for (int i = 0; i < l; i++)
                    {
                        float v = 1.0f;
                        int c = s[i] - '0';
                        if (c >= 0 && c <= 9)
                        {
                            v = (float)(0.5 + 0.5 * c / 9.0);
                        }
                        bs[si][i] = v;
                    }
                }
            }
        }

        public COLOR Get(int tx, int ty)
        {
            Room r = ROOMS().Map.Get(tx, ty);

            int x1 = r.X1(tx, ty);
            int y1 = r.Y1(tx, ty);

            imp.Set(color);
            imp.ShadeSelf(bs[(ty - y1) % bs.Length][(tx - x1) % bs[0].Length]);
            return imp;
        }
    }
}