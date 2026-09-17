package settlement.room.military.training.barracks;

import static settlement.main.SETT.ROOMS;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.misc.job.SETT_JOB;
import snake2d.util.bit.Bit;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;

final class BarracksThing implements SETT_JOB{

	private final Coo coo = new Coo();
	final Coo cooMan = new Coo();
	private int data;
	private int dataManikin;
	private final ROOM_BARRACKS b;
	private BarracksInstance ins;
	
	static final Bit reserved 	= new Bit(0b00001);
	static final Bit used 		= new Bit(0b00010);
	
	
	BarracksThing(ROOM_BARRACKS b) {
		this.b = b;
	}
	
	private void save() {
		
		ROOMS().data.set(ins, cooMan, dataManikin);
		ROOMS().data.set(ins, coo, data);
		
		
	}
	
	BarracksThing init(int tx, int ty) {
		coo.set(tx, ty);
		if (ROOMS().fData.tile.is(coo, b.constructor.work)) {
			ins = b.get(coo.x(), coo.y());
			for (int di = 0; di < DIR.ORTHO.size(); di++) {
				DIR d = DIR.ORTHO.get(di);
				if (ins.is(tx, ty, d) && ROOMS().fData.tile.is(coo, d, b.constructor.manikin)) {
					ins = b.get(coo.x(), coo.y());
					cooMan.set(coo);
					cooMan.increment(d.x(), d.y());
					data = ROOMS().data.get(coo);
					dataManikin = ROOMS().data.get(cooMan);
					return this;
				}
			}
			throw new RuntimeException();
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
		dataManikin = used.clear(dataManikin);
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
		dataManikin = used.set(dataManikin);
		save();
		
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
