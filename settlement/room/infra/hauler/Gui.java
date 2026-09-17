package settlement.room.infra.hauler;

import static settlement.room.infra.logistics.MoveDic.¤¤fetch;
import static settlement.room.infra.logistics.MoveDic.¤¤fetchD;
import static settlement.room.infra.logistics.MoveDic.¤¤fetching;
import static settlement.room.infra.logistics.MoveDic.¤¤storing;
import static settlement.room.infra.logistics.MoveDic.¤¤storingD;

import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.sprite.UI.Icon;
import init.sprite.UI.UI;
import settlement.room.infra.hauler.HaulerTally.TallyData;
import settlement.room.infra.logistics.MoveDic;
import settlement.room.infra.logistics.MoveOrderPull;
import settlement.room.infra.logistics.MoveOrderPullUI;
import settlement.room.infra.logistics.MoveOrderPullersUI;
import settlement.room.main.RoomInstance;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.renderable.RENDEROBJ;
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
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.gui.table.GTableSorter.GTFilter;
import util.gui.table.GTableSorter.GTSort;
import util.info.GFORMAT;
import view.sett.ui.room.UIRoomBulkApplier;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<HaulerInstance, ROOM_HAULER> {
	
	
	Gui(ROOM_HAULER s) {
		super(s);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<HaulerInstance> g, int x1, int y1) {

		
		{
			GuiSection s = new GuiSection();
			int i = 0;
			

			for (TallyData d : blueprint.tally.datas){
				s.addGridD(new GStat() {

				@Override
				public void update(GText text) {
					GFORMAT.i(text, d.get(g.get()));
					
				}
				}.hv(d.name), i++, 2, 160, 32, DIR.N);
			}
			section.addRelBody(8, DIR.S, s);
				
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
						if (g.get().prio() && g.get().coolFetch > -1) {
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
						if (g.get().prio() && g.get().coolFetch > -1) {
							b.add(b.text().warnify().add(MoveDic.¤¤fetchProblem));
						}
						
					}
					
				};
				
				p.body.setDim(48);
				s.addRightC(0, p);
			
			}
			
			MoveOrderPullUI ui = new MoveOrderPullUI(g, g, null, HaulerInstance.ORDERS);
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

		section.addRelBody(8, DIR.S, new UIPickerRes(true) {
			
			@Override
			protected void select(RESOURCE r, int li) {
				g.get().setResource(r);
			}
			
			@Override
			protected RESOURCE getResource() {
				return g.get().resource();
			}
		});
		
	}
	
	@Override
	protected void appendTableButt(GuiSection s, GETTER<RoomInstance> ins) {

		s.add(new SPRITE.Imp(Icon.M) {

			@Override
			public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
				HaulerInstance in = (HaulerInstance) ins.get();
				RESOURCE res = in.resource();
				SPRITE ico = res == null ? UI.icons().m.cancel : res.icon();
				ico.render(r, X1, X2, Y1, Y2);
			}
		}, 0, s.body().y2());

	}

	@Override
	protected void appendMain(GGrid icons, GGrid text, GuiSection sExtra) {
		for (TallyData d : blueprint.tally.datas){
			text.add(new GStat() {

				@Override
				public void update(GText text) {
					GFORMAT.i(text, d.total(null));
					
				}
			}.hh(d.name));
		}
	}
	
	
	@Override
	protected void problem(HaulerInstance i, Stack<Str> free, LISTE<CharSequence> errors,
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
	protected void hover(GBox box, HaulerInstance i) {
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
		box.NL(8);
		
		if (i.resource() != null) {
			box.add(i.resource().icon());
			box.NL();
			for (TallyData d : blueprint.tally.datas) {
				box.textLL(d.name);
				box.tab(7);
				box.add(GFORMAT.i(box.text(), d.get(i)));
				box.NL();
				
			}
			
		}
		
	}

	@Override
	protected void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts,
			LISTE<UIRoomBulkApplier> appliers) {
		super.appendTableFilters(filters, sorts, appliers);
		
		for (RESOURCE res : RESOURCES.ALL()) {
			filters.add(new GTFilter<RoomInstance>(res.names) {
				
				@Override
				public boolean passes(RoomInstance h) {
					HaulerInstance i = (HaulerInstance) h;
					if (i.resource() == res)
						return true;
					return false;
				}
			});
		}
	}
	
}
