package settlement.room.law.guard;

import game.GAME;
import game.battle.div.Div;
import init.settings.S;
import init.sprite.UI.UI;
import init.type.HTYPES;
import settlement.main.SETT;
import settlement.stats.STATS;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.renderable.RENDEROBJ;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GGrid;
import util.gui.misc.GHeader;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;
import view.main.VIEW;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;
import view.ui.div.UIGuardDivSelector;

class Gui extends UIRoomModuleImp<GuardInstance, ROOM_GUARD> {
	
	private static CharSequence ¤¤guards = "Guard Force";
	private static CharSequence ¤¤effDesc = "Degrade and employment determines the efficiency of a guard-post.";
	
	static {
		D.ts(Gui.class);
	}
	
	
	Gui(ROOM_GUARD s) {
		super(s);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<GuardInstance> g, int x1, int y1) {
		
	
		
		section.addRelBody(16, DIR.S, new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.perc(text, g.get().eff());
			}
		}.hv(Dic.¤¤Efficiency, ¤¤effDesc));
		
		
		if (S.get().developer) {
			section.addRelBody(16, DIR.S, new GStat() {
				
				@Override
				public void update(GText text) {
					SETT.OVERLAY().envThing(SETT.ENV().map.GUARD);
					GFORMAT.i(text, blueprint.reporter.crimes(g.get()));
					text.s().add(blueprint.reporter.crimes(null));
				}
			}.hv("crimes"));
			
			section.addRelBody(16, DIR.S, new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.i(text, blueprint.reporter.executions(g.get()));
					text.s().add(blueprint.reporter.executions(null));
				}
			}.hv("executions"));
		}

	}
	
	@Override
	protected void appendMain(GGrid grid, GGrid text, GuiSection sExtra) {
	GuiSection s = new GuiSection();
		

		
		s.add(new GHeader(¤¤guards));
		
		s.addDown(2, new GStat() {
			
			@Override
			public void update(GText text) {
				int a = 0;
				for (Div d : GAME.ARMIES().player().divisions())
					if (blueprint.activeDuty.is(d))
						a++;
				GFORMAT.i(text, a);
			}
		}.hh(Dic.¤¤Divisions, 120));
		
		
		
		s.add(new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.i(text, STATS.POP().pop(HTYPES.GUARD()));
			}
		}.hh(Dic.¤¤Soldiers, 120), 0, s.body().y2()+4);
		
		s.add(new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.f0(text, blueprint.power.get());
			}
		}.hh(Dic.¤¤Power, 120), 0, s.body().y2()+4);
		
		s.add(new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.perc(text, SETT.ENV().map.GUARD.stat().data().getD(null));
			}
		}.hh(STATS.ENV().info.name, 120), 0, s.body().y2()+4);
		
		s.add(new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.f0(text, blueprint.averageUpgrade());
			}
		}.hh(Dic.¤¤Upgrade, 120), 0, s.body().y2()+4);
		
		
		
		s.addRelBody(64, DIR.E, new GButt.ButtPanel(UI.icons().m.shield) {
			
			RENDEROBJ oo = new UIGuardDivSelector();
			
			
			@Override
			protected void clickA() {
				VIEW.inters().popup.show(oo, this);
			}
			
		}.pad(8, 8));
		
		if (S.get().developer)
			s.addDown(2, blueprint.patrols.debugButt());
		
		
		text.section.addRelBody(8, DIR.S, s);

	}
	
	@Override
	protected void hover(GBox box, GuardInstance i) {
		//SETT.OVERLAY().RadiusInter(blueprint, blueprint.finder);
	}
	


}
