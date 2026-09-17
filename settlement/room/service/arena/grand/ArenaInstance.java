package settlement.room.service.arena.grand;

import static settlement.main.SETT.ROOMS;

import game.GAME;
import game.time.TIME;
import settlement.path.AVAILABILITY;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.RECTANGLE;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class ArenaInstance extends RoomInstance implements ROOM_SERVICER{

	private static final long serialVersionUID = 1L;

	static final int CHEER_TIME = TIME.secondsPerDay()/128;
	final RoomServiceInstance service;
	final RECTANGLE arena;
	byte executions = 0;
	int cheerTime;
	boolean cheer;
	
	protected ArenaInstance(ROOM_ARENA b, TmpArea area, RoomInit init, RECTANGLE aa) {
		super(b, area, init);
		arena = aa;
		
		int ww = 0;
		int ss = 0;
		
		GAME.Notify("here");
		
		
		for (COORDINATE c : body()) {
			if (is(c)) {
				
				if (b.constructor.util.tile(c.x(), c.y()) == b.constructor.util.iArena)
					ww++;
				if (b.constructor.util.service(c.x(), c.y()))
					ss++;
				
			}
			
		}
		service = new RoomServiceInstance(ss, blueprintI().data);
		employees().maxSet(ww/6);
		employees().neededSet(ww/6);
		
		activate();
	}
	
	
	@Override
	protected void activateAction() {
		blueprintI().executions -= executions;
		blueprintI().executionsMax += 4;
		for (COORDINATE c : body()) {
			if (is(c) && blueprintI().ser.get(c.x(), c.y())!= null) {
				blueprintI().ser.findableReserveCancel();
			}
		}
	}

	@Override
	protected void deactivateAction() {
		blueprintI().executions += executions;
		blueprintI().executionsMax -= 4;
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
	public RoomServiceInstance service() {
		return service;
	}

	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, (double)employees().employed()/employees().max());
	}
	
	@Override
	protected void dispose() {
		
	}

	@Override
	public ROOM_ARENA blueprintI() {
		return (ROOM_ARENA) blueprint();
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		i.lit();
		FurnisherItemTile s = blueprintI().constructor.util.tile(i.tile());
		return s.sprite.render(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade(), false);
	}
	
	@Override
	protected boolean renderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		FurnisherItemTile s = blueprintI().constructor.util.tile(i.tile());
		s.sprite.renderAbove(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade());
		return false;
	}
	
	@Override
	protected boolean renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		FurnisherItemTile s = blueprintI().constructor.util.tile(i.tile());
		s.sprite.renderBelow(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade());
		return false;
	}
	
	@Override
	protected AVAILABILITY getAvailability(int tile) {
		FurnisherItemTile it = blueprintI().constructor.util.tile(tile);
		if (it == null)
			return AVAILABILITY.ROOM;
		return it.availability;
	}
	
	

}
