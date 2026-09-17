using System;
using System.Collections.Generic;
using settlement.room.infra.transport;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.misc.util;
using snake2d.util.bit;
using snake2d.util.datatypes;
using snake2d.util.misc;

final class Job
{
    private TransportInstance ins;
    private readonly Coo coo = new Coo();

    public readonly RoomBits breserved = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0000_0000_0000_0001));
    public readonly RoomBits bamount = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_1111_1111_1111_0000));
    public readonly RoomBits bamountr = new RoomBits(coo, new Bits(0b0000_0000_0001_1111_0000_0000_0000_0000));

    public Job(ROOM_TRANSPORT blue)
    {
    }

    public void Remove(TransportInstance ins)
    {
        this.ins = ins;
        foreach (COORDINATE c in ins.Body())
        {
            if (ins.Is(c) && SETT.ROOMS().fData.tile.Get(c) == B().Constructor.Ww)
            {
                coo.Set(c);
                int am = bamount.Get();
                Remove();
                bamount.Set(ins, 0);
                breserved.Set(ins, 0);
                if (ins.Data.Resource() != null && am > 0)
                {
                    SETT.THINGS().resources.Create(c, ins.Data.Resource(), am);
                }
            }
        }
    }

    public void Add(TransportInstance ins)
    {
        this.ins = ins;
        foreach (COORDINATE c in ins.Body())
        {
            if (ins.Is(c) && SETT.ROOMS().fData.tile.Get(c) == B().Constructor.Ww && storage.StorageReservable() > 0)
            {
                coo.Set(c);
                Add();
            }
        }
    }

    private static ROOM_TRANSPORT B()
    {
        return SETT.ROOMS().TRANSPORT;
    }

    public SETT_JOB Job(int tx, int ty)
    {
        ins = B().Get(tx, ty);
        if (ins == null)
            return null;
        if (SETT.ROOMS().fData.tile.Get(tx, ty) == B().Constructor.Ww)
        {
            coo.Set(tx, ty);
            if ((ins.DestSpaceMask().IsClear() && ins.Data.PrepD() < 2) || ins.Data.NeedsPrep())
                return prep;
            return load;
        }
        return null;
    }

    public TILE_STORAGE Storage(int tx, int ty)
    {
        ins = B().Get(tx, ty);
        if (ins == null)
            return null;
        else if (SETT.ROOMS().fData.tile.Get(tx, ty) == B().Constructor.Ww)
        {
            coo.Set(tx, ty);
            return storage;
        }
        return null;
    }

    public readonly TILE_STORAGE storage = new TILE_STORAGE()
    {
        public int Y()
        {
            return coo.Y();
        }

        public int X()
        {
            return coo.X();
        }

        public void StorageUnreserve(int amount)
        {
            Remove();
            bamountr.Inc(ins, -amount);
            Add();
        }

        public int StorageReserved()
        {
            return bamountr.Get();
        }

        public void StorageReserve(int amount)
        {
            Remove();
            bamountr.Inc(ins, amount);
            Add();
        }

        public int StorageReservable()
        {
            if (ins.Data.Resource() != null)
                return bamountr.Max() - bamount.Get() - bamountr.Get();
            return 0;
        }

        public void StorageDeposit(int amount)
        {
            Remove();
            bamountr.Inc(ins, -amount);
            Add();
            if (!ins.Data.NeedsPrep() || true)
            {
                if (ins.Data.Stored() < ROOM_TRANSPORT.MAX_LOAD)
                {
                    int am = amount;
                    am = CLAMP.i(am, 0, ROOM_TRANSPORT.MAX_LOAD - ins.Data.Stored());
                    ins.Data.Store(am);
                    amount -= am;
                }
            }

            if (amount > 0)
            {
                Remove();
                bamount.Inc(ins, amount);
                //bamountr.Inc(ins, -amount);
                Add();
            }
            ins.Go();
        }

        public RESOURCE Resource()
        {
            return ins.Resource();
        }

        public bool StorageIsFindable()
        {
            return false;
        }
    };

    private void Remove()
    {
        ins.Data.UnloadedInc(-bamount.Get());
        if (storage.StorageReservable() > 0)
        {
            SETT.PATH().finders.storage.ReportAbsence(storage);
            if (bamount.Get() == 0 && bamountr.Get() == 0)
                ins.Data.UnloadedSpotsInc(-1);
        }
    }

    private void Add()
    {
        ins.Data.UnloadedInc(bamount.Get());
        if (storage.StorageReservable() > 0)
        {
            SETT.PATH().finders.storage.ReportPresence(storage);
            if (bamount.Get() == 0 && bamountr.Get() == 0)
                ins.Data.UnloadedSpotsInc(1);
        }
    }

    public readonly SETT_JOB load = new SETT_JOB()
    {
        private int time = 1;

        public bool JobUseTool()
        {
            return false;
        }

        public void JobStartPerforming()
        {
        }

        public SoundRace JobSound()
        {
            return null;
        }

        public RBIT JobResourceBitToFetch()
        {
            return null;
        }

        public bool JobReservedIs(RESOURCE r)
        {
            return breserved.Get() == 1;
        }

        public void JobReserveCancel(RESOURCE r)
        {
            breserved.Set(ins, 0);
        }

        public bool JobReserveCanBe()
        {
            return breserved.Get() == 0 && bamount.Get() > 0 && ins.Data.Stored() < ROOM_TRANSPORT.MAX_LOAD;
        }

        public void JobReserve(RESOURCE r)
        {
            breserved.Set(ins, 1);
        }

        public double JobPerformTime(Humanoid skill)
        {
            return time;
        }

        public RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ri)
        {
            JobReserveCancel(r);
            if (bamount.Get() > 0 && ins.Data.Stored() < ROOM_TRANSPORT.MAX_LOAD)
            {
                int am = bamount.Get();
                am = CLAMP.i(am, 0, ROOM_TRANSPORT.MAX_LOAD - ins.Data.Stored());
                ins.Data.Store(am);
                Remove();
                bamount.Inc(ins, -am);
                Add();
            }

            ins.Go();
            return null;
        }

        public CharSequence JobName()
        {
            return Gui.¤¤organise;
        }

        public COORDINATE JobCoo()
        {
            return coo;
        }
    };

    public readonly SETT_JOB prep = new SETT_JOB()
    {
        private int time = 16;

        public bool JobUseTool()
        {
            return false;
        }

        public void JobStartPerforming()
        {
        }

        public SoundRace JobSound()
        {
            return null;
        }

        public RBIT JobResourceBitToFetch()
        {
            return null;
        }

        public bool JobReservedIs(RESOURCE r)
        {
            return breserved.Get() == 1;
        }

        public void JobReserveCancel(RESOURCE r)
        {
            breserved.Set(ins, 0);
        }

        public bool JobReserveCanBe()
        {
            return breserved.Get() == 0;
        }

        public void JobReserve(RESOURCE r)
        {
            breserved.Set(ins, 1);
        }

        public double JobPerformTime(Humanoid skill)
        {
            return time;
        }

        public RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ri)
        {
            JobReserveCancel(r);
            double am = SETT.ROOMS().STOCKPILE.bonus().Get(skill.Indu()) / SETT.ROOMS().STOCKPILE.bonus().BaseValue * ins.Efficiency() * time;
            ins.Data.Prep(am);
            ins.Go();
            return null;
        }

        public CharSequence JobName()
        {
            return Gui.¤¤preparing;
        }

        public COORDINATE JobCoo()
        {
            return coo;
        }
    };
}