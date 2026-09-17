package settlement.room.infra.janitor;

import java.io.IOException;

import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.FileGetter;
import snake2d.util.sets.Bitsmap1D;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class JanitorInstance extends RoomInstance implements JOBMANAGER_HASER{

	private static final long serialVersionUID = 1L;
	boolean searchForJobs = true;
	long tableRes = 0;
	boolean auto = true;
	final short rx, ry;
	BITS bits = new BITS();
	
	
	
	protected JanitorInstance(ROOM_JANITOR b, TmpArea area, RoomInit init) {
		super(b, area, init);
		//resData = Alloc.ii(b.res.intsize);

		employees().maxSet((int) blueprintI().constructor.workers.get(this));
		employees().neededSet((int) Math.ceil(blueprintI().constructor.workers.get(this)/5.0));
		activate();
		int x = 0, y = 0;
		for (COORDINATE c : body()) {
			if (is(c) && SETT.ROOMS().fData.tile.is(c, b.constructor.ta)) {
				x = c.x();
				y = c.y();
			}
		}
		rx = (short) x;
		ry = (short) y;
		bits = new BITS();
	}
	
	@Override
	protected boolean loadExtra(FileGetter file) throws IOException {
		if (bits.fetchAms == null)
			bits.fetchAms = new Bitsmap1D(0, 5, RESOURCES.ALL().size());
		return super.loadExtra(file);
	}
	
	@Override
	protected void loadFix() {
		if (bits == null || !RESOURCES.map().loader().isSame()) {
			bits = new BITS();
		}
		
		
		
		if (bits.fetchAms == null)
			bits.fetchAms = new Bitsmap1D(0, 5, RESOURCES.ALL().size());
		
		super.loadFix();
	}
	
	@Override
	public boolean isBadMaintenanceTile(int tx, int ty) {
		return tx == rx && ty == ry;
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
		searchForJobs = true;
		bits.update();
	}
	
	@Override
	public JOB_MANAGER getWork() {
		return blueprintI().jm.get(this);
	}
	
	@Override
	protected void dispose() {
		for (RESOURCE res : RESOURCES.ALL()) {
			if (bits.resAm(res) > 0) {
				SETT.THINGS().resources.create(rx, ry, res, bits.resAm(res));
			}
		}
	}

	@Override
	public ROOM_JANITOR blueprintI() {
		return (ROOM_JANITOR) blueprint();
	}

	

}
