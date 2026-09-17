package settlement.room.infra.transport;

import game.time.TIME;
import init.resources.RBIT;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.infra.logistics.MoveJob;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVEJOBBER;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVE_DEST;
import settlement.room.infra.logistics.MoveOrderPull;
import settlement.room.infra.logistics.MoveOrderPull.MoveOrderPullInstance;
import settlement.room.infra.stockpile.ROOM_STOCKPILE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.job.ROOM_RADIUS.ROOM_RADIUS_INSTANCE;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomState;
import settlement.room.main.util.RoomState.RoomStateInstance;
import snake2d.LOG;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.misc.CLAMP;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class TransportInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_RADIUS_INSTANCE, ROOM_MOVEJOBBER, ROOM_MOVE_DEST, MoveOrderPullInstance{

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	
	final static int ORDERS = 4;
	
	private final Jobs jobs;
	
	boolean auto;
	
	private boolean fetching = true;
	boolean prio = false;
	byte coolFetch = 0;
	
	final Cart data;
	
	final MoveOrderPull[] pullOrders = new MoveOrderPull[ORDERS];
	private byte orderIP = 0;
	
	private short lastSourceX,lastSourceY;
	byte radius = 20;

	private final short sx,sy;
	
	float fetchTime;
	float distance;
	float stationWorkers;
	boolean stationProblem = false;
	
	TransportInstance(ROOM_TRANSPORT p, TmpArea area, RoomInit init) {
		super(p, area, init);

		data = new Cart();;
		int sx = -1;
		int sy = -1;
		for (COORDINATE c : body()) {
			if (is(c) && SETT.ROOMS().fData.tile.get(c) == p.constructor.an) {
				sx = c.x();
				sy = c.y();
			}
		}
		if (sx == -1)
			throw new RuntimeException();
		this.sx = (short) sx;
		this.sy = (short) sy;
		jobs = new Jobs(this);
		
		employees().maxSet((int) (jobs.size()));
		employees().neededSet(jobs.size());

		activate();
	
	}
	
	@Override
	protected void loadFix() {
		data.loadFix();
	};
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}
	
	@Override
	protected void dispose() {
		data.resourceSet(null, this);
	}
	
	@Override
	protected void updateAction(double ds, boolean day) {

		stationProblem = false;
		go();
		
		if (day) {
			double fetch = employees().fetchBonus();
			employees().fetchBonusConsume((int)fetch);
			fetch *= TIME.secondsPerDayI();
			fetch = CLAMP.d(fetch/employees().employed(), 0, 1);
			if (fetchTime == 0)
				fetchTime = (float) fetch;
			else
				fetchTime = (float) CLAMP.d(4.0*fetchTime/5.0 + fetch/5.0, 0, 1);
		}
		
		
		if (coolFetch > 0) {
			coolFetch--;
		}
		if (day) {
			lastSourceX = -1;
		}
		for (MoveOrderPull o : pullOrders) {
			if (o != null && o.cooldown > 0)
				o.cooldown--;
		}

		jobs.searchAgain();
	}

	@Override
	protected void activateAction() {
	
	}

	@Override
	protected void deactivateAction() {
		
	}

	
	@Override
	public ROOM_TRANSPORT blueprintI() {
		return (ROOM_TRANSPORT) blueprint();
	}
	
	public void reportMoved(int dist) {
		if (distance == 0)
			distance = dist;
		else
			distance = (float) (distance*4/5 + dist/5.0);
	}


	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}
	
	void go() {
		if (!data.canGo())
			return;
		
		if (stationProblem)
			return;
		
		
		COORDINATE c = SETT.ROOMS().STATION.reserve(resource());
		
		if (c == null) {
			stationProblem = true;
			return;
		}
		byte ran = (byte) SETT.tileRan(sx, sy);
		DIR d = DIR.ORTHO.get(SETT.ROOMS().fData.item.get(sx, sy).rotation);
		if (SETT.HALFENTS().transports.loader(sx+d.x(), sy+d.y(), ran, resource(), ROOM_TRANSPORT.MAX_LOAD, d, c)) {
			double w = SETT.ROOMS().STATION.workersPerload(c.x(), c.y());
			if (stationWorkers == 0)
				stationWorkers = (float) w;
			else
				stationWorkers = (float) (stationWorkers*4/5 + w/5.0);
			
			data.go();
			
		}else {
			SETT.ROOMS().STATION.reserveCancel(resource(), c.x(), c.y());
			stationProblem = true;
		}
	}
	

	
	public void finishDeliveryJob(int am) {
		data.deliver(am);
	}

	@Override
	public boolean searching() {
		return true;
	}
	
	@Override
	public TILE_STORAGE storage(int tx, int ty) {
		return blueprintI().job.storage(tx, ty);
	};
	
	private static class Jobs extends JobPositions<TransportInstance> {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Jobs(TransportInstance ins) {
			super(ins);
		}
		
		@Override
		protected boolean isAndInit(int tx, int ty) {
			return ins.blueprintI().job.job(tx, ty) != null;
		}
		
		@Override
		protected SETT_JOB get(int tx, int ty) {
			ins.go();
			return ins.blueprintI().job.job(tx, ty);
		}
	}
	
	@Override
	public TILE_STORAGE destCrate(RBIT okMask, int minAm, int ox, int oy) {


		if (!destSpaceMask().has(okMask))
			return null;
		TILE_STORAGE t = blueprintI().job.storage(ox, oy);
		
		if (t != null && t.storageReservable() == blueprintI().job.bamountr.max()) {
			return t;
		}
		
		int best = -1;
		
		for (int i = 0; i < jobs.size(); i++) {
			
			t = blueprintI().job.storage(jobs.get(i).x(), jobs.get(i).y());
			
			if (t != null && t.storageReservable() > 0) {
				if (t.storageReservable() == blueprintI().job.bamountr.max())
					return t;
				else
					best = i;
			}
		}
		
		if (best != -1) {
			return blueprintI().job.storage(jobs.get(best).x(), jobs.get(best).y());
		}
			
		
		LOG.ln("Weirdness!");
		return null;
		
	}

	@Override
	public RBIT destSpaceMask() {
		if (data.resource() == null)
			return RBIT.NONE;
		
		return data.resource().bit;
	}

	@Override
	public double storedD(RESOURCE res) {
		if (data.resource() == res)
			return (double)data.stored() /(ROOM_TRANSPORT.MAX_LOAD);
		return 1;
	}

	@Override
	public RBIT moveCapacity() {
		if (data.resource() == null)
			return RBIT.NONE;
		return data.resource().bit;
	}

	@Override
	public MoveJob moveJob(Humanoid skill) {
		

		if (coolFetch <= 0 && data.unloadedSpots() > 0) {
			int am = SETT.ROOMS().STOCKPILE.carryCap(skill);
			
			if (fetching || prio) {
				
				
				MoveJob j = MoveJob.fetch(this, this, am, radius(), lastSourceX, lastSourceY, fetching ? destSpaceMask() : RBIT.NONE, prio ? destSpaceMask() : RBIT.NONE);
				if (j != null) {
					lastSourceX = (short) j.source.x();
					lastSourceY = (short) j.source.y();
					coolFetch = -1;
					return j;
				}
				
			}
			
			for (int ooi = 0; ooi < pullOrders.length; ooi++) {
				orderIP++;
				if (orderIP >= pullOrders.length)
					orderIP = 0;
				MoveOrderPull p = pullOrders[orderIP];
				if (p != null && p.cooldown <= 0) {
					MoveJob j = p.job(this, Math.min(am, ROOM_STOCKPILE.MIN_CARRY), am);
					
					if (j != null) {
						p.cooldown = -1;
						return j;
					}
					p.cooldown = 4;
				}
			}
			
			
			coolFetch = 4;
		}
		
		
		
		return null;
	}

	@Override
	public int moveMinAmount() {
		return 1;
	}

	@Override
	public MoveOrderPull[] moveOrdersPull() {
		return pullOrders;
	}

	@Override
	public RBIT moveOrderPullAccepted() {
		return moveCapacity();
	}

	@Override
	public RBIT moveOrderPullAvailable() {
		return data.unloadedSpots() > 0 ? moveCapacity() : RBIT.NONE;
	}
	
	public boolean fetching() {
		return fetching;
	}
	
	public void fetchingSet(boolean f) {
		this.fetching = f;
		coolFetch = 0;
	}
	
	@Override
	public int moveMaxRadius() {
		return 400;
	}
	
	@Override
	public int radius() {
		return (this.radius + 5)*8;
	}
	
	@Override
	public byte radiusRaw() {
		return radius;
	}

	@Override
	public void radiusRawSet(byte r) {
		this.radius = r;
	}

	public RESOURCE resource() {
		return data.resource();
	}
	
	public double efficiency() {
		double d = CLAMP.d((double)employees().employed()/jobs.size(), 0, 1);
		d*= d;
		d += 8.0 / ROOM_TRANSPORT.MAX_LOAD;
		return d*(1-getDegrade());
	}

	@Override
	public void copyFrom(MoveOrderPullInstance same) {
		TransportInstance ins = (TransportInstance) same;
		data.resourceSet(ins.resource(), this);
		fetchingSet(ins.fetching());
		prio = ins.prio;
		coolFetch = 0;
		radius = ins.radius;
		auto = ins.auto;
		employees().neededSet(ins.employees().target());
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

		private boolean fetching;
		private boolean prio;
		private final boolean broken;
		int ri = -1;
		private MoveOrderPull[] orders;
		
		
		public State(TransportInstance ins, boolean broken) {
			super(ins);
			this.broken = broken;
			this.fetching = ins.fetching;
			this.prio = ins.prio;
			if (broken) {
				this.orders = ins.pullOrders;
				
			}
			ri = ins.resource() == null ? -1 : ins.resource().index();
		}
		
		@Override
		public void applyIns(RoomInstance ins) {
			if (ins instanceof TransportInstance) {
				TransportInstance s = (TransportInstance) ins;
				if (ri >= 0)
					s.data.resourceSet(RESOURCES.ALL().getC(ri), s);
				
				if (broken) {
					for (int i = 0; i < orders.length; i++) {
						if (orders[i] != null) {
							MoveOrderPull p = new MoveOrderPull(orders[i].destCoo(), orders[i].resbits);
							s.pullOrders[i] = p;
						}
						
					}
					
				}
				s.fetchingSet(fetching);
				if (prio != s.prio)
					prio = s.prio;
			}
			
		}
		
		
	}
	
}