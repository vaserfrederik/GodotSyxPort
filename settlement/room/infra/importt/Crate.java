package settlement.room.infra.importt;

import settlement.room.main.RoomInstance;
import settlement.room.main.job.StorageCrate;

final class Crate extends StorageCrate{

	protected final ROOM_IMPORT b;
	ImportInstance ins;
	
	Crate(ROOM_IMPORT b) {
		this.b = b;
	}

	@Override
	protected boolean is(int tx, int ty) {
		if (b.is(tx, ty)) {
			ins = b.getter.get(tx, ty);
			if (b.constructor.isCrate(tx, ty)) {
				return true;
			}
		}
		return false;
	}
	
	@Override
	protected void count(int delta) {
		ins.count(this, delta);
	}
	
	@Override
	protected int max(RoomInstance ins) {
		return ImportInstance.crateMax;
	}

	@Override
	protected double spoilRate(RoomInstance ins) {
		return 1.0;
	}
	
	@Override
	public boolean isStorage() {
		return false;
	}
	
	@Override
	public boolean isPrio() {
		return false;
	}
	
	@Override
	public boolean storageIsFindable() {
		return false;
	}
	
}