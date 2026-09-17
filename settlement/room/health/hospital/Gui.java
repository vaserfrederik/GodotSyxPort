package settlement.room.health.hospital;

import game.faction.FACTIONS;
import init.resources.RESOURCE;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.sets.LISTE;
import snake2d.util.sets.Stack;
import snake2d.util.sprite.text.Str;
import util.data.GETTER;
import util.gui.misc.GButt;
import util.gui.misc.GGrid;
import util.text.D;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<HospitalInstance, ROOM_HOSPITAL> {

	private static CharSequence ¤¤nn = "Fetch:";
	private static CharSequence ¤¤hov = "Allowing this resource use increases recovery rate by 75%";
	
	static {
		D.ts(Gui.class);
	}
	
	Gui(ROOM_HOSPITAL s) {
		super(s);
	}
	


	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<HospitalInstance> getter, int x1, int y1) {
		
		GuiSection s = new GuiSection();
		
		
		for (int i = 0; i < blueprint.consumtion.ins().size(); i++) {
			RESOURCE res = blueprint.consumtion.ins().get(i).resource;
			final int k = i;
			GButt.ButtPanel b = new GButt.ButtPanel(res.icon()) {
				
				@Override
				protected void renAction() {
					selectedSet(getter.get().fetch[k]);
					activeSet(blueprint.resLocks.get(k).passes(FACTIONS.player()));
				}
				
				@Override
				protected void clickA() {
					getter.get().fetch[k] = !getter.get().fetch[k];
				}
				
				@Override
				public void hoverInfoGet(GUI_BOX text) {
					super.hoverInfoGet(text);
					text.NL();
					blueprint.resLocks.get(k).hover(text, FACTIONS.player());
					
				}
				
			};
			b.hoverTitleSet("" + ¤¤nn + " " + res.names);
			b.hoverInfoSet(¤¤hov);
			b.pad(16, 4);
			s.addRightC(0, b);
			
			
		}
		
		section.addRelBody(8, DIR.S, s);
		
		
	}

	@Override
	protected void problem(HospitalInstance i, Stack<Str> free, LISTE<CharSequence> errors,
			LISTE<CharSequence> warnings) {
		// TODO Auto-generated method stub
		super.problem(i, free, errors, warnings);
	}
	
}
