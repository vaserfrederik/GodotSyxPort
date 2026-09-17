package settlement.room.main.util;

import settlement.room.main.RoomBlueprintImp;

public final class RoomInit {

	public final double[] res;
	public final double[] stats;
	public double resMul = 1;
	public final int degrade;
	
	public RoomInit(RoomBlueprintImp b, int degrade) {
		if (b.constructor() != null) {
			res = new double[b.constructor().resources()];
			stats = new double[b.constructor().stats().size()];
		}
		else {
			stats = new double[0];
			res = new double[0];
		}
		this.degrade = degrade;
	}
	
}
