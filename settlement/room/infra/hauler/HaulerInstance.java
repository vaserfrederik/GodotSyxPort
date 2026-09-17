package settlement.room.infra.hauler;

import static settlement.main.SETT.ROOMS;

import init.resources.RBIT;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.misc.util.RESOURCE_TILE;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.infra.logistics.MoveJob;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVEJOBBER;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVE_DEST;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVE_SOURCE;
import settlement.room.infra.logistics.MoveOrderPull;
import settlement.room.infra.logistics.MoveOrderPull.MoveOrderPullInstance;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.ROOM_RADIUS.ROOM_RADIUS_INSTANCE;
import settlement.room.main.job.StorageCrate;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomState;
import settlement.room.main.util.RoomState.RoomStateInstance;
import snake2d.LOG;
import snake2d.util.datatypes.COORDINATE;

final class HaulerInstance extends RoomInstance implements ROOM_RADIUS_INSTANCE, ROOM_MOVE_SOURCE, ROOM_MOVE_DEST, ROOM_MOVEJOBBER, MoveOrderPullInstance{

	private static final long serialVersionUID = 1L;
	public final static int ORDERS = 2;
	
	private boolean hasSpace;
	private byte resourceI = -1;
	byte coolFetch = 0;
	byte coolOrganize = 0;
	private short ox,oy;
	final MoveOrderPull[] orders = new MoveOrderPull[ORDERS];
	private boolean storing;
	private boolean prio;
	private boolean fetching = true;
	boolean auto = true;
	final StorageCrate.StorageData[] sdata;
	int[] tdata;
	private byte orderI = 0;
	byte radius = 100;
	
	HaulerInstance(ROOM_HAULER blueprint, TmpArea area, RoomInit init) {
		super(blueprint, area, init);
		ROOMS().data.set(this, mX(), mY(), 0);
		sdata = blueprint.crate.make(this);
		
		employees().maxSet(Math.max(body().width(), body().height())*5);
		employees().neededSet(1);
		activate();
		blueprint.tally.init(this);
	}

	@Override
	protected void loadFix() {
		if (resourceI >= 0) {
			RESOURCE nr = RESOURCES.map().loader().get(resourceI);
			if (nr == null) {
				resourceI = -1;
			}
		}
		
	}
	
	void updateMasks() {
		hasSpace = t().space.get(this)-t().spaceReserved.get(this)-t().amount.get(this) > 0;
	}
	
	private void reset() {
		coolFetch = 0;
		coolOrganize = 0;
		for (MoveOrderPull o : orders) {
			if (o != null)
				o.cooldown = 0;
		}
	}

	private final HaulerTally t() {
		return blueprintI().tally;
	}

	public void setResource(RESOURCE res) {
		
		if (resource() != null) {
			for (COORDINATE c : body()) {
				if (is(c) && blueprintI().crate.get(c.x(), c.y(), this, sdata) != null) {
					blueprintI().crate.clear();
				}
			}
		}
		resourceI = res == null ? -1 : res.bIndex();
		if (res != null) {
			for (COORDINATE c : body()) {
				if (is(c) && blueprintI().crate.get(c.x(), c.y(), this, sdata) != null) {
					blueprintI().crate.resourceSet(res);
				}
			}
			
		}
		for (MoveOrderPull o : orders) {
			if (o != null)
				o.resbits.clearSet(resource() == null ? RBIT.NONE : resource().bit);
		}
		reset();
	}
	
	@Override
	protected void updateAction(double ds, boolean day) {

		if (!active() || employees().employed() <= 0)
			return;
		
		if (coolFetch > 0) {
			coolFetch--;
		}
		if (coolOrganize > 0) {
			coolOrganize--;
		}
		
		for (MoveOrderPull o : orders) {
			if (o != null && o.cooldown > 0)
				o.cooldown--;
		}
	}
	
	@Override
	protected void dispose() {
		for (COORDINATE c : body()) {
			if (is(c) && blueprintI().crate.get(c.x(), c.y(), this, sdata) != null) {
				blueprintI().crate.dispose();
			}
		}
	}
	
	@Override
	protected void activateAction() {
		// TODO Auto-generated method stub

	}

	@Override
	protected void deactivateAction() {
		// TODO Auto-generated method stub

	}

	public double getUsedSpace() {
		if (resource() == null)
			return 0;
		double d = t().amount.get(this);
		double c = t().space.get(this);
		if (c == 0)
			return 0;
		return d/c;
	}
	
	public RESOURCE resource() {
		if (resourceI == -1)
			return null;
		return RESOURCES.ALL().get(resourceI);
	}

	@Override
	public boolean searching() {
		return true;
	}
	
