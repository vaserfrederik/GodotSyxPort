package settlement.room.service.food.tavern;

import java.io.IOException;

import init.constant.C;
import init.resources.RESOURCES;
import init.resources.ResGDrink;
import init.resources.ResGroup;
import settlement.main.SETT;
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
import settlement.room.sprite.RoomSpriteCombo;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.misc.CLAMP;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

class Constructor extends Furnisher {

	public static int ISTORAGE = 1;
	public static int ITABLE = 2;
	
	
	private final ROOM_TAVERN blue;
	final FurnisherStat tables;
	final FurnisherStat coziness;
	private final RoomSprite1x1 sJug;
	private final RoomSprite1x1 sFill;
	
	protected Constructor(ROOM_TAVERN blue, RoomInitData init) throws IOException {
		super(init, 3, 2, 88, 44);
		this.blue = blue;
		tables = new FurnisherStat.FurnisherStatServices(this, blue);
		coziness = new FurnisherStat.FurnisherStatRelative(this, tables);
		
		Json sp = init.data().json("SPRITES");
		
		sJug = new RoomSprite1x1(sp, "JUG_1X1");
		sFill = new RoomSprite1x1(sp, "JUG_FILL_1X1");
		
		RoomSprite sTable = new RoomSpriteCombo(sp, "TABLE_COMBO");
		
		RoomSprite sStore = new RoomSprite1x1(sp, "STORAGE_1X1") {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (SETT.ROOMS().fData.candle.is(it.tile()))
					return;
				TavernInstance i = blue.getter.get(it.tile());
				if (i != null) {
					int ran = it.ran();
					ResGroup<ResGDrink> es = RESOURCES.DRINKS();
					for (int ri = 1; ri <= 2; ri++) {
						ResGDrink res = es.all().get((ran&0x0F)%es.all().size());
						ran = ran >> 4;
						double d = blue.dist.stored(res.resource).get(i);
						d/= i.distData.maxAmount*ri;
						d*= 16;
						ran = ran >> 4;
						res.resource.renderLaying(r, it.x(), it.y(), ran, d);
					}
				}
				super.renderAbove(r, s, data, it, degrade);
			}
			
		};
		
