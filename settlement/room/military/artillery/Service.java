package settlement.room.military.artillery;

import game.battle.Army;
import init.constant.C;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.path.finders.SFinderSoldierManning.FINDABLE_MANNING;
import snake2d.util.datatypes.DIR;

class Service {

	private final ROOM_ARTILLERY blue;
	private int x,y;
	private ArtilleryInstance ins;
	
	Service(ROOM_ARTILLERY blue){
		this.blue = blue;
	}
	
	public FINDABLE_MANNING get(int tx, int ty) {
		ins = blue.get(tx, ty);
		if (ins != null && ins.mustered()) {
			if (SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.SERVICE) {
				x = tx;
				y = ty;
				return ser;
			}
		}
		return null;
	}
	
	public void activate(int tx, int ty) {
		if (get(tx, ty) != null) {
			SETT.ROOMS().data.set(ins, x, y, 0);
			blue.service(tx, ty).report(tx,  ty, 1);
		}
	}
	
	public void deactivate(int tx, int ty) {
		if (get(tx, ty) != null) {
			if (get(tx, ty).findableReservedCanBe())
				blue.service(tx, ty).report(tx,  ty, -1);
			SETT.ROOMS().data.set(ins, x, y, 0);
		}
	}
	
	
	private final FINDABLE_MANNING ser = new FINDABLE_MANNING() {
		
		@Override
		public int y() {
			return y;
		}
		
		@Override
		public int x() {
			return x;
		}
		
		@Override
		public boolean findableReservedIs() {
			return SETT.ROOMS().data.get(x, y) == 1;
		}
		
		@Override
		public boolean findableReservedCanBe() {
			return !findableReservedIs();
		}
		
		@Override
		public void findableReserveCancel() {
			if (findableReservedIs()) {
				blue.service(x, y).report(x,  y, 1);
				SETT.ROOMS().data.set(ins, x, y, 0);
				ins.men--;
			}
			
		}
		
		@Override
		public void findableReserve() {
			if (!findableReservedIs()) {
				blue.service(x, y).report(x,  y, -1);
				SETT.ROOMS().data.set(ins, x, y, 1);
				ins.men++;
			}
			
		}

		@Override
		public DIR faceDIR() {
			return DIR.get((x<<C.T_SCROLL)+C.TILE_SIZEH,  (y<<C.T_SCROLL)+C.TILE_SIZEH, ins.centre());
		}

		@Override
		public void work(double time, Humanoid a) {
			ins.work(time, a);
			
		}

		@Override
		public boolean needsWork() {
			return ins.needsWork();
		}

		@Override
		public Army army() {
			return ins.army();
		}
	};
	
	
}
