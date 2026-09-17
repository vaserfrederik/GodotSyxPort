package settlement.room.infra.station;

import static settlement.main.SETT.PATH;

import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.main.SETT;
import settlement.misc.util.RESOURCE_TILE;
import settlement.room.infra.transport.ROOM_TRANSPORT;
import settlement.room.main.util.RoomBits;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;

final class Crate {

	
	public final int MAX_AM = ROOM_TRANSPORT.MAX_LOAD;
	Coo coo = new Coo();
	StationInstance ins;
	public final RoomBits resource = new RoomBits(coo, 		new Bits(0b0000_0000_0000_0000_0000_1111_1111_0000));
	public final RoomBits reserved = new RoomBits(coo, 		new Bits(0b0000_0000_0011_1111_1111_0000_0000_0000));
	public final RoomBits stored = new RoomBits(coo, 		new Bits(0b1111_1111_1100_0000_0000_0000_0000_0000));
	
	private final ROOM_STATION b;
	
	Crate(ROOM_STATION b){
		this.b = b;
		if (MAX_AM > stored.max())
			throw new RuntimeException();
	}
	
	public RESOURCE_TILE get(int tx, int ty) {
		ins = b.get(tx, ty);
		if (ins == null)
			return null;
		if ((SETT.ROOMS().fData.tileData.get(tx, ty) & Constructor.BIT_CRATE) != 0) {
			coo.set(tx,ty);
			return tile;
		}
		return null;
	}
	
	public void deliver(int am) {
		remove();
		stored.inc(ins, am);
		add();
	}
	
	public void resourceSet(RESOURCE res) {
		
		RESOURCE old = resource();
		
		if (old != null) {
			int am = stored.get();
			if (am > 0) {
				for (DIR dd : DIR.ORTHO) {
					if (!PATH().solidity.is(coo, dd)) {
						SETT.THINGS().resources.create(coo.x()+dd.x(), coo.y()+dd.y(), tile.resource(), am);
						break;
					}
				}
			}
		}
		
		remove();
		stored.set(ins, 0);
		reserved.set(ins, 0);
		resource.set(ins, 0);
		if (old != null)
			b.tally(old).add(ins.tally(old), ins);
			
		if (res != null) {
			b.tally(res).remove(ins.tally(res), ins);
			int ri = res == null ? 0 : res.index()+1;
			resource.set(ins, ri);
			add();
		}
	}
	
	private final RESOURCE_TILE tile = new RESOURCE_TILE() {
		
		@Override
		public int y() {
			return coo.y();
		}
		
		@Override
		public int x() {
			return coo.x();
		}
		
		@Override
		public boolean findableReservedIs() {
			return resource() != null && reserved.get() > 0;
		}
		
		@Override
		public boolean findableReservedCanBe() {
			return resource() != null && reserved.get() < stored.get();
		}
		
		@Override
		public void findableReserveCancel() {
			if (resource() == null)
				return;
			remove();
			reserved.inc(ins, -1);
			add();
		}
		
		@Override
		public void findableReserve() {
			if (resource() == null)
				return;
			remove();
			reserved.inc(ins, 1);
			add();
		}
		
		@Override
		public void resourcePickup() {
			if (resource() == null)
				return;
			remove();
			reserved.inc(ins, -1);
			stored.inc(ins, -1);
			add();
		}
		
		@Override
		public RESOURCE resource() {
			return Crate.this.resource();
		}
		
		@Override
		public int reservable() {
			return stored.get()-reserved.get();
		}
		
		@Override
		public int amount() {
			return stored.get();
		}
		
		@Override
		public boolean isStorage() {
			return true;
		}
		
		@Override
		public boolean isPrio() {
			return false;
		}
	};
	
	public RESOURCE resource() {
		int ri = resource.get();
		if (ri <= 0)
			return null;
		ri-= 1;
		if (ri >= RESOURCES.ALL().size())
			return null;
		return RESOURCES.ALL().get(ri);
	}
	
	private void remove() {
		RESOURCE res = tile.resource();
		if (res == null)
			return;
		ins.tally(res).remove(res, Crate.this, ins);
		if (tile.findableReservedCanBe())
			PATH().finders.resource.reportAbsence(tile);
	}
	
	private void add() {
		RESOURCE res = tile.resource();
		if (res == null)
			return;
		ins.tally(res).add(res, Crate.this, ins);
		if (tile.findableReservedCanBe())
			PATH().finders.resource.reportPresence(tile);
	}

}
