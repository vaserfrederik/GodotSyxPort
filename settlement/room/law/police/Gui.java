package settlement.room.law.police;

import game.boosting.BOOSTABLES;
import game.boosting.BoostSpec;
import init.race.appearence.RPortrait;
import init.settings.S;
import init.sprite.UI.UI;
import init.type.HCLASS;
import init.type.HCLASS_RACE;
import settlement.entity.humanoid.Humanoid;
import settlement.stats.STATS;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.sprite.SPRITE;
import util.GUTIL;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GGrid;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.gui.table.GRows;
import util.gui.table.GScrollRows;
import util.gui.table.GTableBuilder;
import util.gui.table.GTableBuilder.GRowBuilder;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;
import view.main.VIEW;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<PoliceInstance, ROOM_POLICE> {
	
	private static CharSequence ¤¤suspects = "Suspects";
	private static CharSequence ¤¤AA = "Suspected Sloth";
	private static CharSequence ¤¤BB = "Suspected Witch";
	private static CharSequence ¤¤CC = "Suspected Warlock";
	private static CharSequence ¤¤DD = "Suspected Shapeshifter";
	private static CharSequence ¤¤EE = "Suspected Heretic";
	private static CharSequence ¤¤FF = "Suspected Beastialist";
	private static CharSequence ¤¤GG = "Suspected Turncoat";
	private static CharSequence ¤¤HH = "Suspected Renegade";
	private static CharSequence ¤¤II = "Suspected Collaborator";
	
	private static CharSequence ¤¤value = "The effect of your police force. Depends on the amount of police divided by the population to keep in check. Diminishing returns.";
	
	private static CharSequence[] tt = new CharSequence[] {¤¤AA, ¤¤BB, ¤¤CC, ¤¤DD, ¤¤EE, ¤¤FF, ¤¤GG, ¤¤HH, ¤¤II};
	
	static {
		D.ts(Gui.class);
	}
	
	Gui(ROOM_POLICE s) {
		super(s);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<PoliceInstance> g, int x1, int y1) {
		
		GuiSection s = new GuiSection() {
			
			@Override
			public void render(SPRITE_RENDERER r, float ds) {
				GUTIL.hList().clearSloppy();
				for (COORDINATE c : g.get().body()) {
					if (g.get().is(c)) {
						Humanoid a = blueprint.work.client(c.x(), c.y());
						if (a != null)
							GUTIL.hList().add(a);
					}
				}
				super.render(r, ds);
			}
			
		};
		
		GTableBuilder bu = new GTableBuilder() {
			
			@Override
			public int nrOFEntries() {
				
				return GUTIL.hList().size();
			}
		};
		
		bu.column(¤¤suspects, 400, new GRowBuilder() {
			
			@Override
			public RENDEROBJ build(GETTER<Integer> ier) {
				GButt.BSection r = new GButt.BSection() {
					
					@Override
					public void hoverInfoGet(GUI_BOX text) {
						if (ier.get() == null)
							return;
						Humanoid h = (Humanoid) GUTIL.hList().get(ier.get());
						VIEW.s().ui.subjects.hoverInfo(h, (GBox) text);
					}
					
				};
				
				r.add(new GStat(UI.FONT().H2) {
					
					@Override
					public void update(GText text) {
						if (ier.get() == null)
							return;
						Humanoid h = (Humanoid) GUTIL.hList().get(ier.get());
						text.lablify();
						text.add(STATS.APPEARANCE().name(h.indu()));
					}
				}, 0, 0);
				
				r.addDown(2, new GStat() {
					
					@Override
					public void update(GText text) {
						if (ier.get() == null)
							return;
						Humanoid h = (Humanoid) GUTIL.hList().get(ier.get());
						text.warnify();
						text.add(tt[STATS.RAN().get(h.indu(), 5)%tt.length]);
					}
				});
				
				r.addRelBody(8, DIR.W, new SPRITE.Imp(RPortrait.P_WIDTH, RPortrait.P_HEIGHT) {
					
					@Override
					public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
						if (ier.get() == null)
							return;
						Humanoid h = (Humanoid) GUTIL.hList().get(ier.get());
						STATS.APPEARANCE().portraitRender(r, h.indu(), X1, Y1, 1);
					}
				});
				
				r.body().setWidth(400-16);
				
				r.pad(8, 3);
				
				return r;
			}
		});
		s.add( bu.create(6, false));
		section.addRelBody(8, DIR.S, s);
		
	}
	
	@Override
	protected void appendMain(GGrid grid, GGrid text, GuiSection sExtra) {
		RENDEROBJ r = null;
		
		r = new GStat() {

			@Override
			public void update(GText text) {
				GFORMAT.perc(text, blueprint.value());
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				b.text(¤¤value);
				b.NL(4);
				
				b.textLL(Dic.¤¤Employees);
				b.tab(6);
				GFORMAT.i(b.text(), blueprint.employment().employed());
				b.NL();
				

				b.textLL(Dic.¤¤Population);
				b.NL();
				for (HCLASS_RACE r : HCLASS_RACE.REAL()) {
					if (blueprint.access(r).is()) {
						b.tab(1).add(r.icon);
						b.tab(6);
						GFORMAT.i(b.text(), STATS.POP().POP.data(r.cl).get(r.race));
						b.NL();
					}
				}
				
				b.text(Dic.¤¤Value);
				b.tab(6);
				GFORMAT.perc(b.text(), blueprint.value());
				b.NL();
			};
			
		}.hh(Dic.¤¤Value);
		text.add(r);
		
		
		for (BoostSpec s : blueprint.spec.all()) {
			
			r = new GStat() {

				@Override
				public void update(GText text) {
					s.booster.format(text, s.get(HCLASS_RACE.clP()));
				}
				
				
			}.hh(s.boostable.name);
			text.add(r);
			
			
		}
		
		
		
		
		GRows rr = new GRows(8);
		
		HCLASS prev = HCLASS_RACE.REAL().get(0).cl;
		for (HCLASS_RACE cl : HCLASS_RACE.REAL()) {
			if (prev != cl.cl) {
				prev = cl.cl;
				rr.nl();
			}
			if (cl.cl.player) {
				rr.add(new GButt.ButtPanel(cl.icon) {
					
					@Override
					protected void renAction() {
						selectedSet(blueprint.access(cl).is());
						super.renAction();
					}
					
					@Override
					protected void clickA() {
						blueprint.access(cl).toggle();
					}
					
					@Override
					public void hoverInfoGet(GUI_BOX text) {
						text.title(cl.name);
						GBox b = (GBox) text;
						b.textLL(BOOSTABLES.BEHAVIOUR().SUBMISSION.name);
						b.tab(6);
						b.add(GFORMAT.perc(b.text(), BOOSTABLES.BEHAVIOUR().SUBMISSION.get(cl)));
						b.NL();
						
						b.textLL(Dic.¤¤Population);
						b.tab(6);
						b.add(GFORMAT.i(b.text(), STATS.POP().POP.data(cl.cl).get(cl.race)));
						b.NL();
					}
					
				});
			}
			
		}
		
		
		
		RENDEROBJ rt = new GScrollRows(rr.rows(), rr.rows().get(0).body().height()*5).view();
		
		text.add(new GButt.ButtPanel(Dic.¤¤Settings) {
			
			@Override
			protected void clickA() {
				VIEW.inters().popup.show(rt, this);
			}
			
		});
		
	}
	
	@Override
	protected void hover(GBox box, PoliceInstance i) {
		box.NL();
		box.text(¤¤suspects);
		box.add(GFORMAT.i(box.text(), i.prisoners));
		if (S.get().developer) {
			box.add(GFORMAT.i(box.text(), i.prisonersMax()));
		}
	}
	


}
