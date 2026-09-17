package settlement.room.knowledge.university;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import snake2d.util.bit.Bit;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class Job {

	private final ROOM_UNIVERSITY b;
	private int data;
	private final Coo coo = new Coo();
	private UniversityInstance ins;
	
	Job(ROOM_UNIVERSITY b){
		this.b = b;
	}
	
	SETT_JOB get(int tx, int ty) {
		ins = b.get(tx, ty);
		if (ins != null) {
			int i = SETT.ROOMS().fData.tileData.get(tx, ty);
			if (i > 0) {
				data = SETT.ROOMS().data.get(tx, ty);
				coo.set(tx, ty);
				return job;
			}
		}
		return null;
	}
	
	private final SETT_JOB job = new SETT_JOB() {
		
		private final Bit reserved = new Bit(1);
		
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
			return reserved.is(data);
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			data = reserved.clear(data);
			SETT.ROOMS().data.set(ins, coo, data);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return !reserved.is(data);
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			data = reserved.set(data);
			SETT.ROOMS().data.set(ins, coo, data);
		}
		
		@Override
		public double jobPerformTime(Humanoid skill) {
			return 45;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram) {
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
