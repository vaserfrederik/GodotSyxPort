package settlement.room.infra.inn;

import game.time.TIME;
import game.tourism.Review;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import util.rendering.RenderData;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public final class InnInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_SERVICER{

	private static final long serialVersionUID = 1L;
	final Jobs jobs;
	final static double WORKER_PER_BED = 1d/8d;
	boolean auto = false;
	int earnings;
	int earningsLast;
	byte year = (byte) TIME.years().bitsSinceStart();
	
	final RoomServiceInstance service;
	
	final Review[] reviews = new Review[] {
		new Review(),
		new Review(),
		new Review(),
		new Review()
	};
	
	protected InnInstance(ROOM_INN b, TmpArea area, RoomInit init) {
		super (b, area, init);
		jobs = new Jobs(this);
		int total = (int) b.constructor.beds.get(this);
		employees().maxSet(2*(int)Math.ceil(b.constructor.workers.get(this)));
		employees().neededSet((int)Math.ceil(b.constructor.workers.get(this)));
		service = new RoomServiceInstance(total, blueprintI().service);
		activate();
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}
	
	@Override
	protected boolean renderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		super.renderAbove(r, shadowBatch, i);
		blueprintI().constructor.aboveR(r, shadowBatch, i, getDegrade());
		return false;
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
		if (year != (byte)TIME.years().bitCurrent()) {
			year = (byte) TIME.years().bitCurrent();
			earningsLast = earnings;
			earnings = 0;
		}
		if (day)
			service.updateDay();
	}
	
	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}
	
	@Override
	protected void dispose() {
		service.dispose(blueprintI().service);
		
	}

	@Override
	public ROOM_INN blueprintI() {
		return (ROOM_INN) blueprint();
	}
	
	static class Jobs extends JobPositions<InnInstance> {

		private static final long serialVersionUID = 1L;

		public Jobs(InnInstance ins) {
			super(ins);
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			ABed b = ins.blueprintI().bed.init(tx, ty);
			if (b != null)
				return b.job;
			return null;
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			
			return ins.blueprintI().bed.init(tx, ty) != null;
			
			
		}
	}
	
	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, blueprintI().constructor.coziness.get(this));
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}

}
