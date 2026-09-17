package settlement.room.service.market;

import java.io.IOException;

import init.constant.C;
import init.race.RACES;
import init.race.RaceResources.RaceResource;
import init.resources.RESOURCE;
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
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	static final int MAX = 16;
	
	final FurnisherStat storage;
	
	final FurnisherStat workers;
	
	private final ROOM_MARKET blue;
	
	private static final int ST = 2;
	private static final int CR = 1;
	
	boolean isCrate(int tx, int ty) {
		return SETT.ROOMS().fData.tileData.get(tx, ty) == CR;
	}
	
	boolean isStore(int tx, int ty) {
		return SETT.ROOMS().fData.tileData.get(tx, ty) == ST;
	}
	
	protected Constructor(ROOM_MARKET blue, RoomInitData init)
			throws IOException {
		super(init, 1, 2, 88, 44);
		this.blue = blue;
		storage = new FurnisherStat.FurnisherStatServices(this, blue, 1);
		workers = new FurnisherStat.FurnisherStatEmployees(this, 0.01);
		
		Json sp = init.data().json("SPRITES");
		
		final RoomSprite spriteCrate = new RoomSprite1x1(sp, "CRATE_1X1") {
			
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, util.rendering.RenderData.RenderIterator it, double degrade, boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				MarketInstance i = blue.getter.get(it.tile());
				if (i != null) {
					int ran = it.ran();
					for (int ri = 1; ri <= 2; ri++) {
						RaceResource res = RACES.res().ALL.get((ran&0x0FF)% RACES.res().ALL.size());
						ran = ran >> 4;
						double d = blue.dist.stored(res.res).get(i);
						d/=i.distData.maxAmount*ri;
						d*= 16;
						ran = ran >> 4;
						res.res.renderLaying(r, it.x(), it.y(), ran, d);
					}
				}
//				mid.render(r, s, data, it, degrade, isCandle);
				return false;
				
			}
		};
		final RoomSprite spriteStall = new RoomSpriteCombo(sp, "STALL_BOTTOM_COMBO") {
			
			RoomSprite1x1 top = new RoomSprite1x1(sp, "STALL_TOP_1X1") {
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return j(tx, ty, rx, ry, d, item);
				}
			};
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == this;
			}
			
			private boolean j(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				if (item.width() == 1 || item.height() == 1)
					return d.id() == item.rotation;
				
				if ((DIR.ORTHO.get(item.rotation).x() * d.x() != 0 || DIR.ORTHO.get(item.rotation).y() * d.y() != 0) && item.sprite(rx, ry) == this)
					return true;
				
				return false;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				top.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, util.rendering.RenderData.RenderIterator it, double degrade, boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				MarketInstance ins = blue.getter.get(it.tile());
				if (blue.dist.isWorked(it.tx(), it.ty())) {
					long ran = it.bigRan();
					DIR dir = top.rot(data);
					
					int dim = C.TILE_SIZE/6;
					
					
					int x1 = it.x()+C.TILE_SIZEH-(C.TILE_SIZEH-dim)*dir.next(2).x();
					int y1 = it.y()+C.TILE_SIZEH-(C.TILE_SIZEH-dim)*dir.next(2).y();
					x1 -= dim*dir.x();
					y1 -= dim*dir.y();
					int start = ((int)ran)%6;
					ran = ran >> 3;
					for (int i = 0; i < 6; i++) {
						int pos = i+start;
						pos %= 6;
						int x = x1 + dir.next(2).x()*pos*dim;
						int y = y1 + dir.next(2).y()*pos*dim;
						RESOURCE res = blue.dist.all.get(((int)ran&0x0F)%blue.dist.all.size());
						
						ran = ran >> 4;
						
						double d = blue.dist.stored(res).get(ins);
						if (d > (ran&0b111)/(double)0b111)
							res.renderOneC(r, x, y, (int)ran);
						ran = ran >> 2;
					}
				}
				return false;
				
			}
		};
		
		final RoomSprite spriteMisc = new RoomSprite1x1(sp, "MISC_BOTTOM_1X1") {
			RoomSprite1x1 top = new RoomSprite1x1(sp, "MISC_1X1");
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (!SETT.ROOMS().fData.candle.is(it.tile()))
					top.renderRandom(r, s, it, it.ran(), degrade);
			};
		};
		
		final RoomSprite sFrame = new RoomSpriteCombo(sp, "CARPET_COMBO") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == this;
			};
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
		};

		
		FurnisherItemTile cr = new FurnisherItemTile(
				this,
				false,
				spriteCrate, 
				AVAILABILITY.SOLID, 
				false).setData(ST);
		
		FurnisherItemTile st = new FurnisherItemTile(
				this,
				false,
				spriteStall, 
				AVAILABILITY.SOLID, 
				false).setData(CR);
		
		FurnisherItemTile mm = new FurnisherItemTile(
				this,
				false,
				spriteMisc, 
				AVAILABILITY.SOLID, 
				true);
	
		
		FurnisherItemTile __ = new FurnisherItemTile(
				this,
				false,
				sFrame, 
				AVAILABILITY.ROOM, 
				false);

		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,st,st,st,mm},
			{__,__,__,__,__},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,st,st,st,st,mm},
			{__,__,__,__,__,__},
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,st,st,st,st,st,mm},
			{__,__,__,__,__,__,__},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,st,st,st,st,st,st,mm},
			{__,__,__,__,__,__,__,__},
		}, 7);

		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,},
			{mm,st,st,cr,},
			{cr,st,st,mm,},
			{__,__,__,__,},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__,},
			{mm,st,st,st,cr,},
			{cr,st,st,st,mm,},
			{__,__,__,__,__,},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__,__,},
			{mm,st,st,st,st,cr,},
			{cr,st,st,st,st,mm,},
			{__,__,__,__,__,__,},
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__,__,__,},
			{mm,st,st,st,st,st,cr,},
			{cr,st,st,st,st,st,mm,},
			{__,__,__,__,__,__,__,},
		}, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__,__,__,__},
			{mm,st,st,st,st,st,st,cr},
			{cr,st,st,st,st,st,st,mm},
			{__,__,__,__,__,__,__,__},
		}, 14);
		
		flush(1, 3);
		
	}

	@Override
	public boolean usesArea() {
		return true;
	}

	@Override
	public boolean mustBeIndoors() {
		return false;
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
		return new MarketInstance(blue, area, init);
	}

}
