package settlement.room.infra.station;

import game.time.TIME;
import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.RESOURCE_TILE;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVE_SOURCE;
import settlement.room.infra.transport.ROOM_TRANSPORT;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomState;
import settlement.room.main.util.RoomState.RoomStateInstance;
import snake2d.LOG;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.Alloc;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.ArrayCooShort;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class StationInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_MOVE_SOURCE {

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	
	private final Jobs jobs;
	
	boolean auto;
	final ArrayCooShort crates;
	transient StationTally[] tally = new StationTally[RESOURCES.ALL().size()];
	private int[] incoming = Alloc.ii(RESOURCES.ALL().size());
	double prepared;
	
	RBITImp bamount = new RBITImp();
	RBITImp bcapacity = new RBITImp();
	
	StationInstance(ROOM_STATION p, TmpArea area, RoomInit init) {
		super(p, area, init);

		tally = new StationTally[RESOURCES.ALL().size()];
		
		for (RESOURCE res : RESOURCES.ALL())
			tally[res.index()] = new StationTally();
		
		int cr = 0;
		for (COORDINATE c : body()) {
			if (is(c) && (SETT.ROOMS().fData.tileData.get(c) & Constructor.BIT_CRATE) != 0) {
				cr++;
			}
		}
		crates = new ArrayCooShort(cr);
		for (COORDINATE c : body()) {
			if (is(c) && (SETT.ROOMS().fData.tileData.get(c) & Constructor.BIT_CRATE) != 0) {
				crates.get().set(c);
				crates.inc();
			}
		}

		jobs = new Jobs(this);
		
		employees().maxSet(ROOM_STATION.MAX_EMPLOYEES);
		employees().neededSet(ROOM_STATION.MAX_EMPLOYEES);

		activate();
	
	}
	
	@Override
	protected void loadFix() {
		incoming = RESOURCES.map().loader().fix(incoming, 0);
	};
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}
	
	@Override
	protected void dispose() {
		for (int i = 0; i < crates.size(); i++) {
			blueprintI().crate.get(crates.get().x(), crates.get().y());
			blueprintI().crate.resourceSet(null);
		}
		for (int ri = 0; ri < RESOURCES.ALL().size(); ri++) {
			RESOURCE r = RESOURCES.ALL().get(ri);
			blueprintI().tally(r).remove(tally(r), this);
			incoming[ri] = 0;
		}
		prepared = 0;
		for (int ri = 0; ri < RESOURCES.ALL().size(); ri++) {
			RESOURCE r = RESOURCES.ALL().get(ri);
			blueprintI().tally(r).add(tally(r), this);
			
		}
	}
	
	@Override
	protected void updateAction(double ds, boolean day) {
		jobs.searchAgain();
	}

	public void allocate(RESOURCE res, int am) {
		
		am = am-tally(res).crates();

		if (am == 0)
			return;
		
		if(am > 0) {
			for (int i = 0; i < crates.size(); i++) {
				if (blueprintI().crate.get(crates.get().x(), crates.get().y()).resource() == null) {
					blueprintI().crate.resourceSet(res);
					am--;
					if (am <= 0)
						return;
				}
				crates.inc();
			}
			am--;
		}
		if(am < 0) {
			for (int i = 0; i < crates.size(); i++) {
				if (blueprintI().crate.get(crates.get().x(), crates.get().y()).resource() == res) {
					blueprintI().crate.resourceSet(null);
					am++;
					if (am >= 0)
						return;
					
				}
				crates.inc();
			}
		}
	}
	
	public void deliver(RESOURCE res, int am) {
		
		unreserve(res);
		
		for (int i = 0; i < crates.size(); i++) {
			blueprintI().crate.get(crates.get().x(), crates.get().y());
			if (blueprintI().crate.resource() == res) {
				int a = blueprintI().crate.MAX_AM-blueprintI().crate.stored.get();
				a = CLAMP.i(a, 0, am);
				blueprintI().crate.deliver(a);
				am -= a;
				if (am <= 0)
					return;
			}
			crates.inc();
		}
		
		for (COORDINATE c : body()) {
			if (!SETT.PATH().solidity.is(c)) {
				SETT.THINGS().resources.create(c, res, am);
				return;
			}
				
		}
		
	}
	
	
	@Override
	protected void activateAction() {

	}

	@Override
	protected void deactivateAction() {
		
	}

	
	@Override
	public ROOM_STATION blueprintI() {
		return (ROOM_STATION) blueprint();
	}
	


	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}
	
	@Override
	public TILE_STORAGE storage(int tx, int ty) {
		return null;
	};
	
	private static class Jobs extends JobPositions<StationInstance> {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Jobs(StationInstance ins) {
			super(ins);
			randomize();
			setAlwaysNew();
		}
		
		@Override
		protected boolean isAndInit(int tx, int ty) {
			return ins.blueprintI().job.get(tx, ty) != null;
		}
		
		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().job.get(tx, ty);
		}
	}

	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		RESOURCE_TILE c = blueprintI().crate.get(tx, ty);
		if (c != null && c.resource() != null)
			return c;
		return null;
	} 
	
	public StationTally tally(RESOURCE res) {
		return tally[res.index()];
	}
	
	public int maxPrep() {
		return TIME.secondsPerDay()*crates.size()/2;
	}
	
	public double prepD() {
		return prepared/maxPrep();
	}
	
	public double efficiency() {
		double d = (double)employees().employed()/employees().max();
		d*= d;
		d *= 1.25;
		d += 8.0 / ROOM_TRANSPORT.MAX_LOAD;
		return d*(1-getDegrade());
	}
	
	public boolean accepting(RESOURCE res) {
		return tally(res).spaceAvailable() - incoming[res.index()] >= blueprintI().crate.MAX_AM && prepared() >= 1.0;
	}
	
	public double prepared() {
		return prepared*TIME.secondsPerDayI();
	}

	public int incoming(RESOURCE res) {
		return incoming[res.index()];
	}
	
	void setPrepared(double prepared) {
	
		int old = (int) prepared();
		prepared = CLAMP.d(prepared, 0, maxPrep());
		int nn = (int) (prepared*TIME.secondsPerDayI());
		if (old != nn && old*nn == 0) {
			for (int ri = 0; ri < RESOURCES.ALL().size(); ri++) {
				RESOURCE r = RESOURCES.ALL().get(ri);
				blueprintI().tally(r).remove(tally(r), this);
			}
			this.prepared = prepared;
			for (int ri = 0; ri < RESOURCES.ALL().size(); ri++) {
				RESOURCE r = RESOURCES.ALL().get(ri);
				blueprintI().tally(r).add(tally(r), this);
				
			}
		}else {
			this.prepared = prepared;
		}
		
		
	}
	
	public void reserve(RESOURCE res) {
		
		setPrepared(prepared-TIME.secondsPerDay());
		blueprintI().tally(res).remove(tally(res), this);
		incoming[res.index()] += blueprintI().crate.MAX_AM;
		incoming[res.index()] = CLAMP.i(incoming[res.index()], 0, Integer.MAX_VALUE);
		blueprintI().tally(res).add(tally(res), this);
	}
	
	public void unreserve(RESOURCE res) {
		blueprintI().tally(res).remove(tally(res), this);
		incoming[res.index()] -= blueprintI().crate.MAX_AM;
		incoming[res.index()] = CLAMP.i(incoming[res.index()], 0, Integer.MAX_VALUE);
		blueprintI().tally(res).add(tally(res), this);
	}

	private static final RBITImp tmp = new RBITImp();
	
	@Override
	public RESOURCE_TILE sourceCrate(RBIT okMask, int minAm, int ox, int oy, double limit) {
		
		tmp.clearSet(okMask);
		tmp.and(bamount);
		
		if (tmp.isClear())
			return null;
		
		for (RESOURCE r : RESOURCES.ALL()) {
			if (tmp.has(r)) {
				StationTally t = tally(r);
				double st = t.space();
				double am = t.stored()-t.reserved()-minAm;
				if (am <= 0 || limit > am/st) {
					tmp.clear(r);
				}
			}
		}
		
		if (tmp.isClear())
			return null;
		
		if (is(ox, oy)){
			RESOURCE_TILE s = blueprintI().crate.get(ox, oy);
			if (s != null && s.resource() != null && tmp.has(s.resource()) && s.reservable() >= minAm) {
				return s;
			}
		}
		

		for (int i = 0; i < crates.size(); i++) {
			crates.inc();
			RESOURCE_TILE s = blueprintI().crate.get(crates.get().x(), crates.get().y());
			if (s.resource() != null && tmp.has(s.resource()) && s.reservable() >= minAm) {
				return s;
			}
		}
		
		if (minAm == 1)
			LOG.ln("Weird indeed");

		return null;
		
	}

	@Override
	public RBIT sourceAmountMask() {
		return bamount;
	}

	@Override
	public RBIT moveCapacity() {
		return bcapacity;
	}

	@Override
	public int moveCapacityAm(RESOURCE res) {
		return tally(res).stored()-tally(res).reserved();
	}

	@Override
	public double storedD(RESOURCE res) {
		double sp = tally(res).space();
		if (sp == 0)
			return 1;
		return tally(res).stored()/sp;
	}
	
	@Override
	public RoomState makeState(int tx, int ty, boolean broken) {
		return new State(this, broken);
	}
	
	private static class State extends RoomStateInstance {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		private final short[] crates = new short[RESOURCES.ALL().size()];

		
		public State(StationInstance ins, boolean broken) {
			super(ins);
			for (RESOURCE r : RESOURCES.ALL()) {
				crates[r.index()] = (short) ins.tally[r.index()].crates();
			}
			
		}
		
		@Override
		public void applyIns(RoomInstance ins) {
			if (ins instanceof StationInstance) {
				StationInstance s = (StationInstance) ins;
				for (int ri = 0; ri < RESOURCES.ALL().size() && ri < RESOURCES.ALL().size(); ri++) {
					s.allocate(RESOURCES.ALL().get(ri), crates[ri]);
				}
			}
			
		}
		
		
	}

}