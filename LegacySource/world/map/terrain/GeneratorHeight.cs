using System;
using static world.WORLD;

namespace world.map.terrain
{
    class GeneratorHeight
    {
        private readonly Polymap polly = new Polymap(TWIDTH(), THEIGHT(), (int)(60 * (TWIDTH() / 256.0)), 1.0);
        private readonly MAP_BOOLEANE checker;
        private readonly HeightMap height;

        public GeneratorHeight(HeightMap height, WorldGen spec)
        {
            this.height = height;
            polly.CheckInit();
            checker = polly.Checker;
            Rise();

            Sink(height);
        }

        private void Sink(HeightMap height)
        {
            int max = WORLD.TAREA() / 2;

            while (max > 0)
                max -= Sink(height, 1000, 16 + RND.rFloat(32));
        }

        private void Rise()
        {
            int max = WORLD.TAREA() / 4;

            while (max > 0)
                max -= Rise(height, RND.rInt(1000), RND.rInt(16));
        }

        private int Sink(HeightMap height, int maxLength, double radius)
        {
            double sx = RND.rInt(TWIDTH());
            double sy = RND.rInt(THEIGHT());

            VectorImp dir = new VectorImp();
            dir.SetAngle(RND.rFloat() * 10);

            GUTIL.Flooder().Init(this);
            GUTIL.Flooder().PushSloppy((int)sx, (int)sy, 0);

            for (int i = 0; i < maxLength; i++)
            {
                sx += dir.nX();
                sy += dir.nY();
                dir.Rotate(RND.rInt0(20));
                if (!IN_BOUNDS((int)sx, (int)sy))
                {
                    break;
                }
                GUTIL.Flooder().PushSloppy((int)sx, (int)sy, 0);
            }

            int am = 0;

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();
                double v = t.GetValue() / radius;
                if (v >= 1)
                    continue;
                v *= v;
                am++;

                height.Set(t, height.Get(t) * (0.4 + 0.6 * v));
                foreach (DIR d in DIR.ALL)
                {
                    if (IN_BOUNDS(t, d))
                    {
                        GUTIL.Flooder().PushSmaller(t, d, t.GetValue() + d.TileDistance());
                    }
                }
            }

            GUTIL.Flooder().Done();
            return am;
        }

        private int Rise(HeightMap height, int maxLength, double radius)
        {
            polly.CheckInit();

            double sx = RND.rInt(TWIDTH());
            double sy = RND.rInt(THEIGHT());

            VectorImp dir = new VectorImp();
            dir.SetAngle(RND.rFloat() * 10);

            GUTIL.Flooder().Init(this);
            GUTIL.Flooder().PushSloppy((int)sx, (int)sy, 0);

            int l2 = maxLength / 2;

            for (int i = 0; i < maxLength; i++)
            {
                sx += dir.nX();
                sy += dir.nY();
                dir.Rotate(RND.rInt0(15));
                if (!IN_BOUNDS((int)sx, (int)sy))
                {
                    break;
                }
                GUTIL.Flooder().PushSloppy((int)sx, (int)sy, 0);
                checker.Set((int)sx, (int)sy, true);

                int ra = (int)(radius * (l2 - MATH.distanceC(l2, i, maxLength)) / l2);
                if (ra > 0)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        int x = (int)sx + RND.rInt0(ra);
                        int y = (int)sy + RND.rInt0(ra);
                        if (IN_BOUNDS(x, y))
                        {
                            GUTIL.Flooder().PushSloppy(x, y, 0);
                            checker.Set(x, y, true);
                        }
                    }
                }
            }

            int am = 0;

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();

                double v = 0;

                if (checker.Is(t))
                {
                    height.Set(t, height.Get(t) * 20);
                    am++;
                }
                else if (t.GetValue() < 3)
                {
                    height.Set(t, height.Get(t) * (1 + 1.0 - t.GetValue() / 3));
                    //height.Set(t, 1);
                    v = 1;
                }
                else
                {
                    continue;
                }
                am++;

                foreach (DIR d in DIR.ALL)
                {
                    if (IN_BOUNDS(t, d))
                    {
                        GUTIL.Flooder().PushSmaller(t, d, t.GetValue() + v * d.TileDistance());
                    }
                }
            }

            GUTIL.Flooder().Done();
            return am;
        }
    }
}