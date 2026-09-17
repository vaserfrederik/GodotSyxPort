using System;
using settlement.room.infra.transport;
using settlement.room.main.util;
using snake2d.util.datatypes;
using snake2d.util.bit;

namespace settlement.room.infra.station
{
    internal sealed class Crate
    {
        public readonly int MAX_AM = ROOM_TRANSPORT.MAX_LOAD;
        public Coo coo = new Coo();
        public StationInstance ins;
        public readonly RoomBits resource = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0000_1111_1111_0000));
        public readonly RoomBits reserved = new RoomBits(coo, new Bits(0b0000_0000_0011_1111_1111_0000_0000_0000));
        public readonly RoomBits stored = new RoomBits(coo, new Bits(0b1111_1111_1100_0000_0000_0000_0000_0000));

        private readonly ROOM_STATION b;

        public Crate(ROOM_STATION b)
        {
            this.b = b;
            if (MAX_AM > stored.max())
                throw new RuntimeException();
        }

        public RESOURCE_TILE Get(int tx, int ty)
        {
            ins = b.Get(tx, ty);
            if (ins == null)
                return null;
            if ((SETT.ROOMS().fData.tileData.Get(tx, ty) & Constructor.BIT_CRATE) != 0)
            {
                coo.Set(tx, ty);
                return tile;
            }
            return null;
        }

        public void Deliver(int am)
        {
            Remove();
            stored.Inc(ins, am);
            Add();
        }

        public void ResourceSet(RESOURCE res)
        {
            RESOURCE old = Resource();

            if (old != null)
            {
                int am = stored.Get();
                if (am > 0)
                {
                    foreach (DIR dd in DIR.ORTHO)
                    {
                        if (!PATH().solidity.Is(coo, dd))
                        {
                            SETT.THINGS().resources.Create(coo.x() + dd.x(), coo.y() + dd.y(), tile.Resource(), am);
                            break;
                        }
                    }
                }
            }

            Remove();
            stored.Set(ins, 0);
            reserved.Set(ins, 0);
            resource.Set(ins, 0);
            if (old != null)
                b.Tally(old).Add(ins.Tally(old), ins);

            if (res != null)
            {
                b.Tally(res).Remove(ins.Tally(res), ins);
                int ri = res == null ? 0 : res.Index() + 1;
                resource.Set(ins, ri);
                Add();
            }
        }

        private readonly RESOURCE_TILE tile = new RESOURCE_TILE
        {
            public int Y => coo.y(),
            public int X => coo.x(),
            public bool FindableReservedIs => Resource() != null && reserved.Get() > 0,
            public bool FindableReservedCanBe => Resource() != null && reserved.Get() < stored.Get(),
            public void FindableReserveCancel
            {
                if (Resource() == null)
                    return;
                Remove();
                reserved.Inc(ins, -1);
                Add();
            },
            public void FindableReserve
            {
                if (Resource() == null)
                    return;
                Remove();
                reserved.Inc(ins, 1);
                Add();
            },
            public void ResourcePickup
            {
                if (Resource() == null)
                    return;
                Remove();
                reserved.Inc(ins, -1);
                stored.Inc(ins, -1);
                Add();
            },
            public RESOURCE Resource => Crate.this.Resource(),
            public int Reservable => stored.Get() - reserved.Get(),
            public int Amount => stored.Get(),
            public bool IsStorage => true,
            public bool IsPrio => false
        };

        public RESOURCE Resource()
        {
            int ri = resource.Get();
            if (ri <= 0)
                return null;
            ri -= 1;
            if (ri >= RESOURCES.ALL().Size())
                return null;
            return RESOURCES.ALL().Get(ri);
        }

        private void Remove()
        {
            RESOURCE res = tile.Resource();
            if (res == null)
                return;
            ins.Tally(res).Remove(res, Crate.this, ins);
            if (tile.FindableReservedCanBe())
                PATH().finders.resource.ReportAbsence(tile);
        }

        private void Add()
        {
            RESOURCE res = tile.Resource();
            if (res == null)
                return;
            ins.Tally(res).Add(res, Crate.this, ins);
            if (tile.FindableReservedCanBe())
                PATH().finders.resource.ReportPresence(tile);
        }
    }
}