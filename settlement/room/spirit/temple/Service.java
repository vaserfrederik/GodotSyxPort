package settlement.room.spirit.temple;

import settlement.main.SETT;
import settlement.misc.util.FSERVICE;

final class Service {

	private TempleInstance ins;
	private int x, y;
	private final ROOM_TEMPLE blue;
	
	Service(ROOM_TEMPLE blue){
		this.blue = blue;
	}
	
	public FSERVICE get(int tx, int ty) {
		ins = blue.get(tx, ty);
		if (ins != null && blue.constructor.wo == SETT.ROOMS().fData.tile.get(tx, ty)) {
			x = tx;
			y = ty;
			return s;
		}
		return null;
	}
	
	public void init(int tx, int ty) {
		if (get(tx, ty) != null) {
			SETT.ROOMS().data.set(ins, x, y, 1);
			s.findableReserveCancel();
		}
	}
	
	public void dispose(int tx, int ty) {
		if (get(tx, ty) != null) {
			s.findableReserve();
		}
	}
	
	
	
	private final FSERVICE s = new FSERVICE() {
		
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
			return SETT.ROOMS().data.get(x,y) == 1;
		}
		
		@Override
		public boolean findableReservedCanBe() {
			return !findableReservedIs();
		}
		
		@Override
		public void findableReserveCancel() {
			if (findableReservedIs()) {
				SETT.ROOMS().data.set(ins, x, y, 0);
				ins.service.report(s, blue.service, 1);
			}
			
		}
		
		@Override
		public void findableReserve() {
			if (!findableReservedIs()) {
				ins.service.report(s, blue.service, -1);
				SETT.ROOMS().data.set(ins, x, y, 1);
			}
		}
		
		@Override
		public void consume() {
			findableReserveCancel();
		}
	};

}
