package settlement.room.law.prison;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import snake2d.util.bit.Bit;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class Cell implements SETT_JOB {

	private final static Bit reserved 	= new Bit(0b0000_0000_0001_0000);
	private Coo coo = new Coo();
	private PrisonInstance ins;
	
	private Cell() {
		
	}

	static final Cell self = new Cell();
	
	static Cell init(int tx, int ty) {
		self.ins = SETT.ROOMS().PRISON.get(tx, ty);
		if (self.ins == null || SETT.ROOMS().fData.tileData.get(tx, ty) != Constructor.CODE_ENTRANCE)
			return null;
		self.coo.set(tx, ty);
		return self;
		
	}
	
	@Override
	public SoundRace jobSound() {
		return ins.blueprintI().employment().sound();
	}
	
	@Override
	public CharSequence jobName() {
		return SETT.ROOMS().PRISON.employment().verb;
	}

	@Override
	public void jobReserve(RESOURCE r) {
		int d = reserved.set(SETT.ROOMS().data.get(coo));
		SETT.ROOMS().data.set(ins, coo, d);
	}

	@Override
	public boolean jobReservedIs(RESOURCE r) {
		return reserved.is(SETT.ROOMS().data.get(coo));
	}

	@Override
	public void jobReserveCancel(RESOURCE r) {
		int d = reserved.clear(SETT.ROOMS().data.get(coo));
		SETT.ROOMS().data.set(ins, coo, d);
	}

	@Override
	public boolean jobReserveCanBe() {
		return !jobReservedIs(null);
	}

	@Override
	public RBIT jobResourceBitToFetch() {
		return null;
	}

	@Override
	public double jobPerformTime(Humanoid skill) {
		return 45;
	}

	@Override
	public void jobStartPerforming() {
		
	}
	
	@Override
	public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram) {
		jobReserveCancel(null);
		return null;
	}

	@Override
	public COORDINATE jobCoo() {
		return coo;
	}

	@Override
	public boolean jobUseTool() {
		return false;
	}
	

	
}
