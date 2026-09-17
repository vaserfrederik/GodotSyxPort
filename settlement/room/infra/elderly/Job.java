package settlement.room.infra.elderly;

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

	private final Bit is = new Bit(0b0001);
	private final Bit reserved = new Bit(0b0010);
	private final Bit using = new Bit(0b0100);
	private final ROOM_RESTHOME b;
	private int data;
	private final Coo coo = new Coo();
	private ResthomeInstance ins;
	
	Job(ROOM_RESTHOME b){
		this.b = b;
	}
	
	SETT_JOB get(int tx, int ty) {
		ins = b.get(tx, ty);
		if (ins != null) {
			if (is.is(SETT.ROOMS().data.get(tx, ty))) {
				data = SETT.ROOMS().data.get(tx, ty);
				coo.set(tx, ty);
				return job;
			}
		}
		return null;
	}
	
	public boolean used(int tx, int ty) {
		if (get(tx, ty) != null)
			return using.is(data);
		return false;
	}
	
	public void set(ResthomeInstance ins, int tx, int ty) {
		
		SETT.ROOMS().data.set(ins, tx, ty, is.set(0));
		
	}
	
	private final SETT_JOB job = new SETT_JOB() {
		
		
		
		@Override
		public boolean jobUseTool() {
			return false;
		}
		
		@Override
		public void jobStartPerforming() {
			data = using.set(data);
			SETT.ROOMS().data.set(ins, coo, data);
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
			data = using.clear(data);
			SETT.ROOMS().data.set(ins, coo, data);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return !reserved.is(data);
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			data = reserved.set(data);
			data = using.clear(data);
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
