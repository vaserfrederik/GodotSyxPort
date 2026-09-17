package settlement.room.service.breeder;

import settlement.entity.ENTITY;
import settlement.entity.humanoid.HEvent;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class BreederInstance extends RoomInstance implements ROOM_PRODUCER_INSTANCE, JOBMANAGER_HASER{

	private static final long serialVersionUID = 1L;
	
	private final long[] pData;
	private final Jobs jobs;
	double kidsProduction = 0;
	boolean auto = true;
	
	protected BreederInstance(ROOM_BREEDER blue, TmpArea area, RoomInit init) {
		super(blue, area, init);
		pData = blue.productionData.makeData();
		
		jobs = new Jobs(this);
		
		employees().maxSet(jobs.size());
		employees().neededSet((int) (Math.ceil(blue.constructor.workers.get(this))));
		
		activate();
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}
	
	@Override
	protected void activateAction() {
		
	}

	@Override
	protected void deactivateAction() {
		
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		blueprintI().productionData.updateRoom(this);
		jobs.searchAgain();
	}
	
	@Override
	public void updateTileDay(int tx, int ty) {
		blueprintI().station.update(tx, ty);
	}

	@Override
	protected void dispose() {
		for (COORDINATE c : body()) {
			if (is(c)) {
				blueprintI().station.dispose(c.x(), c.y());
			}
		}
		for (ENTITY e : SETT.ENTITIES().getAllEnts()) {
			if (e instanceof Humanoid) {
				Humanoid a = (Humanoid) e;
				HEvent.Handler.removeRoom(a, this);
				
			}
		}
		
	}
	@Override
	public ROOM_BREEDER blueprintI() {
		return (ROOM_BREEDER) blueprint();
	}


	@Override
	public long[] productionData() {
		return pData;
	}

	@Override
	public JobPositions<BreederInstance> getWork() {
		return jobs;
	}
	
	@Override
	public Industry industry() {
		return blueprintI().industries().get(0);
	}
	
	private static class Jobs extends JobPositions<BreederInstance> {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Jobs(BreederInstance ins) {
			super(ins);
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			return ins.blueprintI().station.init(tx, ty);
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().station.get(tx, ty);
		}
		
		
	}

	@Override
	public int industryI() {
		return 0;
	}

}
