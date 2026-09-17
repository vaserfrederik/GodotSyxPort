using System;
using snake2d.util.bit;
using snake2d.util.datatypes;
using settlement.main;
using settlement.entity.humanoid;
using settlement.room.main;
using settlement.misc.job;
using settlement.room.service.module;
using init.resources;

namespace settlement.room.service.lavatory
{
    public class Lavatory : FSERVICE
    {
        public const int BIT = 0b1000000000000000;
        public const int BIT_WASH = 0b0100000000000000;
        public static readonly Bits USAGE = new Bits(0b1110000);
        public const int S_USED = 1;
        public const int S_BEEING_CLEANED = 2;
        public const int S_RESERVABLE = 0;
        public const int S_RESERVED = 3;
        public const int S_UNUSABLE = 4;

        private static readonly Lavatory self = new Lavatory();
        private readonly Coo coo = new Coo();
        private LavatoryInstance ins;
        private int data;
        private ROOM_LAVATORY blue;

        private Lavatory()
        {
        }

        public static Lavatory Get(int tx, int ty)
        {
            RoomBlueprint p = SETT.ROOMS().map.blueprint.Get(tx, ty);
            if (p is ROOM_LAVATORY)
            {
                int data = ROOMS().data.Get(tx, ty);
                if ((data & BIT) != 0)
                {
                    self.blue = (ROOM_LAVATORY)p;
                    self.ins = self.blue.Get(tx, ty);
                    self.data = data;
                    self.coo.Set(tx, ty);
                    return self;
                }
            }
            return null;
        }

        private void Save()
        {
            int old = ROOMS().data.Get(coo);
            if (old == data)
                return;
            int current = data;
            data = old;
            ins.service.Report(this, ins.blueprintI().data, -1);
            data = current;
            ins.service.Report(this, ins.blueprintI().data, 1);
            ROOMS().data.Set(ins, coo, data);
        }

        public static bool IsOpen(int data)
        {
            return USAGE.Get(data) < 4;
        }

        public void Init(RoomServiceInstance ser)
        {
            ser.Report(this, ins.blueprintI().data, 1);
        }

        private void StateSet(int state)
        {
            data &= ~0b01111;
            data |= state;
        }

        private int State()
        {
            return data & 0b01111;
        }

        public override bool FindableReservedIs()
        {
            return State() == S_RESERVED;
        }

        public override bool FindableReservedCanBe()
        {
            return State() == S_RESERVABLE;
        }

        public override void FindableReserve()
        {
            if (!FindableReservedCanBe())
                throw new RuntimeException();
            StateSet(S_RESERVED);
            Save();
        }

        public override void FindableReserveCancel()
        {
            if (State() == S_RESERVED)
            {
                StateSet(S_RESERVABLE);
                Save();
            }
        }

        public override void Consume()
        {
            if (!FindableReservedIs())
                throw new RuntimeException();
            data = USAGE.Inc(data, 1);
            if (USAGE.Get(data) >= 4)
                StateSet(S_USED);
            else
                StateSet(S_RESERVABLE);
            Save();
        }

        public void Dispose()
        {
            StateSet(S_UNUSABLE);
            Save();
        }

        public void Fix()
        {
            StateSet(S_RESERVABLE);
            Save();
        }

        public override int X()
        {
            return coo.X();
        }

        public override int Y()
        {
            return coo.Y();
        }

        public DIR GetDir()
        {
            FurnisherItem it = ROOMS().fData.item.Get(coo);
            if (it == null)
                return DIR.N;

            foreach (DIR d in DIR.ORTHO)
            {
                if (!ROOMS().fData.sprite.Is(coo, d) && (d.OrthoID() == it.rotation || d.Perpendicular().OrthoID() == it.rotation))
                    return d;
            }
            return DIR.N;
        }

        public SETT_JOB job = new SETT_JOB()
        {
            public void JobReserve(RESOURCE r)
            {
                if (!JobReserveCanBe())
                    throw new RuntimeException();
                StateSet(S_BEEING_CLEANED);
                Save();
            }

            public bool JobReservedIs(RESOURCE r)
            {
                return State() == S_BEEING_CLEANED;
            }

            public void JobReserveCancel(RESOURCE r)
            {
                if (State() == S_BEEING_CLEANED)
                {
                    if (USAGE.Get(data) < 4)
                        StateSet(S_RESERVABLE);
                    else
                        StateSet(S_USED);
                    Save();
                }
            }

            public RBIT JobResourceBitToFetch()
            {
                return null;
            }

            public double JobPerformTime(Humanoid skill)
            {
                return 45;
            }

            public RESOURCE JobPerform(Humanoid skill, RESOURCE res, int ram)
            {
                if (!JobReservedIs(res))
                    throw new RuntimeException();
                StateSet(S_RESERVABLE);
                data = USAGE.Set(data, 0);
                Save();
                return null;
            }

            public COORDINATE JobCoo()
            {
                return coo;
            }

            public CharSequence JobName()
            {
                return blue.Employment().verb;
            }

            public void JobStartPerforming()
            {
                // TODO Auto-generated method stub
            }

            public bool JobUseTool()
            {
                return false;
            }

            public SoundRace JobSound()
            {
                return blue.Employment().Sound();
            }

            public bool JobReserveCanBe()
            {
                return State() == S_USED || (State() == S_RESERVABLE && USAGE.Get(data) > 2);
            }
        };
    }
}