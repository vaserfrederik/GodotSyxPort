package settlement.room.infra.monument;

import java.io.IOException;

import settlement.room.main.category.RoomCategories;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.util.RoomInitData;
import settlement.room.main.util.RoomsCreator;
import snake2d.util.sets.LIST;

public final class ROOM_MONUMENTS {

	public final LIST<ROOM_MONUMENT> all;
	
	public ROOM_MONUMENTS(RoomInitData init, RoomCategories CATS) throws IOException{
		

		
		this.all = new RoomsCreator<ROOM_MONUMENT>(init, "MONUMENT",
				CATS.DECOR) {
			
			@Override
			public ROOM_MONUMENT create(String key, RoomInitData data, RoomCategorySub cat, int index)
					throws IOException {
				init.init(key);
				if (data.data().has("TYPE")) {
					
					if (data.data().value("TYPE").equals("TORCH"))
						return new Torch(data, index, key, cat);
				}
				
				return new Imp(data, index, key, cat);
			}
		}.all();
	}
	
}
