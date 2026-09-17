package settlement.room.main;

import static settlement.main.SETT.IN_BOUNDS;
import static settlement.main.SETT.PATH;
import static settlement.main.SETT.TAREA;
import static settlement.main.SETT.TWIDTH;

import java.io.IOException;

import settlement.main.SETT;
import settlement.room.main.util.RoomAreaWrapper;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Rec;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.map.MAP_INT;
import snake2d.util.map.MAP_OBJECT;

public final class RoomsMap implements MAP_OBJECT<Room>{

	
	private final static int NOTHING = 0;
	private final int[] roomI = Alloc.ii(SETT.TAREA);

	private int singletonI = 1;
	private static final int singletonMax = 256;
	
	private static final int CHUNKSIZE = 1024;
	private Room[] rooms = new Room[CHUNKSIZE];
	private int lastIndex = singletonMax;
	
	RoomsMap(){
		
	}
	
	final SAVABLE saver = new SAVABLE() {
		
		@Override
		public void save(FilePutter file) {
			file.i(rooms.length);
			file.is(roomI);

			int am = 0;
			for (Room r : rooms) {
				if (r != null && !r.singleton)
					am++;
			}
			file.i(am);
			
			for (Room r : rooms) {
				if (r == null || r.singleton)
					continue;
				SETT.ROOMS().collection.saver().save(r.blueprint(), file);
				
				int pos = file.getPosition();
				file.i(0);
				file.object(r);
				r.saveExtra(file);
				int le = file.getPosition()-pos-4;
				file.setAtPosition(pos, le);
			}
		}
		
		@Override
		public void load(FileGetter file) throws IOException {
			
			Room[] nn = new Room[file.i()];
			file.is(roomI);
			
			for (int i = 0; i < singletonMax; i++) {
				nn[i] = rooms[i];
			}
			rooms = nn;
			
			int am = file.i();
			
			for (int i = 0; i < am; i++) {
				RoomBlueprint print = SETT.ROOMS().collection.loader().load(file);
				int le = file.i();
				if (print != null) {
					Room r = (Room) file.object(true);
					if (r != null) {
						r.bI = (short) print.index();
						if (r.loadExtra(file)) {
							r.loadFix();
							rooms[r.index()] = r;
							continue;
						}
					}
					
				}
				file.setPosition(file.getPosition()+le);
				
			}
			
			for (int i = 0; i < TAREA; i++) {
				if (roomI[i] != NOTHING && rooms[roomI[i]] == null) {
					SETT.ROOMS().fData.clean(i);
					SETT.ROOMS().pData.set(i, 0);
				}
			}
			
			for (lastIndex = singletonI; lastIndex < roomI.length; lastIndex++) {
				if (roomI[lastIndex] == NOTHING) {
					break;
				}
			}
			
		}
		
		@Override
		public void clear() {
			Room[] nn = new Room[CHUNKSIZE];
			for (int i = 0; i < singletonI; i++) {
				nn[i] = rooms[i];
			}
			rooms = nn;
			for (int i = 0; i < TAREA; i++) {
				roomI[i] = NOTHING;
			}
			
			
		}
	};
	
	public Room getByIndex(int index) {
		return rooms[index];
	}
	
	public int max() {
		return rooms.length;
	}

	
	int create(Room r, boolean singleton) {
		if (singleton) {
			if (singletonI > singletonMax)
				throw new RuntimeException("too many singleton rooms!");
			int i = singletonI;
			singletonI ++;
			if (rooms[i] != null)
				throw new RuntimeException();
			rooms[i] = r;
			return i;
		}
		
		for (; lastIndex < rooms.length; lastIndex++) {
			if (rooms[lastIndex] == null) {
				rooms[lastIndex] = r;
				return lastIndex;
			}
		}
		
		lastIndex = singletonI;
		for (; lastIndex < rooms.length; lastIndex++) {
			if (rooms[lastIndex] == null) {
				rooms[lastIndex] = r;
				return lastIndex;
			}
		}
		Room[] nn = new Room[rooms.length+CHUNKSIZE];
		for (int i = 0; i < rooms.length; i++)
			nn[i] = rooms[i];
		rooms = nn;
		rooms[lastIndex] = r;
		return lastIndex;
		
		
	}
	
	final Room getRaw(int tx, int ty) {
		return rooms[roomI[tx+ty*TWIDTH]];	
	}
	
	void set(int tile, Room n) {
		if (get(tile) != null)
			throw new RuntimeException(tile + " " + n + " " + get(tile));
		SETT.ROOMS().fData.clean(tile);
		roomI[tile] = n.roomI;
		SETT.ROOMS().pData.set(tile, 0);
	}
	
