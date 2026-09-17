package settlement.room.service.arena.pit;

import java.io.IOException;

import init.constant.C;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.furnisher.FurnisherStat.FurnisherStatI;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSpriteBoxN;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.color.OPACITY;
import snake2d.util.color.OpacityImp;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class ArenaConstructor extends Furnisher{

	private final ROOM_FIGHTPIT blue;
	
	final FurnisherStatI workers;
	final FurnisherStat spectators;
	static final int STATION = 1;
	static final int ARENA = 2;
	private final FurnisherItemTile cc;

	



	
	protected ArenaConstructor(ROOM_FIGHTPIT blue, RoomInitData init)
			throws IOException {
		super(init, 1, 2);

		this.blue = blue;
		workers = new FurnisherStatI(this);
		spectators = new FurnisherStat.FurnisherStatServices(this, blue);
		Json sp = init.data().json("SPRITES");
		final RoomSprite sSeats = new RoomSpriteBoxN(sp, "SEAT_BOX") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return getLevel(rx, ry, item) >= getLevel(rx-d.x(), ry-d.y(), item);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				boolean ret = super.render(r, s, data, it, degrade, isCandle);
				renderLevel(r, getData2(it), it);
				return ret;
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return (byte) getLevel(rx, ry, item);
			}
		};
		
		final RoomSprite sWall = new RoomSprite1x1(sp, "WALL_1X1") {
			
			final RoomSprite ss = new RoomSpriteBoxN(sSeats) {
				
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return true;
				}
			};
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {

				return item.sprite(rx, ry) == null;
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return ss.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return ss.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		
		final RoomSprite sRim = new RoomSpriteBoxN(sp, "RIM_BOX") {
			

			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return getLevel(rx, ry, item) >= getLevel(rx-d.x(), ry-d.y(), item);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}

		};
		
		final RoomSprite sTower = new RoomSprite1x1(sp, "TOWER_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return false;
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, rotates);
			}
		};
		
		final RoomSprite sTorch = new RoomSprite1x1(sp, "TORCH_1X1") {
			
			RoomSprite1x1 tt = new RoomSprite1x1(sp, "TORCH_TOP_1X1") {
				
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return false;
				}
			};
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return false;
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
				tt.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return tt.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		
		
		
		FurnisherItemTile xx = new FurnisherItemTile(
				this,
				sWall,
				AVAILABILITY.SOLID, 
				false);
		
		cc = new FurnisherItemTile(
				this,
				sTorch,
				AVAILABILITY.SOLID, 
				false);
		
		FurnisherItemTile pp = new FurnisherItemTile(
				this,
				sTower,
				AVAILABILITY.SOLID, 
				false);
		
		
		FurnisherItemTile ee = new FurnisherItemTile(
				this,
				true,
				sSeats,
				AVAILABILITY.ROOM, 
				false);
		
		FurnisherItemTile sl = new FurnisherItemTile(
				this,
				new SStairs(sp, "STAIRS_LEFT_1X1"),
				AVAILABILITY.ROOM, 
				false);
		FurnisherItemTile sc = new FurnisherItemTile(
				this,
				new SStairs(sp, "STAIRS_CENTRE_1X1"),
				AVAILABILITY.ROOM, 
				false);
		FurnisherItemTile sr = new FurnisherItemTile(
				this,
				new SStairs(sp, "STAIRS_RIGHT_1X1"),
				AVAILABILITY.ROOM, 
				false);
		
		FurnisherItemTile _1 = new FurnisherItemTile(
				this,
				sSeats,
				AVAILABILITY.ROOM, 
				false);
		
		FurnisherItemTile _2 = new FurnisherItemTile(
				this,
				sSeats,
				AVAILABILITY.PENALTY4, 
				false);
		_2.setData(STATION);
		
		FurnisherItemTile xu = new FurnisherItemTile(
				this,
				sRim,
				AVAILABILITY.SOLID, 
				false);
		
		FurnisherItemTile __ = new FurnisherItemTile(
				this,
				null,
				AVAILABILITY.ROOM, 
				false);
		__.setData(ARENA);
		


		new FurnisherItem(new FurnisherItemTile[][] {
			{pp,xx,xx,xx,pp,xx,xx,xx,xx,xx,pp,xx,xx,xx,pp},
			{xx,_1,_1,_1,_1,_1,_1,_1,_1,_1,_1,_1,_1,_1,xx},
			{xx,_1,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_1,xx},
			{xx,_1,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_1,xx},
			{pp,_1,_2,_2,cc,xu,xu,xu,xu,xu,cc,_2,_2,_1,pp},
			{xx,_1,_2,_2,xu,__,__,__,__,__,xu,_2,_2,_1,xx}, 
			{xx,_1,_2,_2,xu,__,__,__,__,__,xu,_2,_2,_1,xx}, 
			{xx,_1,_2,_2,xu,__,__,__,__,__,xu,_2,_2,_1,xx}, 
			{xx,_1,_2,_2,xu,__,__,__,__,__,xu,_2,_2,_1,xx}, 
			{xx,_1,_2,_2,xu,__,__,__,__,__,xu,_2,_2,_1,xx}, 
			{pp,_1,_2,_2,cc,xu,sl,sc,sr,xu,cc,_2,_2,_1,pp},
			{xx,_1,_2,_2,_2,_2,sl,sc,sr,_2,_2,_2,_2,_1,xx},
			{xx,_1,_2,_2,_2,_2,sl,sc,sr,_2,_2,_2,_2,_1,xx},
			{xx,_1,_1,_1,_1,_1,sl,sc,sr,_1,_1,_1,_1,_1,xx},
			{pp,xx,xx,xx,xx,pp,ee,ee,ee,pp,xx,xx,xx,xx,pp},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{pp,xx,xx,xx,pp,xx,xx,xx,xx,xx,xx,xx,xx,pp,xx,xx,xx,pp},
			{xx,_1,_1,_1,_1,_1,_1,_1,_1,_1,_1,_1,_1,_1,_1,_1,_1,xx},
			{xx,_1,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_1,xx},
			{xx,_1,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_2,_1,xx},
			{pp,_1,_2,_2,cc,xu,xu,xu,xu,xu,xu,xu,xu,cc,_2,_2,_1,pp},
			{xx,_1,_2,_2,xu,__,__,__,__,__,__,__,__,xu,_2,_2,_1,xx}, 
			{xx,_1,_2,_2,xu,__,__,__,__,__,__,__,__,xu,_2,_2,_1,xx}, 
			{xx,_1,_2,_2,xu,__,__,__,__,__,__,__,__,xu,_2,_2,_1,xx}, 
			{pp,_1,_2,_2,cc,xu,sl,sr,xu,xu,sl,sr,xu,cc,_2,_2,_1,pp},
			{xx,_1,_2,_2,_2,_2,sl,sr,_2,_2,sl,sr,_2,_2,_2,_2,_1,xx},
			{xx,_1,_2,_2,_2,_2,sl,sr,_2,_2,sl,sr,_2,_2,_2,_2,_1,xx},
			{xx,_1,_1,_1,_1,_1,sl,sr,_1,_1,sl,sr,_1,_1,_1,_1,_1,xx},
			{pp,xx,xx,xx,xx,pp,ee,ee,pp,pp,ee,ee,pp,xx,xx,xx,xx,pp},
		}, 2);
		
		flush(1, 3);
		
	
	}

	
	private int getLevel(int rx, int ry, FurnisherItem item) {
		for (int i = 1; i < item.width(); i++) {
			for (int di = 0; di < DIR.ORTHO.size(); di++) {
				DIR d = DIR.ORTHO.get(di);
				int dx = rx+d.x()*i;
				int dy = ry+d.y()*i;
				if (item.get(dx, dy) == null)
					return i-1;
			}		
		}
		throw new RuntimeException();
	}

	@Override
	public boolean usesArea() {
		return false;
	}

	@Override
	public boolean mustBeIndoors() {
		return false;
	}
	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		ArenaInstance a =  new ArenaInstance(blue, area, init);
		for (COORDINATE c : a.body()) {
			if (a.is(c) && SETT.ROOMS().fData.tile.get(c) == cc) {
				SETT.LIGHTS().candle(c.x(), c.y(), 0);
			}
		}
		return a;
	}
	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		for (int di = 0; di < DIR.ALLC.size(); di++) {
			DIR d = DIR.ALLC.get(di);
			if (area.is(tx, ty, d) && SETT.ROOMS().fData.tile.get(tx, ty, d).data() == ARENA) {
				super.putFloor(tx, ty, upgrade, area);
				return;
			}
		}
			
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	
	private void renderLevel(SPRITE_RENDERER r, int level, RenderIterator it) {
		int cc = level & 0b01111;
		OpacityImp.TMP.set(cc*8);
		OpacityImp.TMP.bind();
		COLOR.BLACK.render(r, it.x(), it.x()+C.TILE_SIZE, it.y(), it.y()+C.TILE_SIZE);
		OPACITY.unbind();
	}
	
	private class SStairs extends RoomSprite1x1 {
		
		public SStairs(Json json, String key) throws IOException {
			super(json, key);
		}

		@Override
		protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
			return  getLevel(rx, ry, item) > getLevel(rx-d.x(), ry-d.y(), item);
		}
		
		@Override
		public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			return (byte) getLevel(rx, ry, item);
		}
		
		@Override
		public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
			super.render(r, s, data & 0b01111, it, degrade, false);
			renderLevel(r, getData2(it), it);
		}
		
		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			return false;
		}
	}
	

}
