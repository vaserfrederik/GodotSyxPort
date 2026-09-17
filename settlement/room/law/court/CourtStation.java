package settlement.room.law.court;

import settlement.main.SETT;
import snake2d.util.bit.Bit;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;

public final class CourtStation{

	private final static Bit workReserved = new Bit(0b010000);
	private final static Bits state = new Bits(0b01111);
	private final static int STATE_RESERVABLE = 0;
	private final static int STATE_RESERVED = 1;
	private final static int STATE_USED = 2;
	private final static int STATE_JUDGING = 3;
	
	private final Coo cooCriminal = new Coo();
	private final Coo cooJudge = new Coo();
	private int rot;
	private int data;
	private CourtInstance ins;
	private static final CourtStation self = new CourtStation();

	
	static boolean isJudge(COORDINATE c) {
		int s = SETT.ROOMS().fData.tileData.get(c.x(), c.y());
		return s == Constructor.codeWork;
	}
	
	static CourtStation init(int tx, int ty) {
		CourtInstance ins = SETT.ROOMS().COURT.get(tx, ty);
		if (ins == null)
			return null;
		int c = SETT.ROOMS().fData.tileData.get(tx, ty);
		self.ins = ins;
		if (c == Constructor.codeCriminal) {
			self.cooCriminal.set(tx, ty);
			self.rot = SETT.ROOMS().fData.item.get(tx, ty).rotation;
			DIR d = DIR.ORTHO.get(self.rot);
			self.cooJudge.set(tx+d.x()*Constructor.distance, ty+d.y()*Constructor.distance);
			self.data = SETT.ROOMS().data.get(self.cooJudge);
			return self;
		}
		if (c == Constructor.codeWork) {
			self.cooJudge.set(tx, ty);
			self.rot = SETT.ROOMS().fData.item.get(tx, ty).rotation;
			DIR d = DIR.ORTHO.get(self.rot).perpendicular();
			self.cooCriminal.set(tx+d.x()*Constructor.distance, ty+d.y()*Constructor.distance);
			self.data = SETT.ROOMS().data.get(self.cooJudge);
			return self;
		}
		return null;
	}
	
	public DIR criminalDir() {
		return DIR.ORTHO.get(rot);
	}
	
	public DIR jundgeDir() {
		return DIR.ORTHO.get(rot).perpendicular();
	}
	
	public boolean criminalReseveredCanBe() {
		return state.get(data) == STATE_RESERVABLE;
	}
	
	void criminalReserve() {
		if (!criminalReseveredCanBe())
			throw new RuntimeException();
		data = state.set(data, STATE_RESERVED);
		save();
	}
	
	public boolean criminalReserved() {
		return state.get(data) >= STATE_RESERVED;
	}
	
	public boolean criminalIsUsing() {
		return state.get(data) == STATE_USED;
	}
	
	public void criminalUse() {
		data = state.set(data, STATE_USED);
		save();
	}

	public void criminalClear() {
		data = state.set(data, STATE_RESERVABLE);
		save();
	}
	
	boolean workReservedCanBe() {
		return !workReserved.is(data) && state.get(data) > STATE_RESERVED;
	}
	
	void workReserve() {
		data = workReserved.set(data);
		save();
	}
	
	public void workUse() {
		data = state.set(data, STATE_JUDGING);
		save();
	}
	
	public boolean criminalIsBeeingHeard() {
		return state.get(data) == STATE_JUDGING;
	}

	public void workCancel() {
		data = workReserved.clear(data);
		save();
	}
	
	public boolean workReserved() {
		if (state.get(data) <= STATE_RESERVED)
			workCancel();
		return workReserved.is(data);
	}	
	
	private void save() {
		add(SETT.ROOMS().data.get(cooJudge.x(), cooJudge.y()), -1);
		add(data, 1);
		SETT.ROOMS().data.set(ins, cooJudge, data);
	}
	
	private void add(int data, int delta) {
		if (state.get(data) != STATE_RESERVABLE)
			ins.inc(delta, 0);
		if (!workReserved.is(data) && state.get(data) > STATE_RESERVED)
			ins.inc(0, delta);
	}
	
	public COORDINATE cooJudge() {
		return cooJudge;
	}

	public COORDINATE cooCriminal() {
		return cooCriminal;
	}

	
}
