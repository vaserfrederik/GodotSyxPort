package settlement.room.industry.woodcutter;

import static settlement.main.SETT.TERRAIN;

import game.audio.SoundRace;
import game.time.TIME;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.ROOMA;
import settlement.room.main.job.RoomResStorage;
import settlement.room.main.util.RoomBits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import util.GUTIL;

class Job{

	private final ROOM_WOODCUTTER print;
	private final Coo jobCoo = new Coo();
	private Instance ins;
	private final RoomBits is = new RoomBits(jobCoo, 0b0000_0001);
	private final RoomBits reserved = new RoomBits(jobCoo, 0b0000_0010);
	private final RoomBits used = new RoomBits(jobCoo, 0b0000_0100) {
		
		@Override
		public void set(ROOMA r, int t) {
			ins.workage -= super.get();
			super.set(r, t);
			ins.workage += super.get();
		};
		
	};
	private final RoomBits chopped = new RoomBits(jobCoo, 0xFFFFFFF0);
	
	
	private final double wv = 60;
	private final int workPerDay = (int) Math.ceil(TIME.workSeconds()/wv);
	
	final RoomResStorage storage;
	
	Job(ROOM_WOODCUTTER print, int store) {
		this.print = print;
		storage = new RoomResStorage(store) {
			
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
					Instance m = print.get(tx, ty);
					m.hasStorage = true;
				}
					
			}
			
		};
	}
	
	
	SETT_JOB init(int tx, int ty, Instance ins) {
		if (!ins.is(tx, ty))
			return null;
		jobCoo.set(tx, ty);
		this.ins = ins;
		if (is.get() == 0)
			return null;
		
		return work;
		
	}
	
	void mark(int tx, int ty, Instance ins) {
		jobCoo.set(tx, ty);
		this.ins = ins;
		is.set(ins, 1);
		
	}
	
	
	void update(int tx, int ty, Instance ins) {
		jobCoo.set(tx, ty);
		if (is.get() == 0)
			return;
		if (SETT.ROOMS().fData.item.get(tx, ty) != null)
			return;
		if (SETT.ROOMS().fData.tileData.get(jobCoo) == Constructor.B_WORK)
			return;
		if (SETT.ROOMS().fData.tileData.get(jobCoo) == Constructor.B_STORAGE)
			return;
		if (chopped.get() == 0) {
			chopped.inc(ins, 1);
			return;
		}
		
		double d = ins.irri - (GUTIL.ran2().get(tx, ty)%0x0FF)/(double)0x0FF;
		if (d > 0) {
			if (SETT.TERRAIN().TREES.isTree(jobCoo.x(), jobCoo.y())) {
				TERRAIN().TREES.amount.increment(tx, ty, 1);
			}else if (SETT.TERRAIN().BUSH.is(jobCoo)) {
				TERRAIN().TREES.SMALL.placeRaw(tx, ty);
				TERRAIN().TREES.amount.set(tx, ty, 1);
			}else {
				SETT.TERRAIN().BUSH.placeFixed(tx, ty);
			}
		}else {
			if (SETT.TERRAIN().TREES.isTree(jobCoo.x(), jobCoo.y())) {
				SETT.TERRAIN().BUSH.placeFixed(tx, ty);
			}else if (SETT.TERRAIN().BUSH.is(jobCoo)) {
				TERRAIN().NADA.placeRaw(tx, ty);
			}
		}
		
	}
	
	static boolean isTreeCurrent(int tx, int ty) {
		return SETT.TERRAIN().TREES.isTree(tx, ty) || SETT.TERRAIN().BUSH.is(tx, ty);
	}
	
	static boolean working(int data) {
		return (data & 0b10) != 0;
	}
	
	private final SETT_JOB work = new SETT_JOB(){
		
		@Override
		public boolean jobReserveCanBe() {
			if (jobReservedIs(null))
				return false;
			if (!ins.hasStorage)
				return false;
			return true;
		}

		@Override
		public COORDINATE jobCoo() {
			return jobCoo;
		}

		@Override
		public CharSequence jobName() {
			return print.employment().verb;
		}

		@Override
		public boolean jobUseTool() {
			return true;
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
			reserved.set(ins, 1);
		}

		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return reserved.get() == 1;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			reserved.set(ins, 0);
			used.set(ins, 0);
		}

		@Override
		public void jobStartPerforming() {
			used.set(ins, 1);
		}

		@Override
		public SoundRace jobSound() {
			return ins.blueprintI().employment().sound();
		}

		@Override
		public RESOURCE jobPerform(Humanoid s, RESOURCE res, int ram) {
			
			jobReserveCancel(null);
			
			chopped.inc(ins, 1);
			if (chopped.get() > workPerDay && SETT.ROOMS().fData.tileData.get(jobCoo) != Constructor.B_WORK) {
				TERRAIN().DECOR_WOOD.placeFixed(jobCoo.x(), jobCoo.y());
				chopped.set(ins, 0);
			}
			
			int am = print.productionData.outs().get(0).work(s, ins, wv);
			
			if (am == 0)
				return null;
			
			if (!ins.hasStorage)
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
		
	};

	
}
