package settlement.room.service.speaker;

import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceInstance;
import snake2d.Renderer;
import snake2d.util.misc.CLAMP;
import snake2d.util.rnd.RND;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class SpeakerInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_SERVICER{

	private static final long serialVersionUID = 1L;
	final RoomServiceInstance service;
	final byte off = (byte) RND.rInt(64);
	byte workers = 0;
	private short services = 0;
	
	protected SpeakerInstance(ROOM_SPEAKER b, TmpArea area, RoomInit init) {
		super(b, area, init);

		service = new RoomServiceInstance((int)b.constructor.spectators.get(this), blueprintI().data);

		employees().maxSet(1);
		employees().neededSet(1);
		activate();
		
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}

	@Override
	protected void activateAction() {
		
		if (workers > 0) {
			setServices(service.total());
		}
		
	}

	@Override
	protected void deactivateAction() {
		setServices(0);
		workers = 0;
	}

	void incServices(int s) {
		if (workers > 0)
			setServices(services + s);
	}
	
	boolean hasService() {
		return workers > 0;
	}
	
	private void setServices(int s) {
		service.report(blueprintI().work.service(body().cX(), body().cY()), blueprintI().data, -services, false);
		this.services = (short) CLAMP.i(s, 0, service.total());
		service.report(blueprintI().work.service(body().cX(), body().cY()), blueprintI().data, services, true);
	}
	
	int services() {
		return services;
	}
	
	@Override
	protected void updateAction(double updateInterval, boolean day) {
		if (active()) {
			if (employees().employed() == 0) {
				if (workers > 0) {
					workers --;
					if (workers == 0) {
						setServices(0);
					}
				}
			}else {
				if (workers < 10) {
					workers = 10;
					setServices(service.total());
				}
			}
		}
		if (day)
			service.updateDay();
	}
	
	@Override
	public JOB_MANAGER getWork() {
		return blueprintI().work.manager(this);
	}
	
	@Override
	protected void dispose() {
		service.dispose(blueprintI().data);
	}

	@Override
	public ROOM_SPEAKER blueprintI() {
		return (ROOM_SPEAKER) blueprint();
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
