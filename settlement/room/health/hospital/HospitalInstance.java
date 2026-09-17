package settlement.room.health.hospital;

import game.faction.FACTIONS;
import game.faction.Faction;
import init.value.Lockable;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobIterator;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomState;
import settlement.room.main.util.RoomState.RoomStateInstance;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class HospitalInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE, ROOM_SERVICER{


	private static final long serialVersionUID = 1L;

	final Jobs jobs;
	private long[] pData;
	private final RoomServiceInstance service;
	boolean[] fetch;
	
	protected HospitalInstance(ROOM_HOSPITAL blue, TmpArea area, RoomInit init) {
		super(blue, area, init);
		jobs = new Jobs(this);
		
		int j = 0;
		for (COORDINATE c : body()) {
			if (is(c) && Bed.job(c.x(), c.y()) != null)
				j++;
		}
		
		fetch = new boolean[blue.resLocks.size()];
		service = new RoomServiceInstance(j, blue.service());
		
		
		employees().maxSet((int)Math.ceil(blue.constructor.workers.get(this)));
		employees().neededSet((int)Math.ceil(blue.constructor.workers.get(this)));
		pData = blue.consumtion.makeData();
		
		int ii = 0;
		for (Lockable<Faction> l : blue.resLocks) {
			fetch[ii++] = l.passes(FACTIONS.player());
		}
		activate();
	}
	
	@Override
	protected void loadFix() {
		pData = blueprintI().consumtion.makeDataFix(pData);
		if (fetch == null || fetch.length != blueprintI().resLocks.size())
			fetch = new boolean[blueprintI().resLocks.size()];
	}

	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		if (day) {
			service.updateDay();
			jobs.searchAgain();
		}
	}

	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}

	@Override
	protected void dispose() {
		for (COORDINATE c : body()) {
			if (is(c) && Bed.service(c.x(), c.y()) != null)
				Bed.service(c.x(), c.y()).findableReserve();
		}
		service.dispose(blueprintI().service);
	}

	@Override
	public ROOM_HOSPITAL blueprintI() {
		return (ROOM_HOSPITAL) blueprint();
	}

	@Override
	protected void activateAction() {
		// TODO Auto-generated method stub
		
	}


	@Override
	protected void deactivateAction() {
		// TODO Auto-generated method stub
		
	}


	@Override
	public long[] productionData() {
		return pData;
	}
	
	@Override
	public Industry industry() {
		return blueprintI().industries().get(0);
	}
	
	@Override
	public int industryI() {
		return 0;
	}
	
	static class Jobs extends JobIterator {

		private static final long serialVersionUID = 1L;

		public Jobs(HospitalInstance ins) {
			super(ins);
			setAlwaysNewJob();
			randomize();
		}

		@Override
		protected SETT_JOB init(int tx, int ty) {
			return Bed.job(tx, ty);
		}
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}


	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, 1);
	}
	
	@Override
	public RoomState makeState(int tx, int ty, boolean broken) {
		return new State(this);
	}
	
	private static class State extends RoomStateInstance {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		private boolean[] opium;
		
		public State(HospitalInstance ins) {
			super(ins);
			this.opium = ins.fetch;
		}
		
		@Override
		protected void applyIns(RoomInstance ins) {
			if (ins instanceof HospitalInstance) {
				((HospitalInstance) ins).fetch = opium;
			}
			super.applyIns(ins);
		}
		
	}

}
