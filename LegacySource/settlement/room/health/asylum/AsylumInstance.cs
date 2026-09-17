using System;
using System.Collections.Generic;
using settlement.constant;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.job;
using settlement.room.main.util;
using snake2d;
using util.rendering;

namespace settlement.room.health.asylum
{
    public sealed class AsylumInstance : RoomInstance, JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE
    {
        private static readonly long SerialVersionUID = 1L;
        public readonly Jobs Jobs;
        public const double WORKER_PER_BED = 1d / 8d;
        private readonly short[] cellsXY;
        private short cellI = 0;
        private long[] pData;
        private short used;

        protected AsylumInstance(ROOM_ASYLUM b, TmpArea area, RoomInit init)
            : base(b, area, init)
        {
            int cells = 0;
            foreach (COORDINATE c in body())
            {
                if (Is(c))
                {
                    Candle(c.x(), c.y());
                    if (SETT.ROOMS().fData.tileData.Get(c) == Constructor.CODE_ENTRANCE)
                    {
                        cells++;
                    }
                }
            }

            cellsXY = new short[cells * 2];
            cells = 0;
            foreach (COORDINATE c in body())
            {
                if (Is(c))
                {
                    if (SETT.ROOMS().fData.tileData.Get(c) == Constructor.CODE_ENTRANCE)
                    {
                        cellsXY[cells++] = (short)c.x();
                        cellsXY[cells++] = (short)c.y();
                    }
                }
            }

            Jobs = new Jobs(this);
            Jobs.Randomize();
            Jobs.SetAlwaysNew();
            pData = b.Consumption.MakeData();
            Employees().MaxSet((int)Math.Ceiling(b.Constructor.Guards.Get(this)));
            Employees().NeededSet((int)Math.Ceiling(b.Constructor.Guards.Get(this)));
            Activate();
        }

        protected override void LoadFix()
        {
            pData = blueprintI().Consumption.MakeDataFix(pData);
        }

        void Candle(int tx, int ty)
        {
            if (SETT.LIGHTS().Is(tx, ty))
            {
                SETT.LIGHTS().Remove(tx, ty);
                FurnisherItem it = SETT.ROOMS().fData.item.Get(tx, ty);

                foreach (DIR d in DIR.ORTHO)
                {
                    if (SETT.ROOMS().fData.item.Is(tx, ty, d, it))
                    {
                        SETT.LIGHTS().Candle(tx, ty, d.X() * (C.TILE_SIZEH - 4), d.Y() * (C.TILE_SIZEH - 4));
                        return;
                    }
                }
            }
        }

        public int Prisoners()
        {
            return used;
        }

        public int PrisonersMax()
        {
            return cellsXY.Length / 2;
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.Lit();
            return base.Render(r, shadowBatch, it);
        }

        protected override void ActivateAction()
        {
            blueprintI().IncPrisoners(used, PrisonersMax());
        }

        protected override void DeactivateAction()
        {
            foreach (ENTITY e in SETT.ENTITIES().GetAllEnts())
            {
                if (e != null && e is Humanoid)
                {
                    Humanoid h = (Humanoid)e;
                    HEvent.Handler.RemoveRoom(h, this);
                }
            }

            for (int i = 0; i < cellsXY.Length; i += 2)
            {
                cellI += 2;
                if (cellI >= cellsXY.Length)
                    cellI = 0;
                int tx = cellsXY[cellI];
                int ty = cellsXY[cellI + 1];
                Cell.Init(tx, ty).ReserveCancel();
            }

            blueprintI().IncPrisoners(-used, -PrisonersMax());
            used = 0;
        }

        void Inc(int delta)
        {
            used += delta;
            if (Active())
                blueprintI().IncPrisoners(delta, 0);
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            Jobs.SearchAgain();
            blueprintI().Consumption.UpdateRoom(this);
        }

        public JOB_MANAGER GetWork()
        {
            return Jobs;
        }

        protected override void Dispose()
        {
        }

        public ROOM_ASYLUM BlueprintI()
        {
            return (ROOM_ASYLUM)Blueprint();
        }

        public COORDINATE RegisterPrisoner(Humanoid a)
        {
            if (used == PrisonersMax())
                throw new RuntimeException();
            if (!Active())
                throw new RuntimeException();

            if (Is(a.Tc()))
            {
                Cell c = Cell.Init(a.Tc().X(), a.Tc().Y());
                if (c != null && !c.ReservedIs())
                {
                    c.Reserve();
                    return c.Coo;
                }
            }

            for (int i = 0; i < cellsXY.Length; i += 2)
            {
                cellI += 2;
                if (cellI >= cellsXY.Length)
                    cellI = 0;
                int tx = cellsXY[cellI];
                int ty = cellsXY[cellI + 1];
                Cell c = Cell.Init(tx, ty);
                if (!c.ReservedIs())
                {
                    c.Reserve();
                    return c.Coo;
                }
            }
            throw new RuntimeException();
        }

        void RemovePrisoner(int tx, int ty)
        {
            Cell c = Cell.Init(tx, ty);
            if (c == null)
                return;
            c.ReserveCancel();
        }

        bool IsReserved(int tx, int ty)
        {
            Cell c = Cell.Init(tx, ty);
            return c != null && c.ReservedIs();
        }

        public class Jobs : JobPositions<AsylumInstance>
        {
            private static readonly long SerialVersionUID = 1L;

            public Jobs(AsylumInstance ins)
                : base(ins)
            {
            }

            protected override SETT_JOB Get(int tx, int ty)
            {
                if (Food.Init(tx, ty) != null)
                {
                    return Food.Init(tx, ty);
                }
                return null;
            }

            protected override bool IsAndInit(int tx, int ty)
            {
                if (Food.Init(tx, ty) != null)
                    return true;
                return false;
            }
        }

        public long[] ProductionData()
        {
            return pData;
        }

        public Industry Industry()
        {
            return blueprintI().Industries().Get(0);
        }

        public int IndustryI()
        {
            return 0;
        }
    }
}