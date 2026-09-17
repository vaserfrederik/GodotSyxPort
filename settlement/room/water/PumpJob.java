package settlement.room.water;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.util.RoomBits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

class PumpJob implements SETT_JOB{


	private final ROOM_PUMP print;
	private final Coo coo = new Coo();
	private PumpInstance ins;
	
	private final RoomBits bReserved = new RoomBits(coo, 	0b0_0001);
	private final RoomBits bWorked = new RoomBits(coo, 		0b0_0010);
	
	PumpJob(ROOM_PUMP print) {
		this.print = print;
	}
	
	
	SETT_JOB init(int tx, int ty, PumpInstance ins) {
		if (!ins.is(tx, ty))
			return null;
		if (!print.constructor.isJob(tx, ty))
			return null;
		this.ins = ins;
		coo.set(tx, ty);
		return this;
	}

	
	
	@Override
	public boolean jobReserveCanBe() {
		if (jobReservedIs(null))
			return false;
		return true;
	}

	@Override
	public COORDINATE jobCoo() {
		return coo;
	}

	@Override
	public CharSequence jobName() {
		return print.employment().verb;
	}

	@Override
	public boolean jobUseTool() {
		return false;
	}
	
	boolean working(int data) {
		return bWorked.get(data) != 0;
	}

	@Override
	public RBIT jobResourceBitToFetch() {
		return null;
	}

	@Override
	public double jobPerformTime(Humanoid skill) {
		return wv;
	}

	@Override
	public void jobReserve(RESOURCE r) {
		if (jobReservedIs(null))
			throw new RuntimeException();
		bReserved.set(ins, 1);
	}

	@Override
	public boolean jobReservedIs(RESOURCE r) {
		return bReserved.get() == 1;
	}
	
	@Override
	public void jobReserveCancel(RESOURCE r) {
		bReserved.set(ins, 0);
		bWorked.set(ins, 0);
	}
	
	@Override
	public void jobStartPerforming() {
		bWorked.set(ins, 1);
	}

	@Override
	public SoundRace jobSound() {
		return ins.blueprintI().employment().sound();
	}

	private final double wv = 45;
	
	@Override
	public RESOURCE jobPerform(Humanoid s, RESOURCE res, int ram) {
		jobReserveCancel(res);
		return null;
	}
	
}
