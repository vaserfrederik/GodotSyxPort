package settlement.room.service.hygine.bath;

import static settlement.main.SETT.ROOMS;
import static settlement.room.service.hygine.bath.Bits.BITS;
import static settlement.room.service.hygine.bath.Bits.SERVICE;

import settlement.misc.util.FSERVICE;
import settlement.room.main.furnisher.FurnisherItem;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

public class Bath implements FSERVICE{

	static final int BIT = Bits.SERVICE;
	private static final Bath self = new Bath();
	
	private int data;
	private final Coo coo = new Coo();
	private BathInstance ins;
	
	static Bath init(int tx, int ty, ROOM_BATH b) {
		if (!b.is(tx, ty))
			return null;
		int data = ROOMS().data.get(tx, ty);
		if ((data & BITS) != BIT)
			return null;
		self.data = data;
		self.coo.set(tx, ty);
		self.ins = b.get(tx, ty);
		return self;
	}
	
	private Bath() {
		
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
		return reserved() < available();
	}

	@Override
	public void findableReserve() {
		if (reserved() >= available())
			throw new RuntimeException();
		reservedSet(reserved()+1);
		save();
	}

	@Override
	public boolean findableReservedIs() {
		return reserved() > 0;
	}

	@Override
	public void findableReserveCancel() {
		if (reserved() > 0) {
			reservedSet(reserved()-1);
			save();
		}
	}
	
	@Override
	public void consume() {
		if (!findableReservedIs())
			throw new RuntimeException();
		reservedSet(reserved()-1);
		availableSet(available()-1);
		save();
	}
	
	int total() {
		return (data >> 8) & 0x0F;
	}
	
	int available() {
		return (data >> 4) & 0x0F;
	}
	
	private void availableSet(int a) {
		data &= 0xFF0F;
		data |= a << 4;
	}
	
	void availabilityInc() {
		availableSet(available()+1);
		save();
	}
	
	boolean availbilityNeeds() {
		return available() < total();
	}
	
	private int reserved() {
		return data &0x0F;
	}
	
	private void reservedSet(int r) {
		data &= 0xFFF0;
		data |= r;
	}
	
	private void save() {
		
		int old = data;
		data = ROOMS().data.get(coo);
		if (old == data)
			return;
		
		ins.service().report(this, ins.blueprintI().data, -(available()-reserved()));
		int a = available();
		data = old;
		ins.service().report(this, ins.blueprintI().data, (available()-reserved()));
		
		if (available() == 0 && a > 0)
			blip(coo.x(), coo.y(), Bits.POOL);
		else if(available() > 0 && a == 0)
			blip(coo.x(), coo.y(), Bits.POOL_FILLED);
		
		ROOMS().data.set(ins, coo, data);
	}
	
	private void blip(int tx, int ty, int data) {
		
		FurnisherItem it = ROOMS().fData.item.get(tx, ty);
		COORDINATE coo = ROOMS().fData.itemX1Y1(tx, ty, Coo.TMP); 
		int sx = coo.x();
		int sy = coo.y();
		
		for (int y = 0; y < it.height(); y++) {
			for (int x = 0; x < it.width(); x++) {
				int dx = sx +x;
				int dy = sy +y;
				if (isPool(dx, dy, ins))
					ROOMS().data.set(ins, dx, dy, data);
				
			}
		}
		
	}
	
	static int initService(int tx, int ty, BathInstance ins){
		
		FurnisherItem it = ROOMS().fData.item.get(tx, ty);
		
		COORDINATE coo = ROOMS().fData.itemX1Y1(tx, ty, Coo.TMP); 
		int sx = coo.x();
		int sy = coo.y();
		
		int size =0;
		for (int y = 0; y < it.height(); y++) {
			for (int x = 0; x < it.width(); x++) {
				int dx = sx +x;
				int dy = sy +y;
				if (isPool(dx, dy, ins))
					size++;
				
			}
		}

		size/= 2;
		if (size <= 0 || size > 0x0F)
			throw new RuntimeException(tx + " " + ty + " " + sx + " " + sy + " " +size);
		
		int data = size << 8;
		data |= SERVICE;
		ROOMS().data.set(ins, tx, ty, data);
		return size;
	}
	
	private static boolean isPool(int tx, int ty, BathInstance ins) {
		return ins.is(tx, ty) && (ROOMS().data.get(tx, ty) & Bits.BITS) == Bits.POOL;
	}
	
	void dispose() {
		reservedSet(0);
		availableSet(0);
		blip(coo.x(), coo.y(), Bits.POOL);
		save();	
	}
	
}
