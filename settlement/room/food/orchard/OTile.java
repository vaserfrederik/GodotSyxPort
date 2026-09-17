package settlement.room.food.orchard;

import game.audio.SoundRace;
import game.faction.FACTIONS;
import game.faction.FResources.RTYPE;
import init.constant.C;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.main.ROOMA;
import settlement.room.main.util.RoomBits;
import snake2d.SPRITE_RENDERER;
import snake2d.util.bit.Bits;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.rnd.RND;
import util.rendering.RenderData.RenderIterator;
import view.sett.SettDebugClick;

final class OTile {

	private Instance ins;
	private Coo coo = new Coo();
	private Coo mCoo = new Coo();
	private final ROOM_ORCHARD b;
	
	public final static int WORK_TIME = 45;
	public final static int INOTHING = 0;
	public final STATE ISAPLING;
	public final STATE ISMALL;
	public final STATE IBIG;
	public final STATE IDEAD;
	private final STATE[] states;
	private final RoomBits bState = 		new RoomBits(coo,	new Bits(0b0000_0000_0000_0000_0000_0000_0000_0111)) {
		@Override
		public void set(int tx, int ty, ROOMA r, int t) {
			
			if (get() == IBIG.index) {
				ins.trees--;
			}
			super.set(tx, ty, r, t);
			if (t == IBIG.index) {
				ins.trees++;
			}
		}
		
	};
	private final RoomBits bHarvested =	 	new RoomBits(coo,	new Bits(0b0000_0000_0000_0000_0000_0000_0000_1000));
	private final RoomBits bDead = 			new RoomBits(mCoo, 	new Bits(0b0000_0000_0000_0000_0000_0000_0011_0000));
	private final RoomBits bProgress = 		new RoomBits(mCoo,	new Bits(0b0000_0000_0000_0000_1111_1111_1100_0000));
	
	private final RoomBits stateReset = 	new RoomBits(coo,	new Bits(0b0000_0000_0000_0000_1111_1111_1111_1111));
	
	private final RoomBits bWorkedDay =	 	new RoomBits(coo,	new Bits(0b0000_0000_0000_0111_0000_0000_0000_0000));
	private final RoomBits bReserved = 		new RoomBits(coo,	new Bits(0b0000_0000_0000_1000_0000_0000_0000_0000));
	
	
	private final RoomBits bdir = 			new RoomBits(coo,	new Bits(0b0000_0000_1111_0000_0000_0000_0000_0000));
	private final RoomBits bRan = 			new RoomBits(coo,	new Bits(0b0000_1111_0000_0000_0000_0000_0000_0000));
//
//	public static final Bits IRRI = 							new Bits(0b1111_0000_0000_0000_0000_0000_0000_0000);
	
	private final DIR[] dirs = new DIR[] {
		DIR.C,DIR.N,DIR.W,DIR.NW
	};
	
