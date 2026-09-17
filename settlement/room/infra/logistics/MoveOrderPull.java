package settlement.room.infra.logistics;

import java.io.IOException;
import java.io.ObjectInputStream;
import java.io.Serializable;

import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import settlement.main.SETT;
import settlement.misc.util.RESOURCE_TILE;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVE_DEST;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVE_SOURCE;
import settlement.room.infra.logistics.MoveOrderPush.MoveOrderPushInstance;
import settlement.room.main.Room;
import settlement.room.main.RoomInstance;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import util.text.D;

public final class MoveOrderPull implements Serializable {

	private static CharSequence ¤¤RoomInvalid = "The room to pull from is invalid";
	private static CharSequence ¤¤noRes = "No resources have been selected.";
	private static CharSequence ¤¤noResBoth = "Neither the source or destination room resources match the selected resources.";
	private static CharSequence ¤¤noResSource = "The source room does not have any crates available of the selected resources.";
	private static CharSequence ¤¤noResDest = "The destination room's resources doesn't match the selected resources.";
	private static CharSequence ¤¤noResSourceA = "The source room does not have any resources available to be pulled.";
	private static CharSequence ¤¤noResDestA = "The destination room does not have any crates available of the the selected resources.";
	private static CharSequence ¤¤noResBothA = "Neither the source nor destination room have crates available.";
	private static CharSequence ¤¤cycle = "The order is cyclic. Resources will be moved back and forth.";
	static {
		D.ts(MoveOrderPull.class);
	}
	private final static RBITImp tmp = new RBITImp();
	private final Coo coo = new Coo();
	public final RBITImp resbits = new RBITImp();
	public byte cooldown = 0;
	public byte pullLimit;

	private short lsx, lsy, ldx, ldy;

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;

	public MoveOrderPull(COORDINATE coo, RBIT res) {
		this.coo.set(coo.x(), coo.y());
		resbits.clearSet(res);
	}
	
	public MoveOrderPull(RoomInstance dest, RBIT res) {
		coo.set(dest.mX(), dest.mY());
		resbits.clearSet(res);
	}

	private void readObject(ObjectInputStream ois) throws ClassNotFoundException, IOException {
		ois.defaultReadObject();
	}

	public CharSequence problem(MoveOrderPullInstance ins) {
		return problem(source(), ins, resbits);
	}

	public static CharSequence problem(ROOM_MOVE_SOURCE source, MoveOrderPullInstance ins, RBIT bits) {
		if (source == null)
			return ¤¤RoomInvalid;

		if (bits.isClear())
			return ¤¤noRes;

		if (!source.moveCapacity().has(bits)) {
			return ¤¤noResSource;
		}
		if (!ins.moveOrderPullAccepted().has(bits)) {
			return ¤¤noResDest;
		}

		tmp.clearSet(bits).and(source.moveCapacity()).and(ins.moveOrderPullAccepted());

		if (tmp.isClear())
			return ¤¤noResBoth;

		return null;
	}

	public CharSequence warning(MoveOrderPullInstance ins) {
		CharSequence p = problem(ins);
		if (p != null)
			return p;

		ROOM_MOVE_SOURCE source = source();

		tmp.clear();
		tmp.or(source.moveCapacity());
		tmp.and(resbits);
		
		
		
		if (tmp.isClear()) {
			return ¤¤noResSource;
		}
		
		tmp.and(source.sourceAmountMask());
		
		if (tmp.isClear()) {
			return ¤¤noResSourceA;
		}

		RBIT bb = ins.moveOrderPullAvailable();
		if (!bb.has(resbits)) {
			return ¤¤noResDestA;
		}

		tmp.and(bb);

		if (tmp.isClear()) {
			return ¤¤noResBothA;
		}

		RESOURCE_TILE t = source.sourceCrate(tmp, ins.moveMinAmount(), lsx, lsy, pullLimit/100.0);
		if (t == null) {
			return ¤¤noResSourceA;
		} else {
			lsx = (short) t.x();
			lsy = (short) t.y();
		}

		RoomInstance isource = (RoomInstance) source;

		if (isource instanceof MoveOrderPullInstance) {
			MoveOrderPullInstance oo = (MoveOrderPullInstance) isource;
			for (MoveOrderPull o : oo.moveOrdersPull()) {
				if (o != null && o.source() == ins) {

					if (ins.moveOrderPullAccepted().has(oo.moveOrderPullAccepted())
							&& resbits.has(oo.moveOrderPullAccepted()) && o.resbits.has(resbits)) {
						return ¤¤cycle;
					}
				}
			}

		}
		
		if (ins instanceof MoveOrderPushInstance) {
			MoveOrderPushInstance pi = (MoveOrderPushInstance) ins;
			for (MoveOrderPush po : pi.moveOrdersPush()) {
				if (po != null) {
					for (MoveOrderPull puo : ins.moveOrdersPull()) {
						if (ins.moveOrderPullAccepted().has(puo.source().moveCapacity())){
							return ¤¤cycle;
						}
					}
				}
			}
		}
		

		return null;

	}

