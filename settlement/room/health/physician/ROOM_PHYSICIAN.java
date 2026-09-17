package settlement.room.health.physician;

import java.io.IOException;

import settlement.main.SETT;
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
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_PHYSICIAN extends RoomBlueprintIns<Instance> implements ROOM_SERVICE_NEED_HASER, ROOM_EMPLOY_AUTO{

	final Constructor constructor;
	final RoomServiceNeed data;
	final Service s;
	
	public ROOM_PHYSICIAN(String key, int typeI, RoomInitData init, RoomCategorySub block) throws IOException {
		super(typeI, init, key, block);
		s = new Service(this);
		data = new RoomServiceNeed(this, init) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return s.getS(tx, ty);
			}
		};
		
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
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
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

	public DIR getLayDir(int sx, int sy) {
		return DIR.ORTHO.getC(SETT.ROOMS().fData.spriteData.get(sx, sy)&0b11);
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
	

}
