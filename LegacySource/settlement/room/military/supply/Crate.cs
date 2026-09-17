using System;
using game.audio;
using game.faction;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.main.util;
using snake2d.util.bit;
using snake2d.util.datatypes;
using world.army;

namespace settlement.room.military.supply
{
    internal class Crate
    {
        private const int noRes = 0x0;
        private readonly Coo coo = new Coo();
        private readonly RoomBits bRes = new BB(new Bits(0x000000FF));
        private readonly RoomBits bAmount = new BB(new Bits(0x0000FF00));
        private readonly RoomBits bReservedSpace = new BB(new Bits(0x00FF0000));
        private readonly RoomBits bTot = new BB(new Bits(0x00FFFFFF));
        private readonly RoomBits bAnimals = new RoomBits(coo, new Bits(0x07000000));
        private readonly RoomBits bAnimalsRes = new RoomBits(coo, new Bits(0x08000000));
        private readonly RoomBits bAway = new BB(new Bits(0x10000000));
        private readonly RoomBits bState = new RoomBits(coo, new Bits(0xC0000000));

        private readonly ROOM_SUPPLY b;
        private SupplyInstance ins;

        protected Crate(ROOM_SUPPLY b)
        {
            this.b = b;
        }

        public Crate Get(int tx, int ty)
        {
            if (b.Is(tx, ty) && SETT.ROOMS().fData.tileData.Get(tx, ty) == 1)
            {
                coo.Set(tx, ty);
                ins = b.Getter.Get(tx, ty);
                return this;
            }
            return null;
        }

        public TILE_STORAGE Storage(int tx, int ty)
        {
            if (Get(tx, ty) != null)
                return Storage();
            return null;
        }

        public TILE_STORAGE Storage()
        {
            if (bAway.Get() == 0)
            {
                return crate;
            }
            return null;
        }

        public SETT_JOB Job()
        {
            return job;
        }

        public bool Away()
        {
            return bAway.Get() == 1;
        }

        public bool AnimalHas()
        {
            return bAnimals.Get() != 0;
        }

        public int ResAmount()
        {
            return bAmount.Get();
        }

        public int GoIsReady()
        {
            if (SETT.ENTRY().IsClosed())
                return 1;
            if (bRes.Get() == noRes)
                return 2;
            if (bAnimals.Get() <= 0)
                return 3;
            if (bAway.Get() == 1)
                return 4;
            if (b.Cache.Deliverable(crate.Resource()) <= 0)
                return 6;
            if (bAmount.Get() == 0)
                return 7;
            if (bReservedSpace.Get() != 0)
                return 8;

            return 0;
        }

        public void Deliver()
        {
            if (GoIsReady() != 0)
                return;

            if (bAmount.Get() < ROOM_SUPPLY.STORAGE && bState.Get() < bState.Max())
            {
                bState.Inc(ins, 1);
                return;
            }
            else
            {
                bState.Set(ins, 0);
            }

            RESOURCE res = crate.Resource();
            int am = bAmount.Get();
            int n = Math.Max(0, b.Cache.Needed(res));
            if (am > n)
            {
                bAmount.Set(ins, n);
                Vacate(am - n);
                am = n;
            }

            if (am <= 0)
                return;

            am = b.Cache.Deliver(res, am);
            FACTIONS.Player().Res().Inc(res, RTYPE.ARMY_SUPPLY, -am);
            bAmount.Inc(ins, -am);
            if (am <= 0)
                return;

            bAway.Set(ins, 1);

            if (ins.LiveCount++ > 10)
            {
                ins.LiveCount = 0;
                bAnimals.Inc(ins, -1);
            }

            DIR dir = DIR.ORTHO.Get(SETT.ROOMS().fData.Item.Get(coo).Rotation);
            byte ran = (byte)SETT.TileRan(coo.X, coo.Y);
            int tx = coo.X;
            int ty = coo.Y;
            SETT.HALFENTS().Transports.Military(coo.X + dir.X * 2, coo.Y + dir.Y * 2, ran, res, am, dir);
            Get(tx, ty);
        }

        public void ResourceSet(RESOURCE res)
        {
            b.Tally.Report(this, ins, -1);
            bTot.Set(coo.X, coo.Y, ins, 0);
            bRes.Set(coo.X, coo.Y, ins, res == null ? noRes : res.Index + 1);
            b.Tally.Report(this, ins, 1);
        }

