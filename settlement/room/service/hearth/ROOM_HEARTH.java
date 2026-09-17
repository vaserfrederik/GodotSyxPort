package settlement.room.service.hearth;

import java.io.IOException;

import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomServiceNeed;
import settlement.room.service.module.RoomServiceNeed.ROOM_SERVICE_NEED_HASER;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;

public final class ROOM_HEARTH extends RoomBlueprintIns<HearthInstance> implements ROOM_SERVICE_NEED_HASER {

	final RoomServiceNeed data; 
	
	final Constructor constructor;
	final Hearth bed;
	
	public ROOM_HEARTH(String key, int index, RoomInitData init, RoomCategorySub block) throws IOException {
		super(index, init, key, block);
		bed = new Hearth(this);
		data = new RoomServiceNeed(this, init) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return bed.get(tx, ty);
			}
		};
		constructor = new Constructor(this, init);
	}
	
	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}
	
	public Hearth bed(int tx, int ty) {
		return bed.get(tx, ty);
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
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

}