	OTile(ROOM_ORCHARD b){
		this.b = b;
		
		new SettDebugClick() {
			
			@Override
			public boolean debug(int px, int py, int tx, int ty) {
				if (get(tx, ty) == null)
					return false;
				
				if (get(tx, ty) != null)
					setState(IDEAD);
				
//				LOG.ln(c.name);
//				LOG.ln("size " + c.size());
//				LOG.ln("growth " + c.growth());
//				
//				c.debug();
				return true;
			}
		};
		final int sdays = 4*(int) (b.time.DAYS_TILL_GROWTH*0.4);
		final int smalldays = 4*b.time.DAYS_TILL_GROWTH-sdays;
		
		ISAPLING = new STATE(1) {
			
			private final int failA = (int) Math.ceil(sdays/16.0);
			
			@Override
			public void fail() {
				bProgress.inc(ins, -failA);
			}
			
			@Override
			public void work(Humanoid a, int skill) {
				if (bProgress.get() >= sdays) {
					setState(ISMALL);
				}else {
					bProgress.inc(ins, skill);
				}
				bDead.inc(ins, -1);
			}
			
			@Override
			public int daysTillGrown() {
				return Math.max(0, ((b.time.DAYS_TILL_GROWTH - bProgress.get()/4)));
			}
			
		};
		ISMALL = new STATE(2) {
			
			@Override
			public void work(Humanoid a, int skill) {
				if (bProgress.get() >= smalldays) {
					setState(IBIG);
				}else
					bProgress.inc(ins, skill);
				bDead.inc(ins, -1);
			}
			
			@Override
			public void fail() {
				if (bDead.get() == bDead.max()) {
					setState(ISAPLING);
				}else
					bDead.inc(ins, 1);
			}
			
			@Override
			public int daysTillGrown() {
				return Math.max(0, ((smalldays - bProgress.get())/4));
			}
			
		};
		IBIG = new STATE(3) {
			
			@Override
			public void work(Humanoid a, int skill) {
				
				if (b.time.isRipe() && bHarvested.get() == 0) {
					
					
					
					double sk = fruitAmount()*ins.skill()*b.AmountPerTile*b.productionData.outs().get(0).rate;
					int am = b.productionData.outs().get(0).inc(ins, sk);
					if (am != 0) {
						ins.deposit(am);
						
					}
					bHarvested.set(ins, 1);
				}
			}
			
			@Override
			public void fail() {
				if (bDead.get() == bDead.max()) {
					setState(IDEAD);
				}else
					bDead.inc(ins, 1);
			}
			
			@Override
			public void update() {
				if (b.time.isDeadDay()) {
					bHarvested.set(ins, 0);
				}
				super.update();
			}
			
			@Override
			public double deadAmount() {
				return bDead.getD();
			}
			
			@Override
			public double fruitAmount() {
				if (bHarvested.get() == 1)
					return 0;
				return 1;
			}
			
		};
		IDEAD = new STATE(4) {
			
			@Override
			public void work(Humanoid a, int skill) {
				setState(ISAPLING);
				SETT.THINGS().resources.create(a.tc(), b.auxRes.resource(), b.auxRes.amount());
				FACTIONS.player().res().inc(b.auxRes.resource(), RTYPE.PRODUCED, b.auxRes.amount());
			}
			
			@Override
			public double deadAmount() {
				return 1.0;
			};
			
		};
		
		states = new STATE[] {
			null,
			ISAPLING,
			ISMALL,
			IBIG,
			IDEAD
		};
		
	}
	
	public void chop() {
		
		if (state() == ISMALL) {
			setState(ISAPLING);
			SETT.THINGS().resources.create(coo, b.auxRes.resource(), b.auxRes.amount()/2);
			FACTIONS.player().res().inc(b.auxRes.resource(), RTYPE.PRODUCED, b.auxRes.amount()/2);
		}else if (state() == IBIG || state() == IDEAD) {
			setState(ISAPLING);
			SETT.THINGS().resources.create(coo, b.auxRes.resource(), b.auxRes.amount());
			FACTIONS.player().res().inc(b.auxRes.resource(), RTYPE.PRODUCED, b.auxRes.amount());
		}
		
	}
	
	public boolean kill() {
		
		if (state() == IBIG) {
			setState(IDEAD);
			return true;
		}
		return false;
	}
	
	public boolean init(int tx, int ty, Instance ins) {
		if (SETT.ROOMS().fData.tileData.get(tx, ty) != Constructor.TREE)
			return false;
		if (isMaster(tx, ty)) {
			coo.set(tx, ty);
			mCoo.set(tx, ty);
			bdir.set(ins, 0);
			bState.set(ins, ISAPLING.index);
			bRan.set(ins, RND.rInt(bRan.max()));
			bWorkedDay.set(ins, (b.time.dayI()-1)&bWorkedDay.max());
			return true;
		}
		

		for (int di = 1; di < dirs.length; di++) {
			DIR d = dirs[di];
			if (SETT.ROOMS().fData.tileData.get(tx, ty, d) == Constructor.TREE && isMaster(tx+d.x(), ty+d.y())) {
				coo.set(tx, ty);
				mCoo.set(tx+d.x(), ty+d.y());
				bdir.set(ins, di);
				bState.set(ins, ISAPLING.index);
				bRan.set(ins, RND.rInt(bRan.max()));
				bWorkedDay.set(ins, (b.time.dayI()-1)&bWorkedDay.max());
				return true;
			}
			
		}
		throw new RuntimeException();
	}
	
	private boolean isMaster(int tx, int ty){
		for (DIR d : dirs) {
			if (d != DIR.C && SETT.ROOMS().fData.tileData.get(tx, ty, d) == Constructor.TREE)
				return false;
		}
		return true;
	}
	
