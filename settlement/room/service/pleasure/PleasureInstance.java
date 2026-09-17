package settlement.room.service.pleasure;

import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import util.rendering.RenderData;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class PleasureInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_SERVICER{

	private static final long serialVersionUID = 1L;
	final Jobs jobs;
	boolean auto = false;
	
	final RoomServiceInstance service;
	
	protected PleasureInstance(ROOM_PLEASURE b, TmpArea area, RoomInit init) {
		super (b, area, init);
		jobs = new Jobs(this);
		int total = jobs.size();
		employees().maxSet(jobs.size());
		employees().neededSet(jobs.size());
		service = new RoomServiceInstance(total, blueprintI().service);
		activate();
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}
	
	@Override
	protected boolean renderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		super.renderAbove(r, shadowBatch, i);
		blueprintI().constructor.aboveR(r, shadowBatch, i, getDegrade());
		return false;
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
	public JOB_MANAGER getWork() {
		return jobs;
	}
	
	@Override
	protected void dispose() {
		service.dispose(blueprintI().service);
		
	}

	@Override
	public ROOM_PLEASURE blueprintI() {
		return (ROOM_PLEASURE) blueprint();
	}
	
	static class Jobs extends JobPositions<PleasureInstance> {

		private static final long serialVersionUID = 1L;

		public Jobs(PleasureInstance ins) {
			super(ins);
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			ABed b = ins.blueprintI().bed.init(tx, ty);
			if (b != null)
				return b.job;
			return null;
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			
			return ins.is(tx, ty) && ins.blueprintI().bed.init(tx, ty) != null;
			
			
		}
	}
	
	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, blueprintI().constructor.coziness.get(this));
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}

}
