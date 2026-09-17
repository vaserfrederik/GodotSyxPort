package settlement.room.health.asylum;

import java.io.IOException;

import init.sprite.SPRITES;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSprite1xN;
import settlement.room.sprite.RoomSpriteCombo;
import settlement.tilemap.floor.Floors.Floor;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	private final ROOM_ASYLUM blue;
	
	final FurnisherStat prisoners = new FurnisherStat.FurnisherStatI(this);
	final FurnisherStat guards = new FurnisherStat.FurnisherStatI(this);
	private final RoomSprite1x1 sCandle;
	private final Floor floor2;
	private final RoomSprite sWalls;
	private final RoomSprite sBars;
	private final RoomSprite sOpening;
	
	static final int CODE_ENTRANCE = 1;
	static final int CODE_FOOD = 2;
	
	protected Constructor(ROOM_ASYLUM blue, RoomInitData init)
			throws IOException {
		super(init, 1, 2, 88, 44);
		this.blue = blue;
		floor2 = SETT.FLOOR().map.get(init.data().value("FLOOR2"), init.data());
		
		Json sjson = init.data().json("SPRITES");
		
		sWalls = new RoomSpriteCombo(sjson, "WALLS_COMBO") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry) != sCandle;
			}
		};
		
		sBars = new RoomSprite1x1(sjson, "BARS_1X1");
		
		sOpening = new RoomSprite1x1(sjson, "OPENING_1X1");
		
		RoomSprite sDummy = new RoomSprite.Dummy() {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				prenderAbove(r, s, it, degrade, true);
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().BIG.filled.render(r, 0, x, y);
			}
			
		};
		
		RoomSprite sOpening = new RoomSprite.Dummy() {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				prenderAbove(r, s, it, degrade, false);
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().BIG.filled.render(r, 0, x, y);
			}
			
			
		};
		
		RoomSprite sbucket = new RoomSprite1x1(sjson, "DECOR_1X1") {
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				prenderAbove(r, s, it, degrade, true);
			};
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return DIR.ORTHO.getC(item.rotation+3) == d;
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().BIG.filled.render(r, 0, x, y);
			}
		};
		

		
		RoomSpriteCombo stablemisc = new RoomSpriteCombo(sjson, "TABLE_COMBO") {
			
			RoomSprite sTableTop = new RoomSprite1x1(sjson, "TABLE_ONTOP_1X1") {
				@Override
				public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
					prenderAbove(r, s, it, degrade, true);
				};
				
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return item.get(rx, ry) == null;
				}
				
				@Override
				public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
						FurnisherItem item) {
					SPRITES.cons().BIG.filled.render(r, 0, x, y);
				}
			};
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				prenderAbove(r, s, it, degrade, true);
			};
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sTableTop.getData(tx, ty, rx, ry, item, itemRan);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				if (blue.is(it.tile()) && blue.get(it.tx(), it.ty()).isReserved(it.tx(), it.ty())) {
					sTableTop.render(r, s, SETT.ROOMS().fData.spriteData2.get(it.tile()), it, degrade, isCandle);
				}
				sTableTop.render(r, s, SETT.ROOMS().fData.spriteData2.get(it.tile()), it, degrade, isCandle);
				return false;
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().BIG.filled.render(r, 0, x, y);
			}
		};
		
		RoomSprite stablefood = new RoomSpriteCombo(sjson, "TABLE_COMBO") {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				prenderAbove(r, s, it, degrade, true);
			};
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				if (blue.is(it.tile())) {
					int am = Food.food(SETT.ROOMS().data.get(it.tile()));
					blue.consumtion.ins().get(0).resource.renderLaying(r, it.x(), it.y(), it.ran(), am);
				}
				return false;
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().BIG.filled.render(r, 0, x, y);
			}
		};
		
		RoomSprite sbedA = new RoomSprite1xN(sjson, "BED_1X1_TOP", true) {
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				prenderAbove(r, s, it, degrade, true);
			};
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().BIG.filled.render(r, 0, x, y);
			}
		};

		RoomSprite sbedB = new RoomSprite1xN(sjson, "BED_1X1_BOTTOM", false) {
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				prenderAbove(r, s, it, degrade, true);
			};
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().BIG.filled.render(r, 0, x, y);
			}
		};
		
		sCandle = new RoomSprite1x1(sjson, "CANDLE_HOLDER_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.get(rx, ry) != null;
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().BIG.filled.render(r, 0, x, y);
			}
		};
		
		final FurnisherItemTile b1 = new FurnisherItemTile(
				this,
				sbedA, 
				AVAILABILITY.NOT_ACCESSIBLE, false);
		final FurnisherItemTile b2 = new FurnisherItemTile(
				this,
				sbedB, 
				AVAILABILITY.NOT_ACCESSIBLE, false);

		final FurnisherItemTile c1 = new FurnisherItemTile(
				this,
				sCandle, 
				AVAILABILITY.AVOID_PASS, true);
		final FurnisherItemTile oo = new FurnisherItemTile(
				this,
				sDummy, 
				AVAILABILITY.AVOID_PASS, false);
		final FurnisherItemTile ni = new FurnisherItemTile(
				this,
				sbucket, 
				AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile ta = new FurnisherItemTile(
				this,
				stablemisc, 
				AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile fo = new FurnisherItemTile(
				this,
				stablefood, 
				AVAILABILITY.ROOM_SOLID, false).setData(CODE_FOOD);
		
		final FurnisherItemTile ss = new FurnisherItemTile(
				this,
				true,
				sOpening, 
				AVAILABILITY.AVOID_PASS, 
				false).setData(CODE_ENTRANCE);
		
		final FurnisherItemTile __ = null;
		

		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ni,oo,b1}, 
			{ta,oo,b2},
			{fo,ss,ni},
			{__,__,c1},
		}, 1);
		
		flush(1, 3);
		
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
		return new AsylumInstance(blue, area, init);
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
	
	

	
	private void prenderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, double degrade, boolean wall) {
		
		FurnisherItem i = SETT.ROOMS().fData.item.get(it.tx(), it.ty());
		sBars.render(r, s, i.rotation, it, degrade, false);
		
		if (!wall) {
			sOpening.render(r, s, (i.rotation + 2)&0b011, it, degrade, false);
			return;
		}
		
		COORDINATE coo = SETT.ROOMS().fData.itemMaster(it.tx(), it.ty(), Coo.TMP);
		int mX = coo.x();
		int mY = coo.y();
		
		RoomInstance ro = SETT.ROOMS().map.instance.get(it.tx(), it.ty());
		if (ro == null)
			return;
		int m = 0;
		for (DIR d : DIR.ORTHO) {
			if (ro.is(it.tx(), it.ty(), d) && SETT.ROOMS().fData.sprite.get(it.tx(), it.ty(), d) != sCandle && SETT.ROOMS().fData.item.get(it.tx(), it.ty(), d) != null) {
				coo = SETT.ROOMS().fData.itemMaster(it.tx()+d.x(), it.ty()+d.y(), Coo.TMP);
				if (coo != null && coo.isSameAs(mX, mY))
					m |= d.mask();
			}
		}
		
		sWalls.render(r, s, m, it, degrade, false);
	}
	
	@Override
	public boolean isHeavy() {
		return true;
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
	

}
