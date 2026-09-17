package settlement.room.military.supply;

import static settlement.main.SETT.PATH;

import game.audio.SoundRace;
import game.faction.FACTIONS;
import game.faction.FResources.RTYPE;
import init.resources.RBIT;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.main.util.RoomBits;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import world.army.AD;

final class Crate {


	private final static int noRes = 0x0;
	
	private final Coo coo = new Coo();
	private final RoomBits bRes =						new BB(	new Bits(0x000000FF));
	private final RoomBits bAmount = 					new BB(	new Bits(0x0000FF00));
	private final RoomBits bReservedSpace =				new BB(	new Bits(0x00FF0000));
	private final RoomBits bTot =						new BB(	new Bits(0x00FFFFFF));
	private final RoomBits bAnimals = new RoomBits(coo,			new Bits(0x07000000));
	private final RoomBits bAnimalsRes 	= new RoomBits(coo,		new Bits(0x08000000));
	private final RoomBits bAway =				new BB(			new Bits(0x10000000));
//	private final RoomBits bAwayres =				new BB(		new Bits(0x20000000));
	private final RoomBits bState =	new RoomBits(coo,			new Bits(0xC0000000));
	
	private final ROOM_SUPPLY b;
	private SupplyInstance ins;
	
	protected Crate(ROOM_SUPPLY b){
		this.b = b;
	}
	
	public Crate get(int tx, int ty) {
		if (b.is(tx, ty) && SETT.ROOMS().fData.tileData.get(tx, ty) == 1) {
			coo.set(tx, ty);
			ins = b.getter.get(tx, ty);			
			return this;
		}
		return null;
	}
	
	public TILE_STORAGE storage(int tx, int ty) {
		if (get(tx, ty) != null)
			return storage();
		return null;
	}
	
	public TILE_STORAGE storage() {
		if (bAway.get() == 0) {
			return crate;
		}
		return null;
	}
	
	public SETT_JOB job() {
		return job;
	}
	
	public boolean away() {
		return bAway.get() == 1;
	}
	
	public boolean animalHas() {
		return bAnimals.get() != 0;
	}
	
	public int resAmount() {
		return bAmount.get();
	}
	
	public int goIsReady() {
		if (SETT.ENTRY().isClosed())
			return 1;
		if (bRes.get() == noRes)
			return 2;
		if (bAnimals.get() <= 0)
			return 3;
		if (bAway.get() == 1)
			return 4;
		if (b.cache.deliverable(crate.resource()) <= 0)
			return 6;
		if (bAmount.get() == 0)
			return 7;
		if (bReservedSpace.get() != 0)
			return 8;

		
		return 0;
	}
	
	public void deliver() {
		if (goIsReady() != 0) {
			return;
		}
		
		if (bAmount.get() < ROOM_SUPPLY.STORAGE && bState.get() < bState.max()) {
//			bState.inc(ins, 1);
//			return;
		}else {
			bState.set(ins, 0);
		}
		RESOURCE res = crate.resource();
		int am = bAmount.get();
		int n = Math.max(0, b.cache.needed(res));
		if (am > n) {
			bAmount.set(ins, n);
			vacate(am-n);
			am = n;
		}
		if (am <= 0)
			return;
		
		am = b.cache.deliver(res, am);
		FACTIONS.player().res().inc(res, RTYPE.ARMY_SUPPLY, -am);
		bAmount.inc(ins, -am);
		if (am <= 0)
			return;
		bAway.set(ins, 1);
		
		if (ins.liveCount++> 10) {
			ins.liveCount = 0;
			bAnimals.inc(ins, -1);
		}
		
		DIR dir = DIR.ORTHO.get(SETT.ROOMS().fData.item.get(coo).rotation);
		byte ran = (byte) SETT.tileRan(coo.x(), coo.y());
		int tx = coo.x();
		int ty = coo.y();
		SETT.HALFENTS().transports.military(coo.x()+dir.x()*2, coo.y()+dir.y()*2, ran, res, am, dir);
		get(tx, ty);

	}

	
	public void resourceSet(RESOURCE res) {
		b.tally.report(Crate.this, ins, -1);
		bTot.set(coo.x(), coo.y(), ins, 0);
		bRes.set(coo.x(), coo.y(), ins, res == null ? noRes : res.index()+1);
		b.tally.report(Crate.this, ins, 1);
	}
	
