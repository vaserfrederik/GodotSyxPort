package settlement.room.spirit.shrine;

import java.io.IOException;

import init.religion.RELIGIONS;
import init.religion.Religion;
import init.type.NEEDS;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomService;
import settlement.room.service.module.RoomService.ROOM_SERVICE_HASER;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;

public final class ROOM_SHRINE extends RoomBlueprintIns<ShrineInstance> implements ROOM_SERVICE_HASER {

	public final Religion religion;
	final RoomService data; 
	
	final Constructor constructor;
	final Service bed;
	
	public ROOM_SHRINE(String key, int index, RoomInitData init, RoomCategorySub block) throws IOException {
		super(index, init, key, block);
		
		religion = RELIGIONS.MAP().read(init.data());
		bed = new Service(this);
		data = new RoomService(this, init, NEEDS.TYPES().SHRINE) {
			
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
	
	public Service bed(int tx, int ty) {
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
	public RoomService service() {
		return data;
	}

}
