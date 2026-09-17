package settlement.room.infra.stockpile;

import settlement.room.main.RoomInstance;
import settlement.room.main.job.StorageCrate;

final class Crate extends StorageCrate{

	protected final ROOM_STOCKPILE b;
	StockpileInstance ins;
	
	Crate(ROOM_STOCKPILE b) {
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
		b.tally().report(this, ins, delta);
		ins.updateMasks(resource());
	}
	
	@Override
	public boolean isPrio() {
		return ins.prioritizing();
	}
	
	@Override
	public boolean isStorage() {
		return true;
	}
	
	@Override
	public boolean isFindable() {
		return !ins.storing();
	}

	@Override
	protected int max(RoomInstance ins) {
		return ((StockpileInstance)ins).crateSize(resource());
	}

	@Override
	protected double spoilRate(RoomInstance ins) {
		return 0.5+0.5*(ins.getDegrade());
	}
	

}
