package settlement.room.infra.export;

import static settlement.main.SETT.PATH;
import static settlement.main.SETT.ROOMS;

import game.GAME;
import game.faction.FACTIONS;
import game.faction.FResources.RTYPE;
import game.time.TIME;
import init.resources.RBIT;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.infra.logistics.MoveJob;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVEJOBBER;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVE_DEST;
import settlement.room.infra.logistics.MoveOrderPull;
import settlement.room.infra.logistics.MoveOrderPull.MoveOrderPullInstance;
import settlement.room.infra.stockpile.ROOM_STOCKPILE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.ROOM_RADIUS.ROOM_RADIUS_INSTANCE;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomState;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.rnd.RND;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

public final class ExportInstance extends RoomInstance implements ROOM_RADIUS_INSTANCE, ROOM_MOVE_DEST, ROOM_MOVEJOBBER, MoveOrderPullInstance{

	/**
	 * 
	 */
	private byte resourceI;
	private static final long serialVersionUID = 1L;
	static final int crateMax = 500;
	final short crates;
	int amount = 0;
	int amountReserved = 0;
	int spaceReserved = 0;
	boolean auto = true;
	
	public static final int ORDERS = 4;
	private final MoveOrderPull[] orders = new MoveOrderPull[ORDERS];
	byte coolFetch = -1;
	
	private short lastCX,lastCY;
	private short ox,oy;
	private byte orderI;
	private boolean fetching = true;
	private boolean prio = true;
	byte radius;

	ExportInstance(ROOM_EXPORT b, TmpArea area, RoomInit init) {
		super(b, area, init);

		int cc = 0;
		for (COORDINATE c : body()) {
			if (is(c)) {
				Crate crate = b.crate(c.x(), c.y());
				if (crate != null)
					cc++;
			}
		}
		
		crates = (short) cc;
		
		employees().maxSet(crates);
		employees().neededSet((int) Math.ceil(crates/20.0));
		activate();
	}

