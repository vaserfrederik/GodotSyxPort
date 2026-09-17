package settlement.room.spirit.dump;

import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.sprite.text.Str;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import view.main.VIEW;

final class DumpInstance extends RoomInstance implements ROOM_SERVICER{

	private final RoomServiceInstance service;
	
	private static final long serialVersionUID = 1L;

	protected DumpInstance(ROOM_DUMP blueprint, TmpArea area, RoomInit init) {
		super(blueprint, area, init);
		int am = 0;
		for (COORDINATE c : body()) {
			if (is(c)) {
				if (!blueprintI().constructor.isEdge(c.x(), c.y(), this)) {
					Dump.init(this, c.x(), c.y());
					am++;
				}
			}
		}
		service = new RoomServiceInstance(am, blueprint.service());
		activate();

	}
	
	@Override
	public ROOM_DUMP blueprintI() {
		return (ROOM_DUMP) blueprint();
	}

	@Override
	protected void activateAction() {
		for (COORDINATE c : body()) {
			if (is(c)) {
				Dump.activate(c.x(), c.y());
			}
		}
		service.clearLoad();
		
	}

	@Override
	protected void deactivateAction() {
		for (COORDINATE c : body()) {
			if (is(c)) {
				Dump.deactivate(c.x(), c.y());
			}
		}
		
	}

	@Override
	protected void dispose() {
		service.dispose(blueprintI().service);		
	}
	
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		Dump.render(r, shadowBatch, i);
		return false;
	}
	
	@Override
	protected boolean renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		blueprintI().constructor.renderTileBelow(r, shadowBatch, i, true);
		return false;
	}
	
	@Override
	protected void updateAction(double updateInterval, boolean day) {
		if (day)
			service.updateDay();
	}
	
	@Override
	public void updateTileDay(int tx, int ty) {
		Dump d = Dump.get(tx, ty);
		if (d != null)
			d.update();
	}

	@Override
	public RoomServiceInstance service() {
		return service;
	}

	@Override
	public double quality() {
		return ROOM_SERVICER.defQuality(this, 1);
	}
	
	@Override
	protected boolean canRemoveAndRemoveAction(int tx, int ty, boolean scatter, Object obj, boolean force) {
		if (force || !prompt())
			return true;
		return false;
	}
	
	private boolean prompt() {
		int time = 0;
		int am = 0;
		for (COORDINATE c : body()) {
			if (is(c)) {
				int t = Dump.daysTillDecompose(c.x(), c.y());
				if (t > 0) {
					am++;
					if (t > time)
						time = t;
				}
			}
		}
		
		if (am > 0) {
			Str.TMP.clear();
			Str.TMP.add(ROOM_DUMP.¤¤RemoveProblem);
			Str.TMP.insert(0, am);
			Str.TMP.insert(1, time);
			VIEW.inters().yesNo.activate(Str.TMP, null, null, false);
			return true;
		}
		return false;
	}

}
