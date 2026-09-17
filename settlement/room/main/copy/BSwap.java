package settlement.room.main.copy;

import game.faction.FACTIONS;
import init.sprite.SPRITES;
import init.sprite.UI.Icon;
import settlement.main.SETT;
import settlement.room.main.ROOMS;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.RoomBlueprintIns;
import snake2d.SPRITE_RENDERER;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.clickable.CLICKABLE;
import snake2d.util.sets.ArrayListResize;
import snake2d.util.sets.KeyMap;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LinkedList;
import snake2d.util.sprite.SPRITE;
import util.gui.misc.GButt;
import util.text.D;
import view.main.VIEW;

final class BSwap {

	private final KeyMap<GuiSection> otherPrints = new KeyMap<GuiSection>();
	private CLICKABLE button;
	private RoomBlueprintImp current;
	private final ArrayListResize<CLICKABLE> wrap = new ArrayListResize<>(4, 16);
	
	private static CharSequence ¤¤swap = "¤Switch to another type of room.";
	static {
		D.ts(BSwap.class);
	}
	
	BSwap(ROOMS m){
		SPRITE sp = new SPRITE.Imp(Icon.M) {
			
			@Override
			public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
				current.iconBig().renderC(r, X1+(X2-X1)/2, Y1+(Y2-Y1)/2);
				SPRITES.icons().m.rotate.renderC(r, X1+(X2-X1)/2, Y1+(Y2-Y1)/2);
				
			}
		};
		
		button = new GButt.ButtPanel(sp) {

			@Override
			protected void clickA() {
				VIEW.inters().popup.show(alt(), this);
			}
			
		}.setDim(Icon.L+4).hoverInfoSet(¤¤swap);
		
		for (int i = 0; i < m.all().size(); i++) {
			RoomBlueprint p = m.all().get(i);
			if (p instanceof RoomBlueprintIns<?>)
				addGroup((RoomBlueprintIns<?>) p, m);
		}
	}
	
	private GuiSection alt(){
		return otherPrints.get(current.key);
	}
	
	void init(RoomBlueprintImp bb) {
		current = bb;
		if (bb.reqs.passes(FACTIONS.player()))
			return;
		
		
		for (RoomBlueprint p : SETT.ROOMS().all()) {
			if (p instanceof RoomBlueprintIns<?>) {
				RoomBlueprintIns<?> ins = (RoomBlueprintIns<?>) p;
				if (ins.getClass() == bb.getClass() && ins.constructor().mustBeIndoors() == bb.constructor().mustBeIndoors() && ins.reqs.passes(FACTIONS.player())) {
					current = ins;
					return;
				}
			}
		}
		
	}
	
	LIST<CLICKABLE> wrap(LIST<CLICKABLE> others){
		wrap.clearSoft();
		if (others != null)
			wrap.add(others);
		if (alt() != null) {
			wrap.add(button);
		}
		return wrap;
	}
	
	RoomBlueprintImp current() {
		return current;
	}
	
	private void addGroup(RoomBlueprintIns<?> blue, ROOMS m) {
		if (otherPrints.get(blue.key) != null)
			return;
		if (!blue.constructor().usesArea())
			return;
		
		
		LinkedList<RoomBlueprintImp> res = new LinkedList<>();
		
		for (RoomBlueprint p : m.all()) {
			if (p instanceof RoomBlueprintIns<?>) {
				RoomBlueprintIns<?> ins = (RoomBlueprintIns<?>) p;
				if (ins.getClass() == blue.getClass() && ins.constructor().mustBeIndoors() == blue.constructor().mustBeIndoors())
					res.add(ins);
			}
		}
		
		if (res.size() <= 1)
			return;
		
		GuiSection s = new GuiSection();
		
		for (RoomBlueprintImp p : res) {
			CLICKABLE c = new GButt.Panel(p.iconBig(), p.info.name) {

				
				@Override
				protected void renAction() {
					selectedSet(current == p);
					activeSet(p.reqs.passes(FACTIONS.player()));
				}
				
				@Override
				public void hoverInfoGet(GUI_BOX text) {
					text.title(p.info.name);
					text.text(p.info.desc);
					text.NL();
					
					if (!p.reqs.passes(FACTIONS.player())) {
						p.reqs.hover(text, FACTIONS.player());
					}
				}
				
				@Override
				protected void clickA() {
					if (p.reqs.passes(FACTIONS.player()))
						current = p;
					VIEW.inters().popup.close();
				}
				
			};
			s.addDownC(0, c);
		}
		
		otherPrints.put(blue.key, s);
	}
	
}
