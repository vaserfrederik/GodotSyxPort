package settlement.room.tests;

import settlement.room.main.ROOMS;
import settlement.room.military.artillery.ROOM_ARTILLERY;
import snake2d.util.misc.ACTION;
import view.interrupter.IDebugPanel;
import view.main.VIEW;
import view.sett.IDebugPanelSett;

public final class RoomTests {

	public RoomTests(ROOMS r) {
		for (ROOM_ARTILLERY a : r.ARTILLERY) {
			IDebugPanelSett.add(new ArtilleryTest(a.eplacer));
		}
		
		IDebugPanel.add("production & trade panel", new ACTION() {
			
			@Override
			public void exe() {
				VIEW.inters().popup.show(new UITradeDebug(), null);
			}
		});
	}
	
}
