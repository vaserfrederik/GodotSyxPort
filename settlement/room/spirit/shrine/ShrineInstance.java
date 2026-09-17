package settlement.room.spirit.shrine;

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

final class ShrineInstance extends RoomInstance implements ROOM_SERVICER {

	private static final long serialVersionUID = 1L;
	final RoomServiceInstance service;
	
	short used;
	
	protected ShrineInstance(ROOM_SHRINE b, TmpArea area, RoomInit init) {
		super(b, area, init);
		
		int am = 0;
		for (COORDINATE c : body()) {
			if (is(c) && b.bed(c.x(), c.y()) != null){
				am++;
			}
		}
		service = new RoomServiceInstance(am, blueprintI().data);
		for (COORDINATE c : body()) {
			if (is(c) && SETT.ROOMS().fData.tileData.is(c, Constructor.codeFire)){
				SETT.LIGHTS().fire(c.x(), c.y(), 0);
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
				Service t = blueprintI().bed.get(c.x(), c.y());
				if (t != null)
					t.dispose();
			}
		}
		service.dispose(blueprintI().data);
		
	}
	
	@Override
	public ROOM_SHRINE blueprintI() {
		return (ROOM_SHRINE) blueprint();
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}

	@Override
	public double quality() {
		double base = (upgrade()+1.0)/(blueprintI().upgrades().max()+1.0);
		return ROOM_SERVICER.defQuality(this, base);
	}
	

}
