package settlement.room.law.prison;

import java.io.IOException;

import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSpriteCombo;
import settlement.tilemap.floor.Floors.Floor;
import snake2d.Errors;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	private final ROOM_PRISON blue;
	
	final int PRISONERS_PER_CELL;
	final int WORKERS_PER_CELL;
	
	final FurnisherStat prisoners = new FurnisherStat.FurnisherStatI(this);
	final FurnisherStat guards = new FurnisherStat.FurnisherStatI(this);
	
	private final FurnisherItemTile cc;
	private final RoomSprite1x1 sCandle;
	private final Floor floor2;
	
	static final int CODE_ENTRANCE = 1;
	static final int CODE_LATRINE = 2;
	static final int CODE_FOOD = 3;
	
	protected Constructor(ROOM_PRISON blue, RoomInitData init)
			throws IOException {
		super(init, 1, 2, 88, 44);
		this.blue = blue;
		floor2 = SETT.FLOOR().map.get(init.data().value("FLOOR2"), init.data());
		
		Json sp = init.data().json("SPRITES");
		
		
		RoomSpriteCombo sWall = new RoomSpriteCombo(sp, "WALLS_COMBO");
		RoomSprite1x1 sBars = new RoomSprite1x1(sp, "BARS_1X1");
		
		RoomSprite sLatrine = new SCellOther(sWall, sBars, new RoomSprite1x1(sp, "LATRINE_EMPTY_1X1")) {
			RoomSprite1x1 full = new RoomSprite1x1(sp, "LATRINE_FULL_1X1");
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				if (blue().is(it.tile()) && Latrine.latrineUsed(SETT.ROOMS().data.get(it.tile())))
					return full.render(r, s, data, it, degrade, isCandle);
				return super.render(r, s, data, it, degrade, isCandle);
			}
			
		};
		
		RoomSprite sFood = new SCellOther(sWall, sBars, new RoomSprite1x1(sp, "FOOD_1X1")) {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				if (blue().is(it.tile())) {
					int am = Food.foodAmount(SETT.ROOMS().data.get(it.tile()));
					if (am > 1) {
						return super.render(r, s, data, it, degrade, isCandle);
					}
				}
				return false;
			}
			
		};
		

		RoomSprite sMisc = new SCellOther(sWall, sBars, new RoomSprite1x1(sp, "MISC_1X1"));
		
		RoomSprite1x1 sOpeningp = new RoomSprite1x1(sp, "OPENING_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.get(rx, ry) == null;
			}
		};
		
		RoomSprite sOpening = new SCellOther(sWall, sBars, sOpeningp) {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				sBars.render(r, s, SETT.ROOMS().fData.item.get(it.tile()).rotation, it, degrade, rotates);
				sOpeningp.render(r, s, getData2(it), it, degrade, rotates);
			}
		};
		
		sCandle = new RoomSprite1x1(sp, "CANDLE_SHELF_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.get(rx, ry) != null;
			}
		};

		
		final FurnisherItemTile c1 = new FurnisherItemTile(
				this,
				sCandle, 
				AVAILABILITY.ROOM, true);
		final FurnisherItemTile dd = new FurnisherItemTile(
				this,
				sMisc, 
				AVAILABILITY.NOT_ACCESSIBLE, false);
		final FurnisherItemTile ll = new FurnisherItemTile(
				this,
				sLatrine, 
				AVAILABILITY.NOT_ACCESSIBLE, false).setData(CODE_LATRINE);
		final FurnisherItemTile ff = new FurnisherItemTile(
				this,
				sFood, 
				AVAILABILITY.NOT_ACCESSIBLE, false).setData(CODE_FOOD);
		cc = new FurnisherItemTile(
				this,
				sMisc, 
				AVAILABILITY.ROOM, false);
		
		
		final FurnisherItemTile ss = new FurnisherItemTile(
				this,
				true,
				sOpening, 
				AVAILABILITY.ROOM, 
				false).setData(CODE_ENTRANCE);
		
		final FurnisherItemTile __ = null;
		

		
		new FurnisherItem(new FurnisherItemTile[][] {
			{dd,dd,dd,dd,dd}, 
			{dd,cc,cc,cc,dd},
			{ll,ss,ff,dd,dd},
			{__,__,__,c1,__},
		}, 1);
		
		flush(1, 3);
		
		PRISONERS_PER_CELL = (int) item(1).stat(prisoners);
		if (PRISONERS_PER_CELL <= 0 || PRISONERS_PER_CELL > 8)
			throw new Errors.GameError("Prisoner stat must be between 1-8" + " " + " " + PRISONERS_PER_CELL);
		
		WORKERS_PER_CELL = (int) item(1).stat(guards);
		if (WORKERS_PER_CELL <= 0 || WORKERS_PER_CELL > 3)
			throw new Errors.GameError("Worker stat must be between 1-3 " + " " + WORKERS_PER_CELL);
		
	}

	boolean isWithinCell(int nx, int ny, int cx, int cy) {
		
		if (SETT.ROOMS().fData.item.get(nx, ny) != null && SETT.ROOMS().fData.sprite.get(nx, ny) != sCandle && SETT.ROOMS().fData.item.get(cx, cy) != null) {
			COORDINATE c = SETT.ROOMS().fData.itemX1Y1(nx, ny, Coo.TMP);
			nx = c.x();
			ny = c.y();
			return SETT.ROOMS().fData.itemX1Y1(cx, cy, Coo.TMP).isSameAs(nx, ny);
		}
		return false;
		
		
	}

	@Override
	public boolean usesArea() {
		return true;
	}

	@Override
	public boolean mustBeIndoors() {
		return true;
	}

	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new PrisonInstance(blue, area, init);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		FurnisherItemTile t = SETT.ROOMS().fData.tile.get(tx, ty);
		if (t != null && t.sprite() != sCandle)
			floor2.placeFixed(tx, ty);
		else
			super.putFloor(tx, ty, upgrade, area);
			
		
	}
	
//	private final FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][] {
//		{0,0,0,0,0,0,0,0},
//		{0,1,0,0,0,0,0,0},
//		{0,1,0,0,0,0,0,0},
//		{0,1,1,1,1,1,1,0},
//		{0,1,1,1,1,1,1,0},
//		{0,1,0,0,0,0,1,0},
//		{0,0,0,0,0,0,0,0},
//		{0,0,0,0,0,0,0,0},
//		},
//		miniColor
//	);
//	
//	@Override
//	public COLOR miniColor(int tx, int ty) {
//		return miniC.get(tx, ty);
//	}
	
	@Override
	public boolean isHeavy() {
		return true;
	}
	
	private static class SCellOther extends RoomSpriteCombo{

		private final RoomSprite other;
		private final RoomSprite1x1 sBars;
		
		public SCellOther(RoomSpriteCombo sWall, RoomSprite1x1 sBars, RoomSprite other) throws IOException {
			super(sWall);
			this.sBars = sBars;
			this.other = other;
		}
		
		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			return other.render(r, s, getData2(it), it, degrade, isCandle);
		}
		
		@Override
		public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			return other.getData(tx, ty, rx, ry, item, itemRan);
		}
		
		@Override
		public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
			sBars.render(r, s, SETT.ROOMS().fData.item.get(it.tile()).rotation, it, degrade, rotates);
			super.render(r, s, data, it, degrade, rotates);
		}
		
	}
	

}
