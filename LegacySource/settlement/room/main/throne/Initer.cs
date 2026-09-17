using settlement.main;
using util;

namespace settlement.room.main.throne
{
    public sealed class Initer
    {
        public Initer(THRONE t)
        {
        }

        public bool PlacebleWhole(int x1, int y1, int rot)
        {
            int x2 = x1 + Sprite.Width(rot);
            int y2 = y1 + Sprite.Height(rot);

            for (int y = y1; y < y2; y++)
            {
                for (int x = x1; x < x2; x++)
                {
                    if (!PlacableTile(x, y))
                        return false;
                }
            }
            return true;
        }

        public bool PlacableTile(int tx, int ty)
        {
            if (JOBS().Getter.Is(tx, ty))
                return false;
            if (ROOMS().Map.Is(tx, ty))
                return false;
            if (TERRAIN().TREES.IsTree(tx, ty))
                return true;
            if (!TERRAIN().NADA.Is(tx, ty) && !TERRAIN().Get(tx, ty).RoofIs() && !TERRAIN().Get(tx, ty).Clearing().IsEasilyCleared())
                return false;
            return true;
        }

        public void MarkArround(int tx, int ty)
        {
            int i = 0;
            while (GUTIL.Circle().Radius(i) < 100)
            {
                int x = tx + GUTIL.Circle().Get(i).X();
                int y = ty + GUTIL.Circle().Get(i).Y();

                if (PlacebleWhole(x, y, 0))
                {
                    SETT.ROOMS().THRONE.SetInstance(tx, ty);
                    return;
                }
                i++;
            }

            SETT.ROOMS().THRONE.SetInstance(tx, ty);
            //throw new RuntimeException();
        }

        public void Place(int x1, int y1, int rot)
        {
            new Instance(x1, y1, rot);
        }
    }
}