package settlement.room.infra.stockpile;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
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
import settlement.thing.ThingsResources.ScatteredResource;
import snake2d.LOG;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.ArrayCooShort;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

public final class StockpileInstance extends RoomInstance implements ROOM_RADIUS_INSTANCE, ROOM_MOVE_SOURCE, ROOM_MOVE_DEST, ROOM_MOVEJOBBER, MoveOrderPullInstance {

	final static int ORDERS = 4;
	private final static long serialVersionUID = -7063521835843676015l;
	private final RBITImp tmp = new RBITImp();
	int[][] tdata;
	
	final ArrayCooShort crates;
	final StorageCrate.StorageData[] sdata;
	int[] resCrates = Alloc.ii(RESOURCES.ALL().size());

	RBITImp fetchMask = new RBITImp();
	RBITImp fetchMaskBig = new RBITImp();
	RBITImp reservableMask = new RBITImp();
	public RBITImp crateMask = new RBITImp();
	byte coolFetch = -1;
	byte coolOrganize = -1;
	boolean hasTriedBig = false;
	private short ox,oy;
	boolean autoE;
	final MoveOrderPull[] orders = new MoveOrderPull[ORDERS];
	private boolean storing;
	private boolean fetching = true;
	private boolean prio = false;
	private byte orderI = 0;
	
	private byte[] limits = Alloc.bb(RESOURCES.ALL().size());
	byte radius = 100;
	
	StockpileInstance(ROOM_STOCKPILE p, TmpArea area, RoomInit init) {
		super(p, area, init);

		tdata = Alloc.i2(p.tally().datas.size(), RESOURCES.ALL().size()+1); 
		sdata = p.crate.make(this);
		
		int crateI = 0;
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			StorageCrate cr = p.crate.get(c.x(), c.y(), this, sdata);
			if (cr != null) {
				crateI++;
			}
		}
		
		crates = new ArrayCooShort(crateI);
	
