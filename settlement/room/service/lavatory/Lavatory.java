package settlement.room.service.lavatory;

import static settlement.main.SETT.ROOMS;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;

public class Lavatory implements FSERVICE{

	final static int BIT = 			0b1000000000000000;
	final static int BIT_WASH = 	0b0100000000000000;
	final static Bits USAGE = new Bits(0b1110000);
	final static int S_USED = 1;
	final static int S_BEEING_CLEANED = 2;
	final static int S_RESERVABLE = 0;
	final static int S_RESERVED = 3;
	final static int S_UNUSABLE = 4;
	
	private final static Lavatory self = new Lavatory();
	private final Coo coo = new Coo();
	private LavatoryInstance ins;
	private int data;
	private ROOM_LAVATORY blue;
	
	private Lavatory(){
		
	}
	
	static Lavatory get(int tx, int ty) {
		RoomBlueprint p = SETT.ROOMS().map.blueprint.get(tx, ty);
		if (p instanceof ROOM_LAVATORY) {
			int data = ROOMS().data.get(tx, ty);
			if ((data & BIT) != 0) {
				self.blue = (ROOM_LAVATORY) p;
				self.ins = self.blue.get(tx, ty);
				self.data = data;
				self.coo.set(tx, ty);
				return self;
			}
		}
		return null;
	}

	private void save() {
		int old = ROOMS().data.get(coo);
		if (old == data)
			return;
		int current = data;
		data = old;
		ins.service.report(this, ins.blueprintI().data, -1);
		data = current;
		ins.service.report(this, ins.blueprintI().data, 1);
		ROOMS().data.set(ins, coo, data);
	}
	
	public static boolean isOpen(int data) {
		return USAGE.get(data) < 4;
	}
	
	public void init(RoomServiceInstance ser) {
		ser.report(this, ins.blueprintI().data, 1);
	}
	
	private void stateSet(int state) {
		data &= ~0b01111;
		data |= state;
	}
	
	private int state() {
		return data & 0b01111;
	}
	
	@Override
	public boolean findableReservedIs() {
		return state() == S_RESERVED;
	}
	
	@Override
	public boolean findableReservedCanBe() {
		return state() == S_RESERVABLE;
	}

	@Override
	public void findableReserve() {
		if (!findableReservedCanBe())
			throw new RuntimeException();
		stateSet(S_RESERVED);
		save();
	}

	@Override
	public void findableReserveCancel() {
		if (state() == S_RESERVED) {
			stateSet(S_RESERVABLE);
			save();
		}
	}
	
	@Override
	public void consume() {
		if (!findableReservedIs())
			throw new RuntimeException();
		data = USAGE.inc(data, 1);
		if (USAGE.get(data) >= 4)
			stateSet(S_USED);
		else
			stateSet(S_RESERVABLE);
		save();
	}
	
	public void dispose() {
		stateSet(S_UNUSABLE);
		save();
	}
	
	public void fix() {
		stateSet(S_RESERVABLE);
		save();
	}
	
	@Override
	public int x() {
		return coo.x();
	}

	@Override
	public int y() {
		return coo.y();
	}
	
	public DIR getDir() {
		
		FurnisherItem it = ROOMS().fData.item.get(coo);
		if (it == null)
			return DIR.N;
		
		for (DIR d : DIR.ORTHO) {
			if (!ROOMS().fData.sprite.is(coo, d) &&(d.orthoID() == it.rotation || d.perpendicular().orthoID() == it.rotation) )
				return d;
		}
		return DIR.N;
		
	}

	final SETT_JOB job = new SETT_JOB() {
		
		@Override
		public void jobReserve(RESOURCE r) {
			if (!jobReserveCanBe())
				throw new RuntimeException();
			stateSet(S_BEEING_CLEANED);
			save();
		}

		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return state() == S_BEEING_CLEANED;
		}

		@Override
		public void jobReserveCancel(RESOURCE r) {
			if (state() == S_BEEING_CLEANED) {
				if (USAGE.get(data) < 4)
					stateSet(S_RESERVABLE);
				else
					stateSet(S_USED);
				save();
			}
		}

		@Override
		public RBIT jobResourceBitToFetch() {
			return null;
		}

		@Override
		public double jobPerformTime(Humanoid skill) {
			return 45;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE res, int ram) {
			if (!jobReservedIs(res))
				throw new RuntimeException();
			stateSet(S_RESERVABLE);
			data = USAGE.set(data, 0);
			save();
			return null;
		}

		@Override
		public COORDINATE jobCoo() {
			return coo;
		}

		@Override
		public CharSequence jobName() {
			return blue.employment().verb;
		}

		@Override
		public void jobStartPerforming() {
			// TODO Auto-generated method stub
			
		}

		@Override
		public boolean jobUseTool() {
			return false;
		}

		@Override
		public SoundRace jobSound() {
			return blue.employment().sound();
		}

		@Override
		public boolean jobReserveCanBe() {
			return state() == S_USED || (state() == S_RESERVABLE && USAGE.get(data) > 2);
		}
	};







	
	
	
}
