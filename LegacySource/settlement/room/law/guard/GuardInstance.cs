using System;
using System.Collections.Generic;
using init.constant;
using settlement.main;
using settlement.room.main;
using settlement.room.main.util;
using snake2d;
using snake2d.util.bit;
using snake2d.util.datatypes;
using util.rendering;

namespace settlement.room.law.guard
{
    public sealed class GuardInstance : RoomInstance
    {
        private static readonly long serialVersionUID = 1L;

        private static readonly Bits standOccupied = new Bits(0b01111);
        private static readonly Bit standReserved = new Bit(0b01);

        private bool search = true;
        private float eff = 0;

        private int[] cdata;

        protected GuardInstance(ROOM_GUARD b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                if (SETT.ROOMS().fData.tileData.get(c) == Constructor.codeLight)
                {
                    int off = SETT.ROOMS().fData.tileData.get(c.x() + 1, c.y()) == Constructor.codeLight ? C.TILE_SIZEH - 1 : 0;
                    SETT.LIGHTS().torchBig(c.x(), c.y(), off);
                }
            }

            employees().maxSet((int)b.constructor.guards.get(this));
            employees().neededSet((int)b.constructor.guards.get(this));

            activate();

            blueprintI().finder.report(blueprintI().service.get(this), 1);
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override void activateAction()
        {
        }

        protected override void deactivateAction()
        {
        }

        protected override void dispose()
        {
            if (blueprintI().reporter.available(this))
                blueprintI().finder.report(blueprintI().service.get(this), -1);
        }

        public override ROOM_GUARD blueprintI()
        {
            return (ROOM_GUARD)blueprint();
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            if (day)
                search = true;
            float eff = (float)eff();
            if (Math.Abs(eff - this.eff) > 0.1)
            {
                this.eff = eff;
                foreach (COORDINATE c in body())
                {
                    if (is(c))
                    {
                        SETT.ENV().map.setChanged(c.x(), c.y());
                    }
                }
            }
            base.updateAction(updateInterval, day);
        }

        public bool guardSpot(COORDINATEE planTile, COORDINATE current)
        {
            if (!search)
                return false;
            if (is(current.x(), current.y()) && SETT.ROOMS().fData.tileData.is(current.x(), current.y(), Constructor.codeStand))
            {
                int d = SETT.ROOMS().data.get(current.x(), current.y());
                if (!standReserved.is(d))
                {
                    d = standReserved.set(d);
                    SETT.ROOMS().data.set(this, current.x(), current.y(), d);
                    planTile.set(current);
                    return true;
                }
            }

            int a = body().width() * body().height();
            int tx = body().x1() + RND.rInt(body().width());
            int ty = body().y1() + RND.rInt(body().height());
            while (a-- >= 0)
            {
                if (is(tx, ty) && SETT.ROOMS().fData.tileData.is(tx, ty, Constructor.codeStand))
                {
                    int d = SETT.ROOMS().data.get(tx, ty);
                    if (!standReserved.is(d))
                    {
                        d = standReserved.set(d);
                        SETT.ROOMS().data.set(this, tx, ty, d);
                        planTile.set(tx, ty);
                        return true;
                    }
                }
                tx++;
                if (tx >= body().x2())
                {
                    tx = body().x1();
                    ty++;
                    if (ty >= body().y2())
                    {
                        ty = body().y1();
                    }
                }
            }
            search = false;
            return false;
        }

        public bool hasPotentialSpots()
        {
            return search;
        }

        public override void upgradeSet(int upgrade)
        {
            base.upgradeSet(upgrade);
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    SETT.ENV().map.setChanged(c.x(), c.y());
                }
            }
        }

        public void guardSpotReturn(int tx, int ty)
        {
            search = true;
            if (!is(tx, ty) || !SETT.ROOMS().fData.tileData.is(tx, ty, Constructor.codeStand))
            {
                throw new RuntimeException(is(tx, ty) + " " + SETT.ROOMS().fData.tileData.is(tx, ty, Constructor.codeStand));
            }
            int d = SETT.ROOMS().data.get(tx, ty);
            d = standReserved.clear(d);
            SETT.ROOMS().data.set(this, tx, ty, d);
        }

        public DIR guardDir(int tx, int ty)
        {
            return blueprintI().constructor.gaurdDir(tx, ty);
        }

        public double eff()
        {
            return (1.0 - getDegrade() * 0.5) * (upgrade() + 1.0) * (employees().employed() / (double)employees().max());
        }
    }
}