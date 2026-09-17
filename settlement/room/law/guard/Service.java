package settlement.room.law.guard;

import settlement.misc.util.FSERVICE;

final class Service implements FSERVICE{

	private GuardInstance ins;
	private int x,y;
	private final ROOM_GUARD b;

	Service(ROOM_GUARD blue) {
		this.b = blue;
	}
	
	Service get(GuardInstance ins) {
		this.ins = ins;
		x = ins.body().cX();
		y = ins.body().cY();
		return this;
	}

	@Override
	public boolean findableReservedCanBe() {
		return b.reporter.available(ins);
	}

	@Override
	public void findableReserve() {
		// TODO Auto-generated method stub
		
	}

	@Override
	public boolean findableReservedIs() {
		return true;
	}

	@Override
	public void findableReserveCancel() {
		// TODO Auto-generated method stub
		
	}

	@Override
	public int x() {
		return x;
	}

	@Override
	public int y() {
		return y;
	}

	@Override
	public void consume() {
		// TODO Auto-generated method stub
		
	}
	
}
