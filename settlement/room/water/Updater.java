package settlement.room.water;

import java.io.IOException;

import settlement.main.SETT;
import snake2d.PathTile;
import snake2d.PathUtilOnline.Flooder;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.datatypes.RECTANGLE;
import snake2d.util.datatypes.Rec;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.ArrayListResize;
import snake2d.util.sets.Bitmap2D;
import snake2d.util.sets.LIST;
import snake2d.util.sets.Queue;
import util.GUTIL;

final class Updater {


	private final ROOM_WATER w;
	private Grid grid1 = new Grid();
	private final ArrayListResize<PumpInstance> pumps = new ArrayListResize<PumpInstance>(128);
	private final LIST<DIR> dirs = DIR.ORTHO.join(DIR.C);
	private final LIST<DIR> ortho = new ArrayList<>(DIR.ORTHO);
	private final double tilesPerSecond = 1;
	private double timer;
	
	public int ops = 0;
	
	public final SAVABLE saver = new SAVABLE() {
		
		@Override
		public void save(FilePutter file) {
			file.d(timer);
			grid1.save(file);
		}
		
		@Override
		public void load(FileGetter file) throws IOException {
			timer = file.d();
			grid1.load(file);
		}
		
		@Override
		public void clear() {
			timer = 0;
			grid1.clear();
		}
	};
	
	public Updater(ROOM_WATER water) {
		this.w = water;
	}
	
	public void reportChange(int tx, int ty, int radius) {
		
		int i = 0;
		while (GUTIL.circle().radius(i) < radius) {
			ops++;
			int dx = GUTIL.circle().get(i).x() + tx;
			int dy = GUTIL.circle().get(i).y() + ty;
			if (w.pumpable.get(dx, dy) != null) {
				grid1.mark(dx, dy);
			}
			
			i++;
		}
		
		for (DIR d : dirs) {
			if (SETT.IN_BOUNDS(tx, ty, d)) {
				grid1.mark(tx+d.x(), ty+d.y());
			}
		}
		
		
		
		
	}
	
	void update(double ds) {

		
		
		timer += ds*tilesPerSecond;
		if (timer < 1)
			return;
		
		timer -= (int)timer;
		

		ops = 0;

		RECTANGLE bb = grid1.pollNext();
		while(bb != null) {
			
			for (COORDINATE c : bb) {
				if (grid1.mark.is(c)) {
					grid1.mark.set(c, false);
					if (w.pumpable.get(c.x(), c.y()) != null) {
						pumps.clearSoft();
						ops ++;
						drain(c.x(), c.y(), pumps);
						if (pumps.size() > 0)
							fill(pumps);
						else
							fail(c.x(), c.y());
					}
				}
			}
			
			
			bb = grid1.pollNext();
			
			
		}
		
	}
	
	private void drain(int tx, int ty, ArrayListResize<PumpInstance> pumps) {
		Flooder f = GUTIL.flooder();
		f.init(this);
		f.pushSloppy(tx, ty, 0);
		
		while(f.hasMore()) {
			PathTile t = f.pollSmallest();
			RoomPumpable p = w.pumpable.get(t.x(), t.y());
			
			t.setValue2(p.dirmask(t.x(), t.y()));
			
			p.drain(t.x(), t.y());
			grid1.mark.set(t, false);
			ops++;
			
			if (p.radius() > 0) {
				int rr = p.radius();
				int i = 0;
				while (GUTIL.circle().radius(i) < rr) {
					ops++;
					int dx = GUTIL.circle().get(i).x() + t.x();
					int dy = GUTIL.circle().get(i).y() + t.y();
					if (w.pumpable.get(dx, dy) == p) {
						f.pushSmaller(dx, dy, t.getValue()+GUTIL.circle().radius(i), t);
					}
					
					i++;
				}
				
				
			}
			
			for (DIR d : ortho) {
				if (w.pumpable.get(t, d) != null)
					f.pushSmaller(t, d, t.getValue()+1, t);
				else {
					PumpInstance ins = w.pump.get(t.x()+d.x(), t.y()+d.y());
					if (ins != null && ins.ox() == t.x()+d.x() && ins.oy() == t.y()+d.y()) {
						pumps.add(ins);
					}
					
					
				}
			}
			
		}
		f.done();
		
	}
	
