package settlement.room.infra.logistics;

import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.util.RESOURCE_TILE;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.main.RoomInstance;
import snake2d.util.datatypes.Coo;
import snake2d.util.misc.CLAMP;

public final class MoveJob {

	public static final MoveJob TMP = new MoveJob();
	private static final RBITImp tmp = new RBITImp();
	public final Coo source = new Coo();
	public final Coo dest = new Coo();
	public RESOURCE res;
	public int maxAm;
	public boolean stored;
	public boolean prio;
	MoveJob(){
		
	}
	
	public interface ROOM_MOVEJOBBER {
		public MoveJob moveJob(Humanoid skill);
	}
	
	public interface ROOM_MOVE_DEST {
		public TILE_STORAGE destCrate(RBIT okMask, int minAm, int ox, int oy);
		public default TILE_STORAGE fetchToCrate(RESOURCE res, int desiredAm) {
			return destCrate(res.bit, 1, -1, -1);
		}
		
		public RBIT destSpaceMask();
		public double storedD(RESOURCE res);
		public RBIT moveCapacity();
	}
	
	public interface ROOM_MOVE_SOURCE {
		public RESOURCE_TILE sourceCrate(RBIT okMask, int minAm, int ox, int oy, double lim);
		public RBIT sourceAmountMask();
		public RBIT moveCapacity();
		public int moveCapacityAm(RESOURCE res);
		public double storedD(RESOURCE res);
	}
	
	public void cancel() {
		
	}
	
//	public static MoveJob fetch(RoomInstance ins, ROOM_MOVE_DEST accepter, int am, int radius, int ox, int oy, RBIT scattered, RBIT stored) {
//		return fetch(ins, accepter, am, radius, ox, oy, bits, RBIT.NONE);
//		
//	}
	
	public static MoveJob fetch(RoomInstance ins, ROOM_MOVE_DEST accepter, int am, int radius, int ox, int oy, RBIT scatterd, RBIT stored) {

		if (scatterd.isClear() && stored.isClear())
			return null;
		
		am = CLAMP.i(am, 1, 100);
		
		RESOURCE_TILE t = RESOURCE_TILE.GETTER.reservable(scatterd, stored, RBIT.NONE, ox, oy);  

//		if (t == null)
//			if (ins.blueprintI() == SETT.ROOMS().STOCKPILE) {
//				LOG.ln("no");
//			}
		
		if (t == null)
			t = SETT.PATH().finders.resource.find(scatterd, stored, RBIT.NONE, ins, radius);

		if (t == null)
			return null;
		
		MoveJob.TMP.source.set(t.x(), t.y());
		
		RESOURCE r = t.resource();
		MoveJob.TMP.stored = t.isStorage();
		MoveJob.TMP.prio = t.isPrio();
		MoveJob.TMP.res = r;
		
		tmp.clearSet(r.bit);
		
		TILE_STORAGE c = accepter.fetchToCrate(r, am);
		
		if (c == null)
			return null;

		
		am = Math.min(c.storageReservable(), am);
		if (am <= 0)
			throw new RuntimeException();
		
		MoveJob.TMP.maxAm = am;
		MoveJob.TMP.dest.set(c.x(), c.y());
		return MoveJob.TMP;
		
	}
	
}

