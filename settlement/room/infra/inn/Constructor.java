package settlement.room.infra.inn;

import java.io.IOException;

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
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	private final ROOM_INN blue;
	
	final FurnisherStat beds = new FurnisherStat.FurnisherStatI(this, 1);
	final FurnisherStat coziness = new FurnisherStat.FurnisherStatRelative(this, beds);
	final FurnisherStat workers = new FurnisherStat.FurnisherStatI(this);
	final FurnisherItemTile cc;
	private final Floor floor2;
	
	public static final int IHEAD = 1;
	public static final int ITAIL = 2;
	private static final int IWALL = 3;
	private final RoomSpriteCombo walls;
	
	FurnisherItemGroup mgroup;

	protected Constructor(ROOM_INN blue, RoomInitData init)
			throws IOException {
		super(init, 3, 3, 88, 44);
		this.blue = blue;
		floor2 = SETT.FLOOR().map.get(init.data().value("FLOOR2"), init.data());
		
		Json sp = init.data().json("SPRITES");
		
		RoomSprite sHead = new RoomSprite1xN(sp, "BED_UNMADE_HEAD_1X1", false) {
			
			final RoomSprite made = new RoomSprite1xN(sp, "BED_MADE_HEAD_1X1", false);
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				
				int x = it.tx() + offX(data);
				int y = it.ty() + offY(data);
				if (blue.is(it.tile()) && ABed.isUnmade(x, y))
					return super.render(r, s, data, it, degrade, isCandle);
				return made.render(r, s, data, it, degrade, isCandle);
			}
			
		};

		RoomSprite sTail = new RoomSprite1xN(sp, "BED_UNMADE_TAIL_1X1", true) {
			
			final RoomSprite made = new RoomSprite1xN(sp, "BED_MADE_TAIL_1X1", true);
			final RoomSprite top = new RoomSprite1x1(sp, "BED_CLAIMED_1X1");
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				
				int x = it.tx() + offX(data);
				int y = it.ty() + offY(data);
				if (blue.is(it.tile()) && ABed.isUnmade(x, y)) {
					super.render(r, s, data, it, degrade, isCandle);
					if (blue.is(it.tile()) && ABed.isClaimed(x, y))
						top.render(r, s, getData2(it), it, degrade, false);
					return false;
				}
				return made.render(r, s, data, it, degrade, isCandle);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {

				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		
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
					return !item.is(rx, ry);
				}
				return DIR.ORTHO.get(item.rotation) == d;
			}
			
		};
		
		RoomSprite sChest = new RoomSprite1x1(sp, "CHEST_1X1");
		
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
		
		walls = new RoomSpriteCombo(sp, "WALLS_COMBO");
		
		RoomSprite sNick = new RoomSprite1x1(sp, "NICKNACK_1X1");	
		
		final FurnisherItemTile h1 = new FurnisherItemTile(
				this,
				sHead,
				AVAILABILITY.AVOID_LIKE_FUCK, 
				false).setData(IHEAD).setData(IHEAD);
		final FurnisherItemTile t1 = new FurnisherItemTile(
				this,
				sTail, 
				AVAILABILITY.AVOID_LIKE_FUCK, 
				false).setData(ITAIL).setData(ITAIL);
		cc = new FurnisherItemTile(
				this,
				sNone, 
				AVAILABILITY.ROOM, false).setData(IWALL);
		final FurnisherItemTile ta = new FurnisherItemTile(
				this,
				sTable, 
				AVAILABILITY.ROOM_SOLID, true).setData(IWALL);
		
		final FurnisherItemTile ss = new FurnisherItemTile(
				this,
				true,
				sNone, 
				AVAILABILITY.ROOM, 
				false);
		
		final FurnisherItemTile sh = new FurnisherItemTile(
				this,
				sShelf,
				AVAILABILITY.ROOM_SOLID, 
				false).setData(IHEAD).setData(IWALL);
		
		final FurnisherItemTile ch = new FurnisherItemTile(
				this,
				sChest,
				AVAILABILITY.ROOM_SOLID, 
				false).setData(IHEAD).setData(IWALL);
		
		final FurnisherItemTile nt = new FurnisherItemTile(
				this,
				sTable, 
				AVAILABILITY.ROOM_SOLID, 
				true);
		
		final FurnisherItemTile ni = new FurnisherItemTile(
				this,
				sNick, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		

		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ch,h1}, 
			{ta,cc,t1},
			{sh,ss,sh},
		}, 1);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ch,h1,ta,ch,h1}, 
			{ta,cc,t1,ta,cc,t1},
			{sh,ss,sh,sh,ss,sh},
		}, 2);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ch,h1,ta,ch,h1,ta,ch,h1}, 
			{ta,cc,t1,ta,cc,t1,ta,cc,t1},
			{sh,ss,sh,sh,ss,sh,sh,ss,sh},
		}, 3);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ch,h1,ta,ch,h1,ta,ch,h1,ta,ch,h1}, 
			{ta,cc,t1,ta,cc,t1,ta,cc,t1,ta,cc,t1},
			{sh,ss,sh,sh,ss,sh,sh,ss,sh,sh,ss,sh},
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
			{ni,nt,nt,ni,sh}, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ni,nt,nt,ni,sh,sh}, 
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ni,nt,nt,ni,sh,sh,sh}, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ni,nt,nt,ni,sh,sh,sh,sh}, 
		}, 7);
		
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
		if (SETT.ROOMS().fData.tileData.get(it.tile()) != 0) {
			int m = 0;
			for (DIR d : DIR.ALL) {
				if (SETT.ROOMS().fData.tile.get(it.tx(), it.ty(), d) == cc) {
					if (!d.isOrtho()) {
						m |= d.next(-1).mask();
						m |= d.next(1).mask();
					}else {
						m |= d.mask();
						m |= d.next(-2).mask();
						m |= d.next(2).mask();
					}
				}
			}
			if (m != 0 && m != 0x0F) {
				walls.render(r, s, m, it, degrade, false);
			}
		}
		
		
	}
	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		FurnisherItem t = SETT.ROOMS().fData.item.get(tx, ty);
		if (t != null && t.group() == mgroup)
			super.putFloor(tx, ty, upgrade, area);
		else
			floor2.placeFixed(tx, ty);
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
		return new InnInstance(blue, area, init);
	}
	
	@Override
	public boolean isHeavy() {
		return true;
	}
}
