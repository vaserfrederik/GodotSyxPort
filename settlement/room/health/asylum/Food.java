package settlement.room.health.asylum;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import snake2d.util.bit.Bit;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class Food implements SETT_JOB {

	private final static Bits amount 		= new Bits(0b001111);
	private final static Bit reserved 		= new Bit(0b010000);
	private Coo coo = new Coo();
	private AsylumInstance ins;
	
	private Food() {
		
	}

	private static Food self = new Food();
	
	static Food init(int tx, int ty) {
		
		self.ins = b().get(tx, ty);
		if (self.ins == null || SETT.ROOMS().fData.tileData.get(tx, ty) != Constructor.CODE_FOOD)
			return null;
		self.coo.set(tx, ty);
		
		return self;
		
	}
	
	
	static int food(int data) {
		return amount.get(data);
	}
	
	int food() {
		return food(SETT.ROOMS().data.get(coo));
	}
	
	void consume() {
		int d = amount.inc(SETT.ROOMS().data.get(coo), -1);
		SETT.ROOMS().data.set(ins, coo, d);
	}
	
	@Override
	public SoundRace jobSound() {
		return null;
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
		if (amount.get(SETT.ROOMS().data.get(coo)) < 1 && ins.jobs.resourceShouldSearch(b().consumtion.ins().get(0).resource))
			return b().consumtion.ins().get(0).resource.bit;
		return null;
	}

	@Override
	public double jobPerformTime(Humanoid skill) {
		return 25;
	}

	@Override
	public void jobStartPerforming() {
		
	}
	
	@Override
	public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ri) {
		if (r == b().consumtion.ins().get(0).resource) {
			int d = amount.inc(SETT.ROOMS().data.get(coo), ri);
			SETT.ROOMS().data.set(ins, coo, d);
			b().consumtion.ins().get(0).inc(b().get(coo.x(), coo.y()), ri);
		}
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
	
	@Override
	public boolean jobUseHands() {
		return false;
	}
	
	private static final ROOM_ASYLUM b() {
		return SETT.ROOMS().ASYLUM;
	}
	
}
