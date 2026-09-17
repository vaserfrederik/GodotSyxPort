package settlement.room.service.food.canteen;

import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCES;
import init.resources.ResG;
import init.resources.ResGEat;
import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.RESOURCE_TILE;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobIterator;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomState;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.Alloc;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class CanteenInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE, ROOM_SERVICER{

	private final static long serialVersionUID = -7063521835843676015l;
	
	boolean autoE = true;
	private long[] pdata;
	private int[] amounts = Alloc.ii(RESOURCES.EDI().all().size());
	private int[] amountIncoming = Alloc.ii(RESOURCES.EDI().all().size());
	private int amountTotal = 0;
	final int maxAmount;
	private int serviceReserved = 0;

	
	private final JobIterator jobs;
	private final RBITImp fetchMask = new RBITImp().or(RESOURCES.EDI().mask);
	private final RBITImp useMask = new RBITImp();
	final RoomServiceInstance service;
	short tableX = -1;
	short tableY = -1;

	CanteenInstance(ROOM_CANTEEN p, TmpArea area, RoomInit init) {
		super(p, area, init);

		
		jobs = new JobIterator(this) {
			private static final long serialVersionUID = 1L;

			@Override
			protected SETT_JOB init(int tx, int ty) {
				return blueprintI().job.get(tx, ty);
			}
		};
		//jobs.setAlwaysNewJob();
		
		int m = 0;
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			if (tableX == -1 && is(c)) {
				tableX = (short) c.x();
				tableY = (short) c.y();
			}
			if (is(c) && p.food.get(c.x(), c.y()) != null)
				m++;
		}
		pdata = blueprintI().industryFuel.makeData();
		service = new RoomServiceInstance(m*SService.MAX, blueprintI().service);
		maxAmount = (int) 2*m;
		
		employees().maxSet((int) Math.ceil(blueprintI().constructor.workers.get(this)*2));
		employees().neededSet((int) Math.ceil(blueprintI().constructor.workers.get(this)));
		activate();
		
		for (ResGEat e : RESOURCES.EDI().all()) {
			if (e.serve) {
				useMask.or(e.resource);
			}
		}
		
		fetchMask.and(useMask);
	}
	
	@Override
	protected void loadFix() {
		pdata = industry().makeDataFix(pdata);
		if (amounts.length != RESOURCES.EDI().all().size()) {
			int[] ams = Alloc.ii(RESOURCES.EDI().all().size());
			int[] amsI = Alloc.ii(RESOURCES.EDI().all().size());
			amountTotal = 0;
			fetchMask.clear();
			fetchMask.or(RESOURCES.EDI().mask);
			useMask.clear();
			for (int i = 0; i < ams.length; i++) {
				ams[i] = amounts[i%amounts.length];
				amsI[i] = amountIncoming[i%amounts.length];
				amountTotal += ams[i];
			}
			this.amounts = ams;
			this.amountIncoming = amsI;
		}
	}

	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		super.render(r, shadowBatch, it);
		it.lit();
		return false;
	}

	@Override
	protected void updateAction(double ds, boolean day) {

		if (day)
			service.updateDay();
		blueprintI().industryFuel.updateRoom(this);
		jobs.searchAgain();
		if (tableX == -1) {
			tableX = (short) body().x1();
			tableY = (short) body().y1();
		}

		
	}
	
	public int amount(ResG e) {
		return amounts[e.index()];
	}
	
	public int amountReserved(ResG e) {
		return amountIncoming[e.index()];
	}
	
	public int amountTotal() {
		return amountTotal;
	}
	
	public int serviceReserved() {
		return serviceReserved;
	}
	
	public RBIT fetchMask() {
		return fetchMask;
	}
	
	public boolean uses(ResG e) {
		return useMask.has(e.resource.bit);
	}
	
	public void usesToggle(ResG e) {
		useMask.toggle(e.resource);
		setMask(e);
	}
	
	void tally(ResG e, int dAmount, int amountReserved) {
		amounts[e.index()] += dAmount;
		this.amountIncoming[e.index()] += amountReserved;
		amountTotal += dAmount;
		blueprintI().total += dAmount;
		blueprintI().amounts[e.index()] += dAmount;
		setMask(e);
	}
	
	void serviceTally(int dReserved) {
		serviceReserved += dReserved;
	}
	
	void consume(ResG e, int amount, int tx, int ty) {
		tally(e, -amount, 0);
		blueprintI().food.get(tx, ty).check();
	}
	
	private void setMask(ResG e) {
		if (amounts[e.index()] + amountIncoming[e.index()] < maxAmount) {
			fetchMask.or(e.resource);
			
		}else {
			fetchMask.clear(e.resource);
			
		}
		fetchMask.and(useMask);
		if (fetchMask.isClear()) {
			jobs.dontSearch();
		}else {
			jobs.searchAgainWithoutResources();
		}
		jobs.resetResourceSearch();
	}
	
	@Override
	protected void dispose() {
		
		int amI = 0;
		for (COORDINATE c : body()) {
			if (is(c)) {
				int a = amounts[amI];
				if (a > 0)
					SETT.THINGS().resources.create(c, RESOURCES.EDI().all().get(amI).resource, a);
				amI++;
				if (amI == amounts.length)
					return;
			}
		}
		
		for (ResG e : RESOURCES.EDI().all())
			tally(e, -amounts[amI], -amountIncoming[amI]);
		
		for (COORDINATE c : body()) {
			if (is(c)) {
				blueprintI().food.dispose(c.x(), c.y());
				blueprintI().job.dispose(c.x(), c.y());
			}
		}
		service.dispose(blueprintI().service);
		
	}
	
	@Override
	public ROOM_CANTEEN blueprintI() {
		return (ROOM_CANTEEN) blueprint();
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return null;
	}
	
	@Override
	public TILE_STORAGE storage(int tx, int ty) {
		return null;
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
	public JOB_MANAGER getWork() {
		return jobs;
	}

	@Override
	public long[] productionData() {
		return pdata;
	}
	
	@Override
	public Industry industry() {
		return blueprintI().industries().get(0);
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}

	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, 0.2 + 0.8 * blueprintI().constructor.tables.get(this));
	}

	@Override
	public int industryI() {
		// TODO Auto-generated method stub
		return 0;
	}

	@Override
	public RoomState makeState(int rx, int ry, boolean broken) {
		return new State(this);
	}
	
	private final static class State extends RoomState.RoomStateInstance {

		private final RBITImp useMask = new RBITImp();
		
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		public State(CanteenInstance ins) {
			super(ins);
			useMask.clear(ins.useMask);
		}
		
		@Override
		protected void applyIns(RoomInstance ins) {
			if (ins instanceof CanteenInstance) {
				CanteenInstance i = (CanteenInstance) ins;
				i.useMask.clear(useMask);
				for (ResG g : RESOURCES.EDI().all()) {
					i.setMask(g);
				}
			}
			super.applyIns(ins);
		}
		
	}


	
}