	@Override
	public double storedD(RESOURCE res) {
		if (res != resource())
			return 0;
		double s =  t().space.get(this);
		if (s == 0)
			return 1;
		return (t().amount.get(this)-t().amountReserved.get(this))/ s;
	}

	@Override
	public RBIT moveCapacity() {
		if (resource() == null)
			return RBIT.NONE;
		return resource().bit;
	}
	
	@Override
	public int moveCapacityAm(RESOURCE res) {
		if (resource() == null)
			return 0;
		return t().space.get(this);
	}
	
	@Override
	public ROOM_HAULER blueprintI() {
		return ROOMS().HAULER;
	}

	@Override
	public boolean destroyTileCan(int tx, int ty) {
		return true;
	}

	@Override
	public ROOM_DEGRADER degrader(int tx, int ty) {
		return null;
	}
	
	@Override
	public RBIT destSpaceMask() {
		if (resource() == null || !hasSpace)
			return RBIT.NONE;
		
		return resource().bit;
	}

	@Override
	public RBIT sourceAmountMask() {
		if (resource() == null)
			return RBIT.NONE;
		return resource().bit;
	}
	
	@Override
	public RESOURCE_TILE sourceCrate(RBIT okMask, int minAmount, int ox, int oy, double limit) {

		
		
		if (resource() == null || sourceAmountMask().isClear())
			return null;
		
		if (!okMask.has(resource()))
			return null;
		
		double st = t().space.get(this);
		double am = t().amount.get(this)-t().amountReserved.get(this)-minAmount;
		if (am <= 0 || limit > am/st) {
			return null;
		}
		
		if (is(ox, oy)){
			RESOURCE_TILE s = blueprintI().crate.get(ox, oy, this, this.sdata);
			if (s != null && s.reservable() >= minAmount)
				return s;
		}
		for (COORDINATE c : body()) {
			StorageCrate s = blueprintI().crate.get(c.x(), c.y(), this, sdata);
			if (s != null && s.reservable() >= minAmount)
				return s;
		}
		
		if (minAmount == 1)
			LOG.ln("Weird indeed");
		
		
		return null;
	}
	
	@Override
	public TILE_STORAGE destCrate(RBIT okMask, int minAmount, int ox, int oy) {
		
		if (resource() == null || destSpaceMask().isClear())
			return null;
		
		if (!okMask.has(resource()))
			return null;
		
		if (is(ox, oy)){
			TILE_STORAGE s = blueprintI().crate.get(ox, oy, this, this.sdata);
			if (s != null && s.resource() != null && okMask.has(s.resource()) && s.storageReservable() >= minAmount)
				return s;
		}
		
		int bestX = -1;
		int bestY = -1;
		int bestV = 0;
		
		for (COORDINATE c : body()) {
			StorageCrate s = blueprintI().crate.get(c.x(), c.y(), this, sdata);
			if (s == null)
				continue;
			int v = 0;
			if (s.storageReservable() >= minAmount) {
				v = 10000-s.storageReservable();
			}if (v > bestV) {
				bestX = s.x();
				bestY = s.y();
				bestV = v;
			}
		}
		
		
		if (bestX == -1 && minAmount == 1) {
			LOG.ln("Weird");
			return null;
		}
		
		return blueprintI().crate.get(bestX, bestY, this, this.sdata);
	}
	
	@Override
	public MoveJob moveJob(Humanoid skill) {
		
		if (resource() == null)
			return null;
		
		int am = SETT.ROOMS().STOCKPILE.carryCap(skill);
		
		if ((fetching ||prio) && coolFetch <= 0){

			RBIT bb = resource().bit;

			MoveJob j = MoveJob.fetch(this, this, am, radius(), ox, oy, fetching ? bb : RBIT.NONE, prio ? bb : RBIT.NONE);
			
			if (j != null) {
				ox = (short) j.source.x();
				oy = (short) j.source.y();
				return j;
			}
			
			coolFetch = 2;
		}
		
		for (int ooi = 0; ooi < orders.length; ooi++) {
			orderI++;
			if (orderI >= orders.length)
				orderI = 0;
			MoveOrderPull p = orders[orderI];
			if (p != null && p.cooldown <= 0) {
				MoveJob j = p.job(this, 1, am);
				
				if (j != null) {
					p.cooldown = -1;
					return j;
				}
				p.cooldown = 2;
			}
		}
		

		
		if (coolOrganize <= 0) {
			
			int sx = -1;
			int sy = -1;
			int amm = 0;
			int maxS = 0;
			
			for (COORDINATE c : body()) {
				StorageCrate s = blueprintI().crate.get(c.x(), c.y(), this, sdata);
				
				if (s != null && s.storageReservable() > 0 && s.amount() > amm) {
					sx = s.x();
					sy = s.y();
					maxS = s.storageReservable();
					amm = s.amount();
				}
			}
			
			if (sx != -1) {
				for (COORDINATE c : body()) {
					RESOURCE_TILE s = blueprintI().crate.get(c.x(), c.y(), this, sdata);
					if (s != null && s.reservable() > 0 && s.amount() < amm) {
						if (s.x() == sx && s.y() == sy)
							continue;
						MoveJob j = MoveJob.TMP;
						am = Math.min(am, s.reservable());
						am = Math.min(am, maxS);
						j.maxAm = am;
						j.res = s.resource();
						j.stored = true;
						j.prio = prio;
						j.source.set(s);
						j.dest.set(sx, sy);
						return j;
					}
				}
			}
			
			

			coolOrganize = 4;
		}
		
		
		return null;
	}
	
