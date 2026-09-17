using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Room.Food.Farm
{
    using static Settlement.Main.SETT.ROOMS;

    using Game.Time;
    using Init.Resources;
    using Settlement.Entity.Animal;
    using Settlement.Entity.Humanoid;
    using Settlement.Main;
    using Settlement.Maintenance;
    using Settlement.Misc.Job;
    using Settlement.Misc.Job.SettJob;
    using Settlement.Misc.Util;
    using Settlement.Path;
    using Settlement.Room.Industry.Module;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Construction;
    using Settlement.Room.Main.Job;
    using Settlement.Room.Main.Util;
    using Snake2D;
    using Util.Rendering;
    using Util.Rendering.ShadowBatch;
    using Util.Datatypes;
    using Util.Math;

    public sealed class FarmInstance : RoomInstance, JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE, ANIMAL_ROOM_RUINER
    {
        private static readonly long SerialVersionUID = 1L;
        private long[] produceData;
        public readonly Tile.IData tData = new Tile.IData(this);

        public double irri = 0;
        private double irriNext = 0;
        private short irriI = 0;

        short resX = 0;
        short resY = 0;
        short stoX = 0;
        short stoY = 0;
        bool resTimeout = false;
        bool storeTimeout = false;
        bool isHarvest = false;

        private readonly JobIterator jobmanager = new JobIterator(this)
        {
            Init = (tx, ty) => blueprintI().Tile(tx, ty).Job()
        };

        public FarmInstance(ROOM_FARM p, TmpArea area, RoomInit init) : base(p, area, init)
        {
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    irri += SETT.GROUND().MOISTURE_TOT.Get(c);
                    p.Tile(c.X(), c.Y()).Init(c, this);
                }
            }

            double w = Math.Ceiling(p.Constructor.Workers.Get(this));
            int jobs = (int)Math.Ceiling(w);
            employees().MaxSet((int)(jobs * 1.5));
            employees().NeededSet(jobs);
            produceData = p.ProductionData.MakeData();
            activate();
        }

        protected override void loadFix()
        {
            produceData = blueprintI().ProductionData.MakeDataFix(produceData);
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            blueprintI().ProductionData.UpdateRoom(this);

            if (day)
            {
                jobmanager.SearchAgain();
                tData.UpdateDay();
                storeTimeout = false;
                if (!tData.ShouldStore())
                {
                    resTimeout = false;
                    isHarvest = false;
                }
                else if (!isHarvest)
                {
                    isHarvest = true;
                    resTimeout = false;
                    storeTimeout = false;
                }
            }
        }

        protected override bool renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            blueprintI().Tile(i.Tx(), i.Ty()).RenderTill(r, shadowBatch, i);
            return false;
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            blueprintI().Tile(it.Tx(), it.Ty()).Render(r, shadowBatch, it);
            return false;
        }

        public RESOURCE GetCrop()
        {
            return blueprintI().Crop.Resource;
        }

        public bool CanBeGraced(int tx, int ty)
        {
            return blueprintI().Tile(tx, ty).DestroyTileCan();
        }

        public void Grace(int tx, int ty)
        {
            blueprintI().Tile(tx, ty).DestroyTile();
        }

        protected override void activateAction()
        {
            // TODO Auto-generated method stub
        }

        protected override void deactivateAction()
        {
            // TODO Auto-generated method stub
        }

        protected override void dispose()
        {
            // TODO Auto-generated method stub
        }

        public JOB_MANAGER GetWork()
        {
            return jobmanager;
        }

        public ROOM_FARM BlueprintI()
        {
            return (ROOM_FARM) blueprint();
        }

        public bool AcceptsWork()
        {
            return true;
        }

        protected override AVAILABILITY GetAvailability(int tile)
        {
            return AVAILABILITY.ROOM;
        }

        public void DestroyTile(int tx, int ty)
        {
            if (DestroyTileCan(tx, ty))
            {
                blueprintI().Tile(tx, ty).DestroyTile();
            }
        }

        public bool DestroyTileCan(int tx, int ty)
        {
            return blueprintI().Tile(tx, ty).DestroyTileCan();
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
            return blueprintI().Industries().Get(0);
        }

        public int IndustryI()
        {
            // TODO Auto-generated method stub
            return 0;
        }

        public void ChangeTo(ROOM_FARM f)
        {
            ConstructionInit init = new ConstructionInit(0, f.Constructor, null, 0, MakeState(mX(), mY(), true));
            TmpArea a = Remove(mX(), mY(), false, this, true);

            ROOMS().Construction.CreateClean(a, init);
        }

        public void UpdateTileDay(int tx, int ty)
        {
            blueprintI().Tile(tx, ty).UpdateDay();
            if (irriI >= area())
            {
                irri = irriNext;
                irriNext = 0;
                irriI = 0;
            }
            irriI++;
            irriNext += SETT.GROUND().MOISTURE_TOT.Get(tx, ty);
        }

        public double ProductionRate(RoomInstance ins, Humanoid h, Industry in, IndustryResource oo)
        {
            if (employees().Employed() == 0)
                return 0;
            return Util.Prospect((FarmInstance)ins) / (employees().Employed() * TIME.Years().BitConversion(TIME.Days()));
        }

        public RESOURCE_TILE GetResTile()
        {
            if (!jobmanager.HasSearchedAll())
                return null;
            if (resTimeout || storeTimeout)
                return null;
            if (!tData.ShouldStore())
                return null;

            if (!is(resX, resY))
            {
                resX = (short)body().X1();
                resY = (short)body().Y1();
            }

            for (int dy = 0; dy < body().Height(); dy++)
            {
                for (int dx = 0; dx < body().Width(); dx++)
                {
                    RESOURCE_TILE rr = RESOURCE_TILE.GETTER.Reservable(industry().Outs().Get(0).Resource, false, false, resX, resY);
                    if (rr != null)
                        return rr;

                    resX++;
                    if (resX >= body().X2())
                    {
                        resX = (short)body().X1();
                        resY++;
                        if (resY >= body().Y2())
                        {
                            resY = (short)body().Y1();
                        }
                    }
                }
            }
            resTimeout = true;
            return null;
        }

        public TILE_STORAGE GetStoreTile()
        {
            if (storeTimeout)
                return null;
            if (!tData.ShouldStore())
                return null;

            TILE_STORAGE s = SETT.PATH().Finders.Storage.Getter.Get(stoX, stoY);
            if (s != null && s.StorageReservable() > 0 && s.Resource() == industry().Outs().Get(0).Resource)
                return s;

            COORDINATE cc = SETT.PATH().Finders.Storage.Reserve(mX(), mY(), industry().Outs().Get(0).Resource, Room.MAX_DIM + 48);

            if (cc != null)
            {
                stoX = (short)cc.X();
                stoY = (short)cc.Y();
                s = SETT.PATH().Finders.Storage.Getter.Get(stoX, stoY);
                s.StorageUnreserve(1);
                return s;
            }

            storeTimeout = true;
            return null;
        }

        public static void Main(string[] args)
        {
            double period = 15;
            double degrade = 1 - 0.25 / period;
            double consumption = 4;

            double res = 0;

            for (int i = 0; i < 16; i++)
            {
                res += consumption;
                res /= degrade;
            }

            LOG.Ln(res / (period * consumption));
        }
    }
}