package settlement.room.food.fish;

import game.GAME;
import init.constant.C;
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
import snake2d.util.datatypes.DIR;
import snake2d.util.rnd.RND;
import util.GUTIL;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class FishInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE {

	
	final Jobs jobs;
	private static final long serialVersionUID = -3170637142258642320l;
	private long[] pData;
	final short sx,sy;
	boolean hasStorage = true;

	FishInstance(ROOM_FISHERY b, TmpArea area, RoomInit init) {
		super(b, area, init);

		BoatMaker.make(this);
		
		int x = -1;
		int y = -1;
		int w = 0;
		
		for (COORDINATE c : body()) {
			if (is(c)) {
				if (SETT.ROOMS().fData.tileData.get(c) == Constructor.B_WORK) {
					
				}else if (SETT.ROOMS().fData.tileData.get(c) == Constructor.B_STORAGE) {
					if (x == -1) {
						x = c.x();
						y = c.y();
					}
				}else if(SETT.TERRAIN().WATER.SHALLOW.is(c)) {
					if (Job.isWork.is(SETT.ROOMS().data.get(c)))
						continue;
					if (w == 0) {
						SETT.ROOMS().data.set(this, c, Job.isWork.set(0));
						w+= RND.rInt(2);
					}else {
						w --;
					}
				}
			}
		}
		
		
		
		if (x == -1 || y == -1)
			GAME.Error(x + " " + y);
		sx = (short) x;
		sy = (short) y;
		
		
		pData = b.productionData.makeData();
		jobs = new Jobs(this);
		
		jobs.randomize();
		jobs.setAlwaysNew();
		
		employees().maxSet((int)blueprintI().constructor.workers.get(this));
		employees().neededSet((int)blueprintI().constructor.workers.get(this));
		activate();
	}
	
	@Override
	protected void loadFix() {
		super.loadFix();
		pData = blueprintI().productionData.makeDataFix(pData);
	}


	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		if (!SETT.ROOMS().fData.item.is(it.tile())) {
			
			if (Job.isWork.is(SETT.ROOMS().data.get(it.tile())) && Job.isShip.is(SETT.ROOMS().data.get(it.tile()))) {
				
				if (!Job.working(SETT.ROOMS().data.get(it.tile())))
					SETT.HALFENTS().dingy.renderBoat(r, shadowBatch, it.x()+C.TILE_SIZEH, it.y()+C.TILE_SIZEH, DIR.ALL.getC(it.ran()), GUTIL.ran2().get(it.tile()), upgrade());
				
			}else {
				int d = SETT.ROOMS().fData.spriteData.get(it.tile());
				
				
				if (d != 0x0F) {
					blueprintI().constructor.sEdge.render(r, shadowBatch, d, it, 0, false);
				}else if (!SETT.TERRAIN().WATER.is.is(it.tile()) && (GUTIL.ran2().get(it.tile()) & 0x0F) == 0) {
					blueprintI().constructor.sMisc.render(r, shadowBatch, 0, it, 0, false);
				}
			}
			
			
		}
		
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
	public ROOM_FISHERY blueprintI() {
		return (ROOM_FISHERY) blueprint();
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return blueprintI().job.storage.get(tx, ty, this);
	}

	@Override
	public Industry industry() {
		return blueprintI().industries().get(0);
	}

	@Override
	public long[] productionData() {
		return pData;
	}
	
	static class Jobs extends JobPositions<FishInstance> {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Jobs(FishInstance ins) {
			super(ins);
		}
		
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
