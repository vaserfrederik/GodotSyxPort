package settlement.room.service.nursery;

import settlement.room.industry.module.IndustryUtil;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GuiSection;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GGrid;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.info.GFORMAT;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<NurseryInstance, ROOM_NURSERY> {


	
	Gui(ROOM_NURSERY s) {
		super(s);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<NurseryInstance> getter, int x1, int y1) {
		
		GuiSection s = new GuiSection();
		
		s.add(new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.f0(text, IndustryUtil.calcProductionRate(blueprint.ChildPErE, blueprint.rate, blueprint.bonus(), getter.get()));
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				b.title(blueprint.bonus().name);
				b.text(blueprint.bonus().desc);
				b.NL();
				IndustryUtil.hoverProductionRate(b, blueprint.ChildPErE, blueprint.rate, blueprint.bonus(), getter.get());
			}
			
		}.hh(blueprint.bonus().icon));
		
		section.addRelBody(8, DIR.S, s);
	
		
	}
	

}
