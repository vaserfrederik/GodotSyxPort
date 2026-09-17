package settlement.room.law.stockade;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.IndustryResource;
import settlement.room.main.util.RoomBits;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class Job {

	public final static int ISTAND = 1;
	public final static int IFOOD = 2;
	public final static int ISHIT = 3;
	
	private final ROOM_STOCKADE b;
	private final Coo coo = new Coo();
	private StockInstance ins;
	private final RoomBits type = new RoomBits(coo, new Bits(0b0111));
	private final RoomBits reserved = new RoomBits(coo, new Bits(0b1000));
	private final RoomBits sreserved = new RoomBits(coo, new Bits(0b10000));
	private final RoomBits data = new RoomBits(coo, new Bits(~0b11111));
	
	
	Job(ROOM_STOCKADE b){
		this.b = b;
	}
	
	public SETT_JOB job(int tx, int ty) {
		ins = b.getter.get(tx, ty);
		if (ins != null) {
			coo.set(tx, ty);
			if (type.get() != 0)
				return job;
		}
		return null;
	}
	
	public int food(int tx, int ty) {
		if (job(tx, ty) != null && type.get() == IFOOD)
			return data.get();
		return 0;
	}
	
	public int shit(int tx, int ty) {
		if (job(tx, ty) != null && type.get() == ISHIT)
			return data.get();
		return 0;
	}
	
	public int type(int tx, int ty) {
		if (job(tx, ty) != null)
			return type.get();
		return 0;
	}
	
	public boolean reserve(int tx, int ty, int type, boolean reserve, boolean use) {
		if (job(tx, ty) == null)
			return false;
		if (this.type.get() == type) {
			if (type == IFOOD && data.get() <= 0)
				return false;
			
			if (use) {
				if (type == IFOOD)
					data.inc(ins, -1);
				else if (type == ISHIT)
					data.inc(ins, 1);
			}
			
			if (reserve) {
				if (sreserved.get() == 0) {
					sreserved.set(ins, 1);
					return true;
				}
				return false;
			}else {
				sreserved.set(ins, 0);
				return true;
			}
			

		}
		return false;
		
	}
	
	
	private final SETT_JOB job = new SETT_JOB() {
		
		@Override
		public boolean jobUseTool() {
			return false;
		}
		
		@Override
		public boolean jobUseHands() {
			return type.get() == ISHIT;
		};
		
		@Override
		public void jobStartPerforming() {
			
		}
		
		@Override
		public SoundRace jobSound() {
			return b.employment().sound();
		}
		
		@Override
		public RBIT jobResourceBitToFetch() {
			if (type.get() == IFOOD) {
				return ins.fetch;
			}
			return null;
		}
		
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return reserved.get() == 1;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			reserved.set(ins, 0);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			if (type.get() == IFOOD && data.get() > 8)
				return false;
			return reserved.get() == 0;
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			if (r != null) {
				ins.jobs.resetResourceSearch();
			}
			reserved.set(ins, 1);
		}
		
		@Override
		public double jobPerformTime(Humanoid a) {
			if (type.get() == ISTAND)
				return 60;
			return 20;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			if (type.get() == IFOOD && rAm > 0) {
				data.inc(ins, rAm);
				for (IndustryResource ii : b.indu.ins()) {
					if (ii.resource == r)
						ii.inc(ins, rAm);
				}
			}else
				data.set(ins, 0);
			reserved.set(ins, 0);
			return null;
		}
		
		@Override
		public CharSequence jobName() {
			return b.employment().title;
		}
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}
	};
	
}
