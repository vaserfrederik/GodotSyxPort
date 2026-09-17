package settlement.room.food.hunter;

import static settlement.main.SETT.ROOMS;

import settlement.main.SETT;
import settlement.room.main.util.RoomBits;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

public class Tile {

	public final Coo coo = new Coo();
	
	public final RoomBits reserved = new RoomBits(coo, 		new Bits(0b0000_0001));
	public final RoomBits cadaver = new RoomBits(coo, 		new Bits(0b0000_0010));
	static final Bits gore = 								new Bits(0b0111_0000);
	
	Tile(ROOM_HUNTER h){
		
	}
	
	public Tile init(int tx, int ty, HunterInstance ins) {
		if (!ins.is(tx, ty))
			return null;
		if (SETT.ROOMS().fData.tile.is(tx, ty, ins.blueprintI().constructor.ww)) {
			this.coo.set(tx, ty);
			return this;
		}
		return null;
	}
	
	void reset(HunterInstance ins, COORDINATE c) {
		int d = ROOMS().data.get(c);
		ROOMS().data.set(ins, c, gore.set(d, 0));
	}
	
	void gore(HunterInstance ins, COORDINATE c) {
		int d = ROOMS().data.get(c);
		ROOMS().data.set(ins, c, gore.inc(d, 1));
	}
}
