package settlement.room.law.police;

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
import settlement.room.main.furnisher.FurnisherStat.FurnisherStatEfficiency;
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

final class PoliceConstructor extends Furnisher{

	private final ROOM_POLICE blue;
	
	final FurnisherStat prisoners = new FurnisherStat.FurnisherStatI(this, 1);
	final FurnisherStatEfficiency efficiency = new FurnisherStatEfficiency(this, prisoners);
	
	final static int bitWork = 0b0001;
	final static int bitService = 0b0011;
	final static int bitBed = 0b0111;
		
	private final RoomSprite table;
	
	
	protected PoliceConstructor(ROOM_POLICE blue, RoomInitData init)
			throws IOException {
		super(init, 5, 2);
		this.blue = blue;
		
		Json sp = init.data().json("SPRITES");
	
		final RoomSprite sStrap = new RoomSprite1x1(sp, "BED_STRAP_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return DIR.ORTHO.get(item.rotation) == d;
			}
		};
		
		final RoomSprite sBenchA = new RoomSprite1xN(sp, "BED_1X1A", true) {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				sStrap.render(r, s, getData2(it), it, degrade, false);
				super.renderAbove(r, s, data, it, degrade);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sStrap.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		
		final RoomSprite sBenchB = new RoomSprite1xN(sp, "BED_1X1A", false) {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (SETT.ENTITIES().getAtTile(it.tx(), it.ty()) != null)
					sStrap.render(r, s, getData2(it), it, degrade, false);
				super.renderAbove(r, s, data, it, degrade);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sStrap.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};

		final RoomSprite sChair = new RoomSprite1x1(sp, "CHAIR_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return DIR.ORTHO.get(item.rotation) == d;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (SETT.ENTITIES().getAtTile(it.tx(), it.ty()) != null)
					sStrap.render(r, s, getData2(it), it, degrade, false);
				
				super.renderAbove(r, s, data, it, degrade);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sStrap.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};
		
		final RoomSprite sCage1 = new RoomSprite1x1(sp, "CAGE_A_1X1") {
			
			final RoomSprite roof = new RoomSprite1x1(sp, "CAGE_A_TOP_1X1");
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return DIR.ORTHO.get(item.rotation) == d;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
				roof.render(r, s, data, it, degrade, false);


			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
		};
		
		final RoomSprite sCage2 = new RoomSprite1x1(sp, "CAGE_B_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return DIR.ORTHO.get(item.rotation) == d;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
		};
		
		final RoomSprite sLatch = new RoomSprite1x1(sp, "CAGE_LATCH_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return DIR.ORTHO.get(item.rotation) == d;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				DIR d = rot(data).perpendicular();
				if (SETT.ENTITIES().getAtTile(it.tx()+d.x(), it.ty()+d.y()) != null)
					return;
				super.render(r, s, data, it, degrade, false);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
		};
		
		
		table = new RoomSpriteCombo(sp, "TABLE_COMBO") {
			
			final RoomSprite1x1 top = new RoomSprite1x1(sp, "NICKNACK_1X1");
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (!SETT.ROOMS().fData.candle.is(it.tile())) {
					top.render(r, s, data, it, degrade, false);
				}
			}
			
		};
		
		FurnisherItemTile __ = new FurnisherItemTile(this, true, RoomSprite.DUMMY, AVAILABILITY.AVOID_PASS, false);
		FurnisherItemTile tt = new FurnisherItemTile(this, table, AVAILABILITY.ROOM_SOLID, true).setData(bitWork);
		
