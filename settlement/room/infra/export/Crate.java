package settlement.room.infra.export;

import init.resources.RESOURCE;
import settlement.main.SETT;
import settlement.misc.util.TILE_STORAGE;
import snake2d.util.bit.Bits;

final class Crate implements TILE_STORAGE{

	
	private final Bits bAmount 			= new Bits(0b0000_0000_0000_0000_0000_0011_1111_1111);
	private final Bits bReserved 		= new Bits(0b0000_0000_0000_1111_1111_1100_0000_0000);
	private final Bits bReservedSpace 	= new Bits(0b0011_1111_1111_0000_0000_0000_0000_0000);
	protected final ROOM_EXPORT b;
	int tx, ty;
	ExportInstance ins;
	
	Crate(ROOM_EXPORT b){
		this.b = b;
	}
	
	Crate get(int tx, int ty) {
		if (b.is(tx, ty)) {
			ins = b.getter.get(tx, ty);
			if (b.constructor.isCrate(tx, ty)) {
				this.tx = tx;
				this.ty = ty;
				return this;
			}
		}
		return null;
	}

	@Override
	public RESOURCE resource() {
		return ins.resource();
	}
	
	public int amount(int data) {
		return bAmount.get(data);
	}
	
	private void remove() {
		RESOURCE r = resource();
		if (r != null) {
			b.tally.inc(r, -ins.amount, -ExportInstance.crateMax*(ins.crates));
			ins.amount -= amount();
			ins.amountReserved -= reserved();
			ins.spaceReserved -= storageReserved();
			b.tally.inc(r, ins.amount, ExportInstance.crateMax*(ins.crates));
		}
	}
	
	private void add() {
		RESOURCE r = resource();
		if (r != null) {
			b.tally.inc(r, -ins.amount, -ExportInstance.crateMax*(ins.crates));
			ins.amount += amount();
			ins.amountReserved += reserved();
			ins.spaceReserved += storageReserved();
			b.tally.inc(r, ins.amount, ExportInstance.crateMax*(ins.crates));
		}
	}
	
	public int amount() {
		return bAmount.get(data());
	}
	
	public void amountSet(int am) {
		remove();
		int d = bAmount.set(data(), am);
		save(d);
		add();
	}
	
	public int reserved() {
		return bReserved.get(data());
	}
	
	public void reservedSet(int r) {
		remove();
		int d = bReserved.set(data(), r);
		save(d);
		add();
	}
	

	
	private int data() {
		return SETT.ROOMS().data.get(tx, ty);
	}
	
	private void save(int d) {
		SETT.ROOMS().data.set(ins, tx, ty, d);
	}
	
	void clear() {
		if (resource() == null)
			return;
		remove();
		save(0);
		
	}

	@Override
	public int x() {
		return tx;
	}

	@Override
	public int y() {
		return ty;
	}

	@Override
	public void storageDeposit(int amount) {
		remove();
		int d = bReservedSpace.inc(data(), -amount);
		d = bAmount.inc(d, amount);
		save(d);
		add();
		
	}

	@Override
	public int storageReservable() {
		return ExportInstance.crateMax-bAmount.get(data())-bReservedSpace.get(data());
	}

	@Override
	public int storageReserved() {
		return bReservedSpace.get(data());
	}

	@Override
	public void storageReserve(int amount) {
		remove();
		int d = bReservedSpace.inc(data(), amount);
		save(d);
		add();
	}

	@Override
	public void storageUnreserve(int amount) {
		remove();
		int d = bReservedSpace.inc(data(), -amount);
		save(d);
		add();
	}
	
	@Override
	public boolean storageIsFindable() {
		return false;
	}

	
}
