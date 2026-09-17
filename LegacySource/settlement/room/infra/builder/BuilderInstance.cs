using System;
using settlement.room.infra.builder;
using game.time;
using settlement.main;
using settlement.maintenance;
using settlement.path;
using settlement.room.main;
using settlement.room.main.job;

namespace settlement.room.infra.builder
{
    [Serializable]
    final class BuilderInstance : RoomInstance, ROOM_RADIUS_INSTANCE
    {
        private static readonly long serialVersionUID = 1L;
        private byte radius = 32;
        private byte failHour = -1;

        public BuilderInstance(ROOM_BUILDER blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            SETT.ROOMS().data.set(this, mX(), mY(), 0);
            employees().maxSet(20);
            employees().neededSet(1);
            activate();
        }

        protected override void activateAction()
        {
            // TODO Auto-generated method stub
        }

        protected override void deactivateAction()
        {
            // TODO Auto-generated method stub
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            failHour = (byte)(TIME.hours().bitCurrent() - 1);
        }

        protected override void dispose()
        {
            // TODO Auto-generated method stub
        }

        public override ROOM_BUILDER blueprintI()
        {
            return SETT.ROOMS().BUILDER;
        }

        protected override AVAILABILITY getAvailability(int tile)
        {
            return AVAILABILITY.AVOID_PASS;
        }

        public override bool destroyTileCan(int tx, int ty)
        {
            return false;
        }

        public override ROOM_DEGRADER degrader(int tx, int ty)
        {
            // TODO Auto-generated method stub
            return null;
        }

        public override int radius()
        {
            return radius;
        }

        public override bool searching()
        {
            return true;
        }

        public override byte radiusRaw()
        {
            return radius;
        }

        public override void radiusRawSet(byte r)
        {
            this.radius = r;
        }
    }
}