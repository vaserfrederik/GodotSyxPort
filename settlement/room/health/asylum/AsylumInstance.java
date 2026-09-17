package settlement.room.health.asylum;

import init.constant.C;
import settlement.entity.ENTITY;
import settlement.entity.humanoid.HEvent;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class AsylumInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE {

	private static final long serialVersionUID = 1L;
	final Jobs jobs;
	final static double WORKER_PER_BED = 1d/8d;
	private final short[] cellsXY;
	private short cellI = 0;
	private long[] pData;
	private short used;
	
	protected AsylumInstance(ROOM_ASYLUM b, TmpArea area, RoomInit init) {
		super(b, area, init);
		
		int cells = 0;
		for (COORDINATE c : body()) {
			if (is(c)) {
				candle(c.x(), c.y());
				if (SETT.ROOMS().fData.tileData.get(c) == Constructor.CODE_ENTRANCE) {
					cells++;
				}
			}
		}
		
		cellsXY = new short[cells*2];
		cells = 0;
		for (COORDINATE c : body()) {
			if (is(c)) {
				if (SETT.ROOMS().fData.tileData.get(c) == Constructor.CODE_ENTRANCE) {
					cellsXY[cells++] = (short) c.x();
					cellsXY[cells++] = (short) c.y();
				}
			}
		}
		
		jobs = new Jobs(this);
		jobs.randomize();
		jobs.setAlwaysNew();
		pData = b.consumtion.makeData();
		employees().maxSet((int)Math.ceil(b.constructor.guards.get(this)));
		employees().neededSet((int)Math.ceil(b.constructor.guards.get(this)));
		activate();
		
	}
	
	
	@Override
	protected void loadFix() {
		pData = blueprintI().consumtion.makeDataFix(pData);
	}
	
	void candle(int tx, int ty) {
		
		if (SETT.LIGHTS().is(tx, ty)) {
			SETT.LIGHTS().remove(tx, ty);
			FurnisherItem it = SETT.ROOMS().fData.item.get(tx, ty);
					
			for (DIR d : DIR.ORTHO) {
				if (SETT.ROOMS().fData.item.is(tx, ty, d, it)) {
					SETT.LIGHTS().candle(tx, ty, d.x()*(C.TILE_SIZEH-4), d.y()*(C.TILE_SIZEH-4));
					return;
				}
			}
		}
	}
	
	public int prisoners() {
		return used;
	}
	
	public int prisonersMax() {
		return cellsXY.length/2;
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}

	@Override
	protected void activateAction() {
		blueprintI().incPrisoners(used, prisonersMax());
	}

	@Override
	protected void deactivateAction() {
		for (ENTITY e : SETT.ENTITIES().getAllEnts()) {
			if (e != null && e instanceof Humanoid) {
				Humanoid h = (Humanoid) e;
				HEvent.Handler.removeRoom(h, this);
			}
		}
		
		for (int i = 0; i < cellsXY.length; i+=2) {
			cellI += 2;
			if (cellI >= cellsXY.length)
				cellI = 0;
			int tx = cellsXY[cellI];
			int ty = cellsXY[cellI+1];
			Cell.init(tx, ty).reserveCancel();
		}
		
		blueprintI().incPrisoners(-used, -prisonersMax());
		used = 0;
	}

	void inc(int delta) {
		used += delta;
		if (active())
			blueprintI().incPrisoners(delta, 0);
		
	}
	
	@Override
	protected void updateAction(double updateInterval, boolean day) {
		jobs.searchAgain();
		blueprintI().consumtion.updateRoom(this);
	}
	
	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}
	
	@Override
	protected void dispose() {
		
		
	}

	@Override
	public ROOM_ASYLUM blueprintI() {
		return (ROOM_ASYLUM) blueprint();
	}
	
	COORDINATE registerPrisoner(Humanoid a) {
		if (used == prisonersMax())
			throw new RuntimeException();
		if (!active())
			throw new RuntimeException();
		
		if (is(a.tc())) {
			Cell c = Cell.init(a.tc().x(), a.tc().y());
			if (c != null && !c.reservedIs()) {
				c.reserve();
				return c.coo;
			}
		}
		
		for (int i = 0; i < cellsXY.length; i+=2) {
			cellI += 2;
			if (cellI >= cellsXY.length)
				cellI = 0;
			int tx = cellsXY[cellI];
			int ty = cellsXY[cellI+1];
			Cell c = Cell.init(tx, ty);
			if (!c.reservedIs()) {
				c.reserve();
				return c.coo;
			}
		}
		throw new RuntimeException();
	}
	
	void removePrisoner(int tx, int ty) {
		Cell c = Cell.init(tx, ty);
		if (c == null)
			return;
		c.reserveCancel();
	}
	
	boolean isReserved(int tx, int ty) {
		Cell c = Cell.init(tx, ty);
		return c != null && c.reservedIs();
	}
	
	static class Jobs extends JobPositions<AsylumInstance> {

		private static final long serialVersionUID = 1L;

		public Jobs(AsylumInstance ins) {
			super(ins);
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			if (Food.init(tx, ty) != null) {
				return Food.init(tx, ty);
			}
			return null;
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			if (Food.init(tx, ty) != null)
				return true;
			return false;
			
			
		}
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

}
