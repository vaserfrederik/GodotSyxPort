package settlement.room.industry.module;

import settlement.room.main.ROOMS;
import settlement.room.main.RoomBlueprint;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;

public class RoomIndustries {

	public final LIST<Industry> all;
	
	public RoomIndustries(ROOMS rooms){
		int am = 0;
		for (RoomBlueprint b : rooms.all()) {
			if (b instanceof INDUSTRY_HASER) {
				INDUSTRY_HASER h = (INDUSTRY_HASER) b;
				am += h.industries().size();
			}
				
		}
		
		ArrayList<Industry> hh = new ArrayList<>(am);
		for (RoomBlueprint b : rooms.all()) {
			if (b instanceof INDUSTRY_HASER) {
				INDUSTRY_HASER h = (INDUSTRY_HASER) b;
				for (Industry i : h.industries())
					hh.add(i);
			}
				
		}
		all = new ArrayList<>(hh);

	}
	
	

	
}
