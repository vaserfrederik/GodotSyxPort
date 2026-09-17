package settlement.room.industry.module.consumption;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.industry.module.ROOM_IDATA_INSTANCE;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.util.RoomBits;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import util.data.BOOLEANCoo;

public abstract class ConsumptionJob implements SETT_JOB{

	protected final Coo coo = new Coo();
//	private final  RoomBits[] AMOUNTS = new RoomBits[] {
//			new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0000_0000_0111_1111)),
//			new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0111_1111_1000_0000)),
//			new RoomBits(coo, new Bits(0b0000_0000_0011_1111_1000_0000_0000_0000)),
//			new RoomBits(coo, new Bits(0b0001_1111_1100_1100_0000_0000_0000_0000)),
//			};
//	private final RoomBits allRes = new RoomBits(coo, new Bits(0b0001_1111_1111_1111_1111_1111_1111_1111));
	private final RoomBits reserved = new RoomBits(coo, new Bits(0b0110_0000_0000_0000_0000_0000_0000_0000));
	private final RoomBits used = new RoomBits(coo, new Bits(0b0010_0000_0000_0000_0000_0000_0000_0000));
	
	protected RoomInstance ins;
	protected ROOM_IDATA_INSTANCE insc; 
	protected final int time;
	protected final RoomBlueprintIns<?> blue;
	protected final RoomConsumption bluec;
	private final BOOLEANCoo is;
	
	
	public ConsumptionJob(RoomBlueprintIns<?> b, RoomConsumption cons, int time, BOOLEANCoo is){
		this.blue = b;
		this.bluec = cons;
		this.time = time;
		this.is = is;
	}
	
	public SETT_JOB get(int tx, int ty) {
		ins = blue.get(tx, ty);
		if (ins == null)
			return null;
		if (is.is(tx, ty)) {
			coo.set(tx, ty);
			insc = (ROOM_IDATA_INSTANCE) ins;
			return this;
			
		}
		return null;
		
	}
	



	
	public boolean used(int tx, int ty) {
		return used.get(SETT.ROOMS().data.get(tx, ty)) == 1;
	}
	
//	public int resAmount(int tx, int ty, IndustryResource res) {
//		return AMOUNTS[res.index()].get(SETT.ROOMS().data.get(tx, ty));
//	}
	
	@Override
	public void jobReserve(RESOURCE r) {
		if (reserved.get() == 1) {
			throw new RuntimeException();
		}
		reserved.set(ins, 1);
		
		if (r != null) {
			IndustryResource rr = bluec.in(r);
			if (rr == null)
				throw new RuntimeException();
			reserved.set(ins, 1);
			bluec.reseved(rr).inc(insc,1);
		}
		
	}

	@Override
	public boolean jobReservedIs(RESOURCE r) {
		return reserved.get() == 1;
	}

	@Override
	public void jobReserveCancel(RESOURCE r) {
		reserved.set(ins, 0);
		used.set(ins, 0);
		
		if (r == null)
			return;
		IndustryResource rr = bluec.in(r);
		if (rr == null)
			return;
		bluec.reseved(rr).inc(insc,-1);
	}

	@Override
	public boolean jobReserveCanBe() {
		if (jobReservedIs(null))
			return false;

		
		return true;
	}

	
	private final RBITImp resBit = new RBITImp();
	@Override
	public RBIT jobResourceBitToFetch() {
		resBit.clear();
		for (IndustryResource in : bluec.ins()) {
			if (bluec.shouldFecth(in, insc, ins)) {
				resBit.or(in.resource.bit);
			}
				
		}
		return resBit.isClear() ? null : resBit;
	}

	@Override
	public double jobPerformTime(Humanoid skill) {
		return time;
	}

	@Override
	public void jobStartPerforming() {
		used.set(ins, 1);
	}
	
	@Override
	public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram) {
		
		jobReserveCancel(r);
		
		if (r != null) {
			IndustryResource rr = bluec.in(r);
			if (rr == null)
				return null;
			if (bluec.enabled(rr, insc)) {
				ram = SETT.ROOMS().resourceUnderflow.deposit(r, ram);
				bluec.stored(rr).inc(insc, ram);
			}else
				SETT.THINGS().resources.create(skill.tc(), r, ram);
			return null;
		}
		
		
		int t = ins.employees().fetchBonus(time);
		double d = IndustryUtil.calcProductionRate(1, skill, bluec, ins);
		
		for (IndustryResource in : bluec.ins()) {
			
			if (bluec.stored(in).get(insc) > 0) {
				int a = in.work(skill, insc, t);
				if (a > 0) {
					a = SETT.ROOMS().resourceUnderflow.withdraw(in.resource, a, bluec.stored(in).get(insc));
					bluec.stored(in).inc(insc, -a);	
				}
			}
			
		}

		perform(t, d);
		return null;
	}
	
	protected abstract void perform(double time, double skill);

	@Override
	public COORDINATE jobCoo() {
		return coo;
	}

	@Override
	public CharSequence jobName() {
		return blue.employment().verb;
	}
	
	@Override
	public SoundRace jobSound() {
		return blue.employment().sound();
	}
	

	
	
	
}