        public RESOURCE RealResource()
        {
            RESOURCE res = crate.Resource();
            int i = bRes.Get();
            if (i == noRes)
                return null;
            if (bReservedSpace.Get() != 0)
                return res;
            if (bAmount.Get() != 0)
                return res;
            return null;
        }

        public void Clear()
        {
            if (crate.Resource() == null)
                return;

            int am = ResAmount();
            Vacate(am);
            ResourceSet(null);
        }

        private void Vacate(int am)
        {
            if (am > 0)
            {
                foreach (DIR dd in DIR.ORTHO)
                {
                    if (!PATH().Solidity.Is(coo, dd))
                    {
                        SETT.THINGS().Resources.Create(coo.X + dd.X, coo.Y + dd.Y, crate.Resource(), am);
                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
            RESOURCE res = crate.Resource();
            if (res == null)
                return;

            int am = ResAmount();

            if (am > 0)
                SETT.THINGS().Resources.Create(coo, res, am);

            b.Tally.Report(this, ins, -1);
        }

        public readonly TILE_STORAGE crate = new TILE_STORAGE()
        {
            public override int Y => coo.Y;
            public override int X => coo.X;
            public override bool StorageIsFindable() => false;
            public override void StorageDeposit(int amount)
            {
                if (ResAmount() + amount > ROOM_SUPPLY.STORAGE)
                    throw new Exception(resource() + " " + ResAmount() + " " + amount + " " + ROOM_SUPPLY.STORAGE);

                bReservedSpace.Inc(ins, -amount);
                bAmount.Inc(ins, amount);
                if (bAmount.Get() >= ROOM_SUPPLY.STORAGE)
                    deliver();
                ins.Jobs.SearchAgain();
            }

            public override int StorageReserved() => bReservedSpace.Get();
            public override int StorageResourcesNeeded() => 8;
            public override RBIT JobResourceBitToFetch() => bAnimals.Get() == 0 ? b.LiveStock.Bit : null;
            public override bool JobReservedIs(RESOURCE r) => bAnimalsRes.Get() == 1;
            public override void JobReserveCancel(RESOURCE r) => bAnimalsRes.Set(ins, 0);
            public override bool JobReserveCanBe()
            {
                if (JobReservedIs(null))
                    return false;
                return bAnimals.Get() == 0 || GoIsReady() == 0 || bAway.Get() == 1;
            }
            public override void JobReserve(RESOURCE r) => bAnimalsRes.Set(ins, 1);
            public override double JobPerformTime(Humanoid a) => 16;
            public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int rAm)
            {
                JobReserveCancel(null);
                if (r != null)
                {
                    bAnimals.Inc(ins, rAm);
                    FACTIONS.Player().Res().Inc(r, RTYPE.PRODUCED, -rAm);
                }
                else
                {
                    bAway.Set(ins, 0);
                    deliver();
                }

                return null;
            }
            public override bool LongFetch() => true;
            public override CharSequence JobName() => b.Employment().Verb;
            public override COORDINATE JobCoo() => coo;
        };

        public readonly SETT_JOB job = new SETT_JOB()
        {
            public override Sound Sound() => null;
            public override bool JobReservedIs(RESOURCE r) => bAnimalsRes.Get() == 1;
            public override void JobReserveCancel(RESOURCE r) => bAnimalsRes.Set(ins, 0);
            public override bool JobReserveCanBe()
            {
                if (JobReservedIs(null))
                    return false;
                return bAnimals.Get() == 0 || GoIsReady() == 0 || bAway.Get() == 1;
            }
            public override void JobReserve(RESOURCE r) => bAnimalsRes.Set(ins, 1);
            public override double JobPerformTime(Humanoid a) => 16;
            public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int rAm)
            {
                JobReserveCancel(null);
                if (r != null)
                {
                    bAnimals.Inc(ins, rAm);
                    FACTIONS.Player().Res().Inc(r, RTYPE.PRODUCED, -rAm);
                }
                else
                {
                    bAway.Set(ins, 0);
                    deliver();
                }

                return null;
            }
            public override bool LongFetch() => true;
            public override int JobResourcesNeeded(Humanoid skill) => 8;
            public override CharSequence JobName() => b.Employment().Verb;
            public override COORDINATE JobCoo() => coo;
        };

        private class BB : RoomBits
        {
            public BB(Bits bits) : base(coo, bits)
            {
            }

            protected override void Remove()
            {
                b.Tally.Report(Crate.this, ins, -1);
                base.Remove();
            }

            protected override void Add()
            {
                b.Tally.Report(Crate.this, ins, 1);
                base.Add();
            }
        }
    }
}