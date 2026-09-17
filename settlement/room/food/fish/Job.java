package settlement.room.food.fish;

import static settlement.main.SETT.ROOMS;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.job.RoomResStorage;
import snake2d.util.bit.Bit;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

class Job{

	static final Bit isWork = new Bit(0b0001);
	static final Bit isShip = new Bit(0b0010);
	static final Bit reserved = new Bit(0b0100);
	static final Bit used = new Bit(0b1000);
	static final Bits shipDir = new Bits(0b1111_0000);
	

	private final ROOM_FISHERY print;
	private final Work WorkHands = new Work(false);
	final RoomResStorage storage = new RoomResStorage(0b011111) {
		
		@Override
		public RESOURCE resource() {
			return print.productionData.outs().get(0).resource;
		}

		@Override
		protected boolean is(int tx, int ty) {
			return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.B_STORAGE;
		}
		
		@Override
		protected void changed(int tx, int ty) {
			if (hasRoom()) {
				FishInstance m = print.get(tx, ty);
				m.hasStorage = true;
			}
				
		}
		
	};
	
	Job(ROOM_FISHERY print) {
		this.print = print;
	}
	
	
	SETT_JOB init(int tx, int ty, FishInstance ins) {
		if (!ins.is(tx, ty))
			return null;
		int d = ROOMS().data.get(tx, ty);

		if (SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.B_STORAGE)
			return null;
		
		if (isWork.is(d) || ROOMS().fData.tileData.get(tx, ty) == Constructor.B_WORK) {
			return WorkHands.init(tx, ty, ins);
		}
		return null;
	}
	
	static boolean working(int data) {
		return used.is(data);
	}
	
	final class Work implements SETT_JOB{
		
		
		
		private final boolean tools;
		
		private final Coo coo = new Coo();
		FishInstance ins;
		int data;
		final static String name = "working";
		private final double wv = 60;
		
		Work(boolean tools){
			this.tools = tools;
		}
		
		
		
		@Override
		public boolean jobReserveCanBe() {
			if (jobReservedIs(null))
				return false;
			if (!ins.hasStorage)
				return false;
			return true;
		}
		
		Work init(int tx, int ty, FishInstance ins) {
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
		public String jobName() {
			return name;
		}

		@Override
		public boolean jobUseTool() {
			return tools;
		}

		@Override
		public RBIT jobResourceBitToFetch() {
			return null;
		}

		@Override
		public double jobPerformTime(Humanoid skill) {
			return 60;
		}

		@Override
		public void jobReserve(RESOURCE r) {
			if (jobReservedIs(null))
				throw new RuntimeException();
			data = reserved.set(data);
			save();
		}

		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return (reserved.is(data));
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			data = reserved.clear(data);
			data = used.clear(data);
			save();
		}
		
		long now;
		
		@Override
		public void jobStartPerforming() {
			now = System.currentTimeMillis();
			data = used.set(data);
			save();
		}

		@Override
		public SoundRace jobSound() {
			return ins.blueprintI().employment().sound();
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid s, RESOURCE res, int ram) {
			secretPerform(s, wv);
			return null;
		}
		
	}
	
	public void secretPerform(Humanoid s, double time) {
		
		WorkHands.jobReserveCancel(null);
		
		int am = print.productionData.outs().get(0).work(s, WorkHands.ins, time);
		
		if (am == 0)
			return;
		
		if (!WorkHands.ins.hasStorage)
			return;
		
		int x1 = WorkHands.ins.sx;
		int y1 = WorkHands.ins.sy;
		RoomResStorage ss = storage.get(x1, y1, WorkHands.ins);
		
		while(ss != null) {
			if (am == 0 && ss.hasRoom())
				return;
			if (ss.hasRoom()) {
				ss.deposit();
				am--;
				continue;
			}
			
			RoomResStorage sss = storage.get(ss.x()+1, ss.y(), WorkHands.ins);
			if (sss == null)
				sss = storage.get(x1, ss.y()+1, WorkHands.ins);
			ss = sss;
		}
		print.productionData.outs().get(0).inc(WorkHands.ins, -am);
		WorkHands.ins.hasStorage = false;
	}
	
}
