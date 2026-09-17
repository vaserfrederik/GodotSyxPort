using System;
using System.Collections.Generic;
using settlement.environment;
using settlement.main;
using settlement.misc.util;
using settlement.room.infra.monument;
using settlement.room.main;
using settlement.room.main.furnisher;
using snake2d.util.datatypes;
using snake2d.util.map;
using snake2d.util.sets;

namespace settlement.environment
{
    public class SettEnvMonument
    {
        private readonly All all;
        private readonly Degrade degrade;
        public MAP_BOOLEAN DEGRADE;

        public SettEnvMonument(LISTE<Updatable> all)
        {
            this.all = new All(all);
            this.degrade = new Degrade(all);
            DEGRADE = degrade.has;
        }

        private static bool IsBlocked(int tx, int ty)
        {
            if (SETT.ROOMS().map.blueprint.get(tx, ty) is ROOM_MONUMENT)
                return false;
            return SETT.LIGHTS().los().get(tx, ty).blocksEnv(tx, ty);
        }

        public void ChangeUpgrade(int tx, int ty)
        {
            SETT.ENV().map.setChanged(tx, ty, all);
        }

        public void ChangeDegrade(int tx, int ty)
        {
            SETT.ENV().map.setChanged(tx, ty, degrade);
        }

        private readonly Bitmap2D extra = new Bitmap2D(new Rec(SettEnvMap.RADIUS * 2 + 10, SettEnvMap.RADIUS * 2 + 10), false);

        public void AddExtra(ROOM_MONUMENT m, FurnisherItem it, int x1, int y1)
        {
            extra.clear();

            EUpdater.traces.CheckInit();

            int ssx = x1 + it.width() / 2;
            int ssy = y1 + it.height() / 2;

            foreach (Ray r in EUpdater.traces.Rays())
            {
                for (int i = 0; i < r.size(); i++)
                {
                    COORDINATE d = r.get(i);
                    int sourceX = ssx;
                    int sourceY = ssy;
                    if ((it.width() & 1) == 0 && d.x() > 0)
                        sourceX--;
                    if ((it.height() & 1) == 0 && d.y() > 0)
                        sourceY--;
                    int dx = d.x() + sourceX;
                    int dy = d.y() + sourceY;

                    if (!SETT.IN_BOUNDS(dx, dy))
                        break;

                    double rad = r.radius(i);

                    if (rad >= m.radius(it))
                        break;

                    if (EUpdater.traces.Check(d))
                    {
                        extra.set(d.x() + sourceX - x1 + SettEnvMap.RADIUS, d.y() + sourceY - y1 + SettEnvMap.RADIUS, true);
                    }
                    if (IsBlocked(dx, dy))
                        break;
                }
            }
        }

        public int Extra(int x1, int y1, int tx, int ty)
        {
            int ex = tx - x1 + SettEnvMap.RADIUS;
            int ey = ty - y1 + SettEnvMap.RADIUS;
            return extra.is(ex, ey) ? 1 : 0;
        }

        private class All : Updatable
        {
            private readonly Bitmap2D has = new Bitmap2D(SETT.TILE_BOUNDS, false);

            public All(LISTE<Updatable> all) : base(all)
            {
            }

            protected override void Update(RECTANGLE bounds, RECTANGLE area)
            {
                foreach (COORDINATE c in area)
                {
                    if (SETT.IN_BOUNDS(c))
                    {
                        has.set(c, false);
                        foreach (ROOM_MONUMENT m in SETT.ROOMS().MONUMENTS.all)
                        {
                            m.mapData.set(c, 0);
                            m.mapUpgrade.set(c, m.mapUpgrade.max());
                        }
                    }
                }

                foreach (COORDINATE c in bounds)
                {
                    Trace2(c, area);
                }
            }

