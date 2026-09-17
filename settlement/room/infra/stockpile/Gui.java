package settlement.room.infra.stockpile;

import static settlement.room.infra.logistics.MoveDic.¤¤allocatedCrates;
import static settlement.room.infra.logistics.MoveDic.¤¤capacityD;
import static settlement.room.infra.logistics.MoveDic.¤¤fetch;
import static settlement.room.infra.logistics.MoveDic.¤¤fetchD;
import static settlement.room.infra.logistics.MoveDic.¤¤fetching;
import static settlement.room.infra.logistics.MoveDic.¤¤storing;
import static settlement.room.infra.logistics.MoveDic.¤¤storingD;

import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.settings.S;
import init.sprite.UI.Icon;
import init.sprite.UI.UI;
import settlement.main.SETT;
import settlement.room.infra.logistics.MoveDic;
import settlement.room.infra.logistics.MoveOrderPull;
import settlement.room.infra.logistics.MoveOrderPullUI;
import settlement.room.infra.logistics.MoveOrderPullersUI;
import settlement.room.infra.stockpile.StockpileTally.TallyData;
import settlement.room.main.RoomInstance;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.color.OPACITY;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.misc.ACTION;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LISTE;
import snake2d.util.sets.LinkedList;
import snake2d.util.sets.Stack;
import snake2d.util.sprite.SPRITE;
import snake2d.util.sprite.text.Str;
import util.colors.GCOLOR;
import util.data.GETTER;
import util.data.GETTER.GETTER_IMP;
import util.data.INT.INTE;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GGrid;
import util.gui.misc.GHeader;
import util.gui.misc.GMeter;
import util.gui.misc.GMeter.GMeterCol;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.gui.slider.GSliderInt;
import util.gui.table.GRows;
import util.gui.table.GScrollRows;
import util.gui.table.GTableSorter.GTFilter;
import util.gui.table.GTableSorter.GTSort;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;
import view.interrupter.ISidePanel;
import view.main.VIEW;
import view.sett.ui.room.UIRoomBulkApplier;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<StockpileInstance, ROOM_STOCKPILE> {
	
	private static CharSequence ¤¤special = "This setting locks usage to a single crate and lets you precisely dictate the maximum amount of items to be stored.";
	
	static {
		D.ts(Gui.class);
	}
	
	Gui(ROOM_STOCKPILE s) {
		super(s);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<StockpileInstance> g, int x1, int y1) {
		
		RENDEROBJ r = null;
		
		r = new GStat() {

			@Override
			public void update(GText text) {
				GFORMAT.percInv(text, g.get().getUsedSpace());
				
			}
		}.hh(Dic.¤¤Capacity).hoverInfoSet(¤¤capacityD);
		grid.add(r);
		
		
		r = new GStat() {

			@Override
			public void update(GText text) {
				GFORMAT.iofk(text, blueprint.tally().crates.get(null, g.get()), g.get().totalCrates());
				
			}
		}.hh( blueprint.tally().crates.name).hoverInfoSet(¤¤allocatedCrates);
		grid.add(r);
		
		{
			GuiSection s = new GuiSection();
			{
				
				GButt.ButtPanel p = new GButt.ButtPanel(UI.icons().m.wheel) {
					
					@Override
					protected void renAction() {
						selectedSet(g.get().fetching());
					}
					
					@Override
					protected void clickA() {
						g.get().fetchingSet(!g.get().fetching());
					}
					
					@Override
					protected void render(SPRITE_RENDERER r, float ds, boolean isActive, boolean isSelected,
							boolean isHovered) {
						
						super.render(r, ds, isActive, isSelected, isHovered);
						if (g.get().fetching() && g.get().coolFetch > -1) {
							GCOLOR.UI().SOSO.hovered.bind();
							UI.icons().s.alert.render(r, body.x1()+6, body.y1()+6);
							COLOR.unbind();
						}
					}
					
					@Override
					public void hoverInfoGet(GUI_BOX text) {
						GBox b = (GBox) text;
						b.title(¤¤fetch);
						b.text(¤¤fetchD);
						b.NL();
						if (g.get().fetching() && g.get().coolFetch > -1) {
							b.add(b.text().warnify().add(MoveDic.¤¤fetchProblem));
						}
						super.hoverInfoGet(text);
					}
					
				};
				p.body.setDim(48);
				s.addRightC(0, p);
				
				p = new GButt.ButtPanel(UI.icons().m.priority) {
					
					@Override
					protected void renAction() {
						selectedSet(g.get().prioritizing());
					}
					
					@Override
					protected void clickA() {
						g.get().prioritizeToggle();
					}
					
					@Override
					protected void render(SPRITE_RENDERER r, float ds, boolean isActive, boolean isSelected,
							boolean isHovered) {
						
						super.render(r, ds, isActive, isSelected, isHovered);
						if (g.get().prioritizing() && g.get().coolFetch > -1) {
							GCOLOR.UI().SOSO.hovered.bind();
							UI.icons().s.alert.render(r, body.x1()+6, body.y1()+6);
							COLOR.unbind();
						}
					}
					
					@Override
					public void hoverInfoGet(GUI_BOX text) {
						GBox b = (GBox) text;
						b.title(MoveDic.¤¤prio);
						b.text(MoveDic.¤¤prioD);
						b.NL();
						if (g.get().prioritizing() && g.get().coolFetch > -1) {
							b.add(b.text().warnify().add(MoveDic.¤¤fetchProblem));
						}
						
					}
					
				};
				
				p.body.setDim(48);
				s.addRightC(0, p);
			
			}
			
			
			MoveOrderPullUI ui = new MoveOrderPullUI(g, g, RESOURCES.ALL(), StockpileInstance.ORDERS);
			s.addRightC(8, ui);
			s.addRightC(0, new MoveOrderPullersUI(g));
			{
				
				GButt.ButtPanel p = new GButt.ButtPanel(UI.icons().m.lock) {
					
					@Override
					protected void renAction() {
						selectedSet(g.get().storing());
					}
					
					@Override
					protected void clickA() {
						g.get().storingSet(!g.get().storing());
					}
					
				};
				p.hoverTitleSet(¤¤storing);
				p.hoverInfoSet(¤¤storingD);
				p.body.setDim(48);
				s.addRightC(8, p);
			
			}
			
			section.addRelBody(4, DIR.S, s);
			
		}

		section.addRelBody(4, DIR.S, new RENDEROBJ.RenderImp(1, 8) {
			
			@Override
			public void render(SPRITE_RENDERER r, float ds) {
				GCOLOR.UI().border().render(r, section.body().x1()+8, section.body().x2()-8, body.y1()+4, body.y1()+5);
			}
		});
		
		section.addRelBody(8, DIR.S, new ResArea2(g, section.getLastY2()+8));
		
		//makeTable2(g, section, section.getLastY2()+20);
		
	}

	

	
	@Override
	protected void appendTableButt(GuiSection s, GETTER<RoomInstance> ins) {

		s.add(new SPRITE.Imp(s.body().width(), Icon.M) {

			@Override
			public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
				StockpileInstance in = (StockpileInstance) ins.get();
				
				double t = in.getUsedSpace();
				GMeter.render(r, GMeter.C_BLUE, t, X1, X2, Y1, Y2);
				
				int x = X1+2;
				
				for (RESOURCE res : RESOURCES.ALL()) {
					if (blueprint.tally().crates.get(res, in) > 0) {
						res.icon().small.render(r, x, Y1+4);
						x += res.icon().small.width();
						if (x >= X2-res.icon().small.width())
							break;
						
					}
				}
				
			}
		}, 0, s.body().y2());

	}
	

	@Override
	protected void problem(StockpileInstance i, Stack<Str> free, LISTE<CharSequence> errors,
			LISTE<CharSequence> warnings) {
		if (i.employees().target() == 0)
			return;
		boolean ok = false;
		boolean has = false;
		for (MoveOrderPull o : i.moveOrdersPull()) {
			if (o != null) {
				has = true;
				CharSequence p = o.problem(i);
				if (p != null) {
					errors.add(p);
					break;
				}else if (o.cooldown >= -1)
					ok = true;
			}
		}
		
		if (i.fetching() && i.coolFetch > -1 && i.coolOrganize > -1 && (has && !ok)) {
			errors.add(MoveDic.¤¤pullProblem);
		}

		super.problem(i, free, errors, warnings);
	}
	
	@Override
	protected void hover(GBox box, StockpileInstance i) {
		super.hover(box, i);
		box.sep();
		if (i.fetching() && i.employees().target() > 0) {
			box.textL(¤¤fetching);
			box.NL();
			if (i.coolFetch > -1) {
				box.add(box.text().warnify().add(MoveDic.¤¤fetchProblem));
				box.NL();
			}
			for (MoveOrderPull o : i.moveOrdersPull())
				if (o != null) {
					CharSequence p = o.warning(i);
					if (p != null) {
						box.add(box.text().warnify().add(p));
						box.NL();
					}
				}
			
		}
		if (i.storing()) {
			box.add(box.text().warnify().add(¤¤storing));
		}
		int m = 0;
		box.NL(8);
		
		for (RESOURCE r : RESOURCES.ALL()) {
			
			if (blueprint.tally().crates.get(r, i) > 0) {
				
				box.tab((m%3)*5);
				
				box.add(r.icon());
				box.add(GFORMAT.iofkInv(box.text(), blueprint.tally().amount.get(r, i), blueprint.tally().space.get(r, i)));
				
				if (m % 3 == 2) {
					box.NL();
				}
				m++;
				
			}
			
			
			
			
		}
		
	}
	
	@Override
	protected void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts,
			LISTE<UIRoomBulkApplier> appliers) {
		sorts.add(new GTSort<RoomInstance>(Dic.¤¤Resource) {
			
			@Override
			public void format(RoomInstance h, GText text) {
				text.add(h.name());
			}
			
			@Override
			public int cmp(RoomInstance current, RoomInstance cmp) {
				return oo(current)-oo(cmp);
			}
			
			private int oo(RoomInstance ins) {
				StockpileInstance i = (StockpileInstance) ins;
				for (int ri = 0; ri < RESOURCES.ALL().size(); ri++)
					if (SETT.ROOMS().STOCKPILE.tally().crates.get(ri, i) > 0)
						return ri;
				return 0;
			}
		});
		for (RESOURCE res : RESOURCES.ALL()) {
			filters.add(new GTFilter<RoomInstance>(res.names) {
				
				@Override
				public boolean passes(RoomInstance h) {
					StockpileInstance i = (StockpileInstance) h;
					if (blueprint.tally().crates.get(res, i) > 0)
						return true;
					return false;
				}
			});
		}
		
		super.appendTableFilters(filters, sorts, appliers);
	}
	
	private static class ResArea2 extends GuiSection{
		
		private ArrayList<RESOURCE> current = new ArrayList<RESOURCE>(RESOURCES.ALL().size());
		private final GETTER<StockpileInstance> g;
		private StockpileInstance lastOpen;
		private double renView = 0;
		private final GuiSection pop;
		
		ResArea2(GETTER<StockpileInstance> g, int y1){
			this.g = g;
			ACTION change = new ACTION() {
					
					@Override
					public void exe() {
						lastOpen = null;
					}
				};
			pop = makeTable(g, change);
			add(new GHeader(Dic.¤¤Resources));
			addRightC(16, new GButt.ButtPanel(Dic.¤¤Settings) {
				@Override
				protected void clickA() {
					VIEW.inters().popup.show(pop, this);
					super.clickA();
				}
			});
			
			
			GRows rows = new GRows(2);
			for (int ri = 0; ri < RESOURCES.ALL().size(); ri++) {
				final int rr = ri;
				GETTER<RESOURCE> gg = new GETTER<RESOURCE>() {
					
					@Override
					public RESOURCE get() {
						return current.get(rr);
					}
					
				};
				
				rows.add(new ResLineDetailed(gg, g, ACTION.NOP));
			}
			

			
			addRelBody(8, DIR.S, new GScrollRows(rows.rows(), ISidePanel.HEIGHT-y1-64) {
				
				@Override
				protected boolean passesFilter(int i, RENDEROBJ o) {
					return i < Math.ceil(current.size()/2.0);
				}
				
			}.view());

			pad(8, 8);
		}
		
		@Override
		public void render(SPRITE_RENDERER r, float ds) {
			if (lastOpen == null || g.get() != lastOpen || Math.abs(renView-VIEW.renderSecond()) > 5) {
				current.clearSloppy();
				for (RESOURCE res : RESOURCES.ALL()) {
					if (SETT.ROOMS().STOCKPILE.tally().space.get(res, g.get()) > 0)
						current.add(res);
				}
				lastOpen = g.get();
			}
			renView = VIEW.renderSecond();
//			
//			for (RESOURCE res : RESOURCES.ALL()) {
//				if (SETT.ROOMS().STOCKPILE.tally().space.get(res, g.get()) > 0)
//					current.add(res);
//			}
			super.render(r, ds);
		}
		
		private static GuiSection makeTable(GETTER<StockpileInstance> g, ACTION change) {
			GuiSection section = new GuiSection();
			LinkedList<RENDEROBJ> rows = new LinkedList<>();
			int rr = 2;
			int r = 0;
			
			GuiSection ss = null;
			int ci = -1;
			for (RESOURCE res: RESOURCES.ALL()) {
				if (res.category != ci) {
					final LinkedList<RESOURCE> ress = new LinkedList<>();
					for (int ri = 0; ri < RESOURCES.ALL().size(); ri++) {
						if (RESOURCES.ALL().get(ri).category == res.category)
							ress.add(RESOURCES.ALL().get(ri));
					}

					
					GuiSection s = new GuiSection();
					s.add(new GButt.ButtPanel(UI.icons().s.minifier) {
						@Override
						protected void clickA() {
							for (RESOURCE r : ress) {
								g.get().allocateCrate(r, SETT.ROOMS().STOCKPILE.tally().crates.get(r, g.get())-1);
							}
						}
					});
					s.addRightC(0, new GButt.ButtPanel(UI.icons().s.magnifier) {
						@Override
						protected void clickA() {
							for (RESOURCE r : ress) {
								g.get().allocateCrate(r, SETT.ROOMS().STOCKPILE.tally().crates.get(r, g.get())+1);
							}
						}
					});
					
					
					rows.add(s);
					ci = res.category;
					r = 0;
					ss = new GuiSection();
					rows.add(ss);
					
				}
				if (r >= rr) {
					r = 0;
					ss = new GuiSection();
					rows.add(ss);
				}
				
				ss.addRightC(0, new ResLineDetailed(new GETTER_IMP<RESOURCE>(res), g, change));
				r++;
			}
			
			section.addRelBody(6, DIR.S, new GStat() {

				@Override
				public void update(GText text) {
					GFORMAT.iofk(text, g.get().blueprintI().tally().crates.get(null, g.get()), g.get().totalCrates());
					
				}
			}.hh(SETT.ROOMS().STOCKPILE.tally().crates.name).hoverInfoSet(¤¤allocatedCrates));
			
			section.addRelBody(6, DIR.S, new GScrollRows(rows, ISidePanel.HEIGHT-8).view());
			return section;
		}
		
		private static class ResLineDetailed extends GuiSection {
			
			private final GETTER<RESOURCE> res;
			RESOURCE prev = null;
			private final GSliderInt gg;
			private final INTE crates;
			private final GETTER<StockpileInstance> g;
			
			ResLineDetailed(GETTER<RESOURCE> res, GETTER<StockpileInstance> g, ACTION change){
				
				add(new SPRITE.Imp(Icon.M) {
					
					@Override
					public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
						res.get().icon().render(r, X1, X2, Y1, Y2);
						
					}
				},0 ,0);
				this.g = g;
				
				this.crates = new INTE() {
					
					@Override
					public int get() {
						return g.get().getSpecialAmount(res.get()) == 0 ? SETT.ROOMS().STOCKPILE.tally().crates.get(res.get(), g.get()) : g.get().getSpecialAmount(res.get());
					}

					@Override
					public int min() {
						return g.get().getSpecialAmount(res.get()) == 0 ? 0 : 1;
					}

					@Override
					public int max() {
						if (g.get().getSpecialAmount(res.get()) == 0)
							return  g.get().totalCrates();
						return Math.min(100, g.get().crateSize());
					}

					@Override
					public void set(int t) {
						if (g.get().getSpecialAmount(res.get()) == 0) {
							int m = 0;
							for (int i = 0; i < RESOURCES.ALL().size(); i++) {
								if (i == res.get().index())
									continue;
								m += SETT.ROOMS().STOCKPILE.tally().crates.get(i, g.get());
								
							}
							if (m + t > g.get().totalCrates()) {
								t = g.get().totalCrates() - m;
							}
							g.get().allocateCrate(res.get(), t);
						}else {
							g.get().setSpecialAmount(res.get(), t);
						}
						change.exe();
					}
				};;
				
				gg = new GSliderInt(crates, 160, 24, true) {
					
					@Override
					protected void renderMidColor(SPRITE_RENDERER r, int x1, int width, int widthFull, int y1, int y2) {
						double a = SETT.ROOMS().STOCKPILE.tally().amount.get(res.get(), g.get());
						double c = g.get().getSpecialAmount(res.get()) == 0 ? crates.get()*g.get().crateSize(res.get()) : g.get().getSpecialAmount(res.get());
						double d = 0;
						if (c > 0)
							d = a/c;
						GMeterCol col = GMeter.C_INACTIVE;
						if (d > 0.9)
							col= GMeter.C_REDPURPLE;
						else if (c > 0)
							col= GMeter.C_REDGREEN;
						else
							GMeter.render(r, GMeter.C_INACTIVE, d, body());
						
						col.bg.render(r, x1, x1+width, y1, y2);
						
						col.dark.render(r, x1, (int) (x1+width*d), y1, y2);
						col.bright.render(r, x1, (int) (x1+width*d), y1+1, y2-1);
						
					}
					
					@Override
					public void hoverInfoGet(GUI_BOX text) {
						
					}
					
				};
				addRightC(4, gg);
				
				GStat s = new GStat() {
					@Override
					public void update(GText text) {
						GFORMAT.i(text, SETT.ROOMS().STOCKPILE.tally().amount.get(res.get(), g.get()));
						
						double max = g.get().getSpecialAmount(res.get()) == 0 ? crates.get()*g.get().crateSize(res.get()) : g.get().getSpecialAmount(res.get());
						if (max == 0)
							text.color(GCOLOR.T().NORMAL);
						else {
							double am = SETT.ROOMS().STOCKPILE.tally().amount.get(res.get(), g.get());
							double d = am / max;
							if (d < 0.5) {
								ColorImp.TMP.interpolate(GCOLOR.T().IBAD, GCOLOR.T().WARNING, d*2.0);
							}else {
								ColorImp.TMP.interpolate(GCOLOR.T().WARNING, GCOLOR.T().IGREAT, (d-0.5)*2.0);
							}
							text.color(ColorImp.TMP);
						}
						
					}
					
					@Override
					public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
						if (hoveredIs() || crates.get() == 0)
							return;
						OPACITY.O50.bind();
						COLOR.BLACK.render(r, X1-2, X2+2, Y1-1, Y2+2);
						OPACITY.unbind();
						super.render(r, X1, X2, Y1, Y2);
					}
				};
				
				addCentredY(s.r(DIR.E), getLastX2()-38-36);
				
				addCentredY(new GButt.ButtPanel(UI.icons().s.arrowDown) {
					
					@Override
					protected void clickA() {
						g.get().setSpecialAmount(res.get(), g.get().getSpecialAmount(res.get()) == 0 ? Math.min(100, g.get().crateSize()) : 0);
					}
					
					@Override
					protected void renAction() {
						activeSet(SETT.ROOMS().STOCKPILE.tally().crates.get(res.get(), g.get()) > 0 || g.get().totalCrates() -  SETT.ROOMS().STOCKPILE.tally().crates.get(null, g.get()) > 0);
						selectedSet(g.get().getSpecialAmount(res.get()) > 0);
					}
					
				}.hoverInfoSet(¤¤special), body().x2()+2);

				
				pad(6, 2);
				
				this.res = res;
				
			}

			@Override
			public void render(SPRITE_RENDERER r, float ds) {
				if (res.get() != prev)
					gg.reset();
				prev = res.get();
				if (res.get() == null)
					return;
				GCOLOR.UI().border().render(r, body(), -2);
				boolean hov = hoveredIs();
				super.render(r, ds);
				if (crates.get() == 0 && !hov) {
					OPACITY.O25.bind();
					COLOR.BLACK.render(r, body());
					OPACITY.unbind();
				}
			}
			
			@Override
			public void hoverInfoGet(GUI_BOX text) {
				if (res.get() != prev)
					gg.reset();
				prev = res.get();
				if (res.get() == null)
					return;
				super.hoverInfoGet(text);
				if (text.emptyIs()) {
					Gui.hover(text, g.get(), res.get());
				}
			}
			
			@Override
			public boolean click() {
				if (res.get() != prev)
					gg.reset();
				prev = res.get();
				if (res.get() == null)
					return false;
				return super.click();
			}
			
		}
		
	}

	
	private static void hover(GUI_BOX text, StockpileInstance ins, RESOURCE res) {
		text.title(res.name);
		text.text(res.desc);
		GBox b = (GBox) text;
		b.sep();
//		

		for (TallyData d : SETT.ROOMS().STOCKPILE.tally().datas) {
			b.textLL(d.name);
			b.tab(7);
			b.add(GFORMAT.i(b.text(), d.get(res, ins)));
			b.NL();
			
		}
		b.sep();
		b.textLL(Dic.¤¤Total);
		b.NL();
		for (TallyData d : SETT.ROOMS().STOCKPILE.tally().datas) {
			b.textLL(d.name);
			b.tab(7);
			b.add(GFORMAT.i(b.text(), d.total(res)));
			b.NL();
			
		}
		
		if (S.get().developer) {
			GText t = b.text();
			t.add(ins.fetchMask.has(res)).s().add(ins.fetchMaskBig.has(res)).s().add(ins.reservableMask.has(res));
			b.add(t);
			b.NL();
			t = b.text();
			t.add(ins.hasTriedBig);
			b.add(t);
		}
		
	}
	
}
