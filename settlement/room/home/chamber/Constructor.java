package settlement.room.home.chamber;

import java.io.IOException;

import init.sprite.game.Sheets;
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
import settlement.room.sprite.RoomSpriteXxX;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	private final ROOM_CHAMBER blue;

	final FurnisherStat servants;
	final FurnisherStat users;
	final FurnisherItemTile bb;
	final int WORK_NEEDED = 22;
	
	
	protected Constructor(ROOM_CHAMBER blue, RoomInitData init)
			throws IOException {
		super(init, 1, 2, 88, 44);
		this.blue = blue;
		servants = new FurnisherStat.FurnisherStatI(this);
		users = new FurnisherStat.FurnisherStatI(this);
		
		Json sp = init.data().json("SPRITES");
		
		final RoomSprite1x1 snick = new RoomSprite1x1(sp, "MISC_1X1");
		
		
		RoomSprite sBed = new RoomSpriteXxX(2) {
			
			@Override
			public Sheets sheet(RenderIterator it) {
				ChamberInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null && ins.occupant() != null)
					return ins.occupant().race().home().clas(ins.occupant().indu()).masterBed.get(ins);
				return null;
			};
			
		};

		RoomSprite sBenchEnd = new RoomSprite1x1(sp, "BENCH_END_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSprite1x1;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				snick.renderRandom(r, s, it, it.ran(), degrade);
			}
			
		};
		RoomSprite sBenchMid = new RoomSprite1x1(sp, "BENCH_CENTRE_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSprite1x1;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				snick.renderRandom(r, s, it, it.ran(), degrade);
			}
		};
		RoomSprite sMantel1 = new RoomSprite1x1(sp, "MANTEL_A_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == sBenchMid;
			}
			
		};
		RoomSprite sMantel2 = new RoomSprite1x1(sp, "MANTEL_B_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == sBenchMid;
			}
		};
		RoomSprite sBedpost1 = new RoomSprite1x1(sp, "BEDPOST_A_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteXxX;
			}
		};
		RoomSprite sBedpost2 = new RoomSprite1x1(sp, "BEDPOST_B_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteXxX;
			}
		};
		
		RoomSprite sCarpets = new RoomSpriteCombo(sp, "CARPET_COMBO") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == this;
			}
		};
		RoomSprite sStatues = new RoomSpriteXxX(2) {
			
			@Override
			public Sheets sheet(RenderIterator it) {
				ChamberInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null && ins.occupant() != null)
					return ins.occupant().race().home().clas(ins.occupant().indu()).statue.get(ins);
				return null;
			}
			
		};
	
		RoomSprite sDummy = new RoomSprite.Dummy() {
			
		};
		
				
		
		bb = new FurnisherItemTile(
				this,
				sBed,
				AVAILABILITY.PENALTY4, 
				false);
		final FurnisherItemTile bc = new FurnisherItemTile(
				this,
				sBed,
				AVAILABILITY.PENALTY4, 
				false);
		final FurnisherItemTile b1 = new FurnisherItemTile(
				this,
				sBedpost1,
				AVAILABILITY.ROOM_SOLID, 
				false);
		final FurnisherItemTile b2 = new FurnisherItemTile(
				this,
				sBedpost2,
				AVAILABILITY.ROOM_SOLID, 
				false);
		final FurnisherItemTile x1 = new FurnisherItemTile(
				this,
				sBenchEnd,
				AVAILABILITY.ROOM_SOLID, 
				true);
		final FurnisherItemTile xx = new FurnisherItemTile(
				this,
				sBenchMid,
				AVAILABILITY.ROOM_SOLID, 
				true);
		final FurnisherItemTile m1 = new FurnisherItemTile(
				this,
				sMantel1,
				AVAILABILITY.ROOM_SOLID, 
				true);
		final FurnisherItemTile m2 = new FurnisherItemTile(
				this,
				sMantel2,
				AVAILABILITY.ROOM_SOLID, 
				true);
		final FurnisherItemTile cc = new FurnisherItemTile(
				this,
				sCarpets,
				AVAILABILITY.ROOM, 
				false);
		final FurnisherItemTile ss = new FurnisherItemTile(
				this,
				sStatues,
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		final FurnisherItemTile ee = new FurnisherItemTile(
				this,
				true,
				sDummy,
				AVAILABILITY.ROOM, 
				false);
		ee.noWalls = true;
		
		final FurnisherItemTile __ = new FurnisherItemTile(
				this,
				sDummy,
				AVAILABILITY.ROOM, 
				false);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{x1,cc,b1,b2,cc,x1}, 
			{xx,cc,bb,bc,cc,m1},
			{xx,cc,bc,bc,cc,m2},
			{x1,cc,__,__,cc,x1},
			{__,cc,__,__,cc,__},
			{ss,ss,__,__,ss,ss},
			{ss,ss,ee,ee,ss,ss},
		}, 1);
		
		flush(1, 3);
	}


	@Override
	public boolean usesArea() {
		return false;
	}

	@Override
	public boolean mustBeIndoors() {
		return true;
	}

	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new ChamberInstance(blue, area, init);
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