            private void Trace2(COORDINATE source, RECTANGLE area)
            {
                Room ro = SETT.ROOMS().map.get(source);

                if (ro == null)
                    return;

                if (!(ro.blueprint() is ROOM_MONUMENT))
                    return;

                if (!IsCentre(ro, source.x(), source.y(), source.x(), source.y()))
                    return;

                ROOM_MONUMENT m = (ROOM_MONUMENT)ro.blueprint();

                int ra = (int)(m.radius(SETT.ROOMS().fData.item.get(source.x(), source.y())));
                if (ra == 0)
                    return;
                if (!area.holdsPoint(source))
                {
                    if (Math.Abs(area.cX() - source.x()) - ra > RADIUS / 2)
                        return;
                    if (Math.Abs(area.cY() - source.y()) - ra > RADIUS / 2)
                        return;
                }

                int up = ro.upgrade(source.x(), source.y());

                EUpdater.traces.CheckInit();

                foreach (Ray r in EUpdater.rays(source.x(), source.y(), area))
                {
                    for (int i = 0; i < r.size(); i++)
                    {
                        if (r.radius(i) >= ra)
                            break;
                        COORDINATE d = r.get(i);

                        int dx = d.x() + source.x();
                        int dy = d.y() + source.y();
                        if (area.holdsPoint(dx, dy) && EUpdater.traces.Check(d))
                        {
                            if (m.mapUpgrade.get(dx, dy) > up)
                            {
                                m.mapUpgrade.set(dx, dy, up);
                            }
                            if (m.mapData.get(dx, dy) < m.maxEnv())
                                m.mapData.increment(dx, dy, 1);
                            has.set(dx, dy, true);
                        }

                        if (IsBlocked(dx, dy))
                            break;
                    }
                }
            }

            private bool IsCentre(Room ro, int dx, int dy, int sourceX, int sourceY)
            {
                int w = ro.width(dx, dy);
                int h = ro.height(dx, dy);
                int x1 = ro.x1(dx, dy);
                int y1 = ro.y1(dx, dy);
                int cx = x1 + w / 2;
                int cy = y1 + h / 2;

                if ((w & 1) == 0 && sourceX > cx)
                    cx--;
                if ((h & 1) == 0 && sourceY > cy)
                    cy--;
                return dx == cx && dy == cy;
            }

            public override double GetBaseValue(int tx, int ty)
            {
                return SETT.ROOMS().map.blueprintImp.get(tx, ty) is ROOM_MONUMENT ? 1 : 0;
            }

            protected override bool Has(int tx, int ty)
            {
                return has.is(tx, ty);
            }

            protected override void Clear()
            {
                foreach (ROOM_MONUMENT m in SETT.ROOMS().MONUMENTS.all)
                {
                    m.mapData.clear();
                    m.mapUpgrade.clear();
                }
                has.clear();
            }
        }

        private class Degrade : Updatable
        {
            private readonly Bitmap2D has = new Bitmap2D(SETT.TILE_BOUNDS, false);

            public Degrade(LISTE<Updatable> all) : base(all)
            {
            }

            public override double GetBaseValue(int tx, int ty)
            {
                Room r = SETT.ROOMS().map.get(tx, ty);
                if (r != null && r.blueprint() is ROOM_MONUMENT)
                {
                    return r.degrader(tx, ty).isRealDegraded() ? 1 : 0;
                }
                return 0;
            }

            protected override void Update(RECTANGLE bounds, RECTANGLE area)
            {
                foreach (COORDINATE c in area)
                {
                    if (SETT.IN_BOUNDS(c))
                    {
                        has.set(c, false);
                    }
                }

                foreach (COORDINATE c in bounds)
                {
                    int tx = c.x();
                    int ty = c.y();
                    Room r = SETT.ROOMS().map.get(tx, ty);
                    if (r != null && r.blueprint() is ROOM_MONUMENT)
                    {
                        if (r.degrader(tx, ty).isRealDegraded())
                        {
                            Trace(tx, ty, area);
                        }
                    }
                }
            }

            private void Trace(int sourceX, int sourceY, RECTANGLE area)
            {
                EUpdater.traces.CheckInit();

                foreach (Ray r in EUpdater.rays(sourceX, sourceY, area))
                {
                    for (int i = 0; i < r.size(); i++)
                    {
                        COORDINATE d = r.get(i);
                        int dx = d.x() + sourceX;
                        int dy = d.y() + sourceY;

                        if (IsBlocked(dx, dy))
                            break;

                        if (area.holdsPoint(dx, dy) && !IsBlocked(dx, dy))
                        {
                            has.set(dx, dy, true);
                        }
                    }
                }
            }

            protected override bool Has(int tx, int ty)
            {
                return has.is(tx, ty);
            }

            protected override void Clear()
            {
                has.clear();
            }
        }
    }
}