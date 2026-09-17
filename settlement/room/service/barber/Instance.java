package settlement.room.service.barber;

import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
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
	final Jobs jobs;
	
	final RoomServiceInstance service;
	boolean auto = true;

	protected Instance(ROOM_BARBER blueprint, TmpArea area, RoomInit init) {
		super(blueprint, area, init);
		jobs = new Jobs(this);
		
		service = new RoomServiceInstance(jobs.size(), blueprintI().data);
		
		employees().maxSet(jobs.size());
		employees().neededSet((int)Math.ceil(blueprint.constructor.workers.get(this)));
		activate();
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i) {
		i.lit();
		return super.render(r, shadowBatch, i);
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		if (day)
			service.updateDay();
		jobs.searchAgain();
	}

	@Override
	protected void activateAction() {
		
	}

	@Override
	protected void deactivateAction() {
		
	}
	
	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}
	
	@Override
	protected void dispose() {
		for (int i = 0; i < jobs.size(); i++) {
			COORDINATE c = jobs.get(i);
			FSERVICE s = blueprintI().ll.service(c.x(), c.y());
			if (s.findableReservedCanBe())
				s.findableReserve();
		}
		
		service.dispose(blueprintI().data);
		
	}
	
	@Override
	public ROOM_BARBER blueprintI() {
		return (ROOM_BARBER) blueprint();
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}

	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, blueprintI().constructor.quality.get(this));
	}
	
	static class Jobs extends JobPositions<Instance> {

		private static final long serialVersionUID = 1L;

		public Jobs(Instance ins) {
			super(ins);
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().ll.job(tx, ty);
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			return ins.blueprintI().ll.job(tx, ty) != null;
			
		}
	}

}
