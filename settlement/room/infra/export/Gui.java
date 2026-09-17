


package settlement.room.infra.export;

import static settlement.room.infra.logistics.MoveDic.¤¤fetch;
import static settlement.room.infra.logistics.MoveDic.¤¤fetchD;

import game.faction.FACTIONS;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.sprite.SPRITES;
import init.sprite.UI.Icon;
import init.sprite.UI.UI;
import init.trade.TR;
import settlement.main.SETT;
import settlement.room.infra.logistics.MoveDic;
import settlement.room.infra.logistics.MoveOrderPullUI;
import settlement.room.main.RoomInstance;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.clickable.CLICKABLE.ClickWrap;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.misc.Dictionary;
import snake2d.util.sets.LISTE;
import snake2d.util.sets.Stack;
import snake2d.util.sprite.SPRITE;
import snake2d.util.sprite.text.Str;
import util.colors.GCOLOR;
import util.data.GETTER;
import util.gui.common.UIPickerRes;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GGrid;
import util.gui.misc.GHeader;
import util.gui.misc.GMeter;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.gui.table.GTableSorter.GTFilter;
import util.gui.table.GTableSorter.GTSort;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;
import view.main.VIEW;
import view.sett.ui.room.UIRoomBulkApplier;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;
import view.ui.goods.UIGoodsExport;

class Gui extends UIRoomModuleImp<ExportInstance, ROOM_EXPORT> {


	private static CharSequence ¤¤NoResource = "¤No resource has been selected for export.";
	private static CharSequence ¤¤prioProb = "¤Globally, there is not enough stored goods to fetch from the un-prioritized storage rooms in the vicinity. Increase the global priority fetch limit.";
	private static CharSequence ¤¤prio = "¤Workers will only fetch as long as the condition for the fetch limit below is met.";
	
	static {
		D.ts(Gui.class);
	}
	
