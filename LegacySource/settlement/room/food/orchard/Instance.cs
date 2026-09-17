using System;
using System.Collections.Generic;
using settlement.main;
using settlement.entity.animal;
using settlement.entity.humanoid;
using settlement.maintenance;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.construction;
using settlement.room.main.job;
using settlement.room.main.util;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.rendering;

namespace settlement.room.food.orchard
{
    public sealed class Instance : RoomInstance, JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE, ANIMAL_ROOM_RUINER
    {
        private static readonly long serialVersionUID = 1L;
        private long[] produceData;
        private double skill;
        private double skillPrev;
        private int skillI;
        public readonly float base;
        public float irri;
        private float irriNext;
        private short irriI;
        public short trees;
        public readonly short treesTotal;

        private readonly short ssx, ssy;
        private byte sdx = 0;
        private byte sdy = 0;

        private readonly JobIterator jobmanager = new JobIterator(this)
        {
            protected override SETT_JOB Init(int tx, int ty)
            {
                OTile t = blueprintI().tile(tx, ty);
                if (t != null)
                    return t.job();
                return null;
            }
        };

        public Instance(ROOM_ORCHARD p, TmpArea area, RoomInit init) : base(p, area, init)
        {
            double t = 0;
            int ssx = -1;
            int ssy = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    if (ssx == -1 && p.constructor.storage.Get(c.x(), c.y(), this) != null)
                    {
                        ssx = c.x();
                        ssy = c.y();
                        SETT.ROOMS().data.Set(this, c, 0);
                        p.constructor.storage.Get(c.x(), c.y(), this).Dispose();
                    }
                    irri += SETT.GROUND().MOISTURE_TOT.Get(c);
                    if (p.tile.Init(c.x(), c.y(), this))
                        t++;
                }
            }
            if (ssx == -1)
                throw new RuntimeException();
            this.ssx = (short)ssx;
            this.ssy = (short)ssy;
            treesTotal = (short)t;
            base = (float)(t / ROOM_ORCHARD.TILES_PER_WORKER);
            int jobs = (int)Math.Ceiling(base);
            employees().MaxSet((int)(jobs * 1.25));
            employees().NeededSet((int)jobs);
            produceData = p.productionData.MakeData();
            Activate();
        }

        protected override void LoadFix()
        {
            produceData = blueprintI().productionData.MakeDataFix(produceData);
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            blueprintI().productionData.UpdateRoom(this);

            if (day)
            {
                jobmanager.SearchAgain();

                if (blueprintI().time.IsDeadDay())
                {
                    skillPrev = skill();
                    skill = 0;
                    skillI = 0;
                }
            }
        }

        public void Deposit(int am)
        {
            if (am == 0)
                return;
            for (int i = 0; i < 4; i++)
            {
                RoomResStorage s = blueprintI().constructor.storage.Get(ssx + sdx, ssy + sdy, this);
                if (s == null)
                {
                    Console.WriteLine($"{ssx} {sdx} {ssy} {sdy}");
                    return;
                }
                sdx++;
                if (sdx >= 2)
                {
                    sdx = 0;
                    sdy++;
                    if (sdy >= 2)
                        sdy = 0;
                }
                am -= s.Deposit(am);
                if (am <= 0)
                    return;
            }

            SETT.THINGS().resources.Create(ssx, ssy, industry().outs().Get(0).resource, am);
        }

        protected override bool CanRemoveAndRemoveAction(int tx, int ty, bool scatter, object obj, bool forced)
        {
            if (scatter)
            {
                foreach (COORDINATE c in body())
                {
                    if (is(c))
                    {
                        OTile t = blueprintI().tile.GetM(c.x(), c.y());
                        if (t != null)
                            t.Chop();
                    }
                }
            }
            return true;
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            int d = SETT.ROOMS().fData.spriteData2.Get(it.tile());
            if (d != 0)
            {
                blueprintI().constructor.sEdge.Render(r, shadowBatch, d, it, 0, false);
            }

            return base.Render(r, shadowBatch, it);
        }

        public bool CanBeGraced(int tx, int ty)
        {
            OTile t = blueprintI().tile(tx, ty);
            return t != null && t.DestroyTileCan();
        }

        public void Grace(int tx, int ty)
        {
            blueprintI().tile(tx, ty).DestroyTile();
        }

        protected override void ActivateAction()
        {
            // TODO Auto-generated method stub
        }

        protected override void DeactivateAction()
        {
            // TODO Auto-generated method stub
        }

        protected override void Dispose()
        {
            foreach (COORDINATE c in body())
            {
                if (blueprintI().constructor.storage.Get(c.x(), c.y(), this) != null)
                    blueprintI().constructor.storage.Get(c.x(), c.y(), this).Dispose();
            }
        }

        public JOB_MANAGER GetWork()
        {
            return jobmanager;
        }

        public ROOM_ORCHARD BlueprintI()
        {
            return (ROOM_ORCHARD)blueprint();
        }

        public bool AcceptsWork()
        {
            return true;
        }

        public void DestroyTile(int tx, int ty)
        {
            if (DestroyTileCan(tx, ty))
            {
                blueprintI().tile(tx, ty).DestroyTile();
            }
        }

        public bool DestroyTileCan(int tx, int ty)
        {
            OTile t = blueprintI().tile(tx, ty);
            return t != null && t.DestroyTileCan();
        }

        public ROOM_DEGRADER Degrader(int tx, int ty)
        {
            return null;
        }

        public long[] ProductionData()
        {
            return produceData;
        }

        public Industry Industry()
        {
            return blueprintI().industries().Get(0);
        }

        public int IndustryI()
        {
            // TODO Auto-generated method stub
            return 0;
        }

        public void IncSkill(double skill)
        {
            this.skill += skill;
            this.skillI++;
        }

        public void ChangeTo(ROOM_ORCHARD f)
        {
            ConstructionInit init = new ConstructionInit(0, f.constructor, null, 0, MakeState(mX(), mY(), false));
            TmpArea a = Remove(mX(), mY(), false, this, true);

            SETT.ROOMS().construction.CreateClean(a, init);
        }

        protected override void UpdateTileDay(int tx, int ty)
        {
            OTile t = blueprintI().tile(tx, ty);
            if (t != null)
                t.UpdateDay();

            if (irriI >= area())
            {
                irri = irriNext;
                irriNext = 0;
                irriI = 0;
            }
            irriI++;
            irriNext += SETT.GROUND().MOISTURE_TOT.Get(tx, ty);
        }

        public RESOURCE_TILE ResourceTile(int tx, int ty)
        {
            return blueprintI().constructor.storage.Get(tx, ty, this);
        }

        public double Skill()
        {
            if (skillI == 0)
                return skillPrev;
            return skill / skillI;
        }

        public bool Event()
        {
            bool ff = false;
            foreach (COORDINATE c in body())
            {
                if (is(c) && RND.rBoolean())
                {
                    OTile t = blueprintI().tile.GetM(c.x(), c.y());
                    if (t != null)
                        ff |= t.Kill();
                }
            }
            return ff;
        }

        public override double ProductionRate(RoomInstance ins, Humanoid h, Industry in, IndustryResource oo)
        {
            return base.ProductionRate(ins, h, in, oo) * trees / treesTotal;
        }
    }
}