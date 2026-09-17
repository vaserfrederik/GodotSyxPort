package settlement.room.service.nursery;

import game.audio.SoundRace;
import game.time.TIME;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.main.ROOMA;
import settlement.room.main.RoomInstance;
import settlement.room.main.util.RoomBits;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

class NurseryStation {

	private final Coo coo = new Coo();
	private NurseryInstance ins;
	private final ROOM_NURSERY b;

	private final RoomBits bMark = new RoomBits(coo, 				new Bits(0b0000_0000_0001));
	private final RoomBits bWorked = new BB(coo, 					new Bits(0b0000_0000_1110));
	private final RoomBits bWorkReserved = new RoomBits(coo, 		new Bits(0b0000_0001_0000));
	private final RoomBits bServiceReserved = new BB(coo, 			new Bits(0b0000_0010_0000));
	
	private final int wt;
	
	NurseryStation(ROOM_NURSERY b){
		this.b = b;
		
		double worksPerDay =  b.ChildPErE*(TIME.workSeconds()/ ROOM_NURSERY.playTime);
		wt = (int) (TIME.workSeconds()/worksPerDay);
	}
	
	private boolean pinit(int tx, int ty) {
		if (b.is(tx, ty)) {
			this.coo.set(tx, ty);
			this.ins = b.get(tx, ty);
			if (bMark.get() == 1)
				return true;
		}
		return false;
	}
	
	public SETT_JOB job(int tx, int ty) {
		if (pinit(tx, ty))
			return job;
		return null;
	}
	
	public FSERVICE service(int tx, int ty) {
		if (pinit(tx, ty))
			return service;
		return null;
	}
	
	public void init(RoomInstance ins, int tx, int ty) {
		coo.set(tx, ty);
		bMark.set(ins, 1);
	}
	
	public int stuff(int tx, int ty) {
		if (pinit(tx, ty))
			return 3-bWorked.get();
		return 0;
	}
	
	private final SETT_JOB job = new SETT_JOB() {
		
		
		
		@Override
		public boolean jobUseTool() {
			return false;
		}
		
		@Override
		public void jobStartPerforming() {
			
		}
		
		@Override
		public SoundRace jobSound() {
			return b.employment().sound();
		}
		
		@Override
		public RBIT jobResourceBitToFetch() {
			return null;
		}
		
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return bWorkReserved.get() == 1;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			bWorkReserved.set(ins, 0);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			if (bWorkReserved.get() == 1)
				return false;
			
			if (bWorked.get() >= 3)
				return false;
			return true;
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			bWorkReserved.set(ins, 1);
		}
		
		@Override
		public double jobPerformTime(Humanoid skill) {
			double d = IndustryUtil.calcProductionRate(1, skill, b.rate, ins);
			if (d == 0)
				return wt*5;
			return wt/(d);
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int am) {
			bWorkReserved.set(ins, 0);
			bWorked.inc(ins, 1);
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
	
	private final FSERVICE service = new FSERVICE() {
		
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
			return bServiceReserved.get() == 1;
		}
		
		@Override
		public boolean findableReservedCanBe() {
			return bServiceReserved.get() == 0 && bWorked.get() > 0;
		}
		
		@Override
		public void findableReserveCancel() {
			bServiceReserved.set(ins, 0);
		}
		
		@Override
		public void findableReserve() {
			if (findableReservedCanBe())
				bServiceReserved.set(ins, 1);
		}
		
		@Override
		public void startUsing() {
			bWorked.inc(ins, -1);
		};
		
		@Override
		public void consume() {
			bServiceReserved.set(ins, 0);
			
			ins.getWork().searchAgain();
		}
	};
	
	private class BB extends RoomBits {

		public BB(COORDINATE coo, Bits bits) {
			super(coo, bits);
		}
		
		@Override
		public void set(int tx, int ty, ROOMA r, int t) {
			ins.service().report(service, ins.blueprintI().service(), -1);
			super.set(tx, ty, r, t);
			ins.service().report(service, ins.blueprintI().service(), 1);
		}
		
	}


	
}
