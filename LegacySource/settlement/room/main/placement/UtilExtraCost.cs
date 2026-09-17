using System;
using System.Collections.Generic;

namespace Settlement.Room.Main.Placement
{
    public class UtilExtraCost
    {
        private double support = 0;
        private double foundation = 0;
        private int tick = -1;
        private readonly RoomPlacer placer;
        private static readonly TileRayTracer tracer = new TileRayTracer(4);

        public UtilExtraCost(RoomPlacer placer)
        {
            this.placer = placer;
        }

        public double Support()
        {
            Cache();
            return support;
        }

        public double Foundation()
        {
            Cache();
            return foundation;
        }

        public double Total()
        {
            Cache();
            return support + foundation;
        }

        private void Cache()
        {
            if (GAME.UpdateI() == tick)
                return;

            tick = GAME.UpdateI();

            support = Psupport(placer.Instance, placer.Blueprint());
            foundation = Pfoundation(placer.Instance, placer.Blueprint());
        }

        public double Get(int tx, int ty)
        {
            Cache();
            return Math.Clamp(GUTIL.Marker().V1.Get(tx, ty) - 1, 0, 4) / 4.0;
        }

        public bool Is(int tx, int ty)
        {
            return ConstructionData.DExpensive.Is(tx, ty, 1);
        }

        public static double Psupport(ROOMA a, RoomBlueprintImp blueprint)
        {
            if (blueprint.Constructor().MustBeIndoors() && blueprint.Constructor().UsesArea())
            {
                return Support(a) * 2;
            }
            return 0;
        }

        public static double Pfoundation(ROOMA a, RoomBlueprintImp blueprint)
        {
            if (a.Area() == 0)
                return 0;
            if (blueprint.Constructor().IsHeavy())
            {
                double d = 0;
                foreach (COORDINATE c in a.Body())
                {
                    if (a.Is(c))
                    {
                        d += SETT.ENV().Foundation.Get(c);
                    }
                }
                d /= a.Area();
                return Foundation(d);
            }
            return 0;
        }

        public static double Foundation(Room room, int rx, int ry)
        {
            int x1 = room.X1(rx, ry);
            int x2 = x1 + room.Width(rx, ry);
            int y1 = room.Y1(rx, ry);
            int y2 = y1 + room.Height(rx, ry);

            double f = 0;
            double a = 0;

            for (int y = y1; y < y2; y++)
            {
                for (int x = x1; x < x2; x++)
                {
                    if (room.IsSame(rx, ry, x, y))
                    {
                        f += SETT.ENV().Foundation.Get(x, y);
                        a++;
                    }
                }
            }

            if (a > 0)
                f /= a;
            return Foundation(f);
        }

        public static double Foundation(double aveFoundation)
        {
            double d = 0.1 - aveFoundation * 0.2;
            d = Math.Round(100 * d) / 100.0;
            return d;
        }

        private static double Support(ROOMA a)
        {
            GUTIL.Marker().Init(typeof(UtilExtraCost));

            foreach (COORDINATE c in a.Body())
            {
                if (a.Is(c))
                {
                    double v = Support(a, c.X, c.Y);
                    GUTIL.Marker().V1.Set(c, v);
                    GUTIL.Marker().V2.Set(c, v);
                }
            }

            double total = 0;
            double exp = 0;
            double value = 0;

            foreach (COORDINATE c in a.Body())
            {
                if (a.Is(c))
                {
                    double v = GUTIL.Marker().V1.Get(c);
                    total++;
                    if (v >= 1)
                    {
                        ConstructionData.DExpensive.Set(a, c, 0);
                    }
                    else
                    {
                        ConstructionData.DExpensive.Set(a, c, 1);
                        exp++;
                    }
                }
            }

            GUTIL.Marker().Done();

            if (total == 0)
                value = 0;
            else
                value = exp / total;
            value *= 4;
            value = Math.Clamp(value, 0, 1);
            return value;
        }

        private static double Support(AREA a, int tx, int ty)
        {
            double s = 0;

            tracer.CheckInit();

            foreach (Ray r in tracer.Rays())
            {
                for (int i = 0; i < r.Size(); i++)
                {
                    int dx = tx + r.Get(i).X();
                    int dy = ty + r.Get(i).Y();
                    if (!SETT.IN_BOUNDS(dx, dy))
                        break;
                    if (!a.Is(dx, dy))
                    {
                        if (!SETT.ROOMS().Map.Is(dx, dy) && tracer.Check(r.Get(i)))
                        {
                            s += Math.Clamp((double)(3.5 - i) / 3.5, 0, 1);
                        }
                        break;
                    }
                }
            }
            return s;
        }
    }
}