	boolean storing() {
		return storing;
	}
	
	void storingSet(boolean s) {
		if (s == storing)
			return;
		for (COORDINATE c : body()) {
			if (is(c) && blueprintI().crate.get(c.x(), c.y(), this, sdata) != null) {
				blueprintI().crate.remove();
			}
		}
		storing = s;
		for (COORDINATE c : body()) {
			if (is(c) && blueprintI().crate.get(c.x(), c.y(), this, sdata) != null) {
				blueprintI().crate.add();
			}
		}
	}
	
	boolean prio() {
		return prio;
	}
	
	void prioSet() {
		for (COORDINATE c : body()) {
			if (is(c) && blueprintI().crate.get(c.x(), c.y(), this, sdata) != null) {
				blueprintI().crate.remove();
			}
		}
		prio = !prio;
		for (COORDINATE c : body()) {
			if (is(c) && blueprintI().crate.get(c.x(), c.y(), this, sdata) != null) {
				blueprintI().crate.add();
			}
		}
	}
	
	boolean fetching() {
		return fetching;
	}
	
	void fetchingSet(boolean s) {
		if (s == fetching)
			return;
		fetching = s;
		reset();
	}
	
	@Override
	public StorageCrate storage(int tx, int ty) {
		return blueprintI().crate.get(tx, ty, this, sdata);
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return blueprintI().crate.get(tx, ty, this, sdata);
	}

	@Override
	public MoveOrderPull[] moveOrdersPull() {
		return orders;
	}

	@Override
	public RBIT moveOrderPullAccepted() {
		if (resource() == null)
			return RBIT.NONE;
		
		return resource().bit;
	}

	@Override
	public RBIT moveOrderPullAvailable() {
		if (resource() == null)
			return RBIT.NONE;
		return destCrate(destSpaceMask(), moveMinAmount(), -1, -1) != null ? destSpaceMask() : RBIT.NONE;
	}

	@Override
	public int moveMinAmount() {
		return 8;
	}

	@Override
	public int moveMaxRadius() {
		return RADIUS*3;
	}
	
	public static final int RADIUS = 80;
	

	
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
	@Override
	public RoomState makeState(int rx, int ry, boolean broken) {
		return new State(this, broken);
	}
	
	private static class State extends RoomStateInstance {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		private final short ri;
		private final boolean broken;
		private MoveOrderPull[] orders;
		private boolean fetching;
		private boolean storing;
		
		public State(HaulerInstance ins, boolean broken) {
			super(ins);
			ri = ins.resourceI;
			
			this.broken = broken;
			if (broken) {
				this.orders = broken ? ins.orders : null;
				this.fetching = ins.fetching;
				this.storing = ins.storing;
			}
			
		}
		
		@Override
		public void applyIns(RoomInstance ins) {
			if (ins instanceof HaulerInstance) {

				HaulerInstance s = (HaulerInstance) ins;
				if (ri != -1)
					s.setResource(RESOURCES.ALL().getC(ri));
				s.fetchingSet(fetching);
				s.storingSet(storing);
				if (broken) {
					if (orders != null)
						for (int i = 0; i < orders.length; i++) {
							if (orders[i] != null) {
								MoveOrderPull p = new MoveOrderPull(orders[i].destCoo(), orders[i].resbits);
								s.orders[i] = p;
							}
							
						}
				}
				
				
			}
			
		}
		
		
	}

	@Override
	public void copyFrom(MoveOrderPullInstance same) {
		HaulerInstance ins = (HaulerInstance) same;
		setResource(ins.resource());
		fetchingSet(ins.fetching());
		if (prio != ins.prio)
			prioSet();
		auto = ins.auto;
		employees().neededSet(ins.employees().target());
	}
}
