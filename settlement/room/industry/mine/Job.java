package settlement.room.industry.mine;

import static settlement.main.SETT.ROOMS;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.job.RoomResStorage;
import snake2d.util.bit.Bit;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

class Job{

	static final Bit isWork = new Bit(0b01);
	private static final Bit reserved = new Bit(0b010);
	private static final Bit used = new Bit(0b0100);

	private final ROOM_MINE print;
	private final Work WorkHands = new Work(false);
	private final Work WorkTools = new Work(true);
	final RoomResStorage storage;
	
	Job(ROOM_MINE print, int stor) {
		this.print = print;
		storage = new RoomResStorage(stor) {
			
			@Override
			public RESOURCE resource() {
				return print.minable.resource;
			}

			@Override
			protected boolean is(int tx, int ty) {
				return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.B_STORAGE;
			}
			
			@Override
			protected void changed(int tx, int ty) {
				if (hasRoom()) {
					MineInstance m = print.get(tx, ty);
					m.hasStorage = true;
				}
					
			}
			
		};
	}
	
	
	SETT_JOB init(int tx, int ty, MineInstance ins) {
		if (!ins.is(tx, ty))
			return null;
		int d = ROOMS().data.get(tx, ty);
		if (isWork.is(d) && SETT.MINERALS().getter.is(tx, ty, print.minable)) {
			return WorkTools.init(tx, ty, ins);
		}else if (ROOMS().fData.tileData.get(tx, ty) == Constructor.B_WORK) {
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
		MineInstance ins;
		int data;
		final static String name = "working";
		private final double wv = 45;
		
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
		
		Work init(int tx, int ty, MineInstance ins) {
			data = ROOMS().data.get(tx, ty);
			coo.set(tx, ty);
			this.ins = ins;
			return this;
		}
		
		void save() {
			int d = ROOMS().data.get(coo);
			if (used.is(d)) {
				ins.workage --;
			}
			ROOMS().data.set(ins, coo, data);
			if (used.is(data)) {
				ins.workage ++;
			}
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
			return 45;
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
		
		@Override
		public void jobStartPerforming() {
			data = used.set(data);
			save();
		}

		@Override
		public SoundRace jobSound() {
			return ins.blueprintI().employment().sound();
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid s, RESOURCE res, int ram) {
			
			jobReserveCancel(null);
			if (!ins.hasStorage)
				return null;
			int am = print.productionData.outs().get(0).work(s, ins, wv);
			
			if (am == 0)
				return null;
			
			
			
			int x1 = ins.sx;
			int y1 = ins.sy;
			RoomResStorage ss = storage.get(x1, y1, ins);
			
			while(ss != null) {
				if (am == 0 && ss.hasRoom())
					return null;
				if (ss.hasRoom()) {
					ss.deposit();
					am--;
					continue;
				}
				
				RoomResStorage sss = storage.get(ss.x()+1, ss.y(), ins);
				if (sss == null)
					sss = storage.get(x1, ss.y()+1, ins);
				ss = sss;
			}
			print.productionData.outs().get(0).inc(ins, -am);
			ins.hasStorage = false;

			
			return null;
		}
		
	}
	
}
