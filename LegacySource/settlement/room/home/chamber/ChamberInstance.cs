using System;
using System.Collections.Generic;
using settlement.main;
using init.type;
using settlement.entity.humanoid;
using settlement.maintenance;
using settlement.misc.job;
using settlement.room.home;
using settlement.room.main;
using snake2d;
using util.rendering;

namespace settlement.room.home.chamber
{
    [Serializable]
    public sealed class ChamberInstance : RoomInstance, JOBMANAGER_HASER, HOME
    {
        private static readonly long serialVersionUID = 1L;
        private readonly JobIterator jobs;
        private readonly COORDINATE serviceCoo;
        private readonly byte sleepDir;
        private int occupant = 0;
        private bool fetching;
        private bool workingWarn = false;
        private bool working;

        protected ChamberInstance(ROOM_CHAMBER b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            jobs = new JobIterator(this)
            {
                Init = (tx, ty) => blueprintI().work.Get(tx, ty)
            };

            jobs.SetAlwaysNewJob();

            COORDINATE s = null;

            foreach (COORDINATE c in body())
            {
                if (Is(c) && ROOMS().fData.tile.Get(c) == blueprintI().constructor.bb)
                {
                    s = new Coo(c);
                }
            }

            if (s == null)
                throw new RuntimeException();

            serviceCoo = s;

            sleepDir = (byte)DIR.S.Next(2 * ROOMS().fData.item.Get(s).rotation).id();

            employees().MaxSet(4);
            employees().NeededSet(4);
            Activate();
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            if (occupant != 0)
                it.lit();
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
            bool warn = workingWarn;
            workingWarn = employees().Employed() < 4;

            if (working && warn && workingWarn)
            {
                if (occupant() != null)
                {
                    STATS.HOME().GETTER.Set(occupant(), null);
                }
                Remove();
                working = false;
                Add();
            }
            else if (!working)
            {
                Remove();
                working = true;
                Add();
            }

            jobs.SearchAgain();
        }

        public JOB_MANAGER GetWork()
        {
            return jobs;
        }

        protected override void Dispose()
        {
            Remove();
            if (occupant() != null)
            {
                STATS.HOME().Dump(occupant());
                STATS.HOME().GETTER.Set(occupant(), null);
            }
        }

        public ROOM_CHAMBER BlueprintI()
        {
            return (ROOM_CHAMBER)blueprint();
        }

        public HOME Vacate(Humanoid h)
        {
            Remove();
            occupant = 0;
            Add();
            return this;
        }

        public HOME Occupy(Humanoid h)
        {
            Remove();
            occupant = h.id();
            Add();
            return this;
        }

        public Humanoid Occupant()
        {
            if (occupant != 0)
                return (Humanoid)SETT.ENTITIES().GetByID(occupant);
            return null;
        }

        public Humanoid Occupant(int oi)
        {
            if (oi == 0)
                return Occupant();
            return null;
        }

        public int Occupants()
        {
            return occupant() != null ? 1 : 0;
        }

        public int ServiceX()
        {
            return serviceCoo.x();
        }

        public int ServiceY()
        {
            return serviceCoo.y();
        }

        public ROOM_DEGRADER Degrader(int tx, int ty)
        {
            return null;
        }

        public double GetDegrade()
        {
            if (working)
                return 0;
            return 0.5;
        }

        private void Remove()
        {
        }

        private void Add()
        {
        }

        public int OccupantsMax()
        {
            return 1;
        }

        public int ResourceAm(int ri)
        {
            if (occupant() == null)
                return 0;
            return STATS.HOME().Current(occupant(), ri);
        }

        public double Isolation()
        {
            return Isolation(mX(), mY());
        }

        public bool CanOccupy(Humanoid h)
        {
            return h.Indu().Clas() == HCLASSES.NOBLE() && occupant() == null;
        }

        public CharSequence TypeName(int tx, int ty)
        {
            return blueprintI().info.name;
        }
    }
}