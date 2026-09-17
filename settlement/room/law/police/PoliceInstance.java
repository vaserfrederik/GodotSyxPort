package settlement.room.law.police;

import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class PoliceInstance extends RoomInstance implements JOBMANAGER_HASER{

	private static final long serialVersionUID = 1L;
	final Jobs jobs;
	
	int prisoners = 0;
	
	protected PoliceInstance(ROOM_POLICE b, TmpArea area, RoomInit init) {
		super(b, area, init);
		
		int spots = 0;
		
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			if ((SETT.ROOMS().fData.tileData.get(c) & PoliceConstructor.bitService) == PoliceConstructor.bitService) {
				spots ++;
			}
		}
		
		jobs = new Jobs(this);
		
		employees().maxSet(spots*3);
		employees().neededSet(spots);
		activate();
		
	}

	public int prisonersMax() {
		return (int) Math.ceil(employees().employed()/3.0);
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
		for (COORDINATE c : body()) {
			if (is(c) && blueprintI().work.job(c.x(), c.y()) != null) {
				blueprintI().work.dispose(c.x(), c.y());
			}
		}
	}

	@Override
	public void updateTileDay(int tx, int ty) {
		blueprintI().work.update(tx, ty); 
		jobs.searchAgain();
	}

	@Override
	public ROOM_POLICE blueprintI() {
		return (ROOM_POLICE) blueprint();
	}
	
	static class Jobs extends JobPositions<PoliceInstance> {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Jobs(PoliceInstance ins) {
			super(ins);
			setAlwaysNew();
			randomize();
		}
		
		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().work.job(tx, ty);
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {			
			return ins.blueprintI().work.job(tx, ty) != null;	
			
		}
	}

	@Override
	protected void dispose() {
		// TODO Auto-generated method stub
		
	}


	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}

}
