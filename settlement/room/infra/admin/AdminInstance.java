package settlement.room.infra.admin;

import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.ROOM_IDATA_INSTANCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class AdminInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_IDATA_INSTANCE{

	private static final long serialVersionUID = 1L;
	final Jobs jobs;
	private long[] pdata;

	protected AdminInstance(ROOM_ADMIN blueprint, TmpArea area, RoomInit init) {
		super(blueprint, area, init);
		jobs = new Jobs(this);
		
		employees().neededSet(jobs.size());
		employees().maxSet(jobs.size());

		jobs.randomize();
		pdata = blueprint.consumption().makeData();
		activate();
		blueprint.data.incStations(jobs.size());
	}
	
	@Override
	protected void loadFix() {
		pdata = blueprintI().consumption().makeDataFix(pdata);
	}
	
//	void performJob(double d) {
//		
//		blueprintI().data.perform(d);
//	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i) {
		i.lit();
		return super.render(r, shadowBatch, i);
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		blueprintI().consumption().updateRoom(this);
		jobs.searchAgain();
	}

	@Override
	protected void activateAction() {
		blueprintI().data.incStations(jobs.size());
	}

	@Override
	protected void deactivateAction() {
		blueprintI().data.incStations(-jobs.size());
	}
	
	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}
	
	@Override
	protected void dispose() {
		blueprintI().consumption().releaseResources(this, this);
		blueprintI().data.incStations(-jobs.size());
	}
	
	@Override
	public ROOM_ADMIN blueprintI() {
		return (ROOM_ADMIN) blueprint();
	}

	@Override
	public long[] productionData() {
		return pdata;
	}
	

	
	static class Jobs extends JobPositions<AdminInstance> {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Jobs(AdminInstance ins) {
			super(ins);
		}
		
		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().job.get(tx, ty);
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
//			if (ROOMS().fData.tile.is(tx, ty, ins.blueprintI().constructor.ww)) {
//				ROOMS().data.set(ins, tx, ty, Job.bit.mask);
//			}else {
//				ROOMS().data.set(ins, tx, ty, Job.ran.set(0, 1 + RND.rInt(Job.ran.mask)));
//			}
			
			return ins.blueprintI().job.get(tx, ty) != null;	
			
		}
	}
	
	

}
