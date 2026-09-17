package settlement.room.law.guard;

import java.io.IOException;

import game.time.TIME;
import init.constant.C;
import settlement.main.SETT;
import settlement.room.main.RoomInstance;
import settlement.room.main.throne.THRONE;
import snake2d.PathGame.PathSimple;
import snake2d.PathTile;
import snake2d.PathUtilOnline.Flooder;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.rnd.RND;
import util.GUTIL;

public final class Patrol implements SAVABLE {

	private static int WIDTH = 1;
	private static int DEPTH = 8;
	public static int MAX = (1 + WIDTH*2)*DEPTH;
	private static int MAX_TILES = 120;
	
	public static double speed = 0.5;
	
	private final PathSimple path = new PathSimple(MAX_TILES + 16);
	private double waitAtDest = RND.rFloat()*TIME.secondsPerHour()*3;
	private double progressTile = 0;
	private short[] txs = new short[DEPTH];
	private short[] tys = new short[DEPTH];
	private byte[] dirs = Alloc.bb(DEPTH);
	
	private Coo tmp = new Coo();
	
	Patrol(){
		
	}
	
	@Override
	public void save(FilePutter file) {
		file.d(waitAtDest);
		file.d(progressTile);
		file.ssE(txs);
		file.ssE(tys);
		file.bsE(dirs);
	}

	@Override
	public void load(FileGetter file) throws IOException {
		waitAtDest = file.d();
		progressTile = file.d();
		file.ssE(txs);
		file.ssE(tys);
		file.bsE(dirs);
		
	}

	@Override
	public void clear() {
		waitAtDest = 0;
		progressTile = 0;
	}
	
	void update(double ds) {
		
		if (!path.hasNext()) {
			waitAtDest-=ds;
			if (waitAtDest < 0) {
				find();
				
				waitAtDest = TIME.secondsPerHour()*(3+RND.rFloat()*3);
			}else {
			
			}
		}else {
			progressTile += ds*speed;
			
			if (progressTile > 1.0) {
				progressTile-=1.0;
				setNext();
				
			}
		}
	}
	
	private void setNext() {
		
		if (!path.hasNext())
			return;
		path.setNext();
		
		for (int i = DEPTH-1; i > 0; i--) {
			txs[i] = txs[i-1];
			tys[i] = tys[i-1];
			dirs[i] = dirs[i-1];
		}
		
		txs[0] = (short) path.x();
		tys[0] = (short) path.y();
		
		int x = path.x();
		int y = path.y();
		if (path.hasNext()) {
			path.setNext();
			DIR dirNext = DIR.get(x, y, path.x(), path.y());
			dirs[0] = (byte) dirNext.id();
			path.setPrev();
		}
	}
	
	private void find() {
		
		if (false) {
			//Find destinations for all patrols in a single run.
			//impove it generally
			
		}

		int sx = path.x();
		int sy = path.y();
		
		if (path.length() == 0 || !SETT.PATH().connectivity.is(path.x(), path.y())) {
			
			if (false || SETT.ROOMS().GUARD.instancesSize() > 0) {
				RoomInstance ins = SETT.ROOMS().GUARD.getInstance(RND.rInt(SETT.ROOMS().GUARD.instancesSize()));
				sx = ins.mX();
				sy = ins.mY();

			}else {
				sx = THRONE.coo().x();
				sy = THRONE.coo().y();
				DIR d = DIR.ORTHO.rnd();
				sx += d.x();
				sy += d.y();

			}
			
			
		}

		DIR dirDir = DIR.get(THRONE.coo().x(), THRONE.coo().y(), sx, sy);
		
		Flooder f = GUTIL.flooder();
		f.init(this);
		
		f.pushSloppy(sx, sy, 0);
		f.setValue2(sx, sy, 0);
		
		double destDist = MAX_TILES + RND.rInt(MAX_TILES);
		
		while(f.hasMore()) {
			
			PathTile t = f.pollSmallest();
			
			if (t.getValue2() > destDist) {
				
				GUTIL.coos().set(0);
				GUTIL.coos().get().set(t);
				
				while(f.hasMore()) {
					t = f.pollSmallest();
					if (t.getValue2() > destDist) {
						GUTIL.coos().inc();
						GUTIL.coos().get().set(t);
					}
				}
				t = GUTIL.pathTools().getTile(GUTIL.coos().get().x(), GUTIL.coos().get().y());

				path.set(t);

				for (int i = 0; i < DEPTH; i++)
					setNext();
				f.done();
				return;
			}
			
			for (int di = 0; di < DIR.ALL.size(); di++) {
				DIR d = DIR.ALL.get(di);
				int dx = t.x() + d.x();
				int dy = t.y() + d.y();
				
				if (!SETT.IN_BOUNDS(dx, dy)) {
					continue;
				}
				
				double a = SETT.PATH().coster.player.getCost(t.x(), t.y(), dx, dy);
				if (a < 0)
					continue;
				
				if (SETT.ENV().map.URBAN.get(dx, dy) == 0)
					a *= 5;
				
				a *= 10 - 9*SETT.ENV().map.SPACE.get(dx, dy);
				if (SETT.FLOOR().getter.get(dx, dy) == null)
					a *= 4;
				if (SETT.ROOMS().map.is(dx, dy)) {
					a *= 4;
				}
				
				double dot = d.xN()*dirDir.xN() + d.yN()*dirDir.yN();
				a += 2 + dot;
				
				a*=d.tileDistance();
				
				if (f.pushSmaller(dx, dy, t.getValue()+a, t) != null) {
					f.setValue2(dx, dy, t.getValue2()+d.tileDistance());
				}
				
			}
			
			
		}

		f.done();
		return;
		
		
	}
	
	public Coo pos(int pos) {
		int side = pos/DEPTH;
		pos = pos%DEPTH;
		DIR d = dir(pos);
		
		double tx = txs[pos] + d.x()*progressTile;
		double ty = tys[pos] + d.y()*progressTile;
		
		if (side != 0) {
			int depth = (int) Math.ceil(side/2.0);
			d = d.next(2 + (side%2)*4);
			tx += d.xN()*(depth);
			ty += d.yN()*(depth);
		}
		tmp.set(tx*C.TILE_SIZE + C.TILE_SIZEH, ty*C.TILE_SIZE + C.TILE_SIZEH);
		return tmp;
		
	}
	
	public DIR dir(int pos) {
		pos = pos%DEPTH;
		return DIR.ALL.get(dirs[pos]);
	}
	
	public int posses() {
		return MAX;
	}


	
}
