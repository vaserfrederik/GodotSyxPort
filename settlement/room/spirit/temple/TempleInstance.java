package settlement.room.spirit.temple;

import game.time.TIME;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.sets.ArrayCooShort;

public class TempleInstance extends RoomInstance implements ROOM_SERVICER{

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	final RoomServiceInstance service;
	final ArrayCooShort jobs;
	int consumed = 0;
	byte year = (byte)(TIME.years().bitsSinceStart());
	short sacrificesTotal;
	short sacrifices;
	short sacrificesRequired = 0;
	boolean resHas = true;
	final int altars;
		
	protected TempleInstance(ROOM_TEMPLE blueprint, TmpArea area, RoomInit init) {
		super(blueprint, area, init);
		
		int s = 0;
		int j = 0;
		int a = 0;
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			if (blueprint.serviceTile.get(c.x(), c.y()) != null)
				s++;
			if (blueprint.job.get(c.x(), c.y()) != null)
				j++;
			if (blueprint.altar.get(c.x(), c.y()) != null) {
				a++;
			}
			
		}
		altars = a;
		jobs = new ArrayCooShort(j);
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			if (blueprint.job.get(c.x(), c.y()) != null) {
				jobs.get().set(c);
				jobs.inc();
			}
		}
		
		service = new RoomServiceInstance(s, blueprint.service);
		
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			blueprint.serviceTile.init(c.x(), c.y());
				
		}
		
		employees().maxSet(jobs.size());
		employees().neededSet(jobs.size());
		
		activate();
		
	}

	@Override
	public ROOM_TEMPLE blueprintI() {
		return (ROOM_TEMPLE) blueprint();
	}

	@Override
	protected void activateAction() {
		
		
	}

	@Override
	protected void deactivateAction() {
		// TODO Auto-generated method stub
		
	}
	
	@Override
	protected void updateAction(double updateInterval, boolean day) {
		resHas = true;
		if (day) {
			if ((byte)(TIME.years().bitsSinceStart()) != year) {
				consumed = 0;
				year = (byte)(TIME.years().bitsSinceStart());
			}
			service.updateDay();
			
			
			sacrifices = (short) Math.ceil(sacrifices/2.0);
			sacrificesTotal = (short) Math.ceil(sacrificesTotal/2.0);
		}
	}
	
	public double sacrificeValue() {
		if (sacrificesTotal == 0)
			return 0;
		return (double) sacrifices/sacrificesTotal;
	}
	
	public double respect() {
		double d = 0.25;
		d += 0.25*blueprintI().constructor.grandure.get(this);
		d += 0.25*blueprintI().constructor.space.get(this);
		d += 0.25*blueprintI().constructor.decor.get(this);
		d*= (double)employees().employed()/employees().target();
		d*= sacrificeValue();
		return d;
		
	}
	
	public int sacrifices() {
		return (int) (jobs.size()*blueprintI().STIME*0.5);
	}
	
	@Override
	public void updateTileDay(int tx, int ty) {
		blueprintI().altar.updateday(tx, ty);
	}

	@Override
	protected void dispose() {
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			blueprintI().serviceTile.dispose(c.x(), c.y());
			blueprintI().altar.dispose(c.x(), c.y());
				
		}
		service.dispose(blueprintI().service);
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}

	@Override
	public double quality() {
		double base = (upgrade()+1.0)/(blueprintI().upgrades().max()+1.0);
		return ROOM_SERVICER.defQuality(this, base*respect());
	}
	
	public TempleJob jobReservable(int tx, int ty) {
		if (is(tx, ty) && blueprintI().job.get(tx, ty) != null && !blueprintI().job.jobReservedIs())
			return blueprintI().job;
		for (int i = 0; i < jobs.size(); i++) {
			TempleJob j = blueprintI().job.get(jobs.get().x(), jobs.get().y());
			jobs.inc();
			if (!j.jobReservedIs())
				return j;
		}
		return null;
	}
	
	public TempleJob job(int tx, int ty) {
		if (is(tx, ty) && blueprintI().job.get(tx, ty) != null)
			return blueprintI().job;
		return null;
	}
	
	public void reportMissing() {
		resHas = false;
	}
	
}
