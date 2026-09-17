package settlement.room.service.module;

import init.type.NEEDS;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.util.RoomInitData;

public abstract class RoomServiceNeed extends RoomServiceAccess {

	public RoomServiceNeed(RoomBlueprintImp b, RoomInitData data) {
		super(b, data, NEEDS.MAP().read(data.data().json("SERVICE")));
	}
	
	public interface ROOM_SERVICE_NEED_HASER extends ROOM_SERVICE_ACCESS_HASER{
		
		@Override
		RoomServiceNeed service();

	}

}
