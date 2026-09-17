package settlement.room.industry.workshop;

import static settlement.main.SETT.ROOMS;

import game.GAME;
import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.RESOURCE_TILE;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class WorkshopInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE{


	JobPositions<WorkshopInstance> jobs;
	private static final long serialVersionUID = -3170637142258642320l;
	private long[] pData;
	boolean hasStorage = true;
	final short sx,sy;
	short WI = 0;
	private short industry = -1;
	

	WorkshopInstance(ROOM_WORKSHOP b, TmpArea area, RoomInit init) {
		super(b, area, init);
		setIndustry(0);
		int x = -1;
		int y = -1;
		
		for (COORDINATE c : body()) {
			if (is(c)) {
				if (SETT.ROOMS().fData.tileData.get(c) == Constructor.B_STORAGE) {
					if (x == -1) {
						x = c.x();
						y = c.y();
					}
				}
			}
		}
		
		if (x == -1 || y == -1)
			GAME.Error(x + " " + y);
		sx = (short) x;
		sy = (short) y;
		
		jobs = new Jobs(this);
		
		
		employees().maxSet(jobs.size());
		employees().neededSet(jobs.size());
		activate();
	}
	

	@Override
	public Industry industry() {
		return blueprintI().indus.get(industry);
	}


	@Override
	public void setIndustry(int i) {
		
		if (i == industry)
			return;
		
		Industry in = blueprintI().industries().get(i);
		if (in == null)
			return;
		pData = in.makeData();
		
		
		if (industry != -1) {

			for (COORDINATE c : body()) {
				if (!is(c))
					continue;
				if (SETT.ROOMS().fData.tileData.is(c.x(), c.y(), Constructor.B_WORK)) {
					if (blueprintI().job.FETCH.get(c.x(), c.y(), this) != null)
						blueprintI().job.FETCH.dispose();
				}
			}
			if (blueprintI().indus.get(industry).outs().get(0).resource != blueprintI().indus.get(i).outs().get(0).resource) {
				hasStorage = true;
				for (COORDINATE c : body()) {
					if (!is(c))
						continue;
					if (blueprintI().job.storage.get(c.x(), c.y(), this) != null)
						blueprintI().job.storage.dispose();
				}
			}
			jobs.searchAgain();
		}
		WI = 0;
		industry = (byte) i;
		
		
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

		
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		
		industry().updateRoom(this);
		
		if (!active())
			return;
		jobs.searchAgain();
		updateIndustryLocks();
		
	}

	@Override
	protected void dispose() {

		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			if (blueprintI().job.storage.get(c.x(), c.y(), this) != null)
				blueprintI().job.storage.dispose();
			else if (blueprintI().job.FETCH.get(c.x(), c.y(), this) != null)
				blueprintI().job.FETCH.dispose();
			
		}
	}

	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}


	@Override
	public ROOM_WORKSHOP blueprintI() {
		return (ROOM_WORKSHOP)blueprint();
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return blueprintI().job.storage.get(tx, ty, this);
	}
	



	@Override
	public long[] productionData() {
		return pData;
	}
	
	
	
	static class Jobs extends JobPositions<WorkshopInstance> {

		public Jobs(WorkshopInstance ins) {
			super(ins);
		}
		
		private static final long serialVersionUID = 8423260307910904017l;
		@Override
		protected boolean isAndInit(int tx, int ty) {
			ROOMS().data.set(ins, tx, ty, 0);
			return get(tx, ty) != null;
		}
		
		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().job.init(tx, ty, ins);
		}
	}



	@Override
	public int industryI() {
		return industry;
	}

	


}
