package settlement.room.service.hygine.well;

import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class WellInstance extends RoomInstance implements ROOM_SERVICER {

	private static final long serialVersionUID = 1L;
	final RoomServiceInstance service;
	
	protected WellInstance(ROOM_WELL b, TmpArea area, RoomInit init) {
		super(b, area, init);
		
		
		int am = 0;
		for (COORDINATE c : body())
			if (is(c) && b.bed.get(c.x(), c.y()) != null)
				am ++;
		service = new RoomServiceInstance(am, blueprintI().data);
		for (COORDINATE c : body())
			if (is(c))
				b.bed.init(c.x(), c.y());
		
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
				Wash t = blueprintI().bed.get(c.x(), c.y());
				if (t != null)
					t.dispose();
			}
		}
		service.dispose(blueprintI().data);
		
	}
	
	@Override
	public ROOM_WELL blueprintI() {
		return (ROOM_WELL) blueprint();
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
