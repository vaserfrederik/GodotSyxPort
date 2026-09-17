package settlement.room.law.court;

import java.io.IOException;

import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.law.PUNISHMENT_SERVICE;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomServiceNeed;
import settlement.room.service.module.RoomServiceNeed.ROOM_SERVICE_NEED_HASER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.rnd.RND;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_COURT extends RoomBlueprintIns<CourtInstance> implements ROOM_SERVICE_NEED_HASER, PUNISHMENT_SERVICE {

	public static final double freeRate = 0.2;
	
	final RoomServiceNeed data;
	
	final Constructor constructor;
	private int executions;
	private int total;
	
	public ROOM_COURT(RoomInitData init, RoomCategorySub block) throws IOException {
		super(0, init, "_COURT", block);
		
		data = new RoomServiceNeed(this, init) {
			@Override
			public FSERVICE service(int tx, int ty) {
				return Service.init(tx, ty);
			}

		};
		
		constructor = new Constructor(this, init);
		
	}
	
	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}
	

	@Override
	public Furnisher constructor() {
		return constructor;
	}

	@Override
	public int punishUsed() {
		return executions;
	}
	
	@Override
	public int punishTotal() {
		return total;
	}
	

	void incPrisoners(int current, int total){
		this.executions += current;
		this.total += total;
	}


	@Override
	protected void saveP(FilePutter f){
		f.i(executions);
		f.i(total);
	}
	
	@Override
	protected void loadP(FileGetter f) throws IOException{
		executions = f.i();
		total = f.i();
	}
	
	@Override
	protected void clearP() {
		executions = 0;
		total = 0;
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return data.finder;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		
	}

	public CourtStation exectuionReserve() {
		if (executions == total)
			return null;
		int i = RND.rInt(instancesSize());
		for (int k = 0; k < instancesSize(); k++) {
			CourtInstance ins = getInstance((k+i)%instancesSize());
			if (ins.active() && ins.executions() < ins.total()) {
				return ins.reserveSpot();
			}
		}
		throw new RuntimeException();
	}
	
	public CourtStation executionSpot(COORDINATE c) {
		if (is(c)) {
			return CourtStation.init(c.x(), c.y());
		}
		return null;
	}
	
	public CourtStation workReserve(Room r) {
		CourtInstance ins = (CourtInstance) r;
		return ins.work();
	}
	

	
	public boolean shouldCheer(int tx, int ty) {
		CourtInstance ins = getter.get(tx, ty);
		return ins != null && ins.executions() > 0;
	}
	
	@Override
	public RoomServiceNeed service() {
		return data;
	}
	

}
