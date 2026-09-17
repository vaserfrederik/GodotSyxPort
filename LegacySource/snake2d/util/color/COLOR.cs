using System;
using System.Collections.Generic;

namespace Snake2D.Util.Color
{
    public interface Color : Sprite, RGB
    {
        public static readonly Color White100 = new ColorImp(127, 127, 127);
        public static readonly Color White120 = new ColorImp(150, 150, 150);
        public static readonly Color White150 = new ColorImp(190, 190, 190);
        public static readonly Color White200 = new ColorImp(255, 255, 255);
        public static readonly Color White85 = new ColorImp(109, 109, 109);
        public static readonly Color White65 = new ColorImp(80, 80, 80);
        public static readonly Color White50 = new ColorImp(64, 64, 64);
        public static readonly Color White35 = new ColorImp(45, 45, 45);
        public static readonly Color White30 = new ColorImp(38, 38, 38);
        public static readonly Color White25 = new ColorImp(31, 31, 31);
        public static readonly Color White20 = new ColorImp(26, 26, 26);
        public static readonly Color White15 = new ColorImp(19, 19, 19);
        public static readonly Color White10 = new ColorImp(13, 13, 13);
        public static readonly Color White05 = new ColorImp(7, 7, 7);

        public static readonly Color Brown = new ColorImp(72, 58, 33);
        public static readonly Color Black = new ColorImp(0, 0, 0);
        public static readonly Color Red = new ColorImp(255, 0, 0);
        public static readonly Color Green = new ColorImp(0, 255, 0);
        public static readonly Color Blue = new ColorImp(0, 0, 255);
        public static readonly Color Yellow = new ColorImp(255, 255, 0);
        public static readonly Color Cyan = new ColorImp(0, 255, 255);
        public static readonly Color Magenta = new ColorImp(255, 0, 255);
        public static readonly Color Orange = new ColorImp(255, 165, 0);
        public static readonly Color Purple = new ColorImp(128, 0, 128);
        public static readonly Color Pink = new ColorImp(255, 192, 203);
        public static readonly Color Grey = new ColorImp(128, 128, 128);
        public static readonly Color LightBlue = new ColorImp(173, 216, 230);
        public static readonly Color LightGreen = new ColorImp(144, 238, 144);
        public static readonly Color LightRed = new ColorImp(255, 69, 0);
        public static readonly Color LightYellow = new ColorImp(255, 255, 224);
        public static readonly Color LightCyan = new ColorImp(224, 255, 255);
        public static readonly Color LightMagenta = new ColorImp(255, 182, 193);

        public static readonly Color Green2 = new ColorImp(0, 255, 0);
        public static readonly Color Green3 = new ColorImp(0, 255, 0);
        public static readonly Color Green4 = new ColorImp(0, 255, 0);
        public static readonly Color Green5 = new ColorImp(0, 255, 0);
        public static readonly Color Green6 = new ColorImp(0, 255, 0);
        public static readonly Color Green7 = new ColorImp(0, 255, 0);
        public static readonly Color Green8 = new ColorImp(0, 255, 0);
        public static readonly Color Green9 = new ColorImp(0, 255, 0);
        public static readonly Color Green10 = new ColorImp(0, 255, 0);
        public static readonly Color Green11 = new ColorImp(0, 255, 0);
        public static readonly Color Green12 = new ColorImp(0, 255, 0);

        public static readonly Color[] UniqueColors = new Color[100];

        public void Bind(Shader shader, string name)
        {
            // Implementation
        }

        public void BindFlat(Shader shader, string name)
        {
            // Implementation
        }

        public static Color[] GenerateUnique(int min, int amount, bool ran)
        {
            Color[] cols = new Color[amount];
            int rM = (int)Math.Pow(amount, 1.0 / 3.0);
            int gM = rM;
            int bM = (int)Math.Ceiling((double)amount / (rM * gM));

            double delta = 127 - min;

            double rD = delta / rM;
            double gD = delta / gM;
            double bD = delta / bM;

            int inIndex = 0;
            outer:
            for (int r = 0; r < rM; r++)
            {
                for (int g = 0; g < gM; g++)
                {
                    for (int b = 0; b < bM; b++)
                    {
                        if (inIndex >= cols.Length)
                            break outer;
                        ColorImp i = new ColorImp();
                        i.SetRed(min + (int)(rD / 2 + r * rD));
                        i.SetGreen(min + (int)(gD / 2 + g * gD));
                        i.SetBlue(min + (int)(bD / 2 + b * bD));
                        cols[inIndex++] = i;
                    }
                }
            }

            for (int i = 0; i < cols.Length; i++)
            {
                int k = RND.RInt(cols.Length);
                Color n = cols[i];
                cols[i] = cols[k];
                cols[k] = n;
            }

            return cols;
        }

        public static Color[] GenerateUnique2(int min, int amount, bool ran)
        {
            int MAX = amount;
            Color[] cols = new Color[MAX];
            int am = 0;
            int div = 2;
            double delta = 127 - min;

            while (am < MAX)
            {
                for (int dr = 0; dr < div; dr++)
                {
                    for (int dg = 0; dg < div; dg++)
                    {
                        for (int db = 0; db < div; db++)
                        {
                            if (dr == dg && dr == db)
                            {
                                continue;
                            }
                            int r = (int)(min + dr * delta / (div - 1));
                            int g = (int)(min + dg * delta / (div - 1));
                            int b = (int)(min + db * delta / (div - 1));
                            if (am < MAX)
                            {
                                cols[am] = new ColorImp(r, g, b);
                            }
                            am++;
                        }
                    }
                }
                div++;
            }

            if (ran)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    int k = RND.RInt(cols.Length);
                    Color n = cols[i];
                    cols[i] = cols[k];
                    cols[k] = n;
                }
            }

            return cols;
        }

        public static bool Equals(Color a, Color c)
        {
            if (a == null || c == null)
                return false;
            return c.Red() == a.Red() && c.Green() == a.Green() && c.Blue() == a.Blue();
        }
    }
}