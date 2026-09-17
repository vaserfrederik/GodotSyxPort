package settlement.room.service.stage;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class Centre {

	private final Bits dused = new Bits(0b0000_0000_0000_0000_0000_0000_0000_0001);
	private final Bits dreserved = new Bits(0b0000_0000_0000_0000_0000_0000_0000_0010);
	private StageInstance ins;
	private final Coo coo = new Coo();
	private int data;
	private final ROOM_STAGE b;
	
	Centre(ROOM_STAGE b){
		this.b = b;
	}
	
	public SETT_JOB job(int tx, int ty) {
		ins = b.getter.get(tx, ty);
		if (ins != null && SETT.ROOMS().fData.tileData.get(tx, ty) == StageConstructor.STATION) {
			coo.set(tx, ty);
			data = SETT.ROOMS().data.get(tx, ty);
			return job;
		}
		return null;
	}
	
	public FSERVICE service(int tx, int ty) {
		ins = b.getter.get(tx, ty);
		if (ins != null && ins.body().cX() == tx && ins.body().cY() == ty) {
			coo.set(tx, ty);
			data = SETT.ROOMS().data.get(tx, ty);
			return service;
		}
		return null;
	}
	
	private void save() {
//		int ndata = data;
//		data = SETT.ROOMS().data.get(coo);
//		if (service.findableReservedCanBe()) { 
//			ins.service.report(service, ins.blueprintI().data, -dservices.get(data));
//		}
//		data = ndata;
//		if (service.findableReservedCanBe()) { 
//			ins.service.report(service, ins.blueprintI().data, dservices.get(data));
//		}
		SETT.ROOMS().data.set(ins, coo, data);
		
	}
	

	
	private final FSERVICE service = new FSERVICE(){
		
		@Override
		public void consume() {
			
		}
		
		@Override
		public int x() {
			return ins.body().cX();
		}

		@Override
		public int y() {
			return ins.body().cY();
		}

		@Override
		public boolean findableReservedCanBe() {
			return ins.services() > 0;
		}

		@Override
		public void findableReserve() {
			if (!findableReservedCanBe()) {
				throw new RuntimeException();	
			}
			ins.incServices(-1);
		}

		@Override
		public boolean findableReservedIs() {
			return ins.hasService();
		}

		@Override
		public void findableReserveCancel() {
			ins.incServices(1);
		}
		
	};
	
	private final SETT_JOB job = new SETT_JOB() {
		
		@Override
		public boolean jobUseTool() {
			return false;
		}
		
		@Override
		public void jobStartPerforming() {
			data = dused.set(data, 1);
			save();
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
			return dreserved.get(data) == 1;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			data = dused.set(data, 0);
			data = dreserved.set(data, 0);
			save();
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return !jobReservedIs(null);
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			data = dreserved.set(data, 1);
			save();
		}
		
		@Override
		public double jobPerformTime(Humanoid a) {
			return 0;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			jobReserveCancel(r);
			return null;
		}
		
		@Override
		public CharSequence jobName() {
			return b.employment().verb;
		}
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}
	};
	
}
