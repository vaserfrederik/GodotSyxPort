package settlement.room.service.stage;

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
import snake2d.util.misc.CLAMP;
import snake2d.util.rnd.RND;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class StageInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_SERVICER{

	private static final long serialVersionUID = 1L;
	final RoomServiceInstance service;
	final byte off = (byte) RND.rInt(64);
	private final Job job = new Job(this);
	
	private short workers;
	private short services = 0;
	
	protected StageInstance(ROOM_STAGE b, TmpArea area, RoomInit init) {
		super(b, area, init);

		service = new RoomServiceInstance((int) b.constructor.spectators.get(this), blueprintI().data);

		employees().maxSet(job.size());
		employees().neededSet(job.size());
		activate();
		
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		if (active()) {
			if (employees().employed() == 0) {
				if (workers > 0) {
					workers --;
					if (workers == 0) {
						setServices(0);
					}
				}
			}else {
				if (workers < 10) {
					workers = 10;
					setServices(service.total());
				}
			}
		}
		if (day)
			service.updateDay();
	}
	
	@Override
	protected void activateAction() {
		
		if (workers > 0) {
			setServices(service.total());
		}
		
	}

	@Override
	protected void deactivateAction() {
		setServices(0);
		workers = 0;
	}

	void incServices(int s) {
		if (workers > 0)
			setServices(services + s);
	}
	
	boolean hasService() {
		return workers > 0;
	}
	
	private void setServices(int s) {
		service.report(blueprintI().work.service(body().cX(), body().cY()), blueprintI().data, -services, false);
		this.services = (short) CLAMP.i(s, 0, service.total());
		service.report(blueprintI().work.service(body().cX(), body().cY()), blueprintI().data, services, true);
	}
	
	int services() {
		return services;
	}
	
	@Override
	public JOB_MANAGER getWork() {
		return job;
	}
	
	@Override
	protected void dispose() {
		service.dispose(blueprintI().data);
	}

	@Override
	public ROOM_STAGE blueprintI() {
		return (ROOM_STAGE) blueprint();
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}

	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, ((double)employees().employed()/employees().max()));
	}
	
	private static class Job extends JobPositions<StageInstance> {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Job(StageInstance ins) {
			super(ins);
			
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			return ins.blueprintI().work.job(tx, ty) != null;
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().work.job(tx, ty);
		}
		
		
	}

}
