using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace world.map.terrain
{
    class GeneratorRiver
    {
        private static int dX;
        private static int dY;
        private static int trials = 0;
        private static int maxLength;
        private static bool fromOcean = false;

        public GeneratorRiver()
        {
            largeRivers();
            smallRivers();
        }

        private void smallRivers()
        {
            LinkedList<Coo> coo = new LinkedList<Coo>();

            for (int y = 0; y < THEIGHT(); y++)
            {
                for (int x = 0; x < TWIDTH(); x++)
                {
                    if (smallStart(x, y) != null)
                    {
                        coo.Add(new Coo(x, y));
                    }
                }
            }

            coo.Shuffle();

            double am = 200 * WORLD.TAREA() / (224.0 * 224.0);

            while (am-- > 0 && coo.Count > 0)
            {
                Coo c = coo.RemoveFirst();

                if (smallRiver(c.x(), c.y()))
                    ;
            }
        }

        private DIR smallStart(int tx, int ty)
        {
            if (!WORLD.WATER().is(tx, ty))
                return null;

            int ri = RND.rInt(DIR.ORTHO.size());

            for (int i = 0; i < DIR.ORTHO.size(); i++)
            {
                DIR d = DIR.ORTHO.getC(ri + i);

                if (!IN_BOUNDS(tx, ty, d))
                    continue;
                if (!MOUNTAIN().is(tx + d.x(), ty + d.y()) && WATER().get(tx, ty, d) == WATER().NOTHING)
                    return d;
            }
            return null;
        }

        private bool smallRiver(int x, int y)
        {
            if (!IN_BOUNDS(x, y))
                return false;

            if (MOUNTAIN().is(x, y))
                return false;

            if (WATER().RIVER.is(x, y))
                return true;

            WATER().RIVER.placeRaw(x, y);

            DIR dir = smallStart(x, y);

            if (dir == null)
                return false;

            return smallRiver(x + dir.x(), y + dir.y());
        }

        private static int largeRivers()
        {
            LinkedList<Coo> coo = new LinkedList<Coo>();

            for (int y = 0; y < THEIGHT(); y++)
            {
                for (int x = 0; x < TWIDTH(); x++)
                {
                    if (WATER().is(x, y))
                    {
                        coo.Add(new Coo(x, y));
                    }
                }
            }

            coo.Shuffle();

            double am = 10 * WORLD.TAREA() / (224.0 * 224.0);

            while (am-- > 0 && coo.Count > 0)
            {
                Coo c = coo.RemoveFirst();

                largeRiver(c.x(), c.y());
            }

            return 0;
        }

        private static void largeRiver(int x, int y)
        {
            if (!IN_BOUNDS(x, y))
                return;

            if (MOUNTAIN().is(x, y))
                return;

            if (WATER().RIVER.is(x, y))
                return;

            WATER().RIVER.placeRaw(x, y);

            int[] dirs = { 0, 1, 2, 3 };

            dirs.Shuffle();

            foreach (int dir in dirs)
            {
                int nx = x + DIR.ORTHO.getC(dir).x();
                int ny = y + DIR.ORTHO.getC(dir).y();

                if (IN_BOUNDS(nx, ny) && !MOUNTAIN().is(nx, ny) && !WATER().RIVER.is(nx, ny))
                {
                    largeRiver(nx, ny);
                }
            }
        }

        private static bool IN_BOUNDS(int x, int y)
        {
            return x >= 0 && x < TWIDTH() && y >= 0 && y < THEIGHT();
        }

        private static int TWIDTH()
        {
            return WORLD.TWIDTH();
        }

        private static int THEIGHT()
        {
            return WORLD.THEIGHT();
        }
    }
}