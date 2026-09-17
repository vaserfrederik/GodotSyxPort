package settlement.room.main.util;

import settlement.main.SETT;
import settlement.room.main.ROOMA;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.misc.CLAMP;
import util.data.INT;

public class RoomBits implements INT{

	protected final Bits bits;
	private final COORDINATE coo;
	
	public RoomBits(COORDINATE coo, int mask){
		bits = new Bits(mask);
		this.coo = coo;
	}
	
	public RoomBits(COORDINATE coo, Bits bits){
		this.bits = bits;
		this.coo = coo;
	}
	
	@Override
	public int get() {
		return bits.get(SETT.ROOMS().data.get(coo));
	}
	
	public int get(int rawData) {
		return bits.get(rawData);
	}
	
	public int get(int tx, int ty) {
		return bits.get(SETT.ROOMS().data.get(tx, ty));
	}

	@Override
	public int min() {
		return 0;
	}

	@Override
	public int max() {
		return bits.mask;
	}

	protected void remove() {
		
	}
	
	protected void add() {
		
	}
	
	public void set(ROOMA r, int t) {
		remove();
		set(coo.x(), coo.y(), r, t);
		add();
	}
	
	public void set(ROOMA r, DIR d, int t) {
		remove();
		set(coo.x()+d.x(), coo.y()+d.y(), r, t);
		add();
	}
	
	public void set(int tx, int ty, ROOMA r, int t) {
		int d = SETT.ROOMS().data.get(tx, ty);
		d = bits.set(d, t);
		SETT.ROOMS().data.set(r, tx, ty, d);
	}
	
	public void inc(ROOMA r, int i) {
		set(r, CLAMP.i(get()+i, 0, bits.mask));
	}

}