	public RESOURCE realResource() {
		RESOURCE res = crate.resource();
		int i = bRes.get();
		if (i == noRes)
			return null;
		if (bReservedSpace.get() != 0)
			return res;
		if (bAmount.get() != 0)
			return res;
		return null;
	}
	
	public void clear() {
		if (crate.resource() == null)
			return;
		
		
		int am = resAmount();
		vacate(am);
		resourceSet(null);
	}
	
	private void vacate(int am) {
		if (am > 0) {
			for (DIR dd : DIR.ORTHO) {
				if (!PATH().solidity.is(coo, dd)) {
					SETT.THINGS().resources.create(coo.x()+dd.x(), coo.y()+dd.y(), crate.resource(), am);
					break;
				}
			}
		}
	}
	
	public void dispose(){
		RESOURCE res = crate.resource();
		if (res == null)
			return;
		int am = resAmount();
		
		if (am > 0)
			SETT.THINGS().resources.create(coo, res, am);
		b.tally.report(this, ins, -1);
	}
	
	public final TILE_STORAGE crate = new TILE_STORAGE() {
		
		
		@Override
		public int y() {
			return coo.y();
		}
		
		@Override
		public int x() {
			return coo.x();
		}
		
		@Override
		public boolean storageIsFindable() {
			return false;
		}
		
		@Override
		public void storageDeposit(int amount) {
			
			
			
			if (resAmount() + amount > ROOM_SUPPLY.STORAGE)
				throw new RuntimeException(resource() + " " + resAmount() + " " + amount + " " + ROOM_SUPPLY.STORAGE);
			
			bReservedSpace.inc(ins, -amount);
			bAmount.inc(ins, amount);
			if (bAmount.get() >= ROOM_SUPPLY.STORAGE)
				deliver();
			ins.jobs.searchAgain();
			
		}

		@Override
		public int storageReserved() {
			return bReservedSpace.get();
		}

		@Override
		public int storageReservable() {
			
			int am = ROOM_SUPPLY.STORAGE - resAmount() - storageReserved();
			if (resource() == null)
				return am;
			int m = b.tally.fetchAmount(resource());
			return Math.min(am, m);
		}

		@Override
		public void storageReserve(int amount) {
			if (storageReservable() < amount)
				throw new RuntimeException();
			
			bReservedSpace.inc(ins, amount);
		}

		@Override
		public void storageUnreserve(int amount) {
			bReservedSpace.inc(ins, -amount);
		}
		
		@Override
		public RESOURCE resource() {
			int i = bRes.get();
			if (i == noRes)
				return null;
			i--;
			if (i >= RESOURCES.ALL().size() || AD.supplies().get(RESOURCES.ALL().get(i)).size() == 0) {
				return null;
			}
			return RESOURCES.ALL().get(i);
		}
		

		
		

	};
	
	public final SETT_JOB job = new SETT_JOB() {
		
		@Override
		public boolean jobUseTool() {
			return false;
		}
		
		@Override
		public void jobStartPerforming() {
			
		}
		
		@Override
		public SoundRace jobSound() {
			return null;
		}
		
		@Override
		public RBIT jobResourceBitToFetch() {
			return bAnimals.get() == 0 ? b.liveStock.bit : null;
		}
		
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return bAnimalsRes.get() == 1;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			bAnimalsRes.set(ins, 0);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			if (jobReservedIs(null))
				return false;
			return bAnimals.get() == 0 || goIsReady() == 0 || bAway.get() == 1;
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			bAnimalsRes.set(ins, 1);
		}
		
		@Override
		public double jobPerformTime(Humanoid a) {
			return 16;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			jobReserveCancel(null);
			if (r != null){
				bAnimals.inc(ins, rAm);
				FACTIONS.player().res().inc(r, RTYPE.PRODUCED, -rAm);
			}else {
				bAway.set(ins, 0);
				deliver();
			}
			
			
			
			return null;
		}
		
		@Override
		public boolean longFetch() {
			return true;
		};
		
		@Override
		public int jobResourcesNeeded(Humanoid skill) {
			return 8;
		};
		
		@Override
		public CharSequence jobName() {
			return b.employment().verb;
		}
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}
	};
	
	private class BB extends RoomBits {

		public BB(Bits bits) {
			super(coo, bits);
		}
		
		@Override
		protected void remove() {

			b.tally.report(Crate.this, ins, -1);
			super.remove();
		}
		
		@Override
		protected void add() {
			b.tally.report(Crate.this, ins, 1);
			super.add();
		}
		
	}

}
