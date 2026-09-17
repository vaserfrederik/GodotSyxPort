package settlement.room.service.arena.pit;

import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.room.main.util.RoomBits;
import snake2d.util.datatypes.Coo;

final class Service implements FSERVICE{

	private final Coo coo = new Coo();
	private final RoomBits bAvailable = new RoomBits(coo, 0b0001);
	private ArenaInstance ins;
	private final ROOM_FIGHTPIT b;
	
	Service(ROOM_FIGHTPIT b){
		this.b = b;
	}
	
	public FSERVICE get(int tx, int ty) {
		if (init(tx, ty))
			return this;
		return null;
	}
	
	public boolean init(int tx, int ty) {
		ins = b.getter.get(tx, ty);
		if (ins != null && SETT.ROOMS().fData.tileData.get(tx, ty) == ArenaConstructor.STATION) {
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
