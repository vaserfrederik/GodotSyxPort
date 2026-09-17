package settlement.room.law.prison;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
import snake2d.util.bit.Bit;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class Latrine implements SETT_JOB, FSERVICE {

	private final static Bit latrine_reserved 	= new Bit(0b0000_0000_0000_0001);
	private final static Bit latrine_used 		= new Bit(0b0000_0000_0000_0010);
	private final static Bit latrine_jobreserved= new Bit(0b0000_0000_0000_0100);
	private Coo coo = new Coo();
	private PrisonInstance ins;
	
	private Latrine() {
		
	}

	static final Latrine self = new Latrine();
	
	static Latrine init(int tx, int ty) {
		self.ins = SETT.ROOMS().PRISON.get(tx, ty);
		if (self.ins == null || SETT.ROOMS().fData.tileData.get(tx, ty) != Constructor.CODE_LATRINE)
			return null;
		self.coo.set(tx, ty);
		return self;
		
	}
	
	
	static boolean latrineUsed(int data) {
		return latrine_used.is(data);
	}
	
	@Override
	public SoundRace jobSound() {
		if (SETT.ROOMS().LAVATORIES.size() > 0)
			return SETT.ROOMS().LAVATORIES.get(0).employment().sound();
		return null;
	}
	
	@Override
	public CharSequence jobName() {
		return SETT.ROOMS().PRISON.employment().verb;
	}

	@Override
	public void jobReserve(RESOURCE r) {
		int d = latrine_jobreserved.set(SETT.ROOMS().data.get(coo));
		SETT.ROOMS().data.set(ins, coo, d);
	}

	@Override
	public boolean jobReservedIs(RESOURCE r) {
		return latrine_jobreserved.is(SETT.ROOMS().data.get(coo));
	}

	@Override
	public void jobReserveCancel(RESOURCE r) {
		int d = latrine_jobreserved.clear(SETT.ROOMS().data.get(coo));
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
		int d = latrine_used.clear(SETT.ROOMS().data.get(coo));
		SETT.ROOMS().data.set(ins, coo, d);
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

	@Override
	public boolean findableReservedCanBe() {
		return !latrine_reserved.is(SETT.ROOMS().data.get(coo));
	}

	@Override
	public void findableReserve() {
		int d = latrine_reserved.set(SETT.ROOMS().data.get(coo));
		SETT.ROOMS().data.set(ins, coo, d);
	}

	@Override
	public boolean findableReservedIs() {
		return latrine_reserved.is(SETT.ROOMS().data.get(coo));
	}

	@Override
	public void findableReserveCancel() {
		int d = latrine_reserved.clear(SETT.ROOMS().data.get(coo));
		SETT.ROOMS().data.set(ins, coo, d);
	}

	@Override
	public int x() {
		return coo.x();
	}

	@Override
	public int y() {
		return coo.y();
	}

	@Override
	public void consume() {
		int d = latrine_used.set(SETT.ROOMS().data.get(coo));
		d = latrine_reserved.clear(d);
		SETT.ROOMS().data.set(ins, coo, d);
	}
	

	
}