	private void fail(int tx, int ty) {
		Flooder f = GUTIL.flooder();
		f.init(this);
		f.pushSloppy(tx, ty, 0);
		
		while(f.hasMore()) {
			PathTile t = f.pollSmallest();
			RoomPumpable p = w.pumpable.get(t.x(), t.y());
			p.pumpFail(t.x(), t.y(), (int)t.getValue2());
			
			if (p.radius() > 0) {
				int rr = p.radius();
				int i = 0;
				while (GUTIL.circle().radius(i) < rr) {
					ops++;
					int dx = GUTIL.circle().get(i).x() + t.x();
					int dy = GUTIL.circle().get(i).y() + t.y();
					if (w.pumpable.get(dx, dy) == p) {
						f.pushSmaller(dx, dy, t.getValue()+GUTIL.circle().radius(i), t);
					}
					
					i++;
				}
				
				
			}
			
			for (DIR d : ortho) {
				if (w.pumpable.get(t, d) != null)
					f.pushSmaller(t, d, t.getValue()+1, t);
			}
			
		}
		f.done();
		
	}
	
	private void fill(LIST<PumpInstance> pumps) {
		
		if (pumps.size() == 0)
			return;
		
		Flooder f = GUTIL.flooder();
		f.init(this);
		double am = 0;
		for (PumpInstance ins : pumps) {
			int a = ins.output();
			am += a;
			if (a > 0)
				f.pushSloppy(ins.ox(), ins.oy(), 0);
		}
		
		while(f.hasMore()) {
			PathTile t = f.pollSmallest();
			ops++;
			RoomPumpable p = w.pumpable.get(t);
			if (p != null) {
				if (am <= 0) {
					p.pumpFail(t.x(), t.y(), (int)t.getValue2());
					
				}else {
					if (t.getParent() != null)
						p.pump(t.x(), t.y(), DIR.get(t.getParent(), t), (int)t.getValue2());
					am -= p.suckAmount(t.x(), t.y());
					
					
					if (p.radius() > 0) {
						int rr = p.radius();
						int i = 0;
						while (GUTIL.circle().radius(i) < rr-2) {
							ops++;
							int dx = GUTIL.circle().get(i).x() + t.x();
							int dy = GUTIL.circle().get(i).y() + t.y();
							
							if (w.pumpable.get(dx, dy) == p) {
								f.pushSmaller(dx, dy, t.getValue()+GUTIL.circle().radius(i), t);
							}
							
							i++;
						}
						am -= p.radius();
						
					}
				}
				
				
				
				for (DIR d : ortho) {
					if (w.pumpable.get(t, d) != null && p.pumpsTo(t.x(), t.y(), t.x()+d.x(), t.y()+d.y()))
						f.pushSmaller(t, d, t.getValue()+1, t);
						
				}
				
			}else {
				for (DIR d : ortho) {
					if (w.pumpable.get(t, d) != null)
						f.pushSmaller(t, d, t.getValue()+1, t);
						
				}
			}
			
			
			
		}
		f.done();
		
	}
	

	
	private static class Grid implements SAVABLE{
		
		final int w = (int)Math.ceil((double)SETT.TWIDTH/GridTile.size);
		final int h = (int)Math.ceil((double)SETT.THEIGHT/GridTile.size);
		private final Queue<GridTile> active = new Queue<>(w*h);
		private final GridTile[][] grid = new GridTile[w][h];
		private final Bitmap2D mark = new Bitmap2D(SETT.TILE_BOUNDS, false);
		
		Grid() {
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++)
					grid[y][x] = new GridTile(x, y);
			}
		}
		
		@Override
		public void save(FilePutter file) {
			
			for (GridTile[] tt : grid) {
				for (GridTile t : tt)
					file.bool(t.marked);
			}
			mark.save(file);
		}

		@Override
		public void load(FileGetter file) throws IOException {
			active.clear();
			for (GridTile[] tt : grid) {
				for (GridTile t : tt) {
					t.marked = file.bool();
					if (t.marked) {
						t.marked = false;
						mark(t.body.cX(), t.body.cY());
					}
				}
			}
			mark.load(file);
		}

		@Override
		public void clear() {
			mark.clear();
			active.clear();
			for (GridTile[] tt : grid) {
				for (GridTile t : tt) {
					t.marked = false;
				}
			}
		}
		
		public RECTANGLE pollNext() {
			if (active.size() == 0)
				return null;
			GridTile t = active.poll();
			t.marked = false;
			return t.body;
		}
		
		public void mark(int tx, int ty) {
			mark.set(tx, ty, true);
			tx /= GridTile.size;
			ty /= GridTile.size;
			GridTile t = grid[ty][tx];
			if (!t.marked) {
				t.marked = true;
				active.push(t);
			}
		}
		
		private static class GridTile {
			
			public final static int size = 16;
			private final RECTANGLE body;
			private boolean marked = false;
			
			GridTile(int gx, int gy){
				
				int x2 = Math.min(gx*size+size, SETT.TWIDTH);
				int y2 = Math.min(gy*size+size, SETT.THEIGHT);
				
				body = new Rec(gx*size, x2,  gy*size, y2);
			}
			
		}


		
	}
	

	
}
