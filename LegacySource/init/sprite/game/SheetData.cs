using System;
using System.Collections.Generic;

namespace Init.Sprite.Game
{
    public sealed class SheetData
    {
        public double FPS = 0;
        private int FPS_INTERVAL = 64;
        public int ShadowLength = 0;
        public int ShadowHeight = 0;
        public bool Rotates = false;
        public readonly List<COLOR> Colors;
        public bool Circular = false;

        private static readonly double RANI = 1.0 / 0xFF;
        private static readonly List<COLOR> onlyWhite = new List<COLOR> { COLOR.WHITE100 };
        private static readonly List<COLOR> shades = new List<COLOR>(48);
        static SheetData()
        {
            for (int i = 0; i < 48; i++)
            {
                int d = i / 3;
                int q = i % 3;
                shades.Add(new ColorImp(127 - d * 2 - 4 * (q & 1), 127 - d * 2 - 4 * ((q >> 1) & 1), 127 - d * 2 - 4 * ((q >> 2) & 1)));
            }
        }

        public static readonly SheetData DUMMY = new SheetData();

        public SheetData()
        {
            Colors = shades;
        }

        public SheetData(Json json)
        {
            Test(json);
            FPS = json.dTry("FPS", 0, 100000, 0);
            FPS_INTERVAL = (int)(64 * json.dTry("FPS_INTERVAL", 0, 1, 1));

            ShadowLength = (int)json.dTry("SHADOW_LENGTH", 0, 100, ShadowLength);
            ShadowHeight = (int)json.dTry("SHADOW_HEIGHT", 0, 100, ShadowHeight);
            Circular = json.bool("CIRCULAR", false);
            Rotates = json.bool("ROTATES", true);
            if (json.Has("COLOR"))
            {
                Colors = new List<COLOR>(ColorImp.Cols(json, "COLOR"));
            }
            else if (json.bool("TINT", true))
            {
                Colors = shades;
            }
            else
                Colors = onlyWhite;
        }

        SheetData(SheetData def, Json json)
        {
            Test(json);
            FPS = json.dTry("FPS", 0, 100000, def.FPS);
            FPS_INTERVAL = (int)(64 * json.dTry("FPS_INTERVAL", 0, 1, def.FPS_INTERVAL / 64.0));
            ShadowLength = (int)json.dTry("SHADOW_LENGTH", 0, 100, def.ShadowLength);
            ShadowHeight = (int)json.dTry("SHADOW_HEIGHT", 0, 100, def.ShadowHeight);
            Circular = json.bool("CIRCULAR", def.Circular);
            Rotates = json.bool("ROTATES", def.Rotates);
            if (json.Has("COLOR"))
            {
                Colors = new List<COLOR>(ColorImp.Cols(json, "COLOR"));
            }
            else if (json.bool("TINT", true))
            {
                Colors = shades;
            }
            else
                Colors = def.Colors;
        }

        private static KeyMap<string> oks;

        private static void Test(Json json)
        {
            if (oks == null)
            {
                oks = new KeyMap<string>();
                oks.Put("FPS", "animation speed");
                oks.Put("FPS_INTERVAL", "animate consistently");
                oks.Put("SHADOW_LENGTH", "length shadow");
                oks.Put("SHADOW_HEIGHT", "height shadow");
                oks.Put("CIRCULAR", "animation circular");
                oks.Put("ROTATES", "rotates");
                oks.Put("COLOR", "color, or colors");
                oks.Put("TINT", "tint sprite randomly");
                oks.Put("FRAMES", "farmes spec");
                oks.Put("OVERWRITE", "special stuff");
                oks.Put("RESOURCES", "special stuff");
            }

            foreach (var k in json.Keys())
            {
                if (!oks.ContainsKey(k))
                {
                    string av = "";
                    foreach (var kk in oks.Keys())
                    {
                        av += kk + " (" + oks.Get(kk) + "), ";
                    }
                    json.Error(k + " is not a valid key in a sprite json. Available: " + av, k);
                }
            }
        }

        public int Frame(int random, double animationSpeed)
        {
            double frame = random >> 8;
            animationSpeed *= FPS;
            if (animationSpeed <= 0)
            {
                return (int)frame;
            }

            frame += TIME.CurrentSecond() * animationSpeed;

            frame += ((random >> 16 & 0x0FF)) * RANI;

            if (FPS_INTERVAL == 64)
                return (int)frame;
            int fi = (((int)frame) + (random >>> 24)) & (64 - 1);
            fi -= FPS_INTERVAL;
            if (fi > 0)
                frame -= fi;

            return (int)frame;
        }

        public COLOR Color(int random)
        {
            return Colors.Get(random);
        }
    }
}