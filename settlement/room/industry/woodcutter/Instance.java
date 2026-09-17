package settlement.room.industry.woodcutter;

import static settlement.main.SETT.TERRAIN;

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
import settlement.room.water.RoomPumpable;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.GUTIL;
import util.rendering.RenderData;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Instance extends RoomInstance implements JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE{

	
	final JobPositions<Instance> jobs;
	private static final long serialVersionUID = -3170637142258642320l;
	private long[] pData;
	final short sx,sy;
	boolean hasStorage = true;
	int workage = 0;
	double irri;
	private double irriNext;
	private short irriI;

	Instance(ROOM_WOODCUTTER b, TmpArea area, RoomInit init) {
		super(b, area, init);

		int x = -1;
		int y = -1;
		final int w = (int)blueprintI().constructor.workers.get(this);
		int ww = 0;
		
		GUTIL.coos().set(0);
		irri = 0;
		
		for (COORDINATE c : body()) {
			if (is(c) && SETT.PATH().reachability.is(c)) {
				
				
				if (SETT.ROOMS().fData.tileData.get(c) == Constructor.B_WORK) {
					b.job.mark(c.x(), c.y(), this);
				}else if (SETT.ROOMS().fData.tileData.get(c) == Constructor.B_STORAGE) {
					if (x == -1) {
						x = c.x();
						y = c.y();
					}
				}else if (SETT.ROOMS().fData.tile.get(c) == null) {
					if(SETT.TILE_MAP().growth.type(c.x(), c.y()) == SETT.TILE_MAP().growth.tree) {
						b.job.mark(c.x(), c.y(), this);
						ww ++;
					}else {
						GUTIL.coos().get().set(c);
						GUTIL.coos().inc();
					}
				}

				irri += SETT.GROUND().MOISTURE_TOT.get(c);
				RoomPumpable.reportChange(c.x(), c.y(), 0);
			}
		}
		

		
		
		
		
		int m = GUTIL.coos().getI();
		GUTIL.coos().shuffle(0, m);
		for (int i = 0; i < m; i++) {
			COORDINATE c = GUTIL.coos().set(i);
			if (ww < w*4) {
				b.job.mark(c.x(), c.y(), this);
				ww++;
			}
		}

		for (COORDINATE c : body()) {
			if (is(c) && b.job.init(c.x(), c.y(), this) != null && !Job.isTreeCurrent(c.x(), c.y())) {
				TERRAIN().DECOR_WOOD.placeFixed(c.x(), c.y());
			}
		}
		
		
		
		if (x == -1 || y == -1)
			GAME.Error(x + " " + y);
		sx = (short) x;
		sy = (short) y;
		
		
		pData = b.productionData.makeData();
		jobs = new Jobs(this);
		
		jobs.randomize();
		
		
		
		
		employees().maxSet(w);
		employees().neededSet(w);
		activate();
	}

	
	
	
	@Override
	protected void loadFix() {
		pData = industry().makeDataFix(pData);
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
	protected boolean renderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		return super.renderAbove(r, shadowBatch, i);
	}
	
	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}

	@Override
	public ROOM_WOODCUTTER blueprintI() {
		return (ROOM_WOODCUTTER) blueprint();
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return blueprintI().job.storage.get(tx, ty, this);
	}

	@Override
	public void updateTileDay(int tx, int ty) {
		blueprintI().job.update(tx, ty, this);
		if (irriI >= area()) {
			irriI = 0;
			irri = irriNext;
			irriNext = 0;
		}
		irriNext += SETT.GROUND().MOISTURE_TOT.get(tx, ty);
		irriI ++;
		super.updateTileDay(tx, ty);
	}

	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		if (!SETT.ROOMS().fData.item.is(it.tile())) {
			int d = SETT.ROOMS().fData.spriteData.get(it.tile());
			if (d != 0x0F) {
				blueprintI().constructor.sedge.render(r, shadowBatch, d, it, getDegrade(), false);
			}
		}
		
		return super.render(r, shadowBatch, it);
	}
	

	@Override
	public long[] productionData() {
		return pData;
	}
	
	@Override
	public Industry industry() {
		return blueprintI().industries().get(0);
	}
	
	private static class Jobs extends JobPositions<Instance>{

		public Jobs(Instance ins) {
			super(ins);
			// TODO Auto-generated constructor stub
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
