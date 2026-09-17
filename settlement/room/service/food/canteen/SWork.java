package settlement.room.service.food.canteen;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.resources.ResG;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.room.infra.stockpile.ROOM_STOCKPILE;
import settlement.room.main.util.RoomBits;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;

class SWork implements SETT_JOB{

	public final static int I = 1;
	
	private CanteenInstance ins;
	private final Coo coo = new Coo();
	private final ROOM_CANTEEN b;
	private final int wt = 60;
	
	private final RoomBits bReserved = new RoomBits(coo, 		new Bits(0b0000_0000_0000_0000_0000_0000_0000_0001));
	private final RoomBits bFreeFetc = new RoomBits(coo, 		new Bits(0b0000_0000_0000_0000_0000_0000_0000_0010));
	private final RoomBits bResource = new RoomBits(coo, 		new Bits(0b0000_0000_0000_0000_0000_1111_1111_0000));
	private final RoomBits bResAmoun = new RoomBits(coo, 		new Bits(0b0000_0000_0000_0000_1111_0000_0000_0000));
	private final RoomBits bCoalAmou = new RoomBits(coo, 		new Bits(0b0000_0000_0000_1111_0000_0000_0000_0000));
	
	SWork(ROOM_CANTEEN b){
		this.b = b;
		if (false) {
			//make a separate storage tile that everyone can fetch to, and have the cooks cook whatever they want.
		}
	}
	
	SWork get(int tx, int ty) {
		if (b.is(tx, ty) && SETT.ROOMS().fData.tileData.get(tx, ty) == I) {
			ins = b.getter.get(tx, ty);
			coo.set(tx, ty);
			return this;
		}
		return null;
	}
	
	public void dispose(int x, int y) {
		if (get(x, y) != null) {
			if (res() != null)
				SETT.THINGS().resources.create(coo, res().resource, bResAmoun.get());
			if (bCoalAmou.get() > 0)
				SETT.THINGS().resources.create(coo, b.industryFuel.ins().get(0).resource, bCoalAmou.get());
		}
	}
	
	public ResG res() {
		if (bResAmoun.get() > 0)
			return RESOURCES.EDI().all().getC(bResource.get());
		return null;
	}
	
	public int resAm() {
		return bResAmoun.get();
	}
	
	public boolean hasCoal() {
		return bCoalAmou.get() > 0;
	}

	@Override
	public void jobReserve(RESOURCE r) {
		bReserved.set(ins, 1);
		if (r != null) {
			ResG g = RESOURCES.EDI().get(r);
			if (g != null) {
				ins.tally(g, 0, jobResourcesNeeded(null));
			}
		}
	}

	@Override
	public boolean jobReservedIs(RESOURCE r) {
		return bReserved.get() == 1;
	}

	@Override
	public void jobReserveCancel(RESOURCE r) {
		bReserved.set(ins, 0);
		if (r != null) {
			ResG g = RESOURCES.EDI().get(r);
			if (g != null) {
				ins.tally(g, 0, -jobResourcesNeeded(null));
			}
		}
	}
	
	@Override
	public boolean jobReserveCanBe() {
		if (bReserved.get() == 1)
			return false;
		if (bCoalAmou.get() == 0)
			return true;
		if (res() == null)
			return !ins.fetchMask().isClear();
		return true;
	}

	@Override
	public RBIT jobResourceBitToFetch() {

		if (bCoalAmou.get() == 0)
			return b.industryFuel.ins().get(0).resource.bit;
		if (res() == null)
			return ins.fetchMask();
		return null;		
	}
	
	@Override
	public int jobResourcesNeeded(Humanoid skill) {
		return ROOM_STOCKPILE.MIN_CARRY;
	}


	@Override
	public double jobPerformTime(Humanoid skill) {
		if (bFreeFetc.get() == 1)
			return 0;
		return wt;
	}

	@Override
	public void jobStartPerforming() {
		// TODO Auto-generated method stub
		
	}
	
	@Override
	public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram) {
		jobReserveCancel(r);
		if (r == b.industryFuel.ins().get(0).resource) {
			ram = SETT.ROOMS().resourceUnderflow.deposit(r, ram);
			bCoalAmou.inc(ins, ram);
			
		}
		
		else if (r != null) {
			ResG g = RESOURCES.EDI().get(r);
			if (g == null)
				return null;
			bResource.set(ins, g.index());
			ram = SETT.ROOMS().resourceUnderflow.deposit(r, ram);
			bResAmoun.inc(ins, ram);
			ins.tally(g, 0, bResAmoun.get());
		}else {
			
			ResG g = res();
			if (g == null)
				return null;
			ins.tally(g, 1, -bResAmoun.get());
			bResAmoun.inc(ins, -1);
			ins.tally(g, 0, bResAmoun.get());
			bFreeFetc.set(ins, 0);
			
			int am = ins.industry().ins().get(0).work(skill, ins, wt);
			am = SETT.ROOMS().resourceUnderflow.withdraw(ins.industry().ins().get(0).resource, am, bCoalAmou.get());
			bCoalAmou.inc(ins, -am);
			
			for (int di = 0; di < DIR.ORTHO.size(); di++) {
				DIR d = DIR.ORTHO.get(di);
				if (b.food.get(coo.x()+d.x(), coo.y()+d.y()) != null) {
					b.food.check();
				}
			}
		}
		
		if (bFreeFetc.get() == 0 && ins.employees().fetchBonus() >= wt) {
			bFreeFetc.set(ins, 1);
			ins.employees().fetchBonusConsume(wt);
		}
		
		return null;
	}

	@Override
	public COORDINATE jobCoo() {
		return coo;
	}

	@Override
	public CharSequence jobName() {
		return b.employment().verb;
	}

	@Override
	public boolean jobUseTool() {
		return false;
	}

	@Override
	public SoundRace jobSound() {
		return b.employment().sound();
	}
	



	
}
