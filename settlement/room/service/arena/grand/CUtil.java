package settlement.room.service.arena.grand;

import java.io.IOException;

import init.constant.C;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
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
import snake2d.util.datatypes.Rec;
import snake2d.util.file.Json;
import util.GUTIL;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class CUtil {
	
	private final ArenaConstructor c;
	public final FurnisherItemTile iTorch;
	public final FurnisherItemTile iWall;
	public final FurnisherItemTile iTower;
	public final FurnisherItemTile iSeat1;
	public final FurnisherItemTile iSeat2;
	public final FurnisherItemTile iEntrance;
	public final FurnisherItemTile iRim;
	public final FurnisherItemTile iArena;
	
	public final FurnisherItemTile[] iStairs;
	
	public CUtil(ArenaConstructor c, Json sp) throws IOException{
		this.c = c;
		final RoomSprite sSeats = new RoomSpriteBoxN(sp, "SEAT_BOX") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return getLevel(tx, ty) >= getLevel(tx-d.x(), ty-d.y());
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
				return (byte) getLevel(tx, ty);
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
				return getLevel(tx, ty) == -1;
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
				return getLevel(tx, ty) >= getLevel(tx-d.x(), ty-d.y());
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
		
		iWall = new FurnisherItemTile(
				c,
				sWall,
				AVAILABILITY.SOLID, 
				false);
		
		iTorch = new FurnisherItemTile(
				c,
				sTorch,
				AVAILABILITY.SOLID, 
				false);
		
		iTower = new FurnisherItemTile(
				c,
				sTower,
				AVAILABILITY.SOLID, 
				false);
		
		iStairs = new FurnisherItemTile[] {
			new FurnisherItemTile(
					c,
					new SStairs(sp, "STAIRS_SINGLE_1X1"),
					AVAILABILITY.ROOM, 
					false),
			new FurnisherItemTile(
					c,
					new SStairs(sp, "STAIRS_LEFT_1X1"),
					AVAILABILITY.ROOM, 
					false),
			new FurnisherItemTile(
					c,
					new SStairs(sp, "STAIRS_RIGHT_1X1"),
					AVAILABILITY.ROOM, 
					false),
			new FurnisherItemTile(
					c,
					new SStairs(sp, "STAIRS_CENTRE_1X1"),
					AVAILABILITY.ROOM, 
					false),
		};
		
		iSeat1 = new FurnisherItemTile(
				c,
				sSeats,
				AVAILABILITY.ROOM, 
				false);
		
		iSeat2 = new FurnisherItemTile(
				c,
				sSeats,
				AVAILABILITY.PENALTY4, 
				false);
		
		iEntrance = new FurnisherItemTile(
				c,
				sSeats,
				AVAILABILITY.ROOM, 
				false);
		
		iRim = new FurnisherItemTile(
				c,
				sRim,
				AVAILABILITY.SOLID, 
				false);
		
		iArena = new FurnisherItemTile(
				c,
				RoomSprite1x1.DUMMY,
				AVAILABILITY.ROOM, 
				false);
	}
	
	public boolean service(int tx, int ty) {
		FurnisherItemTile t = tile(tx, ty);
		return t == iSeat1 || t == iSeat2;
	}
	
	Rec init(TmpArea ins) {
		Rec res = new Rec(0);
		for (COORDINATE c : ins.body()) {
			if (ins.is(c)) {
				FurnisherItemTile it = get(c.x(), c.y(), ins);
				if (it == iArena) {
					if (res.width() == 0)
						res.setDim(1).moveX1Y1(c.x(), c.y());
					else
						res.unify(c.x(), c.y());
				}
				set(ins, it, c.x(), c.y());
			}
		}
		
		int x1 = ins.body().x1();
		int x2 = ins.body().x2()-1;
		int y1 = ins.body().y1();
		int y2 = ins.body().y2()-1;
		final int w = x2-x1;
		final int h = y2-y1;
		setStairs(ins, x1, y2, DIR.E, ins.body().width());
		setStairs(ins, x2, y2, DIR.N, ins.body().height());
		setStairs(ins, x2, y1, DIR.W, ins.body().width());
		setStairs(ins, x1, y1, DIR.S, ins.body().height());
		
		set(ins, iTower, x1, y1);
		set(ins, iTower, x2, y1);
		set(ins, iTower, x1, y2);
		set(ins, iTower, x1, y2);
		
		int t = 3;
		
		
		setTorch(ins, x1+t, y2-t, DIR.E, w-t*2);
		setTorch(ins, x2-t, y2-t, DIR.N, h-t*2);
		setTorch(ins, x2-t, y1+t, DIR.W, w-t*2);
		setTorch(ins, x1+t, y1+t, DIR.S, h-t*2);
		
		int seats = seatDepth(ins);
		if (seats > 6) {
			
			t = seats-2;
			setTorch(ins, x1+t, y2-t, DIR.E, w-t*2);
			setTorch(ins, x2-t, y2-t, DIR.N, h-t*2);
			setTorch(ins, x2-t, y1+t, DIR.W, w-t*2);
			setTorch(ins, x1+t, y1+t, DIR.S, h-t*2);
		}
		return res;
	}
	
	private void setStairs(TmpArea ins, int x1, int y1, DIR dir, int dim) {
		FurnisherItem st = c.groups().get(0).item(0, 0);
		for (int i = 0; i < dim; i++) {
			int x = x1 + dir.x()*i;
			int y = y1 + dir.y()*i;
			if (SETT.ROOMS().fData.item.get(x, y) == st) {
				int bb = 0;
				if (SETT.ROOMS().fData.item.get(x+dir.x(), y+dir.y()) == st)
					bb |= 1;
				else
					set(ins, iTower, x+dir.x(), y+dir.y());
				if (SETT.ROOMS().fData.item.get(x-dir.x(), y-dir.y()) == st)
					bb |= 2;
				else
					set(ins, iTower, x-dir.x(), y-dir.y());
				FurnisherItemTile s = iStairs[bb];
				set(ins, iEntrance, x, y);
				DIR in = dir.next(-2);
				for (int k = 1; k < 100; k++){
					int dx = x + in.x()*k;
					int dy = y + in.y()*k;
					FurnisherItemTile ttt = get(dx, dy, ins);
					if (ttt != iSeat2 && ttt != iRim && ttt != iSeat1)
						break;
					set(ins, s, dx, dy);
					
				}
				
				
			}
		}
	}
	
	private void setTorch(TmpArea ins, int x, int y, DIR dir, int dim) {
		
		set(ins, iTorch, x, y);
		set(ins, iTorch, x+dir.x()*dim, y+dir.y()*dim);
		

		int left = 5;
		int right = 5;
		
		for (int i = 4; i <= dim/2; i++) {
			int x1 = x+dir.x()*i;
			int y1 = y+dir.y()*i;
			int x2 = x+dir.x()*(dim-i);
			int y2 = y+dir.y()*(dim-i);
			
			if (tile(x1, y1) == iSeat2) {
				if (left > 10 || (left >=5 && tile(x1+dir.x(), y1+dir.y()) != iSeat2)) {
					set(ins, iTorch, x1, y1);
					left = 0;
				}
			}
			
			if (tile(x2, y2) == iSeat2) {
				if (right > 10 || (right >=5 && tile(x2-dir.x(), y2-dir.y()) != iSeat2)) {
					set(ins, iTorch, x2, y2);
					right = 0;
				}
			}
			right++;
			left++;
			
			
		}
		
	}
	
	private void set(TmpArea ins, FurnisherItemTile it, int tx, int ty) {
		FurnisherItem tt = c.groups().get(0).item(0, 0);
		byte dd = it.sprite.getData(tx, ty, ty-ins.body().x1(), ty-ins.body().y1(), tt, GUTIL.ran2().get(tx, ty));
		SETT.ROOMS().fData.spriteData.set(tx, ty, dd);
		dd = it.sprite.getData2(tx, ty, tx-ins.body().x1(), ty-ins.body().y1(), tt,  GUTIL.ran2().get(tx, ty));
		SETT.ROOMS().fData.spriteData2.set(tx, ty, dd);
		SETT.ROOMS().data.set(ins, tx, ty, it.index());
		SETT.PATH().availability.updateAvailability(tx, ty);
	}
	
	
	public FurnisherItemTile tile(int tile) {
		int d = SETT.ROOMS().data.get(tile);
		d &= 0b011111;
		return c.tile(d);
	}
	
	public FurnisherItemTile tile(int tx, int ty) {
		int d = SETT.ROOMS().data.get(tx, ty);
		d &= 0b011111;
		return c.tile(d);
	}
	
	private int seatDepth(AREA ins) {
		int dim = Math.min(ins.body().width(), ins.body().height());
		int arena = (int) (dim*0.4); 
		arena = Math.max(arena, 4);
		return arena;
	}
	
	public FurnisherItemTile get(int tx, int ty, AREA ins) {
		int l = getLevel(tx, ty);
		
		int arena = seatDepth(ins);
		if (l == 0)
			return iWall;
		if (l == 1)
			return iSeat1;
		if (l < arena-1)
			return iSeat2;
		if (l < arena)
			return iRim;
		return iArena;
	}
	
	public FurnisherItemTile get(int tx, int ty) {
		Room rr = SETT.ROOMS().map.get(tx, ty);
		if (!(rr instanceof AREA))
			return null;
		
		if (rr.constructor() != c)
			return null;
		
		return get(tx, ty, (AREA) rr);
		
	}
	
	public boolean canBeEntrance(int tx, int ty) {
		Room r = SETT.ROOMS().map.get(tx, ty);
		if (r == null || !(r instanceof AREA))
			return false;
		AREA a = (AREA) r;
		int arena = seatDepth(a);

		if (ty == a.body().y1() || ty == a.body().y2()-1) {
			if (tx-a.body().x1() <= arena)
				return false;
			if (a.body().x2()-1 - tx <= arena)
				return false;
			
		}else if (tx == a.body().x1() || tx == a.body().x2()-1) {
			if (ty-a.body().y1() <= arena)
				return false;
			if (a.body().y2()-1 - ty <= arena)
				return false;
		}
		return true;
	}
	

	
	public int getLevel(int tx, int ty) {
		
		Room rr = SETT.ROOMS().map.get(tx, ty);
		if (!(rr instanceof AREA))
			return -1;
		
		if (rr.constructor() != c)
			return -1;
		
		AREA a = (AREA) rr;

		int distX = Math.min(Math.abs(a.body().x1()-tx), Math.abs(a.body().x2()-tx-1));
		int distY = Math.min(Math.abs(a.body().y1()-ty), Math.abs(a.body().y2()-ty-1));

		if (distX == 0 && distY == 0) {
			return 0;
		}
		
		int dist = Math.min(distX, distY);
		
		return dist;
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
			return  getLevel(tx, ty) > getLevel(tx-d.x(), ty-d.y());
		}
		
		@Override
		public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			return (byte) getLevel(tx, ty);
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