		{
			FurnisherItemTile ss = new FurnisherItemTile(this, sBenchB, AVAILABILITY.NOT_ACCESSIBLE, false);
			FurnisherItemTile sh = new FurnisherItemTile(this, sBenchA, AVAILABILITY.AVOID_PASS, false).setData(bitBed|bitService);
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,},
				{tt,ss,tt,},
			}, 1);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,sh,__,},
				{tt,ss,tt,ss,tt,},
			}, 2);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,sh,__,sh,__,},
				{tt,ss,tt,ss,tt,ss,tt,},
			}, 3);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,sh,__,sh,__,sh,__,},
				{tt,ss,tt,ss,tt,ss,tt,ss,tt,},
			}, 4);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,sh,__,sh,__,sh,__,sh,__,},
				{tt,ss,tt,ss,tt,ss,tt,ss,tt,ss,tt,},
			}, 5);
			
			flush(3);
			
		}
		
		{
			FurnisherItemTile ss = new FurnisherItemTile(this, sChair, AVAILABILITY.NOT_ACCESSIBLE, false).setData(bitService);

			new FurnisherItem(new FurnisherItemTile[][] {
				{__,__,__,},
				{tt,ss,tt,},
			}, 1);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,__,__,__,__,},
				{tt,ss,tt,ss,tt,},
			}, 2);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,__,__,__,__,__,__,},
				{tt,ss,tt,ss,tt,ss,tt,},
			}, 3);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,__,__,__,__,__,__,__,__,},
				{tt,ss,tt,ss,tt,ss,tt,ss,tt,},
			}, 4);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,__,__,__,__,__,__,__,__,__,__,},
				{tt,ss,tt,ss,tt,ss,tt,ss,tt,ss,tt,},
			}, 5);
			
			flush(3);
		}
		
		FurnisherItemTile sh = new FurnisherItemTile(this, true, sLatch, AVAILABILITY.AVOID_PASS, false);
		
		{
			FurnisherItemTile ss = new FurnisherItemTile(this, sCage1, AVAILABILITY.NOT_ACCESSIBLE, false).setData(bitService);

			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,},
				{tt,ss,tt,},
			}, 1);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,sh,__,},
				{tt,ss,tt,ss,tt,},
			}, 2);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,sh,__,sh,__,},
				{tt,ss,tt,ss,tt,ss,tt,},
			}, 3);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,sh,__,sh,__,sh,__,},
				{tt,ss,tt,ss,tt,ss,tt,ss,tt,},
			}, 4);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,sh,__,sh,__,sh,__,sh,__,},
				{tt,ss,tt,ss,tt,ss,tt,ss,tt,ss,tt,},
			}, 5);
			
			flush(3);
		}
		
		{
			FurnisherItemTile ss = new FurnisherItemTile(this, sCage2, AVAILABILITY.NOT_ACCESSIBLE, false).setData(bitService);

			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,},
				{tt,ss,tt,},
			}, 1);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,sh,__,},
				{tt,ss,tt,ss,tt,},
			}, 2);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,sh,__,sh,__,},
				{tt,ss,tt,ss,tt,ss,tt,},
			}, 3);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,sh,__,sh,__,sh,__,},
				{tt,ss,tt,ss,tt,ss,tt,ss,tt,},
			}, 4);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,sh,__,sh,__,sh,__,sh,__,sh,__,},
				{tt,ss,tt,ss,tt,ss,tt,ss,tt,ss,tt,},
			}, 5);
			
			flush(3);
		}
		
		tt = new FurnisherItemTile(this, true, table, AVAILABILITY.ROOM_SOLID, true).setData(bitWork);
		
		{
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,},
			}, 1);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,},
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
				{tt,},
				{tt,},
			}, 2);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,},
				{tt,tt,},
			}, 4);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,tt,},
				{tt,tt,tt,},
			}, 6);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,tt,tt,},
				{tt,tt,tt,tt,},
			}, 8);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,tt,tt,tt,},
				{tt,tt,tt,tt,tt,},
			}, 10);
			
			flush(3);
		}
		
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
	public boolean mustBeOutdoors() {
		return false;
	}
	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new PoliceInstance(blue, area, init);
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
//		{0,0,0,0,0,0,0,0},
//		{0,1,1,0,0,1,1,0},
//		{1,0,0,1,1,0,0,1},
//		{1,0,0,1,1,0,0,1},
//		{0,1,1,0,0,1,1,0},
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
