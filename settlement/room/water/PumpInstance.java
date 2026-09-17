package settlement.room.water;

import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobIterator;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class PumpInstance extends RoomInstance implements JOBMANAGER_HASER {


	private JobIterator jobs;
	private static final long serialVersionUID = -3170637142258642320l;
	
	private int workersHas;
	private int workersNeed;
	public short value;
	private int dw;
	private static int maxWorkAm = 8;
	public double valueMax = 100;
	
	private final short ox,oy;

	PumpInstance(ROOM_PUMP b, TmpArea area, RoomInit init) {
		super(b, area, init);

		jobs = new Jobs(this);
		int px = 0; 
		int py = 0;
		for (COORDINATE c : body()) {
			if (is(c) && SETT.ROOMS().fData.tile.get(c) == blueprintI().constructor.ou) {
				px = c.x();
				py = c.y();
				break;
			}
		}
		
		

		ox = (short) px;
		oy = (short) py;
		
		setEmployees();
		employees().neededSet(employees().max());
		
		valueMax = (int) Math.ceil(25*employees().max());
		
		dw = maxWorkAm-2;
		value = 64;
		workersHas = 1;
		workersNeed = 1;
		activate();
	}

	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}

	@Override
	protected void activateAction() {
		SETT.ROOMS().WATER.updater.reportChange(ox, oy, 0);
	}

	@Override
	protected void deactivateAction() {
		SETT.ROOMS().WATER.updater.reportChange(ox, oy, 0);
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		
		if (!active())
			return;
		workersHas += employees().employed();
		workersNeed += employees().max();
		dw++;
		if (dw >= maxWorkAm) {
			short nv = (short) Math.ceil((1.0 - 0.8*getDegrade())*valueMax*workersHas/workersNeed);
			workersHas = 0;
			workersNeed = 0;
			dw = 0;
			if (nv != value) {
				value = nv;
			
				SETT.ROOMS().WATER.updater.reportChange(ox, oy, 0);
			}
		}
	}
	
	@Override
	protected void dispose() {

	}

	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}


	@Override
	public ROOM_PUMP blueprintI() {
		return (ROOM_PUMP)blueprint();
	}
	
	private void setEmployees() {
		int am = 0;
		for (COORDINATE c : body()) {
			if (is(c) && blueprintI().job.init(c.x(), c.y(), this) != null) {
				am++;
			}
		}
		employees().maxSet(am);
		if (am > employees().max())
			 employees().neededSet(employees().max());
		jobs.searchAgain();
	}
	
	
	
	private static class Jobs extends JobIterator {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public Jobs(PumpInstance ins) {
			super(ins);
		}
		
		@Override
		protected SETT_JOB init(int tx, int ty) {
			PumpInstance ins = SETT.ROOMS().WATER.pump.get(tx, ty);
			if (ins != null)
				return SETT.ROOMS().WATER.pump.job.init(tx, ty, ins);
			return null;
		}
	}

	public int ox() {
		return ox;
	}
	
	public int oy() {
		return oy;
	}
	
	public int output() {
		if (!active())
			return 0;
		return value;
	}

	public double aniSpeed() {
		return (double)employees().employed() / employees().max();
	}

	@Override
	public void upgradeSet(int upgrade) {
		super.upgradeSet(upgrade);
		setEmployees();
	}
	


}
