package settlement.room.service.arena.grand;

import settlement.misc.util.FSERVICE;
import settlement.room.main.util.RoomBits;
import snake2d.util.datatypes.Coo;

final class Service implements FSERVICE{

	private final Coo coo = new Coo();
	private final RoomBits bAvailable = new RoomBits(coo, 0b0001_0000_0000);
	private ArenaInstance ins;
	private final ROOM_ARENA b;
	
	Service(ROOM_ARENA b){
		this.b = b;
	}
	
	public FSERVICE get(int tx, int ty) {
		if (init(tx, ty))
			return this;
		return null;
	}
	
	public boolean init(int tx, int ty) {
		ins = b.getter.get(tx, ty);
		if (ins != null && b.constructor.util.service(tx, ty)) {
			coo.set(tx, ty);
			return true;
		}
		return false;
	}
	
	@Override
	public void consume() {
		
	}
	
	@Override
	public int x() {
		return coo.x();
	}

	@Override
	public int y() {
		return coo.y();
	}

	@Override
	public boolean findableReservedCanBe() {
		return bAvailable.get() == 1;
	}

	@Override
	public void findableReserve() {
		if (!findableReservedCanBe()) {
			throw new RuntimeException();	
		}
		ins.service.report(this, ins.blueprintI().data, -1);
		bAvailable.set(ins, 0);
	}

	@Override
	public boolean findableReservedIs() {
		return bAvailable.get() == 0;
	}

	@Override
	public void findableReserveCancel() {
		if (findableReservedCanBe())
			return;
		bAvailable.set(ins, 1);
		ins.service.report(this, ins.blueprintI().data, 1);
		
	}
	
}
