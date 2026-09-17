package settlement.room.service.nursery;

import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.GUTIL;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class NurseryInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_SERVICER{

	private static final long serialVersionUID = 1L;
	
	private final Jobs jobs;
	private final RoomServiceInstance service;
	
	protected NurseryInstance(ROOM_NURSERY blue, TmpArea area, RoomInit init) {
		super(blue, area, init);
		
		GUTIL.coos().set(0);
		
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			if (SETT.ROOMS().fData.tileData.get(c) == NurseryConstructor.TABLE) {
				blue.ss.init(this, c.x(), c.y());
			}else if (SETT.ROOMS().fData.tileData.get(c) == NurseryConstructor.CARPET) {
				GUTIL.coos().get().set(c);
				GUTIL.coos().inc();
			}
		}
		
		int carps = GUTIL.coos().getI();
		int cc = (int) Math.round(GUTIL.coos().getI()*0.25);
		GUTIL.coos().shuffle(carps);
		for (int i = 0; i < cc; i++) {
			GUTIL.coos().set(i);
			blue.ss.init(this, GUTIL.coos().get().x(), GUTIL.coos().get().y());
		}
		
		
		jobs = new Jobs(this);
		
		employees().maxSet(jobs.size());
		employees().neededSet((int) (Math.ceil(blue.constructor.workers.get(this))));
		service = new RoomServiceInstance(jobs.size(), blue.service());
		System.out.println(jobs.size());
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
		jobs.searchAgain();
		if (day)
			service.updateDay();
	}
	
	@Override
	public void updateTileDay(int tx, int ty) {

	}

	@Override
	protected void dispose() {

		service.dispose(blueprintI().service);
	}
	@Override
	public ROOM_NURSERY blueprintI() {
		return (ROOM_NURSERY) blueprint();
	}


	@Override
	public JobPositions<NurseryInstance> getWork() {
		return jobs;
	}
	
	@Override
	public RoomServiceInstance service() {
		return service;
	}


	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, 1);
	}
	
	private static class Jobs extends JobPositions<NurseryInstance> {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Jobs(NurseryInstance ins) {
			super(ins);
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			if (ins.is(tx, ty))
				return ins.blueprintI().ss.job(tx, ty) != null;
			return false;
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			if (ins.is(tx, ty))
				return ins.blueprintI().ss.job(tx, ty);
			return null;
		}
		
		
	}

}
