package settlement.room.service.pleasure;

import java.io.IOException;

import init.constant.C;
import init.sprite.SPRITES;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemGroup;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherItemTools;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSprite1xN;
import settlement.room.sprite.RoomSpriteCombo;
import settlement.tilemap.floor.Floors.Floor;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.rnd.RND;
import util.GUTIL;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import view.main.VIEW;

final class Constructor extends Furnisher{

	private final ROOM_PLEASURE blue;
	
	final FurnisherStat beds = new FurnisherStat.FurnisherStatI(this, 1);
	final FurnisherStat coziness = new FurnisherStat.FurnisherStatRelative(this, beds);
	final FurnisherStat workers = new FurnisherStat.FurnisherStatI(this);
	private final FurnisherItemTile ww;
	private final Floor floor2;
	
	private static final int IIN = 1;
	public static final int ISERVICE = 2;
	private static final int IWALL = 3;
	private final RoomSpriteCombo walls;
	
	FurnisherItemGroup mgroup;
	
	private final COLOR[] pixCols = new COLOR[256];

	protected Constructor(ROOM_PLEASURE blue, RoomInitData init)
			throws IOException {
		super(init, 3, 3, 88, 44);
		this.blue = blue;
		floor2 = SETT.FLOOR().map.get(init.data().value("FLOOR2"), init.data());
		
		Json sp = init.data().json("SPRITES");
		
		{
			COLOR pixCol = new ColorImp(init.data(), "COLOR_PIXEL_BASE");
			
			for (int i = 0; i < pixCols.length; i++) {
				int r = pixCol.red();
				int g = pixCol.green();
				int b = pixCol.blue();
				double hue = 0.25 + 0.5*RND.rFloat();
				
				
				r = (int) (hue*r + RND.rFloat()*5);
				g = (int) (hue*g + RND.rFloat()*5);
				b = (int) (hue*b + RND.rFloat()*5);
				pixCols[i] = new ColorImp(r, g, b);
			}
			
		}
		
		RoomSprite sHead = new RoomSprite1xN(sp, "BED_HEAD_1X1", false);
		RoomSprite sTail = new RoomSprite1xN(sp, "BED_TAIL_1X1", true);
		
		final RoomSprite stop = new RoomSprite1x1(sp, "TABLE_TOP_1X1");
		

		
		RoomSprite sTable = new RoomSpriteCombo(sp, "TABLE_COMBO") {
			
			
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (!SETT.ROOMS().fData.candle.is(it.tile()))
					stop.render(r, s, data, it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return stop.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};
		
		RoomSprite sNone = new RoomSprite() {

			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().BIG.filled.render(r, 0, x, y);
			}

			@Override
			public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return 0;
			}

			@Override
			public int sData() {
				// TODO Auto-generated method stub
				return 0;
			}

		};
		
		RoomSprite sShelf = new RoomSprite1x1(sp, "SHELF_1X1") {
			
			final RoomSprite top = new RoomSprite1x1(sp, "SHELF_TOP_1X1");
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				top.render(r, s, data, it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				
				
				if (item.width() >= 3 && item.height() >= 3) {
					
					rx -= d.x()*2;
					ry -= d.y()*2;
					
					return item.get(rx, ry) != null && item.sprite(rx, ry) == sNone;
					
				}
				return DIR.ORTHO.get(item.rotation) == d;
			}
			
			
		};
		
