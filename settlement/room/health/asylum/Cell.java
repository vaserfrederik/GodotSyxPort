package settlement.room.health.asylum;

import settlement.main.SETT;
import snake2d.util.bit.Bit;
import snake2d.util.datatypes.Coo;

final class Cell {

	private final static Bit reserved = new Bit(0b0001);
	final Coo coo = new Coo();
	
	private Cell() {
		
	}

	private static Cell self = new Cell();
	private AsylumInstance ins;
	
	static Cell init(int tx, int ty) {
		
		self.ins = b().get(tx, ty);
		if (self.ins == null || SETT.ROOMS().fData.tileData.get(tx, ty) != Constructor.CODE_ENTRANCE)
			return null;
		
		self.coo.set(tx, ty);
		return self;
		
	}

	public void reserve() {
		if (!reservedIs()) {
			int d = reserved.set(SETT.ROOMS().data.get(coo));
			SETT.ROOMS().data.set(ins, coo, d);
			ins.inc(1);
		}
	}

	public void reserveCancel() {
		if (reservedIs()) {
			int d = reserved.clear(SETT.ROOMS().data.get(coo));
			SETT.ROOMS().data.set(ins, coo, d);
			ins.inc(-1);
		}
	}

	public boolean reservedIs() {
		return reserved.is(SETT.ROOMS().data.get(coo));
	}
	
	private static final ROOM_ASYLUM b() {
		return SETT.ROOMS().ASYLUM;
	}
	
}
