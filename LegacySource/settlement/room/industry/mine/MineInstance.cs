using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Room.Industry.Mine
{
    public class MineInstance : RoomInstance, IJobManagerHaser, IRoomProducerInstance
    {
        public JobPositions<MineInstance> Jobs { get; private set; }
        private long[] pData;
        public short sx, sy;

        int workage = 0;
        bool hasStorage = true;

        public MineInstance(ROOM_MINE b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            int x = -1;
            int y = -1;

            foreach (var c in body())
            {
                if (is(c))
                {
                    if (SETT.ROOMS().fData.tileData.get(c) == Constructor.B_WORK)
                    {
                        SETT.ROOMS().data.set(this, c, Job.isWork.set(0));
                    }
                    else if (SETT.ROOMS().fData.tileData.get(c) == Constructor.B_STORAGE)
                    {
                        if (x == -1)
                        {
                            x = c.x();
                            y = c.y();
                        }
                    }
                    else if (SETT.MINERALS().getter.is(c, b.minable) && SETT.ROOMS().fData.item.get(c) == null)
                    {
                        SETT.ROOMS().data.set(this, c, Job.isWork.set(0));
                    }
                }
            }

            if (x == -1 || y == -1)
                GAME.Error(x + " " + y);
            sx = (short)x;
            sy = (short)y;

            pData = b.productionData.makeData();
            Jobs = new Jobs(this);

            Jobs.randomize();
            int w = (int)Math.Floor(b.constructor.workers.get(this));
            employees().maxSet(w);
            employees().neededSet(w);
            activate();
        }

        protected override void loadFix()
        {
            pData = blueprintI().productionData.makeDataFix(pData);
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            return base.render(r, shadowBatch, it);
        }

        protected override bool renderBelow(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            GROUND().renderMinerals(r, i.tile(), i.ran(), i.x(), i.y());
            return base.renderBelow(r, shadowBatch, i);
        }

        protected override void activateAction()
        {
        }

        protected override void deactivateAction()
        {
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            blueprintI().productionData.updateRoom(this);

            if (!active())
                return;
            Jobs.searchAgain();
        }

        protected override void dispose()
        {
            foreach (var c in body())
            {
                if (blueprintI().job.storage.get(c.x(), c.y(), this) != null)
                    blueprintI().job.storage.dispose();
            }
        }

        public JOB_MANAGER getWork()
        {
            return Jobs;
        }

        public ROOM_MINE blueprintI()
        {
            return (ROOM_MINE)blueprint();
        }

        public RESOURCE_TILE resourceTile(int tx, int ty)
        {
            return blueprintI().job.storage.get(tx, ty, this);
        }

        public long[] productionData()
        {
            return pData;
        }

        public Industry industry()
        {
            return blueprintI().industries().get(0);
        }

        public class Jobs : JobPositions<MineInstance>
        {
            public Jobs(MineInstance ins) : base(ins) { }

            protected override bool isAndInit(int tx, int ty)
            {
                return get(tx, ty) != null;
            }

            protected override SETT_JOB get(int tx, int ty)
            {
                return ins.blueprintI().job.init(tx, ty, ins);
            }
        }

        public int industryI()
        {
            return 0;
        }
    }
}