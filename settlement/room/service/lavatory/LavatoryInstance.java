package settlement.room.service.lavatory;

import static settlement.main.SETT.ROOMS;

import game.GAME;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.sets.ArrayCooShort;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

public final class LavatoryInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_SERVICER{

	private static final long serialVersionUID = 1L;
	final Jobs jobs;
	
	private final ArrayCooShort extras;
	private int extraI;
	final RoomServiceInstance service;
	boolean auto = true;

	protected LavatoryInstance(ROOM_LAVATORY blueprint, TmpArea area, RoomInit init) {
		super(blueprint, area, init);
		jobs = new Jobs(this);
		
		service = new RoomServiceInstance(jobs.size(), blueprintI().data);
		
		employees().maxSet(jobs.size());
		employees().neededSet((int)Math.ceil(blueprint.constructor.workers.get(this)));
		
		int e = 0;
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			Lavatory ll = Lavatory.get(c.x(), c.y());
			if (ll != null)
				ll.init(service);
			FurnisherItemTile it = ROOMS().fData.tile.get(c); 
			if (it != null) {
				int d = it.data();
				if ((d & Lavatory.BIT_WASH) == Lavatory.BIT_WASH)
					e++;;
			}
		}
		extras = new ArrayCooShort(e);
		e = 0;
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			
			FurnisherItemTile it = ROOMS().fData.tile.get(c); 
			if (it != null) {
				int d = it.data();
				if ((d & Lavatory.BIT_WASH) == Lavatory.BIT_WASH)
					extras.set(e++).set(c);
			}
			
			
		}
		activate();
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i) {
		i.lit();
		return super.render(r, shadowBatch, i);
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		if (day)
			service.updateDay();
		jobs.searchAgain();
	}

	@Override
	protected void activateAction() {
		
	}

	@Override
	protected void deactivateAction() {
		
	}
	
	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}
	
	@Override
	protected void dispose() {
		service.dispose(blueprintI().data);
		for (int i = 0; i < jobs.size(); i++) {
			COORDINATE c = jobs.get(i);
			Lavatory.get(c.x(), c.y()).dispose();
		}
	}
	
	public COORDINATE getExtra() {
		if (extraI == extras.size())
			return null;
		return extras.set(extraI++);
	}
	
	public void returnExtra(int tx, int ty) {
		if (!is(tx, ty))
			return;
		int data = ROOMS().data.get(tx, ty);
		if ((data & Lavatory.BIT_WASH) != Lavatory.BIT_WASH)
			return;
		if (extraI == 0) {
			GAME.Notify("WEIRDNESS!");
		}else {
			extraI--;
			extras.set(extraI).set(tx,ty);
		}
		
		
	}

	@Override
	public ROOM_LAVATORY blueprintI() {
		return (ROOM_LAVATORY) blueprint();
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}

	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, 0.5+0.5*blueprintI().constructor.basins.get(this));
	}
	
	static class Jobs extends JobPositions<LavatoryInstance> {

		private static final long serialVersionUID = 1L;

		public Jobs(LavatoryInstance ins) {
			super(ins);
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			Lavatory t = Lavatory.get(tx, ty);
			if (t == null)
				return null;
			return t.job;
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			FurnisherItemTile it = ROOMS().fData.tile.get(tx, ty); 
			if (it != null) {
				int d = it.data();
				ROOMS().data.set(ins, tx, ty, d);
				if (d == Lavatory.BIT) {
					
					return true;
				}
				
			}
			return false;	
			
		}
	}

}
