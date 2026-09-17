using System;
using snake2d.util.datatypes;
using snake2d.util.map;

namespace settlement.misc
{
    public sealed class SettPlacability
    {
        public readonly MAP_BOOLEAN solidityWill = new MAP_BOOLEAN()
        {
            public override bool Is(int tile)
            {
                throw new RuntimeException();
            }

            public override bool Is(int tx, int ty)
            {
                if (SETT.PATH().solidity.Is(tx, ty))
                    return true;
                if (SETT.JOBS().getter.Get(tx, ty) != null && SETT.JOBS().getter.Get(tx, ty).BecomesSolid())
                    return true;
                if (SETT.ROOMS().fData.tile.Get(tx, ty) != null && SETT.ROOMS().fData.tile.Get(tx, ty).IsBlocker())
                    return true;
                return false;
            }
        };

        public readonly MAP_BOOLEAN willBlock = new MAP_BOOLEAN()
        {
            public override bool Is(int tile)
            {
                throw new RuntimeException();
            }

            public override bool Is(int tx, int ty)
            {
                for (int di = 0; di < DIR.ORTHO.Size; di++)
                {
                    DIR d = DIR.ORTHO.Get(di);
                    if (WillBeBlocked(d, tx, ty))
                        return true;
                }
                return false;
            }

            private bool WillBeBlocked(DIR from, int tx, int ty)
            {
                tx += from.X();
                ty += from.Y();
                FurnisherItemTile t = SETT.ROOMS().fData.tile.Get(tx, ty);
                if (t != null && t.MustBeReachable)
                {
                    from = from.Perpendicular();
                    for (int di = 0; di < DIR.ORTHO.Size; di++)
                    {
                        DIR d = DIR.ORTHO.Get(di);
                        if (d == from)
                            continue;
                        if (!SETT.IN_BOUNDS(tx, ty, d))
                            continue;
                        if (!solidityWill.Is(tx, ty, d))
                            return false;
                    }
                    return true;
                }
                return false;
            }
        };

        public bool WillBeBlocked(int tx, int ty, int rx, int ry, MAP_OBJECT<object> dontCareAboutNonNull)
        {
            int dc = 0;
            for (int di = 0; di < DIR.ORTHO.Size; di++)
            {
                DIR d = DIR.ORTHO.Get(di);
                if (dontCareAboutNonNull.Is(rx, ry, d))
                {
                    dc++;
                    continue;
                }
                if (!solidityWill.Is(tx, ty, d))
                    return false;
            }
            return dc != DIR.ORTHO.Size;
        }
    }
}