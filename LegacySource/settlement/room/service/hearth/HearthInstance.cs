using settlement.main;
using settlement.room.main;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d;
using System.Collections.Generic;
using util.rendering;

namespace settlement.room.service.hearth
{
    public class HearthInstance : RoomInstance, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        public RoomServiceInstance service;
        public short used;

        protected HearthInstance(ROOM_HEARTH b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            int am = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c) && SETT.ROOMS().fData.tileData.is(c, Constructor.codeService))
                {
                    am++;
                }
            }
            service = new RoomServiceInstance(am, blueprintI().data);
            foreach (COORDINATE c in body())
            {
                if (is(c) && SETT.ROOMS().fData.tileData.is(c, Constructor.codeFire))
                {
                    SETT.LIGHTS().fire(c.x(), c.y(), 0);
                    SETT.LIGHTS().hide(c.x(), c.y(), true);
                }
                if (blueprintI().bed.get(c.x(), c.y()) != null)
                    blueprintI().bed.init();
            }
            activate();
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
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
            if (day)
                service.updateDay();
        }

        protected override void dispose()
        {
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    SETT.JOBS().clearer.set(c);
                    Hearth t = blueprintI().bed.get(c.x(), c.y());
                    if (t != null)
                        t.dispose();
                }
            }
            service.dispose(blueprintI().data);
        }

        public override ROOM_HEARTH blueprintI()
        {
            return (ROOM_HEARTH)blueprint();
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