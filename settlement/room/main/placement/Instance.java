package settlement.room.main.placement;

import static settlement.main.SETT.FLOOR;
import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TERRAIN;
import static settlement.main.SETT.THEIGHT;
import static settlement.main.SETT.TWIDTH;
import static settlement.room.main.construction.ConstructionData.dBroken;
import static settlement.room.main.construction.ConstructionData.dConstructed;
import static settlement.room.main.construction.ConstructionData.dFloored;

import java.io.IOException;

import init.sprite.UI.Icon;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.path.AVAILABILITY;
import settlement.room.main.ROOMS;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.sprite.RoomSprite;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.RECTANGLE;
import snake2d.util.datatypes.Rec;
import snake2d.util.file.FileGetter;
import snake2d.util.misc.CLAMP;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Instance extends Room.RoomInstanceImp{

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;

	private final Rec bounds = new Rec();
	private int area = 0;
	final Coo mCoo = new Coo();
	transient RoomBlueprintImp blue;

	int unroofed = 0;
	int upgrade = 0;

	private boolean bodyChange = false;
	
	protected void set(int tx, int ty) {
		if (SETT.IN_BOUNDS(tx, ty) && !SETT.ROOMS().map.is(tx, ty)) {
			int i = tx+ty*SETT.TWIDTH;			
			setSoft(tx, ty);
			ROOMS().data.set(this, i, 0);
			SETT.TILE_MAP().miniCUpdate(tx, ty);
		}
	}
	
	void set(TmpArea area, RoomBlueprintImp blue) {
		init(blue);
		
		for (COORDINATE c : area.body()) {
			
			if (!area.is(c))
				continue;
			
			
			SETT.LIGHTS().remove(c.x(), c.y());
			
		}
		
		bounds.set(area);
		this.area = area.area();
		mCoo.set(area.mx(), area.my());
		
		area.replaceAndClear(this);
	}
	
	@Override
	public int upgrade() {
		return upgrade;
	}
	
	@Override
	public void upgradeSet(int upgrade) {
		if (blue== null)
			this.upgrade = 0;
		else
			this.upgrade = CLAMP.i(upgrade, 0, blue.upgrades().max());
	}
	
	private void setSoft(int tx, int ty) {
		if (SETT.IN_BOUNDS(tx, ty)) {
			bounds.unify(tx, ty);
			area++;
			setIndex(tx, ty);
			ROOMS().data.set(this, tx, ty, 0);
			if (area == 1)
				mCoo.set(tx, ty);
			if (!TERRAIN().get(tx, ty).roofIs())
				unroofed++;
		}
	}
	
	void clear(int tx, int ty) {
		if (SETT.IN_BOUNDS(tx, ty)) {
			
			int i = tx+ty*SETT.TWIDTH;
			
			if (is(tx, ty)) {
				bodyChange = true;
				ROOMS().fData.itemClear(tx, ty, this);
				ROOMS().data.set(this, tx, ty, 0);
				clearIndex(tx, ty);
				SETT.TILE_MAP().miniCUpdate(tx, ty);
				if (dFloored.is(ROOMS().data.get(i), 1))
					FLOOR().clearer.clear(i);
				
				area--;
				if (!TERRAIN().get(tx, ty).roofIs())
					unroofed--;
				if (area == 0) {
					bounds.set(TWIDTH, 0, THEIGHT, 0);
					mCoo.set(-1, -1);
				}
			}
			
		}
	}
	
	private void setBlueprint(RoomBlueprintImp blue) {
		if (area > 0) {
			for (COORDINATE c : bounds) {
				clear(c.x(), c.y());
			}
		}
		clearRegardless();
		setBlue(blue);
		upgradeSet(0);
	}
	
	void init(RoomBlueprintImp blue) {
		setBlueprint(blue);
	}
	
	void clear(RoomBlueprintImp blue){
		setBlueprint(blue);
		clearRegardless();
		this.blue = blue;
		upgradeSet(0);
	}
	
	void clearRegardless(){
		
		bounds.set(TWIDTH, 0, THEIGHT, 0);
		mCoo.set(-1, -1);
		area = 0;
		unroofed = 0;
		setBlue(null);
		upgrade = 0;
	}
	
	private void setBlue(RoomBlueprintImp blue) {
		this.blue = blue;
	}
	
	protected Instance(ROOMS m, RoomBlueprint p) {
		super(m, p, true);
	}
	
	@Override
	protected boolean loadExtra(FileGetter file) throws IOException {
		return false;
	}

	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		
		FurnisherItemTile it = ROOMS().fData.tile.get(i.tile());
		if (it != null && it.sprite() != null) {
			if (dConstructed.is(i.tile(), 0)) {
//				SPRITES.cons().color.ok.bind();
//				it.sprite.renderPlaceholder(r, i.x(), i.y(), ROOMS().fData.spriteData.get(i.tile()), i.tx(), i.ty(), it);
			}else if(dBroken.is(i.tile(), 1)) {
				it.sprite().renderBroken(r, shadowBatch, i.x(), i.y(), i, ROOMS().fData.item.get(i.tile()));
			}else {
				
				return it.sprite().render(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, 0, false);
			}
//			if (it.mustBeReachable)
//				SPRITES.cons().ICO.arrows_inwards.render(r, i.x(), i.y());
			
		}else {
//			if (ConstructionData.dExpensive.is(i.tile(), 1))
//				GCOLORS_PLACABLE.SOSO.bind();
//			else
//				SPRITES.cons().color.ok.bind();
//			int m = 0;
//			for (DIR d : DIR.ORTHO) {
//				if (is(i.tx(), i.ty(), d))
//					m |= d.mask();
//			}
//			ROOMS().placement.blueprint.constructor().renderEmbryo(r, m, i, dFloored.is(i.tile(), 1), this);
		}
		
//		if (ROOMS().placement.autoWalls.isOn() && !repair) {
//			ROOMS().placement.door.renderWall(r, this, i);
//		}
		
		
//		COLOR.unbind();
		return false;
	}
	
	private Rec tmp = new Rec();
	private Rec tmp2 = new Rec();
	
	@Override
	protected boolean renderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator it) {
		
		if (bodyChange && area > 0) {
			bodyChange = false;
			boolean first = true;
			tmp.set(body());
			for (COORDINATE c : tmp) {
				if (is(c)) {
					if (first) {
						first = false;
						tmp2.setDim(1).moveX1Y1(c);
					}else {
						tmp2.unify(c.x(), c.y());
					}
				}
			}
			bounds.set(tmp2);
		}
		
		if (dConstructed.is(it.tile(), 1) && dBroken.is(it.tile(), 0)) {
			RoomSprite sp = ROOMS().fData.sprite.get(it.tile());
			if (sp != null)
				sp.renderAbove(r, shadowBatch, ROOMS().fData.spriteData.get(it.tile()), it, 0);
		}
		return false;
	}

	@Override
	protected boolean renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator it) {
		if (dConstructed.is(it.tile(), 1) && dBroken.is(it.tile(), 0)) {
			RoomSprite sp = ROOMS().fData.sprite.get(it.tile());
			if (sp != null)
				sp.renderBelow(r, shadowBatch, ROOMS().fData.spriteData.get(it.tile()), it, 0);
		}
		if (constructor() != null) {
			constructor().renderTileBelow(r, shadowBatch, it, dFloored.is(it.tile(), 1));
		}
		return false;
	}

	@Override
	protected AVAILABILITY getAvailability(int tile) {
		return null;
	}
	
	@Override
	public Furnisher constructor() {
		if (blue == null)
			return null;
		return blue.constructor();
	}
	
	protected Object readResolve() {
		Instance i = ROOMS().placement.placer.instance;
		i.bounds.set(bounds);
		i.area = area;
		i.mCoo.set(mCoo);
		i.unroofed = unroofed;
		return i;
	}

	@Override
	public RECTANGLE body() {
		return bounds;
	}


	@Override
	public void destroyTile(int tx, int ty) {
		
	}

	@Override
	public boolean destroyTileCan(int tx, int ty) {
		return false;
	}

	@Override
	public ROOM_DEGRADER degrader(int tx, int ty) {
		return null;
	}

	@Override
	public int mX() {
		return mCoo.x();
	}

	@Override
	public int mY() {
		return mCoo.y();
	}

	@Override
	public int area() {
		return area;
	}

	@Override
	public CharSequence name(int tx, int ty) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public Icon icon() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public int resAmount(int ri, int upgrade) {
		return 0;
	}

	@Override
	public boolean is(int tile) {
		return SETT.ROOMS().map.indexGetter.get(tile) == roomI;
	}

	@Override
	public TmpArea remove(int tx, int ty, boolean scatter, Object user, boolean forced) {
		// TODO Auto-generated method stub
		return null;
	}
	
	
	

}
