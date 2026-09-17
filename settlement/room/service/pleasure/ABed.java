package settlement.room.service.pleasure;

import static settlement.main.SETT.ROOMS;

import game.audio.SoundRace;
import game.time.TIME;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
import settlement.room.main.ROOMA;
import settlement.room.main.util.RoomBits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class ABed {

	private static final int UNAVAILABLE = 0;
	private static final int AVAILABLE = 1;
	private static final int RESERVED = 2;
	
	
	private final Coo coo = new Coo();
	private final RoomBits state = new RoomBits(coo, 0x0F) {
		
		@Override
		public void set(ROOMA r, int t) {
			if (state.get() == AVAILABLE) {
				ins.service.report(service, blue.service, -1);
			}
			super.set(r, t);
			if (state.get() == AVAILABLE)
				ins.service.report(service, blue.service, 1);
			wdata.set(r, 0);
		};
	};
	
	private final RoomBits worked = new RoomBits(coo, 0b0001_0000);
	private final RoomBits workedHasBeen = new RoomBits(coo, 0b0010_0000);
	
	private final RoomBits clientReady = new RoomBits(coo, 		0b0000_0001_0000_0000);
	public  final RoomBits clientUndressed = new RoomBits(coo, 	0b0000_0010_0000_0000);
	private final RoomBits workerReady = new RoomBits(coo, 		0b0000_0100_0000_0000);
	private final RoomBits workerUndressed = new RoomBits(coo, 	0b0000_1000_0000_0000);
	private final RoomBits wdata = new RoomBits(coo, 			0b0000_1111_0000_0000);
	
	
	private PleasureInstance ins;
	private final ROOM_PLEASURE blue;

	ABed(ROOM_PLEASURE blue) {
		this.blue = blue;
	}
	
	public ABed init(int tx, int ty) {
		if (blue.is(tx, ty)) {
			if (ROOMS().fData.tileData.is(tx, ty, Constructor.ISERVICE)) {
				coo.set(tx, ty);
				ins = blue.get(tx, ty);
				return this;
			}
		}
		return null;
	}
	
	
	public boolean clientShouldUndress() {
		clientReady.set(ins, 1);
		if (workerUndressed.get() == 1) {
			return true;
		}
		return false;
	}
	
	public void clientUndress() {
		clientReady.set(ins, 1);
		clientUndressed.set(ins, 1);
	}
	
	public boolean workerReadyShouldUndress() {
		workerReady.set(ins, 1);
		if (clientReady.get() == 1) {
			workerUndressed.set(ins, 1);
			return true;
		}
		return false;
	}
	
	public final FSERVICE service = new FSERVICE() {

		@Override
		public void consume() {
			if (state.get() != RESERVED)
				throw new RuntimeException();
			if (worked.get() == 1 || workedHasBeen.get() == 1) {
				state.set(ins, AVAILABLE);
				workedHasBeen.set(ins, 0);
			}else
				state.set(ins, UNAVAILABLE);
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
			return worked.get() == 1;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			if (jobReservedIs(r)) {
				worked.set(ins, 0);
			}
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return !jobReservedIs(null);
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			if (!jobReserveCanBe())
				throw new RuntimeException();
			worked.set(ins, 1);
			if (state.get() == UNAVAILABLE)
				state.set(ins, AVAILABLE);
		}
		
		@Override
		public double jobPerformTime(Humanoid skill) {
			return ws;
		}

		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			if (!jobReservedIs(r))
				throw new RuntimeException();
			worked.set(ins, 0);
			if (state.get() == UNAVAILABLE)
				state.set(ins, AVAILABLE);
			else
				workedHasBeen.set(ins, 1);
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
