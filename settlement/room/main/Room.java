package settlement.room.main;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;
import java.io.Serializable;

import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.misc.util.RESOURCE_TILE;
import settlement.misc.util.TILE_STORAGE;
import settlement.path.AVAILABILITY;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomState;
import snake2d.Renderer;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.INDEXED;
import snake2d.util.sprite.SPRITE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

public abstract class Room implements Serializable, INDEXED{

	public final static int MAX_SIZE = 2048;
	public final static int MAX_DIM = 55;
	private static final long serialVersionUID = 1L;
	protected final int roomI;
	short bI;
	final boolean singleton;

	
	protected Room(ROOMS m, RoomBlueprint p, boolean singleton) {
		roomI = m.map.create(this, singleton);
		bI = (short) p.index();
		this.singleton = singleton;
	}
	
	public final RoomBlueprint blueprint() {
		return ROOMS().all().get(bI);
	}
	
	protected abstract boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i);
	
	protected boolean renderAbove(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i) {
		return false;
	}
	
	protected boolean renderBelow(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i) {
		return false;
	}
	

	protected void loadFix() {
		
	}
	
//	public ICON.MEDIUM icon(){
//		return blueprint().icon();
//	}
	
	public abstract CharSequence name(int tx, int ty);
	
	protected abstract AVAILABILITY getAvailability(int tile);
	
	public abstract TmpArea remove(int tx, int ty, boolean scatter, Object user, boolean forced);
	
	protected void saveExtra(FilePutter file) {
		
	}
	
	protected boolean loadExtra(FileGetter file) throws IOException{
		return true;
	}
	
	@Override
	public final int index() {
		return roomI;
	}
	
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return null;
	}
	
	public TILE_STORAGE storage(int tx, int ty) {
		return null;
	}
	
	public abstract boolean destroyTileCan(int tx, int ty);
	public abstract void destroyTile(int tx, int ty);
	
	public final double getDegrade(int tx, int ty) {
		ROOM_DEGRADER deg = degrader(tx, ty);
		if (deg != null)
			return deg.get();
		return 0;
	}
	
	public abstract ROOM_DEGRADER degrader(int tx, int ty);
	
	public abstract int mX(int tx, int ty);
	public abstract int mY(int tx, int ty);
	public abstract int x1(int tx, int ty);
	public abstract int y1(int tx, int ty);
	public abstract int width(int tx, int ty);
	public abstract int height(int tx, int ty);
	
	public Furnisher constructor() {
		return null;
	}
	


	public abstract SPRITE icon();

	public double isolation(int tx, int ty) {
		return 1.0;
	}
	
	public void isolationSet(int tx, int ty, double isolation) {
		
	}
	
	public void updateTileDay(int tx, int ty) {

	}
	
	public abstract int resAmount(int ri, int upgrade);
	
	public int upgrade(int tx, int ty) {
		return 0;
	}
	
	public void upgradeSet(int tx, int ty, int upgrade) {
		
	}

	public boolean wallJoiner() {
		return false;
	}
	
	public RoomState makeState(int tx, int ty, boolean broken) {
		return RoomState.DUMMY;
	}
	
	protected final void setIndex(int tx, int ty) {
		SETT.ROOMS().map.set(tx+ty*SETT.TWIDTH, this);
	}
	
	protected final void clearIndex(int tx, int ty) {
		SETT.ROOMS().map.clear(tx+ty*SETT.TWIDTH, this);
	}
	
	protected final TmpArea delete(int mx, int my, Object o) {
		TmpArea a = ROOMS().map.delete(this, mx, my, o);
		return a;
	}
	
	public abstract int area(int tx, int ty);
	
	public boolean isBadMaintenanceTile(int tx, int ty) {
		return false;
	}
	
//	@Override
//	public boolean is(int tx, int ty) {
//		return SETT.IN_BOUNDS(tx, ty) && is(tx+ty*SETT.TWIDTH);
//	}
	
	public abstract boolean isSame(int tx, int ty, int ox, int oy);
	
	public static abstract class RoomInstanceImp extends Room implements ROOMA{
		
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		protected RoomInstanceImp(ROOMS m, RoomBlueprint p, boolean singleton) {
			super(m, p, singleton);
		}
		
		@Override
		public boolean isSame(int tx, int ty, int ox, int oy) {
			return SETT.IN_BOUNDS(ox, oy) && SETT.ROOMS().map.indexGetter.get(tx, ty) == roomI && SETT.ROOMS().map.indexGetter.get(ox, oy) == roomI;
		}
		
	
		
		@Override
		public int mX(int tx, int ty) {
			return mX();
		}
		
		@Override
		public int mY(int tx, int ty) {
			return mY();
		}
		
		@Override
		public int x1(int tx, int ty) {
			return body().x1();
		}
		
		@Override
		public int y1(int tx, int ty) {
			return body().y1();
		}
		
		@Override
		public int width(int tx, int ty) {
			return body().width();
		}
		
		@Override
		public int height(int tx, int ty) {
			return body().height();
		}
		
		@Override
		public boolean is(int tx, int ty) {
			return SETT.IN_BOUNDS(tx, ty) && is(tx+ty*SETT.TWIDTH);
		}
		
		@Override
		public int area(int tx, int ty) {
			return area();
		}
		
		@Override
		public int upgrade(int tx, int ty) {
			return upgrade();
		}
		
		@Override
		public void upgradeSet(int tx, int ty, int upgrade) {
			upgradeSet(upgrade);
		}
		
		public int upgrade() {
			return 0;
		}
		
		public void upgradeSet(int upgrade) {
			
		}
		
	}
	
}