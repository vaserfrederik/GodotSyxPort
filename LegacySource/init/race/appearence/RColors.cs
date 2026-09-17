using System;
using System.Collections.Generic;
using System.Linq;

namespace Init.Race.Appearance
{
    public static class RColors
    {
        public const int BLOOD_MASK = 64 - 1;
        public readonly COLOR blood;

        private static readonly ColorCollection dummy = new ColorCollection(COLOR.WHITE100);
        //public readonly ColorCollection dead;
        private readonly COLOR[][] clothes;
        //private readonly COLOR[][] armour;
        private readonly List<ColorCollection> all;
        public static readonly COLOR grey = new ColorImp(170, 170, 170);
        public static readonly COLOR dead = new ColorImp(128, 128, 150);

        private readonly RMAP<ColorCollection> collection;

        public RColors(Json data)
        {
            clothes = Clothes("COLOR_CLOTHES", data, STATS.EQUIP().CLOTHES.stat().indu().Max(null) + 1, 16);
            //armour = Armour("COLOR_ARMOUR_LEVELS", data, STATS.EQUIP().BATTLEGEAR.stat().indu().Max(null) + 1, 16);
            blood = new ColorImp(data, "COLOR_BLOOD");

            data = data.Json("COLORS");
            all = new List<ColorCollection>(data.Keys.Count);

            var map = new KeyMap<ColorCollection>();

            foreach (var k in data.Keys)
            {
                map.Put(k, new ColorCollection(all, data, k));
            }

            //skin = map.Get("SKIN");
            //hair = map.Get("HAIR");
            //leg = map.Get("LEG");
            //{
            //    COLOR[] deadColors = new COLOR[16];
            //    for (int i = 0; i < deadColors.Length; i++)
            //        deadColors[i] = new ColorImp().Interpolate(skin.Get(i), COLOR.WHITE100, 0.3);
            //    this.dead = new ColorCollection(deadColors);
            //}

            collection = new RMAP<ColorCollection>("COLORS", all);
        }

        public COLOR Clothes(int level, int var)
        {
            return clothes[var & 0x0F][level % clothes[0].Length];
        }

        //public COLOR Armour(int level, int var)
        //{
        //    return armour[var & 0x0F][level % armour[0].Length];
        //}

        //public COLOR Skin(Induvidual indu)
        //{
        //    return skin.Get((int)(indu.Randomness2() >> ((skin.Ran) * 8)));
        //}
        //
        //public COLOR Hair(Induvidual indu)
        //{
        //    if (turngray && indu.HType() == HTYPE.RETIREE)
        //        return grey;
        //    return hair.Get((int)(indu.Randomness2() >> ((hair.Ran) * 8)));
        //}
        //
        //public COLOR Leg(Induvidual indu)
        //{
        //    return leg.Get((int)(indu.Randomness2() >> ((leg.Ran) * 8)));
        //}

        public class ColorCollection : MAPPED
        {
            public static readonly ColorCollection DUMMY = new ColorCollection(COLOR.WHITE100);

            private readonly string key;
            protected readonly COLOR[] colors;
            private readonly int index;
            public readonly int ran;
            public bool TurnsGrayWhenOld;
            public bool TurnsWhiteWhenDead;
            public bool AddsSickColor;
            public readonly DOUBLE_O<Induvidual> StatDerive;

            private ColorCollection(List<ColorCollection> all, Json json, string key)
            {
                this.key = key;
                json = json.Json(key);
                this.index = all.Add(this);
                TurnsGrayWhenOld = json.Bool("TURNS_GRAY_WHEN_OLD", false);
                TurnsWhiteWhenDead = json.Bool("TURNS_WHITE_WHEN_DEAD", false);
                AddsSickColor = json.Bool("TURNS_SICKLY", false);
                ran = index & 15;

                if (json.Has("PICK_BY_STAT"))
                {
                    DOUBLE_O<Induvidual> statDerive = null;
                    STAT s = STATS.STAT(json.Value("PICK_BY_STAT"));
                    if (s != null)
                    {
                        statDerive = s.indu();
                    }
                    this.StatDerive = statDerive;
                }
                else
                {
                    this.StatDerive = null;
                }

                var lcols = ColorImp.Cols(json, "VALUES");
                COLOR[] cols = new COLOR[16];

                int k = 0;
                foreach (var c in lcols)
                {
                    cols[k++] = c;
                }

                if (json.Has("GENERATE_RANDOMIZE"))
                {
                    double d = json.D("GENERATE_RANDOMIZE");
                    for (int i = k; i < 16; i++)
                    {
                        cols[i] = new ColorImp(cols[i % lcols.Count]).Shade(1.0 - d * (i / 16.0));
                    }
                }
                else
                {
                    COLOR[] nn = new COLOR[cols.Length];
                    for (int i = 0; i < cols.Length; i++)
                    {
                        nn[i] = lcols.Get(CLAMP.i(lcols.Count * i / cols.Length, 0, lcols.Count - 1));
                    }
                    cols = nn;
                }

                this.colors = cols;
            }