		final RoomSprite sChair = new RoomSprite1x1(sp, "CHAIR_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry) != this;
			}
		};
		
		RoomSprite sServiceTop = new RoomSprite1x1(sp, "TABLE_DECOR_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx-d.x()*2, ry-d.y()*2) == sChair;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				
				if (blue.dist.service(it.tx(), it.ty()) == null)
					return;
				
				DIR dir = rot(data);
				int ran = it.ran();
				
				
				if (blue.dist.isWorked(it.tx(), it.ty())) {
					ResGDrink drink = RESOURCES.DRINKS().all().getC(ran);
					
					int sx = 2*C.SCALE - dir.x()*5*C.SCALE;
					int sy = 2*C.SCALE - dir.y()*5*C.SCALE;
					it.setOff(sx, sy);
					sJug.renderRandom(r, s, it, ran, 0);
					sFill.renderRandom(r, s, it, ran, degrade, drink.color);
					ran = ran >> 3;
					;
				}
				
				int used = blue.dist.usedAmount(it.tx(), it.ty());
				if (!blue.dist.isWorked(it.tx(), it.ty())) {
					used = 4;
				}
					
				if (used > 0) {
					int dd = 4*C.SCALE;
					DIR d = dir.next(-2);
					if ((ran & 1) == 1)
						d = d.perpendicular();
					ran = ran>>1;
					
					
					

					used = CLAMP.i(used, 0, 4);
					for (int i = 0; i < used; i++) {
						int sx = -d.x()*6*C.SCALE + d.x()*i*dd + (-1+(ran&3));
						ran = ran >> 2;
						int sy = -d.y()*6*C.SCALE + d.y()*i*dd + (-1+(ran&3));
						ran = ran >> 2;
						it.setOff(sx, sy);
						sJug.renderRandom(r, s, it, ran, 0);
						ran = ran >> 3;
					}
					
				}
				
			}
			
		};
		
		final RoomSprite sService = new RoomSpriteCombo(sTable) {
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sServiceTop.getData(tx, ty, rx, ry, item, itemRan);
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				sServiceTop.render(r, s, getData2(it), it, degrade, false);
				sServiceTop.renderAbove(r, s, getData2(it), it, degrade);
			}
		};
		
	
		
		final RoomSprite sBarrel = new RoomSprite1x1(sp, "MISC_1X1");
		
		final RoomSprite sTableMisc = new RoomSpriteCombo(sTable) {
			
			final RoomSprite top = new RoomSprite1x1(sp, "MISC_DECOR_1X1");
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (!SETT.ROOMS().fData.candle.is(it.tile()))
					top.render(r, s, getData2(it), it, degrade, false);
			}
		};
		
		FurnisherItemTile pl = new FurnisherItemTile (
				this,
				false,
				sService,
				AVAILABILITY.ROOM_SOLID,
				false
				);
		pl.setData(ITABLE);
		FurnisherItemTile  st = new FurnisherItemTile (
				this,
				true,
				sStore, 
				AVAILABILITY.ROOM_SOLID,
				true
				);
		st.setData(ISTORAGE);
		FurnisherItemTile  ch = new FurnisherItemTile (
				this,
				true,
				sChair, 
				AVAILABILITY.AVOID_PASS,
				false
				);
		FurnisherItemTile  mm = new FurnisherItemTile (
				this,
				false,
				sBarrel, 
				AVAILABILITY.ROOM_SOLID,
				false
				);
		FurnisherItemTile __ = null;
		FurnisherItemTile  nn = new FurnisherItemTile (
				this,
				false,
				sTableMisc, 
				AVAILABILITY.ROOM_SOLID,
				true
				);
		FurnisherItemTile  sm = new FurnisherItemTile (
				this,
				false,
				sStore, 
				AVAILABILITY.ROOM_SOLID,
				true
				);
		
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,pl,st},
			{__,ch,__},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,pl,pl,st},
			{__,ch,ch,__},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,pl,pl,pl,st},
			{__,ch,ch,ch,__},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,pl,pl,pl,pl,st},
			{__,ch,ch,ch,ch,__},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,pl,pl,pl,pl,pl,st},
			{__,ch,ch,ch,ch,ch,__},
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,pl,pl,pl,pl,pl,pl,st},
			{__,ch,ch,ch,ch,ch,ch,__},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,pl,pl,pl,pl,pl,pl,pl,st},
			{__,ch,ch,ch,ch,ch,ch,ch,__},
		}, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,pl,pl,pl,pl,pl,pl,pl,pl,st},
			{__,ch,ch,ch,ch,ch,ch,ch,ch,__},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,__},
			{st,pl,st},
			{st,pl,st},
			{__,ch,__},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,ch,__},
			{st,pl,pl,st},
			{st,pl,pl,st},
			{__,ch,ch,__},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,ch,ch,__},
			{st,pl,pl,pl,st},
			{st,pl,pl,pl,st},
			{__,ch,ch,ch,__},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,ch,ch,ch,__},
			{st,pl,pl,pl,pl,st},
			{st,pl,pl,pl,pl,st},
			{__,ch,ch,ch,ch,__},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,ch,ch,ch,ch,__},
			{st,pl,pl,pl,pl,pl,st},
			{st,pl,pl,pl,pl,pl,st},
			{__,ch,ch,ch,ch,ch,__},
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,ch,ch,ch,ch,ch,__},
			{st,pl,pl,pl,pl,pl,pl,st},
			{st,pl,pl,pl,pl,pl,pl,st},
			{__,ch,ch,ch,ch,ch,ch,__},
		}, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,ch,ch,ch,ch,ch,ch,__},
			{st,pl,pl,pl,pl,pl,pl,pl,st},
			{st,pl,pl,pl,pl,pl,pl,pl,st},
			{__,ch,ch,ch,ch,ch,ch,ch,__},
		}, 14);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,ch,ch,ch,ch,ch,ch,ch,__},
			{st,pl,pl,pl,pl,pl,pl,pl,pl,st},
			{st,pl,pl,pl,pl,pl,pl,pl,pl,st},
			{__,ch,ch,ch,ch,ch,ch,ch,ch,__},
		}, 16);
		
		flush(3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{nn},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sm,nn},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,st,nn},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,sm,nn,mm},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,mm,sm,nn,nn},
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,mm,sm,nn,nn,st},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,mm,sm,nn,nn,sm,mm},
		}, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sm,mm},
			{sm,nn},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,sm,mm},
			{mm,sm,nn},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,sm,mm,mm},
			{mm,sm,nn,nn},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,mm,sm,mm,mm},
			{mm,mm,sm,nn,nn},
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,mm,sm,mm,mm,sm},
			{mm,mm,sm,nn,nn,sm},
		}, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,mm,sm,mm,mm,sm,mm},
			{mm,mm,sm,nn,nn,sm,mm},
		}, 14);
		
		flush(3);
		
		FurnisherItemTools.makeUnder(this, sp, "CARPET_COMBO");
		
	}
	
//	void renderTable(SPRITE_RENDERER r, ShadowBatch s, int rotMask, RenderIterator it) {
//		
//		
//		int data = ROOMS().data.get(it.tile());
//		
//		int af = blue.table.amountFull(data);
//		int ae = blue.table.amountEmpty(data);
//		if (af + ae == 0)
//			return;
//		
//
//		
//		int cx = 0;
//		int cy = 0;
//		
//		int dd = 4*C.SCALE;
//		
//		int di = it.ran()% DIR.ALLC.size();
//		
//		int ran = it.ran();
//		
//		for (int i = 0; i < 9 && af > 0 || ae > 0; i++) {
//			DIR d = DIR.ALLC.get(di%DIR.ALLC.size());
//			di++;
//			
//			it.setOff(cx+d.x()*dd, cy+d.y()*dd);
//			
//			
//			if (af > 0) {
//				sJug.renderRandom(r, s, it, ran, 0);
//				sFill.renderRandom(r, s, it, ran, 0);
//				af--;
//			}else if (ae > 0) {
//				sJug.renderRandom(r, s, it, ran, 0);
//				ae--;
//			}
//			ran = ran >> 3;
//		}
//		
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
	public Room create(TmpArea area, RoomInit init) {
		return new TavernInstance(blue, area, init);
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
//		{0,1,1,1,1,1,1,0},
//		{0,0,1,0,0,1,0,0},
//		{0,0,1,0,0,1,0,0},
//		{0,0,1,0,0,1,0,0},
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
