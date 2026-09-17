using settlement.main;
using settlement.path.components;
using settlement.path.path;

namespace settlement.path.finders
{
    public sealed class SFinderEntry
    {
        public SFinderEntry()
        {
        }

        public bool Find(int sx, int sy, SPath path, int max)
        {
            SComponent s = SETT.PATH().comps.superComp.Get(sx, sy);
            if (s == null)
                return false;
            if (s.HasEntry())
            {
                if (path.Request(sx, sy, point, max))
                    return true;
            }
            if (s.HasEdge())
            {
                return path.Request(sx, sy, any, max);
            }
            return false;
        }

        public bool Any(int sx, int sy, SPath path, int max)
        {
            SComponent s = SETT.PATH().comps.superComp.Get(sx, sy);
            if (s == null)
                return false;
            if (s.HasEdge())
            {
                return path.Request(sx, sy, any, max);
            }
            return false;
        }

        public bool AnyHas(int sx, int sy)
        {
            SComponent s = SETT.PATH().comps.superComp.Get(sx, sy);
            if (s == null)
                return false;
            return s.HasEdge();
        }

        private readonly SFINDER point = new SFINDER()
        {
            public override bool IsInComponent(SComponent c, double distance)
            {
                return c.HasEntry();
            }

            public override bool IsTile(int tx, int ty, int tileNr)
            {
                if (SETT.PATH().solidity.Is(tx, ty))
                    return false;
                if (tx == 0 || tx == SETT.TWIDTH - 1 || ty == 0 || ty == SETT.THEIGHT - 1)
                {
                    return SETT.ENTRY().points.map.Is(tx, ty);
                }
                return false;
            }
        };

        private readonly SFINDER any = new SFINDER()
        {
            public override bool IsInComponent(SComponent c, double distance)
            {
                return c.HasEdge();
            }

            public override bool IsTile(int tx, int ty, int tile)
            {
                if (SETT.PATH().solidity.Is(tx, ty))
                    return false;
                return tx == 0 || tx == SETT.TWIDTH - 1 || ty == 0 || ty == SETT.THEIGHT - 1;
            }
        };
    }
}