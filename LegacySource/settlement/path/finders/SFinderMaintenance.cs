using System;
using settlement.main;
using init.resources;
using settlement.path.components;
using settlement.path.finders;
using settlement.path.path;
using snake2d.util.datatypes;

namespace settlement.path.finders
{
    public sealed class SFinderMaintenance
    {
        private readonly RBITImp tbits = new RBITImp();
        private Coo coo = new Coo();

        public SFinderMaintenance()
        {
        }

        public bool Has(int sx, int sy, RBIT bits)
        {
            return PATH().comps.data.maintenanceRes.Has(sx, sy, bits) ||
                   PATH().comps.data.maintenance.Has(sx, sy);
        }

        public bool Find(RBIT bits, COORDINATE start, SPath path, int maxdistance)
        {
            return Find(bits, start.x(), start.y(), path, maxdistance);
        }

        public bool Find(RBIT bits, int sx, int sy, SPath path, int maxdistance)
        {
            if (Has(sx, sy, bits))
            {
                this.tbits.ClearSet(bits);
                if (path.Request(sx, sy, Finder, maxdistance))
                {
                    coo.Set(path.DestX(), path.DestY());
                    return true;
                }
            }
            return false;
        }

        public COORDINATE Find(RBIT bits, int sx, int sy, int maxdistance)
        {
            this.tbits.ClearSet(bits);
            if (Has(sx, sy, tbits))
            {
                SPathUtilResult r = SETT.PATH().finders.finder().Find(sx, sy, Finder, maxdistance);
                if (r != null)
                {
                    coo.Set(r.destX, r.destY);
                    return coo;
                }
            }
            return null;
        }

        public COORDINATE FindWithin(RBIT bits, int sx, int sy, int maxdistance, int mx, int my)
        {
            if (sx == mx && sy == my)
                return Find(bits, sx, sy, maxdistance);

            if (Has(sx, sy, bits))
            {
                this.tbits.ClearSet(bits);
                RadiusChecker.self.Check(mx, my, maxdistance);
                if (!RadiusChecker.self.Is(sx, sy))
                {
                    sx = mx;
                    sy = my;
                }
                SPathUtilResult r = SETT.PATH().finders.finder().Find(sx, sy, Finder2, maxdistance + 128);
                if (r != null)
                {
                    coo.Set(r.destX, r.destY);
                    return coo;
                }
            }
            return null;
        }

        public RBIT Mask(int sx, int sy)
        {
            return PATH().comps.data.maintenanceRes.Bits(sx, sy);
        }

        private SFINDER Finder = new SFINDER()
        {
            public bool IsInComponent(SComponent c, double distance)
            {
                return PATH().comps.data.maintenanceRes.Has(c, tbits)
                       || PATH().comps.data.maintenance.Get(c) > 0;
            }

            public bool IsTile(int tx, int ty, int tileNr)
            {
                if (SETT.MAINTENANCE().reservable.Is(tx, ty))
                {
                    RESOURCE res = SETT.MAINTENANCE().resource.Get(tx, ty);
                    return res == null || tbits.Has(res);
                }
                return false;
            }
        };

        private SFINDER Finder2 = new SFINDER()
        {
            public bool IsInComponent(SComponent c, double distance)
            {
                return PATH().comps.data.maintenanceRes.Has(c, tbits)
                       || PATH().comps.data.maintenance.Get(c) > 0;
            }

            public bool CanCross(SComponent c)
            {
                return RadiusChecker.self.Is(c);
            }

            public bool IsTile(int tx, int ty, int tileNr)
            {
                if (SETT.MAINTENANCE().reservable.Is(tx, ty))
                {
                    RESOURCE res = SETT.MAINTENANCE().resource.Get(tx, ty);
                    return res == null || tbits.Has(res);
                }
                return false;
            }
        };

        public void Add(int tx, int ty)
        {
            if (SETT.MAINTENANCE().reservable.Is(tx, ty))
            {
                RESOURCE res = SETT.MAINTENANCE().resource.Get(tx, ty);
                if (res != null)
                {
                    PATH().comps.data.maintenanceRes.ReportPresence(tx, ty, res);
                }
                else
                {
                    PATH().comps.data.maintenance.ReportPresence(tx, ty);
                }
            }
        }

        public void Remove(int tx, int ty)
        {
            if (SETT.MAINTENANCE().reservable.Is(tx, ty))
            {
                RESOURCE res = SETT.MAINTENANCE().resource.Get(tx, ty);
                if (res != null)
                {
                    PATH().comps.data.maintenanceRes.ReportAbsence(tx, ty, res);
                }
                else
                {
                    PATH().comps.data.maintenance.ReportAbsence(tx, ty);
                }
            }
        }
    }
}