package settlement.room.service.breeder;

import game.audio.SoundRace;
import game.time.TIME;
import init.resources.RBIT;
import init.resources.RESOURCE;
import init.type.CAUSE_ARRIVES;
import init.type.HTYPE;
import init.type.HTYPES;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.main.util.RoomBits;
import settlement.stats.STATS;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.misc.CLAMP;

class Station {

	final Coo coo = new Coo();
	final Coo masterCoo = new Coo();
	BreederInstance ins;
	final ROOM_BREEDER b;

	private final RoomBits masterRes = new RoomBits(masterCoo, 				new Bits(0x00000FFF));
	private final RoomBits masterActivity = new RoomBits(masterCoo, 		new Bits(0x0001F000));
	private final RoomBits masterActivityCount = new RoomBits(masterCoo, 	new Bits(0x00010000));
	private final RoomBits masterWorkPlaces = new RoomBits(masterCoo, 		new Bits(0x0FF00000));
	
	
	private final RoomBits reserved = new RoomBits(coo, 				new Bits(0x0000000F));
	
	Station(ROOM_BREEDER b){
		this.b = b;
	}
	
	SETT_JOB get(int tx, int ty) {
		if (ini(tx, ty) && SETT.ROOMS().fData.tileData.get(tx, ty) == BreederConstructor.WORK) {
			this.coo.set(tx, ty);
			return job;
		}
		return null;
	}
	
	private boolean ini(int tx, int ty) {
		if (b.is(tx, ty) && SETT.ROOMS().fData.item.is(tx, ty)) {
			SETT.ROOMS().fData.itemX1Y1(tx, ty, masterCoo);
			this.ins = b.get(tx, ty);
			return true;
		}
		return false;
	}
	
	boolean init(int tx, int ty) {
		SETT_JOB j = get(tx, ty);
		if (j != null)
			masterWorkPlaces.inc(ins, 1);
		return j != null;
	}
	
	public void dispose(int x, int y) {
		if (!ini(x, y))
			return;
		int am = masterRes.get();
		masterRes.set(ins, 0);
		if (am > 0) {
			SETT.THINGS().resources.create(x, y, b.indus.get(0).ins().get(0).resource, am);
		}
		
	}
	
	public int resources(int tx, int ty, int ran) {
		if (!ini(tx, ty))
			return 0;
		
		double rr = (double)masterRes.get()/masterWorkPlaces.get();
		rr = (int) rr + ((ran&0x0F)/(double)0x0F)*rr;

		return CLAMP.i((int)rr, 0, 8);
		
	}
	
	public boolean worm(int tx, int ty, int ran) {
		if (!ini(tx, ty))
			return false;
		
		double a = masterActivity.getD()*2.0;
		
		return a > ((ran&0x0F)/(double)0x0F);
		
	}
	
	public double aSpeed(int tx, int ty) {
		if (!ini(tx, ty))
			return 0;
		
		double a = masterActivity.getD()*2.0;
		return CLAMP.d(a, 0, 1);
		
	}
	
	
	public void update(int tx, int ty) {
		if (get(tx, ty) != null)
			masterActivity.inc(ins, -1);
	}
	
	
	private int resources() {
		return masterRes.get()/masterWorkPlaces.get();
	}
	
	final SETT_JOB job = new SETT_JOB() {
		
		private final int wt = 30;
		
		@Override
		public boolean jobUseTool() {
			return false;
		}
		
		@Override
		public void jobStartPerforming() {
			
		}
		
		@Override
		public SoundRace jobSound() {
			return b.employment().sound();
		}
		
		@Override
		public RBIT jobResourceBitToFetch() {
			if (resources() < 1)
				return b.indus.get(0).ins().get(0).resource.bit;
			return null;
		}
		
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return reserved.get() == 1;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			reserved.set(ins, 0);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			if (reserved.get() == 1)
				return false;
			if (!b.canWork())
				return false;
			return true;
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			reserved.set(ins, 1);
		}
		
		@Override
		public double jobPerformTime(Humanoid skill) {
			return wt;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE res, int am) {
			jobReserveCancel(res);
			if (res != null) {
				am = SETT.ROOMS().resourceUnderflow.deposit(res, am);
				if (am > 0) {
					masterRes.inc(ins, am);
				}
				return null;
			}
			
			if (masterActivityCount.get() == 0)
				masterActivity.inc(ins, 1);
			
			int t = ins.employees().fetchBonus(wt);
			
			for (IndustryResource r : ins.industry().ins()) {
				int a = r.work(skill, ins, t);
				if (a > 0) {
					int max = masterRes.get();
					a = SETT.ROOMS().resourceUnderflow.withdraw(r.resource, a, max);
					masterRes.inc(ins, -a);
				}
			}
			
			double w = IndustryUtil.calcProductionRate(t*b.PRODUCTION_SPEED_DAY/TIME.workSeconds(), skill, b.productionData, ins);
			ins.kidsProduction += w;
			
			while(ins.kidsProduction > 1 && b.canWork()) {
				ins.kidsProduction--;
				HTYPE ty = HTYPES.CHILD();
				if (b.prosecute) {
					ty = HTYPES.CHILD_SLAVE();
				}
				Humanoid h = SETT.HUMANOIDS().create(b.race, skill.tc().x(), skill.tc().y(), ty, CAUSE_ARRIVES.BORN());
				if (h != null) {
					STATS.POP().age.DAYS.set(h.indu(), 0);
					STATS.POP().TYPE.NATIVE.set(h.indu());
					STATS.REL().setParent(h.indu(), skill.indu());
				}
			}
			
			return null;
		}
		
		@Override
		public CharSequence jobName() {
			return b.employment().verb;
		}
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}
	};



	
}
