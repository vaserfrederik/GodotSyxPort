package settlement.room.main;

import static settlement.main.SETT.IN_BOUNDS;
import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TAREA;
import static settlement.main.SETT.TILE_BOUNDS;
import static settlement.main.SETT.TWIDTH;

import java.io.IOException;

import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room.RoomInstanceImp;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.util.RoomAreaWrapper;
import settlement.room.sprite.RoomSprite;
import snake2d.LOG;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.map.MAP_BOOLEAN;
import snake2d.util.map.MAP_BOOLEANE;
import snake2d.util.map.MAP_INT;
import snake2d.util.map.MAP_INTE;
import snake2d.util.map.MAP_OBJECT;
import snake2d.util.rnd.RND;
import snake2d.util.sets.Bitmap1D;

public class MapDataF {

	public final static short NOTHING = 0;
	
	private final byte[] itemI = Alloc.bb(TAREA);
	private final byte[] tileI = Alloc.bb(TAREA);
	private final byte[] spriteDataI = Alloc.bb(TAREA);
	private final byte[] spriteDataI2 = Alloc.bb(TAREA);
	private final Bitmap1D masterTileI = new Bitmap1D(TAREA, false);
	private final Bitmap1D candleI = new Bitmap1D(TAREA, false);
	
	MapDataF(ROOMS r){
		
	}
	
	final SAVABLE saver = new SAVABLE() {
		
		@Override
		public void save(FilePutter file) {
			file.bs(itemI);
			file.bs(tileI);
			file.bs(spriteDataI);
			file.bs(spriteDataI2);
			masterTileI.save(file);
			candleI.save(file);
			//save rooms!
		}
		
		@Override
		public void load(FileGetter file) throws IOException {
			file.bs(itemI);
			file.bs(tileI);
			file.bs(spriteDataI);
			file.bs(spriteDataI2);
			masterTileI.load(file);
			candleI.load(file);
		}

		@Override
		public void clear() {
			for (int i = 0; i < TAREA; i++) {
				itemI[i] = NOTHING;
				tileI[i] = NOTHING;
			}
			masterTileI.setAll(false);
			candleI.setAll(false);
		}
		
	};
	
	private final MAP_INTE itemIndex = new MAP_INTE() {
		
		@Override
		public int get(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return get(tx+ty*TWIDTH);
			return 0;
		}
		
		@Override
		public int get(int tile) {
			return itemI[tile] & 0x0FF;
		}
		
		@Override
		public MAP_INTE set(int tx, int ty, int value) {
			if (IN_BOUNDS(tx, ty)) {
				return set(tx+ty*TWIDTH, value);
			}
			return this;
		}
		
		@Override
		public MAP_INTE set(int tile, int value) {
			itemI[tile] = (byte) value;
			return this;
		}
	};
	
	public final MAP_INT itemIndexx = itemIndex;
	
	public final MAP_OBJECT<FurnisherItem> item = new MAP_OBJECT<FurnisherItem>() {
		
		@Override
		public FurnisherItem get(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return get(tx+ty*TWIDTH);
			return null;
		}
		
		@Override
		public FurnisherItem get(int tile) {
			Room r = ROOMS().map.get(tile);
			if (r != null && r.constructor() != null) {
				return r.constructor().item(itemI[tile] & 0x0FF);
			}
			return null;
		}
	};
	
	public final MAP_INT tileData = new MAP_INT() {
		
		@Override
		public int get(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return get(tx+ty*TWIDTH);
			return 0;
		}
		
		@Override
		public int get(int t) {
			FurnisherItemTile tt = tile.get(t);
			if (tt != null)
				return tt.data();
			return 0;
		}
	};
	
	public void itemSet(int x1, int y1, FurnisherItem item, RoomInstanceImp r) {
		int ran = RND.rInt() & 0x0FFFF;
		
		
		
		if (r.constructor() != null && r.constructor() != item.group.blueprint)
			throw new RuntimeException(""+r.name(x1, y1));
		
		for (int y = 0; y < item.height(); y++) {
			for (int x = 0; x < item.width(); x++) {
				int dx = x+x1;
				int dy = y+y1;
				
				if (!IN_BOUNDS(dx, dy)) {
					throw new RuntimeException(x1 + " " + y1 + " " + x + " " + y);
				}
				FurnisherItemTile t = item.get(x, y);
				if (t != null) {
					int i = dx+dy*TWIDTH;
					if (!r.is(dx, dy)) {
						throw new RuntimeException(r.constructor().blue().info.name + " " + item.width() + " " + item.height() +  " THIS IS A SPECIAL ERROR THAT THE DEV IS LOOKING FOR. Please help. If you can reproduce the issue in one of your saves, send the save to: info@songsofsyx.com !");
					}
						
					if (itemIndex.get(i) != NOTHING)
						throw new RuntimeException(""+item + " " + (r.constructor() != null ? r.constructor().blue().key : ""));
					if (tileIndex.get(i) != NOTHING)
						throw new RuntimeException(r.constructor().blue().key);
					masterTileI.set(i, x == item.firstX() && y == item.firstY());
					
					tileIndex.set(i, t.index());
					itemIndex.set(i, item.index());
					
					if (t.sprite() != null) {
						byte d = t.sprite().getData(dx, dy, x, y, item, ran);
						spriteDataI[i] = d;
						d = t.sprite().getData2(dx, dy, x, y, item, ran);
						spriteDataI2[i] = d;
					}
				}
				
			}
		}
	}
	
