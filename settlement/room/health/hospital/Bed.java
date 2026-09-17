package settlement.room.health.hospital;

import game.audio.SoundRace;
import game.faction.FACTIONS;
import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
import settlement.room.main.ROOMA;
import settlement.room.main.util.RoomBits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class Bed {

	private final Coo coo = new Coo();
	private HospitalInstance ins;
	
	private final RoomBits wreserved = 		new RoomBits(coo, 0b0000_0000_0001);
	private final RoomBits wres1 = 			new RoomBits(coo, 0b0000_0000_1110);
	private final RoomBits wres2 = 			new RoomBits(coo, 0b0000_0111_0000);
	private final RoomBits freeWork = 		new RoomBits(coo, 0b0000_1000_0000);
	private final RoomBits sstate = 		new RoomBits(coo, 0b1111_0000_0000) {
		
		@Override
		public void set(ROOMA r, int t) {
			ins.service().report(service, ins.blueprintI().service(), -1);
			super.set(r, t);
			ins.service().report(service, ins.blueprintI().service(), 1);
		};
		
	};
	
	private final int I_UNAVAILABLE = 0;
	private final int I_AVAILABLE = 1;
	private final int I_RESERVED = 2;
	private final int I_USED = 3;
	
	private static Bed self = new Bed();
	
	public static SETT_JOB job(int tx, int ty) {
		if (self.init(tx, ty))
			return self.job;
		return null;
	}
	
	public static FSERVICE service(int tx, int ty) {
		if (self.init(tx, ty))
			return self.service;
		return null;
	}
	public static boolean res1(int tx, int ty) {
		if (!self.init(tx, ty))
			return false;
		return self.wres1.get() > 0;
	}
	
	public static boolean res2(int tx, int ty) {
		if (!self.init(tx, ty))
			return false;
		return self.wres2.get() > 0;
	}
	
	public static void consume(int tx, int ty) {
		
	}
	
	private boolean init(int tx, int ty) {
		ins = b().get(tx, ty);
		if (ins != null && SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.CODE_S) {
			coo.set(tx, ty);
			return true;
		}
		return false;
	}
	
	public static boolean made(int tx, int ty) {
		return self.init(tx, ty) && self.sstate.get() != self.I_UNAVAILABLE;
	}
	
	public static boolean resource(int tx, int ty) {
		return self.init(tx, ty) && self.wres1.get() > 0 && self.sstate.get() != self.I_USED;
	}
	
	private static ROOM_HOSPITAL b() {
		return SETT.ROOMS().HOSPITAL;
	}
	
	private final SETT_JOB job = new SETT_JOB() {
		
		private final int wt = 30;
		private final RBITImp bits = new RBITImp();
		
		@Override
		public boolean jobUseTool() {
			return false;
		}
		
		@Override
		public boolean jobUseHands() {
			return sstate.get() == I_UNAVAILABLE;
		};
		
		@Override
		public void jobStartPerforming() {
			
		}
		
		@Override
		public SoundRace jobSound() {
			return b().employment().sound();
		}
		
		@Override
		public RBIT jobResourceBitToFetch() {
			bits.clear();
			if (sstate.get() == I_UNAVAILABLE)
				return null;
			if (wres1.get() == 0 && ins.fetch[0] && b().resLocks.get(0).passes(FACTIONS.player()))
				bits.or(b().indus.get(0).ins().get(0).resource);
			if (wres2.get() == 0 && ins.fetch[1] && b().resLocks.get(1).passes(FACTIONS.player()))
				bits.or(b().indus.get(0).ins().get(1).resource);
			return bits.isClear() ? null : bits;
		}
		
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return wreserved.get() == 1;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			wreserved.set(ins, 0);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			if (wreserved.get() == 1)
				return false;
			if (sstate.get() == I_UNAVAILABLE)
				return true;
			if (wres1.get() == 0 && ins.fetch[0] && b().resLocks.get(0).passes(FACTIONS.player()))
				return true;
			if (wres2.get() == 0 && ins.fetch[1] && b().resLocks.get(1).passes(FACTIONS.player()))
				return true;
			return false;
			
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			wreserved.set(ins, 1);
		}
		
		@Override
		public double jobPerformTime(Humanoid a) {
			return freeWork.get() == 1 ? 1 : wt;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			jobReserveCancel(r);
			if (r == b().indus.get(0).ins().get(0).resource) {
				b().indus.get(0).ins().get(0).inc(ins, rAm);
				wres1.inc(ins, rAm);
			}else if (r == b().indus.get(0).ins().get(1).resource) {
				b().indus.get(0).ins().get(1).inc(ins, rAm);
				wres2.inc(ins, rAm);
			}else {
				if (sstate.get() == I_UNAVAILABLE) {
					sstate.set(ins, I_AVAILABLE);
				}
			}
			freeWork.set(ins, ins.employees().fetchBonusConsume(wt+1) ? 1 : 0);
			return null;
		}
		
		@Override
		public CharSequence jobName() {
			return b().employment().verb;
		}
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}
		
		
	};
	
	public final FSERVICE service = new FSERVICE() {
		
		@Override
		public int y() {
			return coo.y();
		}
		
		@Override
		public int x() {
			return coo.x();
		}
		
		@Override
		public boolean findableReservedIs() {
			return sstate.get() == I_RESERVED || sstate.get() == I_USED;
		}
		
		@Override
		public boolean findableReservedCanBe() {
			return sstate.get() == I_AVAILABLE;
		}
		
		@Override
		public void findableReserveCancel() {
			sstate.set(ins, I_AVAILABLE);
		}
		
		@Override
		public void findableReserve() {
			if (findableReservedCanBe())
				sstate.set(ins, I_RESERVED);
		}
		
		@Override
		public void startUsing() {
			sstate.set(ins, I_USED);
		};
		
		@Override
		public void consume() {
			wres1.inc(ins, -1);
			wres2.inc(ins, -1);
			sstate.set(ins, I_UNAVAILABLE);
			ins.jobs.searchAgain();
		}
	};
	
}