	@Override
	protected void loadFix() {
		if (resourceI > 0 && RESOURCES.map().loader().get(resourceI-1) == null) {
			amount = 0;
			amountReserved = 0;
			resourceI = 0;
			spaceReserved = 0;
			for (COORDINATE c : body()) {
				if (is(c)) {
					SETT.ROOMS().data.set(this, c, 0);
				}
			}
		}
	};
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}
	
	@Override
	protected void dispose() {
		resourceSet(null);
	}
	
	@Override
	protected void updateAction(double ds, boolean day) {
		
		if (coolFetch > 0) {
			coolFetch--;
		}

		for (MoveOrderPull o : orders) {
			if (o != null && o.cooldown > 0)
				o.cooldown--;
		}
		
		if (!active() || employees().employed() <= 0)
			return;


	}
	
	public RESOURCE resource() {
		if (resourceI == 0)
			return null;
		return RESOURCES.ALL().get(resourceI-1);
	}
	
	void resourceSet(RESOURCE r) {
		if (r == resource())
			return;
		
		if (resource() != null) {
			for (COORDINATE c : body()) {
				if (!is(c))
					continue;
				Crate crate = blueprintI().crate(c.x(), c.y());
				if (crate == null)
					continue;
				int am = crate.amount();
				crate.clear();
				if (am > 0) {
					for (DIR dd : DIR.ORTHO) {
						if (!PATH().solidity.is(c, dd)) {
							blueprintI().FETCHER.vacate(c.x()+dd.x(), c.y()+dd.y(), resource(), am);
							break;
						}
					}
				}
			}
			blueprintI().tally.inc(resource(), 0, -ExportInstance.crateMax*crates);	
		}
		if (amount != 0) {
			GAME.Notify(resource().name + " "+ this.amount);
			amount = 0;
		}
		
		resourceI = (byte) (r == null ? 0 : r.index()+1);
		if (resource() != null) {
			blueprintI().tally.inc(resource(), 0, ExportInstance.crateMax*crates);	
		}
		coolFetch = 0;
		

		for (MoveOrderPull o : orders) {
			if (o != null) {
				o.resbits.clear();
				if (r != null)
					o.resbits.or(r);
			}
			if (o != null && o.cooldown > 0)
				o.cooldown = 0;
		}
		
	}

	@Override
	protected void activateAction() {
	
	}

	@Override
	protected void deactivateAction() {
		
	}

	@Override
	public ROOM_EXPORT blueprintI() {
		return ROOMS().EXPORT;
	}
	
	@Override
	public RoomState makeState(int tx, int ty, boolean broken) {
		return new State(this, broken);
	}
	
	private static class State extends RoomState.RoomStateInstance {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		private boolean fetching;
		private boolean prio;
		private final boolean broken;
		private MoveOrderPull[] orders;
		private final int ri;
		
		public State(ExportInstance ins, boolean broken) {
			super(ins);
			this.broken = broken;
			this.fetching = ins.fetching;
			this.prio = ins.prio;
			this.ri = ins.resourceI;
			if (broken) {
				this.orders = ins.orders;
			}
			
		}
		
		@Override
		public void applyIns(RoomInstance ins) {
			if (ins instanceof ExportInstance) {
				if (ri != 0)
					((ExportInstance)ins).resourceSet(RESOURCES.ALL().get(ri-1));
				
				ExportInstance s = (ExportInstance) ins;
				if (broken) {
					for (int i = 0; i < orders.length; i++) {
						if (orders[i] != null) {
							MoveOrderPull p = new MoveOrderPull(orders[i].destCoo(), orders[i].resbits);
							s.orders[i] = p;
						}
						
					}
					
				}
				if (fetching != s.fetching())
					s.fetchingSet(fetching);
				if (prio != s.prio())
					s.prioSet();
				
			}
			
		}
		
		
		
	}

	@Override
	public boolean searching() {
		if (employees().employed() > 0) {
			if (coolFetch < 0)
				return true;
			for (MoveOrderPull p : orders) {
				if (p != null && p.cooldown < 0)
					return true;
			}
		}
		return false;
	}

	@Override
	public MoveOrderPull[] moveOrdersPull() {
		return orders;
	}

	@Override
	public RBIT moveOrderPullAccepted() {
		return resource() == null ? RBIT.NONE : resource().bit;
	}

	@Override
	public RBIT moveOrderPullAvailable() {
		return destCrate(moveOrderPullAccepted(), ROOM_STOCKPILE.MIN_CARRY, lastCX, lastCY) == null ? RBIT.NONE : resource().bit;
	}

	@Override
	public int moveMinAmount() {
		return ROOM_STOCKPILE.MIN_CARRY;
	}

	@Override
	public int moveMaxRadius() {
		return 200;
	}

	@Override
	public MoveJob moveJob(Humanoid skill) {
		if (resource() == null)
			return null;
		
		int am = SETT.ROOMS().STOCKPILE.carryCap(skill);
		
		boolean pr = prio;
		if (blueprintI().toFetchToExport(resource()) <= 0)
			pr = false;
		
		if ((fetching || pr) && coolFetch <= 0){

			RBIT bb = resource().bit;
			
			MoveJob j = MoveJob.fetch(this, this, am, radius(), ox, oy, fetching ? bb : RBIT.NONE, pr ? bb : RBIT.NONE);
			
			if (j != null) {
				ox = (short) j.source.x();
				oy = (short) j.source.y();
				coolFetch = -1;
				return j;
			}
			
			coolFetch = 4;
		}
		
		for (int ooi = 0; ooi < orders.length; ooi++) {
			orderI++;
			if (orderI >= orders.length)
				orderI = 0;
			MoveOrderPull p = orders[orderI];
			if (p != null && p.cooldown <= 0) {
				MoveJob j = p.job(this, Math.min(am, 1), am);
				
				if (j != null) {
					p.cooldown = -1;
					return j;
				}
				p.cooldown = 4;
			}
		}
		return null;
	}

	@Override
	public TILE_STORAGE destCrate(RBIT okMask, int minAm, int ox, int oy) {
		if (!okMask.has(destSpaceMask()))
			return null;
		
		if (is(lastCX, lastCY)) {
			Crate c = blueprintI().crate(lastCX, lastCY);
			if (c != null && c.storageReservable() >= minAm) {
				return c;
			}
		}
		
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			Crate crate = blueprintI().crate(c.x(), c.y());
			if (crate != null && crate.storageReservable() >= minAm) {
				lastCX = (short) c.x();
				lastCY = (short) c.y();
				return crate;
			}
		}
		return null;
	}
	
	@Override
	public TILE_STORAGE storage(int tx, int ty) {
		return  blueprintI().crate(tx, ty);
	}
	
	@Override
	public RBIT destSpaceMask() {
		if (resource() == null)
			return RBIT.NONE;
		if (crates*crateMax-amount-spaceReserved <= 0)
			return RBIT.NONE;
		return resource().bit;
	}

	@Override
	public double storedD(RESOURCE res) {
		return (double)(crates*crateMax-amount-spaceReserved)/(crates*crateMax);
	}

	@Override
	public RBIT moveCapacity() {
		return resource() == null ? RBIT.NONE : resource().bit;
	}

	public boolean fetching() {
		return fetching;
	}
	
	public void fetchingSet(boolean f) {
		this.fetching = f;
		coolFetch = 0;
	}
	
	@Override
	public int radius() {
		return (this.radius + 10)*8;
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
	public void updateTileDay(int tx, int ty) {
		if (resource() == null)
			return;
		Crate c = blueprintI().crate(tx, ty);
		if (c == null)
			return;
		int am = c.amount()-c.reserved();
		if (am <= 0)
			return;
		if (am < 0)
			return;
		
		double d = am*resource().degradeSpeed()/TIME.years().bitConversion(TIME.days());
		int i = (int)d;
		if (d -i > RND.rFloat())
			i++;
		i = Math.min(am, i);
		if (i > 0) {
			c.amountSet(c.amount()-i);
			FACTIONS.player().res().inc(resource(), RTYPE.SPOILAGE, -i);
		}
		
		
		super.updateTileDay(tx, ty);
	}

	@Override
	public void copyFrom(MoveOrderPullInstance same) {
		ExportInstance ins = (ExportInstance) same;
		resourceSet(ins.resource());
		fetchingSet(ins.fetching());
		auto = ins.auto;
		prio = ins.prio;
		radius = ins.radius;
		employees().neededSet(ins.employees().target());
	}
	
	boolean prio() {
		return prio;
	}
	
	void prioSet() {
		prio = !prio;
		coolFetch = -1;
	}
	

	
}