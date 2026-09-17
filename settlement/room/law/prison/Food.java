package settlement.room.law.prison;

import game.audio.SoundRace;
import game.faction.FACTIONS;
import game.faction.FResources.RTYPE;
import init.resources.RBIT;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
import snake2d.util.bit.Bit;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class Food implements SETT_JOB, FSERVICE {

	private final static Bits food_amount 	= new Bits(		0b0000_0000_0000_1111_1111);
	private final static Bit food_reserved 	= new Bit(		0b0000_0000_0001_0000_0000);
	private final static Bit job_reserved 	= new Bit(		0b0000_0000_0010_0000_0000);
	private Coo coo = new Coo();
	private PrisonInstance ins;
	
	private Food() {
		
	}

	private static Food self = new Food();
	
	static Food init(int tx, int ty) {
		
		self.ins = SETT.ROOMS().PRISON.get(tx, ty);
		if (self.ins == null || SETT.ROOMS().fData.tileData.get(tx, ty) != Constructor.CODE_FOOD)
			return null;
		
		self.coo.set(tx, ty);
		return self;
		
	}
	
	
	static int foodAmount(int data) {
		return food_amount.get(data);
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
		int d = job_reserved.set(SETT.ROOMS().data.get(coo));
		SETT.ROOMS().data.set(ins, coo, d);
	}

	@Override
	public boolean jobReservedIs(RESOURCE r) {
		return job_reserved.is(SETT.ROOMS().data.get(coo));
	}

	@Override
	public void jobReserveCancel(RESOURCE r) {
		int d = job_reserved.clear(SETT.ROOMS().data.get(coo));
		SETT.ROOMS().data.set(ins, coo, d);
	}

	@Override
	public boolean jobReserveCanBe() {
		return !jobReservedIs(null) && food_amount.get(SETT.ROOMS().data.get(coo)) < 8;
	}

	@Override
	public RBIT jobResourceBitToFetch() {
		return ins.fetch;
	}
	
	@Override
	public int jobResourcesNeeded(Humanoid skill) {
		return food_amount.mask - food_amount.get(SETT.ROOMS().data.get(coo));
	}

	@Override
	public double jobPerformTime(Humanoid skill) {
		return 0;
	}

	@Override
	public void jobStartPerforming() {
		
	}
	
	@Override
	public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ri) {
		int d = food_amount.inc(SETT.ROOMS().data.get(coo), ri);
		SETT.ROOMS().data.set(ins, coo, d);
		if (RESOURCES.EDI().get(r) != null) {
			ins.blueprintI().indu.ins().get(RESOURCES.EDI().get(r).index()).inc(ins, ri);
		}else
			FACTIONS.player().res().inc(r, RTYPE.CONSUMED, -ri);
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
	public boolean findableReservedCanBe() {
		return food_amount.get(SETT.ROOMS().data.get(coo)) > 0 && !food_reserved.is(SETT.ROOMS().data.get(coo));
	}

	@Override
	public void findableReserve() {
		int d = food_reserved.set(SETT.ROOMS().data.get(coo));
		SETT.ROOMS().data.set(ins, coo, d);
	}

	@Override
	public boolean findableReservedIs() {
		return food_reserved.is(SETT.ROOMS().data.get(coo));
	}

	@Override
	public void findableReserveCancel() {
		int d = food_reserved.clear(SETT.ROOMS().data.get(coo));
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
		int d = food_amount.inc(SETT.ROOMS().data.get(coo), -1);
		d = food_reserved.clear(d);
		SETT.ROOMS().data.set(ins, coo, d);
	}
	

	
}
