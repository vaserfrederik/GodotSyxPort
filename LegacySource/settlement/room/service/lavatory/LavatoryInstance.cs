using System;
using System.Collections.Generic;
using settlement.main;
using game;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.job;
using settlement.room.main.util;
using settlement.room.service.module;
using settlement.room.service.lavatory;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.rendering;

namespace settlement.room.service.lavatory
{
    [Serializable]
    public sealed class LavatoryInstance : RoomInstance, JOBMANAGER_HASER, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        private readonly Jobs jobs;

        private readonly ArrayCooShort extras;
        private int extraI;
        private readonly RoomServiceInstance service;
        private bool auto = true;

        protected LavatoryInstance(ROOM_LAVATORY blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            jobs = new Jobs(this);

            service = new RoomServiceInstance(jobs.Size, blueprintI().data);

            employees().MaxSet(jobs.Size);
            employees().NeededSet((int)Math.Ceiling(blueprint.Constructor.workers.Get(this)));

            int e = 0;
            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                Lavatory ll = Lavatory.Get(c.X(), c.Y());
                if (ll != null)
                    ll.Init(service);
                FurnisherItemTile it = ROOMS().fData.tile.Get(c);
                if (it != null)
                {
                    int d = it.data();
                    if ((d & Lavatory.BIT_WASH) == Lavatory.BIT_WASH)
                        e++;
                }
            }
            extras = new ArrayCooShort(e);
            e = 0;
            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;

                FurnisherItemTile it = ROOMS().fData.tile.Get(c);
                if (it != null)
                {
                    int d = it.data();
                    if ((d & Lavatory.BIT_WASH) == Lavatory.BIT_WASH)
                        extras.Set(e++).Set(c);
                }
            }
            Activate();
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            i.lit();
            return base.Render(r, shadowBatch, i);
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            if (day)
                service.UpdateDay();
            jobs.SearchAgain();
        }

        protected override void ActivateAction()
        {
        }

        protected override void DeactivateAction()
        {
        }

        public JOB_MANAGER GetWork()
        {
            return jobs;
        }

        protected override void Dispose()
        {
            service.Dispose(blueprintI().data);
            for (int i = 0; i < jobs.Size; i++)
            {
                COORDINATE c = jobs.Get(i);
                Lavatory.Get(c.X(), c.Y()).Dispose();
            }
        }

        public COORDINATE GetExtra()
        {
            if (extraI == extras.Size)
                return null;
            return extras.Set(extraI++);
        }

        public void ReturnExtra(int tx, int ty)
        {
            if (!is(tx, ty))
                return;
            int data = ROOMS().data.Get(tx, ty);
            if ((data & Lavatory.BIT_WASH) != Lavatory.BIT_WASH)
                return;
            if (extraI == 0)
            {
                GAME.Notify("WEIRDNESS!");
            }
            else
            {
                extraI--;
                extras.Set(extraI).Set(tx, ty);
            }
        }

        public ROOM_LAVATORY BlueprintI()
        {
            return (ROOM_LAVATORY)Blueprint();
        }

        public RoomServiceInstance Service()
        {
            return service;
        }

        public double Quality()
        {
            return ROOM_SERVICER.DefQuality(this, 0.5 + 0.5 * blueprintI().Constructor.basins.Get(this));
        }

        private class Jobs : JobPositions<LavatoryInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(LavatoryInstance ins) : base(ins)
            {
            }

            protected override SETT_JOB Get(int tx, int ty)
            {
                Lavatory t = Lavatory.Get(tx, ty);
                if (t == null)
                    return null;
                return t.job;
            }

            protected override bool IsAndInit(int tx, int ty)
            {
                FurnisherItemTile it = ROOMS().fData.tile.Get(tx, ty);
                if (it != null)
                {
                    int d = it.data();
                    ROOMS().data.Set(ins, tx, ty, d);
                    if (d == Lavatory.BIT)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}