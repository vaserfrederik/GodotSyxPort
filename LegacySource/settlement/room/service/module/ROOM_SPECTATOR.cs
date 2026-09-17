using settlement.entity.humanoid;
using settlement.path.finders;

namespace settlement.room.service.module
{
    public abstract class ROOM_SPECTATOR : ROOM_ACTIVITY
    {
        public abstract RoomServiceAccess Service();

        public override SFinderRoomService Finder()
        {
            return Service().Finder;
        }

        public interface ROOM_SPECTATOR_HASER : ROOM_SERVICE_NEED_HASER
        {
            ROOM_SPECTATOR Spec();
        }

        public bool IsOpenNow()
        {
            return true;
        }

        public void DoSomeThingExtraWhenAccess(Humanoid a)
        {
        }
    }
}