		crateI = 0;
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			if (p.crate.get(c.x(), c.y(), this, sdata) != null) {
				crates.set(crateI++).set(c.x(), c.y());
			}
		}
		
		crates.shuffle(crates.size());
		
		crates.set(0);
		
		while(crates.hasNext()) {
			ScatteredResource s = SETT.THINGS().resources.tGet.get(crates.get());
			StorageCrate c = p.crate.get(crates.get().x(), crates.get().y(), this, sdata);
			
			if (s != null) {
				
				c.resourceSet(s.resource());
				int am = CLAMP.i(s.amount(), 0, crateSize(s.resource()) - c.amount());
				c.amountSet(c.amount() + am);
				while(am-- > 0) {
					if (!s.findableReservedIs())
						s.findableReserve();
					s.resourcePickup();
				}
			}
			crates.next();
		}
		crates.set(0);
		
		employees().maxSet(crates.size()*8);
		activate();
		

	}
	
	@Override
	protected void loadFix() {
		if (limits == null)
			limits = Alloc.bb(RESOURCES.ALL().size());
		
		RESOURCES.map().loader().fix(limits, (byte)0);
		super.loadFix();
	}
	
	@Override
	protected boolean loadExtra(FileGetter file) throws IOException {
		if (resCrates == null || resCrates.length != RESOURCES.ALL().size())
			resCrates = Alloc.ii(RESOURCES.ALL().size());
		return super.loadExtra(file);
	}
	
	void updateMasks() {
		for (RESOURCE r : RESOURCES.ALL()) {
			updateMasks(r);
		}
	}
	
	void updateMasks(RESOURCE r) {
		crateMask.clear(r);
		fetchMask.clear(r);
		fetchMaskBig.clear(r);
		reservableMask.clear(r);
		if (t().crates.get(r, this) > 0) {
			crateMask.or(r);
		}
		
		int am = t().space.get(r, this);
		am -= t().amount.get(r, this)+t().spaceReserved.get(r, this);
		if (am > 0) {
			fetchMask.or(r);
			if (am > t().space.get(r, this)/2) {
				fetchMaskBig.or(r);
			}
		}
		if (t().amount.get(r, this)-t().amountReserved.get(r, this) > 0)
			reservableMask.or(r.bit);
		
	}
	
	private void reset() {
		coolFetch = -1;
		coolOrganize = -1;
		hasTriedBig = false;
		for (MoveOrderPull o : orders) {
			if (o != null)
				o.cooldown = 0;
		}
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		super.render(r, shadowBatch, it);
		it.lit();
		return false;
	}
	
	
	private final StockpileTally t() {
		return blueprintI().tally();
	}

	void allocateCrate(RESOURCE res, int amount){
		setSpecialAmount(res, 0);
		pallocateCrate(res, amount);
		reset();
	}
	
	private void pallocateCrate(RESOURCE res, int amount){
		
		while(amount < t().crates.get(res.bIndex(), this)) {
			
			int best = -1;
			int smallest = Integer.MAX_VALUE;
			for (int i = 0; i < crates.size(); i++) {
				StorageCrate crate = blueprintI().crate.get(crates.get().x(), crates.get().y(), this, sdata);
				int ci = crates.getI();
				crates.inc();
				if (crate.resource() != res)
					continue;
				
				if (crate.amount() < smallest) {
					best = ci;
					smallest = crate.amount();
				}
			}
			
			if (best == -1)
				break;
			
			crates.set(best);
			StorageCrate crate = blueprintI().crate.get(crates.get().x(), crates.get().y(), this, sdata);
			crate.clear();
			
		}
		
		if(amount > t().crates.get(res, this)) {
			for (int i = 0; i < crates.size(); i++) {
				StorageCrate crate = blueprintI().crate.get(crates.get().x(), crates.get().y(), this, sdata);
				crates.inc();
				if (crate.resource() != null)
					continue;
				crate.resourceSet(res);
				
				if (t().crates.get(res, this) == amount)
					break;
				
			}
		}
		reset();
	}
	
	void setSpecialAmount(RESOURCE res, int amount) {
		amount = CLAMP.i(amount, 0, crateSize());
		if (amount == limits[res.index()])
			return;
		
		amount = CLAMP.i(amount, 0, crateSize());
		
		if (amount < 0 || amount > crateSize())
			throw new RuntimeException(res + " " + amount + " " + crateSize());
		
		pallocateCrate(res, 1);
		if (t().crates.get(res, this) != 1)
			return;
		
		for (int i = 0; i < crates.size(); i++) {
			StorageCrate crate = blueprintI().crate.get(crates.get().x(), crates.get().y(), this, sdata);
			crates.inc();
			if (crate.resource() != res)
				continue;
			crate.reservedSet(crate.reservable()+crate.reserved());
			
			int am = 0;
			int max = (int) (amount == 0 ? (blueprintI().upgrades().boost(upgrade())-1) : amount); 
			while(crate.reserved() > 0 && am < max) {
				crate.resourcePickup();
				am++;
			}
			crate.clear();
			limits[res.index()] = (byte) amount;
			crate.resourceSet(res);
			crate.storageReserve(am);
			crate.storageDeposit(am);
			return;
		}
		
		LOG.ln("weird!");
	}

	public int getSpecialAmount(RESOURCE res) {
		return limits[res.index()];
	}
	
	public int totalCrates() {
		return crates.size();
	}


	@Override
	public void upgradeSet(int upgrade) {
		
		if (upgrade < upgrade()) {
			int max = (int) (blueprintI().upgrades().boost(upgrade)-1);
			for (int i = 0; i < crates.size(); i++) {
				StorageCrate c = blueprintI().crate.get(crates.get().x(), crates.get().y(), this, sdata);
				if (c.resource() != null) {
					int tooMuch = c.amount() - max;
					if (c.reserved() > max) {
						c.reservedSet(max);
					}
					int newAm = Math.min(max, c.amount());
					if (newAm + c.storageReserved() > max) {
						c.storageUnreserve(newAm + c.storageReserved() - max);
					}
					
					if (tooMuch > 0) {
						c.amountSet(max);
						SETT.THINGS().resources.create(c, c.resource(), tooMuch);
					}
				}
				
				crates.inc();
			}
		}
		
		for (int i = 0; i < crates.size(); i++) {
			blueprintI().crate.get(crates.get().x(), crates.get().y(), this, sdata).remove();
			crates.inc();
		}
		super.upgradeSet(upgrade);
		for (int i = 0; i < crates.size(); i++) {
			blueprintI().crate.get(crates.get().x(), crates.get().y(), this, sdata).add();
			crates.inc();
		}
	}
	

	@Override
	protected void updateAction(double ds, boolean day) {

		if (!active() || employees().employed() <= 0)
			return;
		
		hasTriedBig = false;
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
		
		for (int i = 0; i < crates.size(); i++) {
			StorageCrate crate = blueprintI().crate.get(crates.get().x(), crates.get().y(), this, sdata);
			crate.dispose();
			crates.inc();
		}
	}
	
	@Override
	public ROOM_STOCKPILE blueprintI() {
		return ROOMS().STOCKPILE;
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return blueprintI().crate.get(tx, ty, this, sdata);
	}
	
	@Override
	public TILE_STORAGE storage(int tx, int ty) {
		return blueprintI().crate.get(tx, ty, this, sdata);
	}
	
	public StorageCrate crate(int tx, int ty) {
		return blueprintI().crate.get(tx, ty, this, sdata);
	}
	
	public double getUsedSpace() {
		double d = t().amount.get(null, this);
		double c = t().space.get(null, this);
		if (c == 0)
			return 0;
		return d/c;
	}

	@Override
	protected void activateAction() {
		// TODO Auto-generated method stub
		
	}

	@Override
	protected void deactivateAction() {
		// TODO Auto-generated method stub
		
	}



	@Override
	public boolean searching() {
		return true;
	}
	

	
	public int crateSize() {
		return (int) (blueprintI().upgrades().boost(upgrade())-1);
	}
	
	public int crateSize(RESOURCE res) {
		if (res == null || limits[res.index()] == 0)
			return (int) (blueprintI().upgrades().boost(upgrade())-1);
		return limits[res.index()];
	}

	@Override
	public double storedD(RESOURCE res) {
		double s =  t().space.get(res, this);
		if (s == 0)
			return 1;
		return (t().amount.get(res, this)-t().amountReserved.get(res, this))/ s;
	}

	@Override
	public RBIT moveCapacity() {
		return crateMask;
	}
	
	@Override
	public int moveCapacityAm(RESOURCE res) {
		return t().space.get(res, this);
	}
	
	@Override
	public RESOURCE_TILE sourceCrate(RBIT okMask, int minAm, int ox, int oy, double limit) {

		tmp.clearSet(okMask);
		tmp.and(reservableMask);
		
		if (tmp.isClear())
			return null;
		
		for (RESOURCE r : RESOURCES.ALL()) {
			if (tmp.has(r)) {
				double st = t().space.get(r, this);
				double am = t().amount.get(r, this)-t().amountReserved.get(r, this)-minAm;
				if (am <= 0 || limit > am/st) {
					tmp.clear(r);
				}
			}
		}
		
		if (tmp.isClear())
			return null;
		
		if (is(ox, oy)){
			RESOURCE_TILE s = blueprintI().crate.get(ox, oy, this, this.sdata);
			if (s != null && s.resource() != null && tmp.has(s.resource()) && s.reservable() >= minAm) {
				return s;
			}
		}
		

		for (int i = 0; i < crates.size(); i++) {
			crates.inc();
			RESOURCE_TILE s = blueprintI().crate.get(crates.get().x(), crates.get().y(), this, this.sdata);
			if (s.resource() != null && tmp.has(s.resource()) && s.reservable() >= minAm) {
				return s;
			}
		}
		
		if (minAm == 1)
			LOG.ln("Weird indeed");

		return null;
	}
	
	@Override
	public TILE_STORAGE fetchToCrate(RESOURCE res, int desiredAm) {
	
		int ri = res.index();

		if (resCrates[ri] >= 0 && resCrates[ri] < crates.size()) {
			int cx = crates.x(resCrates[ri]);
			int cy = crates.y(resCrates[ri]);
			
			if (is(cx, cy)){
				TILE_STORAGE s = blueprintI().crate.get(cx, cy, this, this.sdata);
				if (s != null && s.resource() == res && s.storageReservable() > 0) {
					if (s.storageReservable() < desiredAm)
						resCrates[ri]++;
					return s;
				}
			}
		}

		int best = 0;
		int backup = -1;
		
		for (int i = 0; i < crates.size(); i++) {
			
			resCrates[ri]++;
			if (resCrates[ri] >= crates.size())
				resCrates[ri] = 0;
			
			
			TILE_STORAGE s = blueprintI().crate.get(crates.x(resCrates[ri]), crates.y(resCrates[ri]), this, this.sdata);
			if (s.resource() == res && s.storageReservable() >= 0) {
				if (s.storageReservable() >= desiredAm) {
					return s;
				}else if (s.storageReservable() > best) {
					best = s.storageReservable();
					backup = resCrates[ri];
				}
					
				
			}
		}
		
		if (backup == -1)
			throw new RuntimeException("nay");
		
		resCrates[ri] = backup;
		return blueprintI().crate.get(crates.x(backup), crates.y(backup), this, this.sdata);
	}
	
	@Override
	public TILE_STORAGE destCrate(RBIT okMask, int minAmount, int ox, int oy) {
		
		tmp.clearSet(okMask);
		tmp.and(minAmount > 64 ? fetchMaskBig : fetchMask);
		if (tmp.isClear())
			return null;
		
		if (is(ox, oy)){
			TILE_STORAGE s = blueprintI().crate.get(ox, oy, this, this.sdata);
			if (s != null && s.resource() != null && okMask.has(s.resource()) && s.storageReservable() >= minAmount)
				return s;
		}
		
		for (int i = 0; i < crates.size(); i++) {
			crates.inc();
			TILE_STORAGE s = blueprintI().crate.get(crates.get().x(), crates.get().y(), this, this.sdata);
			if (s.resource() != null && okMask.has(s.resource()) && s.storageReservable() >= minAmount) {
				return s;
			}
		}
		
		if (minAmount > 1)
			return null;
		
		debugFuck();
		throw new RuntimeException();
	}
	
	private void debugFuck() {
		int[] st = Alloc.ii(RESOURCES.ALL().size());
		int[] rr = Alloc.ii(RESOURCES.ALL().size());
		for (int i = 0; i < crates.size(); i++) {
			crates.inc();
			StorageCrate s = blueprintI().crate.get(crates.get().x(), crates.get().y(), this, this.sdata);
			if (s.resource() != null) {
				st[s.resource().index()] += s.storageReservable();
				rr[s.resource().index()] += s.reservable();
			}
		}
		
		for (RESOURCE r : RESOURCES.ALL()) {
			int am = t().space.get(r, this);
			am -= t().amount.get(r, this)+t().spaceReserved.get(r, this);
			LOG.ln(r + " " + fetchMask.has(r) + " " + am + " " + st[r.index()]);
			LOG.ln((t().amount.get(r, this)-t().amountReserved.get(r, this)) + " " + rr[r.index()]);
		}
		
	}
	
	@Override
	public MoveJob moveJob(Humanoid skill) {
		
		int am = SETT.ROOMS().STOCKPILE.carryCap(skill);
		
		if ((fetching || prio) && coolFetch <= 0){
			
			MoveJob j = null;
			
			if (!hasTriedBig)
				j = MoveJob.fetch(this, this, am, radius(), ox, oy, fetching ? fetchMaskBig: RBIT.NONE, prio ? fetchMaskBig : RBIT.NONE);
			if (j == null) {
				j = MoveJob.fetch(this, this, am, radius(), ox, oy, fetching ? fetchMask: RBIT.NONE, prio ? fetchMask : RBIT.NONE);
				hasTriedBig = true;
			}
			
			if (j != null) {
				ox = (short) j.source.x();
				oy = (short) j.source.y();
				coolFetch = -1;
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
				p.cooldown = 4;
			}
		}


		
		if (coolOrganize <= 0) {
			
			MoveJob j = blueprintI().org.organise(this, am);
			if (j != null) {
				coolOrganize = -1;
				return j;
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
		for (int i = 0; i < crates.size(); i++) {
			blueprintI().crate.get(crates.get().x(), crates.get().y(), this, sdata).remove();
			crates.inc();
		}
		storing = s;
		for (int i = 0; i < crates.size(); i++) {
			blueprintI().crate.get(crates.get().x(), crates.get().y(), this, sdata).add();
			crates.inc();
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
	
	public void prioritizeToggle() {
		for (int i = 0; i < crates.size(); i++) {
			blueprintI().crate.get(crates.get().x(), crates.get().y(), this, sdata).remove();
			crates.inc();
		}
		prio = !prio;
		for (int i = 0; i < crates.size(); i++) {
			blueprintI().crate.get(crates.get().x(), crates.get().y(), this, sdata).add();
			crates.inc();
		}
		reset();
	}

	public boolean prioritizing() {
		return prio;
	}
	




	@Override
	public RBIT destSpaceMask() {
		return fetchMask;
	}

	@Override
	public RBIT sourceAmountMask() {
		return reservableMask;
	}

	@Override
	public MoveOrderPull[] moveOrdersPull() {
		return orders;
	}

	@Override
	public RBIT moveOrderPullAccepted() {
		return crateMask;
	}

	@Override
	public RBIT moveOrderPullAvailable() {
		tmp.clear();
		for (int i = 0; i < crates.size(); i++) {
			crates.inc();
			TILE_STORAGE s = blueprintI().crate.get(crates.get().x(), crates.get().y(), this, this.sdata);
			if (s.resource() != null && s.storageReservable() >= moveMinAmount()) {
				tmp.or(s.resource());
			}
		}
		return tmp;
	}

	@Override
	public int moveMinAmount() {
		return 8;
	}

	@Override
	public int moveMaxRadius() {
		return RADIUS*2;
	}
	
	public static final int RADIUS = 140;
	
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
	public RoomState makeState(int tx, int ty, boolean broken) {
		return new State(this, broken);
	}
	
	private static class State extends RoomStateInstance {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		private final short[] crates = new short[RESOURCES.ALL().size()];

		private final byte[] limits = Alloc.bb(RESOURCES.ALL().size());
		private boolean fetching;
		private boolean storing;
		private boolean prio;
		private final boolean broken;
		private MoveOrderPull[] orders;
		
		
		public State(StockpileInstance ins, boolean broken) {
			super(ins);
			for (RESOURCE r : RESOURCES.ALL()) {
				crates[r.index()] = (short) SETT.ROOMS().STOCKPILE.tally().crates.get(r, ins);
			}
			for (int i = 0; i < limits.length; i++) {
				limits[i] = ins.limits[i];
			}
			this.broken = broken;
			this.fetching = ins.fetching;
			this.storing = ins.storing;
			this.prio = ins.prio;
			if (broken) {
				this.orders = ins.orders;
				
			}
			
		}
		
		@Override
		public void applyIns(RoomInstance ins) {
			if (ins instanceof StockpileInstance) {
				StockpileInstance s = (StockpileInstance) ins;
				for (int ri = 0; ri < RESOURCES.ALL().size() && ri < crates.length; ri++) {
					s.allocateCrate(RESOURCES.ALL().get(ri), crates[ri]);
				}
				for (int ri = 0; ri < RESOURCES.ALL().size() && ri < crates.length; ri++) {
					s.setSpecialAmount(RESOURCES.ALL().get(ri), limits[ri]);
				}
				if (broken) {
					for (int i = 0; i < orders.length; i++) {
						if (orders[i] != null) {
							MoveOrderPull p = new MoveOrderPull(orders[i].destCoo(), orders[i].resbits);
							s.orders[i] = p;
						}
						
					}
					
				}
				s.fetchingSet(fetching);
				s.storingSet(storing);
				if (prio != s.prio)
					s.prioritizeToggle();
			}
			
		}
		
		
	}

	@Override
	public void copyFrom(MoveOrderPullInstance same) {
		StockpileInstance ins = (StockpileInstance) same;
		for (RESOURCE r : RESOURCES.ALL()) {
			int cr = (int) Math.ceil((double)crates.size()*blueprintI().tally().crates.get(r.index(), ins)/ins.crates.size());
			allocateCrate(r, cr);
			if (ins.limits[r.index()] > 0)
				setSpecialAmount(r, ins.limits[r.index()]);
		}
		employees().neededSet(ins.employees().target());
		fetchingSet(ins.fetching());
		autoE = ins.autoE;
		if (prioritizing() != ins.prioritizing())
			prioritizeToggle();
	}
}