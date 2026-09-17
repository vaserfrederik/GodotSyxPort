using System;
using settlement.room.main;
using settlement.room.service.module;
using snake2d;
using util.rendering;
using util.datatypes;

namespace settlement.room.law.stocks
{
    final class Instance : RoomInstance, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        readonly RoomServiceInstance service;
        byte available;

        protected Instance(ROOM_STOCKS b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            service = new RoomServiceInstance((int)b.constructor.spectators.get(this), blueprintI().data);
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    Tile t = blueprintI().tile.get(c.x(), c.y());
                    if (t != null)
                    {
                        t.init();
                    }
                }
            }
            activate();
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override void activateAction()
        {
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    Tile t = blueprintI().tile.get(c.x(), c.y());
                    if (t != null)
                    {
                        t.stateSet(STATE.available);
                    }
                }
            }
        }

        protected override void deactivateAction()
        {
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    Tile t = blueprintI().tile.get(c.x(), c.y());
                    if (t != null)
                    {
                        t.stateSet(STATE.none);
                    }
                }
            }
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            if (day)
                service.updateDay();
        }

        protected override void dispose()
        {
            service.dispose(blueprintI().data);
        }

        public override ROOM_STOCKS blueprintI()
        {
            return (ROOM_STOCKS)blueprint();
        }

        public override RoomServiceInstance service()
        {
            return service;
        }

        public override double quality()
        {
            return ROOM_SERVICER.defQuality(this, 1);
        }
    }
}