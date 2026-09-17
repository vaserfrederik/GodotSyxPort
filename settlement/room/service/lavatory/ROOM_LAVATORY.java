package settlement.room.service.lavatory;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomServiceNeed;
import settlement.room.service.module.RoomServiceNeed.ROOM_SERVICE_NEED_HASER;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;

public final class ROOM_LAVATORY extends RoomBlueprintIns<LavatoryInstance> implements ROOM_SERVICE_NEED_HASER, ROOM_EMPLOY_AUTO{

	final RoomServiceNeed data;
	
	final Constructor constructor;

	public ROOM_LAVATORY(RoomInitData init, int typeIndex, String key, RoomCategorySub block) throws IOException {
		super(typeIndex, init, key, block);
		data = new RoomServiceNeed(this, init) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return Lavatory.get(tx, ty);
			}
			
		};
		constructor = new Constructor(this, init);
	}

	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}
	
	public Lavatory getService(int tx, int ty) {
		return Lavatory.get(tx, ty);
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return data.finder;
	}
	
	public boolean isExtra(int tx, int ty) {
		if (is(tx, ty)) {
			int d = ROOMS().data.get(tx, ty);
			return (d & Lavatory.BIT_WASH) == Lavatory.BIT_WASH;
		}
		return false;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		data.saver.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		data.saver.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		data.saver.clear();
	}

	@Override
	public RoomServiceNeed service() {
		return data;
	}
	
	@Override
	public boolean autoEmploy(Room r) {
		return ((LavatoryInstance)r).auto;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((LavatoryInstance)r).auto = b;
	}
	
}
