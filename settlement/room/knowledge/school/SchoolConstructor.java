package settlement.room.knowledge.school;

import java.io.IOException;

import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherItemTools;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class SchoolConstructor extends Furnisher{

	public final FurnisherStat stations;
	public final FurnisherStat quality;
	
	private final ROOM_SCHOOL blue;
	
	static final int ISERVICE = 1;
	static final int IWORK = 2;
	
	protected SchoolConstructor(ROOM_SCHOOL blue, RoomInitData init)
			throws IOException {
		super(init, 3, 2, 88, 44);
		this.blue = blue;
		stations = new FurnisherStat.FurnisherStatServices(this, blue);
		quality = new FurnisherStat.FurnisherStatEfficiency(this, stations);
		
		Json sp = init.data().json("SPRITES");
		
		final RoomSprite sService = new RoomSprite1x1(sp, "TABLE_1X1") {
			
			final RoomSprite top = new RoomSprite1x1(sp, "BOOK_1X1") {
				
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
				}
				
			};
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (blue.is(it.tile())) {
					DIR d = DIR.ORTHO.get(getRot(data));
					FSERVICE f = blue.station.service(it.tx()+d.x(), it.ty()+d.y());
					if (f != null && (f.findableReservedCanBe() || f.findableReservedIs())) {
						top.render(r, s, getData2(it), it, degrade, false);
					}
				}
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
			
			
		}.sData(2);
		
		final RoomSprite sBench = new RoomSprite1x1(sp, "STOOL_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 2;
			}
			
		}.sData(1);

		final RoomSprite sShelf = new RoomSprite1x1(sp, "SHELF_1X1") {
			
			final RoomSprite top = new RoomSprite1x1(sp, "SHELF_TOP_1X1");
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				if (item.width() > 1 && item.height() > 1) {
					return (d == DIR.ORTHO.get(item.rotation) || d == DIR.ORTHO.get(item.rotation).perpendicular()) && item.sprite(rx, ry) == this;		
				}
				return DIR.ORTHO.get(item.rotation) == d;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				top.render(r, s, data, it, degrade, false);
			}
		};
		
		final RoomSprite sTable = new RoomSprite1x1(sService) {
			
			final RoomSprite top = new RoomSprite1x1(sp, "TABLE_TOP_1X1");
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				if (item.width() > 1 && item.height() > 1) {
					return (d == DIR.ORTHO.get(item.rotation) || d == DIR.ORTHO.get(item.rotation).perpendicular()) && item.sprite(rx, ry) == this;		
				}
				return DIR.ORTHO.get(item.rotation) == d;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (!SETT.ROOMS().fData.candle.is(it.tile())) {
					top.render(r, s, getData2(it), it, degrade, false);
				}
			}

			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};
		
		final FurnisherItemTile ss = new FurnisherItemTile(this,true, sService, AVAILABILITY.ROOM_SOLID, false);
		ss.setData(IWORK);
		final FurnisherItemTile bb = new FurnisherItemTile(this,true, sBench, AVAILABILITY.AVOID_PASS, false);
		bb.setData(ISERVICE);
		final FurnisherItemTile sh = new FurnisherItemTile(this,false, sShelf, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile ta = new FurnisherItemTile(this,false, sTable, AVAILABILITY.ROOM_SOLID, true);
		
		final FurnisherItemTile __ = null;
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ss,ta,},
			{__,bb,__,}, 
		}, 1, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ss,ss,ta,},
			{__,bb,bb,__,}, 
		}, 2, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ss,ss,ss,ta,},
			{__,bb,bb,bb,__,}, 
		}, 3, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ss,ss,ss,ss,ta,},
			{__,bb,bb,bb,bb,__,}, 
		}, 4, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ss,ss,ss,ss,ta,ss,},
			{__,bb,bb,bb,bb,__,bb,}, 
		}, 5, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ta,ss,ss,ss,ss,ta,ss,},
			{bb,__,bb,bb,bb,bb,__,bb,}, 
		}, 6, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ta,ss,ss,ss,ss,ta,ss,ss,},
			{bb,__,bb,bb,bb,bb,__,bb,bb,}, 
		}, 7, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ta,ss,ss,ss,ss,ta,ss,ss,},
			{bb,bb,__,bb,bb,bb,bb,__,bb,bb,}, 
		}, 8, 8);
		
		flush(1, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ta }, 
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ta, sh }, 
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ta, sh, sh }, 
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ta, sh, sh, sh }, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ta, sh, sh, sh, sh }, 
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ta, sh, }, 
			{ ta, sh, }, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ta, sh, sh, }, 
			{ ta, sh, sh, }, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ta, sh, sh, sh, }, 
			{ ta, sh, sh, sh, }, 
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ta, sh, sh, sh, sh }, 
			{ ta, sh, sh, sh, sh }, 
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ta, sh, sh, sh, sh, sh }, 
			{ ta, sh, sh, sh, sh, sh }, 
		}, 12);
		
		flush(3);
		
		FurnisherItemTools.makeUnder(this, sp, "CARPET_COMBO");
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
		return new SchoolInstance(blue, area, init);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	
	@Override
	public boolean isHeavy() {
		return true;
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

}
