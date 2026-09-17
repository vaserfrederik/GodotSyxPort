package settlement.room.infra.hauler;

import settlement.main.SETT;
import settlement.room.main.RoomInstance;
import settlement.room.main.job.StorageCrate;

final class Crate extends StorageCrate{

	static final int size = 80;
	protected final ROOM_HAULER b;
	HaulerInstance ins;
	
	Crate(ROOM_HAULER b) {
		this.b = b;
	}

	@Override
	protected boolean is(int tx, int ty) {
		if (b.is(tx, ty)) {
			ins = b.getter.get(tx, ty);
			if (SETT.ROOMS().fData.tileData.is(tx, ty, 1)) {
				return true;
			}
		}
		return false;
	}
	
	@Override
	public boolean isStorage() {
		return true;
	}
	
	@Override
	public boolean isPrio() {
		return ins.prio();
	}
	
	@Override
	protected void count(int delta) {
		b.tally.report(this, ins, delta);
		ins.updateMasks();
	}
	
	@Override
	public boolean isFindable() {
		return !ins.storing();
	}

	@Override
	protected int max(RoomInstance ins) {
		return size;
	}

	@Override
	protected double spoilRate(RoomInstance ins) {
		return 1.0;
	}

}
