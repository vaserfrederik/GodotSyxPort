package settlement.room.main.util;


import java.io.IOException;

import init.paths.PATHS;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.category.RoomCategorySub;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LinkedList;

public abstract class RoomsCreator<T extends RoomBlueprint>{
	

	final String type;
	final RoomCategorySub cat;
	final RoomInitData data;
	
	public RoomsCreator(RoomInitData data, String type, RoomCategorySub cat) throws IOException{
		this.type = type;
		this.cat = cat;
		this.data = data;
	}
	
	public abstract T create(String key, RoomInitData data, RoomCategorySub cat, int index) throws IOException;

	public LIST<T> all()  throws IOException{
		data.setType(type);
		
		LinkedList<T> tmp = new LinkedList<>();
		for (String s : PATHS.INIT().getFolder("room").getFiles()) {
			if (s.startsWith(type) && s.length() > type.length() && s.charAt(type.length()) == '_')
				tmp.add(create(s, data, cat, tmp.size()));
			
		}
		
		return new ArrayList<T>(tmp);
	}


}