	public STATE state() {
		return states[bState.get()];
	}
	
	public OTile get(int tx, int ty) {
		ins = b.get(tx, ty);
		if (ins == null)
			return null;
		if (SETT.ROOMS().fData.tileData.get(tx, ty) != Constructor.TREE)
			return null;
		coo.set(tx, ty);
		if (bState.get() == INOTHING)
			return null;
		DIR d = dirs[bdir.get()];
		mCoo.set(tx+d.x(), ty+d.y());
		return this;
	}
	
	public OTile getM(int tx, int ty) {
		if (get(tx, ty) != null && bdir.get() == 0)
			return this;
		return null;
	}
	
	public boolean destroyTileCan() {
		return bState.get() > ISAPLING.index;
	}
	
	public void destroyTile() {
		setState(ISAPLING);
	}

	
	private void setState(STATE state) {
		int ox = coo.x();
		int oy = coo.y();

		for (int y = 0; y < 2; y++) {
			for (int x = 0; x < 2; x++) {
				int tx = mCoo.x() + x;
				int ty = mCoo.y() + y;
				if (get(tx, ty) == null) {

				}else {
					
					get(tx, ty);
					stateReset.set(ins, 0);
					bState.set(ins, state.index);
				}
			}
		}
		get(ox, oy);
		return;
	}
	
	public void renderDebug(SPRITE_RENDERER r, RenderIterator it) {
		it.setOff(0, 0);
		if (bReserved.get() == 0) {
			COLOR.BLUE100.render(r, it.x(), it.y());
		}else {
			COLOR.GREEN100.render(r, it.x(), it.y());
		}
		
		if (bWorkedDay.get() == ((b.time.dayI())&bWorkedDay.max())) {
			COLOR.YELLOW100.render(r, it.x()+C.TILE_SIZEH, it.y());
		}
	}
	
	public void updateDay() {
		if (Bits.getDistance(bWorkedDay.get(), b.time.dayI(), bWorkedDay.max()) > 2) {
			state().fail();
			bWorkedDay.set(ins, (b.time.dayI()-1)&bWorkedDay.max());
			bReserved.set(ins, 0);
		}
		state().update();
	}
	
	public SETT_JOB job() {
		return job;
	}
	
	private final SETT_JOB job = new SETT_JOB() {
		
		@Override
		public boolean jobUseTool() {
			return state() == IDEAD;
		}
		
		@Override
		public void jobStartPerforming() {
			// TODO Auto-generated method stub
			
		}
		
		@Override
		public SoundRace jobSound() {
			return state() == IDEAD ? SETT.TERRAIN().TREES.SMALL.clearing().sound(coo.x(), coo.y()) : ins.blueprintI().employment().sound();
		}
		
		@Override
		public RBIT jobResourceBitToFetch() {
			return null;
		}
		
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return bReserved.get() == 1;
		}
		
		@Override
		public void jobReserveCancel(RESOURCE r) {
			bReserved.set(ins, 0);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return bReserved.get() == 0 && bWorkedDay.get() != ((b.time.dayI())&bWorkedDay.max());
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			bReserved.set(ins, 1);
		}
		
		@Override
		public double jobPerformTime(Humanoid a) {
			return WORK_TIME;
		}
		
		@Override
		public CharSequence jobName() {
			return b.employment().verb;
		}
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			bWorkedDay.set(ins, (b.time.dayI())&bWorkedDay.max());
			bReserved.set(ins, 0);
			double s = IndustryUtil.calcProductionRate(1, skill, ins.industry(), ins);
			ins.incSkill(s);
			int am = (int) s;
			if (s-am > RND.rFloat())
				am++;
			state().work(skill, am);
			return null;
		}
	};

	
	public static class STATE {
		
		public final int index;
		
		STATE(int i){
			this.index = i;
		}
		
		public void work(Humanoid a, int skill) {
			
		}
		
		public void fail() {
			
		}
		
		public double deadAmount() {
			return 0;
		}
		
		public double fruitAmount() {
			return 0;
		}
		
		public int daysTillGrown() {
			return Integer.MAX_VALUE;
		}
		
		public void update() {
			
		}
		
	}
	
}
