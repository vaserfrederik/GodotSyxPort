using System;
using System.Collections.Generic;
using settlement.main;
using game;
using game.faction;
using init.constant;
using init.resources;
using init.type;
using settlement.entity;
using settlement.entity.animal;
using settlement.entity.humanoid;
using settlement.maintenance;
using settlement.misc.job;
using settlement.misc.util;
using settlement.path;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.thing.ThingsCadavers;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.misc;
using util.rendering;

namespace settlement.room.food.pasture
{
    public sealed class PastureInstance : RoomInstance, JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE
    {
        public const double WORKERPERANIMAL = 0.15;
        private const long serialVersionUID = 1L;

        private readonly short depX, depY;

        public readonly short animalsMax;
        public short animalsCurrent;
        public short animalsCubs = 0;
        public short animalsToFetch;

        public bool missingLivestock = false;
        public bool searchForLivestock = true;

        public bool auto = false;
        private long[] productionData;

        private float skillPrev;
        private float skill;
        public float prodPrev;
        public int work;
        public readonly int workMax;
        public float animalsToDie = 0;

        public float water;
        private short waterCount = 0;
        public float waterN;

        private short industry = 0;

        public PastureInstance(ROOM_PASTURE p, TmpArea area, RoomInit init) : base(p, area, init)
        {
            int dx = mX();
            int dy = mY();
            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                if (blueprintI().s2.get(c.x(), c.y(), this) != null)
                {
                    dx = c.x();
                    dy = c.y();
                    break;
                }
                water += SETT.GROUND().MOISTURE_TOT.get(c.x(), c.y());
            }

            depX = (short)dx;
            depY = (short)dy;
            animalsMax = (short)Math.Ceiling(blueprintI().constructor.ferarea.get(this) * p.ANIMALS_PER_TILE);
            animalsToFetch = animalsMax;

            workMax = (int)(blueprintI().constructor.ferarea.get(this) * ROOM_PASTURE.WORKERS_PER_TILE * blueprintI().jobsPerDay);
            skillPrev = (float)blueprintI().bonus().get(HCLASS_RACE.clP(null, HCLASSES.CITIZEN()));
            double work = blueprintI().constructor.workers.get(this);
            employees().maxSet((int)Math.Ceiling(work) * 2);
            employees().neededSet((int)Math.Ceiling(work));
            productionData = industry().makeData();
            activate();
        }

        protected override void loadFix()
        {
            industry = (short)CLAMP.i(industry, 0, blueprintI().indus.size() - 1);
            productionData = industry().makeDataFix(productionData);
        }

        public override void updateTileDay(int tx, int ty)
        {
            waterCount++;
            waterN += SETT.GROUND().MOISTURE_TOT.get(tx, ty);
            if (waterCount >= 10) // Assuming 10 tiles per day for simplicity
            {
                water += waterN / waterCount;
                waterN = 0;
                waterCount = 0;
            }
        }

        public override bool render(Renderer renderer, int tile)
        {
            return base.render(renderer, tile);
        }

        protected override void activateAction()
        {
            // TODO Auto-generated method stub
        }

        protected override void deactivateAction()
        {
            // TODO Auto-generated method stub
        }

        private RoomResStorage dStorage(RoomResStorage s)
        {
            if (s == blueprintI().s2)
                return blueprintI().s2.get(depX, depY, this);
            for (int k = 0; k < DIR.ORTHO.size(); k++)
            {
                int dx = depX + DIR.ORTHO.get(k).x();
                int dy = depY + DIR.ORTHO.get(k).y();
                RoomResStorage d = s.get(dx, dy, this);
                if (d != null)
                    return d;
            }
            throw new Exception();
        }

