using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using settlement.main;
using init.resources;
using settlement.misc.util;
using settlement.room.main;
using settlement.room.main.util;
using util.rendering;

namespace settlement.room.infra.importt
{
    public sealed class ImportInstance : RoomInstance, ROOM_MOVE_SOURCE
    {
        private static readonly long serialVersionUID = 1L;
        public static readonly int crateMax = 600;
        private int amount;
        private int spaceReserved;
        private int amountReserved;
        private byte resource = -1;

        public readonly StorageCrate.StorageData[] sdata;

        public ImportInstance(ROOM_IMPORT p, TmpArea area, RoomInit init) : base(p, area, init)
        {
            sdata = p.crate.make(this);
            Activate();
        }

        protected override void LoadFix()
        {
            if (resource != -1 && RESOURCES.map().loader().get(resource) == null)
            {
                foreach (COORDINATE c in Body())
                {
                    if (Is(c) && BlueprintI().crate.get(c.x(), c.y(), this, sdata) != null)
                    {
                        BlueprintI().crate.disposeSilent();
                    }
                }
            }
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.Render(r, shadowBatch, it);
        }

        public int Amount()
        {
            return amount;
        }

        public int AmoutReserved()
        {
            return amountReserved;
        }

        public int SpaceReserved()
        {
            return spaceReserved;
        }

        public int Capacity()
        {
            return sdata.Length * crateMax;
        }

        void Count(Crate c, int delta)
        {
            amount += delta * c.amount();
            amountReserved += delta * c.reserved();
            spaceReserved += delta * c.reservedSpace();
            BlueprintI().tally.count(c.resource(), delta * c.amount(), delta * c.max(this));
        }

        protected override void Dispose()
        {
            foreach (COORDINATE c in Body())
            {
                if (Is(c) && BlueprintI().crate.get(c.x(), c.y(), this, sdata) != null)
                {
                    BlueprintI().crate.dispose();
                }
            }
            amount = 0;
            spaceReserved = 0;
            amountReserved = 0;
            resource = -1;
        }

        protected override void ActivateAction()
        {
        }

        protected override void DeactivateAction()
        {
        }

        public ROOM_IMPORT BlueprintI()
        {
            return ROOMS().IMPORT;
        }

        void Allocate(RESOURCE res)
        {
            if (res == Resource())
                return;
            Dispose();

            resource = res == null ? (byte)-1 : res.bIndex();
            if (res != null)
            {
                foreach (COORDINATE c in Body())
                {
                    if (BlueprintI().crate.get(c.x(), c.y(), this, sdata) != null)
                    {
                        BlueprintI().crate.resourceSet(res);
                    }
                }
            }
        }

        public RESOURCE_TILE ResourceTile(int tx, int ty)
        {
            return BlueprintI().crate.get(tx, ty, this, sdata);
        }

        public TILE_STORAGE Storage(int tx, int ty)
        {
            return BlueprintI().crate.get(tx, ty, this, sdata);
        }

        public RESOURCE Resource()
        {
            if (resource < 0 || resource >= RESOURCES.ALL().size())
                return null;
            return RESOURCES.ALL().get(resource);
        }

        public RoomState MakeState(int tx, int ty, bool broken)
        {
            return new State(this);
        }

        private class State : RoomStateInstance
        {
            private static readonly long serialVersionUID = 1L;
            private readonly int ri;

            public State(ImportInstance ins) : base(ins)
            {
                this.ri = ins.resource;
            }

            protected override void ApplyIns(RoomInstance ins)
            {
                base.ApplyIns(ins);
                if (ri != -1)
                    ((ImportInstance)ins).Allocate(RESOURCES.ALL().get(ri));
            }
        }

        public RESOURCE_TILE SourceCrate(RBIT okMask, int minAmount, int ox, int oy, double lim)
        {
            if (Resource() == null || SourceAmountMask().isClear())
                return null;

            if (!okMask.Has(Resource()))
                return null;

            if (lim > StoredD(Resource()))
                return null;

            double am = Amount() - AmoutReserved() - minAmount;
            if (am <= 0)
                return null;

            if (Is(ox, oy))
            {
                RESOURCE_TILE s = BlueprintI().crate.get(ox, oy, this, this.sdata);
                if (s != null && s.reservable() >= minAmount)
                    return s;
            }
            foreach (COORDINATE c in Body())
            {
                StorageCrate s = BlueprintI().crate.get(c.x(), c.y(), this, sdata);
                if (s != null && s.reservable() >= minAmount)
                    return s;
            }

            if (minAmount == 1)
                LOG.ln("Weird indeed");

            return null;
        }

        public RBIT SourceAmountMask()
        {
            if (Amount() - AmoutReserved() > 0)
                return MoveCapacity();
            return RBIT.NONE;
        }

        public RBIT MoveCapacity()
        {
            return Resource() == null ? RBIT.NONE : Resource().bit;
        }

        public int MoveCapacityAm(RESOURCE res)
        {
            if (Resource() == res)
                return Capacity();
            return 0;
        }

        public double StoredD(RESOURCE res)
        {
            if (res == Resource())
                return (double)(Amount() - AmoutReserved()) / Capacity();
            return 0;
        }
    }
}