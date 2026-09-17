package settlement.room.service.pleasure;

import java.io.IOException;

import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderFindable;
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
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_PLEASURE extends RoomBlueprintIns<PleasureInstance> implements ROOM_EMPLOY_AUTO, ROOM_SERVICE_NEED_HASER{

	public static final String TYPE = "PLEASURE";
	final Constructor constructor;
	final ABed bed;
	final RoomServiceNeed service;
	
	public ROOM_PLEASURE(String key, int tindex, RoomInitData init, RoomCategorySub block) throws IOException {
		super(tindex, init, key, block);
		bed = new ABed(this);
		constructor = new Constructor(this, init);
		service = new RoomServiceNeed(this, init) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				if (bed.init(tx, ty) != null)
					return bed.service;
				return null;
			}
		};
	}
	
	@Override
	protected void update(double ds) {
		
		
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		service.saver.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		service.saver.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		service.saver.clear();
	}
	
	@Override
	public boolean autoEmploy(Room r) {
		return ((PleasureInstance)r).auto;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((PleasureInstance)r).auto = b;
	}

	@Override
	public SFinderFindable service(int tx, int ty) {
		if (bed.init(tx, ty) != null)
			return service.finder;
		return null;
	}

	@Override
	public RoomServiceNeed service() {
		return service;
	}
	
	public boolean clientShouldUndress(int tx, int ty) {
		if (bed.init(tx, ty) != null) {
			return bed.clientShouldUndress();
		}
		return false;
	}
	
	public void clientUndress(int tx, int ty) {
		if (bed.init(tx, ty) != null) {
			bed.clientUndress();
		}
	}
	
	public boolean workerReadyShouldUndress(int tx, int ty) {
		if (bed.init(tx, ty) != null) {
			return bed.workerReadyShouldUndress();
		}
		return false;
	}

}
