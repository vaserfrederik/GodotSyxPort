package settlement.room.infra.builder;

import game.time.TIME;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.path.AVAILABILITY;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.ROOM_RADIUS.ROOM_RADIUS_INSTANCE;
import settlement.room.main.util.RoomInit;

final class BuilderInstance extends RoomInstance implements ROOM_RADIUS_INSTANCE{

	private static final long serialVersionUID = 1L;
	byte radius = 32;
	byte failHour = -1;
	
	BuilderInstance(ROOM_BUILDER blueprint, TmpArea area, RoomInit init) {
		super(blueprint, area, init);
		SETT.ROOMS().data.set(this, mX(), mY(), 0);
		employees().maxSet(20);
		employees().neededSet(1);
		activate();
	}

	@Override
	protected void activateAction() {
		// TODO Auto-generated method stub
		
	}

	@Override
	protected void deactivateAction() {
		// TODO Auto-generated method stub
		
	}
	
	@Override
	protected void updateAction(double updateInterval, boolean day) {
		failHour = (byte) (TIME.hours().bitCurrent() -1);
	}

	@Override
	protected void dispose() {
		// TODO Auto-generated method stub
		
	}

	@Override
	public ROOM_BUILDER blueprintI() {
		return SETT.ROOMS().BUILDER;
	}
	
	@Override
	protected AVAILABILITY getAvailability(int tile) {
		return AVAILABILITY.AVOID_PASS;
	}
	
	@Override
	public boolean destroyTileCan(int tx, int ty) {
		return false;
	}

	@Override
	public ROOM_DEGRADER degrader(int tx, int ty) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public int radius() {
		return radius;
	}

	@Override
	public boolean searching() {
		return true; 
	}

	@Override
	public byte radiusRaw() {
		return radius;
	}

	@Override
	public void radiusRawSet(byte r) {
		this.radius = r;
	}


}
