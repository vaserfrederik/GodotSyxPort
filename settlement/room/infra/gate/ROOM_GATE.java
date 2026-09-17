package settlement.room.infra.gate;

import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TERRAIN;

import java.io.IOException;

import init.constant.C;
import init.sprite.SPRITES;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
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
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSpriteRot;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import snake2d.util.sprite.TILE_SHEET;
import util.gui.misc.GBox;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import util.spritecomposer.ComposerDests;
import util.spritecomposer.ComposerSources;
import util.spritecomposer.ComposerThings.ITileSheet;
import util.spritecomposer.ComposerUtil;
import util.text.D;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_GATE extends RoomBlueprintImp{

	public static String type = "GATEHOUSE";
	private final MConstructor constructor;
	private final Instance instance;
	
	private static CharSequence ¤¤Locked = "¤Locked. Subjects are unable to pass. Click to unlock.";
	private static CharSequence ¤¤Unlocked = "¤Unlocked. Subjects can pass, but not enemies. Click to lock gate for subjects.";

	
	static {
		D.ts(ROOM_GATE.class);
	}
	
	public ROOM_GATE(RoomInitData init, int typeIndex, String key, RoomCategorySub cat) throws IOException {
		super(init, typeIndex, key, cat);
		this.constructor = new MConstructor(this, init);
		this.instance = new Instance(init.m, this);
		
	}

	@Override
	public Room get(int tx, int ty) {
		if (ROOMS().map.get(tx, ty) == instance)
			return instance;
		return null;
	}

	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new UIRoomModule() {
			@Override
			public void hover(GBox box, Room i, int rx, int ry) {
				box.NL();
				if (locked(rx, ry)) {
					box.add(box.text().errorify().add(¤¤Locked));
				}else
					box.add(box.text().normalify2().add(¤¤Unlocked));
			}
			
		});
	}
	
	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return null;
	}
	
	@Override
	public MConstructor constructor() {
		return constructor;
	}
	
	private static Coo cooLock = new Coo();
	
	public void lock(int tx, int ty, boolean lock) {
		if (ROOMS().map.get(tx, ty) == instance) {
			FurnisherItem it = SETT.ROOMS().fData.item.get(tx, ty);
			COORDINATE x1y1 = SETT.ROOMS().fData.itemX1Y1(tx, ty, cooLock);
			if (it == null || x1y1 == null)
				return;
			
			for (int y = 0; y < it.height(); y++) {
				for (int x = 0; x < it.width(); x++) {
					if (!it.is(x, y))
						continue;
					int dx = x1y1.x()+x;
					int dy = x1y1.y()+y;
					if (!instance.isSame(tx, ty, dx, dy))
						continue;
					SETT.ROOMS().fData.spriteData2.set(dx, dy, lock?1:0);
				}
			}
			for (int y = 0; y < it.height(); y++) {
				for (int x = 0; x < it.width(); x++) {
					if (!it.is(x, y))
						continue;
					int dx = x1y1.x()+x;
					int dy = x1y1.y()+y;
					if (!instance.isSame(tx, ty, dx, dy))
						continue;
					SETT.PATH().availability.updateAvailability(dx, dy);
				}
			}
			
		}
	}
	
	public boolean locked(int tx, int ty) {
		if (ROOMS().map.get(tx, ty) == instance) {
			return SETT.ROOMS().fData.spriteData2.get(tx, ty) == 1;
		}
		return false;
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
		public ROOM_GATE blueprintI() {
			return (ROOM_GATE) blueprint();
		}
		
		@Override
		protected void removeAction(ROOMA a) {

			for (COORDINATE c : a.body()){
				if (a.is(c) && TERRAIN().TREES.isTree(c.x(), c.y()))
					TERRAIN().NADA.placeFixed(c.x(), c.y());
			}
			
			
		}
		
		@Override
		protected AVAILABILITY getAvailability(int tile) {
			if (SETT.ROOMS().fData.spriteData2.get(tile) == 1)
				return AVAILABILITY.ROOM_SOLID;
			else
				return super.getAvailability(tile);
		}

	}
	
	public static class MConstructor extends Furnisher {
		
		private final ROOM_GATE blue;

		MConstructor(ROOM_GATE blue, RoomInitData init)
				throws IOException {
			super(init, 1, 0, 144,104);
			this.blue = blue;
			makeItems(this, init);
		}
		
		public void makeItems(Furnisher f, RoomInitData init) throws IOException {
			
			TILE_SHEET sheet = new ITileSheet(init.sp(), 144,120) {
				
				@Override
				protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)  {
					s.full.init(0, 0, 1, 1, 1, 3, d.s16);
					for (int i = 0; i < 3; i++) {
						s.full.setSkip(1, i).paste(3, true);
					}
					s.full.init(s.full.body().x2(), 0, 1, 1, 2, 3, d.s16);
					for (int i = 0; i < 6; i++) {
						s.full.setSkip(1, i).paste(3, true);
					}
					
					return d.s16.saveGame();
				}
			}.get();
			
			Sprite s1 = new Sprite(sheet, 0);
			Sprite s1b = new Sprite(sheet, 0, true);
			Sprite s2 = new Sprite(sheet, s1.tileEnd);
			Sprite s3 = new Sprite(sheet, s2.tileEnd);
			
			Sprite s1_1 = new Sprite(sheet, s3.tileEnd);
			Sprite s1_2 = new Sprite(sheet, s1_1.tileEnd);
			Sprite s1_1b = new Sprite(sheet, s3.tileEnd, true);
			Sprite s1_2b = new Sprite(sheet, s1_1.tileEnd, true);
			Sprite s2_1 = new Sprite(sheet, s1_2.tileEnd);
			Sprite s2_2 = new Sprite(sheet, s2_1.tileEnd);
			Sprite s3_1 = new Sprite(sheet, s2_2.tileEnd);
			Sprite s3_2 = new Sprite(sheet, s3_1.tileEnd);
			
			
			
			Tile t1 = new Tile(s1);
			Tile t1b = new Tile(s1b);
			Tile t2 = new Tile(s2);
			Tile t3 = new Tile(s3);
			
			Tile t1_1 = new Tile(s1_1);
			Tile t1_2 = new Tile(s1_2);
			Tile t1_1b = new Tile(s1_1b);
			Tile t1_2b = new Tile(s1_2b);
			Tile t2_1 = new Tile(s2_1);
			Tile t2_2 = new Tile(s2_2);
			Tile t3_1 = new Tile(s3_1);
			Tile t3_2 = new Tile(s3_2);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{t1},
				{t1b},
			}, 1);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{t1},
				{t2},
				{t1b},
			}, 1.5);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{t1},
				{t2},
				{t3},
				{t1b},
			}, 2);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{t1_1,t1_2},
				{t2_1,t2_2},
				{t3_1,t3_2},
				{t2_1,t2_2},
				{t1_2b,t1_1b},
			}, 6);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{t1_1,t1_2},
				{t2_1,t2_2},
				{t3_1,t3_2},
				{t2_1,t2_2},
				{t3_1,t3_2},
				{t1_2b,t1_1b},
			}, 8);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{t1_1,t1_2},
				{t2_1,t2_2},
				{t3_1,t3_2},
				{t2_1,t2_2},
				{t3_1,t3_2},
				{t2_1,t2_2},
				{t1_2b,t1_1b},
			}, 10);
			
			
			f.flush(3);
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
		
		@Override
		public void putFloor(int tx, int ty, int upgrade, AREA area) {
			if (SETT.FLOOR().getter.get(tx, ty) != null)
				return;
			super.putFloor(tx, ty, upgrade, area);
		}

		
		private static class Sprite extends RoomSpriteRot {

			private int off;
			
			public Sprite(TILE_SHEET sheet, int startTile) {
				super(sheet, startTile, 1, SPRITES.cons().ROT.full);
				off = 0;
				setShadow(16, 0);
			}
			
			public Sprite(TILE_SHEET sheet, int startTile, boolean off) {
				super(sheet, startTile, 1, SPRITES.cons().ROT.full);
				this.off = 2;
				setShadow(16, 0);
			}
			
			@Override
			protected boolean joinsWith(RoomSprite s, boolean outof, int dir, DIR test, int rx, int ry,
					FurnisherItem item) {
				return DIR.ORTHO.getC(dir+off) == test;
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(SPRITE_RENDERER.DUMMY, s, data, it, degrade, false);
				return false;
			}
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, ShadowBatch.DUMMY, data, it, degrade, false);
			}
			
		}
		
		private class Tile extends FurnisherItemTile {

			public Tile(RoomSprite sprite) {
				super(MConstructor.this, false, sprite, AVAILABILITY.ENEMY, false);
			}
			
		}
		
	}
	
	@Override
	public double strength(int tile) {
		FurnisherItem it = SETT.ROOMS().fData.item.get(tile);
		return 400*Math.max(it.width(), it.height())*C.TILE_SIZE;
	}
	
	@Override
	protected void save(FilePutter saveFile) {
		// TODO Auto-generated method stub
		
	}

	@Override
	protected void load(FileGetter saveFile) throws IOException {
		// TODO Auto-generated method stub
		
	}

	@Override
	protected void clear() {
		
	}

	
}
