package settlement.room.main.util;

import java.io.IOException;

import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.path.AvailabilityListener;
import settlement.room.main.ROOMS;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprint;
import settlement.tilemap.terrain.TFortification;
import settlement.tilemap.terrain.Terrain.TerrainTile;
import snake2d.util.MATH;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.datatypes.Rec;
import snake2d.util.datatypes.RecShort;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.map.MAP_BOOLEAN;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.ArrayList;
import util.GUTIL;
import util.info.INFO;
import util.text.D;
import util.text.Dic;

public final class RoomIsolation {

	private final int CHUNKSIZE = 32;
	private final int CHUNCS = Integer.numberOfTrailingZeros(CHUNKSIZE);
	private final Chunk[][] chunks = new Chunk[(int) Math.ceil((double)SETT.THEIGHT/CHUNKSIZE)][(int) Math.ceil((double)SETT.TWIDTH/CHUNKSIZE)];
	private final ArrayList<Chunk> toUpdate = new ArrayList<>(chunks.length*chunks[0].length); 
	

	private final Rec rec = new Rec();
	public final INFO info;
	private final RoomAreaWrapper wrap = new RoomAreaWrapper();
	
	public RoomIsolation(ROOMS r) {
		D.gInit(this);
		info = new INFO(Dic.¤¤Isolation, D.g("desc", "Insulation prevents room degradation and sound pollution. Surrounding walls increase insulation, while doors and gaps decrease it. Poorly insulated rooms need more maintenance. Poorly insulated homes degrade furniture faster."));
		
		for (int y = 0; y < chunks.length; y++) {
			for (int x = 0; x < chunks[y].length; x++) {
				chunks[y][x] = new Chunk(x, y);
			}
		}
		
		new AvailabilityListener() {
			
			@Override
			protected void changed(int tx, int ty, AVAILABILITY a, AVAILABILITY old, boolean playerChange) {
				setChanged(tx, ty, a, old);
			}
		};
	}
	
	private void setChanged(int tx, int ty, AVAILABILITY a, AVAILABILITY old) {
		if (a.player < 0 != old.player < 0) {
			for (int di = 0; di < DIR.ALL.size(); di++) {
				DIR d = DIR.ALL.get(di);
				Room r = SETT.ROOMS().map.get(tx, ty, d);
				setChanged(r, tx+d.x(), ty+d.y());
			}
		}
	}
	
	private void setChanged(Room r, int x, int y) {
		if (r != null && r.constructor() != null && r.constructor().needsIsolation()) {
			int mx = r.mX(x, y)>>CHUNCS;
			int my = r.mY(x, y)>>CHUNCS;
			Chunk c = chunks[my][mx];
			if (!c.added) {
				toUpdate.add(c);
				c.added = true;
			}
		}
	}
	
	public void update() {
		while(!toUpdate.isEmpty()) {
			Chunk ch = toUpdate.removeLast();
			
			for (COORDINATE c : ch.body) {
				Room r = SETT.ROOMS().map.get(c);
				if (r != null && r.mX(c.x(), c.y()) == c.x() && r.mY(c.x(), c.y()) == c.y()) {
					r.isolationSet(c.x(), c.y(), getProspect(r.blueprint(), wrap.init(r, c.x(), c.y()), null));
					wrap.done();
				}
			}
			ch.added = false;
			
		}
		
	}
	
	public double getProspect(RoomBlueprint blue, AREA r, MAP_BOOLEAN isWall) {
		
		this.isWall = isWall;
		
		double unwalled = 0;
		double total = 0;
		rec.set(r.body());
		GUTIL.marker().init(this);
		
		for (COORDINATE c : rec) {
			
			if (!r.is(c) || !isEdge(r, c))
				continue;
			
			total++;
			for (DIR d : DIR.ALL) {
				
				if (!SETT.IN_BOUNDS(c, d)) {
					unwalled++;
					continue;
				}
				if (r.is(c, d))
					continue;
				
				if (!wall.is(c, d) && !GUTIL.marker().is(c, d)) {
					if (blue ==  SETT.ROOMS().HOME) {	
						if (blue != SETT.ROOMS().map.blueprintImp.get(c, d)) {
							GUTIL.marker().set(c, d, true);
							unwalled ++;
						}
					}else {
						GUTIL.marker().set(c, d, true);
						unwalled ++;
					}
					continue;
				}
			}
		}
		
		GUTIL.filler().done();
		
		int bonus = (int) Math.ceil(total/10.0);
		double v = total-unwalled + bonus;
		v /= total;
		v = CLAMP.d(v, 0, 1);
		v = MATH.pow15.pow(v);
		return v;
		
	}
	
	private MAP_BOOLEAN isWall;
	
	private final MAP_BOOLEAN wall = new MAP_BOOLEAN() {

		@Override
		public boolean is(int tile) {
			throw new RuntimeException();
		}

		@Override
		public boolean is(int tx, int ty) {
			if (!SETT.IN_BOUNDS(tx, ty))
				return false;
			if (isWall != null && isWall.is(tx,ty))
				return true;
			TerrainTile t = SETT.TERRAIN().get(tx, ty);
			if (t instanceof TFortification.Normal)
				return true;
			return t.clearing().isStructure() && t.getAvailability(tx, ty) != null && t.getAvailability(tx, ty).player < 0;
		}
		
		
	};
	
	private boolean isEdge(AREA r, COORDINATE c) {
		for (DIR d : DIR.ALL)
			if (!r.is(c, d))
				return true;
		return false;
	}
	
	final SAVABLE saver = new SAVABLE() {
		
		@Override
		public void save(FilePutter file) {
			
		}
		
		@Override
		public void load(FileGetter file) throws IOException {
			
		}
		
		@Override
		public void clear() {
			
		}
	};
	
	private class Chunk {
		
		private final RecShort body;
		private boolean added;
		
		Chunk(int x1, int y1){
			x1*= CHUNKSIZE;
			y1 *= CHUNKSIZE;
			int x2 = x1+CHUNKSIZE;
			x2 = Math.min(x2, SETT.TWIDTH);
			int y2 = y1+CHUNKSIZE;
			y2 = Math.min(y2, SETT.THEIGHT);
			body = new RecShort(x1, x2, y1, y2);
		}
		
		
	}
	
}