		RoomSprite sChest = new RoomSprite1x1(sp, "NICKNACK_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				
				
				if (item.width() >= 3 && item.height() >= 3) {
					
					rx -= d.x()*2;
					ry -= d.y()*2;
					
					return item.get(rx, ry) != null && item.sprite(rx, ry) == sNone;
					
				}
				return DIR.ORTHO.get(item.rotation) == d;
			}
		};
		

		
		walls = new RoomSpriteCombo(sp, "WALLS_COMBO");

		
		final FurnisherItemTile b1 = new FurnisherItemTile(
				this,
				sHead,
				AVAILABILITY.ROOM_SOLID, 
				false).setData(IWALL);
		final FurnisherItemTile b2 = new FurnisherItemTile(
				this,
				sTail,
				AVAILABILITY.ROOM_SOLID, 
				false).setData(IWALL);
		
		ww = new FurnisherItemTile(
				this,
				false,
				sNone, 
				AVAILABILITY.ROOM, false).setData(ISERVICE);
		
		
		final FurnisherItemTile ta = new FurnisherItemTile(
				this,
				sTable, 
				AVAILABILITY.ROOM_SOLID, true).setData(IWALL);
		
		final FurnisherItemTile __ = new FurnisherItemTile(
				this,
				true,
				sNone, 
				AVAILABILITY.ROOM, 
				false).setData(IIN);
		
		final FurnisherItemTile sh = new FurnisherItemTile(
				this,
				sShelf,
				AVAILABILITY.ROOM_SOLID, 
				false).setData(IWALL);
		
		final FurnisherItemTile ch = new FurnisherItemTile(
				this,
				sChest,
				AVAILABILITY.ROOM_SOLID, 
				false).setData(IWALL);
		
		final FurnisherItemTile ns = new FurnisherItemTile(
				this,
				sShelf,
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		final FurnisherItemTile nt = new FurnisherItemTile(
				this,
				sTable, 
				AVAILABILITY.ROOM_SOLID, 
				true);
		
		final FurnisherItemTile ni = new FurnisherItemTile(
				this,
				sChest, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		

		
		new FurnisherItem(new FurnisherItemTile[][] {
			{b1,ta,ta}, 
			{b2,ww,sh},
			{sh,__,ch},
		}, 1);
		new FurnisherItem(new FurnisherItemTile[][] {
			{b1,ta,ta,b1,ta,ta}, 
			{b2,ww,sh,b2,ww,sh},
			{sh,__,ch,sh,__,ch},
		}, 2);
		new FurnisherItem(new FurnisherItemTile[][] {
			{b1,ta,ta,b1,ta,ta,b1,ta,ta}, 
			{b2,ww,sh,b2,ww,sh,b2,ww,sh},
			{sh,__,ch,sh,__,ch,sh,__,ch},
		}, 3);
		new FurnisherItem(new FurnisherItemTile[][] {
			{b1,ta,ta,b1,ta,ta,b1,ta,ta,b1,ta,ta}, 
			{b2,ww,sh,b2,ww,sh,b2,ww,sh,b2,ww,sh},
			{sh,__,ch,sh,__,ch,sh,__,ch,sh,__,ch},
		}, 4);
		
		
		mgroup = flush(1, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{nt},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{nt,ni}, 
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ni,nt,ni}, 
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ni,nt,nt,ns}, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ni,nt,nt,ni,ns}, 
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ni,nt,nt,ni,ns,ns}, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ni,nt,nt,ni,ns,ns,ns}, 
		}, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ni,nt,nt,ni,ns,ns,ns,ns}, 
		}, 8);
		
		flush(3);
		
		FurnisherItemTools.makeUnder(this, sp, "CARPET_COMBO");
		
	}
	
//	boolean isHead(COORDINATE c, DIR d) {
//		RoomSprite s = SETT.ROOMS().fData.sprite.get(c, d);
//		return s == sHead;
//	}
//
//	boolean isTail(int tx, int ty) {
//		RoomSprite s = SETT.ROOMS().fData.sprite.get(tx, ty);
//		return s == sTail;
//	}

	@Override
	public boolean usesArea() {
		return true;
	}

	@Override
	public boolean mustBeIndoors() {
		return true;
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	
	
	
	public void aboveR(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, double degrade) {
		if (SETT.ROOMS().fData.tileData.get(it.tile()) > 0) {
			
			boolean blur = blue.bed.init(it.tx(), it.ty()) != null && blue.bed.clientUndressed.get() == 1;
			int m = 0;
			for (DIR d : DIR.ALL) {
				if (SETT.ROOMS().fData.tile.get(it.tx(), it.ty(), d) == ww) {
					if (!d.isOrtho()) {
						m |= d.next(-1).mask();
						m |= d.next(1).mask();
					}else {
						m |= d.mask();
						m |= d.next(-2).mask();
						m |= d.next(2).mask();
					}
					if (!blur) {
						ABed b = blue.bed.init(it.tx()+d.x(), it.ty()+d.y());
						if (b != null && b.clientUndressed.get() == 1)
							blur = true;
					}
				}
			}
			
			double rs = VIEW.renderSecond();
			
			if (blur) {
				long ran = GUTIL.ran2().get(it.tile());
				ran = ran <<32;
				ran |= GUTIL.ran1().get(it.tile());
				int D = C.TILE_SIZEH/2;
				for (int y = 0; y < 4; y++) {
					for (int x = 0; x < 4; x++) {
						int ci = (int)((ran&0x0FF)+(rs*10));
						pixCols[ci%0x0FF].render(r, it.x()+D*x, it.x()+D*x+D, 
								it.y()+D*y, it.y()+D*y+D);
						ran = ran>>4;
					}
				}
			}
			
			if (SETT.ROOMS().fData.tileData.get(it.tile()) > IIN) {
				if (m != 0 && m != 0x0F) {
					walls.render(r, s, m, it, degrade, false);
				}
			}
			
			
			
		}
		
		
	}
	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		FurnisherItem t = SETT.ROOMS().fData.item.get(tx, ty);
		if (t != null && t.group() == mgroup)
			floor2.placeFixed(tx, ty);
		else
			super.putFloor(tx, ty, upgrade, area);
	}
	
//	private final FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][] {
//		{0,0,0,0,0,0,0,0},
//		{0,1,1,1,1,1,1,1},
//		{0,1,1,1,1,1,0,1},
//		{0,1,1,1,1,1,0,1},
//		{0,1,1,1,1,1,0,1},
//		{0,1,1,1,1,1,0,1},
//		{0,1,1,1,1,1,1,1},
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
	public Room create(TmpArea area, RoomInit init) {
		return new PleasureInstance(blue, area, init);
	}
	
	@Override
	public boolean isHeavy() {
		return true;
	}
	

}