	void clear(int tile, Room old) {
		if (old != get(tile))
			throw new RuntimeException(tile + " " + indexGetter.get(tile) + " " + old + " " + get(tile) + " " + get(tile%SETT.TWIDTH, tile/SETT.TWIDTH));
		SETT.ROOMS().fData.clean(tile);
		SETT.ROOMS().pData.set(tile, 0);
		roomI[tile] = NOTHING;
	}
	
	void replace(int tile, Room old, Room current) {
		if (old != get(tile))
			throw new RuntimeException((tile%TWIDTH) + " " + (tile/TWIDTH) + " " + get(tile) + " " + old + " " + current);
		roomI[tile] = current.roomI;
	}
	
	TmpArea delete(Room room, int mx, int my, Object user) {
		
		TmpArea a = SETT.ROOMS().tmpArea(user);
		a.set(room, mx, my);
		if (!(room instanceof RoomSingleton) && room.blueprint() != SETT.ROOMS().THRONE) {
			if (rooms[room.roomI] == null)
				throw new RuntimeException();
			rooms[room.roomI] = null;
			if (room.roomI < lastIndex) {
				lastIndex = room.roomI;
			}
		}
		
		init(a);

		return a;
		
	}
	
	public void init(AREA room) {
		tmp.set(room.body());
		for (COORDINATE c : tmp) {
			if (room.is(c)) {
				SETT.TILE_MAP().miniCUpdate(c.x(), c.y());
				PATH().availability.updateAvailability(c.x(), c.y());
				SETT.ENV().map.setChanged(c.x(), c.y());
				PATH().availability.updateService(c.x(), c.y());
			}
		}
	}
	
	private final Rec tmp = new Rec();
	
	@Override
	public Room get(int tile) {
		return rooms[roomI[tile]];
	}

	@Override
	public Room get(int tx, int ty) {
		if (IN_BOUNDS(tx, ty)) {
			return rooms[roomI[tx+ty*TWIDTH]];
		}
		return null;
	}
	
	public final MAP_OBJECT<RoomBlueprint> blueprint = new MAP_OBJECT<RoomBlueprint>() {

		@Override
		public RoomBlueprint get(int tile) {
			int i = roomI[tile];
			if (i != NOTHING) {
				Room r = getByIndex(i);
				if (r != null)
					return r.blueprint();
			}
			return null;
		}

		@Override
		public RoomBlueprint get(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return get(tx + ty * TWIDTH);
			return null;
		}

	};
	
	public final MAP_OBJECT<RoomBlueprintImp> blueprintImp = new MAP_OBJECT<RoomBlueprintImp>() {

		@Override
		public RoomBlueprintImp get(int tile) {
			int i = roomI[tile];
			if (i != NOTHING) {
				Room r =  getByIndex(i);
				if (r != null && r.constructor() != null)
					return r.constructor().blue();
			}
			return null;
		}

		@Override
		public RoomBlueprintImp get(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return get(tx + ty * TWIDTH);
			return null;
		}

	};
	
	public final MAP_INT indexGetter = new MAP_INT() {

		@Override
		public int get(int tx, int ty) {
			return get(tx + ty * TWIDTH);
		}

		@Override
		public int get(int tile) {
			return roomI[tile];
		}
	};
	
	public final MAP_OBJECT<RoomInstance> instance = new MAP_OBJECT<RoomInstance>() {

		@Override
		public RoomInstance get(int tile) {
			Room r = RoomsMap.this.get(tile);
			if (r instanceof RoomInstance)
				return (RoomInstance) r;
			return null;
		}

		@Override
		public RoomInstance get(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return get(tx + ty * TWIDTH);
			return null;
		}

	};
	
	public final MAP_OBJECT<ROOMA> rooma = new MAP_OBJECT<ROOMA>() {

		private final RoomAreaWrapper wrap = new RoomAreaWrapper();
		
		@Override
		public ROOMA get(int tile) {
			wrap.done();
			Room r = RoomsMap.this.get(tile);
			if (r != null)
				return wrap.init(r, tile%TWIDTH, tile/TWIDTH);
			return null;
		}

		@Override
		public ROOMA get(int tx, int ty) {
			wrap.done();
			if (IN_BOUNDS(tx, ty)) {
				Room r = RoomsMap.this.get(tx, ty);
				if (r != null)
					return wrap.init(r, tx, ty);
			}
			return null;
		}

	};
	



}
