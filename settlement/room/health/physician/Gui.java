package settlement.room.health.physician;

import settlement.room.main.RoomInstance;
import snake2d.util.gui.GuiSection;
import snake2d.util.sets.LISTE;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GGrid;
import util.gui.table.GTableSorter.GTFilter;
import util.gui.table.GTableSorter.GTSort;
import view.sett.ui.room.UIRoomBulkApplier;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<Instance, ROOM_PHYSICIAN> {

	Gui(ROOM_PHYSICIAN s) {

		super(s);


	}
	
	@Override
	public void hover(GBox box, Instance i) {
		super.hover(box, i);
		
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<Instance> getter, int x1, int y1) {
		

		
	}
	
	@Override
	protected void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts,
			LISTE<UIRoomBulkApplier> appliers) {
		
	}


}
