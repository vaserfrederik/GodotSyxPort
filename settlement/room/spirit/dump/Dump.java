package settlement.room.spirit.dump;

import init.constant.C;
import init.race.RACES;
import init.race.Race;
import settlement.entity.humanoid.spirte.HCorpseRenderer;
import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.thing.ThingsCorpses.Corpse;
import snake2d.Renderer;
import snake2d.util.bit.Bit;
import snake2d.util.bit.Bits;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

class Dump implements FSERVICE{

	private static final Bit is = new Bit		(0x00000001);
	private static final Bit reserved = new Bit	(0x00000002);
	private static final Bits time = new Bits	(0x000000FC);
	private static final Bits race = new Bits	(0x0000FF00);
	private static final Bit active = new Bit	(0x00010000);
	
	private static Dump self = new Dump();
	
	private int tx,ty,data;
	private DumpInstance ins;
	
	
	
	private Dump() {
		
	}
	
	static void init(DumpInstance ins, int tx, int ty) {
		SETT.ROOMS().data.set(ins, tx, ty, is.set(0));
	}
	
	static void activate(int tx, int ty) {
		if (get(tx, ty) != null) {
			self.data = active.set(self.data);
			self.data = reserved.clear(self.data);
			self.save();
		}
	}
	
	static void deactivate(int tx, int ty) {
		if (get(tx, ty) != null) {
			self.data = active.clear(self.data);
			self.data = reserved.clear(self.data);
			self.save();
		}
	}
	
	
	static int daysTillDecompose(int tx, int ty) {
		if (get(tx, ty) != null) {
			return time.get(self.data);
		}
		return 0;
	}
	
	
	static Dump get(int tx, int ty) {
		self.ins = SETT.ROOMS().DUMP.getter.get(tx, ty);
		if (self.ins != null) {
			self.data = SETT.ROOMS().data.get(tx, ty);
			if (is.is(self.data)) {
				self.tx = tx;
				self.ty = ty;
				return self;
			}
			
			
		}
		return null;
	}

	private void save() {
		int now = data;
		data = SETT.ROOMS().data.get(tx, ty);
		ins.service().report(this, ins.blueprintI().service(), -1);
		data = now;
		ins.service().report(this, ins.blueprintI().service(), 1);
		SETT.ROOMS().data.set(ins, tx, ty, data);
	}
	
	@Override
	public boolean findableReservedCanBe() {
		return active.is(data) && time.get(data) == 0 && !reserved.is(data);
	}

	@Override
	public void findableReserve() {
		if (!findableReservedCanBe())
			throw new RuntimeException();
		data = reserved.set(data);
		save();
	}

	@Override
	public boolean findableReservedIs() {
		return active.is(data) && reserved.is(data);
	}

	@Override
	public void findableReserveCancel() {
		data = reserved.clear(data);
		save();
		
	}

	@Override
	public int x() {
		return tx;
	}

	@Override
	public int y() {
		return ty;
	}

	@Override
	public void consume() {
		throw new RuntimeException();
	}
	
	void burry(Corpse corpse) {
		boolean a = active.is(data);
		data = is.set(0);
		data = race.set(data, corpse.indu().race().index);
		data = time.set(data, 16);
		data = reserved.clear(data);
		data = active.set(data, a);
		save();
	}
	
	void update() {
		if (time.get(data) > 0) {
			data = time.inc(data, -1);
			if (time.get(data) == 0)
				data = reserved.clear(data);
			save();
		}
	}
	
	static void render(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		int data = SETT.ROOMS().data.get(i.tile());
		if (is.is(data)) {
			int t = time.get(data);
			if (t > 0) {
				
				double deg = 0.5 + (16-t)/8.0;
				Race rr = RACES.all().get(race.get(data));
				int ran = i.ran();
				int di = ran & 0x07;
				ran = ran >> 3;
				int dx = -4 + (ran&0x07);
				ran = ran >> 4;
				int dy = -4 + (ran&0x07);
				ran = ran >> 4;
				
				if (deg > 1.0)
					HCorpseRenderer.renderSkelleton(rr, true, di, false, r, shadowBatch, ran, i.x()+dx*C.SCALE, i.y()+dy*C.SCALE);
				else
					HCorpseRenderer.renderDump(rr, deg, di, r, shadowBatch, ran, i.x()+dx*C.SCALE, i.y()+dy*C.SCALE);
			}
			
			
		}
	}
	
}