	public void itemClear(int tx, int ty, Room r) {
		FurnisherItem i = item.get(tx, ty);
		if (i == null)
			return;
		COORDINATE m = itemMaster(tx, ty, masterFind, r);
		int sx = m.x()-i.firstX();
		int sy = m.y()-i.firstY();
		for (int y = 0; y < i.height(); y++) {
			for (int x = 0; x < i.width(); x++) {
				if (i != null && i.get(x, y) != null) {
					int dx = sx+x;
					int dy = sy+y;
					if (!r.isSame(tx, ty, dx, dy)) {
						throw new RuntimeException(r.constructor().blue().info.name + " " + i.width() + " " + i.height() +  " THIS IS A SPECIAL ERROR THAT THE DEV IS LOOKING FOR. Please help. If you can reproduce the issue in one of your saves, send the save to: info@songsofsyx.com !");
					}
					
					int index = dx+dy*TWIDTH;
					itemIndex.set(index, NOTHING);
					tileIndex.set(index, NOTHING);
					masterTileI.set(index, false);
					candleI.set(index, false);
					SETT.LIGHTS().remove(x, y);
				}
			}
		}
		FurnisherItem i2 = item.get(tx, ty);
		if (i2 != null) {
			throw new RuntimeException(i.rotation + " " + i.group.index() + " " + i.group.blueprint.blue().key + " " + i2.rotation + " " + i2.group.index() + " " + i2.group.blueprint.blue().key);
		}
	}
	
	private static final RoomAreaWrapper wrap = new RoomAreaWrapper();
	
	public void clear(int mx, int my, Room r) {
		ROOMA a = wrap.init(r, mx, my);
		for (COORDINATE c : a.body()) {
			if (a.is(c)) {
				itemClear(c.x(), c.y(), r);
			}
		}
		wrap.done();
	}
	
	private final Coo masterFind = new Coo();


	public COORDINATE itemMaster(int tx, int ty, Coo res, Room room) {
		
		if (ROOMS().map.indexGetter.get(tx, ty) != room.index())
			throw new RuntimeException(tx + " " + ty + room + " " + ROOMS().map.get(tx, ty));
		
		final int itI = itemIndex.get(tx, ty);
		final FurnisherItem it = room.constructor().item(itI);
		if (it == null) {
			throw new RuntimeException("" + itI);
		}
		
		if (masterTileI.get(tx+ty*TWIDTH)) {
			res.set(tx, ty);
			return res;
		}
		
		final int w = it.width();
		final int h = it.height();
		
		final int x1 = tx-w+1+it.firstX();
		final int y1 = ty-h+1+it.firstY();
		
		for (int y = 0; y < h; y++) {
			for (int x = 0; x < w; x++) {
				int dx = x1 + x;
				int dy = y1 + y;
				if (!TILE_BOUNDS.holdsPoint(dx, dy))
					continue;
				if (ROOMS().map.indexGetter.get(dx, dy) != room.index())
					continue;
				if (itI != itemIndex.get(dx, dy))
					continue;
				if (!masterTileI.get(dx+dy*TWIDTH))
					continue;
				int qx = dx-it.firstX();
				int qy = dy-it.firstY();
				qx = tx -qx;
				qy = ty -qy;
				if (it.get(qx, qy) == null)
					continue;
				res.set(dx, dy);
				return res;
			}
		}
		LOG.ln(tx + " " + ty + " " + room + " " + room.index() + " " + it.firstX() + " " + it.firstY() + " " + w + " " + h);
		
		for (int y = 0; y <= h; y++) {
			for (int x = 0; x <= w; x++) {
				int dx = x1 + x;
				int dy = y1 + y;
				int qx = dx-it.firstX();
				int qy = dy-it.firstY();
				qx = tx -qx;
				qy = ty -qy;
				LOG.ln(x + " " + y + " " + dx + " " + dy + " " + (ROOMS().map.indexGetter.get(dx, dy) == room.index()) + (itI != itemIndex.get(dx, dy)) + " " + masterTileI.get(dx+dy*TWIDTH) + " " + (it.get(qx, qy) == null));
				if (!TILE_BOUNDS.holdsPoint(dx, dy))
					continue;
				if (ROOMS().map.indexGetter.get(dx, dy) != room.index())
					continue;
				if (itI != itemIndex.get(dx, dy))
					continue;
				if (!masterTileI.get(dx+dy*TWIDTH))
					continue;
				if (it.get(qx, qy) == null)
					continue;
				res.set(dx, dy);
				return res;
			}
		}
		LOG.ln(tx + " " + ty + " " + room + " " + room.constructor() + " " + it.group.name + " " + it.width() + " " + it.height());
		
		String m = "A very strange bug has happened that the developer is looking for. It has to do with the remove tool that has been used to remove houses. "
				+ "The house in question was at tile x: " + tx + " y" + + ty + " " + 
				"If you can replicate this by attemting the removal again around these tiles, the devloper is dying for a reproducable example. Please send the save with instructions of where to delete to info@songsofsyx.com. You can find the saves through the game launcher."; 
		
		throw new RuntimeException(m);
	}
	
