using System;
using settlement.main;
using init.resources;
using settlement.path.components;
using settlement.path.path;
using settlement.room.main;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace settlement.path.finders
{
    public sealed class SFinderResourceStore
    {
        private Coo result = new Coo();
        private readonly Updater updater = new Updater();
        private static readonly SETT SETT = new SETT();
        private static readonly ROOMS ROOMS = SETT.ROOMS;

        SFinderResourceStore()
        {
            new TestPath("storage", null)
            {
                protected override void place(int sx, int sy, SPath p)
                {
                    TILE_STORAGE j = findAny(sx, sy, int.MaxValue);
                    if (j != null)
                    {
                        LOG.ln(j.x() + " " + j.y());
                        p.request(sx, sy, j);
                    }
                    else
                    {
                        LOG.ln("Nay");
                    }
                }
            };
        }

        void update(double ds)
        {
            updater.update(ds);
        }

        readonly RBITImp resMask = new RBITImp();
        readonly RBITImp storeMask = new RBITImp();

        private FindableDatas d()
        {
            return SETT.PATH().comps.data;
        }

        private readonly SCompPatherExister wierd = new SCompPatherExister()
        {
            public override bool isInComponent(SComponent c, double distance)
            {
                storeMask.Or(d().storage.bits(c));
                resMask.Or(d().resScattered.bits(c));

                return storeMask.Has(resMask);
            }

            public override void init(SComponentLevel l)
            {
                resMask.Clear();
                storeMask.Clear();
            }
        };

        private SFINDER fin = new SFINDER()
        {
            public override bool isInComponent(SComponent c, double distance)
            {
                return resMask.Has(d().storage.bits(c));
            }

            public override bool isTile(int tx, int ty, int tileNr)
            {
                Room r = SETT.ROOMS().map.get(tx, ty);
                if (r != null)
                {
                    TILE_STORAGE result = r.storage(tx, ty);
                    if (result != null && result.storageIsFindable() && result.storageReservable() > 0 && result.resource() != null && resMask.Has(result.resource()))
                    {
                        this.result.set(result);
                        return true;
                    }
                }
                return false;
            }
        };

        /**
         * 
         * @param sx
         * @param sy
         * @param maxDistance
         * @param path
         * @return null if nothing was found. Else a job. If job resource == null, then the path is not set. Otherwise it is set.
         */
        public TILE_STORAGE findAny(int sx, int sy, int maxDistance)
        {
            if (maxDistance == int.MaxValue)
            {
                resMask.clearSet(d().resScattered.bits(sx, sy));
                COORDINATE c = SETT.PATH().finders.finder().findDest(sx, sy, fin, maxDistance);
                if (c != null)
                {
                    return ROOMS().map.get(result).storage(result.x(), result.y());
                }
                return null;
            }

            if (SETT.PATH().comps.pather.exists(sx, sy, wierd, maxDistance))
            {
                if (SETT.PATH().finders.finder().findDest(sx, sy, fin, maxDistance) != null)
                    return ROOMS().map.get(result).storage(result.x(), result.y());
            }
            return null;
        }

        public TILE_STORAGE find(int sx, int sy)
        {
            if (!has(sx, sy))
                return null;

            TILE_STORAGE ss = findAny(sx, sy, 100);
            if (ss == null)
                updater.failShort(sx, sy);
            return ss;
        }

        public bool has(int sx, int sy)
        {
            if (!hasAny(sx, sy))
                return false;

            return updater.tryShort(sx, sy);
        }

        public bool hasAny(int sx, int sy)
        {
            SComponent s = SETT.PATH().comps.superComp.get(sx, sy);
            if (s == null)
                return false;

            return d().resScattered.bits(s).Has(d().storage.bits(s));
        }

        private sealed class Updater
        {
            private readonly Bitmap1D tryShort = new Bitmap1D(short.MaxValue, false);
            private readonly double speed = 1.0 / 64.0;
            double ci = 0;

            public Updater()
            {
            }

            public void update(double ds)
            {
                int old = (int)ci;
                int max = SETT.PATH().comps.levels.get(0).componentsMax();
                if (max <= 0)
                    return;
                ci += ds * max * speed;
                int now = (int)ci;
                int delt = old - now;

                if (ci >= max)
                {
                    ci -= max;
                }

                for (int k = 0; k <= delt; k++)
                {
                    int i = k + old;
                    i %= max;
                    tryShort.set(i, false);
                }
            }

            public bool tryShort(int tx, int ty)
            {
                SComponent c = SETT.PATH().comps.levels.get(0).get(tx, ty);
                if (c == null)
                    return false;
                if (tryShort.get(c.index()))
                    return false;
                return true;
            }

            public void failShort(int tx, int ty)
            {
                SComponent c = SETT.PATH().comps.levels.get(0).get(tx, ty);
                if (c == null)
                    return;
                tryShort.set(c.index(), true);
            }
        }
    }
}