package settlement.room.main;

import static settlement.main.SETT.IN_BOUNDS;
import static settlement.main.SETT.TAREA;
import static settlement.main.SETT.TWIDTH;

import java.io.IOException;
import java.util.Arrays;

import settlement.main.SETT;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.map.MAP_INT;

public interface MapRoomData extends MAP_INT{
	
	static class Data implements MapRoomData, SAVABLE{

		private final int[] dataI = Alloc.ii(TAREA);
		
		@Override
		public void save(FilePutter file) {
			file.is(dataI);
		}
		
		@Override
		public void load(FileGetter file) throws IOException {
			file.is(dataI);
		}

		@Override
		public void clear() {
			Arrays.fill(dataI, 0);
			
		}
		
		@Override
		public int get(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return get(tx+ty*TWIDTH);
			return 0;
		}
		
		@Override
		public int get(int tile) {
			return dataI[tile];
		}

		@Override
		public void set(ROOMA r, int tile, int value) {
			if (!r.is(tile))
				throw new RuntimeException(r + " " + tile%TWIDTH + " " + tile/TWIDTH + " " + SETT.ROOMS().map.get(tile));
			dataI[tile] = value;
		}
		
		void set(int tile, int value) {
			dataI[tile] = value;
		}

	}
	
	public default void inc(ROOMA r, COORDINATE c, int value) {
		inc(r, c.x(), c.y(), value);
	}
	
	public default void inc(ROOMA r, COORDINATE c, DIR d, int value) {
		inc(r, c.x()+d.x(), c.y()+d.y(), value);
	}
	
	public default void inc(ROOMA r, int tx, int ty, int value) {
		if (IN_BOUNDS(tx, ty)) {
			inc(r, tx+ty*TWIDTH, value);
		}
	}
	
	public default void inc(ROOMA r, int tx, int ty, DIR d, int value) {
		inc(r, tx+d.x(), ty+d.y(), value);
	}

	public default void inc(ROOMA r, int tile, int value) {
		set(r, tile, get(tile)+value);
	}
	
	public default void set(ROOMA r, COORDINATE c, int value) {
		set(r, c.x(), c.y(), value);
	}
	
	public default void set(ROOMA r, COORDINATE c, DIR d, int value) {
		set(r, c.x()+d.x(), c.y()+d.y(), value);
	}
	
	public default void set(ROOMA r, int tx, int ty, int value) {
		if (IN_BOUNDS(tx, ty)) {
			set(r, tx+ty*TWIDTH, value);
		}
	}
	
	public default void set(ROOMA r, int tx, int ty, DIR d, int value) {
		set(r, tx+d.x(), ty+d.y(), value);
	}

	public void set(ROOMA r, int tile, int value);

}
