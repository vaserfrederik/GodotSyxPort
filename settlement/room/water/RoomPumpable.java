package settlement.room.water;

import settlement.main.SETT;
import snake2d.util.datatypes.DIR;

public abstract class RoomPumpable {

	protected abstract void drain(int tx, int ty);
	
	
	protected abstract void pump(int tx, int ty, DIR from, int dirmask);

	protected void pumpFail(int tx, int ty, int dirmask) {
		
	}
	
	/**
	 * 
	 * @param tx
	 * @param ty
	 * @return a number that is asked before being drained. This number then goes to pump, if pumped
	 */
	protected abstract int dirmask(int tx, int ty);
	protected int radius() {
		return 0;
	}
	
	public static void reportChange(int tx, int ty, int radius) {
		SETT.ROOMS().WATER.updater.reportChange(tx, ty, radius);
	}
	
	protected abstract boolean pumpsTo(int fromX, int fromY, int tx, int ty);
	
	public double suckAmount(int tx, int ty) {
		return 1;
	}
	
	public abstract double irrigation(int tx, int ty);
	
	
	public interface ROOM_PUMPABLE {
		
		public RoomPumpable pumpable(int tx, int ty);
		
	}
}
