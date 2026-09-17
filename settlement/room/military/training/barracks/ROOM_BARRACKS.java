package settlement.room.military.training.barracks;

import java.io.IOException;

import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.military.training.ROOM_M_TRAINER;
import snake2d.util.datatypes.COORDINATE;

public final class ROOM_BARRACKS extends ROOM_M_TRAINER<BarracksInstance> {
	
	final Constructor constructor;
	final BarracksThing thing = new BarracksThing(this);

	public ROOM_BARRACKS(int typeIndex, RoomInitData data, String key) throws IOException {
		super(typeIndex, data, key);
		
		constructor = new Constructor(this, data);
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	public COORDINATE faceCoo(int tx, int ty) {
		return thing.init(tx, ty).cooMan;
	}

}
