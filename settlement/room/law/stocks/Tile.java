package settlement.room.law.stocks;

import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import snake2d.util.bit.Bits;
import snake2d.util.misc.CLAMP;

final class Tile {

	private final Bits stage = new Bits(0b01111);
	private final Bits available = new Bits(0b01111_0000);
	
	private final ROOM_STOCKS b;
	private int x, y;
	private Instance ins;
	
	Tile(ROOM_STOCKS b){
		this.b = b;
	}
	
	public Tile get(int tx, int ty) {
		if (b.constructor.service(tx, ty)) {
			x = tx;
			y = ty;
			ins = b.get(tx, ty);
			return this;
		}
		return null;
	}
	
	public enum STATE {
		
		none,available,reserved,used
		
	}
	
	private final STATE[] states = STATE.values();
	
	public STATE state() {
		return states[stage.get(SETT.ROOMS().data.get(x, y))];
	}
	
	public boolean used() {
		return state() == STATE.reserved || state() == STATE.used;
	}
	
	public void init() {
		stateSet(STATE.available);
		availableSet(8);
	}
	
	public void stateSet(STATE state) {
		if (state() == STATE.none && state != STATE.none)
			b.total++;
		
		if (state() == STATE.reserved || state() == STATE.used) {
			b.used --;
			ins.available--;
		}
		int d = stage.set(SETT.ROOMS().data.get(x, y), state.ordinal());
		SETT.ROOMS().data.set(ins, x, y, d);
		if (state == STATE.reserved || state == STATE.used) {
			b.used ++;
			ins.available++;
		}
	}
	
	private void availableSet(int am) {
		am = CLAMP.i(am, 0, 8);
		ins.service.report(service, b.data, -1, true);
		
		int d = available.set(SETT.ROOMS().data.get(x, y), am);
		SETT.ROOMS().data.set(ins, x, y, d);
		ins.service.report(service, b.data, 1, true);
	}
	
	public final FSERVICE service = new FSERVICE() {
		
		@Override
		public int y() {
			return y;
		}
		
		@Override
		public int x() {
			return x;
		}
		
		@Override
		public boolean findableReservedIs() {
			return available.get(SETT.ROOMS().data.get(x, y)) < 8;
		}
		
		@Override
		public boolean findableReservedCanBe() {
			return available.get(SETT.ROOMS().data.get(x, y)) > 0;
		}
		
		@Override
		public void findableReserveCancel() {
			availableSet(available.get(SETT.ROOMS().data.get(x, y) + 1));
		}
		
		@Override
		public void findableReserve() {
			availableSet(available.get(SETT.ROOMS().data.get(x, y) - 1));
		}

		@Override
		public void consume() {
			findableReserveCancel();
		}
		
		@Override
		public void startUsing() {
			
		};
	};
	
	
}
