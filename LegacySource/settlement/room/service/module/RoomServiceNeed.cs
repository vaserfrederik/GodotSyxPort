using settlement.room.main;
using settlement.room.main.util;
using init.type;

namespace settlement.room.service.module
{
    public abstract class RoomServiceNeed : RoomServiceAccess
    {
        public RoomServiceNeed(RoomBlueprintImp b, RoomInitData data)
            : base(b, data, NEEDS.MAP().Read(data.data().json("SERVICE")))
        {
        }

        public interface ROOM_SERVICE_NEED_HASER : ROOM_SERVICE_ACCESS_HASER
        {
            new RoomServiceNeed service();
        }
    }
}