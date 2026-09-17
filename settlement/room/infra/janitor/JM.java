package settlement.room.infra.janitor;

import game.GAME;
import game.audio.SoundRace;
import game.faction.FACTIONS;
import game.faction.FResources.RTYPE;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class JM implements JOB_MANAGER{

	private JanitorInstance ins;
	private final ROOM_JANITOR b;
	private final Coo coo = new Coo();
	
	JM(ROOM_JANITOR b){
		this.b = b;
	}
	
	JOB_MANAGER get(JanitorInstance ins) {
		this.ins = ins;
		return this;
	}
	

	
	@Override
	public void reportResourceFound(RESOURCE res) {

	}

	@Override
	public boolean resourceReachable(RESOURCE res) {
		return !ins.bits.resMissing(res);
	}
	
	@Override
	public boolean resourceShouldSearch(RESOURCE res) {
		return SETT.PATH().finders.maintenance.mask(ins.mX(), ins.mY()).has(res);
	}
	
	@Override
	public SETT_JOB getReservableJob(COORDINATE prefered) {
		
		if (prefered == null) {
			if (!ins.bits.resMaskWorker(ins).isClear()) {
				coo.set(ins.rx, ins.ry);
				return res;
			}
			
			if (!ins.searchForJobs)
				return null;
			
			SETT_JOB j = search(ins.mX(), ins.mY(), ROOM_JANITOR.radius);
			
			if (j == null)
				ins.searchForJobs = false;
			
			return j;
		}
		
		if (!ins.bits.resMaskFetcherMust(ins).isClear()) {
			coo.set(ins.rx, ins.ry);
			return res;
		}
		
		if (ins.is(prefered) && !ins.bits.resMaskFetcher(ins).isClear()) {
			coo.set(ins.rx, ins.ry);
			return res;
		}
		
		int tx = prefered.x();
		int ty = prefered.y();
		
		coo.set(prefered);
		
		if (SETT.MAINTENANCE().reservable.is(tx, ty)) {
			RESOURCE res = SETT.MAINTENANCE().resource.get(tx, ty);
			if (res == null || ins.bits.resAm(res) > 0)
				return work;
		}
		if (!ins.is(prefered)) {
			SETT_JOB j = search(prefered.x(), prefered.y(), 32);
			if (j != null)
				return j;
		}
		
		if (!ins.searchForJobs)
			return null;
		
		return search(ins.mX(), ins.mY(), ROOM_JANITOR.radius);
	}

	@Override
	public SETT_JOB reportResourceMissing(RBIT resourceMask, int jx, int jy) {
		ins = b.get(jx, jy);
		if (ins != null) {
			ins.bits.resSetMissing(resourceMask);
			coo.set(jx, jy);
			SETT_JOB j = getReservableJob(coo);
			return j;
			
		}
		return null;
	}
	
	
	@Override
	public SETT_JOB getJob(COORDINATE c) {

		coo.set(c);
		
		if (coo.isSameAs(ins.rx, ins.ry)) {
			return res;
		}
		
		if (SETT.MAINTENANCE().isser.is(coo)) {
			return work;
		}
		
		return null;
	}
	
	private SETT_JOB search(int sx, int sy, int distance) {
		
		COORDINATE c = SETT.PATH().finders.maintenance.findWithin(ins.bits.resHave(), sx, sy, ROOM_JANITOR.radius, ins.mX(), ins.mY());
		
		if (c != null) {
			coo.set(c);
			return work;
		}
		
		return null;
	}

	private final SETT_JOB work = new Job();
	
	int lx,ly;
	
	private class Job implements SETT_JOB {
		
		private int wt = 20;
		
		@Override
		public boolean jobUseTool() {
			return true;
		}
		
		@Override
		public void jobStartPerforming() {
			
		}
		
		@Override
		public SoundRace jobSound() {
			return b.employment().sound();
		}
		
		@Override
		public RBIT jobResourceBitToFetch() {
			return null;
		}
		
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return SETT.MAINTENANCE().reserved.is(coo.x(), coo.y());
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			SETT.MAINTENANCE().reserved.set(coo, false);
			r = SETT.MAINTENANCE().resource.get(coo);
			if (r != null && ins.bits.resAm(r) > 0) {
				ins.bits.resInc(ins, r, 1);
				FACTIONS.player().res().inc(r, RTYPE.MAINTENANCE, 1);
			}
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return SETT.MAINTENANCE().reservable.is(coo);
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			if (coo.isSameAs(ins.rx, ins.ry))
				GAME.Notify("FUCKFUCK");
			r = SETT.MAINTENANCE().resource.get(coo);
			if (r != null && ins.bits.resAm(r) > 0) {
				ins.bits.resInc(ins, r, -1);
				FACTIONS.player().res().inc(r, RTYPE.MAINTENANCE, -1);
			}
			SETT.MAINTENANCE().reserved.set(coo, true);
		}
		
		@Override
		public double jobPerformTime(Humanoid skill) {
			if (coo.isSameAs(lx, ly) || SETT.MAINTENANCE().resource.get(coo) != null)
				return 1;
			
			return SETT.MAINTENANCE().pFreeFetch.is(coo) ? 1 : wt;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram) {
			SETT.MAINTENANCE().reserved.set(coo, false);
			SETT.MAINTENANCE().maintain(coo.x(), coo.y());

			lx = coo.x();
			ly = coo.y();
			SETT.MAINTENANCE().pFreeFetch.set(coo, ins.employees().fetchBonusConsume(wt+1));
			return null;
		}
		
		@Override
		public CharSequence jobName() {
			return b.employment().verb;
		}
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}
		
	}
	
	final SETT_JOB res = new SETT_JOB() {
		
		@Override
		public boolean jobUseTool() {
			return false;
		}
		
		@Override
		public void jobStartPerforming() {
			
		}
		
		@Override
		public SoundRace jobSound() {
			return null;
		}
		
		@Override
		public RBIT jobResourceBitToFetch() {
			return ins.bits.resMaskFetcher(ins);
		}
		
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return ins.bits.resReserved(r);
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			ins.bits.resReserve(ins, r, false);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return !ins.bits.resMaskFetcher(ins).isClear();
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			ins.bits.resReserve(ins, r, true);
		}
		
		@Override
		public double jobPerformTime(Humanoid skill) {
			return 0;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram) {
			ins.bits.resReserve(ins, r, false);
			ins.bits.resInc(ins, r, ram);
			
			boolean view = false;

			
			for (int i = 0; i < 8; i++) {
				if (((ins.tableRes >> (i*8)) & 0x0FF) == r.index()+1) {
					view = true;
					break;
				}
			}
			
			if (!view) {
				ins.tableRes = ins.tableRes << 8;
				ins.tableRes |= r.index()+1;
			}
			
			
			return null;
		}
		
		@Override
		public CharSequence jobName() {
			return b.employment().verb;
		}
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}
		
	};

	@Override
	public void resetResourceSearch() {
		ins.bits.update();
	}

}
