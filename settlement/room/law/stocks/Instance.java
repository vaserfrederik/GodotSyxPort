package settlement.room.law.stocks;

import settlement.room.law.stocks.Tile.STATE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class Instance extends RoomInstance implements ROOM_SERVICER{

	private static final long serialVersionUID = 1L;
	final RoomServiceInstance service;
	byte available;
	
	protected Instance(ROOM_STOCKS b, TmpArea area, RoomInit init) {
		super(b, area, init);
		
		service = new RoomServiceInstance((int)b.constructor.spectators.get(this), blueprintI().data);
		for (COORDINATE c : body()) {
			if (is(c)) {
				Tile t =  blueprintI().tile.get(c.x(), c.y());
				if (t != null ) {
					t.init();
				}
			}
		}
		activate();
		
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}

	@Override
	protected void activateAction() {
		for (COORDINATE c : body()) {
			if (is(c)) {
				Tile t =  blueprintI().tile.get(c.x(), c.y());
				if (t != null ) {
					t.stateSet(STATE.available);
				}
			}
		}
		
	}

	@Override
	protected void deactivateAction() {
		for (COORDINATE c : body()) {
			if (is(c)) {
				Tile t =  blueprintI().tile.get(c.x(), c.y());
				if (t != null ) {
					t.stateSet(STATE.none);
				}
			}
		}
	}

	
	@Override
	protected void updateAction(double updateInterval, boolean day) {
		if (day)
			service.updateDay();
	}

	@Override
	protected void dispose() {
		service.dispose(blueprintI().data);
	}

	@Override
	public ROOM_STOCKS blueprintI() {
		return (ROOM_STOCKS) blueprint();
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}

	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, 1);
	}
	
	

}