package settlement.room.health.physician;

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
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class Instance extends RoomInstance implements JOBMANAGER_HASER, ROOM_SERVICER{

	private static final long serialVersionUID = 1L;
	final JobPositions<Instance> jobs;
	final RoomServiceInstance service;
	boolean auto = true;
	
	protected Instance(ROOM_PHYSICIAN b, TmpArea area, RoomInit init) {
		super(b, area, init);
		
		jobs = new Jobs(this);
		jobs.setAlwaysNew();
		service = new RoomServiceInstance(jobs.size(), blueprintI().data);
		employees().maxSet((int) Math.ceil(blueprintI().constructor.workers.get(this)));
		employees().neededSet(employees().max());
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
		if (day) {
			service.updateDay();
		}
	}
	
	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}
	
	@Override
	protected void dispose() {
		
		
		for (COORDINATE c : body()) {
			if (is(c))
				blueprintI().s.dispose(this, c.x(), c.y());
		}
		service.dispose(blueprintI().data);
	}
	
	@Override
	public RoomServiceInstance service() {
		return service;
	}
	
	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, blueprintI().constructor.quality.get(this));
	}

	@Override
	public ROOM_PHYSICIAN blueprintI() {
		return (ROOM_PHYSICIAN) blueprint();
	}
	
	private static class Jobs extends JobPositions<Instance> {

		private static final long serialVersionUID = 1L;

		public Jobs(Instance ins) {
			super(ins);

		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			return ins.blueprintI().s.getJ(tx, ty) != null;
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().s.getJ(tx, ty);
		}
		
		
	}
	

}
