using System;
using System.Collections.Generic;
using game;
using game.faction;
using game.time;
using init.resources;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.main;
using settlement.room.main.util;
using snake2d;
using util.rendering;

namespace settlement.room.food.cannibal
{
    [Serializable]
    public class CannibalInstance : RoomInstance
    {
        private static readonly long SerialVersionUID = 1L;

        private readonly ArrayCooShort coos;
        public readonly ArrayCooShort cages;
        private int[] resources;
        private byte year = (byte)TIME.years().bitsSinceStart();
        public short prisoners;
        public short reservable;

        public CannibalInstance(ROOM_CANNIBAL blue, TmpArea area, RoomInit init) : base(blue, area, init)
        {
            resources = Alloc.ii(blue.resources().Length);
            int am = 0;
            int ca = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c) && blue.job.init(c.x(), c.y(), this) != null)
                {
                    am++;
                }
                if (is(c) && blue.cage(c.x(), c.y()) != null)
                {
                    ca++;
                }
            }

            coos = new ArrayCooShort(am);
            cages = new ArrayCooShort(ca);
            am = 0;
            ca = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c) && blue.job.init(c.x(), c.y(), this) != null)
                {
                    coos.set(am++).set(c);
                }
                if (is(c) && blue.cage(c.x(), c.y()) != null)
                {
                    cages.get().set(c);
                    cages.inc();
                    ca++;
                }
            }
            cages.set(0);
            employees().maxSet(ca);
            employees().neededSet((int)Math.Ceiling(ca / 2.0));
            activate();
        }

        protected override void loadFix()
        {
            if (resources.Length != blueprintI().resources().Length)
            {
                resources = Alloc.ii(blueprintI().resources().Length);
            }
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override void activateAction()
        {
        }

        protected override void deactivateAction()
        {
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            byte y = (byte)TIME.years().bitsSinceStart();
            if (year != y)
            {
                year = y;
                for (int i = 0; i < resources.Length; i++)
                {
                    resources[i] = 0;
                }
            }
        }

        protected override void dispose()
        {
        }

        public SETT_JOB getWork()
        {
            for (int i = 0; i < coos.size(); i++)
            {
                coos.inc();
                SETT_JOB j = blueprintI().job.init(coos.get().x(), coos.get().y(), this);
                if (!j.jobReservedIs(null))
                    return j;
            }

            for (int i = 0; i < coos.size(); i++)
            {
                coos.inc();
                SETT_JOB j = blueprintI().job.init(coos.get().x(), coos.get().y(), this);
                j.jobReserveCancel(null);
            }

            for (int i = 0; i < coos.size(); i++)
            {
                coos.inc();
                SETT_JOB j = blueprintI().job.init(coos.get().x(), coos.get().y(), this);
                if (!j.jobReservedIs(null))
                    return j;
            }

            return null;
        }

        public void resetGore(COORDINATE c)
        {
            blueprintI().job.reset(this, c);
        }

        public void gore(COORDINATE c)
        {
            blueprintI().job.gore(this, c);
        }

        public SETT_JOB getWork(COORDINATE c)
        {
            return blueprintI().job.init(c.x(), c.y(), this);
        }

        public override ROOM_CANNIBAL blueprintI()
        {
            return (ROOM_CANNIBAL)blueprint();
        }

        public override RESOURCE_TILE resourceTile(int tx, int ty)
        {
            return null;
        }

        public int produce(RESOURCE res, int am)
        {
            int i = 0;
            foreach (RESOURCE r in blueprintI().resources())
            {
                if (r == res)
                {
                    resources[i] += am;
                    break;
                }
                i++;
            }
            blueprintI().produced[res.index()] += am;
            GAME.player().res().inc(res, RTYPE.PRODUCED, am);
            return i;
        }
    }
}