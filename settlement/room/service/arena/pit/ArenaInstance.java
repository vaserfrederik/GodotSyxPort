package settlement.room.service.arena.pit;

import game.time.TIME;
import settlement.main.SETT;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.rnd.RND;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class ArenaInstance extends RoomInstance implements ROOM_SERVICER{

	private static final long serialVersionUID = 1L;
	final RoomServiceInstance service;
	final byte off = (byte) RND.rInt(64);
	
	short gladiators = 0;
	public final byte ax,ay;
	
	int cheerTime;
	boolean cheer;
	
	static final int CHEER_TIME = TIME.secondsPerDay()/128;
	
	
	protected ArenaInstance(ROOM_FIGHTPIT b, TmpArea area, RoomInit init) {
		super(b, area, init);

		int ss = 0;
		for (COORDINATE c : body()) {
			if (is(c) && b.ser.init(c.x(), c.y())) {
				ss++;
			}
		}
		
		int ax = 0,ay = 0;
		outer:
		for (int y = 0; y < body().height(); y++) {
			for (int x = 0; x < body().width(); x++) {
				if (SETT.ROOMS().fData.tileData.get(body().x1()+x, body().y1()+y) == ArenaConstructor.ARENA) {
					ax = x;
					ay = y;
					break outer;
				}
			}
		}
		this.ax = (byte) ax;
		this.ay = (byte) ay;
		
		service = new RoomServiceInstance(ss, blueprintI().data);
		int w = (short) b.constructor.workers.get(this);
		employees().maxSet(w);
		employees().neededSet(w);
		activate();
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}
	
	@Override
	protected void activateAction() {
		cheerTime = (int) (TIME.currentSecond()) - CHEER_TIME;
		blueprintI().incG(gladiators,  ROOM_FIGHTPIT.EXECUTIONS);
		for (COORDINATE c : body()) {
			if (is(c) && blueprintI().ser.get(c.x(), c.y())!= null) {
				 blueprintI().ser.findableReserveCancel();
			}
		}
	}

	@Override
	protected void deactivateAction() {
		blueprintI().incG(-gladiators,  -ROOM_FIGHTPIT.EXECUTIONS);
		for (COORDINATE c : body()) {
			if (is(c) && blueprintI().ser.get(c.x(), c.y())!= null && blueprintI().ser.findableReservedCanBe()) {
				 blueprintI().ser.findableReserve();
			}
		}
	}


	@Override
	protected void updateAction(double updateInterval, boolean day) {
		if (day) {
			service.updateDay();
		}
	}
	
	@Override
	protected void dispose() {
		
	}

	@Override
	public ROOM_FIGHTPIT blueprintI() {
		return (ROOM_FIGHTPIT) blueprint();
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}

	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, (double)employees().employed()/employees().max());
	}
	
	public int gladiatorsNeeded() {
		if (active())
			return ROOM_FIGHTPIT.EXECUTIONS - gladiators;
		return 0;
	}
	
	public void reserveGladiator(int delta) {
		gladiators += delta;
		if (active()) {
			blueprintI().incG(delta, 0);
		}
		
	}

}
