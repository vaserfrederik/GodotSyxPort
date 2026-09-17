package settlement.room.infra.importt;

import java.io.IOException;

import init.resources.RESOURCE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_IMPORT extends RoomBlueprintIns<ImportInstance>{

	public final ImportTally tally = new ImportTally();
	final Crate crate = new Crate(this);
	
	final Constructor constructor;
	
	public ROOM_IMPORT(RoomInitData init, RoomCategorySub cat) throws IOException {
		super(0, init, "_IMPORT", cat);
		
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
		tally.saver.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		tally.saver.load(saveFile);
		for (int i = 0; i < instancesSize(); i++) {
			tally.count(getInstance(i).resource(), getInstance(i).amount(), getInstance(i).capacity());
		}
	}
	
	@Override
	protected void clearP() {
		tally.saver.clear();
	}
	
	public final ImportThingy UNLOADER = new ImportThingy(this, tally);
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}
	
	public int getBestPrice(RESOURCE res) {
		return 0;
	}
	
	
}
