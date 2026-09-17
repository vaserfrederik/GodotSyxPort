package settlement.room.food.orchard;

import static settlement.main.SETT.ROOMS;

import settlement.entity.animal.ANIMAL_ROOM_RUINER;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.RESOURCE_TILE;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.construction.ConstructionInit;
import settlement.room.main.job.JobIterator;
import settlement.room.main.job.RoomResStorage;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.rnd.RND;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class Instance extends RoomInstance implements JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE, ANIMAL_ROOM_RUINER {

	private static final long serialVersionUID = 1L;
	private long[] produceData;
	private double skill;
	private double skillPrev;
	private int skillI;
	public final float base;
	public float irri;
	private float irriNext;
	private short irriI;
	short trees;
	final short treesTotal;
	
	private final short ssx, ssy;
	private byte sdx = 0;
	private byte sdy = 0;
	
	private final JobIterator jobmanager = new JobIterator(this) {
		private static final long serialVersionUID = 1L;

		@Override
		protected SETT_JOB init(int tx, int ty) {
			OTile t = blueprintI().tile(tx, ty);
			if (t != null)
				return t.job();
			return null;
		}
	};

	Instance(ROOM_ORCHARD p, TmpArea area, RoomInit init) {
		super(p, area, init);
		double t = 0;
		int ssx = -1;
		int ssy = 0;
		for (COORDINATE c : body()) {
			if (is(c)) {
				if (ssx == -1 && p.constructor.storage.get(c.x(), c.y(), this) != null) {
					ssx = c.x();
					ssy = c.y();
					SETT.ROOMS().data.set(this, c, 0);
					p.constructor.storage.get(c.x(), c.y(), this).dispose();
				}
				irri += SETT.GROUND().MOISTURE_TOT.get(c);
				if (p.tile.init(c.x(), c.y(), this))
					t ++;
			}
			
		}
		if (ssx == -1)
			throw new RuntimeException();
		this.ssx = (short) ssx;
		this.ssy = (short) ssy;
		treesTotal = (short) t;
		base = (float) (t/ROOM_ORCHARD.TILES_PER_WORKER);
		int jobs = (int) Math.ceil(base);
		employees().maxSet((int) (jobs*1.25));
		employees().neededSet((int) jobs);
		produceData = p.productionData.makeData();
		activate();
		
		
	}

	@Override
	protected void loadFix() {
		produceData = blueprintI().productionData.makeDataFix(produceData);
	}
	
	
	@Override
	protected void updateAction(double updateInterval, boolean day) {

		blueprintI().productionData.updateRoom(this);
		
		if (day) {
			jobmanager.searchAgain();
			
			if (blueprintI().time.isDeadDay()) {
				skillPrev = skill();
				skill = 0;
				skillI = 0;
			}
			
		}
		
	}

	public void deposit(int am) {
		if (am == 0)
			return;
		for (int i = 0; i < 4; i++) {
			RoomResStorage s = blueprintI().constructor.storage.get(ssx+sdx, ssy+sdy, this);
			if (s == null) {
				System.out.println(ssx + " " + sdx + " " + ssy + " " + sdy);
				return;
			}
			sdx++;
			if (sdx >= 2) {
				sdx = 0;
				sdy++;
				if (sdy >= 2)
					sdy = 0;
			}
			am -= s.deposit(am);
			if (am <= 0)
				return;
			
		}
		
		SETT.THINGS().resources.create(ssx,ssy, industry().outs().get(0).resource, am);
		
	}
	
	@Override
	protected boolean canRemoveAndRemoveAction(int tx, int ty, boolean scatter, Object obj, boolean forced) {
		if (scatter) {
			for (COORDINATE c : body()) {
				if (is(c)) {
					OTile t = blueprintI().tile.getM(c.x(), c.y());
					if (t != null)
						t.chop();
				}
				
			}
		}
		return true;
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		int d = SETT.ROOMS().fData.spriteData2.get(it.tile());
		if (d != 0) {
			blueprintI().constructor.sEdge.render(r, shadowBatch, d, it, 0, false);
		}
		
		return super.render(r, shadowBatch, it);
	}
	
	@Override
	public boolean canBeGraced(int tx, int ty) {
		OTile t = blueprintI().tile(tx, ty);
		return t != null && t.destroyTileCan();
	}

	@Override
	public void grace(int tx, int ty) {
		blueprintI().tile(tx, ty).destroyTile();
	}

	@Override
	protected void activateAction() {
		// TODO Auto-generated method stub

	}

	@Override
	protected void deactivateAction() {
		// TODO Auto-generated method stub

	}

	@Override
	protected void dispose() {
		for (COORDINATE c : body()) {
			if (blueprintI().constructor.storage.get(c.x(), c.y(), this) != null)
				blueprintI().constructor.storage.get(c.x(), c.y(), this).dispose();
		}

	}

	@Override
	public JOB_MANAGER getWork() {
		return jobmanager;
	}

	@Override
	public ROOM_ORCHARD blueprintI() {
		return (ROOM_ORCHARD) blueprint();
	}

	@Override
	public boolean acceptsWork() {
		return true;
	}

	@Override
	public void destroyTile(int tx, int ty) {
		if (destroyTileCan(tx, ty)) {
			blueprintI().tile(tx, ty).destroyTile();
		}
	}

	@Override
	public boolean destroyTileCan(int tx, int ty) {
		OTile t = blueprintI().tile(tx, ty);
		return t != null && t.destroyTileCan();
	}

	@Override
	public ROOM_DEGRADER degrader(int tx, int ty) {
		return null;
	}

	@Override
	public long[] productionData() {
		return produceData;
	}


	@Override
	public Industry industry() {
		return blueprintI().industries().get(0);
	}


	@Override
	public int industryI() {
		// TODO Auto-generated method stub
		return 0;
	}
	
	public void incSkill(double skill) {
		this.skill += skill;
		this.skillI ++;
	}
	
	public void changeTo(ROOM_ORCHARD f) {
		ConstructionInit init = new ConstructionInit(0, f.constructor, null, 0, makeState(mX(), mY(), false));
		TmpArea a = remove(mX(), mY(), false, this, true);
		
		ROOMS().construction.createClean(a, init);
		
	}
	
	@Override
	public void updateTileDay(int tx, int ty) {
		
		OTile t = blueprintI().tile(tx, ty);
		if (t != null)
			t.updateDay();
		
		if (irriI >= area()) {
			irri = irriNext;
			irriNext = 0;
			irriI = 0;
		}
		irriI ++;
		irriNext += SETT.GROUND().MOISTURE_TOT.get(tx, ty);
		
		
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return blueprintI().constructor.storage.get(tx, ty, this);
	}
	

	public double skill() {
		if (skillI == 0)
			return skillPrev;
		return skill/skillI;
	}


	public boolean event() {
		boolean ff = false;
		for (COORDINATE c : body()) {
			if (is(c) && RND.rBoolean()) {
				OTile t = blueprintI().tile.getM(c.x(), c.y());
				if (t != null)
					ff |= t.kill();
			}
			
		}
		return ff;
	}
	
	@Override
	public double productionRate(RoomInstance ins, Humanoid h, Industry in, IndustryResource oo) {
		return ROOM_PRODUCER_INSTANCE.super.productionRate(ins, h, in, oo)*trees/treesTotal;
	}
	
}
