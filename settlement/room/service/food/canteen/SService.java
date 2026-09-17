package settlement.room.service.food.canteen;

import init.resources.RESOURCES;
import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.Coo;
import snake2d.util.misc.CLAMP;
import util.data.INT.INTE;

class SService implements FSERVICE {

	public final static int I = 2;
	public final static int MAX = 3;
	private final ROOM_CANTEEN e;
	private CanteenInstance ins;
	private final Coo coo = new Coo();
	
	private int data;
	
	private final INTE reserved = new INTE() {
		
		private final Bits bits = new Bits(0x000F);
		
		@Override
		public int min() {
			return 0;
		}
		
		@Override
		public int max() {
			return CLAMP.i(MAX, 0, RESOURCES.EDI().all().size());
		}
		
		@Override
		public int get() {
			return bits.get(data);
		}
		
		@Override
		public void set(int t) {
			data = bits.set(data, t);
		}
	};
	
	private final INTE available = new INTE() {
		
		private final Bits bits = new Bits(0x00F0);
		
		@Override
		public int min() {
			return 0;
		}
		
		@Override
		public int max() {
			return CLAMP.i(MAX, 0, RESOURCES.EDI().all().size());
		}
		
		@Override
		public int get() {
			return bits.get(data);
		}
		
		@Override
		public void set(int t) {
			data = bits.set(data, t);
		}
	};
	
	private int total() {
		return reserved.get() + available.get();
	}
	
	SService(ROOM_CANTEEN e){
		this.e = e;
	}
	
	
	SService get(int tx, int ty) {
		if (e.is(tx, ty) && SETT.ROOMS().fData.tileData.get(tx, ty) == I) {
			ins = e.getter.get(tx, ty);
			coo.set(tx, ty);
			data = SETT.ROOMS().data.get(tx, ty);
			return this;
		}
		return null;
	}
	
//	int consume(int am) {
//		am = CLAMP.i(am, 0, available.get());
//		
//		if (ins.amountTotal() > ins.serviceReserved() && total() < MAX) {
//			available.inc(1);
//		}
//		
//		if (ins.amountTotal() <= 0 || (ins.amountTotal() < ins.serviceReserved())) {
//			if (available.get() > 0)
//				available.inc(-1);
//			else if(reserved.get() > 0)
//				reserved.inc(-1);
//		}
//		save();
//	}
	
	void check() {
		if (ins.amountTotal() > ins.serviceReserved() && total() < MAX) {
			available.inc(1);
		}
		
		if (ins.amountTotal() <= 0 || (ins.amountTotal() < ins.serviceReserved())) {
			if (available.get() > 0)
				available.inc(-1);
			else if(reserved.get() > 0)
				reserved.inc(-1);
		}
		save();
	}
	
	private void save() {
		int tmp = data;
		data = SETT.ROOMS().data.get(coo);
		if (tmp == data)
			return;
		ins.serviceTally(-available.get());
		ins.service.report(this, e.service, -1, available.get()-reserved.get(), reserved.get());
		
		data = tmp;
		SETT.ROOMS().data.set(ins, coo, data);
		
		ins.service.report(this, e.service, 1, available.get()-reserved.get(), reserved.get());
		ins.serviceTally(available.get());
	}
	
	void dispose(int tx, int ty) {
		if (get(tx, ty) != null) {
			available.set(0);
			reserved.set(0);
			save();
		}
		
	}
	
	@Override
	public boolean findableReservedCanBe() {
		return available.get() > reserved.get();
	}

	@Override
	public void findableReserve() {
		reserved.inc(1);
		save();
	}

	@Override
	public boolean findableReservedIs() {
		return reserved.get() > 0;
	}

	@Override
	public void findableReserveCancel() {
		reserved.inc(-1);
		save();
		check();
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
	public void consume() {
		findableReserveCancel();
	}
	
}
