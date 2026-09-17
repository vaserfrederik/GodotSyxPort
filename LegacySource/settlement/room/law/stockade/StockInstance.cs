using System;
using System.Collections.Generic;
using System.Linq;
using init.resources;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.types.prisoner;
using settlement.main;
using settlement.maintenance;
using settlement.misc.job;
using settlement.misc.util;
using settlement.path;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.job;
using settlement.stats;
using snake2d;
using util.rendering;
using util.misc;

namespace settlement.room.law.stockade
{
    [Serializable]
    internal sealed class StockInstance : RoomInstance, JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE
    {
        private const long serialVersionUID = 1L;

        public readonly short prisonersMax;
        public short prisonersCurrent;
        public readonly RBITImp fetch = new RBITImp();
        private long[] productionData;
        public readonly Jobs jobs;
        public float riotChance = 1;
        public bool hasWarned = false;

        public StockInstance(ROOM_STOCKADE p, TmpArea area, RoomInit init) : base(p, area, init)
        {
            prisonersMax = (short)Math.Ceiling(blueprintI().constructor.prisoners.get(this));
            double work = blueprintI().constructor.workers.get(this);
            employees().maxSet((int)Math.Ceiling(work));
            employees().neededSet((int)Math.Ceiling(work));
            productionData = blueprintI().indu.makeData();
            jobs = new Jobs(this);
            foreach (ResGEat e in RESOURCES.EDI().all())
                if (e.serve)
                    fetch.or(e.resource);
            activate();
        }

        protected override void loadFix()
        {
            productionData = blueprintI().indu.makeDataFix(productionData);
        }

        public override void updateTileDay(int tx, int ty)
        {
            base.updateTileDay(tx, ty);
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            blueprintI().indu.updateRoom(this);
            jobs.searchAgain();
            if (day && prisonersCurrent > 0)
            {
                float prev = riotChance;

                double v = 2.0 * employees().employed() / employees().max() - 1;
                v /= 2;

                if (!jobs.resNotFound.isClear())
                    v -= 1;
                riotChance += v;
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
                    foreach (ENTITY e in SETT.ENTITIES().getAllEnts())
                    {
                        if (e is Humanoid)
                        {
                            Humanoid a = (Humanoid)e;
                            if (AIModule_Prisoner.isPrisoner(a, this))
                            {
                                STATS.LAW().escapeInc();
                                a.kill(false, CAUSE_LEAVES.OTHER());
                            }
                        }
                    }
                }
            }
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            bool ret = base.render(r, shadowBatch, it);
            blueprintI().constructor.renderFence(r, shadowBatch, it, 0, true);
            return ret;
        }

        protected override void activateAction()
        {
            blueprintI().prisoners += prisonersCurrent;
            blueprintI().prisonersMax += prisonersMax;
        }

        protected override void deactivateAction()
        {
            foreach (ENTITY e in SETT.ENTITIES().getAllEnts())
            {
                if (e != null && e is Humanoid)
                {
                    Humanoid h = (Humanoid)e;
                    HEvent.Handler.removeRoom(h, this);
                }
            }
            blueprintI().prisoners -= prisonersCurrent;
            blueprintI().prisonersMax -= prisonersMax;
            prisonersCurrent = 0;
        }

        protected override void dispose()
        {
        }

        public JOB_MANAGER getWork()
        {
            return jobs;
        }

        public ROOM_STOCKADE blueprintI()
        {
            return (ROOM_STOCKADE)blueprint();
        }

        protected override AVAILABILITY getAvailability(int tile)
        {
            int tx = tile % SETT.TWIDTH;
            int ty = tile / SETT.TWIDTH;
            if (blueprintI().constructor.isFence(this, tx, ty))
                return AVAILABILITY.SOLID;
            return base.getAvailability(tile);
        }

        public override void destroyTile(int tx, int ty)
        {
            base.destroyTile(tx, ty);
        }

        public override bool destroyTileCan(int tx, int ty)
        {
            return getAvailability(tx + ty * SETT.TWIDTH).player < 0;
        }

        public ROOM_DEGRADER degrader(int tx, int ty)
        {
            return null;
        }

        public RESOURCE_TILE resourceTile(int tx, int ty)
        {
            return null;
        }

        public long[] productionData()
        {
            return productionData;
        }

        public Industry industry()
        {
            return blueprintI().indu;
        }

        public int industryI()
        {
            return 0;
        }

        public sealed class Jobs : JobPositions<StockInstance>
        {
            private const long serialVersionUID = 1L;

            public Jobs(StockInstance ins) : base(ins)
            {
                setAlwaysNew();
            }

            protected override bool isAndInit(int tx, int ty)
            {
                return get(tx, ty) != null;
            }

            protected override SETT_JOB get(int tx, int ty)
            {
                return ins.blueprintI().job.job(tx, ty);
            }
        }
    }
}