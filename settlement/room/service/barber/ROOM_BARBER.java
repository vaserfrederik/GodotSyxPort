package settlement.room.service.barber;

import java.io.IOException;

import game.time.TIME;
import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomServiceNeed;
import settlement.room.service.module.RoomServiceNeed.ROOM_SERVICE_NEED_HASER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;

public final class ROOM_BARBER extends RoomBlueprintIns<Instance> implements ROOM_SERVICE_NEED_HASER, ROOM_EMPLOY_AUTO{

	public static final String TYPE = "BARBER";
	final RoomServiceNeed data;
	
	final Constructor constructor;
	final Tile ll;
	
	public ROOM_BARBER(RoomInitData init, int typeIndex, String key, RoomCategorySub block) throws IOException {
		super(typeIndex, init, key, block);
		data = new RoomServiceNeed(this, init) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return ll.service(tx, ty);
			}
			
		};
		constructor = new Constructor(this, init);
		ll = new Tile(this, (int) (TIME.workSeconds()*init.data().d("WORK_TIME_IN_DAYS")));
	}

	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return data.finder;
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
		return ((Instance)r).auto;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((Instance)r).auto = b;
	}
	
	public DIR dir(int tx, int ty) {
		FurnisherItem it = SETT.ROOMS().fData.item.get(tx, ty);
		if (it == null)
			return DIR.N;
		return DIR.W.next(2*it.rotation);
	}
	
}
