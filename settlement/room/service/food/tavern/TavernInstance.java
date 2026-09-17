package settlement.room.service.food.tavern;

import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import settlement.room.service.food.eatery.RoomDistribution;
import settlement.room.service.food.eatery.RoomDistribution.InstanceData;
import settlement.room.service.food.eatery.RoomDistribution.RoomDistributionIns;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class TavernInstance extends RoomInstance implements JOBMANAGER_HASER, RoomDistributionIns{
	
	private static final long serialVersionUID = 1L;
	final RoomServiceInstance service;
	final Jobs jobs;
	final InstanceData distData;
	boolean auto = true;
	
	protected TavernInstance(ROOM_TAVERN b, TmpArea area, RoomInit init) {
		super(b, area, init);
		
		jobs = new Jobs(this);
		jobs.setAlwaysNew();

		int sers = 0;
		for (COORDINATE c : body()) {
			if (is(c) && b.service(c.x(), c.y()) != null) {
				sers++;
			}
		}
		distData = b.dist.makeData(sers);
		
		service = new RoomServiceInstance(sers, blueprintI().serviceData);
		
		employees().maxSet((int)Math.ceil(jobs.size()/2.0));
		employees().neededSet((int)Math.ceil(jobs.size()/4.0));
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
		distData.update(distributionNlueData());
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
		distributionNlueData().dispose(this);
		service.dispose(blueprintI().serviceData);
	}

	@Override
	public ROOM_TAVERN blueprintI() {
		return (ROOM_TAVERN) blueprint();
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}

	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, blueprintI().constructor.coziness.get(this));
	}
	
	static class Jobs extends JobPositions<TavernInstance> {

		private static final long serialVersionUID = 1L;

		public Jobs(TavernInstance ins) {
			super(ins);
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().dist.job(tx, ty);
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			return ins.blueprintI().dist.job(tx, ty) != null;
		}
	}
	
	@Override
	public InstanceData distributionData() {
		return distData;
	}

	@Override
	public RoomDistribution distributionNlueData() {
		return blueprintI().dist;
	}
	

}
