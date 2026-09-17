package settlement.room.main.job;

import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.THINGS;

import game.GAME;
import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.RoomInstance;
import snake2d.SPRITE_RENDERER;
import snake2d.util.bit.Bit;
import snake2d.util.bit.Bits;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.misc.CLAMP;
import util.rendering.ShadowBatch;

public abstract class RoomResDeposit implements SETT_JOB {

	private final static Bits[] AMOUNTS = new Bits[] {
		new Bits(0b0000_0000_0000_0000_0000_0000_0001_1111),
		new Bits(0b0000_0000_0000_0000_0000_0011_1110_0000),
		new Bits(0b0000_0000_0000_0000_0111_1100_0000_0000),
		new Bits(0b0000_0000_0000_1111_1000_0000_0000_0000),
		};

	private final static Bit[] RESERVED = new Bit[] {
		new Bit(0b0000_0000_0001_0000_0000_0000_0000_0000),
		new Bit(0b0000_0000_0010_0000_0000_0000_0000_0000),
		new Bit(0b0000_0000_0100_0000_0000_0000_0000_0000),
		new Bit(0b0000_0000_1000_0000_0000_0000_0000_0000),
	};

	private static Bit WRESERVED = 	new Bit(0b0000_0001_0000_0000_0000_0000_0000_0000);
	private static Bit WUSED = 		new Bit(0b0000_0010_0000_0000_0000_0000_0000_0000);
	private static final int ww = 45;
	protected Coo coo = new Coo();
	protected int data;
	protected final static String name = "Gettings raw materials";
	private ROOM_PRODUCER_INSTANCE ins;
	private RoomInstance insi;
	private final RoomBlueprintImp blue;


	protected RoomResDeposit(RoomBlueprintImp blue) {
		this.blue = blue;
	}

	public RoomResDeposit get(int tx, int ty, RoomInstance i) {
		if (i != null && i.is(tx, ty) && i instanceof ROOM_PRODUCER_INSTANCE && is(tx, ty)) {
			coo.set(tx, ty);
			data = ROOMS().data.get(tx, ty);
			insi = i;
			ins = (ROOM_PRODUCER_INSTANCE) i;
			return this;
		}
		return null;
	}

	protected abstract boolean is(int tx, int ty);
	protected abstract boolean regularJobCanBeReserved(COORDINATE coo);
	protected abstract void regularJobStore(COORDINATE coo, int am);
	
	public int amount(int res) {
		return AMOUNTS[res].get(data);
	}

	public boolean hasOneOfEach() {
		for (int i = 0; i < resAm(); i++) {
			if (amount(i) <= 0)
				return false;
		}
		return true;
	}

//	public void withdrawOneOfEach() {
//		int od = data;
//		for (int i = 0; i < resAm(); i++) {
//			if (amount(i) <= 0)
//				throw new RuntimeException();
//			data = AMOUNTS[i].inc(data, -1);
//		}
//		debug("W", od);
//		save();
//	}
//
//	public void depositOneOfEach() {
//		int od = data;
//		for (int i = 0; i < resAm(); i++) {
//			data = AMOUNTS[i].inc(data, 1);
//		}
//		debug("D", od);
//		save();
//	}

	private void amountSet(int a, int res) {
		data = AMOUNTS[res].set(data, a);
	}

	@Override
	public boolean jobReserveCanBe() {
		if (!WRESERVED.is(data) && regularJobCanBeReserved(coo) && hasOneOfEach())
			return true;
		return jobResourceBitToFetch() != null;
	}

	public void render(SPRITE_RENDERER r, ShadowBatch shadowBatch, int x, int y, int ran) {
		shadowBatch.setHeight(1).setDistance2Ground(0);
		for (int i = 0; i < resAm(); i++) {
			if (amount(i) > 0) {
				res(i).renderLaying(shadowBatch, x, y, ran, amount(i));
				res(i).renderLaying(r, x, y, ran, amount(i));
				ran = ran >> 3;
			}
			
//			if (RESERVED[i].is(data)) {
//				COLOR.ORANGE100.render(r, x, y);
//			}
//			if (WRESERVED.is(data)) {
//				COLOR.RED100.render(r, x, y);
//			}
			
		}
		
//		for (int i = 0; i < 4; i++) {
//			if (RESERVED[i].is(data)) {
//				COLOR.RED100.render(r, x, y);
//			}
//		}
	}
	
	public void renderDebug(SPRITE_RENDERER r, int x, int y) {
				
		if (!WRESERVED.is(data) && regularJobCanBeReserved(coo) && hasOneOfEach()) {
			COLOR.BLUE100.render(r, x, y);
		}else if (WRESERVED.is(data)){
			COLOR.RED100.render(r, x, y);
		}else {
			COLOR.ORANGE100.render(r, x, y);
		}
		
		COLOR.RED100.render(r, x+32, y);
		if (jobResourceBitToFetch() != null) {
			COLOR.BLUE100.render(r, x+32, y);
		}
		
		for (int i = 0; i < resAm(); i++) {
			if (RESERVED[i].is(data)) {
				COLOR.ORANGE100.render(r, x+32, y);
			}
		}
		
		
		if (jobReserveCanBe()) {
			COLOR.GREEN100.render(r, x, y+32);
		}
		
//		for (int i = 0; i < 4; i++) {
//			if (RESERVED[i].is(data)) {
//				COLOR.RED100.render(r, x, y);
//			}
//		}
	}
	
	
	public void renderNeighs(RoomInstance ins, SPRITE_RENDERER r, ShadowBatch shadowBatch, int x, int y, int ran) {
		int tx = coo.x();
		int ty = coo.y();
		shadowBatch.setHeight(1).setDistance2Ground(0);
		for (int i = 0; i < resAm(); i++) {
			int aa = get(tx, ty, ins).amount(i);
			for (int di = 0; di < DIR.ORTHO.size(); di++) {
				DIR d = DIR.ORTHO.get(di);
				if (get(tx+d.x(), ty+d.y(), ins) != null) {
					aa += amount(i);
				}
			}
			
			if (aa > 0) {
				aa = (int) Math.ceil(aa/3.0);
				res(i).renderLaying(shadowBatch, x, y, ran, aa);
				res(i).renderLaying(r, x, y, ran, aa);
				ran = ran >> 3;
			}
		}
	}
	

