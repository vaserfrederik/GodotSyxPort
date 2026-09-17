package settlement.room.main.throne;

import static settlement.main.SETT.JOBS;
import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TERRAIN;

import settlement.main.SETT;
import util.GUTIL;

public final class Initer {

	Initer(THRONE t){
		
	}
	
	
	public boolean placebleWhole(int x1, int y1, int rot) {
		int x2 = x1 + Sprite.width(rot);
		int y2 = y1 + Sprite.height(rot);
		
		for (int y = y1; y < y2; y++) {
			for (int x = x1; x < x2; x++) {
				if (!placableTile(x, y))
					return false;
			}
		}
		return true;
	}
	
	public boolean placableTile(int tx, int ty) {
		if (JOBS().getter.is(tx, ty))
			return false;
		if (ROOMS().map.is(tx, ty))
			return false;
		if (TERRAIN().TREES.isTree(tx, ty))
			return true;
		if (!TERRAIN().NADA.is(tx, ty) && !TERRAIN().get(tx, ty).roofIs() && !TERRAIN().get(tx, ty).clearing().isEasilyCleared())
			return false;
		return true;
	}
	
	public void markArround(int tx, int ty) {
		int i = 0;
		while (GUTIL.circle().radius(i) < 100) {
			int x = tx+GUTIL.circle().get(i).x();
			int y = ty+GUTIL.circle().get(i).y();
			
			if (placebleWhole(x, y, 0)) {
				SETT.ROOMS().THRONE.setInstance(tx, ty);
				return;
			}
			i++;
		}
		
		SETT.ROOMS().THRONE.setInstance(tx, ty);
		//throw new RuntimeException();
	}
	
	
	public void place(int x1, int y1, int rot) {
		new Instance(x1, y1, rot);
	}
	
}
