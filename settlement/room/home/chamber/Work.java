package settlement.room.home.chamber;

import static settlement.main.SETT.PATH;
import static settlement.main.SETT.ROOMS;

import game.audio.SoundRace;
import init.race.RACES;
import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.misc.job.SETT_JOB;
import settlement.path.AVAILABILITY;
import settlement.stats.Induvidual;
import settlement.stats.STATS;
import settlement.stats.colls.StatsHome.StatFurniture;
import settlement.stats.equip.WearableResource;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.misc.CLAMP;

final class Work implements SETT_JOB{
	
	private static final int NEEDS = 0;
	private static final int RESERVED = 1;
	private int data;
	private final Coo coo = new Coo();
	private ChamberInstance ins;
	private final ROOM_CHAMBER blue;
	
	Work(ROOM_CHAMBER blue) {
		this.blue = blue;
	}
	
	Work get(int tx, int ty) {
		if (blue.is(tx, ty)) {
			int data = ROOMS().data.get(tx, ty);
			if (PATH().availability.get(tx, ty)== AVAILABILITY.ROOM) {
				this.data = data;
				this.coo.set(tx, ty);
				this.ins = blue.get(tx, ty);
				
				return this;
			}
		}
		return null;
	}
	
	void clear() {
		ROOMS().data.set(ins, coo, 0);
	}
	
	private void save() {
		
		int old = ROOMS().data.get(coo);
		
		if (old != data) {
			ROOMS().data.set(ins, coo, data);
		}
	}
	
	private int state() {
		return data & 0b01111;
	}
	
	private void stateSet(int state) {
		data &= ~0b01111;
		data |= state;
		save();
	}
	
	@Override
	public boolean jobUseTool() {
		return false;
	}
	
	@Override
	public void jobStartPerforming() {
		
	}
	
	@Override
	public SoundRace jobSound() {
		return ins.blueprintI().employment().sound();
	}
	
	int i = 0;
	
	private final RBITImp bit = new RBITImp();
	
	@Override
	public RBIT jobResourceBitToFetch() {
		i++;
		if ((i & 1) == 0 && !ins.fetching && ins.occupant() != null) {
			bit.clear();
			Induvidual in = ins.occupant().indu();
			for (StatFurniture f : STATS.HOME().getTmp(in)) {
				if (f.needed(ins.occupant().indu()) > 0 && ins.jobs.resourceReachable(f.resource(in)))
					bit.or(f.resource(in));
			}
			
			return bit.isClear() ? null : bit;
		}
		return null;
	}
	
	
	@Override
	public boolean jobReservedIs(RESOURCE r) {
		return state() == RESERVED;
	}
	
	@Override
	public void jobReserveCancel(RESOURCE r) {
		if (jobReservedIs(r)) {
			if (r != null)
				ins.fetching = false;
			stateSet(NEEDS);
		}
	}
	
	@Override
	public boolean jobReserveCanBe() {
		return state() == NEEDS || state() == 2;
	}
	
	@Override
	public void jobReserve(RESOURCE r) {
		if (!jobReserveCanBe())
			throw new RuntimeException();
		stateSet(RESERVED);
		if (r != null)
			ins.fetching = true;
	}
	
	@Override
	public double jobPerformTime(Humanoid skill) {
		return 30;
	}
	
	@Override
	public RESOURCE jobPerform(Humanoid skill, RESOURCE res, int ram) {
		if (!jobReservedIs(res))
			throw new RuntimeException();
		stateSet(NEEDS);
		
		if (res != null) {
			if (ins.occupant() != null) {
				Induvidual in = ins.occupant().indu();
				for (WearableResource rr : RACES.res().get(ins.occupant().indu().popCL(), res)) {
					int nn = rr.needed(in);
					int aa = CLAMP.i(ram, 0, nn);
					rr.inc(in, aa);
					ram -= aa;
					if (ram <= 0)
						break;
				}
				
			}
			ins.fetching = false;
		}
		
		return null;
	}
	
	@Override
	public CharSequence jobName() {
		return ins.blueprintI().employment().verb;
	}
	
	@Override
	public COORDINATE jobCoo() {
		return coo;
	}


	
}
