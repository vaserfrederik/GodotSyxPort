package settlement.room.service.arena;

import init.sprite.UI.UI;
import settlement.room.main.RoomInstance;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GuiSection;
import util.data.GETTER;
import util.gui.misc.GGrid;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.text.D;
import view.sett.ui.room.UIRoomModule;

public class RoomArenaGui extends UIRoomModule {

	private static CharSequence ¤¤Executions = "Executions";
	private static CharSequence ¤¤ExecutionsD = "The amount of prisoners that are currently being executed.";
	
	static {
		D.ts(RoomArenaGui.class);
	}
	
	private final RoomArenaWork w;
	
	public RoomArenaGui(RoomArenaWork w) {
		this.w = w;
	}
	
	@Override
	public void appendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1) {
		section.addRelBody(8, DIR.S, new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.iofk(text, w.executions(get.get()), w.executionsMax(get.get()));
			}
		}.hh(¤¤Executions).hoverInfoSet(¤¤ExecutionsD));
	}
	
	@Override
	public void appendManageScr(GGrid icons, GGrid text, GuiSection extra) {
		icons.add(new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.iofk(text, w.executions(), w.executionsMax());
			}
		}.hh(UI.icons().s.death).hoverTitleSet(¤¤Executions).hoverInfoSet(¤¤ExecutionsD));
		
		// TODO Auto-generated method stub
		super.appendManageScr(icons, text, extra);
	}

}
