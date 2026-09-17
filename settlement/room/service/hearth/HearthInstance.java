package settlement.room.service.hearth;

import settlement.main.SETT;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class HearthInstance extends RoomInstance implements ROOM_SERVICER {

	private static final long serialVersionUID = 1L;
	final RoomServiceInstance service;
	
	short used;
	
	protected HearthInstance(ROOM_HEARTH b, TmpArea area, RoomInit init) {
		super(b, area, init);
		
		int am = 0;
		for (COORDINATE c : body()) {
			if (is(c) && SETT.ROOMS().fData.tileData.is(c, Constructor.codeService)){
				am++;
			}
		}
		service = new RoomServiceInstance(am, blueprintI().data);
		for (COORDINATE c : body()) {
			if (is(c) && SETT.ROOMS().fData.tileData.is(c, Constructor.codeFire)){
				SETT.LIGHTS().fire(c.x(), c.y(), 0);
				SETT.LIGHTS().hide(c.x(), c.y(), true);
			}
			if (blueprintI().bed.get(c.x(), c.y()) != null)
				blueprintI().bed.init();
		}
		activate();
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
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
		if (day)
			service.updateDay();
	}
	
	@Override
	protected void dispose() {
		for (COORDINATE c : body()) {
			if (is(c)) {
				SETT.JOBS().clearer.set(c);
				Hearth t = blueprintI().bed.get(c.x(), c.y());
				if (t != null)
					t.dispose();
			}
		}
		service.dispose(blueprintI().data);
		
	}
	
	@Override
	public ROOM_HEARTH blueprintI() {
		return (ROOM_HEARTH) blueprint();
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
