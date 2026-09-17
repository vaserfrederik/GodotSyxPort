package settlement.room.infra.embassy;

import java.io.Serializable;

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

final class EmbassyInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_IDATA_INSTANCE{

	private static final long serialVersionUID = 1L;
	final Jobs jobs;

	private long[] pdata;
	
	protected EmbassyInstance(ROOM_EMBASSY blueprint, TmpArea area, RoomInit init) {
		super(blueprint, area, init);
		jobs = new Jobs(this);
		
		employees().neededSet((int) Math.ceil(jobs.size()));
		employees().maxSet(jobs.size());


		blueprint.data.incStations(jobs.size());
		pdata = blueprintI().consumption().makeData();
		activate();
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i) {
		i.lit();
		return super.render(r, shadowBatch, i);
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		jobs.searchAgain();
		blueprintI().consumption().updateRoom(this);
	}

	@Override
	protected void loadFix() {
		pdata = blueprintI().consumption().makeDataFix(pdata);
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
		blueprintI().data.incStations(-jobs.size());
		blueprintI().consumption().releaseResources(this, this);
		
	}
	

	
	@Override
	public ROOM_EMBASSY blueprintI() {
		return (ROOM_EMBASSY) blueprint();
	}
	
	
	
	static class Jobs extends JobPositions<EmbassyInstance> {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Jobs(EmbassyInstance ins) {
			super(ins);
		}
		
		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().job.get(tx, ty);
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			return ins.blueprintI().job.get(tx, ty) != null;	
			
		}
	}
	


	
	
	@Override
	public long[] productionData() {
		return pdata;
	}



	static class Res implements Serializable {
		
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		public int reserved;
		public int current;
		public boolean unreachable = false;
		public boolean disabled = true;
		
	}
	
	
}
