package settlement.room.health.physician;

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
import settlement.room.sprite.RoomSprite1xN;
import settlement.room.sprite.RoomSpriteCombo;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	private final ROOM_PHYSICIAN blue;
	final FurnisherStat workers;
	final FurnisherStat services;
	final FurnisherStat quality;
	
	static final int BIT_SERVICE = 3;
	
	protected Constructor(ROOM_PHYSICIAN blue, RoomInitData init)
			throws IOException {
		super(init, 2, 3, 88, 44);
		this.blue = blue;
		
		workers = new FurnisherStat.FurnisherStatI(this);
		services = new FurnisherStat.FurnisherStatServices(this, blue, 1);
		quality = new FurnisherStat.FurnisherStatRelative(this, services);
		
		Json js = init.data().json("SPRITES");
		RoomSprite sShelf = new RoomSprite1x1(js, "SHELF_1X1"){
			
			RoomSprite top = new RoomSprite1x1(js, "SHELF_ONTOP_1X1");
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return d.orthoID() == item.rotation;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				top.render(r, s, data, it, degrade, false);			}
		};
		RoomSprite sBunkA = new RoomSprite1xN(js, "BUNK_1X1_TOP", false);
		RoomSprite sBunkB = new RoomSprite1xN(js, "BUNK_1X1_BOTTOM", true);
		
		final RoomSprite1x1 top = new RoomSprite1x1(js, "TABLE_ONTOP_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				if (!(item.sprite(rx+d.perpendicular().x()*2, ry+d.perpendicular().y()*2) instanceof RoomSpriteCombo))
					return true;
				return false;
			}
		};
		
		RoomSprite sTable = new RoomSpriteCombo(js, "TABLE_COMBO") {
			
			
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (!SETT.LIGHTS().is(it.tx(), it.ty())) {
					top.render(r, s, SETT.ROOMS().fData.spriteData2.get(it.tile()), it, degrade, false);
				}
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};
		
		RoomSprite sStorage = new RoomSprite1x1(js, "STORAGE_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == sBunkB;
			}
		};
		
		
		RoomSprite sDummy = new RoomSprite.Dummy();

		
		final FurnisherItemTile sh = new FurnisherItemTile(
				this,
				sShelf,
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		final FurnisherItemTile ch = new FurnisherItemTile(
				this,
				sStorage,
				AVAILABILITY.ROOM_SOLID, 
				false);	
		
		final FurnisherItemTile ta = new FurnisherItemTile(
				this,
				sTable,
				AVAILABILITY.ROOM_SOLID, 
				true);	
		
		final FurnisherItemTile tt = new FurnisherItemTile(
				this,
				true,
				sTable,
				AVAILABILITY.ROOM_SOLID, 
				true);		
	
		final FurnisherItemTile b1 = new FurnisherItemTile(
				this,
				true,
				sBunkA,
				AVAILABILITY.NOT_ACCESSIBLE, 
				false).setData(BIT_SERVICE);	
		
		final FurnisherItemTile b2 = new FurnisherItemTile(
				this,
				sBunkB,
				AVAILABILITY.NOT_ACCESSIBLE, 
				false);	
		
		final FurnisherItemTile ee = new FurnisherItemTile(
				this,
				true,
				sDummy,
				AVAILABILITY.ROOM, 
				false);
		ee.noWalls = true;
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ch},
			{b1,},
			{b2,},
			{ta,},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ch,ch},
			{b1,b1},
			{b2,b2},
			{ta,ta},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{b2,b2},
			{b1,b1},
			{ta,ta},
			{b1,b1},
			{b2,b2},
			{ch,ch},
		}, 4);
		
		flush(1, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt}, 
		}, 1);
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,sh}, 
		}, 2);
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,sh,sh}, 
		}, 3);
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,sh,sh,sh}, 
		}, 4);
		new FurnisherItem(new FurnisherItemTile[][] {
			{sh,tt,sh,sh,sh}, 
		}, 5);
		new FurnisherItem(new FurnisherItemTile[][] {
			{sh,sh,tt,sh,sh,sh}, 
		}, 6);
		new FurnisherItem(new FurnisherItemTile[][] {
			{sh,sh,sh,tt,sh,sh,sh}, 
		}, 7);
		
		flush(1, 3);

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
		return new Instance(blue, area, init);
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
//		{0,1,0,0,0,0,0,0},
//		{0,1,0,0,0,0,0,0},
//		{0,1,1,1,1,1,1,0},
//		{0,1,1,1,1,1,1,0},
//		{0,1,1,1,1,1,1,0},
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
