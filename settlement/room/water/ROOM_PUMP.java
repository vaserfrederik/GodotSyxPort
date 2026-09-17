package settlement.room.water;

import java.io.IOException;

import settlement.main.SETT;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

final class ROOM_PUMP extends RoomBlueprintIns<PumpInstance> {

	final PumpJob job;
	final PumpConstructor constructor;

	public ROOM_PUMP(RoomInitData init,RoomCategorySub cat) throws IOException {
		super(0, init, "_WATERPUMP", cat);
		
		constructor = new PumpConstructor(this, init);
		job = new PumpJob(this);
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new PumpGui(this).make());
	}
	
	@Override
	protected void update(double ds) {
		SETT.ROOMS().WATER.updater.update(ds);
		
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		SETT.ROOMS().WATER.updater.saver.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		SETT.ROOMS().WATER.updater.saver.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		SETT.ROOMS().WATER.updater.saver.clear();
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	boolean isCanalConnection(int tx, int ty) {
		return is(tx, ty) && SETT.ROOMS().fData.tile.get(tx, ty) == constructor.ou;
	}
//	
//	double getOutput(int tx, int ty) {
//		return is(tx, ty) && SETT.ROOMS().fData.tile.get(tx, ty) == constructor.ou ? 1 : 0;
//	}

}
