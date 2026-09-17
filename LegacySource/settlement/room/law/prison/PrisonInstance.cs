using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Room.Law.Prison
{
    public class PrisonInstance : RoomInstance, JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE
    {
        private static readonly long serialVersionUID = 1L;
        public Jobs jobs;

        private short prisoners = 0;

        private static readonly Bits bprisoners = new Bits(0b01111);
        private readonly short[] cellsXY;
        private short cellI = 0;
        private readonly RBITImp fetch = new RBITImp();
        private long[] productionData;
        private float riotChance = 1;
        private bool hasWarned = false;

        protected PrisonInstance(ROOM_PRISON b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            int cells = 0;
            foreach (var c in body())
            {
                if (Is(c))
                {
                    candle(c.x, c.y);
                    if (SETT.ROOMS().fData.tileData.Get(c) == Constructor.CODE_ENTRANCE)
                    {
                        cells++;
                    }
                }
            }

            cellsXY = new short[cells * 2];
            cells = 0;
            foreach (var c in body())
            {
                if (Is(c))
                {
                    if (SETT.ROOMS().fData.tileData.Get(c) == Constructor.CODE_ENTRANCE)
                    {
                        cellsXY[cells++] = (short)c.x;
                        cellsXY[cells++] = (short)c.y;
                    }
                }
            }
            productionData = blueprintI().indu.MakeData();
            jobs = new Jobs(this);

            int am = (int)Math.Ceiling(b.constructor.guards.Get(this));
            employees().maxSet(am);
            employees().neededSet(am);

            foreach (var e in RESOURCES.EDI().All())
                if (e.serve)
                    fetch.Or(e.resource);
            Activate();
            jobs.SetAlwaysNew();
        }

        protected override void LoadFix()
        {
            productionData = blueprintI().indu.MakeDataFix(productionData);
            jobs.SetAlwaysNew();
            jobs.resNotFound.Clear();
        }

        void candle(int tx, int ty)
        {
            if (SETT.LIGHTS().Is(tx, ty))
            {
                SETT.LIGHTS().Remove(tx, ty);
                FurnisherItem it = SETT.ROOMS().fData.item.Get(tx, ty);

                foreach (DIR d in DIR.ORTHO)
                {
                    if (SETT.ROOMS().fData.item.Is(tx, ty, d, it))
                    {
                        SETT.LIGHTS().Candle(tx, ty, d.x * (C.TILE_SIZEH - 4), d.y * (C.TILE_SIZEH - 4));
                        return;
                    }
                }
            }
        }

        public int Prisoners()
        {
            return prisoners;
        }

        public int PrisonersMax()
        {
            return blueprintI().constructor.PRISONERS_PER_CELL * cellsXY.Length / 2;
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.Render(r, shadowBatch, it);
        }

        protected override void ActivateAction()
        {
            blueprintI().IncPrisoners(prisoners, prisonersMax());
        }

        protected override void DeactivateAction()
        {
            foreach (var e in SETT.ENTITIES().GetAllEnts())
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
                int data = SETT.ROOMS().data.Get(tx, ty);
                data = bprisoners.Set(data, 0);
                SETT.ROOMS().data.Set(this, tx, ty, data);
            }

            blueprintI().IncPrisoners(-prisoners, -prisonersMax());
            prisoners = 0;
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            blueprintI().indu.UpdateRoom(this);
            jobs.SearchAgain();
            if (day && prisoners() > 0)
            {
                float prev = riotChance;

                double v = (double)employees().Employed() / employees().Max() - 1;

                RBIT.RBITImp.tmp.ClearSet(fetch);
                RBIT.RBITImp.tmp.Xor(jobs.resNotFound);
                if (RBIT.RBITImp.tmp.IsClear())
                    v -= 1;

                v /= 4;

                if (v == 0)
                {
                    riotChance = 1;
                    return;
                }

                riotChance += (float)v;
                riotChance = (float)CLAMP.d(riotChance, 0, 1);
                if (riotChance < 0.5 && riotChance < prev && !hasWarned)
                {
                    Gui.mWarn(this);
                    hasWarned = true;
                }
                else if (riotChance <= 0)
                {
                    Gui.m(this);
                    hasWarned = false;
                    riotChance = 1;
                    foreach (var e in SETT.ENTITIES().GetAllEnts())
                    {
                        if (e is Humanoid)
                        {
                            Humanoid a = (Humanoid)e;
                            if (AIModule_Prisoner.IsPrisoner(a, this))
                            {
                                STATS.LAW().escapeInc();
                                a.kill(false, CAUSE_LEAVES.OTHER());
                            }
                        }
                    }
                }
            }
        }

        public JOB_MANAGER GetWork()
        {
            return jobs;
        }

        protected override void Dispose()
        {
        }

        public ROOM_PRISON BlueprintI()
        {
            return (ROOM_PRISON)blueprint();
        }

        COORDINATE RegisterPrisoner(COORDINATE c)
        {
            if (prisoners >= PrisonersMax())
                throw new RuntimeException();
            if (!active())
                throw new RuntimeException();

            if (Is(c) && SETT.ROOMS().fData.tileData.Get(c) == Constructor.CODE_ENTRANCE)
            {
                int tx = c.x;
                int ty = c.y;
                int data = SETT.ROOMS().data.Get(tx, ty);
                if (bprisoners.Get(data) < blueprintI().constructor.PRISONERS_PER_CELL)
                {
                    IncPrisoner(tx, ty, 1);
                    Coo.TMP.Set(tx, ty);
                    return Coo.TMP;
                }
            }

            int prisonersInCells = 0;
            for (int i = 0; i < cellsXY.Length; i += 2)
            {
                int tx = cellsXY[i];
                int ty = cellsXY[i + 1];
                int data = SETT.ROOMS().data.Get(tx, ty);
                prisonersInCells += bprisoners.Get(data);
            }

            if (prisonersInCells < PrisonersMax())
            {
                for (int i = 0; i < cellsXY.Length; i += 2)
                {
                    int tx = cellsXY[i];
                    int ty = cellsXY[i + 1];
                    int data = SETT.ROOMS().data.Get(tx, ty);
                    if (bprisoners.Get(data) < blueprintI().constructor.PRISONERS_PER_CELL)
                    {
                        IncPrisoner(tx, ty, 1);
                        Coo.TMP.Set(tx, ty);
                        return Coo.TMP;
                    }
                }
            }

            throw new RuntimeException("No available cells for prisoner");
        }

        void IncPrisoner(int tx, int ty, int am)
        {
            if (am < 0 && (bprisoners.Get(SETT.ROOMS().data.Get(tx, ty)) + am < 0 || prisoners < 0))
                return;
            if (am > 0 && (bprisoners.Get(SETT.ROOMS().data.Get(tx, ty)) + am > blueprintI().constructor.PRISONERS_PER_CELL || prisoners > PrisonersMax()))
            {
                GAME.Error("prison " + (prisoners > PrisonersMax()));
                return;
            }
            prisoners += am;
            int data = bprisoners.Inc(SETT.ROOMS().data.Get(tx, ty), am);
            SETT.ROOMS().data.Set(this, tx, ty, data);
            blueprintI().IncPrisoners(am, 0);
        }

        void RemovePrisoner(int tx, int ty)
        {
            if (!Is(tx, ty))
                return;
            if (SETT.ROOMS().fData.tileData.Get(tx, ty) != Constructor.CODE_ENTRANCE)
                return;
            int data = SETT.ROOMS().data.Get(tx, ty);
            if (bprisoners.Get(data) == 0)
                return;
            IncPrisoner(tx, ty, -1);
        }

        bool IsReserved(int tx, int ty)
        {
            if (SETT.ROOMS().fData.tileData.Get(tx, ty) != Constructor.CODE_ENTRANCE)
                return false;
            int data = SETT.ROOMS().data.Get(tx, ty);
            if (bprisoners.Get(data) == 0)
                return false;
            return true;
        }

        public class Jobs : JobPositions<PrisonInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(PrisonInstance ins) : base(ins)
            {
                SetAlwaysNew();
            }

            protected override SETT_JOB Get(int tx, int ty)
            {
                if (Food.init(tx, ty) != null)
                    return Food.init(tx, ty);
                if (Latrine.init(tx, ty) != null)
                    return Latrine.init(tx, ty);
                if (Cell.init(tx, ty) != null)
                    return Cell.init(tx, ty);
                return null;
            }

            protected override bool IsAndInit(int tx, int ty)
            {
                if (Food.init(tx, ty) != null)
                    return true;
                if (Latrine.init(tx, ty) != null)
                    return true;
                if (Cell.init(tx, ty) != null)
                    return true;
                return false;
            }
        }

        public long[] ProductionData()
        {
            return productionData;
        }

        public Industry Industry()
        {
            return blueprintI().indu;
        }

        public int IndustryI()
        {
            return 0;
        }
    }
}