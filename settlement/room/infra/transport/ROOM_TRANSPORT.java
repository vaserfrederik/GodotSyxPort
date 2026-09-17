package settlement.room.infra.transport;

import java.io.IOException;

import init.resources.RESOURCE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.job.ROOM_RADIUS.ROOM_RADIUSE;
import settlement.room.main.util.RoomInitData;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_TRANSPORT extends RoomBlueprintIns<TransportInstance> implements ROOM_RADIUSE, ROOM_EMPLOY_AUTO {

	public final static int MAX_LOAD = 400;
	public final static int MAX_EMPLOYEES = 16;
	final Constructor constructor;
	final Job job = new Job(this);
	
	public ROOM_TRANSPORT(RoomInitData init, RoomCategorySub cat) throws IOException {
		super(0, init, "_TRANSPORT", cat);
		constructor = new Constructor(this, init);
		
	}

	@Override
	protected void update(double ds) {
		
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return null;
	}
	
	@Override
	protected void saveP(FilePutter saveFile){
		
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		
	}
	
	@Override
	protected void clearP() {
		
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}

	@Override
	public boolean autoEmploy(Room r) {
		return ((TransportInstance)r).auto;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((TransportInstance)r).auto = b; 
	}
	
	@Override
	public ROOM_RADIUS_INSTANCE radiusInstance(Room t) {
		return (TransportInstance) t;
	}

	public boolean hasActive(RESOURCE res) {
		return true;
	}

	public void endDelivery(short startTx, short startTy, RESOURCE res, int amount, int distance) {
		TransportInstance ins = get(startTx, startTy);
		if (ins != null) {
			ins.finishDeliveryJob(amount);
			ins.reportMoved(distance);
		}
		
		
	}
	

}
