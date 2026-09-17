package settlement.room.infra.elderly;

import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.path.AVAILABILITY;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobIterator;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.GUTIL;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class ResthomeInstance extends RoomInstance implements JOBMANAGER_HASER{

	private static final long serialVersionUID = 1L;
	final Jobs jobs;

	protected ResthomeInstance(ROOM_RESTHOME blueprint, TmpArea area, RoomInit init) {
		super(blueprint, area, init);
		
		int work = 0;
		int open = 0;
		
		GUTIL.coos().set(0);
		
		for (COORDINATE c : body()) {
			if (is(c)) {
				if (SETT.ROOMS().fData.tileData.get(c) != 0) {
					blueprint.job.set(this, c.x(), c.y());
					work++;
				}
				else if (SETT.ROOMS().fData.availability.get(c) == AVAILABILITY.ROOM) {
					GUTIL.coos().set(open).set(c);
					open++;
				}
			}
		}
		
		int am = (int) blueprint.constructor.stations.get(this);
		am -= work;
		if (am > open)
			am = open;
		
		GUTIL.coos().shuffle(am);
		
		for (int i = 0; i < am; i++) {
			blueprint.job.set(this, GUTIL.coos().set(i).x(), GUTIL.coos().set(i).y());
		}
		
		
		
		jobs = new Jobs(this);
		jobs.randomize();
		jobs.setAlwaysNewJob();
		employees().neededSet(am + work);
		employees().maxSet(am + work);

		
		
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

	}
	
	@Override
	public ROOM_RESTHOME blueprintI() {
		return (ROOM_RESTHOME) blueprint();
	}
	
	static class Jobs extends JobIterator {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Jobs(ResthomeInstance ins) {
			super(ins);
		}

		@Override
		protected SETT_JOB init(int tx, int ty) {
			return ((ROOM_RESTHOME) ins().blueprintI()).job.get(tx, ty);
		}

	}

}
