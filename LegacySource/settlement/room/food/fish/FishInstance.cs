using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Room.Food.Fish
{
    using Game;
    using Init.Constant;
    using Settlement.Main;
    using Settlement.Misc.Job;
    using Settlement.Misc.Util;
    using Settlement.Room.Industry.Module;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Job;
    using Settlement.Room.Main.Util;
    using Snake2D.Renderer;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Rnd;
    using Util;
    using Util.Rendering;

    internal sealed class FishInstance : RoomInstance, IJobManagerHolder, IRoomProducerInstance
    {
        public Jobs Jobs { get; private set; }
        private static readonly long SerialVersionUID = -3170637142258642320L;
        private long[] pData;
        public short sx, sy;
        public bool hasStorage = true;

        public FishInstance(ROOM_FISHERY b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            BoatMaker.Make(this);

            int x = -1;
            int y = -1;
            int w = 0;

            foreach (var c in Body())
            {
                if (Is(c))
                {
                    if (SETT.ROOMS().fData.tileData.Get(c) == Constructor.B_WORK)
                    {
                    }
                    else if (SETT.ROOMS().fData.tileData.Get(c) == Constructor.B_STORAGE)
                    {
                        if (x == -1)
                        {
                            x = c.X();
                            y = c.Y();
                        }
                    }
                    else if (SETT.TERRAIN().WATER.SHALLOW.Is(c))
                    {
                        if (Job.IsWork.Is(SETT.ROOMS().data.Get(c)))
                            continue;
                        if (w == 0)
                        {
                            SETT.ROOMS().data.Set(this, c, Job.IsWork.Set(0));
                            w += RND.rInt(2);
                        }
                        else
                        {
                            w--;
                        }
                    }
                }
            }

            if (x == -1 || y == -1)
                GAME.Error(x + " " + y);
            sx = (short)x;
            sy = (short)y;

            pData = b.ProductionData.MakeData();
            Jobs = new Jobs(this);

            Jobs.Randomize();
            Jobs.SetAlwaysNew();

            Employees().MaxSet((int)BlueprintI().Constructor.Workers.Get(this));
            Employees().NeededSet((int)BlueprintI().Constructor.Workers.Get(this));
            Activate();
        }

        protected override void LoadFix()
        {
            base.LoadFix();
            pData = BlueprintI().ProductionData.MakeDataFix(pData);
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            if (!SETT.ROOMS().fData.item.Is(it.Tile()))
            {
                if (Job.IsWork.Is(SETT.ROOMS().data.Get(it.Tile())) && Job.IsShip.Is(SETT.ROOMS().data.Get(it.Tile())))
                {
                    if (!Job.Working(SETT.ROOMS().data.Get(it.Tile())))
                        SETT.HALFENTS().dingy.RenderBoat(r, shadowBatch, it.X() + C.TILE_SIZEH, it.Y() + C.TILE_SIZEH, DIR.ALL.Get(it.Ran()), GUTIL.Ran2().Get(it.Tile()), Upgrade());
                }
                else
                {
                    int d = SETT.ROOMS().fData.spriteData.Get(it.Tile());

                    if (d != 0x0F)
                    {
                        BlueprintI().Constructor.sEdge.Render(r, shadowBatch, d, it, 0, false);
                    }
                    else if (!SETT.TERRAIN().WATER.Is.Is(it.Tile()) && (GUTIL.Ran2().Get(it.Tile()) & 0x0F) == 0)
                    {
                        BlueprintI().Constructor.sMisc.Render(r, shadowBatch, 0, it, 0, false);
                    }
                }
            }

            return base.Render(r, shadowBatch, it);
        }

        protected override void ActivateAction()
        {
        }

        protected override void DeactivateAction()
        {
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            BlueprintI().ProductionData.UpdateRoom(this);

            if (!Active())
                return;
            Jobs.SearchAgain();
        }

        protected override void Dispose()
        {
            foreach (var c in Body())
            {
                if (BlueprintI().Job.Storage.Get(c.X(), c.Y(), this) != null)
                    BlueprintI().Job.Storage.Dispose();
            }
        }

        public JOB_MANAGER GetWork()
        {
            return Jobs;
        }

        public ROOM_FISHERY BlueprintI()
        {
            return (ROOM_FISHERY)Blueprint();
        }

        public RESOURCE_TILE ResourceTile(int tx, int ty)
        {
            return BlueprintI().Job.Storage.Get(tx, ty, this);
        }

        public Industry Industry()
        {
            return BlueprintI().Industries().Get(0);
        }

        public long[] ProductionData()
        {
            return pData;
        }

        internal sealed class Jobs : JobPositions<FishInstance>
        {
            public Jobs(FishInstance ins) : base(ins)
            {
            }

            protected override bool IsAndInit(int tx, int ty)
            {
                return Get(tx, ty) != null;
            }

            protected override SETT_JOB Get(int tx, int ty)
            {
                return Ins.BlueprintI().Job.Init(tx, ty, Ins);
            }
        }

        public int IndustryI()
        {
            return 0;
        }
    }
}