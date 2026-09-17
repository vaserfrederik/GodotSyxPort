package settlement.room.water;

import java.io.IOException;

import settlement.environment.SettEnvMap.SettEnv;
import settlement.environment.SettEnvMap.SettEnvValue;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.overlay.Addable;
import settlement.path.AVAILABILITY;
import settlement.path.finders.SFinderFindable;
import settlement.room.main.ROOMA;
import settlement.room.main.ROOMS;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.RoomSingleton;
import settlement.room.main.TmpArea;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.util.RoomAreaWrapper;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.water.RoomPumpable.ROOM_PUMPABLE;
import snake2d.PathTile;
import snake2d.Renderer;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.LISTE;
import util.GUTIL;
import util.gui.misc.GBox;
import util.rendering.RenderData;
import util.rendering.RenderData.RenderIterator;
import util.text.D;
import view.sett.ui.room.UIRoomModule;
import view.tool.ToolPlacer;

final class Canal extends RoomBlueprintImp implements ROOM_PUMPABLE{

	public final CanalConstructor constructor;
	public final CanalInstance instance;
	
	private static CharSequence ¤¤problem = "Currently not operational. Make sure it's connected to a water pump's outlet, and that the connected pumps produce enough flow to reach it.";
	private static CharSequence ¤¤ok = "Operational";
	
	static {
		D.ts(Canal.class);
	}
	
