using System;
using settlement.main;
using game;
using init.resources;
using settlement.misc.util;
using settlement.path.components;
using settlement.path.path;
using settlement.room.main;
using settlement.room.main.throne;
using snake2d.util.datatypes;
using snake2d.util.map;
using snake2d.util.sets;

namespace settlement.path.finders
{
    public sealed class SFinderResourceStorage
    {
        private readonly RBITImp mask = new RBITImp();
        private RESOURCE result;

        public SFinderResourceStorage()
        {
        }

        public void reportPresence(TILE_STORAGE r)
        {
            if (r.storageIsFindable())
                s().reportPresence(r.x(), r.y(), r.resource());
        }

        public void reportAbsence(TILE_STORAGE r)
        {
            if (r.storageIsFindable())
                s().reportAbsence(r.x(), r.y(), r.resource());
        }

        public bool has(int sx, int sy, RESOURCE r)
        {
            return s().has(sx, sy, r.bit);
        }

        public bool has(RESOURCE r)
        {
            return has(THRONE.coo().x(), THRONE.coo().y(), r);
        }

        public bool has(int sx, int sy, RBIT mask)
        {
            return s().has(sx, sy, mask);
        }

        public RBIT hasMask(int sx, int sy)
        {
            return s().bits(sx, sy);
        }

        private FindableDataRes s()
        {
            return PATH().comps.data.storage;
        }

        public bool reserve(COORDINATE start, RESOURCE r, SPath path, int maxdistance)
        {
            return reserve(start, r.bit, path, maxdistance) == r;
        }

        public RESOURCE reserve(COORDINATE start, RBIT resMask, SPath path, int maxdistance)
        {
            if (has(start.x(), start.y(), resMask))
            {
                mask.clearSet(resMask);

                if (path.request(start.x(), start.y(), fin, maxdistance))
                {
                    reserve(path.destX(), path.destY());
                    return result;
                }
            }
            return null;
        }

        private readonly SFINDER fin = new SFINDER()
        {
            public override bool isInComponent(SComponent c, double distance)
            {
                return s().has(c, mask);
            }

            public override bool isTile(int tx, int ty, int tileNr)
            {
                Room res = ROOMS().map.get(tx, ty);
                if (res != null)
                {
                    TILE_STORAGE s = res.storage(tx, ty);
                    if (s != null && s.storageIsFindable() && s.resource() != null && s.storageReservable() > 0 && mask.has(s.resource()))
                    {
                        result = s.resource();
                        return true;
                    }
                }
                return false;
            }
        };

        public COORDINATE reserve(COORDINATE start, RESOURCE r, int maxdistance)
        {
            return reserve(start.x(), start.y(), r, maxdistance);
        }

        public COORDINATE reserve(int sx, int sy, RESOURCE r, int maxdistance)
        {
            mask.clearSet(r.bit);
            if (reserve(sx, sy, r.bit, maxdistance) != null)
                return fres.a;
            return null;
        }

        private readonly TupleImp<COORDINATE, RESOURCE> fres = new TupleImp<COORDINATE, RESOURCE>();

        public Tuple<COORDINATE, RESOURCE> reserve(int sx, int sy, RBIT r, int maxdistance)
        {
            if (!has(sx, sy, r))
                return null;
            mask.clearSet(r);
            COORDINATE c = SETT.PATH().finders.finder().findDest(sx, sy, fin, maxdistance);
            if (c != null)
            {
                reserve(c.x(), c.y());
                fres.a = c;
                fres.b = result;
                return fres;
            }
            return null;
        }

        public bool isReservedAndAvailable(COORDINATE reserved, RESOURCE r)
        {
            return isReservedAndAvailable(reserved.x(), reserved.y(), r.bIndex());
        }

        public bool isReservedAndAvailable(int x, int y, short r)
        {
            Room res = ROOMS().map.get(x, y);
            if (res != null)
            {
                TILE_STORAGE s = res.storage(x, y);
                return s != null && s.storageReserved() > 0 && s.resource() != null && s.resource().bIndex() == r;
            }
            return false;
        }

        public bool isReservedAndAvailable(COORDINATE reserved, byte r)
        {
            return isReservedAndAvailable(reserved.x(), reserved.y(), r);
        }

        public void deposit(COORDINATE reserved, RESOURCE r)
        {
            deposit(reserved.x(), reserved.y(), r.bIndex());
        }

        public void deposit(int x, int y, short r)
        {
            Room res = ROOMS().map.get(x, y);
            if (res != null)
            {
                TILE_STORAGE s = res.storage(x, y);
                if (s != null && s.storageReserved() > 0 && s.resource().bIndex() == r)
                {
                    s.storageDeposit(1);
                }
            }
            else
            {
                GAME.Notify("no resource to pick up at: " + x + " " + y);
            }
        }

        public void cancelReservation(COORDINATE reserved, byte resourceIndex)
        {
            cancelReservation(reserved.x(), reserved.y(), resourceIndex);
        }

        public void cancelReservation(int x, int y, short resourceIndex)
        {
            Room res = ROOMS().map.get(x, y);
            if (res != null)
            {
                TILE_STORAGE s = res.storage(x, y);
                if (s != null && s.storageReserved() > 0 && s.resource().bIndex() == resourceIndex)
                {
                    s.storageUnreserve(1);
                }
            }
        }

        private void reserve(int tx, int ty)
        {
            Room res = ROOMS().map.get(tx, ty);
            if (res != null)
            {
                TILE_STORAGE s = res.storage(tx, ty);
                if (s != null && s.storageReservable() > 0 && s.resource() == result)
                {
                    s.storageReserve(1);
                    return;
                }
            }
            throw new Exception();
        }

        public MAP_OBJECT<TILE_STORAGE> getter = new MAP_OBJECT<TILE_STORAGE>()
        {
            public TILE_STORAGE get(int tile)
            {
                throw new Exception();
            }

            public TILE_STORAGE get(int tx, int ty)
            {
                Room r = SETT.ROOMS().map.get(tx, ty);
                if (r != null)
                    return r.storage(tx, ty);
                return null;
            }
        };
    }
}