package settlement.room.service.food.eatery;

import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.RESOURCE_TILE;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobIterator;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomState;
import settlement.room.service.food.eatery.RoomDistribution.InstanceData;
import settlement.room.service.food.eatery.RoomDistribution.RoomDistributionIns;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class EateryInstance extends RoomInstance implements JOBMANAGER_HASER, RoomDistributionIns{

	private final static long serialVersionUID = -7063521835843676015l;
	boolean autoE = true;
	private final JobIterator jobs;
	final RoomServiceInstance service;
	final InstanceData distData;

	EateryInstance(ROOM_EATERY p, TmpArea area, RoomInit init) {
		super(p, area, init);

		int maxAmount = 2*(int) blueprintI().constructor.storage.get(this);
		distData = p.dist.makeData(maxAmount);
		jobs = new JobIterator(this) {
			private static final long serialVersionUID = 1L;

			@Override
			protected SETT_JOB init(int tx, int ty) {
				return blueprintI().dist.job(tx, ty);
			}
		};
		//jobs.setAlwaysNewJob();
		
		int m = 0;
		for (COORDINATE c : body()) {
			if (is(c) && blueprintI().constructor.isCrate(c.x(), c.y()))
				m++;
		}
		service = new RoomServiceInstance(m, blueprintI().service);
		employees().maxSet(m);
		employees().neededSet((int) Math.ceil(blueprintI().constructor.workers.get(this)));
		activate();
	}

	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		super.render(r, shadowBatch, it);
		it.lit();
		return false;
	}

	@Override
	protected void loadFix() {
		
	}
	
	@Override
	protected void updateAction(double ds, boolean day) {
		if (day)
			service.updateDay();
		distData.update(distributionNlueData());
		jobs.searchAgain();
		if (!active() || employees().employed() <= 0)
			return;
		
	}
	
	@Override
	public void updateTileDay(int tx, int ty) {
		
	}
	
	@Override
	protected void dispose() {
		
		distributionNlueData().dispose(this);
		service.dispose(blueprintI().service);
		
	}
	
	@Override
	public ROOM_EATERY blueprintI() {
		return (ROOM_EATERY) blueprint();
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return null;
	}
	
	@Override
	public TILE_STORAGE storage(int tx, int ty) {
		return null;
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
	public JOB_MANAGER getWork() {
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
	
	@Override
	public RoomState makeState(int tx, int ty, boolean broken) {
		return distributionNlueData().makeState(this, broken);
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