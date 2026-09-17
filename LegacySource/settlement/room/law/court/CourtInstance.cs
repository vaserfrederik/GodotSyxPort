using System;
using System.Collections.Generic;

namespace Settlement.Room.Law.Court
{
    public class CourtInstance : RoomInstance, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        private short executions;
        private short workCurrent;
        private readonly short total;
        private readonly short[] cellsXY;
        private short cellI = 0;
        private short wI = 0;
        public readonly RoomServiceInstance service;

        protected CourtInstance(ROOM_COURT b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            int spots = 0;
            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                if (CourtStation.isJudge(c))
                {
                    spots++;
                }
            }

            cellsXY = new short[spots * 2];
            total = (short)spots;
            executions = 0;
            spots = 0;
            int sers = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    if (CourtStation.isJudge(c))
                    {
                        cellsXY[spots++] = (short)c.x();
                        cellsXY[spots++] = (short)c.y();
                    }
                    if (Service.Init(c.x(), c.y()) != null)
                        sers++;
                }
            }
            employees().maxSet(total);
            employees().neededSet(total);
            service = new RoomServiceInstance(sers, blueprintI().data);
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    Service.InitInit(c.x(), c.y(), this);
                }
            }
            activate();
        }

        protected override void loadFix()
        {
            base.loadFix();
        }

        public int total()
        {
            return total;
        }

        public int executions()
        {
            return executions;
        }

        public void inc(int executions, int workCurrent)
        {
            this.executions += executions;
            this.workCurrent += workCurrent;
            if (active())
            {
                blueprintI().incPrisoners(executions, 0);
            }
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override void activateAction()
        {
            blueprintI().incPrisoners(executions, total);
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    Service s = Service.Init(c.x(), c.y());
                    if (s != null)
                    {
                        s.activate();
                    }
                }
            }
        }

        protected override void deactivateAction()
        {
            blueprintI().incPrisoners(-executions, -total);
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    Service s = Service.Init(c.x(), c.y());
                    if (s != null)
                    {
                        s.deactivate();
                    }
                }
            }
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            if (day)
                service.updateDay();
        }

        protected override void dispose()
        {
            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                FSERVICE s = Service.Init(c.x(), c.y());
                if (s != null && s.findableReservedCanBe())
                    s.findableReserve();
            }
            service.dispose(blueprintI().data);
        }

        public override ROOM_COURT blueprintI()
        {
            return (ROOM_COURT)blueprint();
        }

        public CourtStation reserveSpot()
        {
            if (executions == total)
                throw new Exception();
            if (!active())
                throw new Exception();
            for (int i = 0; i < cellsXY.Length; i += 2)
            {
                cellI += 2;
                if (cellI >= cellsXY.Length)
                    cellI = 0;
                int tx = cellsXY[cellI];
                int ty = cellsXY[cellI + 1];
                CourtStation s = CourtStation.Init(tx, ty);
                if (s.criminalReseveredCanBe())
                {
                    s.criminalReserve();
                    return s;
                }
            }
            throw new Exception();
        }

        public CourtStation work()
        {
            if (workCurrent == 0)
                return null;
            for (int i = 0; i < cellsXY.Length; i += 2)
            {
                wI += 2;
                if (wI >= cellsXY.Length)
                    wI = 0;
                int tx = cellsXY[wI];
                int ty = cellsXY[wI + 1];
                CourtStation s = CourtStation.Init(tx, ty);
                if (s.workReservedCanBe())
                {
                    s.workReserve();
                    return s;
                }
            }
            throw new Exception();
        }

        public override RoomServiceInstance service()
        {
            return service;
        }

        public override double quality()
        {
            return ROOM_SERVICER.defQuality(this, (double)employees().employed() / employees().max());
        }
    }
}