	public COORDINATE itemMaster(int tx, int ty, Coo res) {
		Room r = SETT.ROOMS().map.get(tx, ty);
		if (r == null)
			return null;
		return itemMaster(tx, ty, res, r);
	}
	
	public COORDINATE itemMaster(COORDINATE c, Coo res) {
		return itemMaster(c.x(), c.y(), res);
	}
	
	public COORDINATE itemX1Y1(int tx, int ty, Coo res, Room room) {
		itemMaster(tx, ty, res, room);
		final int itI = itemIndex.get(tx, ty);
		final FurnisherItem it = room.constructor().item(itI);
		res.increment(-it.firstX(), -it.firstY());
		return res;
	}
	
	public COORDINATE itemX1Y1(int tx, int ty, Coo res) {
		Room r = SETT.ROOMS().map.get(tx, ty);
		if (r == null)
			return null;
		return itemX1Y1(tx, ty, res, r);
	}
	
	
	public COORDINATE itemX1Y1(int tx, int ty, DIR d, Coo res) {
		return itemX1Y1(tx+d.x(), ty+d.y(), res);
	}
	
	public COORDINATE itemX1Y1(COORDINATE c, Coo res) {
		return itemX1Y1(c.x(), c.y(), res);
	}
	
//	public final MAP_OBJECT<COORDINATE> itemUpperLeft = new MAP_OBJECT<COORDINATE>() {
//
//		@Override
//		public COORDINATE get(int tile) {
//			return get(tile%TWIDTH, tile/TWIDTH);
//		}
//
//		@Override
//		public COORDINATE get(int tx, int ty) {
//			COORDINATE t = masterTile(tx, ty);
//			if (t == null)
//				return t;
//			final FurnisherItem it = item.get(tx, ty);
//			masterFind.set(t.x()-it.firstX(), t.y()-it.firstY());
//			return masterFind;
//		}
//	};
	
	private final MAP_INTE tileIndex = new MAP_INTE() {
		
		@Override
		public int get(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return get(tx+ty*TWIDTH);
			return 0;
		}
		
		@Override
		public int get(int tile) {
			return tileI[tile] & 0x0FF;
		}
		
		@Override
		public MAP_INTE set(int tx, int ty, int value) {
			if (IN_BOUNDS(tx, ty)) {
				return set(tx+ty*TWIDTH, value);
			}
			return this;
		}
		
		@Override
		public MAP_INTE set(int tile, int value) {
			tileI[tile] = (byte) (value & 0x0FF);
			return this;
		}
	};
	
	public MAP_INT tIndex = tileIndex;
	
	public final MAP_OBJECT<FurnisherItemTile> tile = new MAP_OBJECT<FurnisherItemTile>() {
		
		@Override
		public FurnisherItemTile get(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return get(tx+ty*TWIDTH);
			return null;
		}
		
		@Override
		public FurnisherItemTile get(int tile) {
			Room r = ROOMS().map.get(tile);
			if (r != null && r.constructor() != null) {
				return r.constructor().tile(tileI[tile] & 0x0FF);
			}
			return null;
		}
	};
	
	public final MAP_OBJECT<RoomSprite> sprite = new MAP_OBJECT<RoomSprite>() {
		
		@Override
		public RoomSprite get(int tx, int ty) {
			FurnisherItemTile t = tile.get(tx, ty);
			if (t != null)
				return t.sprite();
			return null;
		}
		
		@Override
		public RoomSprite get(int t) {
			FurnisherItemTile tt = tile.get(t);
			if (tt != null)
				return tt.sprite();
			return null;
		}
	};
	
