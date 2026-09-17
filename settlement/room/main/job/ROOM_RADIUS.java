package settlement.room.main.job;

import settlement.room.main.Room;

public interface ROOM_RADIUS {
	
	public interface ROOM_RADIUS_INSTANCE {
		public int radius();
		public boolean searching();
		public byte radiusRaw();
		public void radiusRawSet(byte r);
//		public default void reportUnavailability() {
//			
//		}
	}
	
	public interface ROOM_RADIUSE extends ROOM_RADIUS{
		public ROOM_RADIUS_INSTANCE radiusInstance(Room t);
	}
}
