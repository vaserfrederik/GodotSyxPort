package settlement.room.service.module;

import settlement.entity.humanoid.Humanoid;
import settlement.path.finders.SFinderRoomService;
import settlement.room.service.module.RoomServiceNeed.ROOM_SERVICE_NEED_HASER;

public abstract class ROOM_SPECTATOR extends ROOM_ACTIVITY{

	public abstract RoomServiceAccess service();
	
	@Override
	public SFinderRoomService finder() {
		return service().finder;
	}
	
	public interface ROOM_SPECTATOR_HASER extends ROOM_SERVICE_NEED_HASER{
		
		ROOM_SPECTATOR spec();
		
	}
	
	
	
	public boolean isOpenNow() {
		return true;
	}
	
	public void doSomeThingExtraWhenAccess(Humanoid a) {
		
	}
	
}
