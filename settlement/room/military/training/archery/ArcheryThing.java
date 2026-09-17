package settlement.room.military.training.archery;

import static settlement.main.SETT.ROOMS;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.misc.job.SETT_JOB;
import snake2d.util.bit.Bit;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class ArcheryThing implements SETT_JOB{

	private final Coo coo = new Coo();
	private int data;
	private final ROOM_ARCHERY b;
	private ArcheryInstance ins;
	
	static final Bit reserved 	= new Bit(0b00001);
	static final Bit used 		= new Bit(0b00010);
	
	
	ArcheryThing(ROOM_ARCHERY b) {
		this.b = b;
	}
	
	private void save() {
		ROOMS().data.set(ins, coo, data);
	}
	
	ArcheryThing init(int tx, int ty) {
		coo.set(tx, ty);
		if (ROOMS().fData.tile.is(coo, b.constructor.plat)) {
			ins = b.get(coo.x(), coo.y());
			data = ROOMS().data.get(coo);
			return this;
		}
		return null;
	}




	@Override
	public void jobReserve(RESOURCE r) {
		data = reserved.set(data);
		save();
	}

	@Override
	public boolean jobReservedIs(RESOURCE r) {
		return reserved.is(data);
	}

	@Override
	public void jobReserveCancel(RESOURCE r) {
		data = reserved.clear(data);
		save();
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
	public double jobPerformTime(Humanoid a) {
		return 0;
	}

	@Override
	public void jobStartPerforming() {
		// TODO Auto-generated method stub
		
	}

	@Override
	public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
		jobReserveCancel(r);
		return null;
	}

	@Override
	public COORDINATE jobCoo() {
		return coo;
	}

	@Override
	public CharSequence jobName() {
		return ins.blueprintI().employment().verb;
	}

	@Override
	public boolean jobUseTool() {
		return false;
	}

	@Override
	public SoundRace jobSound() {
		return ins.blueprintI().employment().sound();
	}

}
