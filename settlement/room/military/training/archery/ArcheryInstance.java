package settlement.room.military.training.archery;

import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobIterator;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class ArcheryInstance extends RoomInstance implements JOBMANAGER_HASER{

	private static final long serialVersionUID = 1L;
	private final JobIterator jobs;
	
	ArcheryInstance(ROOM_ARCHERY b, TmpArea area, RoomInit init) {
		super(b, area, init);
		int am = 0;
		for (COORDINATE c : body()) {
			if (is(c)) {
				ArcheryThing t = b.thing.init(c.x(), c.y());
				
				if (t != null) {
					am++;
				}
			}
		}
		jobs = new Jobs(this);
		employees().maxSet(am);
		employees().neededSet(am);
		
		activate();
	}

	private static class Jobs extends JobIterator {
		public Jobs(RoomInstance ins) {
			super(ins);
			// TODO Auto-generated constructor stub
		}

		private static final long serialVersionUID = 1L;

		@Override
		protected SETT_JOB init(int tx, int ty) {
			return ((ROOM_ARCHERY) ins().blueprintI()).thing.init(tx, ty);
		}
	}
	
	@Override
	public ROOM_ARCHERY blueprintI() {
		return (ROOM_ARCHERY) blueprint();
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
	}
	
	@Override
	protected void dispose() {
		
	}

	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		i.lit();
		return super.render(r, shadowBatch, i);
	}

	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}

	
}