	public Canal(RoomInitData init,RoomCategorySub cat) throws IOException {
		super(init, 0, "_WATERCANAL", cat);
		this.instance = new CanalInstance(init.m, this);
		constructor = new CanalConstructor(init);
	}

	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new UIRoomModule() {
		
			@Override
			public void hover(GBox box, Room i, int rx, int ry) {
				Canal.hover(box, rx, ry);
			}
		});
	}
	
	@Override
	public SFinderFindable service(int tx, int ty) {
		return null;
	}

	@Override
	public Furnisher constructor() {
		return constructor;
	}

	@Override
	protected void save(FilePutter file) {
		// TODO Auto-generated method stub
		
	}

	@Override
	protected void load(FileGetter file) throws IOException {
		// TODO Auto-generated method stub
		
	}

	@Override
	protected void clear() {
		// TODO Auto-generated method stub
		
	}

	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}	
	
	public static void hover(GUI_BOX box, int tx, int ty) {
		GBox b = (GBox) box;
		b.NL();
		boolean flow = SETT.ROOMS().data.get(tx, ty) != 0;
		if (!flow) {
			b.add(b.text().warnify().add(¤¤problem));
		}else
			b.add(b.text().normalify2().add(¤¤ok));
		b.NL();
		PumpGui.hoverSystem(b, tx, ty);
	}

	private final class CanalConstructor extends Furnisher{

		private final Overlay overlay = new Overlay();
		
		private final RoomSprite sp;
		protected CanalConstructor(RoomInitData init)
				throws IOException {
			super(init, 1, 0);
			
			sp = new WSprite.RSprite(Canal.this, instance.pump, true);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{new FurnisherItemTile(this, false, sp, AVAILABILITY.AVOID_PASS, false)},
			}, 1);
			
			flush(1, 0);
		}

		
		@Override
		public boolean joinsWithFloor() {
			return true;
		}

		@Override
		public boolean usesArea() {
			return false;
		}

		@Override
		public boolean mustBeIndoors() {
			return false;
		}

		@Override
		public Room create(TmpArea area, RoomInit init) {
			int tx = area.mX();
			int ty = area.my();
			instance.place(area);
			SETT.ROOMS().fData.spriteData2.set(tx, ty, 1);
			for (DIR d : DIR.ORTHO) {
				if (Canal.this.is(tx, ty, d)) {
					SETT.ROOMS().fData.spriteData2.set(tx, ty, d, 1);
				}
			}
			
			return SETT.ROOMS().map.get(tx, ty);
		}

		@Override
		public RoomBlueprintImp blue() {
			return Canal.this;
		}
		
		
		
		@Override
		public boolean envValue(SettEnv e, SettEnvValue v, int tx, int ty) {
			
			if (blue().is(tx, ty) && SETT.ROOMS().data.get(tx, ty) != 0 && e == SETT.ENV().map.WATER_SWEET) {
				v.value = 1;
				v.radius = 1;
				return true;
			}
			return false;
		}
		
		
		
		
		@Override
		public boolean envValue(SettEnv e) {
			return e == SETT.ENV().map.WATER_SWEET;
		}
		
		@Override
		public boolean removeFertility() {
			return false;
		}
		
		@Override
		public boolean isSpecialAreaPlacable() {
			return true;
		}
		
		@Override
		public void renderExtra(SPRITE_RENDERER r, int x, int y, int tx, int ty, int rx, int ry, FurnisherItem item) {
			super.renderExtra(r, x, y, tx, ty, rx, ry, item);
		}
		
		
		@Override
		public Addable overlay() {
			return overlay;
		}

	}
	
	private class Overlay extends Addable{

		public Overlay() {
			super(true, false);
		}
		
		@Override
		public void renderBelow(Renderer r, RenderIterator it) {
			if (SETT.ROOMS().construction.isser.is(it.tile()))
				return;
			double d = SETT.GROUND().MOISTURE_TOT.get(it.tile());
			if (GUTIL.flooder().hasBeenPushed(it.tx(), it.ty()))
				d += 2*(1-CLAMP.d(GUTIL.flooder().getValue(it.tx(), it.ty())/15.0, 0, 1));
			
			d = CLAMP.d(d, 0, 1);
			renderUnder(d, r, it, false);
			if (d > 0.75) {
				d = (d-0.75)*4;
				renderPluses(d, r, it);
			}
		};
		
		@Override
		public void initBelow(RenderData data) {
			
			for (COORDINATE c : data.tBounds()) {
				if (SETT.IN_BOUNDS(c))
					GUTIL.flooder().setValue2(c, 0);
			}

			AREA a = ToolPlacer.area();
			if (a.area() == 0)
				return;
			
			GUTIL.flooder().init(this);
			
			
			for (COORDINATE c : a.body()) {
				if (a.is(c) && c.distance(data.tBounds().cX(), data.tBounds().cY()) < Math.max(data.tBounds().width()/2, data.tBounds().height()/2) + 18) {
					GUTIL.flooder().pushSloppy(c, 0);
					
					
				}
			}
			
			while(GUTIL.flooder().hasMore()) {
				PathTile t = GUTIL.flooder().pollSmallest();
				if (t.getValue() >= 15)
					continue;
				
				for (DIR d : DIR.ALL) {
					if (SETT.IN_BOUNDS(t, d))
						GUTIL.flooder().pushSloppy(t, d, t.getValue()+d.tileDistance());
				}
			}
			
			
			super.initBelow(data);
		}
		
		@Override
		public void finishBelow() {
			GUTIL.flooder().done();
		}
		
	}
	
	private static class CanalInstance extends RoomSingleton {

		
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		private final static transient RoomAreaWrapper wrap = new RoomAreaWrapper();
		
		CanalInstance(ROOMS m, RoomBlueprint p){
			super(m, p);
			
		}
		
		protected Object readResolve() {
			  return blueprintI().instance;
		}

		@Override
		public Canal blueprintI() {
			return (Canal) blueprint();
		}
		
		@Override
		public ROOM_DEGRADER degrader(int tx, int ty) {
			return null;
		}
		
		@Override
		public void updateTileDay(int tx, int ty) {
			
		}
		
		@Override
		protected void removeAction(ROOMA ins) {
			super.removeAction(ins);
			RoomPumpable.reportChange(ins.mX(), ins.mY(), 0);
		}

		@Override
		protected void addAction(ROOMA ins) {
			super.removeAction(ins);
			RoomPumpable.reportChange(ins.mX(), ins.mY(), 0);
		}


		
		private RoomPumpable pump = new RoomPumpable() {
			
			@Override
			public void drain(int tx, int ty) {
				wrap.init(CanalInstance.this, tx, ty);
				SETT.ROOMS().data.set(wrap.area(), tx, ty, 0);
				wrap.done();
			}

			@Override
			public void pump(int tx, int ty, DIR d, int dirmask) {
				wrap.init(CanalInstance.this, tx, ty);
				int da = SETT.ROOMS().data.get(tx, ty);
				da |= d.mask();
				SETT.ROOMS().data.set(wrap.area(), tx, ty, da);
				wrap.done();
				if ((dirmask & 0x0F) != (dirmask(tx, ty) & 0x0F)) {
					SETT.ENV().map.setChanged(tx, ty, SETT.ENV().map.WATER_SWEET);
				}
			}
			
			@Override
			protected void pumpFail(int tx, int ty, int dirmask) {
				if (dirmask != 0)
					SETT.ENV().map.setChanged(tx, ty);
			};

			@Override
			public int dirmask(int tx, int ty) {
				return SETT.ROOMS().data.get(tx, ty) & 0x0F;
			}

			@Override
			public int radius() {
				return 0;
			}

			@Override
			protected boolean pumpsTo(int fromX, int fromY, int tx, int ty) {
				return true;
			}

			@Override
			public double irrigation(int tx, int ty) {
				return SETT.ROOMS().data.get(tx, ty) == 0 ? 0 : 1;
			};
			
		};


		
	}
	
	@Override
	public RoomPumpable pumpable(int tx, int ty) {
		if (is(tx, ty))
			return instance.pump;
		return null;
	}
}