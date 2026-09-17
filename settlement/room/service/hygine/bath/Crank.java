package settlement.room.service.hygine.bath;

import static settlement.main.SETT.ROOMS;
import static settlement.room.service.hygine.bath.Bits.BITS;
import static settlement.room.service.hygine.bath.Bits.RESERVED;
import static settlement.room.service.hygine.bath.Bits.SERVICE;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.util.RoomBits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;

public class Crank implements SETT_JOB{

	final int wt = 20;
	static final int BIT = Bits.CRANK;
	private static final Crank self = new Crank();
	Bath bath;
	final Coo coo = new Coo();
	BathInstance ins;
	private final RoomBits reserved = new RoomBits(coo, new snake2d.util.bit.Bits(RESERVED));
	private final RoomBits working = new RoomBits(coo, new snake2d.util.bit.Bits(RESERVED >> 1));
	private final RoomBits free = new RoomBits(coo, new snake2d.util.bit.Bits(RESERVED >> 2));
	static Crank init(int tx, int ty, ROOM_BATH b) {
		if (!b.is(tx, ty))
			return null;
		
		BathInstance ins = b.getter.get(tx, ty);
		
		int data = ROOMS().data.get(tx, ty);
		if ((data & BITS) != BIT)
			return null;
		for (DIR d : DIR.ORTHO) {
			if (ins.is(tx, ty, d) && (ROOMS().data.get(tx, ty, d) & BITS) == SERVICE) {
				self.bath = b.bath(tx+d.x(), ty+d.y());
				self.coo.set(tx, ty);
				self.ins = b.get(tx, ty);
				return self;
			}
		}
		throw new RuntimeException();
	}
	
	private Crank() {
		
	}

	public static boolean working(int data) {
		return self.working.get(data) == 1;
	}
	
	@Override
	public boolean jobUseTool() {
		return false;
	}
	
	
	@Override
	public void jobStartPerforming() {
		working.set(ins, 1);
	}
	
	@Override
	public SoundRace jobSound() {
		return ins.blueprintI().employment().sound();
	}
	
	@Override
	public RBIT jobResourceBitToFetch() {
		return null;
	}
	
	@Override
	public boolean jobReservedIs(RESOURCE r) {
		return reserved.get() != 0;
	}
	
	@Override
	public void jobReserveCancel(RESOURCE r) {
		working.set(ins, 0);
		reserved.set(ins, 0);
	}
	
	@Override
	public boolean jobReserveCanBe() {
		return bath.availbilityNeeds() && !jobReservedIs(null);
	}
	
	@Override
	public void jobReserve(RESOURCE r) {
		if (jobReservedIs(null))
			throw new RuntimeException();
		reserved.set(ins, 1);
	}
	
	@Override
	public double jobPerformTime(Humanoid skill) {
		return free.get() == 1 ? 1 : wt;
	}
	
	@Override
	public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram) {
		bath.availabilityInc();
		jobReserveCancel(null);
		if (ins.employees().fetchBonusConsume(wt+1)) {
			free.set(ins, 1);
		}else {
			free.set(ins, 0);
		}
		return null;
	}
	
	final String name = "pumping water";
	
	@Override
	public String jobName() {
		return name;
	}
	
	@Override
	public COORDINATE jobCoo() {
		return coo;
	}
	
}