	public final MAP_INTE spriteData = new MAP_INTE() {
		
		@Override
		public int get(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return get(tx+ty*TWIDTH);
			return 0;
		}
		
		@Override
		public int get(int tile) {
			return spriteDataI[tile] & 0x0FF;
		}
		
		@Override
		public MAP_INTE set(int tx, int ty, int value) {
			if (IN_BOUNDS(tx, ty)) {
				return set(tx+ty*TWIDTH, value);
			}
			return this;
		}
		
		@Override
		public MAP_INTE set(int tile, int value) {
			spriteDataI[tile] = (byte) (value & 0x0FF);
			return this;
		}
	};
	
	public final MAP_INTE spriteData2 = new MAP_INTE() {
		
		@Override
		public int get(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return get(tx+ty*TWIDTH);
			return 0;
		}
		
		@Override
		public int get(int tile) {
			return spriteDataI2[tile] & 0x0FF;
		}
		
		@Override
		public MAP_INTE set(int tx, int ty, int value) {
			if (IN_BOUNDS(tx, ty)) {
				return set(tx+ty*TWIDTH, value);
			}
			return this;
		}
		
		@Override
		public MAP_INTE set(int tile, int value) {
			spriteDataI2[tile] = (byte) (value & 0x0FF);
			return this;
		}
	};
	
	public final MAP_BOOLEANE candle = new MAP_BOOLEANE() {
		
		@Override
		public boolean is(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return is(tx+ty*TWIDTH);
			return false;
		}
		
		@Override
		public boolean is(int tile) {
			return candleI.get(tile);
		}

		@Override
		public MAP_BOOLEANE set(int tile, boolean value) {
			candleI.set(tile, value);
			return this;
		}

		@Override
		public MAP_BOOLEANE set(int tx, int ty, boolean value) {
			candleI.set(tx+ty*TWIDTH, value);
			return this;
		}
	};
	
	public final MAP_BOOLEAN isMaster = new MAP_BOOLEAN() {
		
		@Override
		public boolean is(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return is(tx+ty*TWIDTH);
			return false;
		}
		
		@Override
		public boolean is(int tile) {
			return masterTileI.get(tile);
		}
	};
	
	public final MAP_OBJECT<AVAILABILITY> availability = new MAP_OBJECT<AVAILABILITY>() {
		
		@Override
		public AVAILABILITY get(int tx, int ty) {
			FurnisherItemTile t = tile.get(tx, ty);
			if (t != null)
				return t.availability;
			return AVAILABILITY.ROOM;
		}
		
		@Override
		public AVAILABILITY get(int tt) {
			FurnisherItemTile t = tile.get(tt);
			if (t != null)
				return t.availability;
			return AVAILABILITY.ROOM;
		}
	};
	
	public final MAP_BOOLEAN blocking = new MAP_BOOLEAN() {

		@Override
		public boolean is(int tt) {
			FurnisherItemTile t = tile.get(tt);
			if (t != null)
				return t.isBlocker();
			return false;
			
		}

		@Override
		public boolean is(int tx, int ty) {

			FurnisherItemTile t = tile.get(tx, ty);
			if (t != null)
				return t.isBlocker();
			return false;
		}
	};
	
	public final MAP_BOOLEAN mustReach = new MAP_BOOLEAN() {

		@Override
		public boolean is(int tt) {
			FurnisherItemTile t = tile.get(tt);
			if (t != null)
				return t.mustBeReachable;
			return false;
			
		}

		@Override
		public boolean is(int tx, int ty) {

			FurnisherItemTile t = tile.get(tx, ty);
			if (t != null)
				return t.mustBeReachable;
			return false;
		}
	};
	
//	final MAP_CLEARER clearer = new MAP_CLEARER() {
//		
//		@Override
//		public MAP_CLEARER clear(int tx, int ty) {
//			if (IN_BOUNDS(tx, ty))
//				clear(tx+ty*TWIDTH);
//			return this;
//		}
//		
//		@Override
//		public MAP_CLEARER clear(int tile) {
//			itemClear(tile%SETT.TWIDTH, tile/SETT.THEIGHT, null);
//			itemI[tile] = NOTHING;
//			tileI[tile] = NOTHING;
//			masterTileI.set(tile, false);
//			candleI.set(tile, false);
//			return this;
//		}
//	};
	
	boolean isClean(int tile) {
		return itemI[tile] == NOTHING && tileI[tile] == NOTHING;
	}
	
	void clean(int tile) {
		itemI[tile] = NOTHING;
		tileI[tile] = NOTHING;
		spriteDataI[tile] = 0;
		spriteDataI2[tile] = 0;
		masterTileI.set(tile, false);
		candleI.set(tile, false);
	}
	
}
