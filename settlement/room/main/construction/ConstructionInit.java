package settlement.room.main.construction;

import static settlement.main.SETT.TERRAIN;

import settlement.main.SETT;
import settlement.room.main.ROOMA;
import settlement.room.main.Room;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomAreaWrapper;
import settlement.room.main.util.RoomState;
import settlement.tilemap.terrain.TBuilding;
import settlement.tilemap.terrain.TBuilding.BuildingComponent;
import settlement.tilemap.terrain.Terrain.TerrainTile;
import snake2d.util.datatypes.COORDINATE;

public class ConstructionInit {

	public final int upgrade;
	public final Furnisher b;
	public final TBuilding structure;
	public final int degrade;
	public final RoomState state;
	
	private static RoomAreaWrapper wrap = new RoomAreaWrapper();
	
	public ConstructionInit(int upgrade, Furnisher b, TBuilding structure, int degrade, RoomState state) {
		this.upgrade = upgrade;
		this.structure = structure;
		this.degrade = degrade;
		this.state = state;
		this.b = b;		
	}
	
	public ConstructionInit(Room room, int rx, int ry, boolean broken) {
		this(room, rx, ry, room.degrader(rx, ry) == null ? 0 : room.degrader(rx, ry).getData(), broken);
	}
	
	private ConstructionInit(Room room, int rx, int ry, int degrade, boolean broken) {
		this(room.upgrade(rx, ry), room.constructor(), findStructure(rx, ry), degrade, room.makeState(rx, ry, broken));
	}
	
	public static TBuilding findStructure(int rx, int ry) {
		
		Room room = SETT.ROOMS().map.get(rx, ry);
		
		if (room == null)
			return null;
		
		if (room instanceof ConstructionInstance)
			return ((ConstructionInstance) room).structure();
		
		ROOMA a = wrap.init(room, rx, ry);
		wrap.done();
		for (COORDINATE c : a.body()) {
			if (a.is(c)) {
				TerrainTile t = TERRAIN().get(c.x(), c.y());
				if (t instanceof BuildingComponent)
					return ((BuildingComponent)TERRAIN().get(c.x(), c.y())).building();
			}
		}
		
		return TERRAIN().BUILDINGS.MUD;
	}
	
}
