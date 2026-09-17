package settlement.room.main.placement;

import static settlement.main.SETT.IN_BOUNDS;
import static settlement.main.SETT.JOBS;
import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TERRAIN;

import settlement.job.Job;
import settlement.main.SETT;
import settlement.tilemap.terrain.TBuilding;
import settlement.tilemap.terrain.Terrain.TerrainTile;
import snake2d.util.map.MAP_BOOLEAN;

public final class UtilWallPlacability{
	
	private UtilWallPlacability() {
		
	}
	
	public static final MAP_BOOLEAN openingIsReal = new MAP_BOOLEAN() {
		
		@Override
		public boolean is(int tx, int ty) {
			return IN_BOUNDS(tx, ty) && !ROOMS().map.is(tx, ty) && get(tx, ty).roofIs();
		}
		
		@Override
		public boolean is(int tile) {
			return is(tile%SETT.TWIDTH, tile/SETT.TWIDTH);
		}
	};
	
	public static final MAP_BOOLEAN openingCanBe = new MAP_BOOLEAN() {
		
		@Override
		public boolean is(int tx, int ty) {
			if (openingIsReal.is(tx, ty))
				return true;
			if (SETT.TERRAIN().MOUNTAIN.is(tx, ty))
				return placable(SETT.JOBS().clearss.tunnel,tx, ty) == null;
			else
				return placable(SETT.JOBS().build_structure.get(0).ceiling,tx, ty) == null;
		}
		
		@Override
		public boolean is(int tile) {
			return is(tile%SETT.TWIDTH, tile/SETT.TWIDTH);
		}
	};
	
	public static final MAP_BOOLEAN openingShouldBuild = new MAP_BOOLEAN() {
		
		@Override
		public boolean is(int tx, int ty) {
			if (openingIsReal.is(tx, ty))
				return false;
			return openingCanBe.is(tx, ty);
		}
		
		@Override
		public boolean is(int tile) {
			return is(tile%SETT.TWIDTH, tile/SETT.TWIDTH);
		}
	};
	
	public static final MAP_BOOLEAN wallisReal = new MAP_BOOLEAN() {
		
		@Override
		public boolean is(int tx, int ty) {
			return IN_BOUNDS(tx, ty) && !ROOMS().map.is(tx, ty) && get(tx, ty).isMassiveWall();
		}
		
		@Override
		public boolean is(int tile) {
			return is(tile%SETT.TWIDTH, tile/SETT.TWIDTH);
		}
	};
	
	public static final MAP_BOOLEAN wallCanBe = new MAP_BOOLEAN() {
		
		@Override
		public boolean is(int tx, int ty) {
			if (wallisReal.is(tx, ty))
				return true;
			if (SETT.TERRAIN().CAVE.is(tx, ty))
				return placable(SETT.JOBS().clearss.caveFill, tx, ty) == null;
			else
				return placable(SETT.JOBS().build_structure.get(0).wall, tx, ty) == null;
		}
		
		@Override
		public boolean is(int tile) {
			return is(tile%SETT.TWIDTH, tile/SETT.TWIDTH);
		}
	};
	
	public static final MAP_BOOLEAN wallShouldBuild = new MAP_BOOLEAN() {
		
		@Override
		public boolean is(int tx, int ty) {
			if (wallisReal.is(tx, ty))
				return false;
			return wallCanBe.is(tx, ty);
		}
		
		@Override
		public boolean is(int tile) {
			return is(tile%SETT.TWIDTH, tile/SETT.TWIDTH);
		}
	};
	
	private static TerrainTile get(int tx, int ty) {
		if (JOBS().getter.is(tx, ty))
			return JOBS().getter.get(tx, ty).becomes(tx, ty);
		return TERRAIN().get(tx, ty);
	}

	private static CharSequence placable(Job j, int tx, int ty) {
		boolean over = Job.overwrite;
		Job.overwrite = true;
		CharSequence s = j.placer().isPlacable(tx, ty, null, null);
		Job.overwrite = over;
		return s;
	}
	
	public static void wallBuild(int tx, int ty, TBuilding building) {
		
		
		if (SETT.ROOMS().map.is(tx, ty))
			return;
		if (SETT.TERRAIN().MOUNTAIN.is(tx, ty)) {
			SETT.JOBS().clearer.set(tx, ty);
			return;
		}
		if (building.wall.is(tx, ty)) {
			SETT.JOBS().clearer.set(tx, ty);
			return;
		}
		
		if (SETT.TERRAIN().CAVE.is(tx, ty)) {
			if (placable(SETT.JOBS().clearss.caveFill, tx, ty) == null)
				SETT.JOBS().clearss.caveFill.placer().place(tx, ty, null, null);
		}else if(SETT.TERRAIN().get(tx, ty) != building.wall && placable(SETT.JOBS().build_structure.get(building.structure.index()).wall, tx, ty) == null)
			SETT.JOBS().build_structure.get(building.structure.index()).wall.placer().place(tx, ty, null, null);
	}
	
	public static void openingBuild(int tx, int ty, TBuilding building) {
		if (SETT.ROOMS().map.is(tx, ty))
			return;
		if(SETT.TERRAIN().CAVE.is(tx, ty)) {
			SETT.JOBS().clearer.set(tx, ty);
			return;
		}
		if (building.roof.is(tx, ty)) {
			SETT.JOBS().clearer.set(tx, ty);
			return;
		}
		
		
		if (SETT.TERRAIN().MOUNTAIN.is(tx, ty)) {
			if (placable(SETT.JOBS().clearss.tunnel, tx, ty) == null)
				SETT.JOBS().clearss.tunnel.placer().place(tx, ty, null, null);
		}else if(placable(SETT.JOBS().build_structure.get(building.structure.index()).ceiling, tx, ty) == null)
			SETT.JOBS().build_structure.get(building.structure.index()).ceiling.placer().place(tx, ty, null, null);
	}

	
}
