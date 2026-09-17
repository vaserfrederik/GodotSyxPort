package settlement.room.food.farm;

import static settlement.main.SETT.THINGS;

import java.io.Serializable;

import game.audio.SoundRace;
import game.time.TIME;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.main.util.RoomBits;
import snake2d.LOG;
import snake2d.SPRITE_RENDERER;
import snake2d.util.MATH;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.misc.CLAMP;
import snake2d.util.rnd.RND;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;
import util.text.D;
import view.sett.SettDebugClick;

final class Tile {

	private FarmInstance ins;
	private Coo coo = new Coo();
	private final ROOM_FARM b;
	private final Time time;
	
	private final RoomBits bReserved = new RoomBits(coo, 		new Bits(0b0000_0000_0000_0000_0000_0000_0000_0111));
	private final RoomBits bDead = new RoomBits(coo, 			new Bits(0b0000_0000_0000_0000_0000_0000_0000_1000));
	private final RoomBits bRandom = new RoomBits(coo, 			new Bits(0b0000_0000_0000_0000_0000_0111_1111_0000));
	private final RoomBits bHarvested = new RoomBits(coo, 		new Bits(0b0000_0000_0000_0000_0000_1000_0000_0000));
	private final RoomBits bSize = new RoomBits(coo, 			new Bits(0b0000_0000_0000_0000_1111_0000_0000_0000));
	private final RoomBits bWorked = new RoomBits(coo, 			new Bits(0b0000_0000_0011_1111_0000_0000_0000_0000));
	private final RoomBits bHasExtraWork = new RoomBits(coo,	new Bits(0b0100_0000_0000_0000_0000_0000_0000_0000));
	private final double bRandomI = 1.0/bRandom.max();
	private final double bSizeI = 1.0/bRandom.max();	


		
	private final Cycle[] cycles;

	
	Tile(ROOM_FARM b){
		this.b = b;
		this.time = b.time;
		cycles = new Cycle[time.days];

		cycles[MATH.mod(time.dayHarvest, time.days)] = CHarvest;
		cycles[MATH.mod(time.dayHarvest+1, time.days)] = CHarvest;
		
		
		for (int i = 0; i < time.daysPlanting; i++) {
			cycles[MATH.mod(time.dayHarvest-i-1, time.days)] = CPlant;
			
		}

		for (int i = 0; i < time.days; i++) {
			if (cycles[i] == null) {
				cycles[i] = CTill;
			}
		}
		
		new SettDebugClick() {
			
			@Override
			public boolean debug(int px, int py, int tx, int ty) {
				if (get(tx, ty) == null)
					return false;
				Cycle c = cycle();
				LOG.ln(c.name);
				LOG.ln("size " + c.size());
				LOG.ln("growth " + c.growth());
				
				c.debug();
				return true;
			}
		};
		
	}
	
	public Tile get(int tx, int ty) {
		ins = b.get(tx, ty);
		if (ins == null)
			return null;
		coo.set(tx, ty);
		return this;
	}
	
	private Cycle cycle() {
		if (bSize.get() > 0)
			return CDead;
		return cycles[time.dayI()];
	}
	
	public void init(COORDINATE c, FarmInstance ins) {
		bRandom.set(ins, RND.rInt(bRandom.max()));
		bReserved.set(ins, (dayR()-1) & 0b111);
	}
	
	public void updateDay() {
		cycle().update();
		


		
		if (Math.ceil(bReserved.get() - dayR()) > 2) {
			bReserved.set(ins, (dayR()-1) & 0b111);
		}
		
		
//		
//		if (coo.x() < ins.body().cX()) {
//			job().jobReserve(null);
//			job().jobPerform(null, null, 0);
//		}
		
	}

	public boolean destroyTileCan() {
		return cycle().size() > 0;
	}
	
	public void destroyTile() {
		cycle().destroyTile();
	}
	
	private int dayR() {
		return ((int) (time.day())&0b111);
	}
	
	public SETT_JOB job() {
		return cycle();
	}
	
	public void render(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i) {
		Cycle c = cycle();
		double am = c.size();
		if (am > 0) {
			
			double ripe = c.ripeness();
			double growth = c.growth();
			b.crop.sprite.renderTrunk(1+b.constructor.direction(i, ins), r, s, i, growth, ripe, am);
			double res = am*c.fruit();
			ripe = c.ripenessFruit();
			b.crop.sprite.renderTop(1+b.constructor.direction(i, ins), r, s, i, growth, ripe, res);
		}
	}
	
	
	public void renderTill(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i) {
		
		int till = CLAMP.i(bWorked.get(), 0, b.time.daysWorking-b.time.daysPlanting);
		double dt = till/(double)(b.time.daysWorking-b.time.daysPlanting);
		b.constructor.renderTill(r, i, ins, dt);
		
	}
	
	private static CharSequence ¤¤till = "¤Tilling";
	private static CharSequence ¤¤tending = "¤Tending";
	private static CharSequence ¤¤harvesting = "¤Harvesting";
	private static CharSequence ¤¤clearing = "¤Clearing";
	
