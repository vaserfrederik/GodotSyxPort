package settlement.room.law.stockade;

import static settlement.main.SETT.TWIDTH;

import init.resources.RBIT.RBITImp;
import init.resources.RESOURCES;
import init.resources.ResGEat;
import init.type.CAUSE_LEAVES;
import settlement.entity.ENTITY;
import settlement.entity.humanoid.HEvent;
import settlement.entity.humanoid.Humanoid;
import settlement.entity.humanoid.ai.types.prisoner.AIModule_Prisoner;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.RESOURCE_TILE;
import settlement.path.AVAILABILITY;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import settlement.stats.STATS;
import snake2d.Renderer;
import snake2d.util.misc.CLAMP;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class StockInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE {

	private static final long serialVersionUID = 1L;
	
	final short prisonersMax;
	short prisonersCurrent;
	final RBITImp fetch = new RBITImp();	
	private long[] productionData;
	final Jobs jobs;
	float riotChance = 1;
	boolean hasWarned = false;
	
	StockInstance(ROOM_STOCKADE p, TmpArea area, RoomInit init) {
		super(p, area, init);


		


		prisonersMax = (short) Math.ceil(blueprintI().constructor.prisoners.get(this));
		double work = blueprintI().constructor.workers.get(this);
		employees().maxSet((int) Math.ceil(work));
		employees().neededSet((int) Math.ceil(work));
		productionData = blueprintI().indu.makeData();
		jobs = new Jobs(this);
		for (ResGEat e : RESOURCES.EDI().all())
			if (e.serve)
				fetch.or(e.resource);
		activate();
		
	}

	@Override
	protected void loadFix() {
		productionData = blueprintI().indu.makeDataFix(productionData);
	}
	
	@Override
	public void updateTileDay(int tx, int ty) {
		super.updateTileDay(tx, ty);
	}
	
	@Override
	protected void updateAction(double updateInterval, boolean day) {
		
		blueprintI().indu.updateRoom(this);
		jobs.searchAgain();
		if (day && prisonersCurrent > 0) {
			
			float prev = riotChance;
			
			double v = (double)2.0*employees().employed()/employees().max() - 1;
			v /= 2;
			
			if (!jobs.resNotFound.isClear())
				v -= 1;
			riotChance += v;
			riotChance = (float) CLAMP.d(riotChance, 0, 1);
			if (riotChance < 0.5 && riotChance < prev && !hasWarned) {
				Gui.mWarn(this);
				hasWarned = true;
			}else if (riotChance <= 0) {
				Gui.m(this);
				hasWarned = false;
				riotChance = 1;
				for (ENTITY e : SETT.ENTITIES().getAllEnts()) {
					if (e instanceof Humanoid) {
						Humanoid a = (Humanoid) e;
						if (AIModule_Prisoner.isPrisoner(a, this)) {
							STATS.LAW().escapeInc();
							a.kill(false, CAUSE_LEAVES.OTHER());
						}		
					}
				}
				
				
			}
		}
		
		
	}
	

	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		boolean ret = super.render(r, shadowBatch, it);
		blueprintI().constructor.renderFence(r, shadowBatch, it, 0, true);
		return ret;
	}

	@Override
	protected void activateAction() {
		blueprintI().prisoners += prisonersCurrent;
		blueprintI().prisonersMax += prisonersMax;
		
	}

	@Override
	protected void deactivateAction() {

		for (ENTITY e : SETT.ENTITIES().getAllEnts()) {
			if (e != null && e instanceof Humanoid) {
				Humanoid h = (Humanoid) e;
				HEvent.Handler.removeRoom(h, this);
			}
		}
		blueprintI().prisoners -= prisonersCurrent;
		blueprintI().prisonersMax -= prisonersMax;
		prisonersCurrent = 0;
	}

	
	@Override
	protected void dispose() {
		

		
	}

	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}

	@Override
	public ROOM_STOCKADE blueprintI() {
		return (ROOM_STOCKADE) blueprint();
	}

	@Override
	protected AVAILABILITY getAvailability(int tile) {
		int tx = tile%SETT.TWIDTH;
		int ty = tile/SETT.TWIDTH;
		if (blueprintI().constructor.isFence(this, tx, ty))
			return AVAILABILITY.SOLID;
		return super.getAvailability(tile);
	}

	@Override
	public void destroyTile(int tx, int ty) {
		super.destroyTile(tx, ty);
	}

	@Override
	public boolean destroyTileCan(int tx, int ty) {
		return getAvailability(tx+ty*TWIDTH).player < 0;
	}

	@Override
	public ROOM_DEGRADER degrader(int tx, int ty) {
		return null;
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return null;

	}
	
	@Override
	public long[] productionData() {
		return productionData;
	}

	@Override
	public Industry industry() {
		return blueprintI().indu;
	}

	@Override
	public int industryI() {
		return 0;
	}

	static class Jobs extends JobPositions<StockInstance> {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Jobs(StockInstance ins) {
			super(ins);
			setAlwaysNew();
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			return get(tx, ty) != null;
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().job.job(tx, ty);
		}
		
	}
	
	
}
