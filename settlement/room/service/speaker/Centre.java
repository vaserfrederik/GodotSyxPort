package settlement.room.service.speaker;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class Centre {

	private SpeakerInstance ins;
	private final Coo coo = new Coo();
	private final ROOM_SPEAKER b;
	
	Centre(ROOM_SPEAKER b){
		this.b = b;
	}
	
	public SETT_JOB job(int tx, int ty) {
		if (init(tx, ty))
			return job;
		return null;
	}
	
	public FSERVICE service(int tx, int ty) {
		if (init(tx, ty))
			return service;
		return null;
	}
	
	private boolean init(int tx, int ty) {
		ins = b.getter.get(tx, ty);
		if (ins != null && tx == ins.body().cX() && ty == ins.body().cY()) {
			coo.set(tx, ty);
			return true;
		}
		return false;
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
			return true;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return true;
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
		
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
	
	public JOB_MANAGER manager(SpeakerInstance ins) {
		this.ins = ins;
		return manager;
	}
	
	private final JOB_MANAGER manager = new JOB_MANAGER() {
		
		@Override
		public boolean resourceReachable(RESOURCE res) {
			return true;
		}
		
		@Override
		public SETT_JOB reportResourceMissing(RBIT resourceMask, int jx, int jy) {
			return null;
		}
		
		@Override
		public SETT_JOB getReservableJob(COORDINATE c) {
			return job(ins.body().cX(), ins.body().cY());
		}
		
		@Override
		public SETT_JOB getJob(COORDINATE c) {
			return job(ins.body().cX(), ins.body().cY());
		}

		@Override
		public void reportResourceFound(RESOURCE res) {
			// TODO Auto-generated method stub
			
		}

		@Override
		public void resetResourceSearch() {
			// TODO Auto-generated method stub
			
		}

		@Override
		public boolean resourceShouldSearch(RESOURCE res) {
			return true;
		}
	};
}
