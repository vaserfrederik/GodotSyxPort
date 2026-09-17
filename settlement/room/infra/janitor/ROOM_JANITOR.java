package settlement.room.infra.janitor;

import java.io.IOException;

import settlement.path.finders.SFinderRoomService;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.util.RoomInitData;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_JANITOR extends RoomBlueprintIns<JanitorInstance> implements ROOM_EMPLOY_AUTO{

	final JM jm = new JM(this);
	public static final int radius = 150;
	final Constructor constructor;
	
	public ROOM_JANITOR(RoomInitData init, RoomCategorySub block) throws IOException {
		super(0, init, "_JANITOR", block);
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
		return ((JanitorInstance) r).auto;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((JanitorInstance) r).auto = b;
		
	}
	

}