            private ColorCollection(COLOR color)
            {
                this.key = "";
                index = -1;
                this.colors = new COLOR[16];
                for (int i = 0; i < 16; i++)
                {
                    this.colors[i] = color;
                }
                ran = 0;
                this.StatDerive = null;
            }

            public ColorCollection(COLOR[] color)
            {
                this.key = "";
                index = -1;
                this.colors = color;
                ran = 0;
                this.StatDerive = null;
            }

            public override int Index()
            {
                return index;
            }

            public COLOR Get(int i)
            {
                return colors[MATH.Mod(i, colors.Length)];
            }

            public COLOR Get(Induvidual inDu, bool isDead)
            {
                if (TurnsGrayWhenOld && inDu.HType() == HTYPES.RETIREE())
                    return grey;

                COLOR col = null;

                if (StatDerive != null)
                {
                    col = Get((int)(StatDerive.GetD(inDu) * 15));
                }
                else
                {
                    col = Get((int)(STATS.RAN().Get(inDu, 64) >> (ran * 4)));
                }

                if (TurnsWhiteWhenDead && isDead)
                    return ColorImp.TMP.Interpolate(col, RColors.dead, 0.3);

                if (AddsSickColor)
                {
                    COLOR b = STATS.DISEASE().Color(inDu);
                    if (b != null)
                        return ColorAdd(col, b);
                    b = GAME.EVENT().Color(inDu);
                    if (b != null)
                        return ColorAdd(col, b);
                }

                return col;
            }

            private readonly ColorImp color = new ColorImp();
            private double ci = 1.0 / 127.0;

            private COLOR ColorAdd(COLOR ca, COLOR cb)
            {
                int r = ca.Red() & 0x0FF;
                int r2 = cb.Red() & 0x0FF;
                if (r2 > 127)
                    r += r2 - 127;
                else
                    r *= r2 * ci;
                if (r > 0x0FF)
                    r = 0x0FF;

                int g = ca.Green() & 0x0FF;
                int g2 = cb.Green() & 0x0FF;
                if (g2 > 127)
                    g += g2 - 127;
                else
                    g *= g2 * ci;
                if (g > 0x0FF)
                    g = 0x0FF;

                int b = ca.Blue() & 0x0FF;
                int b2 = cb.Blue() & 0x0FF;
                if (b2 > 127)
                    b += b2 - 127;
                else
                    b *= b2 * ci;
                if (b > 0x0FF)
                    b = 0x0FF;

                color.Set(r, g, b);
                return color;
            }

            public override string Key()
            {
                return key;
            }
        }

        private COLOR[][] Clothes(string key, Json data, int levels, int vars)
        {
            Json[] isJson = data.Jsons(key, 1, vars);
            COLOR[][] cols = new COLOR[vars][];
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i] = new COLOR[levels];
                if (i >= isJson.Length)
                {
                    cols[i][levels - 1] = new ColorImp(isJson[i % isJson.Length]);
                }
                else
                {
                    cols[i][levels - 1] = new ColorImp(isJson[i]);
                }
            }

            for (int v = 0; v < vars; v++)
            {
                for (int s = levels - 2; s >= 0; s--)
                {
                    double d = (s + 1.0) / (levels - 1);
                    cols[v][s] = cols[v][levels - 1].MakeSaturated(d);
                }
            }

            return cols;
        }
    }
}