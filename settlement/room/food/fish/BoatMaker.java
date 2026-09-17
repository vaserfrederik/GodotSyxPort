package settlement.room.food.fish;

import settlement.main.SETT;
import settlement.room.main.RoomInstance;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;

class BoatMaker {
	
	public static void make(FishInstance ins) {
		{
			for (COORDINATE c : ins.body()) {
				if (ins.is(c)) {
					if (SETT.ROOMS().fData.tile.get(c) != null) {
						continue;
					}
					
					if(SETT.TERRAIN().WATER.SHALLOW.is(c) && SETT.TERRAIN().WATER.deepSeaFishSpot.is(c)) {
						make(ins, c);
					}
				}
			}
		}
	}
	
	private static void make(FishInstance ins, COORDINATE start) {
		for (int di = 0; di < DIR.ALL.size(); di++) {
			if (make(ins, start, DIR.ALL.get(di)))
				return;
		}
		
	}
	
	private static boolean make(FishInstance ins, COORDINATE start, DIR dir) {
		

		int fx = start.x();
		int fy = start.y();
		for (int i = 0; i <= RoomInstance.MAX_DIM; i++) {
			int tx = fx + dir.x();
			int ty = fy + dir.y();
			if (ins.is(fx, fy) && SETT.TERRAIN().WATER.SHALLOW.is(fx, fy) && !SETT.TERRAIN().WATER.open.is(fx, fy)) {
				
				boolean border = true;
				for (DIR d : DIR.ALL) {
					if (!ins.is(fx, fy, d) || Job.isWork.is(SETT.ROOMS().data.get(fx, fy, d))) {
						border = false;
						break;
					}
				}
				if (border) {
					int data = Job.isWork.set(0);
					data = Job.isShip.set(data);
					data = Job.shipDir.set(data, dir.perpendicular().id());
					SETT.ROOMS().data.set(ins, fx, fy, data);
					return true;
				}else
					return false;
			}
			if (!ins.body().holdsPoint(tx, ty))
				return false;
			if (!SETT.TERRAIN().WATER.is.is(tx, ty))
				return false;
			if (!dir.isOrtho())
				if (!SETT.TERRAIN().WATER.is.is(fx, ty) || !!SETT.TERRAIN().WATER.is.is(tx, fy))
					return false;
			
			fx = tx;
			fy = ty;
			
			
		}
		return false;
	}
	
}