	Gui(ROOM_EXPORT s) {
		super(s);
		
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<ExportInstance> g, int x1, int y1) {
		
		
		
	
		
		{
			
			final UIPickerRes pop = new UIPickerRes(true) {
				
				@Override
				protected void select(RESOURCE r, int li) {
					g.get().resourceSet(r);
					VIEW.inters().popup.close();
				}
				
				@Override
				protected RESOURCE getResource() {
					return g.get().resource();
				}
				
				@Override
				protected void hoverResource(RESOURCE res, GBox b) {
					
					FACTIONS.player().seller(TR.get(res)).hover(b);
					
					
				}
			};
			
			GuiSection s = new GuiSection();
			
			SPRITE la = new SPRITE.Imp(Icon.M) {
				
				@Override
				public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
					if (g.get().resource() != null)
						g.get().resource().icon().render(r, X1, X2, Y1, Y2);
					else
						SPRITES.icons().m.questionmark.render(r, X1, X2, Y1, Y2);
				}
			};
			
			GButt.ButtPanel b = new GButt.ButtPanel(la) {
				
				@Override
				protected void clickA() {
					VIEW.inters().popup.show(pop, this, true);
				}
				
			};
			b.body.setDim(48);
			
			s.add(new GHeader(Dic.¤¤Exporting));
			s.addRelBody(6, DIR.E, b);
			
			RENDEROBJ r = null;
			r = new GStat() {

				@Override
				public void update(GText text) {
					GFORMAT.iofk(text, g.get().amount, g.get().crates*ExportInstance.crateMax);
				}
				
				@Override
				public void hoverInfoGet(GBox b) {
					b.textLL(Dic.¤¤Inbound);
					b.add(GFORMAT.i(b.text(), g.get().spaceReserved));
					b.NL();
					b.textLL(Dic.¤¤Outbound);
					b.add(GFORMAT.i(b.text(), g.get().amountReserved));
					
					b.sep();
					if (g.get().resource() != null) {
						
						b.textLL(Dic.¤¤Total);
						b.NL(8);
						
						b.textLL(Dic.¤¤Stored);
						b.tab(6);
						b.add(GFORMAT.i(b.text(), blueprint.tally.amount.get(g.get().resource())));
						b.NL();
						b.textLL(Dic.¤¤Capacity);
						b.tab(6);
						b.add(GFORMAT.i(b.text(), blueprint.tally.capacity.get(g.get().resource())));
						b.NL();
						
						
						
					}
					
					
					
					
					
					
				};
			}.hv(Dic.¤¤Stored);
			s.addRightC(32, r);
			
			r = new GStat() {

				@Override
				public void update(GText text) {
					GFORMAT.i(text, g.get().amountReserved);
					
				}
			}.hv(Dic.¤¤Sold);
			s.addRightC(64, r);
			
			
			

			section.addRelBody(2, DIR.S, s);
		}
		
		
		
		
		
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
						selectedSet(g.get().prio());
					}
					
					@Override
					protected void clickA() {
						g.get().prioSet();
					}
					
					@Override
					protected void render(SPRITE_RENDERER r, float ds, boolean isActive, boolean isSelected,
							boolean isHovered) {
						
						super.render(r, ds, isActive, isSelected, isHovered);
						if (g.get().resource() == null || (blueprint.toFetchToExport(g.get().resource()) < 0 || g.get().prio() && g.get().coolFetch > -1)) {
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
						b.text(¤¤prio);
						b.NL();
						
						if (g.get().resource() == null) {
							b.add(b.text().warnify().add(¤¤NoResource));
						}else if (blueprint.toFetchToExport(g.get().resource()) < 0) {
							b.add(b.text().warnify().add(¤¤prioProb));
						}else if (g.get().prio() && g.get().coolFetch > -1) {
							b.add(b.text().warnify().add(MoveDic.¤¤fetchProblem));
						}
						
					}
					
				};
				
				p.body.setDim(48);
				s.addRightC(0, p);
			
			}
			
			s.addRightC(8, new MoveOrderPullUI(g, g, null, ExportInstance.ORDERS));
			
			
			
			
			
			section.addRelBody(2, DIR.S, s);
		}
		

		
		{
			
			UIGoodsExport ex = new UIGoodsExport(false);
			ClickWrap s = new ClickWrap(ex) {
				
				@Override
				protected RENDEROBJ pget() {
					if (g.get().resource() == null)
						return null;
					ex.res.set(g.get().resource().tr());
					return ex;
				}
			};
			section.addRelBody(2, DIR.S, s);
			
		}
		
	}
	
	@Override
	protected void appendTableButt(GuiSection s, GETTER<RoomInstance> ins) {

		s.add(new SPRITE.Imp(Icon.S) {

			@Override
			public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
				RESOURCE ro = ((ExportInstance) ins.get()).resource();
				SPRITE s = ro == null ? SPRITES.icons().s.cancel : ro.icon().small;
				s.render(r, X1, Y1);
			}
		}, 0, s.body().y2());

		s.addRightC(8, new SPRITE.Imp(s.body().width() - 8 - s.getLastX2(), 12) {

			@Override
			public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
				ExportInstance in = (ExportInstance) ins.get();

				double t = in.crates*ExportInstance.crateMax;
				double n = in.amount;
				double i = in.amountReserved;
				GMeter.renderDelta(r, (n-i) / t, (n) / 2, X1, X2, Y1, Y2);
			}
		});

	}
	
	@Override
	protected void problem(ExportInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings) {
		if (i.resource() == null) {
			errors.add(¤¤NoResource);
		}
//		else if (!blueprint.tally.shouldWork(i.resource()))
//			errors.add(¤¤NoSell);
	}
	
	@Override
	protected void hover(GBox box, ExportInstance i) {
		if (i.resource() != null) {
			box.setResource(i.resource(), i.amount, i.crates*ExportInstance.crateMax);
		}
	}

	@Override
	protected void appendMain(GGrid grid, GGrid text, GuiSection sExtra) {
		RENDEROBJ r = null;
		
		r = new GStat() {

			@Override
			public void update(GText text) {
				double am = 0;
				double cap = 0;
				
				for (RESOURCE r : RESOURCES.ALL()) {
					am += blueprint.tally.amount.get(r);
					cap += blueprint.tally.capacity.get(r);
				}
				GFORMAT.percInv(text, am/cap);
				
			}
		}.hh(Dic.¤¤Capacity);
		text.add(r);
		r = new GStat() {

			@Override
			public void update(GText text) {
				int am = 0;
				for (RESOURCE r : RESOURCES.ALL()) {
					am += blueprint.tally.amount.get(r);
				}
				GFORMAT.i(text, am);
				
			}
		}.hh(Dic.¤¤Stored);
		text.add(r);
		r = new GStat() {

			@Override
			public void update(GText text) {
				int am = 0;
				for (RESOURCE r : RESOURCES.ALL()) {
					am += SETT.HALFENTS().caravans.withdrawals(r, null); 
				}
				GFORMAT.i(text, am);
				
			}
		}.hh(Dic.¤¤Outbound);
		
		text.add(r);
		
	}
	
	@Override
	protected void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts,
			LISTE<UIRoomBulkApplier> appliers) {
		final CharSequence none = "--";
		GTSort<RoomInstance> s = new GTSort<RoomInstance>(Dic.¤¤Resource) {

			@Override
			public int cmp(RoomInstance current, RoomInstance cmp) {
				return Dictionary.compare(name(current), name(cmp));
			}

			@Override
			public void format(RoomInstance h, GText text) {
				text.add(name(h));
			}
			
			private CharSequence name(RoomInstance ins) {
				if (ins != null && ins instanceof ExportInstance) {
					ExportInstance i = (ExportInstance) ins;
					if (i.resource() == null)
						return none;
					return i.resource().name;
				}
				return none;
			}
			
		};
		sorts.add(s);
	}
}
