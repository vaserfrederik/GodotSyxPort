package settlement.room.main.job;

import static settlement.main.SETT.PATH;

import java.io.Serializable;

import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.RoomInstance;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.rnd.RND;
import util.GUTIL;

public abstract class JobIterator implements JOB_MANAGER, Serializable{

	protected final RoomInstance ins;
	private final Coo search = new Coo();
	private boolean hasSearchedAll;
	private final RBITImp resSearch = new RBITImp();
	private final RBITImp resNotFound = new RBITImp();
	private boolean alwaysNew = false;
	private boolean randomize = false;
	
	private final static long serialVersionUID = -6055758404619685045l;
	
	public JobIterator(RoomInstance ins) {
		this.ins = ins;
		search.set(
				ins.body().x1() + RND.rInt(ins.body().width()), 
				ins.body().y1() + RND.rInt(ins.body().height()));
	}
	
	public void setAlwaysNewJob() {
		alwaysNew = true;
	}
	
	public void randomize() {
		randomize = true;
	}
	
	public void randomizeN() {
		randomize = false;
	}
	
	@Override
	public SETT_JOB reportResourceMissing(RBIT resMask, int jx, int jy) {
		resSearch.or(resMask);
		resNotFound.or(resMask);
		return getReservableJob();
	}
	
	@Override
	public void reportResourceFound(RESOURCE res) {
		resSearch.clear(res);
		resNotFound.clear(res);
		return;
	}
	
	private boolean resourceCheck(SETT_JOB j) {
		if (j.jobResourceBitToFetch() == null) {
			return true;
		}
		if (!PATH().finders.resource.normal.has(j.jobCoo().x(), j.jobCoo().y(), j.jobResourceBitToFetch())) {
			resSearch.or(j.jobResourceBitToFetch());
			resNotFound.or(j.jobResourceBitToFetch());
			return false;
		}
		return !resSearch.hasAll(j.jobResourceBitToFetch());
			
	}
	
	@Override
	public boolean resourceReachable(RESOURCE res) {
		return !resNotFound.has(res.bit);
	}
	
	@Override
	public boolean resourceShouldSearch(RESOURCE res) {
		return !resSearch.has(res.bit);
	}
	
	private SETT_JOB getReservableJob() {
		
		if (hasSearchedAll)
			return null;
		if (randomize)
			search.set(
					ins.body().x1() + RND.rInt(ins.body().width()), 
					ins.body().y1() + RND.rInt(ins.body().height()));
		
		int tiles = ins().area();
		for (int i = 0; i < tiles; i++) {
			if (ins.is(search)) {
				SETT_JOB j = reservable(search.x(), search.y());
				if (j != null && resourceCheck(j)) {
					if (alwaysNew)
						incSearch();
					return j;
				}
			}
			incSearch();
		}
		hasSearchedAll = true;
		return null;
	}
	
	private SETT_JOB reservable(int tx, int ty) {
		if (!ins.is(tx, ty))
			return null;
		SETT_JOB j = init(tx, ty);
		if (j == null)
			return null;
		if (!j.jobReserveCanBe())
			return null;
		if (!resourceCheck(j)) {
			return null;	
		}
		return j;
	}
	
	
	@Override
	public SETT_JOB getReservableJob(COORDINATE prefered) {
		if (alwaysNew || prefered == null)
			return getReservableJob();
		int tx = prefered.x();
		int ty = prefered.y();
		
		SETT_JOB j = reservable(tx, ty);
		if (j == null)
			j = getReservableAdjacentJob(tx, ty);
		if (j == null)
			return getReservableJob();
		return j;
		
	}
	
	private void incSearch() {
		do {
			search.increment(1, 0);
			if (search.x() >= ins.body().x2()) {
				search.increment(0, 1);
				search.xSet(ins.body().x1());
				if (search.y() >= ins.body().y2())
					search.ySet(ins.body().y1());
			}
		}while(!ins.is(search));
		
	}

	public boolean hasSearchedAll() {
		return hasSearchedAll;
	}

	@Override
	public SETT_JOB getJob(COORDINATE c) {
		if (ins.is(c)) {
			return init(c.x(), c.y());
		}
		return null;
	}

	protected abstract SETT_JOB init(int tx, int ty);
	
	public void searchAgain() {
		resSearch.clear();
		this.hasSearchedAll = false;
	}
	
	@Override
	public void resetResourceSearch() {
		resSearch.clear();
		resNotFound.clear();
		
	}
	
	public void searchAgainWithoutResources() {
		this.hasSearchedAll = false;
	}
	

	private SETT_JOB getReservableAdjacentJob(int tx, int ty) {
		
		if (hasSearchedAll)
			return null;
		
		if (alwaysNew)
			return null;
		
		int i = 1;
		while(GUTIL.circle().radius(i) < 5) {
			SETT_JOB j = reservable(tx + GUTIL.circle().get(i).x(), ty + GUTIL.circle().get(i).y());
			if (j != null)
				return j;
			i++;
		}
		return null;
	}

	public void dontSearch() {
		this.hasSearchedAll = true;
	}
	
	protected RoomInstance ins() {
		return ins;
	}
}