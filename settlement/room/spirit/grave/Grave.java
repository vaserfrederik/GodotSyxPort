package settlement.room.spirit.grave;

import static settlement.main.SETT.ROOMS;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.room.main.RoomBlueprintIns;
import settlement.thing.ThingsCorpses.Corpse;
import snake2d.util.bit.Bit;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class Grave {

	static final int ITEM_MARK = 1;
	static final int DIG_MARK = 2;
	private static Bits id = new Bits			(0x000000FFFF);
	private static Bits state = new Bits		(0x00000F0000);
	private static Bit reserved = new Bit		(0x0000100000);
	private static Bits time = new Bits			(0x00FF000000);
	
	private final static int UNUSED = 0;
	private final static int RESERVED = 2;
	private final static int USED = 3; 
	private int data;
	private final Coo coo = new Coo();
	private GraveInstance ins;
	
	private final RoomBlueprintIns<GraveInstance> b;
	private final GraveData d;
	
	Grave(RoomBlueprintIns<GraveInstance> b, GraveData d) {
		this.b = b;
		this.d = d;
	}
	
	int daysTillDecompose(int tx, int ty) {
		if (get(tx, ty) != null) {
			return time.get(data);
		}
		return 0;
	}
	
	boolean init(int tx, int ty, int i) {
		if (b.is(tx, ty)) {
			if (SETT.ROOMS().fData.tileData.get(tx, ty) == ITEM_MARK){
				ROOMS().data.set(ins, tx, ty, id.set(0, i));
				return true;
			}
		}
		return false;
	}
	
	Grave get(int tx, int ty) {
		if (b.is(tx, ty)) {
			if (SETT.ROOMS().fData.tileData.get(tx, ty) == ITEM_MARK){
				int data = ROOMS().data.get(tx, ty);
				this.data = data;
				coo.set(tx, ty);
				ins = b.get(tx, ty);
				return this;
			}
		}
		return null;
	}
	
	void updateDay2() {
		if (state.get(data) == USED) {
			data = time.inc(data, -1);
			ROOMS().data.set(ins, coo, data);
			if (time.get(data) == 0) {
				GraveInfo.get(ins, id.get(data)).clear();
				data = state.set(data, UNUSED);
				save();
			}
		}
		
	}
	
	FSERVICE service(int tx, int ty) {
		Grave g = get(tx, ty);
		if (g != null)
			return g.service;
		return null;
	}
	
	GRAVE_JOB job(int tx, int ty) {
		Grave g = get(tx, ty);
		if (g != null)
			return g.job;
		return null;
	}
	
	boolean isUsable() {
		return state.get(data) == UNUSED;
	}

	
	static boolean isUsed(int tx, int ty) {
		int i = state.get(ROOMS().data.get(tx, ty));
		return i == USED;
	}
	
	private void save() {
		
		int old = ROOMS().data.get(coo);
		
		if (old != data) {
			int current = data;
			data = old;
			if (service.findableReservedCanBe())
				ROOMS().graveServiceSpots.report(service, -1);
			
			if (state.get(data) == UNUSED) {
				ins.count(-1);
			}
			
			data = current;
			if (service.findableReservedCanBe())
				ROOMS().graveServiceSpots.report(service, 1);
			
			if (state.get(data) == UNUSED) {
				ins.count(1);
			}
			
			
			ROOMS().data.set(ins, coo, data);
		}
	}

	final FSERVICE service = new FSERVICE() {
		
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
			return (state.get(data) == USED) && !reserved.is(data);
		}

		@Override
		public void findableReserve() {
			if (!findableReservedCanBe())
				throw new RuntimeException();
			data = reserved.set(data);
			save();
		}

		@Override
		public boolean findableReservedIs() {
			return reserved.is(data);
		}

		@Override
		public void findableReserveCancel() {
			data = reserved.clear(data);
			data = state.set(data, USED);
			save();
		}

		@Override
		public void consume() {
			data = reserved.clear(data);
			save();
		}
		
	};
	
	private final GRAVE_JOB job = new GRAVE_JOB() {
		
		@Override
		public boolean jobUseTool() {
			return true;
		}
		
		@Override
		public void jobStartPerforming() {
			// TODO Auto-generated method stub
			
		}
		
		@Override
		public SoundRace jobSound() {
			return ins.blueprintI().employment().sound();
		}
		
		@Override
		public RBIT jobResourceBitToFetch() {
			return null;
		}
		
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			if (state.get(data) == RESERVED) {
				return true;
			}
			return false;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			if (jobReservedIs(r)) {
				data = state.set(data, UNUSED);
				save();
			}
		}
		
		@Override
		public boolean jobReserveCanBe() {
			if (state.get(data) == UNUSED) {
				return true;
			}
			return false;
		}
		
		@Override
		public void jobReserve(RESOURCE r) {			
			data = state.set(data, RESERVED);
			save();
		}
		
		@Override
		public double jobPerformTime(Humanoid skill) {
			return 10;
		}
		
		@Override
		public CharSequence jobName() {
			return null;
		}
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}

		@Override
		public void buryAndPerform(Corpse c) {
			if (c != null) {
				d.get(c.indu().clas()).burry(c);
				GraveInfo.get(ins, id.get(data)).bury(c);
				c.remove();
				data = state.set(data, USED);
				data = time.set(data, d.composeTime);
			}else {
				data = state.set(data, UNUSED);
			}
			
			save();
		}
		
	};


	
	public boolean reuse() {
		if (state.get(data) == USED) {
			data = state.set(data, UNUSED);
			save();
			return true;
		}
		return false;
	}
	
	void dispose(){
		if (state.get(data) == UNUSED || state.get(data) == RESERVED) {
			
		}
		
		if (service.findableReservedCanBe())
			ROOMS().graveServiceSpots.report(service, -1);
		
		data = state.set(data, UNUSED);
		ROOMS().data.set(ins, coo, data);
	}

	void deactivate() {
		if (state.get(data) == UNUSED || state.get(data) == RESERVED) {
			
		}
		
	}


	
}
