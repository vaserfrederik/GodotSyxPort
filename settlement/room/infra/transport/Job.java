package settlement.room.infra.transport;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.main.util.RoomBits;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.misc.CLAMP;

final class Job {


	private TransportInstance ins;
	private final Coo coo = new Coo();
	
	public final RoomBits breserved = new RoomBits(coo, 		new Bits(0b0000_0000_0000_0000_0000_0000_0000_0001));
	public final RoomBits bamount = new RoomBits(coo,			new Bits(0b0000_0000_0000_0000_1111_1111_1111_0000));
	public final RoomBits bamountr = new RoomBits(coo,			new Bits(0b0000_0000_0001_1111_0000_0000_0000_0000));
	Job(ROOM_TRANSPORT blue){
		
	}
	
	void remove(TransportInstance ins) {
		this.ins = ins;
		for (COORDINATE c : ins.body()) {
			if (ins.is(c) && SETT.ROOMS().fData.tile.get(c) == b().constructor.ww) {
				coo.set(c);
				int am = bamount.get();
				remove();
				bamount.set(ins, 0);
				breserved.set(ins, 0);
				if (ins.data.resource() != null && am > 0) {
					SETT.THINGS().resources.create(c, ins.data.resource(), am);
				}
			}
		}
	}
	
	void add(TransportInstance ins) {
		this.ins = ins;
		for (COORDINATE c : ins.body()) {
			if (ins.is(c) && SETT.ROOMS().fData.tile.get(c) == b().constructor.ww && storage.storageReservable() > 0) {
				coo.set(c);
				add();
			}
		}
	}
	
	private static ROOM_TRANSPORT b() {
		return SETT.ROOMS().TRANSPORT;
	}
	
	public SETT_JOB job(int tx, int ty) {
		ins = b().get(tx, ty);
		if (ins == null)
			return null;
		if (SETT.ROOMS().fData.tile.get(tx, ty) == b().constructor.ww) {
			coo.set(tx,ty);
			if ((ins.destSpaceMask().isClear() && ins.data.prepD() < 2) || ins.data.needsPrep())
				return prep;
			return load;
		}
		return null;
	}
	
	public TILE_STORAGE storage(int tx, int ty) {
		ins = b().get(tx, ty);
		if (ins == null)
			return null;
		else if (SETT.ROOMS().fData.tile.get(tx, ty) == b().constructor.ww) {
			coo.set(tx,ty);
			return storage;
		}
		return null;
	}
	
	public final TILE_STORAGE storage = new TILE_STORAGE() {
		
		@Override
		public int y() {
			return coo.y();
		}
		
		@Override
		public int x() {
			return coo.x();
		}
		
		@Override
		public void storageUnreserve(int amount) {
			remove();
			bamountr.inc(ins, -amount);
			add();
		}
		
		@Override
		public int storageReserved() {
			return bamountr.get();
		}
		
		@Override
		public void storageReserve(int amount) {
			remove();
			bamountr.inc(ins, amount);
			add();
		}
		
		@Override
		public int storageReservable() {
			if (ins.data.resource() != null)
				return bamountr.max()-bamount.get()-bamountr.get();
			return 0;
		}
		
		@Override
		public void storageDeposit(int amount) {
			remove();
			bamountr.inc(ins, -amount);
			add();
			if (!ins.data.needsPrep() || true) {
				if (ins.data.stored() < ROOM_TRANSPORT.MAX_LOAD) {
					int am = amount;
					am = CLAMP.i(am, 0, ROOM_TRANSPORT.MAX_LOAD-ins.data.stored());
					ins.data.store(am);
					amount -= am;
				}
			}
			
			if (amount > 0){
				remove();
				bamount.inc(ins, amount);
				//bamountr.inc(ins, -amount);
				add();
			}
			ins.go();
		}
		
		@Override
		public RESOURCE resource() {
			return ins.resource();
		}
		
		@Override
		public boolean storageIsFindable() {
			return false;
		};
	};
	
	void remove() {
		ins.data.unloadedInc(-bamount.get());
		if (storage.storageReservable() > 0) {
			SETT.PATH().finders.storage.reportAbsence(storage);
			if (bamount.get() == 0 && bamountr.get() == 0)
				ins.data.unloadedSpotsInc(-1);
		}
	}
	
	void add() {
		ins.data.unloadedInc(bamount.get());
		if (storage.storageReservable() > 0) {
			SETT.PATH().finders.storage.reportPresence(storage);
			if (bamount.get() == 0 && bamountr.get() == 0)
				ins.data.unloadedSpotsInc(1);
		}
	}
	
	public final SETT_JOB load = new SETT_JOB() {
		
		private int time = 1;
		
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
			return null;
		}
		
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return breserved.get() == 1;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			breserved.set(ins, 0);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return breserved.get() == 0 && bamount.get() > 0 && ins.data.stored() < ROOM_TRANSPORT.MAX_LOAD;
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			breserved.set(ins, 1);
		}
		
		@Override
		public double jobPerformTime(Humanoid skill) {
			return time;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ri) {
			jobReserveCancel(r);
			if (bamount.get() > 0 && ins.data.stored() < ROOM_TRANSPORT.MAX_LOAD) {
				int am = bamount.get();
				am = CLAMP.i(am, 0, ROOM_TRANSPORT.MAX_LOAD-ins.data.stored());
				ins.data.store(am);
				remove();
				bamount.inc(ins, -am);
				add();
			}
			
			
			ins.go();
			return null;
		}
		
		@Override
		public CharSequence jobName() {
			return Gui.¤¤organise;
		}
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}
	};
	
	public final SETT_JOB prep = new SETT_JOB() {
		
		private int time = 16;
		
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
			return null;
		}
		
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return breserved.get() == 1;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			breserved.set(ins, 0);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return breserved.get() == 0;
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			breserved.set(ins, 1);
		}
		
		@Override
		public double jobPerformTime(Humanoid skill) {
			return time;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ri) {

			
			jobReserveCancel(r);
			double am = SETT.ROOMS().STOCKPILE.bonus().get(skill.indu())/SETT.ROOMS().STOCKPILE.bonus().baseValue*ins.efficiency()*time;
			ins.data.prep(am);
			ins.go();
			return null;
		}
		
		@Override
		public CharSequence jobName() {
			return Gui.¤¤preparing;
		}
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}
	};
	
}
