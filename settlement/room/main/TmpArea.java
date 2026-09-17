package settlement.room.main;

import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TWIDTH;
import static settlement.room.main.construction.ConstructionData.dFloored;

import java.io.IOException;

import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.path.AVAILABILITY;
import settlement.path.finders.SFinderFindable;
import settlement.room.main.Room.RoomInstanceImp;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomAreaWrapper;
import snake2d.Renderer;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.RECTANGLE;
import snake2d.util.datatypes.Rec;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.map.MAP_SETTER;
import snake2d.util.sprite.SPRITE;
import util.GUTIL;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public final class TmpArea implements MAP_SETTER, ROOMA {

	private final RoomBlueprint b = new RoomBlueprint("_TMPAREA") {
		
		@Override
		protected void update(double ds) {
			if (lastUser != null)
				error();
			
		}
		
		@Override
		public SFinderFindable service(int tx, int ty) {
			return null;
		}
		
		@Override
		protected void save(FilePutter file) {
			// TODO Auto-generated method stub
			
		}
		
		@Override
		public COLOR miniC(int tx, int ty) {
			return null;
		}
		
		@Override
		protected void load(FileGetter file) throws IOException {
			clear();
		}
		
		@Override
		protected void clear() {
			TmpArea.this.clear();
		}

		@Override
		public COLOR miniCPimped(ColorImp origional, int tx, int ty, boolean northern, boolean southern) {
			return origional;
		}
	};
	
	private final Instance ins;
	private Object lastUser;
	private StackTraceElement[] els = new StackTraceElement[0];
	private static final RoomAreaWrapper wrap = new RoomAreaWrapper();
	private Furnisher cons;
	private boolean removeFloor;
	
	TmpArea(ROOMS m){
		ins = new Instance(m, b);
	}
	
	void init(Object user) {
		if (lastUser != null)
			error();
		lastUser = user;
		cons = null;
		removeFloor = true;
//		els = new RuntimeException().getStackTrace();
	}
	
	public void setDontRemoveFloor() {
		removeFloor = false;
	}
	
	public void setRemoveFloor() {
		removeFloor = true;
	}
	
	public void set(Room o, int rx, int ry) {
		
		ROOMA a = wrap.init(o, rx, ry);
		cons = o.constructor();
		GUTIL.coos().set(0);
		
		
		for (COORDINATE c : a.body()) {
			if (a.is(c)) {
				GUTIL.coos().get().set(c);
				GUTIL.coos().inc();
			}
		}
		
		int k = GUTIL.coos().getI();
		
		for (int i = 0; i < k; i++) {
			COORDINATE c = GUTIL.coos().set(i);
			ROOMS().map.replace(c.x()+c.y()*SETT.TWIDTH, o, ins);
			ins.setP(c.x(), c.y());
		}
		wrap.done();
	}
	
	@Override
	public MAP_SETTER set(int tile) {
		ins.set(tile%TWIDTH, tile/TWIDTH);
		return this;
	}

	@Override
	public MAP_SETTER set(int tx, int ty) {
		ins.set(tx, ty);
		return this;
	}
	
	public void replaceAndClear(Room o) {
		
		
		for (COORDINATE c : ins.body) {
			if (ins.is(c)) {
				ROOMS().map.replace(c.x()+c.y()*SETT.TWIDTH, ins, o);
			}
		}
		clear();
	}
	
	public void clear() {
		
		SETT.ROOMS().fData.clear(mx(), my(), ins);
		if (lastUser != null) {
			for (COORDINATE c : ins.body) {
				if (ins.is(c)) {
					ROOMS().map.clear(c.x()+c.y()*SETT.TWIDTH, ins);
					if (removeFloor || dFloored.is(ROOMS().data.get(c.x(), c.y()), 1))
						SETT.FLOOR().clearer.clear(c);
				}
			}
		}
		
		lastUser = null;
		ins.area = 0;
		ins.body.setDim(0).moveX1Y1(-1, -1);
		ins.mx = -1;
		ins.my = -1;
	}
	
	public void clearAndUpdate() {
		
		SETT.ROOMS().fData.clear(mx(), my(), ins);
		
		if (lastUser != null) {
			for (COORDINATE c : ins.body) {
				if (ins.is(c)) {
					ROOMS().map.clear(c.x()+c.y()*SETT.TWIDTH, ins);
					if (removeFloor || dFloored.is(ROOMS().data.get(c.x(), c.y()), 1))
						SETT.FLOOR().clearer.clear(c);
				}
			}
		}
		
		lastUser = null;
		ins.area = 0;
		ins.body.setDim(0).moveX1Y1(-1, -1);
		ins.mx = -1;
		ins.my = -1;
	}
	
	public RoomInstanceImp room() {
		return ins;
	}
	
	public int mx() {
		return ins.mx;
	}
	
	public int my() {
		return ins.my;
	}
	
	private void error() {
		if (lastUser != null) {
			for (StackTraceElement e : els) {
				System.err.println(e);
			}
			throw new RuntimeException("In use by: " + lastUser);
			
		}
		throw new RuntimeException();
	}

	private static class Instance extends Room.RoomInstanceImp {
		

		private int area = 0;
		private Rec body = new Rec();
		private short mx,my;
		
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		protected Instance(ROOMS m, RoomBlueprint p) {
			super(m, p, true);
		}

		void setP(int tx, int ty){
			
			if (area == 0) {
				mx = (short) tx;
				my = (short) ty;
				body.setDim(1).moveX1Y1(tx, ty);
			}else {
				body.unify(tx, ty);
			}
			area++;
		}
		
		void set(int tx, int ty){
			
			setP(tx,ty);
			ROOMS().map.set(tx+ty*SETT.TWIDTH, this);
		}

		@Override
		public int area() {
			return area;
		}

		@Override
		public RECTANGLE body() {
			return body;
		}

		@Override
		public boolean is(int tile) {
			return SETT.ROOMS().map.indexGetter.get(tile) == roomI;
		}

		@Override
		protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
			SETT.ROOMS().tmpArea.error();
			return false;
		}

		@Override
		public CharSequence name(int tx, int ty) {
			return "should never be";
		}

		@Override
		protected AVAILABILITY getAvailability(int tile) {
			return null;
		}

		@Override
		public boolean destroyTileCan(int tx, int ty) {
			SETT.ROOMS().tmpArea.error();
			return false;
		}

		@Override
		public ROOM_DEGRADER degrader(int tx, int ty) {
			return null;
		}

		@Override
		public int mX() {
			return mx;
		}

		@Override
		public int mY() {
			return my;
		}

		@Override
		public SPRITE icon() {
			SETT.ROOMS().tmpArea.error();
			return null;
		}

		@Override
		public int resAmount(int ri, int upgrade) {
			return 0;
		}
		
		@Override
		public Furnisher constructor() {
			return SETT.ROOMS().tmpArea.cons;
		}

		

		@Override
		public void destroyTile(int tx, int ty) {
			SETT.ROOMS().tmpArea.error();
		}

		@Override
		public TmpArea remove(int tx, int ty, boolean scatter, Object user, boolean forced) {
			SETT.ROOMS().tmpArea.error();
			return null;
		}

	}

	@Override
	public RECTANGLE body() {
		return ins.body();
	}

	@Override
	public boolean is(int tile) {
		return ins.is(tile);
	}

	@Override
	public boolean is(int tx, int ty) {
		return ins.is(tx, ty);
	}

	@Override
	public int area() {
		return ins.area;
	}

	@Override
	public int index() {
		return ins.index();
	}

	@Override
	public int mX() {
		return ins.mX();
	}

	@Override
	public int mY() {
		return ins.mY();
	}






}