        protected override void dispose()
        {
            foreach (ENTITY e in ENTITIES().fillTiles(body()))
            {
                if (is(e.physics.tileC()))
                {
                    if (e is Animal)
                    {
                        Animal a = (Animal)e;
                        if (a.domesticated())
                        {
                            SETT.THINGS().resources.create(e.physics.tileC(), RESOURCES.LIVESTOCK(), 1);
                            a.helloMyNameIsInigoMontoyaYouKilledMyFatherPrepareToDie();
                        }
                    }
                }
            }

            blueprintI().s2.get(depX, depY, this).dispose();
            dStorage(blueprintI().s1).dispose();
            dStorage(blueprintI().s3).dispose();
        }

        public void slaughterAll()
        {
            double produce = 0;
            foreach (ENTITY e in ENTITIES().fillTiles(body()))
            {
                if (is(e.physics.tileC()))
                {
                    if (e is Animal)
                    {
                        Animal a = (Animal)e;
                        bool cub = a.cub();
                        if (a.domesticated())
                        {
                            Cadaver c = a.slaugher();
                            if (c != null)
                                c.makeSkelleton();

                            produce += blueprintI().slaughterAmount(cub, industry());
                        }
                    }
                }
            }

            int i = 0;
            foreach (IndustryResource r in industry().outs())
            {
                if (r.resource == RESOURCES.LIVESTOCK())
                    continue;
                int am = r.inc(this, produce * r.rate);
                RoomResStorage s = dStorage(blueprintI().st[i++]);
                while (am-- > 0 && s.hasRoom())
                {
                    s.deposit();
                }
                if (am > 0)
                {
                    THINGS().resources.create(s.x(), s.y(), r.resource, am);
                }
            }

            if (animalsCurrent != 0)
                GAME.Notify("" + animalsCurrent);
            animalsCurrent = 0;
            animalsCubs = 0;
            animalsToFetch = animalsMax;
            work = 0;
            skill = 0;
            animalsToDie = 0;
        }

        public void removeAnimal(bool cub)
        {
            animalsCurrent--;
            if (cub && animalsCubs > 0)
                animalsCubs--;
            if (animalsCurrent < 0)
            {
                GAME.Notify("werid!");
                animalsCurrent = 0;
            }
        }

        public void reportAdult()
        {
            if (animalsCubs > 0)
                animalsCubs--;
        }

        public JOB_MANAGER getWork()
        {
            return JobManager.init(this);
        }

        public ROOM_PASTURE blueprintI()
        {
            return (ROOM_PASTURE)blueprint();
        }

        protected override AVAILABILITY getAvailability(int tile)
        {
            int tx = tile % SETT.TWIDTH;
            int ty = tile / SETT.TWIDTH;
            if (blueprintI().constructor.isFence(this, tx, ty))
                return blueprintI().isIndoors ? AVAILABILITY.AVOID_PASS : AVAILABILITY.SOLID;
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
            RESOURCE_TILE t = blueprintI().s1.get(tx, ty, this);
            if (t != null)
                return t;
            t = blueprintI().s2.get(tx, ty, this);
            if (t != null)
                return t;
            return blueprintI().s3.get(tx, ty, this);
        }

        public long[] productionData()
        {
            return productionData;
        }

        double skill()
        {
            if (work == 0)
                return skillPrev;
            if (work < 5)
            {
                double d = work / 5.0;
                return skillPrev * (1 - d) + (skill * d) / work;
            }
            return skill / work;
        }

        public int animalsCurrent()
        {
            return animalsCurrent;
        }

        public double productionRate(RoomInstance ins, Humanoid h, Industry in, IndustryResource oo)
        {
            if (employees().employed() == 0)
                return 0;
            return oo.rate * IndustryUtil.roomBonus(this, in) / employees().employed();
        }

        public void setIndustry(int i)
        {
            blueprintI().s2.get(depX, depY, this).dispose();
            dStorage(blueprintI().s1).dispose();
            dStorage(blueprintI().s3).dispose();
            Industry in = blueprintI().industries().get(i);
            if (in == null)
                return;
            productionData = in.makeData();
            industry = (short)i;
        }

        public Industry industry()
        {
            return blueprintI().industries().getC(industryI());
        }

        public int industryI()
        {
            return industry;
        }
    }
}