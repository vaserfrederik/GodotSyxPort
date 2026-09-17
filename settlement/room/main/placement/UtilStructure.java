package settlement.room.main.placement;

import game.GAME;
import game.save.PROP;
import game.save.PROP.PropGame;
import init.structure.STRUCTURES;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.tilemap.terrain.TBuilding;
import settlement.tilemap.terrain.Terrain.TerrainTile;
import snake2d.util.datatypes.COORDINATE;
import util.data.GETTER.GETTER_IMP;

public final class UtilStructure extends GETTER_IMP<TBuilding> {
	
	private final RoomPlacer p;
	private final PropGame saver = PROP.game("ROOM_CONSTRUCTION");
	
	UtilStructure(RoomPlacer p){
		this.p = p;
		
		set(SETT.TERRAIN().BUILDINGS.MUD);
		
		
	}
	
	SETT_JOB getWallJob(int tx, int ty) {
		
		if (SETT.TERRAIN().CAVE.is(tx, ty))
			return SETT.JOBS().clearss.caveFill;
		return SETT.JOBS().build_structure.get(get().structure.index()).wall;
		
	}
	
	SETT_JOB getCeilingJob(int tx, int ty) {
		
		if (SETT.TERRAIN().MOUNTAIN.is(tx, ty))
			return null;
		if (get().roof.is(tx, ty))
			return null;
		if (SETT.TERRAIN().CAVE.is(tx, ty))
			return null;
		return SETT.JOBS().build_structure.get(get().structure.index()).ceiling;
	}
	
	void read() {
		String k = saver.chars("STRUCTURE");
		if (k != null && STRUCTURES.map().tryGet(k) != null) {
			set(STRUCTURES.map().tryGet(k).terrain());
		}
		
	}
	
	int unroofed = 0;
	int tick = 0;
	
	public int roofs() {
		
		if (GAME.updateI() != tick) {
			unroofed = 0;
			for (COORDINATE c : p.instance.body()) {
				if (!p.instance.is(c))
					continue;
				
				if (getCeilingJob(c.x(), c.y()) != null) {
					unroofed ++;
				}
			}
			if (p.autoWalls.is()) {
				unroofed += p.door.getOpenings();
			}
		}
		return unroofed;
	}
	@Override
	public void set(TBuilding t) {
		super.set(t);
		saver.charsSet("STRUCTURE", t.structure.key);
	}
	public int walls() {
		if (!p.autoWalls.is())
			return 0;
		return p.door.getWalls();
	}
	
	public int mountainWalls() {
		if (!p.autoWalls.is())
			return 0;
		return p.door.getMountains();
	}

	public void set(int tx, int ty) {
		TerrainTile t = SETT.TERRAIN().get(tx, ty);
		if (t instanceof TBuilding.BuildingComponent) {
			set(((TBuilding.BuildingComponent)t).building());
		}
			
		
	}
	
	
	
}
