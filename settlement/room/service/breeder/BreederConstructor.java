package settlement.room.service.breeder;

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
import settlement.room.sprite.RoomSpriteXxX;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class BreederConstructor extends Furnisher{

	private final ROOM_BREEDER blue;
	final FurnisherStat workers;
	final FurnisherStat coziness;
	static final int WORK = 1;
	
	protected BreederConstructor(ROOM_BREEDER blue, RoomInitData init)
			throws IOException {
		super(init, 2, 2);
		this.blue = blue;
		
		workers = new FurnisherStat.FurnisherStatEmployees(this);
		coziness = new FurnisherStat.FurnisherStatRelative(this, workers);
		
		Json sData = init.data().json("SPRITES");
		
		RoomSprite bug = new RoomSprite1x1(sData, "1x1_WORM") {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				int ll = (int) (it.ran()+TIME.currentSecond());
				double sp = ((ll >> 4)&3)/3.0;
				animate(sp);
				return super.render(r, s, data, it, degrade, isCandle);
			}
			
		};
		
		RoomSprite rimC = new SRim(sData, "1x1_RIM_CORNER", bug, 3);
		RoomSprite rim = new SRim(sData, "1x1_RIM_EDGE", bug, 2);
		RoomSprite mid = new RoomSprite.Imp(){
			

			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
				
				long rr = it.bigRan();
				
				for (int i = 0; i < 4; i++) {
					int ran = (int) (rr & 0xFF);
					rr = rr >> 16;
					
					if (blue.station.worm(it.tx(), it.ty(), ran)) {
						bug.render(r, s, ran, it, degrade, false);
					
						DIR d = DIR.ALL.getC((ran)&7);
						ran = ran >> 3;
						it.setOff(d.x()*C.TILE_SIZEH/2, d.y()*C.TILE_SIZEH/2);
					}
				}
				
				
				int am = blue.station.resources(it.tx(), it.ty(), it.ran());
				if (am > 0) {
					blue.indus.get(0).ins().get(0).resource.renderLaying(r, it.x(), it.y(), it.ran(), am);
				}
				
				return false;
			};
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return bug.getData(tx, ty, rx, ry, item, itemRan);
			}

			@Override
			public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				// TODO Auto-generated method stub
				return 0;
			};
			
		}.sDataSet(2);
		

		RoomSprite sRimDec = new RoomSprite1x1(sData, "1x1_RIM_DECOR"){
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				animate(blue.station.aSpeed(it.tx(), it.ty()));
				return super.render(r, s, data, it, degrade, isCandle);
			}
			
		};
		
		RoomSprite sCorner = new RoomSprite1x1(sData, "1x1_CORNER");

		RoomSprite sExtra1 = new RoomSprite1x1(sData, "1x1_DECOR");
		RoomSprite sExtra2 = new RoomSpriteXxX(sData, "2x2_DECOR", 2);
		
		final FurnisherItemTile ee = new FurnisherItemTile(
				this,
				rimC,
				AVAILABILITY.SOLID, 
				false);
		final FurnisherItemTile cc = new FurnisherItemTile(
				this,
				rim,
				AVAILABILITY.SOLID, 
				false);
		final FurnisherItemTile mm = new FurnisherItemTile(
				this,
				mid, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		final FurnisherItemTile xx = new FurnisherItemTile(
				this,
				false,
				sRimDec, 
				AVAILABILITY.SOLID, 
				false);
		
		final FurnisherItemTile ww = new FurnisherItemTile(
				this,
				true,
				sRimDec, 
				AVAILABILITY.AVOID_PASS, 
				false);
		
		ww.setData(WORK);
		
		
		final FurnisherItemTile __ = new FurnisherItemTile(
				this,
				sCorner, 
				AVAILABILITY.ROOM_SOLID, 
				true);
		
		final FurnisherItemTile ex = new FurnisherItemTile(
				this,
				sExtra2, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		final FurnisherItemTile e1 = new FurnisherItemTile(
				this,
				sExtra1, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,xx,xx,xx,__}, 
			{ww,ee,cc,ee,ww}, 
			{ww,cc,mm,cc,ww}, 
			{ww,ee,cc,ee,ww}, 
			{__,xx,xx,xx,__}, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,xx,xx,xx,__}, 
			{ww,ee,cc,ee,ww}, 
			{ww,cc,mm,cc,ww}, 
			{ww,cc,mm,cc,ww}, 
			{ww,ee,cc,ee,ww}, 
			{__,xx,xx,xx,__}, 
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,xx,xx,xx,__}, 
			{ww,ee,cc,ee,ww}, 
			{ww,cc,mm,cc,ww}, 
			{ww,cc,mm,cc,ww}, 
			{ww,cc,mm,cc,ww}, 
			{ww,ee,cc,ee,ww}, 
			{__,xx,xx,xx,__}, 
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,xx,xx,xx,xx,__}, 
			{ww,ee,cc,cc,ee,ww}, 
			{ww,cc,mm,mm,cc,ww}, 
			{ww,cc,mm,mm,cc,ww}, 
			{ww,cc,mm,mm,cc,ww}, 
			{ww,ee,cc,cc,ee,ww}, 
			{__,ww,ww,ww,ww,__}, 
		}, 14);
		
		flush(3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{e1},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ex,ex},
			{ex,ex},
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
		return new BreederInstance(blue, area, init);
	}
	
	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	
	@Override
	public boolean isHeavy() {
		return true;
	}
	
	private class SRim extends RoomSprite1x1 {

		private final RoomSprite worm;
		private final int dirOff;
		
		public SRim(Json json, String key, RoomSprite worm, int dirOff) throws IOException {
			super(json, key);
			this.worm = worm;
			this.dirOff = dirOff;
			sData(1);
			
		}
		
		@Override
		protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
			rx -= d.x();
			ry -= d.y();
			d = d.next(dirOff);
			rx += d.x();
			ry += d.y();
			
			return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 2;
		}
		
		private final Coo coo = new Coo();
		
		@Override
		public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
			COORDINATE c = SETT.ROOMS().fData.itemX1Y1(it.tx(), it.ty(), coo);
			if (c != null) {
				it.ranOffset(c.x()-it.tx(), c.y()-it.ty());
			}
			super.render(r, s, data, it, degrade, false);
		};
		
		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
			
			long ran = it.bigRan();
			
			DIR d = rot(data);
			d = d.next(dirOff);
			
			if (blue.station.worm(it.tx(), it.ty(), (int)ran)) {
				it.setOff(d.x()*C.TILE_SIZEH/2, d.y()*C.TILE_SIZEH/2);
				worm.render(r, s, (int)ran, it, degrade, false);
			}
			ran = ran >> 32;
			if (blue.station.worm(it.tx(), it.ty(), (int)ran)) {
				it.setOff(d.x()*C.TILE_SIZEH, d.y()*C.TILE_SIZEH);
				it.ranOffset(data, data);
				it.ranSwap();
				worm.render(r, s, (int)ran, it, degrade, false);
			}
			return false;
			
		};
		
		@Override
		public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			return worm.getData(tx, ty, rx, ry, item, itemRan);
		};
		
		@Override
		public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
				FurnisherItem item) {
			DIR d = rot(data);
			int s = 0;
			if ((dirOff & 1) != 0) {
				s |= d.next(2).mask();
				s |= d.next(4).mask();
			}
			else {
				s|= d.mask();
				s|= d.next(2).mask();
				s |= d.next(4).mask();;
			}
			SPRITES.cons().BIG.outline.render(r, s, x, y);
		}
		
	}


}
