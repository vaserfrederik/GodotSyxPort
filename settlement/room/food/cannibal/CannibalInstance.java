package settlement.room.food.cannibal;

import game.GAME;
import game.faction.FResources.RTYPE;
import game.time.TIME;
import init.resources.RESOURCE;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.RESOURCE_TILE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.Alloc;
import snake2d.util.sets.ArrayCooShort;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

public final class CannibalInstance extends RoomInstance {

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	private final ArrayCooShort coos;
	final ArrayCooShort cages;
	private int[] resources;
	private byte year = (byte) TIME.years().bitsSinceStart();
	short prisoners;
	short reservable;
	
	CannibalInstance(ROOM_CANNIBAL blue, TmpArea area, RoomInit init) {
		super(blue, area, init);
		
		resources = Alloc.ii(blue.resources().length);
		int am = 0;
		int ca = 0;
		for (COORDINATE c : body()) {
			if (is(c) && blue.job.init(c.x(), c.y(), this) != null) {
				am++;
			}
			if (is(c) && blue.cage(c.x(), c.y()) != null) {
				ca++;
			}
		}
		
		coos = new ArrayCooShort(am);
		cages = new ArrayCooShort(ca);
		am = 0;
		ca = 0;
		for (COORDINATE c : body()) {
			if (is(c) && blue.job.init(c.x(), c.y(), this) != null) {
				coos.set(am++).set(c);
			}
			if (is(c) && blue.cage(c.x(), c.y()) != null) {
				cages.get().set(c);
				cages.inc();
				ca++;
			}
		}
		cages.set(0);
		employees().maxSet(ca);
		employees().neededSet((int) Math.ceil(ca/2.0));
		activate();
		
	}
	
	@Override
	protected void loadFix() {
		if (resources.length != blueprintI().resources().length) {
			resources = Alloc.ii(blueprintI().resources().length);
		}
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
		byte y = (byte) TIME.years().bitsSinceStart();
		if (year != y) {
			year = y;
			for (int i = 0; i < resources.length; i++) {
				resources[i] = 0;
			}
		}
		
		
	}

	@Override
	protected void dispose() {
		
		
	}

	public SETT_JOB getWork() {
		
		for (int i = 0; i < coos.size(); i++) {
			coos.inc();
			SETT_JOB j = blueprintI().job.init(coos.get().x(), coos.get().y(), this);
			if (!j.jobReservedIs(null))
				return j;
		}
		
		for (int i = 0; i < coos.size(); i++) {
			coos.inc();
			SETT_JOB j = blueprintI().job.init(coos.get().x(), coos.get().y(), this);
			j.jobReserveCancel(null);
		}
		
		for (int i = 0; i < coos.size(); i++) {
			coos.inc();
			SETT_JOB j = blueprintI().job.init(coos.get().x(), coos.get().y(), this);
			if (!j.jobReservedIs(null))
				return j;
		}
		
		return null;
	}
	
	public void resetGore(COORDINATE c) {
		blueprintI().job.reset(this, c);
	}
	
	public void gore(COORDINATE c) {
		blueprintI().job.gore(this, c);
	}
	
	public SETT_JOB getWork(COORDINATE c) {
		return blueprintI().job.init(c.x(), c.y(), this);
	}
	
//	@Override
//	public double getworkEffort(SKILLSET s) {
//		return blueprintI().production.getWorkEffort(s, blueprintI().constructor.efficiency.get(this), getDegrade());
//	}
	
	@Override
	public ROOM_CANNIBAL blueprintI() {
		return (ROOM_CANNIBAL) blueprint();
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return null;
	}
	
	public int produce(RESOURCE res, int am) {
		
		int i = 0;
		for (RESOURCE r : blueprintI().resources()) {
			if (r == res) {
				resources[i] += am;
				break;
			}
			i++;
		}
		blueprintI().produced[res.index()] += am;
		GAME.player().res().inc(res, RTYPE.PRODUCED, am);
		return i;
	}

}
