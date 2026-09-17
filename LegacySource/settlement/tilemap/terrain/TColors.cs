using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.spritecomposer;

namespace settlement.tilemap.terrain
{
    public sealed class TColors
    {
        public readonly Tree tree;
        public readonly Minimap minimap;

        public readonly COLOR waternormal;
        public readonly COLOR waterWinter;

        public TColors() : this(new Json(Path.Combine(PATHS.CONFIG().init.gets("SettColors"))))
        {
        }

        private TColors(Json j)
        {
            tree = new Tree();
            minimap = new Minimap(j);
            j = j.json("WATER");
            waternormal = new ColorImp(j, "NORMAL");
            waterWinter = new ColorImp(j, "WINTER");
        }

        public void Update(double ds)
        {
            tree.Update(ds);
        }

        public void Init()
        {
            tree.Update(4);
        }

        private sealed class Tree
        {
            private readonly ColorImp[] cols = new ColorImp[64];
            public readonly LIST<COLOR> fertile;
            private readonly LIST<COLOR> dry;
            private readonly LIST<COLOR> autumn;
            private readonly LIST<COLOR> winter;

            public Tree() : this(new Json(Path.Combine(PATHS.SPRITE_SETTLEMENT_MAP().get("TreeColors")), 536, 76))
            {
            }

            private Tree(Json j)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    cols[i] = new ColorImp();
                }

                fertile = row(0, j);
                dry = row(1, j);
                autumn = row(2, j);
                winter = row(3, j);
            }

            private LIST<COLOR> row(int row, Json j)
            {
                LIST<COLOR> cc = new ComposerThings.IColorSampler()
                {
                    Next = (i, c, s, d) =>
                    {
                        s.full.setSkip(1, row * 16 + i);
                        return s.full.sample();
                    },
                    Init = (c, s, d) =>
                    {
                        s.full.init(0, 0, 1, 1, 16, 4, d.s16);
                        return 16;
                    }
                }.getHalf();

                List<COLOR> nn = new List<COLOR>(cols.Length);

                for (int i = 0; i < cols.Length; i++)
                {
                    COLOR c = cc.getC(i);
                    if (i >= nn.Count)
                        c = new ColorImp(c).shadeSelf(RND.rFloat1(0.1));
                    nn.Add(c);
                }
                return new LIST<COLOR>(nn);
            }

            private double time = 0;

            public void Update(double ds)
            {
                time -= ds;
                if (time > 0)
                    return;
                time += 2;

                double moist = SETT.WEATHER().moisture.getD();
                if (moist < 0.5)
                {
                    moist = moist / 0.5;
                }
                else
                    moist = 1.0;

                double winter = 1.0 - SETT.WEATHER().growth.getD();
                double autumn = 0;
                if (winter <= 0.5 && SETT.WEATHER().growth.isAutumn())
                {
                    autumn = Math.Pow(winter * 2.0, 0.5);
                    winter = 0;
                }
                else if (winter > 0.5)
                {
                    autumn = SETT.WEATHER().growth.isAutumn() ? 1.0 : 0;
                    winter = (winter - 0.5) * 2.0;
                    moist += winter;
                }
                for (int i = 0; i < cols.Length; i++)
                {
                    Set(i, autumn, winter, 1.0 - moist);
                }
            }

            private readonly ColorImp c1 = new ColorImp();
            private readonly ColorImp c2 = new ColorImp();

            private void Set(int i, double autumn, double winter, double dry)
            {
                c1.interpolate(fertile.get(i), this.autumn.get(i), autumn);
                c2.interpolate(c1, this.winter.get(i), winter);
                cols[i].interpolate(c2, this.dry.get(i), dry);
            }

            public COLOR Get(int ran)
            {
                return cols[ran & 63];
            }

            public COLOR Def()
            {
                return fertile.get(0);
            }

            public COLOR Dry(int ran)
            {
                return dry.getC(ran);
            }

            public COLOR Winter(int ran)
            {
                return winter.getC(ran);
            }
        }

        public sealed class Minimap
        {
            public readonly COLOR tree;
            public readonly COLOR water;
            public readonly COLOR water_deep;
            public readonly COLOR rock;
            public readonly COLOR growable;
            public readonly COLOR mountain;

            private Minimap(Json j)
            {
                j = j.json("MINIMAP");
                tree = new ColorImp(j, "TREE").shadeSelf(2.0);
                water = new ColorImp(j, "WATER").shadeSelf(2.0);
                water_deep = new ColorImp(j, "WATER_DEEP").shadeSelf(2.0);
                rock = new ColorImp(j, "ROCK").shadeSelf(2.0);
                growable = new ColorImp(j, "GROWABLE").shadeSelf(2.0);
                mountain = new ColorImp(j, "MOUNTAIN").shadeSelf(2.0);
            }
        }

        public sealed class Water
        {
            public readonly COLOR normal;
            public readonly COLOR winterMask;

            private Water(Json j)
            {
                j = j.json("WATER");
                normal = new ColorImp(j, "NORMAL");
                winterMask = new ColorImp(j, "WINTER_MASK");
            }
        }
    }
}