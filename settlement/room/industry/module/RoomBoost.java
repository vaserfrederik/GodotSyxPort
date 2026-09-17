package settlement.room.industry.module;

import settlement.room.main.RoomInstance;
import util.info.INFO;

public interface RoomBoost {
	public INFO info();
	public double get(RoomInstance r);
	public default double min() {
		return 0;
	}
	public default double max() {
		return 1.0;
	}
}