package settlement.room.main.job;

import static settlement.main.SETT.PATH;
import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.THINGS;

import settlement.main.SETT;
import settlement.misc.util.RESOURCE_TILE;
import settlement.room.main.ROOMA;
import snake2d.SPRITE_RENDERER;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.DIR;
import util.rendering.ShadowBatch;

public abstract class RoomResStorage implements RESOURCE_TILE{

	private final Bits amount;
	private final Bits reserved;
	
	private final int max;
	
	protected int x,y;
	protected int data;
	protected ROOMA ins;
	
	protected RoomResStorage(int max) {
		this.max = max;
		int s = 0;
		int mask = 0;
		while(max != 0) {
			max/= 2;
			s++;
			mask = mask << 1;
			mask |= 1;
		}
		if (mask > 0x0FFFF)
			throw new RuntimeException("too big " + max + " " + (max & mask));
		amount = 	new Bits(mask);
		reserved = 	new Bits(mask << (s));
	}
	
	@Override
	public boolean isStorage() {
		return false;
	}
	
	@Override
	public boolean isPrio() {
		return false;
	}
	
	public RoomResStorage get(int tx, int ty, ROOMA i) {
		ins = i;
		if (i.is(tx, ty) && is(tx, ty)) {
			x = tx;
			y = ty;
			data = ROOMS().data.get(tx, ty);
			return this;
		}
		return null;
	}
	
	protected abstract boolean is(int tx, int ty);
	
	protected void set(int tx, int ty) {
		x = tx;
		y = ty;
		data = ROOMS().data.get(tx, ty);
	}
	
	private void save() {
		
		int old = data;
		data = ROOMS().data.get(x, y);
		if (findableReservedCanBe()) {
			PATH().finders.resource.reportAbsence(this);
		}
		data = old;
		ROOMS().data.set(ins, x, y, data);
		if (findableReservedCanBe()) {
			PATH().finders.resource.reportPresence(this);
		}
		changed(x, y);
	}
	
	protected void changed(int tx, int ty) {
		
	};
	
	@Override
	public boolean hasRoom() {
		return amount.get(data) < max;
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
	public void findableReserve() {
		if (reserved.get(data) >= amount.get(data))
			throw new RuntimeException();
		data = reserved.inc(data, 1);
		save();
	}
	
	@Override
	public int reservable() {
		return amount.get(data)-reserved.get(data);
	}

	@Override
	public void findableReserveCancel() {
		if (reserved.get(data) > 0) {
			data = reserved.inc(data, -1);
			save();
		}
		
	}

	@Override
	public boolean findableReservedIs() {
		return reserved.get(data) > 0 && amount.get(data) > 0;
	}

	@Override
	public boolean findableReservedCanBe() {
		return reserved.get(data) < amount.get(data);
	}

	@Override
	public void resourcePickup() {
		if (findableReservedIs()) {
			data = amount.inc(data, -1);
			data = reserved.inc(data, -1);
			save();
		}
		
	}
	
	public void deposit() {
		if (!hasRoom())
			throw new RuntimeException();
		data = amount.inc(data, 1);
		save();
	}
	
	public int deposit(int am) {
		int a = max-amount();
		a = Math.min(am, a);
		data = amount.inc(data, a);
		save();
		return a;
		
	}
	
	public void dispose() {
		if (amount.get(data) > 0 && resource() != null) {
			boolean unload = false;
			for (int di = 0; di < DIR.ALL.size(); di++) {
				int dx = x() + DIR.ALL.get(di).x();
				int dy = y() + DIR.ALL.get(di).y();
				if (SETT.PATH().connectivity.is(dx, dy)) {
					unload = true;
					THINGS().resources.create(dx, dy, resource(), amount.get(data));
					break;
				}
			}
			if (!unload) {
				THINGS().resources.create(this, resource(), amount.get(data));
			}
		}
		
		data = 0;
		save();
		
	}
	
	public void render(SPRITE_RENDERER r, ShadowBatch shadowBatch, int tx, int ty, int x, int y, int ran) {
		if (get(tx, ty, SETT.ROOMS().map.rooma.get(tx, ty)) == null)
			return;
		
		int a = amount.get(data);
		if (a > 0) {
			a = (int) Math.ceil(a/2.0);
			shadowBatch.setHeight(1).setDistance2Ground(0);
			resource().renderLaying(shadowBatch, x, y, ran, a);
			resource().renderLaying(r, x, y, ran, a);
		}
	}

	@Override
	public int amount() {
		return amount.get(data);
	}
	
	public int max() {
		return max;
	}

}
