package settlement.room.infra.inn;

import static settlement.main.SETT.ROOMS;

import game.audio.SoundRace;
import game.time.TIME;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
import settlement.room.main.ROOMA;
import settlement.room.main.util.RoomBits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;

final class ABed {

	private static final int UNMADE = 0;
	private static final int AVAILABLE = 1;
	private static final int RESERVED = 2;
	private static final int WORK_RESERVED = 3;
	private final Coo coo = new Coo();
	private final RoomBits claimed = new RoomBits(coo, 0x100);
	private final RoomBits work = new RoomBits(coo, 0x0F0);
	private final RoomBits state = new RoomBits(coo, 0x0F) {
		
		@Override
		public void set(ROOMA r, int t) {
			if (state.get() == AVAILABLE) {
				ins.service.report(service, blue.service, -1);
			}
			super.set(r, t);
			if (state.get() == AVAILABLE)
				ins.service.report(service, blue.service, 1);
			claimed.set(ins, 0);
		};
	};
	private InnInstance ins;
	private final ROOM_INN blue;

	ABed(ROOM_INN blue) {
		this.blue = blue;
	}
	
	public ABed init(int tx, int ty) {
		if (blue.is(tx, ty)) {
			if (ROOMS().fData.tileData.is(tx, ty, Constructor.ITAIL)) {
				coo.set(tx, ty);
				ins = blue.get(tx, ty);
				return this;
			}
		}
		return null;
	}
	
	static boolean isUnmade(int tx, int ty) {
		int s = (ROOMS().data.get(tx, ty) & 0x0F);
		return s == UNMADE || s == WORK_RESERVED;
	}
	
	static boolean isClaimed(int tx, int ty) {
		int s = (ROOMS().data.get(tx, ty) & 0x0100);
		return s != 0;
	}
	
	public static DIR sleepDir(int tx, int ty) {
		for (int i = 0; i < DIR.ORTHO.size(); i++) {
			DIR d = DIR.ORTHO.get(i);
			if (SETT.ROOMS().fData.tileData.is(tx, ty, Constructor.IHEAD))
				return d;
		}
		return DIR.C;
	}
	
	public final FSERVICE service = new FSERVICE() {

		@Override
		public void consume() {
			if (state.get() != RESERVED)
				throw new RuntimeException();
			state.set(ins, UNMADE);
			work.set(ins, 0);
			ins.jobs.searchAgain();
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
		public boolean findableReservedCanBe() {
			return state.get() == AVAILABLE;
		}

		@Override
		public void findableReserve() {
			if (state.get() != AVAILABLE)
				throw new RuntimeException();
			state.set(ins, RESERVED);
			
		}

		@Override
		public boolean findableReservedIs() {
			return state.get() == RESERVED;
		}
		
		@Override
		public void startUsing() {
			claimed.set(ins, 1);
		};

		@Override
		public void findableReserveCancel() {
			if (state.get() == RESERVED)
				state.set(ins, AVAILABLE);
		}
	};
		
	final SETT_JOB job = new SETT_JOB() {
		
		private int ws = (int) (TIME.workSeconds()/10);
		@Override
		public boolean jobUseTool() {
			return false;
		}
		
		@Override
		public void jobStartPerforming() {
			
		}
		
		@Override
		public SoundRace jobSound() {
			return ins.blueprintI().employment().sound();
		}
		
		@Override
		public RBIT jobResourceBitToFetch() {
			return null;
		}
		
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return state.get() == WORK_RESERVED;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			if (jobReservedIs(r)) {
				state.set(ins, UNMADE);
			}
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return state.get() == UNMADE;
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			if (!jobReserveCanBe())
				throw new RuntimeException();

			state.set(ins, WORK_RESERVED);
			
		}
		
		@Override
		public double jobPerformTime(Humanoid skill) {
			return ws;
		}

		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			if (!jobReservedIs(r))
				throw new RuntimeException();
			work.inc(ins, 1);
			if (work.get() == 8)
				state.set(ins, AVAILABLE);
			else {
				state.set(ins, UNMADE);
			}
			return null;
		};
		
		@Override
		public CharSequence jobName() {
			return blue.employment().verb;
		}
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		};
		
	};


	
}
