package settlement.room.law.execution;

import java.util.Arrays;

import init.settings.S;
import settlement.room.law.execution.ExecutionStation.Client;
import settlement.room.main.Room;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Rec;
import snake2d.util.file.Alloc;
import util.gui.misc.GBox;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;
import view.sett.ui.room.UIRoomModule;

class Gui extends UIRoomModule {
	
	private static CharSequence ¤¤available = "Available Executions";
	private static CharSequence ¤¤spectators = "Spectators";
	
	static {
		D.ts(Gui.class);
	}
	
	private final ROOM_EXECTUTION b;

	Gui(ROOM_EXECTUTION s) {
		this.b = s;
	}

	final int[] states = Alloc.ii(ExecutionStation.STATE_DEAD+1);
	private final Rec body = new Rec();
	
	@Override
	public void hover(GBox box, Room room, int rx, int ry) {
		
		box.NL();
		
		body.moveX1Y1(room.x1(rx, ry), room.y1(rx, ry));
		body.setDim(room.width(rx, ry), room.height(rx, ry));

		int available = 0;
		int total = 0;
		Arrays.fill(states, 0);
		int specs = 0;
		int specsTot = 0;
		for (COORDINATE c : body) {
			if (room.isSame(rx, ry, c.x(), c.y())) {
				Client cl = b.stations.client(c.x(), c.y());
				if (cl != null) {
					total ++;
					if (!cl.clientReserved())
						available++;
					if (b.stations.service(c.x(), c.y()) != null) {
						specsTot += ExecutionStation.services;
						specs += b.stations.sevices(c.x(), c.y());
					}
					states[b.stations.state(c.x(), c.y())]++;
				}	
			}
		}
		
		box.textLL(¤¤spectators);
		box.tab(7);
		box.add(GFORMAT.iofk(box.text(), specs, specsTot));
		box.NL();
		
		box.textLL(¤¤available);
		box.tab(7);
		box.add(GFORMAT.iofk(box.text(), available, total));
		box.NL();
		
		box.textLL(Dic.¤¤Total);
		box.tab(7);
		box.add(GFORMAT.iofk(box.text(), b.stations.available(), b.stations.total()));
		box.NL();
		
		if (S.get().developer) {
			for (int i = 0; i < states.length; i++) {
				box.add(box.text().add(i).add(':'));
				box.add(box.text(). add(states[i]));
				box.NL();
			}
		}

	}
	


}
