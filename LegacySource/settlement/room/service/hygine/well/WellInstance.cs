using System;
using settlement.room.main;
using settlement.room.service.module;
using snake2d;
using util.rendering;

namespace settlement.room.service.hygine.well
{
    public sealed class WellInstance : RoomInstance, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        private readonly RoomServiceInstance service;

        protected WellInstance(ROOM_WELL b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            int am = 0;
            foreach (COORDINATE c in body())
                if (Is(c) && b.bed.Get(c.x(), c.y()) != null)
                    am++;

            service = new RoomServiceInstance(am, blueprintI().Data);
            foreach (COORDINATE c in body())
                if (Is(c))
                    b.bed.Init(c.x(), c.y());

            Activate();
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
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
            if (day)
                service.UpdateDay();
        }

        protected override void Dispose()
        {
            foreach (COORDINATE c in body())
            {
                if (Is(c))
                {
                    Wash t = blueprintI().bed.Get(c.x(), c.y());
                    if (t != null)
                        t.Dispose();
                }
            }
            service.Dispose(blueprintI().Data);
        }

        public override ROOM_WELL BlueprintI()
        {
            return (ROOM_WELL)Blueprint();
        }

        public override RoomServiceInstance Service()
        {
            return service;
        }

        public override double Quality()
        {
            return ROOM_SERVICER.DefQuality(this, 1);
        }
    }
}