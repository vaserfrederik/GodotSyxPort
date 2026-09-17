package settlement.room.infra.bench;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import init.sprite.game.SheetType;
import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.path.AVAILABILITY;
import settlement.path.finders.SFinderFindable;
import settlement.path.finders.SFinderRoomService;
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
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomFinderHaser;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.Json;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.LISTE;
import util.gui.misc.GBox;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import util.text.Dic;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_BENCH extends RoomBlueprintImp implements RoomFinderHaser{

	private final MConstructor constructor;
	private final Instance instance;
	
	public final SFinderRoomService finder = new SFinderRoomService("Bench") {
		private int x,y;
		
		private final FSERVICE s = new FSERVICE() {
			
			@Override
			public int y() {
				return y;
			}
			
			@Override
			public int x() {
				return x;
			}
			
			@Override
			public boolean findableReservedIs() {
				return SETT.ROOMS().fData.spriteData2.get(x, y) == 1;
			}
			
			@Override
			public boolean findableReservedCanBe() {
				return SETT.ROOMS().fData.spriteData2.get(x, y) == 0;
			}
			
			@Override
			public void findableReserveCancel() {
				if (findableReservedIs()) {
					finder.report(x, y, 1);
				}
				SETT.ROOMS().fData.spriteData2.set(x, y, 0);
			}
			
			@Override
			public void findableReserve() {
				if (!findableReservedIs()) {
					finder.report(x, y, -1);
				}
				SETT.ROOMS().fData.spriteData2.set(x, y, 1);
			}

			@Override
			public void consume() {
				findableReserveCancel();
			}
		};
		
		@Override
		public FSERVICE get(int tx, int ty) {
			if (ROOM_BENCH.this.is(tx, ty)) {
				x = tx;
				y = ty;
				return s;
			}
			return null;
		}
	};
	
	public ROOM_BENCH(RoomInitData init, RoomCategorySub cat) throws IOException {
		super(init, 0, "_BENCH", cat);
		this.constructor = new MConstructor(this, init);
		this.instance = new Instance(init.m, this);

	}

	
	@Override
	protected void save(FilePutter f) {
	}

	@Override
	protected void load(FileGetter f) throws IOException {
	}

	@Override
	protected void clear() {
	}
	
	@Override
	public Room get(int tx, int ty) {
		if (ROOMS().map.get(tx, ty) == instance)
			return instance;
		return null;
	}

	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return finder;
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new UIRoomModule() {
			@Override
			public void hover(GBox box, Room room, int rx, int ry) {
				if (upgrades().max() > 0) {
					box.NL();
					box.text(Dic.¤¤Upgrade);
					box.tab(6);
					box.add(GFORMAT.iofkInv(box.text(), room.upgrade(rx, ry), upgrades().max()));
					box.NL();
				}
			}
		});
		
	}
	

	final static class Instance extends RoomSingleton{

		
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		Instance(ROOMS m, RoomBlueprint p){
			super(m, p);
			
		}
		
		protected Object readResolve() {
			  return blueprintI().instance;
		}

		@Override
		public ROOM_BENCH blueprintI() {
			return (ROOM_BENCH) blueprint();
		}
		
		@Override
		protected void addAction(ROOMA ins) {
			for (COORDINATE c : ins.body()) {
				if (ins.is(c)) {
					SETT.ROOMS().fData.spriteData2.set(c.x(), c.y(), 0);
					blueprintI().finder.report(blueprintI().finder.get(c), 1);
				}
			}
		}
		
		@Override
		protected void removeAction(ROOMA ins) {
			for (COORDINATE c : ins.body()) {
				if (ins.is(c) && SETT.ROOMS().fData.spriteData2.get(c) == 0) {
					blueprintI().finder.report(blueprintI().finder.get(c), -1);
				}
			}
			super.removeAction(ins);
		}
		
		@Override
		public int upgrade(int tx, int ty) {
			return CLAMP.i(SETT.ROOMS().extraBit.get(mX(tx, ty), mY(tx, ty)), 0, blueprintI().upgrades().max());
		}
		
		@Override
		public void upgradeSet(int tx, int ty, int upgrade) {
			int up = CLAMP.i(upgrade, 0, blueprintI().upgrades().max());
			SETT.ROOMS().extraBit.set(tx, ty, up);
			ROOMA a = SETT.ROOMS().map.rooma.get(tx, ty);
			for (COORDINATE c : a.body()){
				if (a.is(c))
					SETT.MAINTENANCE().setChanged(c.x(), c.y());
				constructor().floor(up).placeFixed(c.x(), c.y());
			}
			
		}
		
		

	}
	
	private static class MConstructor extends Furnisher {
		
		private final ROOM_BENCH blue;
		
		MConstructor(ROOM_BENCH blue, RoomInitData init)
				throws IOException {
			super(init, 1, 0, 88, 44);
			
			this.blue = blue;
		

			
			Json sData = init.data().json("SPRITES");
			RoomSprite ssmall = new RoomSprite1x1(sData, "BENCH_1X1") {
				
				
				@Override
				public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
						boolean isCandle) {
					DIR rot = rot(data);
					data &= ~0x03;
					data |= rot.perpendicular().orthoID();
					super.render(r, s, data, it, degrade, isCandle);
					return false;
				}
				
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return d == DIR.ORTHO.get(item.rotation);
				}
				
				@Override
				public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
						FurnisherItem item) {
					SheetType.s1x1.renderOverlay(
							x, y, r, item.get(rx, ry).availability, 
							0, rotates ? data : -1, true);
				}

				
			};
			
			FurnisherItemTile tt = new FurnisherItemTile(
					this,
					true,
					ssmall,
					AVAILABILITY.AVOID_LIKE_FUCK,
					false
					);
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt},
			}, 1);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt},
			}, 2);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,tt,},
			}, 3);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,tt,tt,},
			}, 4);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,tt,tt,tt,},
			}, 5);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,tt,tt,tt,tt,},
			}, 6);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,tt,tt,tt,tt,tt,},
			}, 7);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,tt,tt,tt,tt,tt,tt,},
			}, 8);
			
			flush(3);
			
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
			return blue.instance.place(area);
		}

		@Override
		public RoomBlueprintImp blue() {
			return blue;
		}
		
	}

	public DIR benchDir(int tx, int ty, DIR d) {
		FurnisherItem it = SETT.ROOMS().fData.item.get(tx, ty);
		if (it == null)
			return d;
		return DIR.ORTHO.get(it.rotation);
	}


	@Override
	public SFinderFindable finder() {
		return finder;
	}


	@Override
	public int radius() {
		return 64;
	}

	@Override
	public boolean registersEnvironment() {
		return true;
	}
	
}
