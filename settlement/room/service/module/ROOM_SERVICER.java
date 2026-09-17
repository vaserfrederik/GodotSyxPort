package settlement.room.service.module;

import settlement.room.main.RoomInstance;
import snake2d.util.misc.CLAMP;

public interface ROOM_SERVICER {

	public RoomServiceInstance service();
	
	public double quality();
	
	public static double defQuality(RoomInstance ins, double base) {
		base *= 1.0 - 0.9*ins.getDegrade();
		//base *= (ins.upgrade()+1.0)/(ins.blueprintI().upgrades().max()+1.0);
		return CLAMP.d(base, 0, 1);
	}
	
}
