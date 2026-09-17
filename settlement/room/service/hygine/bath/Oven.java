package settlement.room.service.hygine.bath;

import static settlement.main.SETT.ROOMS;
import static settlement.room.service.hygine.bath.Bits.BITS;
import static settlement.room.service.hygine.bath.Bits.RESERVED;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

public class Oven implements SETT_JOB{

	static final int BIT = Bits.OVEN;
	private static final Oven self = new Oven();
	int data;
	final Coo coo = new Coo();
	BathInstance ins;
	
	static Oven init(int tx, int ty, ROOM_BATH b) {
		if (!b.is(tx, ty))
			return null;
		int data = ROOMS().data.get(tx, ty);
		if ((data & BITS) != BIT)
			return null;
		self.data = data;
		self.coo.set(tx, ty);
		self.ins =b.get(tx, ty);
		return self;
	}
	
	private Oven() {
		
	}

	@Override
	public boolean jobUseTool() {
		return false;
	}
	
	private void save() {
		ROOMS().data.set(ins, coo.x(), coo.y(), data);
	}
	
	@Override
	public void jobStartPerforming() {

	}
	
	@Override
	public SoundRace jobSound() {
		return null;
	}
	
	@Override
	public RBIT jobResourceBitToFetch() {
		if (ins.blueprintI().consumtion.ins().get(0) == null)
			return null;
		return ins.blueprintI().consumtion.ins().get(0).resource.bit;
	}
	
	@Override
	public boolean jobReservedIs(RESOURCE r) {
		return (data & RESERVED) != 0;
	}
	
	@Override
	public void jobReserveCancel(RESOURCE r) {
		data &= ~RESERVED;
		save();
	}
	
	@Override
	public boolean jobReserveCanBe() {
		return ins.heat < ins.service().total()*2 && !jobReservedIs(null);
	}
	
	@Override
	public void jobReserve(RESOURCE r) {
		if (jobReservedIs(null))
			throw new RuntimeException();
		data |= RESERVED;
		save();
	}
	
	@Override
	public int jobResourcesNeeded(Humanoid skill) {
		return SETT.ROOMS().STOCKPILE.carryCap(skill);
	}
	
	@Override
	public double jobPerformTime(Humanoid skill) {
		return 0;
	}
	
	@Override
	public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ri) {
		ins.heat += ri;
		ins.blueprintI().consumtion.ins().get(0).inc(ins, ri);
		jobReserveCancel(r);
		return null;
	}
	
	final String name = "bringing fuel to furnace";
	
	@Override
	public String jobName() {
		return name;
	}
	
	@Override
	public COORDINATE jobCoo() {
		return coo;
	}
	
}
