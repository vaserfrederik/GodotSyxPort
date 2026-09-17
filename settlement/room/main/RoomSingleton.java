package settlement.room.main;

import static settlement.main.SETT.FLOOR;
import static settlement.main.SETT.PATH;
import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TERRAIN;
import static settlement.main.SETT.TWIDTH;

import java.io.Serializable;

import init.resources.RESOURCE;
import init.sprite.UI.Icon;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.path.AVAILABILITY;
import settlement.room.main.construction.ConstructionData;
import settlement.room.main.construction.ConstructionInit;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.util.Deleter;
import settlement.room.main.util.RoomAreaWrapper;
import settlement.room.sprite.RoomSprite;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.Rec;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public abstract class RoomSingleton extends Room {

	private static final long serialVersionUID = 1L;
	private int size;
	private final Rec tiles = new Rec(0,0,0,0);
	private final Coo dataCoo = new Coo();
	private final Coo upperLeft = new Coo();
	private int dataTile;
	private int data;
	protected transient FurnisherItem item;
	
	protected static final transient RoomAreaWrapper wrap = new RoomAreaWrapper();
	protected static final transient RoomAreaWrapper wrapD = new RoomAreaWrapper();
	
	protected RoomSingleton(ROOMS m, RoomBlueprint p){
		super(m, p, true);
	}
	
	public final RoomSingleton place(TmpArea area) {
		
		int mx = area.mx();
		int my = area.my();
		
		area.replaceAndClear(this);
		iniHard(mx, my);
		ROOMA a = wrap.init(this, mx, my);
		addAction(a);
		
		
		SETT.ROOMS().map.init(a);
		isolationSet(a.mX(), a.mY(), SETT.ROOMS().isolation.getProspect(blueprint(), a, null));
		wrap.done();
		SETT.MAINTENANCE().initRoomDegrade(this, a.mX(), a.mY());
		
		
		return this;
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		RoomSprite s = ROOMS().fData.sprite.get(i.tile());
		if (s != null)
			return s.render(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade(i.tx(), i.ty()), ROOMS().fData.candle.is(i.tile()));
		return false;
	}
	
	@Override
	protected boolean renderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		RoomSprite s = ROOMS().fData.sprite.get(i.tile());
		if (s != null)
			s.renderAbove(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade(i.tx(), i.ty()));
		return false;
	}
	
	@Override
	protected boolean renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		RoomSprite s = ROOMS().fData.sprite.get(i.tile());
		if (s != null)
			s.renderBelow(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade(i.tx(), i.ty()));
		return false;
	}
	
	@Override
	protected AVAILABILITY getAvailability(int tile) {
		FurnisherItemTile t = ROOMS().fData.tile.get(tile);
		if (t != null)
			return t.availability;
		return AVAILABILITY.ROOM;
	}
	
	@Override
	public final Furnisher constructor() {
		return blueprintI().constructor();
	}
	
	@Override
	public boolean destroyTileCan(int tx, int ty) {
		return ROOMS().fData.availability.get(tx, ty).player < 0 || ROOMS().fData.availability.get(tx, ty).enemy < 0;
	}
	
	@Override
	public void destroyTile(int tx, int ty) {
		iniHard(tx, ty);
		ConstructionInit init = new ConstructionInit(this, tx, ty, true);
		ROOMA ar = wrap.init(this, tx, ty);
		
		
		for (COORDINATE c : ar.body()) {
			if (!ar.is(c))
				continue;
			PATH().availability.updateService(c.x(), c.y());
			if (!TERRAIN().get(c).clearing().isStructure())
				TERRAIN().NADA.placeFixed(c.x(), c.y());
		}
		wrap.done();
		TmpArea a = remove(tx, ty, false, this, true);
		ROOMS().construction.breakIt(a, init, tx, ty);
		
	}
	

	@Override
	public int area(int tx, int ty) {
		iniHard(tx, ty);
		return size;
	}
	
	@Override
	public final TmpArea remove(int tx, int ty, boolean scatter, Object obj, boolean force) {
		iniHard(tx, ty);
		ROOMA a = wrap.init(this, tx, ty);
		SETT.ROOMS().stats.broken().remove(a.mX(), a.mY());
		removeAction(a);
		if (scatter)
			Deleter.scatterMaterials(a, constructor(), upgrade(tx, ty), getDegrade(tx, ty));
		
		for (COORDINATE c : a.body()) {
			if (!a.is(c))
				continue;
			SETT.LIGHTS().remove(c.x(), c.y());
			FLOOR().clearer.clear(c.x(), c.y());
			SETT.ROOMS().data.set(a, c, 0);
			ConstructionData.dConstructed.set(a, c, 1);
			ConstructionData.dFloored.set(a, c, 1);
		}
		
		TmpArea ar = ROOMS().map.delete(this, a.mX(), a.mY(), obj);
		
		wrap.done();
		wrap.clear();
		tiles.setDim(0).moveX1Y1(-1, -1);
		
		return ar;
	}
	
	protected void addAction(ROOMA ins) {
		
	}
	
	
	protected void removeAction(ROOMA ins) {
		
	}
	
	@Override
	public ROOM_DEGRADER degrader(int tx, int ty) {
		iniHard(tx, ty);
		degA = wrapD.init(this,  dataCoo.x(),  dataCoo.y());
		wrapD.done();
		return degrader;
	}
	
	private transient ROOMA degA;
	private final Degrader degrader = new Degrader();
	
	private class Degrader extends ROOM_DEGRADER implements Serializable {
		
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		@Override
		public int resSize() {
			return item.group.blueprint.resources();
		}

		@Override
		public int resAmount(int i) {
			return (int) Math.ceil(item.cost2(i, upgrade(degA.mX(), degA.mY())));
		}

		@Override
		public RESOURCE res(int i) {
			return item.group.blueprint.resource(i);
		}

		@Override
		public double degRate() {
			return degradeResNeeded();
		}

		@Override
		public  int getData() {
			return data;
		}

		@Override
		protected void setData(int v, boolean realDegradeChange) {
			double old = get();
			data = v;
			
			ROOMS().data.set(degA, dataTile, data);
			if (old != get())
				degradeChange(dataCoo.x(), dataCoo.y(), old, get(), realDegradeChange);
		}

		@Override
		public int roomArea() {
			return item.width()*item.height();
		}
		
		@Override
		public double base() {
			return blueprintI().degradeRate();
		}

		@Override
		public double expenseRate() {
			return 1;
		}

		@Override
		public double rate(double bonus) {
			double am = 0;
			for (int ri = 0; ri < resSize(); ri++) {
				am += resAmount(ri);
			}
			return Degrader.rate(bonus, base(), isolation(dataCoo.x(), dataCoo.y()), am, area(dataCoo.x(), dataCoo.y()));
		}
	};
	
	@Override
	public int resAmount(int ri, int upgrade) {
		return (int) Math.ceil(item.cost2(ri, upgrade));
	}
	
	protected void degradeChange(int mx, int my, double oldD, double newD, boolean realDegradeChange) {
		
	}
	
	protected double degradeResNeeded() {
		return 0.5;
	}
	
	public abstract RoomBlueprintImp blueprintI();
	
	@Override
	public CharSequence name(int tx, int ty) {
		iniHard(tx, ty);
		if (blueprintI().constructor() != null && blueprintI().constructor().groups().size() > 1)
			return item.group.name;
		return blueprintI().info.name;
	}
	
	@Override
	public Icon icon() {
		return blueprintI().iconBig();
	}
	
	private final boolean ini(int tx, int ty) {
		if (tiles.holdsPoint(tx, ty) && item != null && item == ROOMS().fData.item.get(tx, ty))
			return true;
		
		ROOMS().fData.itemX1Y1(tx, ty, upperLeft, this);
		item = ROOMS().fData.item.get(tx, ty);
		dataCoo.set(upperLeft);
		dataCoo.increment(item.firstX(), item.firstY());
		
		tiles.moveX1Y1(upperLeft);
		tiles.setWidth(item.width());
		tiles.setHeight(item.height());
		size = item.width()*item.height();
		dataTile = dataCoo.x()+dataCoo.y()*TWIDTH;
		data = ROOMS().data.get(dataTile);
		
		return true;
	}
	
	protected void iniHard(int tx, int ty) {
		if (!ini(tx, ty)) {
			throw new RuntimeException(tx + " " + ty + " " + this + " " + SETT.ROOMS().map.get(tx, ty));
		}
	}
	
	@Override
	public boolean isSame(int tx, int ty, int ox, int oy) {
		if (blueprint().is(tx, ty)) {
			iniHard(tx, ty);
			return iss(ox, oy);
		}
		return false;
	}
	
	private static Coo upperLeftTest = new Coo();
	
	private boolean iss(int tx, int ty) {
		if (tiles.holdsPoint(tx, ty) && item == ROOMS().fData.item.get(tx, ty)) {
			ROOMS().fData.itemX1Y1(tx, ty, upperLeftTest, this);
			if (upperLeftTest.isSameAs(upperLeft))
				return true;
		}
		return false;
	}
	
	@Override
	public int mX(int tx, int ty) {
		iniHard(tx, ty);
		return dataCoo.x();
	}
	
	@Override
	public int mY(int tx, int ty) {
		iniHard(tx, ty);
		return dataCoo.y();
	}
	
	@Override
	public int x1(int tx, int ty) {
		iniHard(tx, ty);
		return tiles.x1();
	}
	
	@Override
	public int y1(int tx, int ty) {
		iniHard(tx, ty);
		return tiles.y1();
	}
	
	@Override
	public int width(int tx, int ty) {
		iniHard(tx, ty);
		return tiles.width();
	}
	
	@Override
	public int height(int tx, int ty) {
		iniHard(tx, ty);
		return tiles.height();
	}


}
