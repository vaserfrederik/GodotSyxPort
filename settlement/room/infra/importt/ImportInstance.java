package settlement.room.infra.importt;

import static settlement.main.SETT.ROOMS;

import init.resources.RBIT;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.misc.util.RESOURCE_TILE;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVE_SOURCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.StorageCrate;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomState;
import settlement.room.main.util.RoomState.RoomStateInstance;
import snake2d.LOG;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

public final class ImportInstance extends RoomInstance implements ROOM_MOVE_SOURCE{

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	static final int crateMax = 600;
	private int amount;
	private int spaceReserved;
	private int amountReserved;
	private byte resource = -1;
	
	final StorageCrate.StorageData[] sdata;

	ImportInstance(ROOM_IMPORT p, TmpArea area, RoomInit init) {
		super(p, area, init);
		sdata = p.crate.make(this);
		activate();
		
	}

	@Override
	protected void loadFix() {
		
		if (resource != -1 && RESOURCES.map().loader().get(resource) == null) {
			for (COORDINATE c : body()) {
				if (is(c) && blueprintI().crate.get(c.x(), c.y(), this, sdata) != null) {
					blueprintI().crate.disposeSilent();
				}
			}
		}
		
	};
	
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}

	public int amount() {
		return amount;
	}
	
	public int amoutReserved() {
		return amountReserved;
	}
	
	public int spaceReserved() {
		return spaceReserved;
	}
	
	public int capacity() {
		return sdata.length*crateMax;
	}
	
	void count(Crate c, int delta) {
		
		amount += delta*c.amount();
		amountReserved += delta*c.reserved();
		spaceReserved += delta*c.reservedSpace();
		blueprintI().tally.count(c.resource(), delta*c.amount(), delta*c.max(this));
		
		
	}

	
	@Override
	protected void dispose() {
		for (COORDINATE c : body()) {
			if (is(c) && blueprintI().crate.get(c.x(), c.y(), this, sdata) != null) {
				blueprintI().crate.dispose();
			}
		}
		amount = 0;
		spaceReserved = 0;
		amountReserved = 0;
		resource = -1;
		
	}

	@Override
	protected void activateAction() {
	
	}

	@Override
	protected void deactivateAction() {
		
	}

	@Override
	public ROOM_IMPORT blueprintI() {
		return ROOMS().IMPORT;
	}
	
	
	void allocate(RESOURCE res){
		if (res == resource())
			return;
		dispose();
		
		resource = res == null ? -1 : res.bIndex();
		if (res != null) {
			for (COORDINATE c : body()) {
				if (blueprintI().crate.get(c.x(), c.y(), this, sdata) != null) {
					blueprintI().crate.resourceSet(res);
				}
			}
		}
		
	}

	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return blueprintI().crate.get(tx, ty, this, sdata);
	}
	
	@Override
	public TILE_STORAGE storage(int tx, int ty) {
		return blueprintI().crate.get(tx, ty, this, sdata);
	}
	
	
	public RESOURCE resource() {
		if (resource < 0 || resource >= RESOURCES.ALL().size())
			return null;
		return RESOURCES.ALL().get(resource);
	}

	@Override
	public RoomState makeState(int tx, int ty, boolean broken) {
		return new State(this);
	}
	
	private static class State extends RoomStateInstance {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		private final int ri;
		
		public State(ImportInstance ins) {
			super(ins);
			this.ri = ins.resource;
		}
		
		@Override
		protected void applyIns(RoomInstance ins) {
			super.applyIns(ins);
			if (ri != -1)
				((ImportInstance)ins).allocate(RESOURCES.ALL().get(ri));
		}
		
		
	}

	@Override
	public RESOURCE_TILE sourceCrate(RBIT okMask, int minAmount, int ox, int oy, double lim) {

		if (resource() == null || sourceAmountMask().isClear())
			return null;
		
		if (!okMask.has(resource()))
			return null;
		
		if (lim > storedD(resource()))
			return null;

		double am = amount() - amoutReserved()-minAmount;
		if (am <= 0) {
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
	public RBIT sourceAmountMask() {
		if (amount() - amoutReserved() > 0)
			return moveCapacity();
		return RBIT.NONE;
	}

	@Override
	public RBIT moveCapacity() {
		return resource() == null ? RBIT.NONE : resource().bit;
	}

	@Override
	public int moveCapacityAm(RESOURCE res) {
		if (resource() == res) {
			return capacity();
		}
		return 0;
	}

	@Override
	public double storedD(RESOURCE res) {
		if (res == resource())
			return (double)(amount()-amoutReserved())/capacity();
		return 0;
	}

	
}