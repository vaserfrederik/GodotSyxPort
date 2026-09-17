package settlement.room.service.arena;

import settlement.room.main.RoomInstance;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.RECTANGLE;

public interface RoomArenaWork {

	public abstract COORDINATE gladiatorGetSpot(RoomInstance ins);
	public RECTANGLE gladiatorArea(int tx, int ty);
	public boolean gladiatorInArena(int tx, int ty);
	public void gladiatorDrawMakeSheer(COORDINATE coo);
	public RoomInstance reserveDeath(COORDINATE coo);
	public void unreserveDeath(int tx, int ty);
	
	public int executions();
	public int executionsMax();
	public int executions(RoomInstance ins);
	public int executionsMax(RoomInstance ins);
}
