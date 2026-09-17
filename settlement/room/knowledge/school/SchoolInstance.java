package settlement.room.knowledge.school;

import settlement.entity.humanoid.Humanoid;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
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

final class SchoolInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE, ROOM_SERVICER{

	private static final long serialVersionUID = 1L;
	final Jobs jobs;
	private long[] pdata;
	private final RoomServiceInstance service;

	protected SchoolInstance(ROOM_SCHOOL blueprint, TmpArea area, RoomInit init) {
		super(blueprint, area, init);
		jobs = new Jobs(this);
		service = new RoomServiceInstance(jobs.size(), blueprint.service);
		
		employees().neededSet((int) Math.ceil(jobs.size()/8));
		employees().maxSet((int) Math.ceil(jobs.size()/3));
		jobs.randomize();
		pdata = blueprint.industry.makeData();
		activate();
	}
	
	@Override
	protected void loadFix() {
		pdata = industry().makeDataFix(pdata);
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i) {
		i.lit();
		return super.render(r, shadowBatch, i);
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		blueprintI().industry.updateRoom(this);
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
		for (COORDINATE c : body()) {
			if (is(c)) {
				blueprintI().station.dispose(c.x(), c.y());
			}
		}
		service.dispose(blueprintI().service);
	}
	
	@Override
	public ROOM_SCHOOL blueprintI() {
		return (ROOM_SCHOOL) blueprint();
	}

	@Override
	public long[] productionData() {
		return pdata;
	}
	
	@Override
	public Industry industry() {
		return blueprintI().industries().get(0);
	}
	
	static class Jobs extends JobPositions<SchoolInstance> {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Jobs(SchoolInstance ins) {
			super(ins);
		}
		
		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().station.job(tx, ty);
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			return ins.blueprintI().station.job(tx, ty) != null;	
			
		}
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
	public int industryI() {
		// TODO Auto-generated method stub
		return 0;
	}
	
	@Override
	public double productionRate(RoomInstance ins, Humanoid h, Industry in, IndustryResource oo) {
		double d = service.load()*service.total();
		return d;
	}



	
}