	static {
		D.ts(Tile.class);
	}
	
	private final Cycle CTill = new Cycle(¤¤till) {
		
		@Override
		boolean is() {
			return true;
		}
		
		@Override
		public double size() {
			return 0;
		}
		
		@Override
		public double ripeness() {
			return 0;
		}
		
		@Override
		public double growth() {
			return 0;
		}
		
		@Override
		public double fruit() {
			return 0;
		}

		@Override
		public double ripenessFruit() {
			return 0;
		}

		@Override
		public void update() {

		}

		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			bWorked.inc(ins, 1);
			
			ins.tData.increase(skill, coo.x(), coo.y());
			if (bHasExtraWork.get() > 0 && is()) {
				bHasExtraWork.inc(ins, -1);
				jobReserveCancel(null);
			}
			return null;
		}

		@Override
		public void destroyTile() {
			int i = bWorked.get();
			int n = i/2;
			bWorked.set(ins, n);
			ins.tData.decrease(i-n);
		}

	
		
		
	};
	

	
	private final Cycle CPlant = new Cycle(¤¤tending) {
		
		@Override
		boolean is() {
			return true;
		}
		
		@Override
		public double size() {
			double d = r();
			d *= bWorked.get()*time.daysWorkingI;
			
			d *= ins.tData.skill();
			d*= ins.blueprintI().constructor.fertility(coo.x(), coo.y());
			d = CLAMP.d(d, 0, 1);
			
			
			return d;
		}
		
		private double r() {
			double d = MATH.distance(time.dayPlant, time.day(), time.days);
			d/= time.daysPlanting;
			d *= 1.25;
			double r = 0.25*bRandom.get()*bRandomI;
			d -= r;
			return CLAMP.d(d, 0, 1);
		}
		
		
		@Override
		public double ripeness() {
			double d = r();
			if (d > 0.75) {
				return CLAMP.d((d - 0.75)*8, 0, 1);
			}
			return 0;
		}
		
		@Override
		public double fruit() {
			double d = r();
			if (d > 0.5) {
				d = (d-0.5)*4;
			}
			d = d * (0.2 + ins.tData.skill()*0.5);
			return CLAMP.d(d, 0, 1);
		}

		@Override
		public double ripenessFruit() {
			double d = r();
			if (d > 0.75) {
				return CLAMP.d((d - 0.75)*4, 0, 1);
			}
			return 0;
		}
		
		@Override
		public double growth() {
			return 1;
		}

		@Override
		public void update() {
			bHarvested.set(ins, 0);
		}
		
		@Override
		public void debug() {

		}

		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			bWorked.inc(ins, 1);
			bHarvested.set(ins, 0);
			ins.tData.increase(skill, coo.x(), coo.y());
			if (bHasExtraWork.get() > 0 && is()) {
				bHasExtraWork.inc(ins, -1);
				jobReserveCancel(null);
			}
			return null;
		}
		
		@Override
		public void destroyTile() {
			int i = bWorked.get();
			int n = i/2;
			bWorked.set(ins, n);
			ins.tData.decrease(i-n);
		}


	};
	
	private final Cycle CHarvest = new Cycle(¤¤harvesting) {
		
		@Override
		boolean is() {
			return size() > 0;
		}
		
		@Override
		public double size() {
			if (bHarvested.get() == 0) {
				double d = bWorked.get()*time.daysWorkingI;
				d *= ins.tData.skill();
				d*= ins.blueprintI().constructor.fertility(coo.x(), coo.y());
				d = CLAMP.d(d, 0, 1);
				
				return d;
				
			}
			return 0.0;
		}
		
		@Override
		public double ripeness() {
			return 1.0;
		}
		
		@Override
		public double fruit() {
			return (0.2 + ins.tData.skill()*0.5);
		}

		@Override
		public double ripenessFruit() {
			return 1.0;
		}
		
		@Override
		public double growth() {
			double d = MATH.distance(time.day(), time.dayDeath, time.days);
			d *= 1.25;
			double r = 0.25*bRandom.get()*bRandomI;
			d -= r;
			return CLAMP.d(d, 0, 1);
		}

		@Override
		public void update() {
			if (bHarvested.get() == 0 && MATH.distance(time.day(), time.dayDeath, time.days) <= 1) {
				bSize.set(ins, (int)Math.ceil(size()*bSize.max()));
			}
		}

		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			bHarvested.set(ins, 1);
			IndustryResource rr = b.industries().get(0).outs().get(0);
			double d = rr.rate;
			
			d *= bWorked.get()*time.daysWorkingI;
			d *= ins.tData.skill();
			d *= time.days;
			d *= ROOM_FARM.WORKERPERTILEI;
			d *= ins.blueprintI().yearMul;
			int am = rr.inc(ins, d);
			if (am > 0) {
				THINGS().resources.create(coo, b.crop.resource, am);
			}
			CDead.jobPerform(skill, r, rAm);
			

			return null;
		}
		
		@Override
		public void destroyTile() {
			bHarvested.set(ins, 1);
			bSize.set(ins, 0);
		}

	
	};
	
	final Cycle CDead = new Cycle(¤¤clearing) {
		
		@Override
		public double size() {
			return bSize.get()*bSizeI;
		}
		
		@Override
		public double ripeness() {
			return 1.0;
		}
		
		@Override
		public double fruit() {
			if (bDead.get() == 1)
				return 0;
			return growth();
		}

		@Override
		public double ripenessFruit() {
			return 1.0;
		}
		
		@Override
		public double growth() {
			if (bDead.get() == 1)
				return 0;
			double d = MATH.distance(time.day(), time.dayDeath, time.days);
			d *= 1.25;
			double r = 0.25*bRandom.get()*bRandomI;
			d -= r;
			if (d >= 1)
				return 0;
			return CLAMP.d(d, 0, 1);
		}

		@Override
		public void update() {
			double now = (TIME.years().bitPartOf()*time.days);
			if (MATH.distance(now, time.dayDeath, time.days) <= 1)
				bDead.set(ins, 1);
			bWorked.set(ins, 0);
		}

		@Override
		boolean is() {
			return true;
		}

		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			bDead.set(ins, 0);
			bSize.set(ins, 0);
			bWorked.set(ins, 0);
			return null;
		}

		@Override
		public void destroyTile() {
			jobPerform(null, null, 0);
		}
		
	};
	
	public static final double WORK_TIME = 4;
	
	private abstract class Cycle implements SETT_JOB {

		private final CharSequence name;
		
		Cycle(CharSequence name){
			this.name = name;
		}
		
		@Override
		public void jobReserve(RESOURCE r) {
			if (jobReservedIs(r))
				throw new RuntimeException();
			
			if (MATH.distance(bReserved.get(), dayR(), 0b111) > 1) {
				bHasExtraWork.set(ins, 1);
			}
			
			bReserved.set(ins, dayR());
		}

		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return bReserved.get() == dayR();
		}

		@Override
		public void jobReserveCancel(RESOURCE r) {
			bReserved.set(ins, ((time.dayI()-1) & 0b111));
		}

		@Override
		public boolean jobReserveCanBe() {
			return is() && !jobReservedIs(null);
		}
		
		@Override
		public RBIT jobResourceBitToFetch() {
			return null;
		}

		@Override
		public double jobPerformTime(Humanoid a) {
			return WORK_TIME;
		}

		@Override
		public void jobStartPerforming() {
			
		}

		abstract boolean is();
		
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}

		@Override
		public CharSequence jobName() {
			return name;
		}

		@Override
		public boolean jobUseTool() {
			return true;
		}

		@Override
		public SoundRace jobSound() {
			return b.employment().sound();
		}
		
		public abstract double size();
		public abstract double fruit();
		public abstract double growth();
		public abstract double ripeness();
		public abstract double ripenessFruit();
		public abstract void update();
		public abstract void destroyTile();
		
		public void debug() {
					
		}

		
	}
	
	final static class IData implements Serializable{
		
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		private float skillPrev = 0;
		private float skill;
		private int skillI;
		

		private short works;
		private int workAcc;
		private byte day = 0;
		
		private final FarmInstance ins;

		
		IData(FarmInstance ins){
			Time t = ins.blueprintI().time;
			day = (byte) t.dayI();
			this.ins = ins;
			
			this.day = (byte) MATH.distance(t.dayDeath, t.day(), t.days);
			
			
		}
		
		void updateDay() {
		
			
			Time t = ins.blueprintI().time;
			this.day = (byte) MATH.distance(t.dayDeath, t.day(), t.days);
			
			if (this.day == 1) {
				workAcc = 0;
				skillPrev = skill/(skillI == 0 ? 1 : skillI);
				skill = 0;
				skillI = 0;
				
			}
			
			
			
			if (this.day >= 1 && this.day <= t.daysWorking) {
				
				
				if (works > 0) {
					workAcc += works;				
				}
			}else {
				this.day = (byte) t.daysWorking;
			}
			works = 0;
			
		}
		
		public double skill() {
			if (skillI == 0)
				return skillPrev;
			return skill/skillI;
		}

		
		public double work() {
			return (double)workAcc / (day*ins.area());
		}
		
		public double workday() {
			if (this.day < 1 || this.day > ins.blueprintI().time.daysWorking)
				return work();
			return (double)works / ins.area();
		}
		
		private void increase(Humanoid skill, int tx, int ty) {
			works ++;
			double s = IndustryUtil.calcProductionRate (1, skill, ins.blueprintI().industries().get(0), ins);

			this.skill += s;
			this.skillI ++;
		}
		
		public void decrease(int am) {
			works -= am;
		}
		
		public CharSequence cName() {
			return ins.blueprintI().tile.cycles[ins.blueprintI().tile.time.dayI()].name;
		}
		
		public boolean shouldStore() {
			 return ins.blueprintI().tile.cycles[ins.blueprintI().tile.time.dayI()] == ins.blueprintI().tile.CHarvest;
		}

	}
	
}
