package settlement.room.service.hygine.bath;

import static settlement.main.SETT.ROOMS;

import game.time.TIME;
import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.ArrayCooShort;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

public final class BathInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_SERVICER, ROOM_PRODUCER_INSTANCE{


	private static final long serialVersionUID = 1L;

	final Jobs jobs;

	
	double heat = 0;
	
	final RoomServiceInstance service;
	
	private final ArrayCooShort benches;
	private int benchI;
	private long[] pData;
	boolean auto = true;
	
	float water;
	private short waterTiles = 0;
	private short waterCount = 0;
	
	protected BathInstance(ROOM_BATH blue, TmpArea area, RoomInit init) {
		super(blue, area, init);
		int s = 0;
		int b = 0;
		
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			if (ROOMS().fData.tile.is(c)) {
				int d = ROOMS().fData.tile.get(c).data();
				ROOMS().data.set(this, c, d);
				if ((d & Bits.BITS) == Bits.BENCH)
					b++;
			}
			if (SETT.ENV().map.WATER_SWEET.get(c) > 0)
				water ++;
			
			
		}
		
		water *= 1.5;
		water /= area();
		water = (float) CLAMP.d(water, 0, 1);
		
		jobs = new Jobs(this);
		
		
		benches = new ArrayCooShort(b);
		
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			int d = ROOMS().data.get(c);
			if ((d & Bits.BITS) == Bits.SERVICE)
				s += Bath.initService(c.x(), c.y(), this);
			if ((d & Bits.BITS) == Bits.BENCH)
				benches.set(--b).set(c);
		}
		
		service = new RoomServiceInstance(s, blueprintI().data);
		
		
		employees().maxSet(jobs.size());
		employees().neededSet((int)Math.ceil(jobs.size()/1.5));
		pData = blue.consumtion.makeData();
		activate();
	}
	
	@Override
	protected void loadFix() {
		pData = industry().makeDataFix(pData);
	}

	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		if (day)
			service.updateDay();
		blueprintI().consumtion.updateRoom(this);
		heat -= updateInterval*coalPerDay()*TIME.secondsPerDayI();
		if (heat < 0)
			heat = 0;
		
		jobs.searchAgain();
	}
	
	@Override
	public void updateTileDay(int tx, int ty) {
		waterCount++;
		waterTiles += SETT.ENV().map.WATER_SWEET.get(tx, ty) > 0 ? 1 : 0;
		
		if (waterCount >= area()) {
			water = (float) CLAMP.d((1.5*waterTiles)/area(), 0, 1);
			waterCount = 0;
			waterTiles = 0;
		}
		
		super.updateTileDay(tx, ty);
	}

	private double coalPerDay() {
		return service.total()*blueprintI().industries().get(0).ins().get(0).rate;
	}
	
	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}
	
	public COORDINATE getBench() {
		if (benchI == benches.size())
			return null;
		return benches.set(benchI++);
	}
	
	public void returnBench(int tx, int ty) {
		if (!is(tx, ty))
			return;
		int data = ROOMS().data.get(tx, ty);
		if ((data & Bits.BITS) != Bits.BENCH)
			return;
		if (benchI == 0)
			return;
		benchI--;
		benches.set(benchI).set(tx,ty);
		
	}

	@Override
	protected void dispose() {
		for (COORDINATE c : body()) {
			Bath b = blueprintI().bath(c.x(), c.y());
			if (b != null)
				b.dispose();
		}
		service.dispose(blueprintI().data);
	}

	public double getHeat() {
		double d = heat/service.total();
		if (d > 1)
			d = 1;
		return d;
	}

	@Override
	public ROOM_BATH blueprintI() {
		return (ROOM_BATH) blueprint();
	}

	@Override
	public RoomServiceInstance service() {
		return service;
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
	public double quality() {
		double h = 0.5 + 0.5*getHeat();
		double w = 1; //0.5 + 0.5*water;
		double b = 0.5 + 0.5*blueprintI().constructor.relaxation.get(this);
		return ROOM_SERVICER.defQuality(this, b*h*w);
	}


	@Override
	public long[] productionData() {
		return pData;
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
	
	static class Jobs extends JobPositions<BathInstance> {

		private static final long serialVersionUID = 1L;

		public Jobs(BathInstance ins) {
			super(ins);
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			SETT_JOB j = Crank.init(tx, ty, ins.blueprintI());
			if (j == null)
				return Oven.init(tx, ty, ins.blueprintI());
			return j;
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			return  Crank.init(tx, ty, ins.blueprintI()) != null || Oven.init(tx, ty, ins.blueprintI()) != null;
		}
	}




}
