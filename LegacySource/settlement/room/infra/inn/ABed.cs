using System;
using settlement.main;
using game.audio;
using game.time;
using init.resources;
using settlement.entity.humanoid;
using settlement.misc.job;
using settlement.misc.util;
using snake2d.util.datatypes;

namespace settlement.room.infra.inn
{
    final class ABed
    {
        private static readonly int UNMADE = 0;
        private static readonly int AVAILABLE = 1;
        private static readonly int RESERVED = 2;
        private static readonly int WORK_RESERVED = 3;
        private readonly Coo coo = new Coo();
        private readonly RoomBits claimed = new RoomBits(coo, 0x100);
        private readonly RoomBits work = new RoomBits(coo, 0x0F0);
        private readonly RoomBits state = new RoomBits(coo, 0x0F)
        {
            public override void Set(ROOMA r, int t)
            {
                if (state.Get() == AVAILABLE)
                {
                    ins.service.Report(service, blue.service, -1);
                }
                base.Set(r, t);
                if (state.Get() == AVAILABLE)
                {
                    ins.service.Report(service, blue.service, 1);
                }
                claimed.Set(ins, 0);
            }
        };
        private InnInstance ins;
        private readonly ROOM_INN blue;

        public ABed(ROOM_INN blue)
        {
            this.blue = blue;
        }

        public ABed Init(int tx, int ty)
        {
            if (blue.Is(tx, ty))
            {
                if (ROOMS().fData.tileData.Is(tx, ty, Constructor.ITAIL))
                {
                    coo.Set(tx, ty);
                    ins = blue.Get(tx, ty);
                    return this;
                }
            }
            return null;
        }

        public static bool IsUnmade(int tx, int ty)
        {
            int s = (ROOMS().data.Get(tx, ty) & 0x0F);
            return s == UNMADE || s == WORK_RESERVED;
        }

        public static bool IsClaimed(int tx, int ty)
        {
            int s = (ROOMS().data.Get(tx, ty) & 0x0100);
            return s != 0;
        }

        public static DIR SleepDir(int tx, int ty)
        {
            for (int i = 0; i < DIR.ORTHO.Size(); i++)
            {
                DIR d = DIR.ORTHO.Get(i);
                if (SETT.ROOMS().fData.tileData.Is(tx, ty, Constructor.IHEAD))
                {
                    return d;
                }
            }
            return DIR.C;
        }

        public readonly FSERVICE service = new FSERVICE()
        {
            public override void Consume()
            {
                if (state.Get() != RESERVED)
                {
                    throw new RuntimeException();
                }
                state.Set(ins, UNMADE);
                work.Set(ins, 0);
                ins.jobs.SearchAgain();
            }

            public override int X()
            {
                return coo.X();
            }

            public override int Y()
            {
                return coo.Y();
            }

            public override bool FindableReservedCanBe()
            {
                return state.Get() == AVAILABLE;
            }

            public override void FindableReserve()
            {
                if (state.Get() != AVAILABLE)
                {
                    throw new RuntimeException();
                }
                state.Set(ins, RESERVED);
            }

            public override bool FindableReservedIs()
            {
                return state.Get() == RESERVED;
            }

            public override void StartUsing()
            {
                claimed.Set(ins, 1);
            }

            public override void FindableReserveCancel()
            {
                if (state.Get() == RESERVED)
                {
                    state.Set(ins, AVAILABLE);
                }
            }
        };

        public readonly SETT_JOB job = new SETT_JOB()
        {
            private int ws = (int)(TIME.workSeconds() / 10);

            public override bool JobUseTool()
            {
                return false;
            }

            public override void JobStartPerforming()
            {
            }

            public override SoundRace JobSound()
            {
                return ins.blueprintI().Employment().Sound();
            }

            public override RBIT JobResourceBitToFetch()
            {
                return null;
            }

            public override bool JobReservedIs(RESOURCE r)
            {
                return state.Get() == WORK_RESERVED;
            }

            public override void JobReserveCancel(RESOURCE r)
            {
                if (JobReservedIs(r))
                {
                    state.Set(ins, UNMADE);
                }
            }

            public override bool JobReserveCanBe()
            {
                return state.Get() == UNMADE;
            }

            public override void JobReserve(RESOURCE r)
            {
                if (!JobReserveCanBe())
                {
                    throw new RuntimeException();
                }

                state.Set(ins, WORK_RESERVED);
            }

            public override double JobPerformTime(Humanoid skill)
            {
                return ws;
            }

            public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int rAm)
            {
                if (!JobReservedIs(r))
                {
                    throw new RuntimeException();
                }
                work.Inc(ins, 1);
                if (work.Get() == 8)
                {
                    state.Set(ins, AVAILABLE);
                }
                else
                {
                    state.Set(ins, UNMADE);
                }
                return null;
            }

            public override CharSequence JobName()
            {
                return blue.Employment().verb;
            }

            public override COORDINATE JobCoo()
            {
                return coo;
            }
        };
    }
}