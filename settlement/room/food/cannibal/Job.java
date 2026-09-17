package settlement.room.food.cannibal;

import static settlement.main.SETT.ROOMS;

import game.audio.SoundRace;
import init.race.RACES;
import init.race.Race;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

class Job{
	
	private final ROOM_CANNIBAL print;
	final Work WORK = new Work();
	static final Bits gore = new Bits(0b00000000000001110000);
	static final Bits race = new Bits(0b11111111111100000000);
	
	Job(ROOM_CANNIBAL print) {
		this.print = print;
	}
	
	void reset(CannibalInstance ins, COORDINATE c) {
		int d = ROOMS().data.get(c);
		ROOMS().data.set(ins, c, gore.set(d, 0));
	}
	
	void gore(CannibalInstance ins, COORDINATE c) {
		int d = ROOMS().data.get(c);
		ROOMS().data.set(ins, c, gore.inc(d, 1));
	}
	
	public Race race(int tx, int ty) {
		return RACES.all().getC(race.get(SETT.ROOMS().data.get(tx, ty)));
	}
	
	SETT_JOB init(int tx, int ty, CannibalInstance ins) {
		if (!ins.is(tx, ty))
			return null;
		if (SETT.ROOMS().fData.tile.is(tx, ty, ins.blueprintI().constructor.ww))
			return WORK.init(tx, ty, ins);
		return null;
	}
	
	final class Work implements SETT_JOB{
		
		private static final int BITRESERVED = 	0b1;
		
		private final Coo coo = new Coo();
		CannibalInstance ins;
		int data;
		
		Work(){
		
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return !jobReservedIs(null);
		}
		
		Work init(int tx, int ty, CannibalInstance ins) {
			data = ROOMS().data.get(tx, ty);
			coo.set(tx, ty);
			this.ins = ins;
			return this;
		}
		
		void save() {
			ROOMS().data.set(ins, coo, data);
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

		@Override
		public RBIT jobResourceBitToFetch() {
			return null;
		}

		@Override
		public double jobPerformTime(Humanoid skill) {
			return 45;
		}

		@Override
		public void jobReserve(RESOURCE r) {
			if (jobReservedIs(null))
				throw new RuntimeException();
			data |= BITRESERVED;
			save();
		}

		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return (data & BITRESERVED) == BITRESERVED;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			data &= ~BITRESERVED;
			save();
		}
		
		@Override
		public void jobStartPerforming() {
			
		}

		@Override
		public SoundRace jobSound() {
			return null;
		}

		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE res, int ram) {
			return null;
		}
		
	}
	
}
