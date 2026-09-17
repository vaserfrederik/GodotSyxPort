package settlement.room.service.food.canteen;

import java.io.IOException;

import init.constant.C;
import init.resources.Meal;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.resources.ResG;
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
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{
	
	final FurnisherStat guests;
	final FurnisherStat workers;
	final FurnisherStat tables;
	private final ROOM_CANTEEN blue;
	private final RoomSprite sPlate;
	

	protected Constructor(ROOM_CANTEEN blue, RoomInitData init)
			throws IOException {
		super(init, 2, 3, 88, 44);
		this.blue = blue;
		guests = new FurnisherStat.FurnisherStatServices(this, blue, 1);
		workers = new FurnisherStat.FurnisherStatEmployees(this, 1);
		tables = new FurnisherStat.FurnisherStatRelative(this, guests);
		
		Json sp = init.data().json("SPRITES");
		
		sPlate = new RoomSprite1x1(sp, "PLATE_1X1");
		
		final RoomSprite spriteOven = new RoomSpriteCombo(sp, "TABLE_COMBO") {
			
			final RoomSprite1x1 beneath = new RoomSprite1x1(sp, "OVEN_BENEATH_1X1");
			final RoomSprite1x1 beneath_used = new RoomSprite1x1(sp, "OVEN_BENEATH_USED_1X1");
			final RoomSprite1x1 oven = new RoomSprite1x1(sp, "OVEN_1X1") {
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return item.get(rx, ry) == null;
				}
			};
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				beneath.render(r, s, getData2(it), it, degrade, isCandle);
				CanteenInstance i = blue.getter.get(it.tile());
				if (i == null)
					return false;
				SWork o = blue.job.get(it.tx(), it.ty());
				if (o == null)
					return false;
				if (o.hasCoal()) {
					beneath_used.render(r, s, getData2(it), it, degrade, isCandle);
				}
				if (o.res() != null) {
					o.res().resource.renderOne(r, it.x(), it.y(), it.ran());					
				}
				return false;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				oven.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return oven.getData(tx, ty, rx, ry, item, itemRan);
			}

		};
		
		
		final RoomSprite spriteFood = new RoomSpriteCombo(sp, "TABLE_COMBO") {
			
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (SETT.ROOMS().fData.candle.is(it.tile()))
					return;
				CanteenInstance ins = blue.getter.get(it.tile());
				if (ins == null)
					return;
				
				int ran = it.ran();
				int dist = C.TILE_SIZE/3;
				int ri = ran%RESOURCES.EDI().all().size();
				for (int i = 0; i < 9; i++) {
					
					ResG e = RESOURCES.EDI().all().get(ri%RESOURCES.EDI().all().size());
					
					double a = (double)0x07*ins.amount(e)/ins.maxAmount;
					if (a > 0 && a >= (ran&0x07)) {
						DIR dir = DIR.ALLC.get(ri%DIR.ALLC.size());
						it.setOff(dir.x()*dist, dir.y()*dist);
						renderDish(r, s, e.resource, it, ran);
					}
					ri++;
					ran = ran >>> 3;
				}
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return super.joins(tx, ty, rx, ry, d, item) || item.sprite(rx, ry) == spriteOven;
			}

		};
		
		final RoomSprite spriteMisc = new RoomSpriteCombo(spriteFood) {
			
			private final RoomSprite top = new RoomSprite1x1(sp, "TOOLS_1X1");
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (SETT.ROOMS().fData.candle.is(it.tile()))
					return;
				top.render(r, s, 0, it, degrade, false);
			}
		};
		
		final RoomSprite spriteChair = new RoomSprite1x1(sp, "STOOL_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteCombo;
			}
		};
		final RoomSprite spriteTableDec = new RoomSpriteCombo(spriteFood) {
			private final RoomSprite top = new RoomSprite1x1(sp, "DECOR_1X1");
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (SETT.ROOMS().fData.candle.is(it.tile()))
					return;
				top.render(r, s, 0, it, degrade, false);
			}
			
		};
		final RoomSprite spriteTable = new RoomSpriteCombo(spriteFood) {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (SETT.ROOMS().fData.candle.is(it.tile()))
					return;
				if (blue.is(it.tile())) {
					int d = SETT.ROOMS().data.get(it.tile());
					if (d > 0) {
						RESOURCE res = Meal.get(d).resource;
						int am = Meal.amount(d);
						blue.chair.render(r, s, rotMask(data), it, am, res);
						return;
					}
				}
			}
			
			
		};
		
		final FurnisherItemTile jj = new FurnisherItemTile(
				this,
				true,
				spriteOven, 
				AVAILABILITY.ROOM_SOLID, 
				false).setData(SWork.I);
		
		final FurnisherItemTile ss = new FurnisherItemTile(
				this,
				true,
				spriteFood, 
				AVAILABILITY.ROOM_SOLID, 
				false).setData(SService.I);
		
		final FurnisherItemTile se = new FurnisherItemTile(
				this,
				false,
				spriteFood, 
				AVAILABILITY.ROOM_SOLID, 
				true);
		
		final FurnisherItemTile mm = new FurnisherItemTile(
				this,
				false,
				spriteMisc, 
				AVAILABILITY.ROOM_SOLID, 
				true);
		
		final FurnisherItemTile st = new FurnisherItemTile(
				this,
				true,
				spriteChair, 
				AVAILABILITY.PENALTY4, 
				false).setData(SChair.I);
		
		final FurnisherItemTile ta = new FurnisherItemTile(
				this,
				false,
				spriteTableDec, 
				AVAILABILITY.ROOM_SOLID, 
				true);
		
		final FurnisherItemTile ts = new FurnisherItemTile(
				this,
				false,
				spriteTable, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,jj,mm},
			{se,ss,se},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,jj,jj,mm},
			{se,ss,ss,se},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,jj,jj,jj,mm},
			{se,ss,ss,ss,se},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,jj,jj,jj,jj,mm},
			{se,ss,ss,ss,ss,se},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,jj,jj,jj,jj,jj,mm},
			{se,ss,ss,ss,ss,ss,se},
		}, 5);
	
		flush(1, 3);
		
		FurnisherItemTile __ = null;
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ts,ta},
			{__,st,__},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ts,ts,ta},
			{__,st,st,__},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ts,ts,ts,ta},
			{__,st,st,st,__},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ts,ts,ts,ts,ta},
			{__,st,st,st,st,__},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ts,ts,ts,ts,ts,ta},
			{__,st,st,st,st,st,__},
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ts,ts,ts,ts,ts,ts,ta},
			{__,st,st,st,st,st,st,__},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ta,ts,ts,ts,ts,ts,ts,ts,ta},
			{__,st,st,st,st,st,st,st,__},
		}, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,st,__},
			{ta,ts,ta},
			{ta,ts,ta},
			{__,st,__},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,st,st,__},
			{ta,ts,ts,ta},
			{ta,ts,ts,ta},
			{__,st,st,__},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,st,st,st,__},
			{ta,ts,ts,ts,ta},
			{ta,ts,ts,ts,ta},
			{__,st,st,st,__},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,st,st,st,st,__},
			{ta,ts,ts,ts,ts,ta},
			{ta,ts,ts,ts,ts,ta},
			{__,st,st,st,st,__},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,st,st,st,st,st,__},
			{ta,ts,ts,ts,ts,ts,ta},
			{ta,ts,ts,ts,ts,ts,ta},
			{__,st,st,st,st,st,__},
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,st,st,st,st,st,st,__},
			{ta,ts,ts,ts,ts,ts,ts,ta},
			{ta,ts,ts,ts,ts,ts,ts,ta},
			{__,st,st,st,st,st,st,__},
		}, 12);
		
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
	public RoomBlueprintImp blue() {
		return blue;
	}
	
//	private final FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][] {
//		{0,0,1,0,1,0,1,0},
//		{0,0,1,0,1,0,1,0},
//		{0,0,1,1,1,1,1,0},
//		{0,0,0,1,1,1,0,0},
//		{0,0,0,0,1,0,0,0},
//		{0,0,0,0,1,0,0,0},
//		{0,0,0,0,1,0,0,0},
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
		return new CanteenInstance(blue, area, init);
	}
	
	public  void renderDish(SPRITE_RENDERER r, ShadowBatch shadowBatch, RESOURCE res, RenderIterator it, int ran) {
		sPlate.render(r, shadowBatch, ran, it, 0, false);
		if (res != null) {
			COLOR.WHITE50.bind();
			res.renderOne(r, it.x(), it.y(), ran);
			COLOR.unbind();
		}
	}
	
	@Override
	public boolean isHeavy() {
		return true;
	}

}
