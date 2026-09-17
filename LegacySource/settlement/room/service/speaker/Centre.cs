using settlement.room.service.speaker;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.misc.job;
using settlement.misc.util;
using snake2d.util.datatypes;

namespace Settlement.Room.Service.Speaker
{
    internal sealed class Centre
    {
        private SpeakerInstance ins;
        private readonly Coo coo = new Coo();
        private readonly ROOM_SPEAKER b;

        internal Centre(ROOM_SPEAKER b)
        {
            this.b = b;
        }

        public SETT_JOB Job(int tx, int ty)
        {
            if (Init(tx, ty))
                return job;
            return null;
        }

        public FSERVICE Service(int tx, int ty)
        {
            if (Init(tx, ty))
                return service;
            return null;
        }

        private bool Init(int tx, int ty)
        {
            ins = b.Getter.Get(tx, ty);
            if (ins != null && tx == ins.Body().CX() && ty == ins.Body().CY())
            {
                coo.Set(tx, ty);
                return true;
            }
            return false;
        }

        private readonly FSERVICE service = new FSERVICE()
        {
            public override void Consume()
            {
            }

            public override int X()
            {
                return ins.Body().CX();
            }

            public override int Y()
            {
                return ins.Body().CY();
            }

            public override bool FindableReservedCanBe()
            {
                return ins.Services() > 0;
            }

            public override void FindableReserve()
            {
                if (!FindableReservedCanBe())
                {
                    throw new RuntimeException();
                }
                ins.IncServices(-1);
            }

            public override bool FindableReservedIs()
            {
                return ins.HasService();
            }

            public override void FindableReserveCancel()
            {
                ins.IncServices(1);
            }
        };

        private readonly SETT_JOB job = new SETT_JOB()
        {
            public override bool JobUseTool()
            {
                return false;
            }

            public override void JobStartPerforming()
            {
            }

            public override SoundRace JobSound()
            {
                return null;
            }

            public override RBIT JobResourceBitToFetch()
            {
                return null;
            }

            public override bool JobReservedIs(RESOURCE r)
            {
                return true;
            }

            public override void JobReserveCancel(RESOURCE r)
            {
            }

            public override bool JobReserveCanBe()
            {
                return true;
            }

            public override void JobReserve(RESOURCE r)
            {
            }

            public override double JobPerformTime(Humanoid a)
            {
                return 0;
            }

            public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int rAm)
            {
                JobReserveCancel(r);
                return null;
            }

            public override CharSequence JobName()
            {
                return b.Employment().Verb;
            }

            public override COORDINATE JobCoo()
            {
                return coo;
            }
        };

        public JOB_MANAGER Manager(SpeakerInstance ins)
        {
            this.ins = ins;
            return manager;
        }

        private readonly JOB_MANAGER manager = new JOB_MANAGER()
        {
            public override bool ResourceReachable(RESOURCE res)
            {
                return true;
            }

            public override SETT_JOB ReportResourceMissing(RBIT resourceMask, int jx, int jy)
            {
                return null;
            }

            public override SETT_JOB GetReservableJob(COORDINATE c)
            {
                return Job(ins.Body().CX(), ins.Body().CY());
            }

            public override SETT_JOB GetJob(COORDINATE c)
            {
                return Job(ins.Body().CX(), ins.Body().CY());
            }

            public override void ReportResourceFound(RESOURCE res)
            {
                // TODO Auto-generated method stub
            }

            public override void ResetResourceSearch()
            {
                // TODO Auto-generated method stub
            }

            public override bool ResourceShouldSearch(RESOURCE res)
            {
                return true;
            }
        };
    }
}