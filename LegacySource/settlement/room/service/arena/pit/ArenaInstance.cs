using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.time;
using settlement.main;
using settlement.room.main;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.rendering;

namespace settlement.room.service.arena.pit
{
    [Serializable]
    final class ArenaInstance : RoomInstance, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        readonly RoomServiceInstance service;
        readonly byte off = (byte)RND.rInt(64);

        short gladiators = 0;
        public readonly byte ax, ay;

        int cheerTime;
        bool cheer;

        static readonly int CHEER_TIME = TIME.secondsPerDay() / 128;

        protected ArenaInstance(ROOM_FIGHTPIT b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            int ss = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c) && b.ser.init(c.x(), c.y()))
                {
                    ss++;
                }
            }

            int ax = 0, ay = 0;
            outer:
            for (int y = 0; y < body().height(); y++)
            {
                for (int x = 0; x < body().width(); x++)
                {
                    if (SETT.ROOMS().fData.tileData.get(body().x1() + x, body().y1() + y) == ArenaConstructor.ARENA)
                    {
                        ax = x;
                        ay = y;
                        break outer;
                    }
                }
            }
            this.ax = (byte)ax;
            this.ay = (byte)ay;

            service = new RoomServiceInstance(ss, blueprintI().data);
            int w = (short)b.constructor.workers.get(this);
            employees().maxSet(w);
            employees().neededSet(w);
            activate();
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override void activateAction()
        {
            cheerTime = (int)(TIME.currentSecond()) - CHEER_TIME;
            blueprintI().incG(gladiators, ROOM_FIGHTPIT.EXECUTIONS);
            foreach (COORDINATE c in body())
            {
                if (is(c) && blueprintI().ser.get(c.x(), c.y()) != null)
                {
                    blueprintI().ser.findableReserveCancel();
                }
            }
        }

        protected override void deactivateAction()
        {
            blueprintI().incG(-gladiators, -ROOM_FIGHTPIT.EXECUTIONS);
            foreach (COORDINATE c in body())
            {
                if (is(c) && blueprintI().ser.get(c.x(), c.y()) != null && blueprintI().ser.findableReservedCanBe())
                {
                    blueprintI().ser.findableReserve();
                }
            }
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            if (day)
            {
                service.updateDay();
            }
        }

        protected override void dispose()
        {
        }

        public override ROOM_FIGHTPIT blueprintI()
        {
            return (ROOM_FIGHTPIT)blueprint();
        }

        public override RoomServiceInstance service()
        {
            return service;
        }

        public override double quality()
        {
            return ROOM_SERVICER.defQuality(this, (double)employees().employed() / employees().max());
        }

        public int gladiatorsNeeded()
        {
            if (active())
                return ROOM_FIGHTPIT.EXECUTIONS - gladiators;
            return 0;
        }

        public void reserveGladiator(int delta)
        {
            gladiators += delta;
            if (active())
            {
                blueprintI().incG(delta, 0);
            }
        }
    }
}