using System;
using init.type;
using settlement.entity.humanoid;
using settlement.main;
using settlement.path.components;
using settlement.path.path;
using settlement.room.home;
using settlement.room.home.house;
using settlement.room.main;
using settlement.room.main.throne;
using settlement.stats;
using snake2d.util.datatypes;

namespace settlement.path.finders
{
    public class SFinderHome : SFINDER
    {
        private Coo current = new Coo();
        private HGROUP type;

        public SFinderHome()
        {
            // TODO Auto-generated constructor stub
        }

        public bool findHome(Humanoid h, SPath path)
        {
            if (h.indu().clas() != HCLASSES.NOBLE() && STATS.WORK().EMPLOYED.get(h) == null)
            {
                HOME home = STATS.HOME().GETTER.get(h, this);
                if (home != null)
                {
                    int sx = home.serviceX();
                    int sy = home.serviceY();
                    return path.requestFull(h.tc(), sx, sy);
                }
                HOME oddJob = SETT.ROOMS().HOME.odd.get(h, this);
                if (oddJob == null)
                    return false;
                STATS.HOME().GETTER.set(h, oddJob);
                int sx = oddJob.serviceX();
                int sy = oddJob.serviceY();
                return path.requestFull(h.tc(), sx, sy);
            }

            {
                HOME home = STATS.HOME().GETTER.get(h, this);
                if (home != null)
                {
                    if (home.is(h.tc().x(), h.tc().y()))
                    {
                        return path.requestFull(h.tc(), h.tc());
                    }
                    int sx = home.serviceX();
                    int sy = home.serviceY();
                    if (STATS.WORK().EMPLOYED.get(h) == null)
                    {
                        return path.requestFull(h.tc(), sx, sy);
                    }
                    current.set(sx, sy);
                }
                else
                {
                    current.set(-1, -1);
                }
            }

            type = HGROUP.get(h);

            if (findP(h, path))
            {
                HOME old = STATS.HOME().GETTER.get(h, this);
                HOME n = SETT.ROOMS().HOME.service.get(path.destX(), path.destY());
                if (old != null)
                {
                    if (path.destX() == old.serviceX() && path.destY() == old.serviceY())
                    {
                        return true;
                    }
                    else
                    {
                        STATS.HOME().GETTER.set(h, n);
                    }
                }
                else
                {
                    STATS.HOME().GETTER.set(h, n);
                }
                return true;
            }

            return false;
        }

        private bool findP(Humanoid h, SPath path)
        {
            RoomInstance ins = STATS.WORK().EMPLOYED.get(h);
            if (ins == null)
            {
                if (h.indu().clas() == HCLASSES.NOBLE())
                    return path.request(THRONE.coo().x(), THRONE.coo().y(), this, int.MaxValue);
                else
                    return path.request(h.tc().x(), h.tc().y(), this, int.MaxValue);
            }
            COORDINATE c = SETT.PATH().finders.finder().findDest(ins, this, 200);
            if (c != null)
                return path.requestFull(h.tc(), c);

            return false;
        }

        public bool isInComponent(SComponent c, double distance)
        {
            if (SETT.PATH().comps.data.home.has(c, type))
                return true;
            if (c.is(current))
                return true;
            return false;
        }

        public bool isTile(int tx, int ty, int tileNr)
        {
            if (current.isSameAs(tx, ty))
                return true;

            HomeInstance home = SETT.ROOMS().HOME.service.get(tx, ty);
            if (home == null)
                return false;
            HTypeBits s = home.availability();
            if (s != null && s.is(type))
                return true;
            return false;
        }
    }
}