	public boolean withDraw(int ri, int amount) {
		amountSet(amount(ri) - amount, ri);
		save();
		return amount(ri) > 0;
	}

	private final RBITImp bits = new RBITImp();
	
	@Override
	public RBIT jobResourceBitToFetch() {
		bits.clear();
		for (int i = 0; i < resAm(); i++) {
			if (!RESERVED[i].is(data)) {
				if (amount(i) < 1)
					bits.or(res(i));
			}
		}
		
		return bits.isClear() ? null : bits;
	}
	
	@Override
	public int jobResourcesNeeded(Humanoid skill) {
		return 0b01111;
	}

	@Override
	public double jobPerformTime(Humanoid skill) {
		return ww;
	}

	@Override
	public void jobStartPerforming() {
		data = WUSED.set(data, true);
		save();
	}
	
	public boolean working(int data) {
		return WUSED.is(data);
	}

	@Override
	public SoundRace jobSound() {
		return null;
	}
	
	@Override
	public RESOURCE jobPerform(Humanoid skill, RESOURCE res, int ram) {
		
		
		
		if (res == null) {
			
			data = WRESERVED.set(data, false);
			data = WUSED.set(data, false);
			save();
			
			if (!regularJobCanBeReserved(coo))
				return null;
			
			int ri = 0;
			final double w = insi.employees().fetchBonus(ww);
			
			for (IndustryResource r : ins.industry().ins()) {
				int a = r.work(skill, ins, w);
				if (a > 0) {
					int max = amount(ri);
					a = SETT.ROOMS().resourceUnderflow.withdraw(r.resource, a, max);
					withDraw(ri, a);
				}
				ri++;
			}

			int am = ins.industry().outs().get(0).work(skill, ins, w);
			if (am > 0)
				regularJobStore(coo, am);
			
			return null;
			
		}
		
		
		boolean has = true;
		
		for (int i = 0; i < resAm(); i++) {
			if (res(i) == res) {
				
				data = RESERVED[i].clear(data);
				ram = SETT.ROOMS().resourceUnderflow.deposit(res, ram);
				if (ram > 0) {
					ram = CLAMP.i(ram, 0, jobResourcesNeeded(skill));
					amountSet(amount(i) + ram, i);
				}
			}
			has &= amount(i) > 0;
		}

		save();
		if (has)
			hasCallback();
		return null;
	}

	protected abstract void hasCallback();

	@Override
	public void jobReserve(RESOURCE r) {
		if (r != null) {
			for (int i = 0; i < resAm(); i++) {
				if (res(i) == r) {
					data = RESERVED[i].set(data);
					save();
					return;
				}
			}
		}else if (!WRESERVED.is(data)) {
			data = WRESERVED.set(data);
			save();
			return;
		}
		
		
		throw new RuntimeException("" + r);
	}

	@Override
	public boolean jobReservedIs(RESOURCE r) {
		if (r == null) {
			return WRESERVED.is(data);
		}
		
		for (int i = 0; i < resAm(); i++) {
			if (res(i) == r) {
				return RESERVED[i].is(data);
			}
		}

		return false;
	}

	@Override
	public void jobReserveCancel(RESOURCE r) {
		if (r == null) {
			data = WRESERVED.set(data, false);
			data = WUSED.set(data, false);
			save();
			return;
		}else {
			for (int i = 0; i < resAm(); i++) {
				if (res(i) == r) {
					if (RESERVED[i].is(data)) {
						data = RESERVED[i].clear(data);
						save();
					}
					return;
				}
			}
		}
		
		GAME.Notify("" + r);

	}
	


	void save() {
		ROOMS().data.set((RoomInstance)ins, coo, data);
	}

	@Override
	public COORDINATE jobCoo() {
		return coo;
	}

	@Override
	public CharSequence jobName() {
		return WRESERVED.is(data) ? blue.employment().verb : name;
	}

	@Override
	public boolean jobUseTool() {
		return false;
	}
	
	public final RESOURCE res(int index) {
		return ins.industry().ins().get(index).resource;
	}
	
	public final int resAm() {
		return ins.industry().ins().size();
	}

	public void dispose() {
		for (int i = 0; i < resAm(); i++) {
			if (amount(i) > 0) {
				boolean unload = false;
				for (int di = 0; di < DIR.ALL.size(); di++) {
					int dx = coo.x() + DIR.ALL.get(di).x();
					int dy = coo.y() + DIR.ALL.get(di).y();
					if (SETT.PATH().connectivity.is(dx, dy)) {
						unload = true;
						THINGS().resources.create(dx, dy, res(i), amount(i));
						break;
					}
				}
				if (!unload) {
					THINGS().resources.create(coo, res(i), amount(i));
				}
			}
				
		}
		data = 0;
		save();
		

	}


	
}
