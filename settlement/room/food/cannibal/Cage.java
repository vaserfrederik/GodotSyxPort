package settlement.room.food.cannibal;

import settlement.main.SETT;
import settlement.room.main.util.RoomBits;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

public class Cage{
	
	private final ROOM_CANNIBAL b;
	private CannibalInstance ins;
	private Coo coo = new Coo();
	

	private final int sNone = 0;
	private final int sReserved = 1;
	private final int sInside = 2;
	private final int sFetching = 3;
	
	private final RoomBits state = new RoomBits(coo, new Bits(0b0111)) {
		
		@Override
		protected void remove() {
			if (get() > sNone) {
				ins.prisoners --;
				b.prisoners --;
			}
			if (get() == sInside) {
				ins.reservable --;
			}
			
		};
		
		@Override
		protected void add() {
			if (get() > sNone) {
				ins.prisoners ++;
				b.prisoners ++;
			}
			if (get() == sInside) {
				ins.reservable ++;
			}
		};
		
	};

	Cage(ROOM_CANNIBAL print) {
		this.b = print;
	}
	
	Cage get(int tx, int ty) {
		
		ins = b.get(tx, ty);
		if (ins != null && SETT.ROOMS().fData.tile.is(tx, ty, ins.blueprintI().constructor.cc)) {
			coo.set(tx, ty);
			return this;
		}
		return null;
	}
	
	public boolean available() {
		return state.get() == sNone;
	}
	
	public void prisonerReserve() {
		state.set(ins, sReserved);
	}
	
	public void prisonerArrive() {
		state.set(ins, sInside);
	}
	
	public boolean prisonerOk() {
		return state.get() != sNone; 
	}
	
	public void prisonerCancel() {
		state.set(ins, sNone);
	}
	
	public boolean canGrab() {
		return state.get() == sInside;
	}
	
	public void grab() {
		state.set(ins, sFetching);
	}
	
	public void grabCancel() {
		if (state.get() == sFetching)
			state.set(ins, sInside);
	}
	
	public COORDINATE coo() {
		return coo;
	}
	
}
