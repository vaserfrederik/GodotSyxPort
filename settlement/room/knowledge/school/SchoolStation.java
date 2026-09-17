package settlement.room.knowledge.school;

import game.audio.SoundRace;
import game.time.TIME;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
import snake2d.util.bit.Bit;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;

final class SchoolStation {

	private SchoolInstance ins;
	private final ROOM_SCHOOL b;
	private final Work work = new Work();
	private final Service service = new Service();
	
	SchoolStation(ROOM_SCHOOL b){
		this.b = b;
	}
	
	FSERVICE service(int tx, int ty) {
		if (b.is(tx, ty)) {
			int i = SETT.ROOMS().fData.tileData.get(tx, ty);
			if (i == SchoolConstructor.ISERVICE) {
				ins = b.get(tx, ty);
				service.data = SETT.ROOMS().data.get(tx, ty);
				service.x = tx;
				service.y = ty;
				for (int di = 0; di < DIR.ORTHO.size(); di++) {
					int dx = DIR.ORTHO.get(di).x()+tx;
					int dy = DIR.ORTHO.get(di).y()+ty;
					if (ins.is(dx, dy) && SETT.ROOMS().fData.tileData.get(dx, dy) == SchoolConstructor.IWORK) {
						work.coo.set(dx, dy);
						work.data = SETT.ROOMS().data.get(dx, dy);
						return service;
					}
					
				}
			}
		}
		return null;
	}
	
	DIR serviceDir(int tx, int ty) {
		if (b.is(tx, ty)) {
			int i = SETT.ROOMS().fData.tileData.get(tx, ty);
			if (i == SchoolConstructor.ISERVICE) {
				ins = b.get(tx, ty);
				for (int di = 0; di < DIR.ORTHO.size(); di++) {
					int dx = DIR.ORTHO.get(di).x()+tx;
					int dy = DIR.ORTHO.get(di).y()+ty;
					if (ins.is(dx, dy) && SETT.ROOMS().fData.tileData.get(dx, dy) == SchoolConstructor.IWORK) {
						return DIR.ORTHO.get(di);
					}
					
				}
			}
		}
		return null;
	}
	
	SETT_JOB job(int tx, int ty) {
		if (b.is(tx, ty)) {
			int i = SETT.ROOMS().fData.tileData.get(tx, ty);
			if (i == SchoolConstructor.IWORK) {
				ins = b.get(tx, ty);
				work.data = SETT.ROOMS().data.get(tx, ty);
				work.coo.set(tx, ty);
				for (int di = 0; di < DIR.ORTHO.size(); di++) {
					int dx = DIR.ORTHO.get(di).x()+tx;
					int dy = DIR.ORTHO.get(di).y()+ty;
					if (ins.is(dx, dy) && SETT.ROOMS().fData.tileData.get(dx, dy) == SchoolConstructor.ISERVICE) {
						service.x = dx;
						service.y = dy;
						service.data = SETT.ROOMS().data.get(dx, dy);
						return work;
					}
					
				}
			}
		}
		return null;
	}
	
	void dispose(int tx, int ty) {
		if (job(tx, ty) == null)
			return;
		if (service.findableReservedCanBe())
			service.findableReserve();
		if (work.paper.get(work.data) > 0)
			SETT.THINGS().resources.create(work.jobCoo(), b.industry.ins().get(0).resource, work.paper.get(work.data));
	}
	
	final class Work implements SETT_JOB {

		private final Bit reserved 	= new Bit	(0b0000_0000_0001);
		private final Bits dones 	= new Bits	(0b0000_0000_0110);
		private final Bits paper 	= new Bits	(0b0000_1111_0000);
		private final Bits fetchFree = new Bits	(0b0001_0000_0000);
		private final Coo coo = new Coo();
		private int data;
		
		private int wt = (int) (TIME.workSeconds()/40);
		
		@Override
		public void jobReserve(RESOURCE r) {
			if (!jobReserveCanBe())
				throw new RuntimeException();
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
			
			if (reserved.is(data))
				return false;
			if (dones.get(data) < 3) {
				return true;
			}
			return false;
		}

		@Override
		public RBIT jobResourceBitToFetch() {
			if (paper.get(data) < 1)
				return b.industry.ins().get(0).resource.bit;
			return null;
		}

		@Override
		public double jobPerformTime(Humanoid skill) {
			if (fetchFree.get(data) == 1)
				return 0;
			return wt;
		}

		@Override
		public void jobStartPerforming() {
			
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram) {
			if (r == b.industry.ins().get(0).resource) {
				data = paper.inc(data, ram);
			}else {
				data = dones.inc(data, 1);
				data = fetchFree.set(data, 0);
				service.setReserveable();
			}
			
			if (fetchFree.get(data) == 0 && ins.employees().fetchBonusConsume(wt)) {
				data = fetchFree.set(data, 1);
			}
			
			jobReserveCancel(r);
			return null;
		}
		
		@Override
		public int jobResourcesNeeded(Humanoid skill) {
			return paper.mask;
		}

		@Override
		public COORDINATE jobCoo() {
			return coo;
		}

		@Override
		public CharSequence jobName() {
			return b.employment().verb;
		}

		@Override
		public boolean jobUseTool() {
			return false;
		}

		@Override
		public SoundRace jobSound() {
			return b.employment().sound();
		}
		
		private void save() {
			int c = data;
			data = SETT.ROOMS().data.get(coo);
			
			data = c;
			
			SETT.ROOMS().data.set(ins, coo, data);
		}
		
		void consume(boolean day) {
			data = dones.inc(data, -1);
			if (day) {
				int p = b.industry.ins().get(0).incDay(ins);
				if (p > 0) {
					data = paper.inc(data, -p);
				}
			}
			save();
			ins.jobs.searchAgain();
		}
		
	}
	
	final class Service implements FSERVICE {
		
		int x, y;
		int data;
		
		private final Bit reserved 		= new Bit(0b0000_0000_0001);
		private final Bit reservable	= new Bit(0b0000_0000_0010);
		private final Bit used			= new Bit(0b0000_0000_0100);
		@Override
		public int y() {
			return y;
		}
		
		@Override
		public int x() {
			return x;
		}
		
		@Override
		public boolean findableReservedIs() {
			return reserved.is(data);
		}
		
		@Override
		public boolean findableReservedCanBe() {
			return !reserved.is(data) && reservable.is(data);
		}
		
		@Override
		public void findableReserveCancel() {
			data = reserved.clear(data);
			save();
		}
		
		@Override
		public void findableReserve() {
			if (findableReservedCanBe()) {
				data = reserved.set(data);
				save();
			}
		}
		
		@Override
		public void startUsing() {
			if (findableReservedIs()) {
				data = used.set(data);
				save();
				work.consume(false);
			}
		}
		
		@Override
		public void consume() {
			data = 0;
			work.consume(true);
			if (work.dones.get(work.data) > 0) {
				data = reservable.set(data);
			}
			save();
		}
		
		void setReserveable(){
			data = reservable.set(data);
			save();
		}
		
		private void save() {
			int c = data;
			data = SETT.ROOMS().data.get(this);
			ins.service().report(this, ins.blueprintI().service(), -1);
			data = c;
			ins.service().report(this, ins.blueprintI().service(), 1);			
			SETT.ROOMS().data.set(ins, x, y, data);
		}
		
	}

	
}
