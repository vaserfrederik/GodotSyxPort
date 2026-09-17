package settlement.room.infra.elderly;

import java.io.IOException;

import settlement.main.SETT;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.employment.RoomEmploymentSimple.EmployerSimple;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_RESTHOME extends RoomBlueprintIns<ResthomeInstance>{

	final ResthomeConstructor constructor;
	final Job job = new Job(this);
	public final EmployerSimple emp = new EmployerSimple(employment());
	
	public ROOM_RESTHOME(String key, int index, RoomInitData init, RoomCategorySub block) throws IOException {
		super(index, init, key, block);
		

		constructor = new ResthomeConstructor(this, init);
		clearP();
	}
	
	@Override
	protected void update(double ds) {

	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return null;
	}

	@Override
	protected void saveP(FilePutter f){

	}
	
	@Override
	protected void loadP(FileGetter f) throws IOException{

		
	}
	
	@Override
	protected void clearP() {

	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(constructor.quality.applier(this));
	}
	
	public DIR sitDir(COORDINATE c) {
		if (SETT.ROOMS().fData.tileData.get(c) == ResthomeConstructor.ICHAIR)
			return DIR.ORTHO.get(SETT.ROOMS().fData.spriteData.get(c) & 0b011);
		return null;
	}
	
	public boolean dance(COORDINATE c) {
		if (SETT.ROOMS().fData.tileData.get(c) == ResthomeConstructor.ISTAGE)
			return true;
		return false;
	}
	
	public boolean cards(COORDINATE c) {
		if (SETT.ROOMS().fData.tileData.get(c) == ResthomeConstructor.ITABLE)
			return true;
		return false;
	}

	public double quality(RoomInstance t) {
		return constructor.quality.get(t)*(1.0-t.getDegrade());
	}

	public double quality() {
		return getStat(constructor.quality.index())*(1.0-degradeAverage());
	}
	
}
