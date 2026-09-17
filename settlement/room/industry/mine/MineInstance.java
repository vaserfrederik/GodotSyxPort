package settlement.room.industry.mine;

import static settlement.main.SETT.GROUND;

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
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class MineInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE {

	
	final JobPositions<MineInstance> jobs;
	private static final long serialVersionUID = -3170637142258642320l;
	private long[] pData;
	final short sx,sy;
	
//	final float o2utputMax;
	
	int workage = 0;
	boolean hasStorage = true;

	MineInstance(ROOM_MINE b, TmpArea area, RoomInit init) {
		super(b, area, init);

		int x = -1;
		int y = -1;
		
//		double max = 0;
		
		for (COORDINATE c : body()) {
			if (is(c)) {
				if (SETT.ROOMS().fData.tileData.get(c) == Constructor.B_WORK) {
					SETT.ROOMS().data.set(this, c, Job.isWork.set(0));
				}else if (SETT.ROOMS().fData.tileData.get(c) == Constructor.B_STORAGE) {
					if (x == -1) {
						x = c.x();
						y = c.y();
					}
				}else if(SETT.MINERALS().getter.is(c, b.minable) && SETT.ROOMS().fData.item.get(c) == null) {
					SETT.ROOMS().data.set(this, c, Job.isWork.set(0));
//					max = Math.max(max, SETT.MINERALS().amountPlayer.get(c));
				}
			}
		}
		
//		this.outputMax = (float) max;
		
		if (x == -1 || y == -1)
			GAME.Error(x + " " + y);
		sx = (short) x;
		sy = (short) y;
		
		
		pData = b.productionData.makeData();
		jobs = new Jobs(this);
		
		jobs.randomize();
		int w = (int) Math.floor(b.constructor.workers.get(this));
		employees().maxSet(w);
		employees().neededSet(w);
		activate();
	}

	@Override
	protected void loadFix() {
		pData = blueprintI().productionData.makeDataFix(pData);
	}

	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		return super.render(r, shadowBatch, it);
	}
	
	@Override
	protected boolean renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		GROUND().renderMinerals(r, i.tile(), i.ran(), i.x(), i.y());
		return super.renderBelow(r, shadowBatch, i);
	}

	@Override
	protected void activateAction() {
		
		
	}

	@Override
	protected void deactivateAction() {

		
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		
		blueprintI().productionData.updateRoom(this);
		
		if (!active())
			return;
		jobs.searchAgain();
		
	}

	@Override
	protected void dispose() {
		for (COORDINATE c : body()) {
			if (blueprintI().job.storage.get(c.x(), c.y(), this) != null)
				blueprintI().job.storage.dispose();
		}
		
	}

	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}

	@Override
	public ROOM_MINE blueprintI() {
		return (ROOM_MINE) blueprint();
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return blueprintI().job.storage.get(tx, ty, this);
	}
	



	@Override
	public long[] productionData() {
		return pData;
	}
	

	@Override
	public Industry industry() {
		return blueprintI().industries().get(0);
	}

	static class Jobs extends JobPositions<MineInstance> {

		public Jobs(MineInstance ins) {
			super(ins);
		}
		
		private static final long serialVersionUID = 8423260307910904017l;
		@Override
		protected boolean isAndInit(int tx, int ty) {
			return get(tx, ty) != null;
		}
		
		@Override
		protected SETT_JOB get(int tx, int ty) {
			return ins.blueprintI().job.init(tx, ty, ins);
		}
		
		
	}

	@Override
	public int industryI() {
		// TODO Auto-generated method stub
		return 0;
	}


}
