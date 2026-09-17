package settlement.room.main;

import static settlement.main.SETT.JOBS;
import static settlement.main.SETT.PATH;
import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TWIDTH;

import init.resources.RESOURCE;
import init.sprite.UI.Icon;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.path.AVAILABILITY;
import settlement.room.main.construction.ConstructionData;
import settlement.room.main.construction.ConstructionInit;
import settlement.room.main.employment.RoomEmploymentIns;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.util.Deleter;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomState;
import settlement.room.sprite.RoomSprite;
import snake2d.Renderer;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.RECTANGLE;
import snake2d.util.datatypes.Rec;
import snake2d.util.misc.CLAMP;
import snake2d.util.sprite.text.Str;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public abstract class RoomInstance extends Room.RoomInstanceImp implements AREA{


	private static final long serialVersionUID = 1L;
	private boolean exists = false;
	private final int size;
	private boolean reachable = true;
	private boolean active = false;
	private final short fx,fy;
	private final RoomEmploymentIns work = new RoomEmploymentIns(this);

	private final Rec tiles;

	private double stats[];
	private double ress[];
	private double resMul;
	private int degrade = 0;
	
	protected final Str iname = new Str.StringReusableSer(32);
	private byte upgrade;
	private float isolation = 0;
	
	protected RoomInstance(RoomBlueprintIns<? extends RoomInstance> blueprint, TmpArea area, RoomInit init) {
		super(ROOMS(), blueprint, false);
		this.size = area.area();
		this.tiles = new Rec(area.body());
		this.stats = init.stats;
		this.ress = init.res;
		this.resMul = init.resMul;
		int a = 0;
		this.degrade = init.degrade;
		int ffx = -1;
		int ffy = 0;
		
		area.replaceAndClear(this);
		
		for (COORDINATE c : body()) {
			if (is(c)) {
				
				if (ffx == -1) {
					if (tiles.width() == 1 && tiles.height() == 1) {
						ffx = c.x();
						ffy = c.y();
					}
					AVAILABILITY av = getAvailability(c.x()+c.y()*TWIDTH);
					if (av.from <= 0 && av.player > 0) {
						ffx = c.x();
						ffy = c.y();
					}
				}
				
				a++;
			}
			
		}
		
		if (ffx == -1)
			throw new RuntimeException(tiles.toString() + " " + a);
		
		fx = (short) ffx;
		fy = (short) ffy;
		
		if (a != this.size)
			throw new RuntimeException(a + " " + this.size);
		
		if (a == 0)
			throw new RuntimeException();
		
		exists = true;
		
		int nr = blueprint.instancesSize() + 1;
		
		iname.add(blueprint.info.name).s().add('#');
		if (nr < 100)
			iname.add('0');
		if (nr < 10)
			iname.add('0');
		iname.add(nr);
		
		blueprint.addInstance(this);
		isolation = (float) SETT.ROOMS().isolation.getProspect(blueprint, this, null);
		SETT.MAINTENANCE().initRoomDegrade(this, mX(), mY());
		

		SETT.ROOMS().map.init(this);
	}
	

	
	public final boolean exists() {
		return exists;
	}
	
	protected final void update(double updateInterval, boolean day) {
		
		updateReachability();	
		
		updateAction(updateInterval, day);
		
		
		((SecretEmployment)work).update(active(), day, this.blueprintI() instanceof ROOM_EMPLOY_AUTO && ((ROOM_EMPLOY_AUTO)this.blueprintI()).autoEmploy(this), updateInterval);
		
	}
	
	public abstract RoomBlueprintIns<? extends RoomInstance> blueprintI();
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		if (blueprintI().soundAmbiance != null)
			blueprintI().soundAmbiance.priorityInc(1);
		RoomSprite s = ROOMS().fData.sprite.get(i.tile());
		if (s != null)
			return s.render(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade(), ROOMS().fData.candle.is(i.tile()));
		return false;
	}
	
	@Override
	protected boolean renderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		RoomSprite s = ROOMS().fData.sprite.get(i.tile());
		if (s != null)
			s.renderAbove(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade());
		return false;
	}
	
	@Override
	protected boolean renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		RoomSprite s = ROOMS().fData.sprite.get(i.tile());
		if (s != null)
			s.renderBelow(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade());
		return false;
	}
	
	@Override
	protected AVAILABILITY getAvailability(int tile) {
		FurnisherItemTile t = ROOMS().fData.tile.get(tile);
		if (t != null)
			return t.availability;
		return AVAILABILITY.ROOM;
	}
	
	private final void updateReachability() {
		boolean was = active(); 
		reachable = PATH().reachability.is(mX(), mY());
		
		if (!active() && was) {
			deactivateAction();
		}else if (active() && !was){
			activateAction();
		}
		((SecretEmployment)work).activate(active());
	}
	
	@Override
	public boolean is(int tile) {
		return ROOMS().map.is(tile, this);
	}
	
	protected void updateAction(double updateInterval, boolean day) {
		
	}
	
	public final boolean reachable() {
		return reachable;
	}

	public final void activate(boolean a) {
		
		if (active()) {
			deactivateAction();
		}
		
		active = a;
		
		if (active()) {
			activateAction();
		}
		
		((SecretEmployment)work).activate(active());
		
		
		
	}
	
	protected abstract void activateAction();
	protected abstract void deactivateAction();
	
	@Override
	public final int area() {
		return size;
	}

	public final boolean active() {
		return exists && active && reachable;
	}
	
	public final void activate() {
		activate(true);
		update(0, false);
	}
	
	public final void deactivate() {
		activate(false);
	}
	
	@Override
	public Str name(int tx, int ty) {
		return iname;
	}
	
	public Str name() {
		return iname;
	}
	
	protected abstract void dispose();
	
	@Override
	public RECTANGLE body () {
		return tiles;
	}
	
	@Override
	public final TmpArea remove(int tx, int ty, boolean scatter, Object obj, boolean forced) {
		if (!exists)
			throw new RuntimeException();
		if (!is(tx, ty))
			throw new RuntimeException();
		
		if (!canRemoveAndRemoveAction(tx, ty, scatter, obj, forced))
			return SETT.ROOMS().tmpArea(obj);
		
		
		
		
		SETT.ROOMS().stats.finished().remove(mX(), mY());
		deactivate();
		
		((SecretEmployment)employees()).dispose();
		
		dispose();
		
		if (scatter && constructor() != null)
			Deleter.scatterMaterials(this, constructor(), upgrade(), getDegrade());
		
		
		
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			SETT.LIGHTS().remove(c.x(), c.y());
			JOBS().clearer.set(c);
			//FLOOR().clearer.clear(c.x(), c.y());	
			SETT.ROOMS().data.set(this, c, 0);
			if (SETT.ROOMS().fData.item.get(c) != null)
				ConstructionData.dConstructed.set(this, c, 1);
			ConstructionData.dFloored.set(this, c, 1);

		}
		exists = false;
		blueprintI().removeInstance(this);
		
		TmpArea a = ROOMS().map.delete(this, mX(), mY(), obj);
		return a;
		
	}
	
	protected boolean canRemoveAndRemoveAction(int tx, int ty, boolean scatter, Object obj, boolean forced) {
		return true;
	}
	
	@Override
	public void destroyTile(int tx, int ty) {
		ConstructionInit in = new ConstructionInit(this, mX(), mY(), true);
		TmpArea a = remove(tx, ty, false, this, true);
		SETT.ROOMS().construction.breakIt(a, in, tx, ty);
	}
	
	@Override
	public boolean destroyTileCan(int tx, int ty) {
		return ROOMS().fData.availability.get(tx, ty).player < 0 || ROOMS().fData.availability.get(tx, ty).enemy < 0;
	}
	
	public boolean acceptsWork() {
		return true;
	}
	
	public double getDegrade() {
		return Degrader.get(this.degrade);
	}
	
	@Override
	public ROOM_DEGRADER degrader(int tx, int ty) {
		if (constructor() != null) {
			deg.ins = this;
			deg.fu = constructor();
			return deg;
		}
		return null;
	}
	
	@Override
	public int mX() {
		return fx;
	}
	
	@Override
	public int mY() {
		return fy;
	}
	
	public final RoomEmploymentIns employees() {
		return work;
	}
	
	@Override
	public final Furnisher constructor() {
		return blueprintI().constructor();
	}

	@Override
	public double isolation(int tx, int ty) {
		if (constructor() == null || !constructor().mustBeIndoors())
			return 1.0;
		return isolation;
	}

	@Override
	public void isolationSet(int tx, int ty, double isolation) {
		this.isolation = (float) isolation;
	}
	
	private final static Degrader deg = new Degrader();
	
	public final double stat(int si) {
		if (stats == null)
			stats = new double[blueprintI().constructor().stats().size()];
		if (stats.length != blueprintI().constructor().stats().size()) {
			double ns[] = new double[blueprintI().constructor().stats().size()];
			for (int i = 0; i < ns.length && i < stats.length; i++)
				ns[i] = stats[i];
			stats = ns;
		}
		return stats[si];
	}
	
	@Override
	public Icon icon() {
		return blueprintI().iconBig();
	}
	
	private static class Degrader extends ROOM_DEGRADER {

		private RoomInstance ins;
		private Furnisher fu;

		@Override
		public int resSize() {
			return ins.constructor().resources();
		}

		@Override
		public int resAmount(int i) {
			return ins.resAmount(i, ins.upgrade());
		}

		@Override
		public RESOURCE res(int i) {
			return fu.resource(i); 
		}
		
		@Override
		public double degRate() {

			return ins.blueprintI().degradeRate();
		}
		

		@Override
		public int getData() {
			return ins.degrade;
		}

		@Override
		protected void setData(int v, boolean realChange) {
			ins.blueprintI().averageDegrade -= (int)Math.ceil(100*get());
			ins.degrade = v;
			ins.blueprintI().averageDegrade += (int)Math.ceil(100*get());
		}

		@Override
		public int roomArea() {
			return ins.area();
		}

		@Override
		public double base() {
			return ins.blueprintI().degradeRate();
		}

		@Override
		public double expenseRate() {
			return ins.resMul;
		}

		@Override
		public double rate(double bonus) {
			double am = 0;
			for (int ri = 0; ri < deg.resSize(); ri++) {
				am += deg.resAmount(ri);
			}
			return Degrader.rate(bonus, base(), ins.isolation(ins.mX(), ins.mY()), am, ins.area());
		}
		
		
	}
	
	@Override
	public int resAmount(int ri, int upgrade) {
		if (ress == null)
			ress = new double[blueprintI().constructor().resources()];
		if (stats.length != blueprintI().constructor().resources()) {
			double ns[] = new double[blueprintI().constructor().resources()];
			for (int i = 0; i < ns.length && i < ress.length; i++)
				ns[i] = ress[i];
			ress = ns;
		}

		if (ri < ress.length)
			return (int) Math.ceil(ress[ri]*blueprintI().upgrades().resMask(upgrade, ri));
		return 0;
		
	}
	
	@Override
	public RoomState makeState(int rx, int ry, boolean broken) {
		return new RoomState.RoomStateInstance(this);
	}
	
	
	
	@Override
	public int upgrade() {
		return CLAMP.i(upgrade, 0, blueprintI().upgrades().max());
	}
	
	@Override
	public void upgradeSet(int upgrade) {
		if (this.upgrade != upgrade) {
			blueprintI().upgrades -= this.upgrade*this.area();
			this.upgrade = (byte) upgrade;
			blueprintI().upgrades += this.upgrade*this.area();
			for (COORDINATE c : body()) {
				if (is(c)) {
					blueprintI().constructor().putFloor(c.x(), c.y(), upgrade, this);
				}
				SETT.MAINTENANCE().setChanged(c.x(), c.y());
			}
			
		}
		
	}
	
	public static abstract class SecretEmployment {
		protected abstract void update(boolean active, boolean day, boolean auto, double seconds);
		protected abstract void activate(boolean active);
		protected abstract void dispose();
		
		protected SecretEmployment() {
			
		}
	}

}