package settlement.room.spirit.dump;

import java.io.IOException;

import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomService;
import settlement.room.service.module.RoomService.ROOM_SERVICE_HASER;
import settlement.thing.ThingsCorpses.Corpse;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import util.text.D;

public class ROOM_DUMP extends RoomBlueprintIns<DumpInstance> implements ROOM_SERVICE_HASER{

	final Constructor constructor;
	final RoomService service;
	public static CharSequence ¤¤RemoveProblem = "¤This resting place still holds the dead and can not be removed. Deactivate the room and allow the corpses to decompose peacefully. Current cadavers: {0}. Days until clear: {1}.";
	
	static {
		D.ts(ROOM_DUMP.class);
	}
	
	public ROOM_DUMP(RoomInitData data, RoomCategorySub cat) throws IOException {
		super(0, data, "_DUMP_CORPSE", cat);
		constructor = new Constructor(this, data);
		service = new RoomService(this, data, null) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return Dump.get(tx, ty);
			}
		};
	}

	@Override
	protected void saveP(FilePutter saveFile) {
		service.saver.save(saveFile);
		
	}

	@Override
	protected void loadP(FileGetter saveFile) throws IOException {
		service.saver.load(saveFile);
		
	}

	@Override
	protected void clearP() {
		service.saver.clear();
	}

	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return service.finder;
	}
	
	@Override
	public RoomService service() {
		return service;
	}

	@Override
	public Furnisher constructor() {
		return constructor;
	}


	
	public void burry(Corpse corpse, int tx, int ty) {
		Dump.get(tx, ty).burry(corpse);
	}

	
	
}
