using System;
using System.Collections.Generic;
using settlement.room.main;
using settlement.room.service.module;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace settlement.room.spirit.temple
{
    public class TempleInstance : RoomInstance, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        public readonly RoomServiceInstance service;
        public readonly ArrayCooShort jobs;
        public int consumed = 0;
        public byte year = (byte)(TIME.years().bitsSinceStart());
        public short sacrificesTotal;
        public short sacrifices;
        public short sacrificesRequired = 0;
        public bool resHas = true;
        public readonly int altars;

        protected TempleInstance(ROOM_TEMPLE blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            int s = 0;
            int j = 0;
            int a = 0;
            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                if (blueprint.serviceTile.get(c.x(), c.y()) != null)
                    s++;
                if (blueprint.job.get(c.x(), c.y()) != null)
                    j++;
                if (blueprint.altar.get(c.x(), c.y()) != null)
                {
                    a++;
                }
            }
            altars = a;
            jobs = new ArrayCooShort(j);
            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                if (blueprint.job.get(c.x(), c.y()) != null)
                {
                    jobs.get().set(c);
                    jobs.inc();
                }
            }

            service = new RoomServiceInstance(s, blueprint.service);

            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                blueprint.serviceTile.init(c.x(), c.y());
            }

            employees().maxSet(jobs.size());
            employees().neededSet(jobs.size());

            activate();
        }

        public override ROOM_TEMPLE blueprintI()
        {
            return (ROOM_TEMPLE)blueprint();
        }

        protected override void activateAction()
        {
        }

        protected override void deactivateAction()
        {
            // TODO Auto-generated method stub
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            resHas = true;
            if (day)
            {
                if ((byte)(TIME.years().bitsSinceStart()) != year)
                {
                    consumed = 0;
                    year = (byte)(TIME.years().bitsSinceStart());
                }
                service.updateDay();

                sacrifices = (short)Math.Ceiling(sacrifices / 2.0);
                sacrificesTotal = (short)Math.Ceiling(sacrificesTotal / 2.0);
            }
        }

        public double sacrificeValue()
        {
            if (sacrificesTotal == 0)
                return 0;
            return (double)sacrifices / sacrificesTotal;
        }

        public double respect()
        {
            double d = 0.25;
            d += 0.25 * blueprintI().constructor.grandure.get(this);
            d += 0.25 * blueprintI().constructor.space.get(this);
            d += 0.25 * blueprintI().constructor.decor.get(this);
            d *= (double)employees().employed() / employees().target();
            d *= sacrificeValue();
            return d;
        }

        public int sacrifices()
        {
            return (int)(jobs.size() * blueprintI().STIME * 0.5);
        }

        public override void updateTileDay(int tx, int ty)
        {
            blueprintI().altar.updateday(tx, ty);
        }

        protected override void dispose()
        {
            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                blueprintI().serviceTile.dispose(c.x(), c.y());
                blueprintI().altar.dispose(c.x(), c.y());
            }
            service.dispose(blueprintI().service);
        }

        public override RoomServiceInstance service()
        {
            return service;
        }

        public override double quality()
        {
            double baseValue = (upgrade() + 1.0) / (blueprintI().upgrades().max() + 1.0);
            return ROOM_SERVICER.defQuality(this, baseValue * respect());
        }

        public TempleJob jobReservable(int tx, int ty)
        {
            if (is(tx, ty) && blueprintI().job.get(tx, ty) != null && !blueprintI().job.jobReservedIs())
                return blueprintI().job;
            for (int i = 0; i < jobs.size(); i++)
            {
                TempleJob j = blueprintI().job.get(jobs.get().x(), jobs.get().y());
                jobs.inc();
                if (!j.jobReservedIs())
                    return j;
            }
            return null;
        }

        public TempleJob job(int tx, int ty)
        {
            if (is(tx, ty) && blueprintI().job.get(tx, ty) != null)
                return blueprintI().job;
            return null;
        }

        public void reportMissing()
        {
            resHas = false;
        }
    }
}