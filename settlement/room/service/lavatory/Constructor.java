package settlement.room.service.lavatory;

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
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.GUTIL;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

class Constructor extends Furnisher {

	private final ROOM_LAVATORY blue;
	
	final FurnisherStat latrines;
	final FurnisherStat workers;
	final FurnisherStat basins;

	protected Constructor(ROOM_LAVATORY blue, RoomInitData init) throws IOException {
		super(init, 2, 3, 88, 44);
		this.blue = blue;
		
		latrines = new FurnisherStat.FurnisherStatServices(this, blue);
		workers = new FurnisherStat.FurnisherStatEmployeesR(this, latrines, 1/8.0);
		basins = new FurnisherStat.FurnisherStatRelative(this, latrines);
		
		Json sp = init.data().json("SPRITES");
		
		RoomSprite sNick = new RoomSprite1x1(sp, "NICKNACK_1X1");
		
		RoomSprite SToil = new RoomSpriteCombo(sp, "SIT_COMBO") {
			
			private RoomSprite rim = new RoomSprite1x1(sp, "SHITHOLE_1X1") {
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return item.sprite(rx, ry) == null && (d.orthoID() == item.rotation || d.perpendicular().orthoID() == item.rotation);
				}
			};
			private RoomSprite lid = new RoomSprite1x1(sp, "SHIT_LID_1X1");
			private RoomSprite shit = new RoomSprite1x1(sp, "SHIT_1X1");
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return rim.getData(tx, ty, rx, ry, item, itemRan);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				if (blue.is(it.tile()) && Lavatory.isOpen(SETT.ROOMS().data.get(it.tile()))) {
					shit.render(r, s, getData2(it), it, degrade, isCandle);
					rim.render(r, s, getData2(it), it, degrade, isCandle);
				}else {
					lid.render(r, s, getData2(it), it, degrade, isCandle);
				}
				
				return false;
			}
			
		};
		
		RoomSprite SCentre = new RoomSpriteCombo(SToil) {
			
			private final RoomSprite top = new RoomSpriteCombo(sp, "SIT_ONTOP_COMBO") {
				
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
				}
				
			};
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				return false;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				top.render(r, s, getData2(it), it, degrade, rotates);
				if (!SETT.ROOMS().fData.candle.is(it.tile())){
					if ((GUTIL.ran2().get(it.tile()) & 0b11) == 0)
						sNick.render(r, s, 0, it, degrade, false);
				}
			};
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		}.sData(1);
		
		RoomSprite sCentreEdge = new RoomSpriteCombo(SToil) {
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (!SETT.ROOMS().fData.candle.is(it.tile()))
					sNick.render(r, s, 0, it, degrade, false);
			};
		};
		
		RoomSprite sSink = new RoomSpriteCombo(sp, "TABLE_COMBO") {
			
			private RoomSprite water = new RoomSprite1x1(sp, "BASIN_WATER_1X1");
			private RoomSprite basin = new RoomSprite1x1(sp, "BASIN_1X1") {
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return item.sprite(rx, ry) == null && (d.orthoID() == item.rotation || d.perpendicular().orthoID() == item.rotation);
				}
			};
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return basin.getData(tx, ty, rx, ry, item, itemRan);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				water.render(r, s, getData2(it), it, degrade, isCandle);
				basin.render(r, s, getData2(it), it, degrade, isCandle);
				return false;
			}
			
		};
		
		RoomSprite sTable = new RoomSpriteCombo(sSink) {
			
			@Override
			public  void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (!SETT.ROOMS().fData.candle.is(it.tile()))
					sNick.render(r, s, data, it, degrade, false);
			};
			
		};
		
		final FurnisherItemTile tt = new FurnisherItemTile(this, true, SToil, AVAILABILITY.AVOID_PASS, false).setData(Lavatory.BIT);
		final FurnisherItemTile cc = new FurnisherItemTile(this, SCentre, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile ce = new FurnisherItemTile(this, sCentreEdge, AVAILABILITY.ROOM_SOLID, true);
		final FurnisherItemTile ww = new FurnisherItemTile(this, true, sSink, AVAILABILITY.ROOM_SOLID, false).setData(Lavatory.BIT_WASH);
		final FurnisherItemTile m1 = new FurnisherItemTile(this, sTable, AVAILABILITY.ROOM_SOLID, true);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ce,cc,ce},
			{tt,tt,tt},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ce,cc,cc,ce},
			{tt,tt,tt,tt},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ce,cc,cc,cc,ce},
			{tt,tt,tt,tt,tt},
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ce,cc,cc,cc,cc,ce},
			{tt,tt,tt,tt,tt,tt},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ce,cc,cc,cc,cc,cc,ce},
			{tt,tt,tt,tt,tt,tt,tt},
		}, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,tt,tt},
			{ce,cc,ce},
			{tt,tt,tt},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,tt,tt,tt},
			{ce,cc,cc,ce},
			{tt,tt,tt,tt},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,tt,tt,tt,tt},
			{ce,cc,cc,cc,ce},
			{tt,tt,tt,tt,tt},
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,tt,tt,tt,tt,tt},
			{ce,cc,cc,cc,cc,ce},
			{tt,tt,tt,tt,tt,tt},
		}, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,tt,tt,tt,tt,tt,tt},
			{ce,cc,cc,cc,cc,cc,ce},
			{tt,tt,tt,tt,tt,tt,tt},
		}, 14);
		
		flush(1, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,m1},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,m1,ww},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,ww,m1,ww},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,ww,m1,ww,ww},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,m1},
			{ww,ww},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,m1,ww},
			{ww,m1,ww},
		},5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,ww,m1,ww},
			{ww,ww,m1,ww},
		}, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,ww,m1,ww,ww},
			{ww,ww,m1,ww,ww},
		}, 9);
		
		flush(0, 3);
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
		return new LavatoryInstance(blue, area, init);
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
//		{0,1,1,1,1,1,1,0},
//		{0,1,1,1,1,1,1,0},
//		{0,1,1,1,1,1,1,0},
//		{0,1,1,0,0,1,1,0},
//		{0,1,0,0,0,0,1,0},
//		{0,1,0,0,0,0,1,0},
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
