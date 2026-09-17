using System;
using game.audio;
using game.time;
using init.resources;
using settlement.entity.humanoid;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.main;
using settlement.room.main.util;
using snake2d.util.datatypes;

namespace settlement.room.service.pleasure
{
    final class ABed
    {
        private static readonly int UNAVAILABLE = 0;
        private static readonly int AVAILABLE = 1;
        private static readonly int RESERVED = 2;

        private readonly Coo coo = new Coo();
        private readonly RoomBits state = new RoomBits(coo, 0x0F)
        {
            public override void set(ROOMA r, int t)
            {
                if (state.get() == AVAILABLE)
                {
                    ins.service.report(service, blue.service, -1);
                }
                base.set(r, t);
                if (state.get() == AVAILABLE)
                    ins.service.report(service, blue.service, 1);
                wdata.set(r, 0);
            }
        };

        private readonly RoomBits worked = new RoomBits(coo, 0b0001_0000);
        private readonly RoomBits workedHasBeen = new RoomBits(coo, 0b0010_0000);

        private readonly RoomBits clientReady = new RoomBits(coo, 0b0000_0001_0000_0000);
        public readonly RoomBits clientUndressed = new RoomBits(coo, 0b0000_0010_0000_0000);
        private readonly RoomBits workerReady = new RoomBits(coo, 0b0000_0100_0000_0000);
        private readonly RoomBits workerUndressed = new RoomBits(coo, 0b0000_1000_0000_0000);
        private readonly RoomBits wdata = new RoomBits(coo, 0b0000_1111_0000_0000);

        private PleasureInstance ins;
        private readonly ROOM_PLEASURE blue;

        public ABed(ROOM_PLEASURE blue)
        {
            this.blue = blue;
        }

        public ABed init(int tx, int ty)
        {
            if (blue.is(tx, ty))
            {
                if (ROOMS().fData.tileData.is(tx, ty, Constructor.ISERVICE))
                {
                    coo.set(tx, ty);
                    ins = blue.get(tx, ty);
                    return this;
                }
            }
            return null;
        }

        public bool clientShouldUndress()
        {
            clientReady.set(ins, 1);
            if (workerUndressed.get() == 1)
            {
                return true;
            }
            return false;
        }

        public void clientUndress()
        {
            clientReady.set(ins, 1);
            clientUndressed.set(ins, 1);
        }

        public bool workerReadyShouldUndress()
        {
            workerReady.set(ins, 1);
            if (clientReady.get() == 1)
            {
                workerUndressed.set(ins, 1);
                return true;
            }
            return false;
        }

        public readonly FSERVICE service = new FSERVICE()
        {
            public override void consume()
            {
                if (state.get() != RESERVED)
                    throw new RuntimeException();
                if (worked.get() == 1 || workedHasBeen.get() == 1)
                {
                    state.set(ins, AVAILABLE);
                    workedHasBeen.set(ins, 0);
                }
                else
                    state.set(ins, UNAVAILABLE);
            }

            public override int x()
            {
                return coo.x();
            }

            public override int y()
            {
                return coo.y();
            }

            public override bool findableReservedCanBe()
            {
                return state.get() == AVAILABLE;
            }

            public override void findableReserve()
            {
                if (state.get() != AVAILABLE)
                    throw new RuntimeException();
                state.set(ins, RESERVED);
            }

            public override bool findableReservedIs()
            {
                return state.get() == RESERVED;
            }

            public override void startUsing()
            {
            }

            public override void findableReserveCancel()
            {
                if (state.get() == RESERVED)
                    state.set(ins, AVAILABLE);
            }
        };

        internal readonly SETT_JOB job = new SETT_JOB()
        {
            private int ws = (int)(TIME.workSeconds() / 10);

            public override bool jobUseTool()
            {
                return false;
            }

            public override void jobStartPerforming()
            {
            }

            public override SoundRace jobSound()
            {
                return ins.blueprintI().employment().sound();
            }

            public override RBIT jobResourceBitToFetch()
            {
                return null;
            }

            public override bool jobReservedIs(RESOURCE r)
            {
                return worked.get() == 1;
            }

            public override void jobReserveCancel(RESOURCE r)
            {
                if (jobReservedIs(r))
                {
                    worked.set(ins, 0);
                }
            }

            public override bool jobReserveCanBe()
            {
                return !jobReservedIs(null);
            }

            public override void jobReserve(RESOURCE r)
            {
                if (!jobReserveCanBe())
                    throw new RuntimeException();
                worked.set(ins, 1);
                if (state.get() == UNAVAILABLE)
                    state.set(ins, AVAILABLE);
            }

            public override double jobPerformTime(Humanoid skill)
            {
                return ws;
            }

            public override RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm)
            {
                if (!jobReservedIs(r))
                    throw new RuntimeException();
                worked.set(ins, 0);
                if (state.get() == UNAVAILABLE)
                    state.set(ins, AVAILABLE);
                else
                    workedHasBeen.set(ins, 1);
                return null;
            }

            public override CharSequence jobName()
            {
                return blue.employment().verb;
            }

            public override COORDINATE jobCoo()
            {
                return coo;
            }
        };
    }
}