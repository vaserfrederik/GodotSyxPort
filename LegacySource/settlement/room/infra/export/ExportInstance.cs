using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.rendering;
using settlement.room.infra.logistics;
using settlement.room.infra.stockpile;
using settlement.room.main;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.util;
using game;
using game.faction;
using game.time;
using init.resources;

namespace settlement.room.infra.export
{
    public sealed class ExportInstance : RoomInstance, ROOM_RADIUS_INSTANCE, ROOM_MOVE_DEST, ROOM_MOVEJOBBER, MoveOrderPullInstance
    {
        private byte resourceI;
        private const long serialVersionUID = 1L;
        private const int crateMax = 500;
        private readonly short crates;
        private int amount = 0;
        private int amountReserved = 0;
        private int spaceReserved = 0;
        private bool auto = true;

        public const int ORDERS = 4;
        private readonly MoveOrderPull[] orders = new MoveOrderPull[ORDERS];
        private byte coolFetch = -1;

        private short lastCX, lastCY;
        private short ox, oy;
        private byte orderI;
        private bool fetching = true;
        private bool prio = true;
        private byte radius;

        public ExportInstance(ROOM_EXPORT b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            int cc = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    Crate crate = b.crate(c.x(), c.y());
                    if (crate != null)
                        cc++;
                }
            }

            crates = (short)cc;

