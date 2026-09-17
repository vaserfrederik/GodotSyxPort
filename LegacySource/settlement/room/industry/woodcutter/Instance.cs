using System;
using System.Collections.Generic;
using System.Linq;
using settlement.main;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.water;
using snake2d;
using util;
using util.rendering;

namespace settlement.room.industry.woodcutter
{
    public sealed class Instance : RoomInstance, JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE
    {
        public JobPositions<Instance> jobs;
        private static readonly long serialVersionUID = -3170637142258642320L;
        private long[] pData;
        public short sx, sy;
        public bool hasStorage = true;
        public int workage = 0;
        public double irri;
        private double irriNext;
        private short irriI;

        public Instance(ROOM_WOODCUTTER b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            int x = -1;
            int y = -1;
            var w = (int)blueprintI().constructor.workers.Get(this);
            int ww = 0;
            GUTIL.coos().Set(0);
            irri = 0;

            foreach (var c in body())
            {
                if (Is(c) && SETT.PATH().reachability.Is(c))
                {
                    if (SETT.ROOMS().fData.tileData.Get(c) == Constructor.B_WORK)
                    {
                        b.job.Mark(c.X, c.Y, this);
                    }
                    else if (SETT.ROOMS().fData.tileData.Get(c) == Constructor.B_STORAGE)
                    {
                        if (x == -1)
                        {
                            x = c.X;
                            y = c.Y;
                        }
                    }
                    else if (SETT.ROOMS().fData.tile.Get(c) == null)
                    {
                        if (SETT.TILE_MAP().growth.Type(c.X, c.Y) == SETT.TILE_MAP().growth.tree)
                        {
                            b.job.Mark(c.X, c.Y, this);
                            ww++;
                        }
                        else
                        {
                            GUTIL.coos().Get().Set(c);
                            GUTIL.coos().Inc();
                        }
                    }

                    irri += SETT.GROUND().MOISTURE_TOT.Get(c);
                    RoomPumpable.ReportChange(c.X, c.Y, 0);
                }
            }

            int m = GUTIL.coos().GetI();
            GUTIL.coos().Shuffle(0, m);
            for (int i = 0; i < m; i++)
            {
                var c = GUTIL.coos().Set(i);
                if (ww < w * 4)
                {
                    b.job.Mark(c.X, c.Y, this);
                    ww++;
                }
            }

            foreach (var c in body())
            {
                if (Is(c) && b.job.Init(c.X, c.Y, this) != null && !Job.IsTreeCurrent(c.X, c.Y))
                {
                    TERRAIN().DECOR_WOOD.PlaceFixed(c.X, c.Y);
                }
            }

            if (x == -1 || y == -1)
                GAME.Error($"{x} {y}");
            sx = (short)x;
            sy = (short)y;

            pData = b.productionData.MakeData();
            jobs = new Jobs(this);
            jobs.Randomize();
            employees().MaxSet(w);
            employees().NeededSet(w);
            Activate();
        }

        protected override void LoadFix()
        {
            pData = industry().MakeDataFix(pData);
        }

        protected override void ActivateAction()
        {
        }

        protected override void DeactivateAction()
        {
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            blueprintI().productionData.UpdateRoom(this);
            if (!active())
                return;
            jobs.SearchAgain();
        }

        protected override void Dispose()
        {
            foreach (var c in body())
            {
                if (blueprintI().job.storage.Get(c.X, c.Y, this) != null)
                    blueprintI().job.storage.Dispose();
            }
        }

        protected override bool RenderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            return base.RenderAbove(r, shadowBatch, i);
        }

        public JOB_MANAGER GetWork()
        {
            return jobs;
        }

        public ROOM_WOODCUTTER BlueprintI()
        {
            return (ROOM_WOODCUTTER)blueprint();
        }

        public RESOURCE_TILE ResourceTile(int tx, int ty)
        {
            return blueprintI().job.storage.Get(tx, ty, this);
        }

        public void UpdateTileDay(int tx, int ty)
        {
            blueprintI().job.Update(tx, ty, this);
            if (irriI >= Area())
            {
                irriI = 0;
                irri = irriNext;
                irriNext = 0;
            }
            irriNext += SETT.GROUND().MOISTURE_TOT.Get(tx, ty);
            irriI++;
            base.UpdateTileDay(tx, ty);
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            if (!SETT.ROOMS().fData.item.Is(it.tile()))
            {
                int d = SETT.ROOMS().fData.spriteData.Get(it.tile());
                if (d != 0x0F)
                {
                    blueprintI().constructor.sedge.Render(r, shadowBatch, d, it, GetDegrade(), false);
                }
            }
            return base.Render(r, shadowBatch, it);
        }

        public long[] ProductionData()
        {
            return pData;
        }

        public Industry Industry()
        {
            return blueprintI().industries().Get(0);
        }

        private class Jobs : JobPositions<Instance>
        {
            public Jobs(Instance ins) : base(ins)
            {
            }

            private static readonly long serialVersionUID = 8423260307910904017L;

            protected override bool IsAndInit(int tx, int ty)
            {
                return Get(tx, ty) != null;
            }

            protected override SETT_JOB Get(int tx, int ty)
            {
                return ins.blueprintI().job.Init(tx, ty, ins);
            }
        }

        public int IndustryI()
        {
            return 0;
        }
    }
}