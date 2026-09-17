package settlement.room.main.placement;

import game.GAME;
import settlement.main.SETT;
import settlement.room.main.furnisher.FurnisherItem;
import snake2d.util.sets.IntegerStack;

final class UtilHistory {
	
	private static final int historyAmount = 1024;
	private HistoryI[] histories = new HistoryI[historyAmount];
	private IntegerStack hStack = new IntegerStack(historyAmount);
	private short historyFirst = -1;
	private final RoomPlacer p;
	
	private short tick;
	private int currentTick;
	
	UtilHistory(RoomPlacer p) {
		for (int i = 0; i < historyAmount; i++) {
			histories[i] = new HistoryI(i);
			hStack.push(i);
		}
		this.p = p;
	}
	
	public void clear() {
		while(historyFirst != -1) {
			hStack.push(historyFirst);
			historyFirst = histories[historyFirst].next;
		}
		
		
		
		historyFirst = -1;
	}
	
	private void init() {
		if (currentTick != GAME.updateI()) {
			tick++;
			currentTick = GAME.updateI();
		}
	}
	
	public void placeDoor(int x1, int y1, int delta) {
		init();
		if (hStack.isEmpty()) {
			clearHistory(-1, historyFirst, 10);
		}
		int hi = hStack.pop();
		HistoryI h = histories[hi];
		h.x = (short) x1;
		h.y = (short) y1;
		h.action = (short) delta == 1 ? HistoryI.actionDoor : HistoryI.actionDoorRemove;
		h.next = historyFirst;
		h.tick = tick;
		historyFirst = (short) hi;
		
	}
	
	public void placeItem(FurnisherItem it, int x1, int y1, int delta) {
		init();
		if (hStack.isEmpty()) {
			clearHistory(-1, historyFirst, 10);
		}
		int hi = hStack.pop();
		HistoryI h = histories[hi];
		
		
		if (delta < 0) {
			h.action = (short) (it.index()*delta);
		}else {
			h.action = (short) (1+ it.index());
			x1 += it.firstX();
			y1 += it.firstY();
		}
		h.x = (short) x1;
		h.y = (short) y1;
		h.next = historyFirst;
		h.tick = tick;
		historyFirst = (short) hi;
	}
	
	public void placeEmbryo(int x1, int y1, int delta) {
		init();
		if (hStack.isEmpty()) {
			clearHistory(-1, historyFirst, 64);
		}
		int hi = hStack.pop();
		HistoryI h = histories[hi];
		h.x = (short) x1;
		h.y = (short) y1;
		h.action = delta == -1 ? HistoryI.actionShrink : HistoryI.actionExpand;
		h.next = historyFirst;
		h.tick = tick;
		historyFirst = (short) hi;
		
	}
	
	private int clearHistory(int previous, int current, int amount) {
		if (current == -1)
			return amount;
		if (clearHistory(current, histories[current].next, amount) > 0) {
			hStack.push(current);
			histories[current].next = -1;
			if (previous == -1) {
				historyFirst = -1;
			}else {
				histories[previous].next = -1;
			}
			
			return amount-1;
		}
		return 0;
	}
	
	public boolean hasHistory() {
		return historyFirst != -1;
	}
	
	public void popHistory() {
		
		short hi = historyFirst;
		int t = histories[hi].tick;
		while(hi != -1 && histories[hi].tick == t) {
			HistoryI h = histories[hi];
			hStack.push(hi);
			hi = h.next;
			historyFirst = hi;
			if (h.action < 256) {
				if (h.action <= 0) {
					FurnisherItem it = p.blueprint().constructor().item(-h.action);
					if (p.placability.itemProblem(h.x, h.y, it.group, it, p.instance) != null){
						clear();
						return;
					}
					
					for (int ry = 0; ry < it.height(); ry++) {
						for (int rx = 0; rx < it.width(); rx++) {
							if (p.placability.itemPlacable(h.x+rx, h.y+ry, rx, ry, it, p.instance) != null || !p.instance.is(h.x+rx, h.y+ry)) {
								clear();
								return;
							}
						}
					}
					SETT.ROOMS().fData.itemSet(h.x, h.y, it, p.instance);
				}else {
					SETT.ROOMS().fData.itemClear(h.x, h.y, p.instance);
				}
			}else if (h.action == HistoryI.actionExpand) {
				p.instance.clear(h.x, h.y);
			}else if (h.action == HistoryI.actionShrink){
				p.instance.set(h.x, h.y);
			}else if(h.action == HistoryI.actionDoor) {
				p.door.removeWithoutHistory(h.x, h.y);
			}else if (h.action == HistoryI.actionDoorRemove) {
				p.door.placeWithoutHistory(h.x, h.y);
				
			}
		}
		
		
	}
	
	private static class HistoryI {
		
		private static short actionExpand = 256+1;
		private static short actionShrink = 256+2;
		private static short actionDoor = (short) (actionShrink+1);
		private static short actionDoorRemove = (short) (actionDoor+16*2);
		
		private short next;
		private short x,y;
		private short action;
		private short tick;
		
		HistoryI(int index){
		}
	}
	
}
