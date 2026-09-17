package settlement.room.water;

import java.io.IOException;

import init.constant.C;
import init.sprite.SPRITES;
import settlement.environment.SettEnvMap.SettEnv;
import settlement.environment.SettEnvMap.SettEnvValue;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
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
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.water.RoomPumpable.ROOM_PUMPABLE;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.Json;
import util.GUTIL;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Drain extends RoomBlueprintImp implements ROOM_PUMPABLE{

	public final Constructor constructor;
	public final DrainInstance instance;
	
	public Drain(RoomInitData init,RoomCategorySub cat) throws IOException {
		super(init, 0, "_WATERDRAIN", cat);
		this.instance = new DrainInstance(init.m, this);
		constructor = new Constructor(init);
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
	
	private final class Constructor extends Furnisher{


		protected Constructor(RoomInitData init)
				throws IOException {
			super(init, 1, 0);


			Json jj = init.data().json("SPRITES");
			final RoomSprite dd = new RoomSprite1x1(jj, "DRAIN_1X1");
			
			RoomSprite sp = new WSprite.RSprite(Drain.this, instance.pump, false) {

				@Override
				public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
					dd.render(r, s, data, it, degrade, false);
					super.renderBelow(r, s, data, it, degrade);
				}
				
			};
			
			

		
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{new FurnisherItemTile(this, false, sp, AVAILABILITY.AVOID_PASS, false)},
			}, 1);
			
			flush(1, 0);
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
				if (Drain.this.is(tx, ty, d)) {
					SETT.ROOMS().fData.spriteData2.set(tx, ty, d, 1);
				}
			}
			
			return SETT.ROOMS().map.get(tx, ty);
		}
		
		@Override
		public void renderExtra(SPRITE_RENDERER r, int x, int y, int tx, int ty, int rx, int ry, FurnisherItem item) {
			int i = 0;
			while (GUTIL.circle().radius(i) < DrainInstance.radius-2) {
				int dx = GUTIL.circle().get(i).x() + tx;
				int dy = GUTIL.circle().get(i).y() + ty;
				int rrx = x + GUTIL.circle().get(i).x()*C.TILE_SIZE;
				int rry = y + GUTIL.circle().get(i).y()*C.TILE_SIZE;
				if (SETT.ROOMS().WATER.pumpable.get(dx, dy) == instance.pump) {
					
					SPRITES.cons().BIG.dashed.render(r, 0, rrx, rry);
				}else if (GUTIL.circle().radius(i) == DrainInstance.radius-3) {
					SPRITES.cons().BIG.outline.render(r, 0, rrx, rry);
				}
				
				i++;
			}
			super.renderExtra(r, x, y, tx, ty, rx, ry, item);
		}
		
		
		@Override
		public boolean envValue(SettEnv e) {
			return e == SETT.ENV().map.WATER_SWEET;
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
		public RoomBlueprintImp blue() {
			return Drain.this;
		}

	}
	
	private static final class DrainInstance extends RoomSingleton {

		private static int radius = 10;
		private final Pump pump = new Pump();
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		private final static transient RoomAreaWrapper wrap = new RoomAreaWrapper();
		
		DrainInstance(ROOMS m, RoomBlueprint p){
			super(m, p);
			
		}
		
		protected Object readResolve() {
			  return blueprintI().instance;
		}

		@Override
		public Drain blueprintI() {
			return (Drain) blueprint();
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
			RoomPumpable.reportChange(ins.mX(), ins.mY(), radius);
		}

		@Override
		protected void addAction(ROOMA ins) {
			super.removeAction(ins);
			RoomPumpable.reportChange(ins.mX(), ins.mY(), radius);
		}
		
		private class Pump extends RoomPumpable {

			@Override
			protected void drain(int tx, int ty) {
				wrap.init(DrainInstance.this, tx, ty);
				SETT.ROOMS().data.set(wrap.area(), tx, ty, 0);
				wrap.done();
			}

			@Override
			protected void pump(int tx, int ty, DIR d, int dirmask) {
				wrap.init(DrainInstance.this, tx, ty);
				int da = SETT.ROOMS().data.get(tx, ty);
				da |= d.mask();
				SETT.ROOMS().data.set(wrap.area(), tx, ty, da);
				wrap.done();
				
			}

			@Override
			protected int dirmask(int tx, int ty) {
				return SETT.ROOMS().data.get(tx, ty) & 0x0F;
			}

			@Override
			protected int radius() {
				return radius;
			}

			@Override
			protected boolean pumpsTo(int fromX, int fromY, int tx, int ty) {
				return true;
			}

			@Override
			public double irrigation(int tx, int ty) {
				return SETT.ROOMS().data.get(tx, ty) == 0 ? 0 : 1;
			}
			

			
		}

		
	}
	

	@Override
	public RoomPumpable pumpable(int tx, int ty) {
		if (is(tx, ty))
			return instance.pump;
		return null;
	}

}