	public void destSet(RoomInstance dest) {
		coo.set(dest.mX(), dest.mY());
	}
	
	public COORDINATE destCoo() {
		return coo;
	}

	public ROOM_MOVE_SOURCE source() {
		Room r = SETT.ROOMS().map.get(coo);
		if (r != null && r instanceof ROOM_MOVE_SOURCE) {
			return (ROOM_MOVE_SOURCE) r;
		}
		return null;
	}

	public RoomInstance sourceI() {
		Room r = SETT.ROOMS().map.get(coo);
		if (r != null && r instanceof ROOM_MOVE_SOURCE) {
			return (RoomInstance) r;
		}
		return null;
	}

	public MoveJob job(ROOM_MOVE_DEST dest, int carryMin, int carryMax) {

		if (carryMin <= 0)
			throw new RuntimeException();

		if (carryMax <= 0)
			throw new RuntimeException();

		carryMin = Math.min(carryMin, carryMax);

		ROOM_MOVE_SOURCE source = source();
		if (source == null)
			return null;

		RBIT fetchBits = dest.destSpaceMask();
		if (!source().sourceAmountMask().has(fetchBits))
			return null;
		tmp.clear();
		tmp.or(fetchBits);
		tmp.and(resbits);

		if (tmp.isClear())
			return null;

		RESOURCE_TILE t = source.sourceCrate(tmp, carryMin, lsx, lsy, pullLimit/100.0);

		if (t == null)
			return null;
		if (t.resource() == null)
			throw new RuntimeException();
		if (!t.resource().bit.has(tmp))
			throw new RuntimeException();
		
		int am = Math.min(carryMax, t.reservable());
		if (am <= 0)
			throw new RuntimeException();
		lsx = (short) t.x();
		lsy = (short) t.y();
		MoveJob j = MoveJob.TMP;
		j.res = t.resource();

		j.source.set(t);
		j.stored = t.isStorage();
		j.prio = t.isPrio();
		TILE_STORAGE st = dest.destCrate(j.res.bit, carryMin, ldx, ldy);
		if (st == null)
			return null;
		am = Math.min(am, st.storageReservable());

		if (am <= 0) {
			throw new RuntimeException(st.storageReservable() + " " + dest);
		}
		j.maxAm = am;
		j.dest.set(st.x(), st.y());

//		st = dest.destCrate(j.res.bit, am, ldx, ldy);
//		if (st.resource() != j.res) {
//			throw new RuntimeException(st.resource() + " " + j.res);
//		}
		ldx = (short) st.x();
		ldy = (short) st.y();

		st = ((RoomInstance) dest).storage(j.dest.x(), j.dest.y());
		
		if (st.resource() != j.res) {
			throw new RuntimeException(st.resource() + " " + j.res);
		}

		return j;

	}

	public interface MoveOrderPullInstance {

		public MoveOrderPull[] moveOrdersPull();

		public RBIT moveOrderPullAccepted();

		public RBIT moveOrderPullAvailable();

		public int moveMinAmount();

		public int moveMaxRadius();
		
		public void copyFrom(MoveOrderPullInstance same);
	}

}
