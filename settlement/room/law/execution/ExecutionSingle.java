package settlement.room.law.execution;

import settlement.main.SETT;
import settlement.room.main.ROOMA;
import settlement.room.main.ROOMS;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.RoomSingleton;
import snake2d.util.datatypes.COORDINATE;

public class ExecutionSingle extends RoomSingleton{

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;

	protected ExecutionSingle(ROOMS m, RoomBlueprint p) {
		super(m, p);
	}

	@Override
	public ROOM_EXECTUTION blueprintI() {
		return SETT.ROOMS().EXECUTION;
	}
	
	@Override
	protected void addAction(ROOMA ins) {
		for (COORDINATE c : ins.body()) {
			if (ins.is(c)) {
				blueprintI().stations.init(c.x(), c.y());
			}
		}
		super.addAction(ins);
	}
	
	@Override
	protected void removeAction(ROOMA ins) {
		for (COORDINATE c : ins.body()) {
			if (ins.is(c)) {
				blueprintI().stations.dispose(c.x(), c.y());
			}
		}
		super.removeAction(ins);
	}
	
	@Override
	public void updateTileDay(int tx, int ty) {
		blueprintI().stations.update(tx, ty);
		super.updateTileDay(tx, ty);
	}

}
