package settlement.room.home.chamber;

import java.io.IOException;

import init.constant.C;
import settlement.path.finders.SFinderFindable;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_CHAMBER extends RoomBlueprintIns<ChamberInstance> {

	
	final Constructor constructor;
	final Work work;
	
	public ROOM_CHAMBER(RoomInitData init, RoomCategorySub block) throws IOException {
		super(0, init, "_HOME_CHAMBER", block);
		work = new Work(this);
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
		
	}
	
	public int getSleepPixelX(int tx, int ty) {
		DIR d = DIR.ALL.get(get(tx, ty).sleepDir).next(-1);
		int x = tx*C.TILE_SIZE + C.TILE_SIZEH;
		x += d.x()*C.TILE_SIZEH;
		return x;
	}
	
	public int getSleepPixelY(int tx, int ty) {
		DIR d = DIR.ALL.get(get(tx, ty).sleepDir).next(-1);
		int y = ty*C.TILE_SIZE + C.TILE_SIZEH;
		y += d.y()*C.TILE_SIZEH;
		return y;
	}
	
	public DIR getSleepDir(int tx, int ty) {
		DIR d = DIR.ALL.get(get(tx, ty).sleepDir).perpendicular();
		return d;
	}

	@Override
	public SFinderFindable service(int tx, int ty) {
		// TODO Auto-generated method stub
		return null;
	}
	
	@Override
	public boolean degrades() {
		return false;
	}

}
