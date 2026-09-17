package settlement.room.main;

import static settlement.main.SETT.IN_BOUNDS;
import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TWIDTH;

import game.GameDisposable;
import init.constant.C;
import settlement.main.SETT;
import settlement.path.finders.SFinderFindable;
import settlement.room.main.ROOMS.RoomResource;
import settlement.room.main.employment.RoomEmploymentSimple;
import settlement.thing.pointlight.LOS;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.map.MAP_OBJECT;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LISTE;
import util.keymap.MAPPED;
import view.sett.ui.room.UIRoomModule;

public abstract class RoomBlueprint extends RoomResource implements MAP_OBJECT<Room>, MAPPED{

	private final int index;
	static ArrayListGrower<RoomBlueprint> ALL = new ArrayListGrower<>();
	static {
		new GameDisposable() {
			@Override
			protected void dispose() {
				ALL.clear();
			}
		};
	}
	
	public final String key;
	
	protected RoomBlueprint(String key) {
		index = ALL.add(this);
		this.key = key;
	}
	

	
	@Override
	public Room get(int tx, int ty) {
		if (IN_BOUNDS(tx, ty))
			return get(tx+ty*TWIDTH);
		return null;
	}

	@Override
	public Room get(int tile) {
		Room r = ROOMS().map.get(tile);
		if (r != null && r.blueprint() == this)
			return r;
		return null;
	}
	
	@Override
	public final int index() {
		return index;
	}

	public abstract SFinderFindable service(int tx, int ty);

	
	public abstract COLOR miniC(int tx, int ty);

	public abstract COLOR miniCPimped(ColorImp origional, int tx, int ty, boolean northern, boolean southern);
	
	public boolean makesDudesDirty() {
		return false;
	}

	public void appendView(LISTE<UIRoomModule> mm) {
		
	}
	
	public double strength(int tile) {
		return 400*C.TILE_SIZE;
	}

	public LOS LOS(int tx, int ty) {
		return SETT.TILE_MAP().LOS(tx, ty);
	}
	
	public RoomEmploymentSimple employment() {
		return null;
	}

	@Override
	public String key() {
		return key;
	}



	public boolean registersEnvironment() {
		return false;
	}



	
}
