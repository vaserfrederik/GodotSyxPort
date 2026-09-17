package settlement.room.infra.station;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.util.RoomBits;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class Job {


	private StationInstance ins;
	private final Coo coo = new Coo();
	
	public final RoomBits breserved = new RoomBits(coo, 		new Bits(0b0000_0000_0000_0000_0000_0000_0000_0001));
	
	private final ROOM_STATION b;
	
	Job(ROOM_STATION blue){
		this.b = blue;
	}
	
	public SETT_JOB get(int tx, int ty) {
		ins = b.get(tx, ty);
		if (ins == null)
			return null;
		if ((SETT.ROOMS().fData.tileData.get(tx, ty) & Constructor.BIT_WORK) != 0) {
			coo.set(tx,ty);
			return job;
		}
		return null;
	}
	
	public final SETT_JOB job = new SETT_JOB() {
		
		private int time = 48;
		
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
			return breserved.get() == 0 && ins.prepared < ins.maxPrep();
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
		public RESOURCE jobPerform(Humanoid skill, RESOURCE res, int ram) {
			jobReserveCancel(res);
			double am = SETT.ROOMS().STOCKPILE.bonus().get(skill.indu())/SETT.ROOMS().STOCKPILE.bonus().baseValue*ins.efficiency()*time;
			ins.setPrepared(ins.prepared + am);
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