            employees().maxSet(crates);
            employees().neededSet((int)Math.Ceiling(crates / 20.0));
            activate();
        }

        protected override void loadFix()
        {
            if (resourceI > 0 && RESOURCES.map().loader().get(resourceI - 1) == null)
            {
                amount = 0;
                amountReserved = 0;
                resourceI = 0;
                spaceReserved = 0;
                foreach (COORDINATE c in body())
                {
                    if (is(c))
                    {
                        SETT.ROOMS().data.set(this, c, 0);
                    }
                }
            }
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override void dispose()
        {
            resourceSet(null);
        }

        protected override void updateAction(double ds, bool day)
        {
            if (coolFetch > 0)
            {
                coolFetch--;
            }

            foreach (MoveOrderPull o in orders)
            {
                if (o != null && o.cooldown > 0)
                    o.cooldown--;
            }

            if (!active() || employees().employed() <= 0)
                return;
        }

        public RESOURCE resource()
        {
            if (resourceI == 0)
                return null;
            return RESOURCES.ALL().get(resourceI - 1);
        }

        void resourceSet(RESOURCE r)
        {
            if (r == resource())
                return;

            if (resource() != null)
            {
                foreach (COORDINATE c in body())
                {
                    if (!is(c))
                        continue;
                    Crate crate = blueprintI().crate(c.x(), c.y());
                    if (crate == null)
                        continue;
                    int am = crate.amount();
                    crate.clear();
                    if (am > 0)
                    {
                        foreach (DIR dd in DIR.ORTHO)
                        {
                            if (!PATH().solidity.is(c, dd))
                            {
                                blueprintI().FETCHER.vacate(c.x() + dd.x(), c.y() + dd.y(), resource(), am);
                                break;
                            }
                        }
                    }
                }
                blueprintI().tally.inc(resource(), 0, -crateMax * crates);
            }
            if (amount != 0)
            {
                GAME.Notify(resource().name + " " + this.amount);
                amount = 0;
            }

            resourceI = (byte)(r == null ? 0 : r.index() + 1);
            if (resource() != null)
            {
                blueprintI().tally.inc(resource(), 0, crateMax * crates);
            }
            coolFetch = 0;

            foreach (MoveOrderPull o in orders)
            {
                if (o != null)
                {
                    o.resbits.clear();
                    if (r != null)
                        o.resbits.or(r);
                }
                if (o != null && o.cooldown > 0)
                    o.cooldown = 0;
            }
        }

        protected override void activateAction()
        {
        }

        protected override void deactivateAction()
        {
        }

        public ROOM_EXPORT blueprintI()
        {
            return ROOMS().EXPORT;
        }

        public RoomState makeState(int tx, int ty, bool broken)
        {
            return new State(this, broken);
        }

        private class State : RoomState.RoomStateInstance
        {
            private static readonly long serialVersionUID = 1L;

            private bool fetching;
            private bool prio;
            private readonly bool broken;
            private MoveOrderPull[] orders;
            private readonly int ri;

            public State(ExportInstance ins, bool broken) : base(ins)
            {
                this.broken = broken;
                this.fetching = ins.fetching;
                this.prio = ins.prio;
                this.ri = ins.resourceI;
                this.orders = new MoveOrderPull[ORDERS];
                for (int i = 0; i < ORDERS; i++)
                {
                    this.orders[i] = ins.orders[i];
                }
            }

            public override void applyTo(RoomInstance inst)
            {
                ExportInstance ins = (ExportInstance)inst;
                ins.resourceI = (byte)ri;
                ins.fetching = fetching;
                ins.prio = prio;
                for (int i = 0; i < ORDERS; i++)
                {
                    ins.orders[i] = orders[i];
                }
            }
        }

        public bool fetching()
        {
            return fetching;
        }

        public void fetchingSet(bool f)
        {
            this.fetching = f;
            coolFetch = 0;
        }

        public int radius()
        {
            return (this.radius + 10) * 8;
        }

        public byte radiusRaw()
        {
            return radius;
        }

        public void radiusRawSet(byte r)
        {
            this.radius = r;
        }

        public void updateTileDay(int tx, int ty)
        {
            if (resource() == null)
                return;
            Crate c = blueprintI().crate(tx, ty);
            if (c == null)
                return;
            int am = c.amount() - c.reserved();
            if (am <= 0)
                return;
            if (am < 0)
                return;

            double d = am * resource().degradeSpeed() / TIME.years().bitConversion(TIME.days());
            int i = (int)d;
            if (d - i > RND.rFloat())
                i++;
            i = Math.Min(am, i);
            if (i > 0)
            {
                c.amountSet(c.amount() - i);
                FACTIONS.player().res().inc(resource(), RTYPE.SPOILAGE, -i);
            }

            base.updateTileDay(tx, ty);
        }

        public void copyFrom(MoveOrderPullInstance same)
        {
            ExportInstance ins = (ExportInstance)same;
            resourceSet(ins.resource());
            fetchingSet(ins.fetching());
            auto = ins.auto;
            prio = ins.prio;
            radius = ins.radius;
            employees().neededSet(ins.employees().target());
        }

        private bool prio()
        {
            return prio;
        }

        private void prioSet()
        {
            prio = !prio;
            coolFetch = -1;
        }

        public TILE_STORAGE destCrate(RBIT okMask, int minAm, int ox, int oy)
        {
            if (!okMask.has(destSpaceMask()))
                return null;

            if (is(lastCX, lastCY))
            {
                Crate c = blueprintI().crate(lastCX, lastCY);
                if (c != null && c.storageReservable() >= minAm)
                {
                    return c;
                }
            }

            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                Crate crate = blueprintI().crate(c.x(), c.y());
                if (crate != null && crate.storageReservable() >= minAm)
                {
                    lastCX = (short)c.x();
                    lastCY = (short)c.y();
                    return crate;
                }
            }
            return null;
        }

        public TILE_STORAGE storage(int tx, int ty)
        {
            return blueprintI().crate(tx, ty);
        }

        public RBIT destSpaceMask()
        {
            if (resource() == null)
                return RBIT.NONE;
            if (crates * crateMax - amount - spaceReserved <= 0)
                return RBIT.NONE;
            return resource().bit;
        }

        public double storedD(RESOURCE res)
        {
            return (double)(crates * crateMax - amount - spaceReserved) / (crates * crateMax);
        }

        public RBIT moveCapacity()
        {
            return resource() == null ? RBIT.NONE : resource().bit;
        }

        public MoveJob moveJob(Humanoid h, int am)
        {
            if (resource() == null)
                return null;

            if (am <= 0)
                return null;

            int availableSpace = crates * crateMax - amount - spaceReserved;
            if (availableSpace <= 0)
                return null;

            int actualAmount = Math.Min(am, availableSpace);

            MoveJob job = new MoveJob(h, this, actualAmount);
            spaceReserved += actualAmount;
            return job;
        }
    }
}