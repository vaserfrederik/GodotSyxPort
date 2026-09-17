using System;
using snake2d.util.bit;
using snake2d.util.datatypes;
using snake2d.util.datatypes;

namespace settlement.room.law.court
{
    public sealed class CourtStation
    {
        private static readonly Bit workReserved = new Bit(0b010000);
        private static readonly Bits state = new Bits(0b01111);
        private const int STATE_RESERVABLE = 0;
        private const int STATE_RESERVED = 1;
        private const int STATE_USED = 2;
        private const int STATE_JUDGING = 3;

        private readonly Coo cooCriminal = new Coo();
        private readonly Coo cooJudge = new Coo();
        private int rot;
        private int data;
        private CourtInstance ins;
        private static readonly CourtStation self = new CourtStation();

        public static bool IsJudge(COORDINATE c)
        {
            int s = SETT.ROOMS().fData.tileData.Get(c.x(), c.y());
            return s == Constructor.CodeWork;
        }

        public static CourtStation Init(int tx, int ty)
        {
            CourtInstance ins = SETT.ROOMS().COURT.Get(tx, ty);
            if (ins == null)
                return null;
            int c = SETT.ROOMS().fData.tileData.Get(tx, ty);
            self.ins = ins;
            if (c == Constructor.CodeCriminal)
            {
                self.cooCriminal.Set(tx, ty);
                self.rot = SETT.ROOMS().fData.item.Get(tx, ty).rotation;
                DIR d = DIR.ORTHO.Get(self.rot);
                self.cooJudge.Set(tx + d.x() * Constructor.Distance, ty + d.y() * Constructor.Distance);
                self.data = SETT.ROOMS().data.Get(self.cooJudge);
                return self;
            }
            if (c == Constructor.CodeWork)
            {
                self.cooJudge.Set(tx, ty);
                self.rot = SETT.ROOMS().fData.item.Get(tx, ty).rotation;
                DIR d = DIR.ORTHO.Get(self.rot).Perpendicular();
                self.cooCriminal.Set(tx + d.x() * Constructor.Distance, ty + d.y() * Constructor.Distance);
                self.data = SETT.ROOMS().data.Get(self.cooJudge);
                return self;
            }
            return null;
        }

        public DIR CriminalDir()
        {
            return DIR.ORTHO.Get(rot);
        }

        public DIR JudgeDir()
        {
            return DIR.ORTHO.Get(rot).Perpendicular();
        }

        public bool CriminalReservedCanBe()
        {
            return state.Get(data) == STATE_RESERVABLE;
        }

        private void CriminalReserve()
        {
            if (!CriminalReservedCanBe())
                throw new RuntimeException();
            data = state.Set(data, STATE_RESERVED);
            Save();
        }

        public bool CriminalReserved()
        {
            return state.Get(data) >= STATE_RESERVED;
        }

        public bool CriminalIsUsing()
        {
            return state.Get(data) == STATE_USED;
        }

        public void CriminalUse()
        {
            data = state.Set(data, STATE_USED);
            Save();
        }

        public void CriminalClear()
        {
            data = state.Set(data, STATE_RESERVABLE);
            Save();
        }

        private bool WorkReservedCanBe()
        {
            return !workReserved.Is(data) && state.Get(data) > STATE_RESERVED;
        }

        private void WorkReserve()
        {
            data = workReserved.Set(data);
            Save();
        }

        public void WorkUse()
        {
            data = state.Set(data, STATE_JUDGING);
            Save();
        }

        public bool CriminalIsBeingHeard()
        {
            return state.Get(data) == STATE_JUDGING;
        }

        public void WorkCancel()
        {
            data = workReserved.Clear(data);
            Save();
        }

        public bool WorkReserved()
        {
            if (state.Get(data) <= STATE_RESERVED)
                WorkCancel();
            return workReserved.Is(data);
        }

        private void Save()
        {
            Add(SETT.ROOMS().data.Get(cooJudge.x(), cooJudge.y()), -1);
            Add(data, 1);
            SETT.ROOMS().data.Set(ins, cooJudge, data);
        }

        private void Add(int data, int delta)
        {
            if (state.Get(data) != STATE_RESERVABLE)
                ins.Inc(delta, 0);
            if (!workReserved.Is(data) && state.Get(data) > STATE_RESERVED)
                ins.Inc(0, delta);
        }

        public COORDINATE CooJudge()
        {
            return cooJudge;
        }

        public COORDINATE CooCriminal()
        {
            return cooCriminal;
        }
    }
}