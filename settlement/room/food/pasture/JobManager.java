package settlement.room.food.pasture;

import static settlement.main.SETT.ROOMS;

import game.GAME;
import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import snake2d.CircleCooIterator;
import snake2d.util.bit.Bit;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.rnd.RND;
import util.GUTIL;

final class JobManager implements JOB_MANAGER{

	private PastureInstance ins;
	private final static JobManager i = new JobManager();
	private final Job job = new Job();
	private final JobBaby jobBaby = new JobBaby();
	final static int workTime = 20;
	private final Bit reserved = new Bit(0b010);
	private final Bit is = new Bit(0b0100);
	
	static JobManager init(PastureInstance ins) {
		i.ins = ins;
		return i;
	}
	
	
	private SETT_JOB getReservableJob() {
		

		if (ins.needsWork()) {
			if (findAvailable()) {
				return job;
			}
			return null;
		}
		
		if (ins.hasLivestockFetch() && ins.searchForLivestock) {
			if (findAvailable()) {
				return jobBaby.init(job.coo.x(), job.coo.y());
			}
			return null;
		}
		return null;
		
	}
	
	private boolean findAvailable() {
		int tx = ins.body().x1()+1 + RND.rInt(ins.body().width()-2);
		int ty = ins.body().y1()+1 + RND.rInt(ins.body().height()-2);
		
		int a = ins.body().width()*ins.body().height();
		for (int i = 0; i < a; i++) {
			if (isAvailable(tx, ty)) {
				return true;
			}
			tx++;
			if (tx >= ins.body().x2()) {
				tx = ins.body().x1()+1;
				ty++;
				if (ty >= ins.body().y2())
					ty = ins.body().y1()+1;
			}
			
		}
		GAME.Notify("" + tx + " " + ty);
		return false;
	}
	
	private boolean isAvailable(int tx, int ty) {
		if (!ins.is(tx, ty))
			return false;
		if (SETT.PATH().availability.get(tx, ty).player < 0)
			return false;
		SETT_JOB j = job.init(tx, ty); 
		return j != null && j.jobReserveCanBe();
	}

	@Override
	public SETT_JOB reportResourceMissing(RBIT resourceMask, int jx, int jy) {
		ins.missingLivestock = true;
		ins.searchForLivestock = false;
		return getReservableJob();
	}
	
	@Override
	public void reportResourceFound(RESOURCE res) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public boolean resourceReachable(RESOURCE res) {
		if (res == RESOURCES.LIVESTOCK())
			return !ins.missingLivestock;
		return true;
	}

	
	@Override
	public SETT_JOB getReservableJob(COORDINATE prefered) {
		if (prefered == null)
			return getReservableJob();
		else
			return getReservableAdjacentJob(prefered);
		
		
	}

	@Override
	public SETT_JOB getJob(COORDINATE c) {
		int tx = c.x();
		int ty = c.y();
		if (!ins.is(tx, ty))
			return null;
		if (SETT.PATH().availability.get(tx, ty).player < 0)
			return null;
		job.init(tx, ty);
		if (is.is(job.data))
			return jobBaby.init(tx, ty);
		return job;
	}
	
	public SETT_JOB getJob(int tx, int ty) {
		if (!ins.is(tx, ty))
			return null;
		if (SETT.PATH().availability.get(tx, ty).player < 0)
			return null;
		job.init(tx, ty);
		if (is.is(job.data))
			return jobBaby.init(tx, ty);
		return job;
	}

	private SETT_JOB getReservableAdjacentJob(COORDINATE c) {
		if (!ins.needsWork())
			return getReservableJob();
		CircleCooIterator it = GUTIL.circle();
		int i = RND.rInt(10);
		while(it.radius(i++)<5) {
			if (RND.oneIn(4)) {
				if (isAvailable(c.x() + it.get(i).x(), c.y() + it.get(i).y()))
					return job;
			}
		}
		return null;
	}
	
	private class JobBaby implements SETT_JOB {

		
		private int data = 0;
		private Coo coo = new Coo();
		
		JobBaby init(int tx, int ty) {
			coo.set(tx, ty);
			data = ROOMS().data.get(tx, ty);
			return this;
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			if (jobReservedIs(r) || r != RESOURCES.LIVESTOCK())
				throw new RuntimeException();
			ins.consumeALivestockFetch();
			data = reserved.set(data);
			data = is.set(data);
			ROOMS().data.set(ins, coo, data);
			ins.missingLivestock = false;
		}

		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return reserved.is(data);
		}

		@Override
		public void jobReserveCancel(RESOURCE r) {
			data = reserved.clear(data);
			data = is.clear(data);
			ROOMS().data.set(ins, coo, data);
		}

		@Override
		public boolean jobReserveCanBe() {
			return !jobReservedIs(RESOURCES.LIVESTOCK());
		}

		@Override
		public RBIT jobResourceBitToFetch() {
			return RESOURCES.LIVESTOCK().bit;
		}

		@Override
		public double jobPerformTime(Humanoid skill) {
			return 0;
		}

		@Override
		public void jobStartPerforming() {
			
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			jobReserveCancel(null);
			ins.work(skill, r, coo);
			return null;
		}
		
		@Override
		public int jobResourcesNeeded(Humanoid skill) {
			return 1;
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
			return true;
		}

		@Override
		public SoundRace jobSound() {
			return null;
		}
		
		@Override
		public boolean longFetch() {
			return true;
		}


		
	}
	
	private class Job implements SETT_JOB {

		private int data = 0;
		private Coo coo = new Coo();
		
		Job init(int tx, int ty) {
			coo.set(tx, ty);
			data = ROOMS().data.get(tx, ty);
			return this;
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			if (jobReservedIs(r))
				throw new RuntimeException();
			data = reserved.set(data);
			data = is.clear(data);
			ROOMS().data.set(ins, coo, data);
		}

		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return reserved.is(data);
		}

		@Override
		public void jobReserveCancel(RESOURCE r) {
			data = reserved.clear(data);
			ROOMS().data.set(ins, coo, data);
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
		public double jobPerformTime(Humanoid skill) {
			return workTime;
		}

		@Override
		public void jobStartPerforming() {
			
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram) {
			jobReserveCancel(r);
			ins.work(skill, r, coo);
			return null;
		}
		
		@Override
		public int jobResourcesNeeded(Humanoid skill) {
			return 0;
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
			return true;
		}

		@Override
		public SoundRace jobSound() {
			return ins.blueprintI().employment().sound();
		}
		
	}

	@Override
	public void resetResourceSearch() {
		ins.missingLivestock = false;
		ins.searchForLivestock = true;
	}


	@Override
	public boolean resourceShouldSearch(RESOURCE res) {
		return ins.searchForLivestock;
	}

}
