package settlement.room.service.hygine.bath;

import snake2d.util.gui.GuiSection;
import snake2d.util.sets.LISTE;
import snake2d.util.sets.Stack;
import snake2d.util.sprite.text.Str;
import util.data.GETTER;
import util.gui.misc.GGrid;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.info.GFORMAT;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<BathInstance, ROOM_BATH> {

	Gui(ROOM_BATH s) {
		super(s);
	}
	
	@Override
	protected void problem(BathInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings) {
		if (i.getHeat() < 1) {
			errors.add(blueprint.sHeatingProblem);
		}
		
//		if (i.water < 1) {
//			errors.add(blueprint.sWaterProblem);
//		}
			
		super.problem(i, free, errors, warnings);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<BathInstance> getter, int x1, int y1) {
		
	
		grid.add(new GStat() {
			@Override
			public void update(GText text) {
				GFORMAT.perc(text, getter.get().getHeat());
			}
		}.hh(blueprint.sHeating).hoverInfoSet(blueprint.sHeatingDesc));
		
//		grid.add(new GStat() {
//			@Override
//			public void update(GText text) {
//				GFORMAT.perc(text, getter.get().water);
//			}
//		}.hh(SETT.ENV().environment.WATER_SWEET.name).hoverInfoSet(SETT.ENV().environment.WATER_SWEET.desc));
		
	}

}
