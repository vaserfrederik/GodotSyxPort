package settlement.room.service.hygine.bath;

import java.io.IOException;

import game.time.TIME;
import init.constant.C;
import init.sprite.SPRITES;
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
import settlement.room.sprite.RoomSpriteTex;
import snake2d.CORE;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.color.OPACITY;
import snake2d.util.color.OpacityImp;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.sprite.TextureCoords;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher {

	private final ROOM_BATH blue;
	final FurnisherItemTile tileBasin;

	final FurnisherStat baths = new FurnisherStat(this, 1) {
		
		@Override
		public double get(AREA area, double fromItems) {
			return fromItems;
		}
		
		@Override
		public GText format(GText t, double value) {
			return GFORMAT.i(t, (int)value);
		}
	};
	
	final FurnisherStat relaxation = new FurnisherStat.FurnisherStatRelative(this, baths, 1.5);
	
	protected Constructor(ROOM_BATH blue, RoomInitData init) throws IOException {
		super(init, 2, 2);
		this.blue = blue;
		
		Json sp = init.data().json("SPRITES");
		
		RoomSprite spriteNormal = new RoomSpriteCombo(sp, "FRAME_COMBO") {
			
			RoomSprite spriteFloor = new RoomSpriteTex(sp, "POOL_FLOOR_TEXTURE");
			COLOR wColor = new ColorImp(init.data(), "WATER_COLOR");
			double wOp = init.data().d("WATER_OPACITY", 0, 1);
			OpacityImp opacity = new OpacityImp((int) (255*wOp*0.5));
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				spriteFloor.render(r, ShadowBatch.DUMMY, 0, it, degrade, isCandle);
				if (blue.is(it.tile())) {
					int i = SETT.ROOMS().data.get(it.tile()) & 0b01;
					if (i > 0) {
						renderB(r, s, it);
					}
				}
				return super.render(r, s, data, it, degrade, isCandle);
			}
			
			public void renderB(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it) {
				
				int x2 = it.x()+C.TILE_SIZE;
				int y2 = it.y()+C.TILE_SIZE;
				

				wColor.bind();
				opacity.bind();
				TextureCoords oo = SPRITES.textures().dis_small.get(it.tx()*C.T_PIXELS+SETT.WEATHER().wind.time.getD()*16, it.ty()*C.T_PIXELS+SETT.WEATHER().wind.time.getD()*16);
				CORE.renderer().renderSprite(it.x(), x2, it.y(), y2, oo);
				oo = SPRITES.textures().dis_small.get((it.tx()+1)*C.T_PIXELS-8*TIME.currentSecond(), (it.ty()+1)*C.T_PIXELS-8*TIME.currentSecond());
				CORE.renderer().renderSprite(it.x(), x2, it.y(), y2, oo);
				COLOR.unbind();
				OPACITY.unbind();
				
		
				
			}
		};
		
		final RoomSprite spriteWork = new RoomSprite1x1(sp, "WORK_1X1") {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				animate(0);
				if (blue.is(it.tile()) && (Crank.working(SETT.ROOMS().data.get(it.tile())))) {
					animate(1);
				}
				return super.render(r, s, data, it, degrade, isCandle);
			}
			
			
		};
		
		final RoomSprite spriteOven = new RoomSprite1x1(sp, "OVEN_1X1");
		
		final RoomSprite spriteService = new RoomSprite1x1(sp, "ENTRANCE_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == spriteNormal;
			}
			
		};
		
		final RoomSprite spritePipe = new RoomSprite1x1(sp, "PIPE_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == spriteOven || item.sprite(rx, ry) == this;
			}
		};
		
		final RoomSprite spriteBenchHead = new RoomSprite1x1(sp, "BENCH_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return d.perpendicular().orthoID() == item.rotation;
			}
		};
		
		final RoomSprite spriteBenchTail = new RoomSprite1x1(spriteBenchHead) {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return d.orthoID() == item.rotation;
			}
		};
		
		final RoomSprite spriteMisc = new RoomSprite1x1(sp, "TORCH_1X1");
		
		FurnisherItemTile ww = new FurnisherItemTile(this, true, spriteWork, AVAILABILITY.ROOM_SOLID, false).setData(Bits.CRANK);
		FurnisherItemTile ss = new FurnisherItemTile(this, true, spriteService, AVAILABILITY.AVOID_PASS, false).setData(Bits.SERVICE);
		FurnisherItemTile nn = new FurnisherItemTile(this, spriteNormal, AVAILABILITY.AVOID_PASS, false).setData(Bits.POOL);
		FurnisherItemTile mm = new FurnisherItemTile(this, spriteMisc, AVAILABILITY.ROOM_SOLID, true);
		FurnisherItemTile ov = new FurnisherItemTile(this, true, spriteOven, AVAILABILITY.ROOM_SOLID, false).setData(Bits.OVEN);
		FurnisherItemTile pi = new FurnisherItemTile(this, spritePipe, AVAILABILITY.ROOM_SOLID, false);
		FurnisherItemTile b1 = new FurnisherItemTile(this, true,spriteBenchHead, AVAILABILITY.AVOID_PASS, false).setData(Bits.BENCH);
		FurnisherItemTile b2 = new FurnisherItemTile(this, spriteBenchTail, AVAILABILITY.AVOID_PASS, false).setData(Bits.BENCH_TAIL);
		FurnisherItemTile __ = null;
		tileBasin = nn;
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,nn,nn},
			{ss,nn,nn}, 
			{mm,ov,pi},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,nn,nn}, 
			{ss,nn,nn},
			{mm,nn,nn}, 
			{pi,ov,pi},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,nn,nn},
			{ss,nn,nn},
			{ww,nn,nn},
			{mm,nn,nn}, 
			{pi,ov,pi},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{pi,nn,nn},
			{pi,nn,nn},
			{ss,nn,nn},
			{ww,nn,nn},
			{mm,nn,nn}, 
			{pi,ov,pi},
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,nn,nn,nn},
			{ss,nn,nn,nn}, 
			{pi,ov,pi,mm}, 
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,nn,nn,nn}, 
			{ss,nn,nn,nn},
			{mm,nn,nn,nn}, 
			{pi,ov,pi,mm},
		}, 4.5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{pi,nn,nn,nn},
			{pi,nn,nn,nn},
			{ww,nn,nn,nn},
			{ss,nn,nn,nn}, 
			{mm,pi,ov,pi}, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,nn,nn,nn},
			{pi,nn,nn,nn},
			{pi,nn,nn,nn},
			{ww,nn,nn,nn},
			{ss,nn,nn,nn}, 
			{mm,pi,ov,pi},
		}, 7.5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{pi,nn,nn,nn,nn}, 
			{pi,nn,nn,nn,nn},
			{mm,nn,nn,nn,nn}, 
			{pi,ov,pi,ss,ww}, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,nn,nn,nn,nn},
			{pi,nn,nn,nn,nn},
			{pi,nn,nn,nn,nn},
			{mm,nn,nn,nn,nn}, 
			{pi,ov,pi,ss,ww}, 
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{pi,nn,nn,nn,nn},
			{ov,nn,nn,nn,nn},
			{pi,nn,nn,nn,nn},
			{mm,nn,nn,nn,nn},
			{pi,nn,nn,nn,nn}, 
			{pi,ww,ss,pi,pi}, 
		}, 10);
		
		flush(1, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{b1,mm},
			{b2,__},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{b1,mm,b1},
			{b2,__,b2},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{b1,mm,b1,b1},
			{b2,__,b2,b2},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{b1,b1,mm,b1,b1},
			{b2,b2,__,b2,b2},
		}, 4);
		
		flush(3);
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
		return new BathInstance(blue, area, init);
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
//		{0,1,1,0,0,1,1,0},
//		{1,0,0,1,1,0,0,1},
//		{0,0,0,0,0,0,0,0},
//		{0,0,0,0,0,0,0,0},
//		{0,1,1,0,0,1,1,0},
//		{1,0,0,1,1,0,0,1},
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
