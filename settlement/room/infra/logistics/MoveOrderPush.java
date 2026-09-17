package settlement.room.infra.logistics;

import java.io.Serializable;

import init.resources.RBIT;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.main.SETT;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVE_DEST;
import settlement.room.main.Room;
import settlement.room.main.RoomInstance;
import snake2d.util.datatypes.Coo;
import util.text.D;

public final class MoveOrderPush implements Serializable{

	private static CharSequence ¤¤RoomInvalid = "The destination room is invalid.";
	private static CharSequence ¤¤ResNone = "No resource have been specified.";
	private static CharSequence ¤¤ResBad = "The resources specified can not be accepted by the destination room.";
	private static CharSequence ¤¤ResANone = "There are not enough resources stocked to deliver.";
	private static CharSequence ¤¤ResANoneDest = "The destination does not have the capacity to accept a delivery.";
	private static CharSequence ¤¤limit = "The destination room's current storage exceeds the current push limit";
	
	
	static {
		D.ts(MoveOrderPush.class);
	}
	
	private final Coo coo = new Coo();
	public byte cooldown = 0;
	private short ox,oy;
	public byte limit = 80;
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;

	public MoveOrderPush(RoomInstance dest) {
		coo.set(dest.mX(), dest.mY());
	}
	
	public CharSequence problem(MoveOrderPushInstance ins) {
		ROOM_MOVE_DEST dest = dest();
		return problem(dest, ins);
	}
	
	public static CharSequence problem(ROOM_MOVE_DEST dest, MoveOrderPushInstance ins) {
		if (dest == null)
			return ¤¤RoomInvalid;
		if (ins.moveOrderPushCapacity().isClear())
			return ¤¤ResNone;
		if (!ins.moveOrderPushCapacity().has(dest.destSpaceMask()))
			return ¤¤ResBad;
		return null;
	}
	
	public CharSequence warning(MoveOrderPushInstance ins) {
		CharSequence p = problem(ins);
		if (p != null)
			return p;
		ROOM_MOVE_DEST dest = dest();
		
		RBIT acc = ins.moveOrderPushAvailable();
		if (!acc.has(dest.destSpaceMask())) {
			return ¤¤ResANone;
		}
		
		boolean h = false;
		for (RESOURCE r : RESOURCES.ALL()) {
			if (acc.has(r) && limit > dest.storedD(r)*100) {
				h = true;
				break;
			}
		}
		
		if (!h) {
			return ¤¤limit;
		}
		
		TILE_STORAGE t = dest.destCrate(acc, ins.moveMinAmount(), ox, oy);
		if (t == null) {
			return ¤¤ResANoneDest;
		}else {
			ox = (short) t.x();
			oy = (short) t.y();
		}
			
		
		return null;
	}
	
	public void destSet(RoomInstance dest) {
		coo.set(dest.mX(), dest.mY());
	}
	
	public ROOM_MOVE_DEST dest(){
		Room r = SETT.ROOMS().map.get(coo);
		if (r != null && r instanceof MoveJob.ROOM_MOVE_DEST) {
			return (ROOM_MOVE_DEST) r;
		}
		return null;
	}

	public RoomInstance destI(){
		Room r = SETT.ROOMS().map.get(coo);
		if (r != null && r instanceof MoveJob.ROOM_MOVE_DEST) {
			return (RoomInstance) r;
		}
		return null;
	}
	
	public interface MoveOrderPushInstance {
		
		public MoveOrderPush[] moveOrdersPush();
		public RBIT moveOrderPushCapacity();
		public RBIT moveOrderPushAvailable();
		public int moveMinAmount();
		public int moveMaxRadius();
	}

}
