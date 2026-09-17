


package settlement.room.infra.station;

import game.GAME;
import game.time.TIME;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.settings.S;
import init.sprite.UI.UI;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.room.infra.transport.ROOM_TRANSPORT;
import settlement.room.main.RoomInstance;
import settlement.room.main.employment.RoomEmploymentIns;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.color.OPACITY;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.Hoverable.HOVERABLE.HoverableAbs;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.rnd.RND;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LISTE;
import snake2d.util.sets.Stack;
import snake2d.util.sprite.text.Str;
import util.colors.GCOLOR;
import util.data.GETTER;
import util.data.INT.INTE;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GGrid;
import util.gui.misc.GHeader;
import util.gui.misc.GMeter;
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
import view.main.VIEW;
import view.sett.ui.room.UIRoomBulkApplier;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<StationInstance, ROOM_STATION> {
	
	private static CharSequence ¤¤prepped = "Prepared Deliveries";
	private static CharSequence ¤¤preppedD = "How many transports this station is ready to receive.";
	private static CharSequence ¤¤efficiency = "Efficiency";
	private static CharSequence ¤¤efficiencyD = "Efficiency is based on the number of workers. Each individual becomes more effective when they work together. Carry capacity also affects efficiency.";
	private static CharSequence ¤¤efficiencyD2 = "Current efficiency enables us to accept {0} transport-loads of {1} items each per day, per worker. ({2} items / day per worker)";
	
	private static CharSequence ¤¤accepting = "There is enough space to accept a transport of this resource.";
	private static CharSequence ¤¤acceptingNot = "There is not enough space or preparations to accept a transport of this resource.";
	
	private static CharSequence ¤¤has = "There are transports transporting this resource.";
	private static CharSequence ¤¤hasNot = "There are no active transports currently transporting this resource.";
	
	private static CharSequence ¤¤prob1 = "No resources have been selected.";
	private static CharSequence ¤¤prob2 = "Some selected resources do not have active transports.";
	
	private static CharSequence ¤¤reserved = "Reserved";
	private static CharSequence ¤¤incoming = "Incoming";
	
	private static CharSequence ¤¤prob3 = "Max preparations have been performed. There are not enough deliveries to arrive.";
	static {
		D.ts(Gui.class);
	}

	
	Gui(ROOM_STATION s) {
		super(s);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<StationInstance> g, int x1, int y1) {

		GuiSection s = new GuiSection();
		s.addDownC(0, new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.f(text, g.get().prepared*TIME.secondsPerDayI(), 2);
				text.s();
				text.add('/');
				text.add((int)(g.get().maxPrep()*TIME.secondsPerDayI()));
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				b.title(¤¤prepped);
				b.text(¤¤preppedD);
			};
			
		}.hh(¤¤prepped));
		
		s.addDownC(0, new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.perc(text, g.get().efficiency()*bonus(g.get()));
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				b.title(¤¤efficiency);
				b.text(¤¤efficiencyD);
				b.NL();
				b.add(SETT.ROOMS().STOCKPILE.bonus().icon);
				b.textLL(SETT.ROOMS().STOCKPILE.bonus().name);
				b.tab(6);
				GText t = b.text();
				t.add('x').s();
				GFORMAT.f1(t, bonus(g.get()));
				b.add(t);
				b.NL(8);
				
				t = b.text();
				t.add(¤¤efficiencyD2);
				t.insert(0, g.get().efficiency()*bonus(g.get()), 2);
				t.insert(1, ROOM_TRANSPORT.MAX_LOAD);
				t.insert(2,  (int)(g.get().efficiency()*bonus(g.get())*ROOM_TRANSPORT.MAX_LOAD));
				b.add(t);
			};
			
		}.hh(¤¤efficiency));
		
		if (S.get().developer) {
			s.addDownC(2, new GButt.ButtPanel(Dic.¤¤Accept) {
				@Override
				protected void clickA() {
					int ri = RND.rInt();
					for (int i = 0; i < RESOURCES.ALL().size(); i++) {
						RESOURCE res = RESOURCES.ALL().getC(i+ri);
						if (g.get().tally(res).crates() > 0) {
							g.get().deliver(res, (int) (ROOM_TRANSPORT.MAX_LOAD*RND.rFloat()));
							return;
						}
					}
				}
				
				
			});
			s.addDownC(2, new GButt.ButtPanel("prep") {
				@Override
				protected void clickA() {
					g.get().setPrepared(g.get().maxPrep());
				}
				
				
			});
		}
		
		section.addRelBody(8, DIR.S, s);
		section.addRelBody(8, DIR.S, new Ress(g));
		
	}
	
	double bonusCache = 1;
	int bonusI = -10;
	
	private double bonus(StationInstance i) {
		if (GAME.updateI() != bonusI) {
			bonusI = GAME.updateI();
			double b = 0;
			int am = 0;
			for (Humanoid a : RoomEmploymentIns.employees(i)) {
				am++;
				b += SETT.ROOMS().STOCKPILE.bonus().get(a.indu());
			}
			if (am > 0) {
				bonusCache = b/am;
				bonusCache /= SETT.ROOMS().STOCKPILE.bonus().baseValue;
			}else {
				bonusCache = 1;
			}
		}
		return bonusCache;
	}
	
	@Override
	protected void hover(GBox box, StationInstance i) {
		super.hover(box, i);
		box.sep();
		
		int m = 0;
		box.NL(8);
		
		for (RESOURCE r : RESOURCES.ALL()) {
			
			if (i.tally(r).crates() > 0) {
				
				box.tab((m%3)*5);
				
				box.add(r.icon());
				box.add(GFORMAT.i(box.text(), i.tally(r).stored()));
				
				if (m % 3 == 2) {
					box.NL();
				}
				m++;
				
			}
		}
		box.NL();
		
		box.textLL(¤¤prepped);
		box.tab(6);
		box.add(GFORMAT.f(box.text(), i.prepared*TIME.secondsPerDayI(), 2));
		box.NL();
		box.textLL(¤¤efficiency);
		box.tab(6);
		box.add(GFORMAT.perc(box.text(), i.efficiency()));
		box.NL();
		
		box.NL(8);

	}
	



	@Override
	protected void problem(StationInstance i, Stack<Str> free, LISTE<CharSequence> errors,
			LISTE<CharSequence> warnings) {
		
		
		int ress = 0;
		int err = 0;
		for (RESOURCE res : RESOURCES.ALL()) {
			if (i.tally(res).crates() > 0) {
				ress ++;
				if (!SETT.ROOMS().TRANSPORT.hasActive(res))
					err ++;
			}
			
		}
		
		if (ress == 0)
			errors.add(¤¤prob1);
		
		if (err > 0 && err == ress)
			errors.add(¤¤prob2);
		else if (err > 0)
			warnings.add(¤¤prob2);
		
		if (i.prepared >= i.maxPrep())
			warnings.add(¤¤prob3);

		super.problem(i, free, errors, warnings);
	}
	
	@Override
	protected void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts,
			LISTE<UIRoomBulkApplier> appliers) {
		// TODO Auto-generated method stub
		super.appendTableFilters(filters, sorts, appliers);
		
	}
	
	private static class Ress extends GuiSection{
		
		private ArrayList<RESOURCE> current = new ArrayList<RESOURCE>(RESOURCES.ALL().size());
		private final GETTER<StationInstance> g;
		private final RessPop pop;
		
		Ress(GETTER<StationInstance> g){
			this.g = g;
			pop = new RessPop(g);
			ArrayList<RENDEROBJ> rows = new ArrayList<RENDEROBJ>(RESOURCES.ALL().size());
			
			if (S.get().developer) {
				for (int ri = 0; ri < RESOURCES.ALL().size(); ri++) {
					GuiSection s = new GuiSection(); 
					s.add(new ResLine(ri, g, current));
					final int k = ri;
					s.addRightC(0, new GButt.ButtPanel(UI.icons().s.cog) {
						
						@Override
						protected void clickA() {
							g.get().unreserve(current.get(k));
							super.clickA();
						}
					});
					
					rows.add(s);
				}
			}else {
				for (int ri = 0; ri < RESOURCES.ALL().size(); ri++) {
					rows.add(new ResLine(ri, g, current));
				}
			}
			

			

			
			add(new GScrollRows(rows, 350) {
				
				@Override
				protected boolean passesFilter(int i, RENDEROBJ o) {
					return i < current.size();
				}
				
			}.view());
			
			
			
			addRelBody(8, DIR.N, new GHeader(Dic.¤¤Resources));
			pad(8, 8);
		}
		
		@Override
		public void render(SPRITE_RENDERER r, float ds) {
			current.clearSloppy();
			for (RESOURCE res : RESOURCES.ALL()) {
				if (g.get().tally(res).crates() > 0)
					current.add(res);
			}
			boolean hov = hoveredIs();
			GButt.ButtPanel.renderBG(r, true, false, hov, body());
			GButt.ButtPanel.renderFrame(r, body());
			
			
			super.render(r, ds);
		}
		
		@Override
		public boolean click() {
			if (super.click())
				return true;
			VIEW.inters().popup.show(pop, this);
			return true;
		}
		
	}
	
	private static class ResLine extends HoverableAbs{
		
		private ArrayList<RESOURCE> current;
		private final GETTER<StationInstance> g;
		private final int k;
		
		ResLine(int k, GETTER<StationInstance> g, ArrayList<RESOURCE> current){
			super(300, 32);
			this.g = g;
			this.current = current;
			this.k = k;
			
		}
		
		@Override
		protected void render(SPRITE_RENDERER r, float ds, boolean isHovered) {
			if (k >= current.size())
				return;
			RESOURCE res = current.get(k);
			
			res.icon().renderCY(r, body.x1(), body.cY());
			StationInstance ins = g.get();
			StationTally t = ins.tally(res);
			
			int x2 = body.x2()-48;
			GMeter.render(r, GMeter.C_REDGREEN, (double)t.stored()/t.space(), (double)(t.stored()+ins.incoming(res))/t.space(),  body.x1()+30, x2, body.y1()+4, body.y2()-4);
			
			Str.TMP.clear().add(t.stored());
			int w = UI.FONT().S.width(Str.TMP);
			OPACITY.O50.bind();
			COLOR.BLACK.render(r, x2-w-8, x2, body.y1()+5, body.y2()-5);
			OPACITY.unbind();
			
			UI.FONT().S.renderCY(r, x2-w-4, body.cY(), Str.TMP);
			

			if (ins.accepting(res)) {
				GCOLOR.T().IGOOD.bind();
			}else {
				GCOLOR.T().IBAD.bind();
			}
			UI.icons().s.storage.renderCY(r, x2+8, body.cY());
			
			if (SETT.ROOMS().TRANSPORT.hasActive(res)) {
				GCOLOR.T().IGOOD.bind();
			}else {
				GCOLOR.T().IBAD.bind();
			}
			UI.icons().s.chevron(DIR.E).renderCY(r, x2+24, body.cY());
			COLOR.unbind();
		}
		
		@Override
		public void hoverInfoGet(GUI_BOX text) {
			if (k >= current.size())
				return;
			RESOURCE res = current.get(k);
			StationInstance ins = g.get();
			StationTally t = ins.tally(res);
			GBox b = (GBox) text;
			
			b.title(res.name);
			
			b.textLL(Dic.¤¤Stored);
			b.tab(6);
			b.add(GFORMAT.i(b.text(), t.stored()));
			b.NL();
			
			b.textLL(Dic.¤¤Capacity);
			b.tab(6);
			b.add(GFORMAT.i(b.text(), t.space()));
			b.NL();
			
			b.textLL(¤¤reserved);
			b.tab(6);
			b.add(GFORMAT.i(b.text(), t.reserved()));
			b.NL();
			
			b.textLL(¤¤incoming);
			b.tab(6);
			b.add(GFORMAT.i(b.text(), ins.incoming(res)));
			b.NL();
			
			if (ins.accepting(res)) {
				b.add(b.text().normalify2().add(¤¤accepting));
			}else {
				b.error(¤¤acceptingNot);
			}
			if (S.get().developer)
				b.add(b.text().add(ins.blueprintI().tally(res).accepting()));
			b.NL();
			
			if (SETT.ROOMS().TRANSPORT.hasActive(res)) {
				b.add(b.text().normalify2().add(¤¤has));
			}else {
				b.error(¤¤hasNot);
			}
			b.NL();
			
			super.hoverInfoGet(text);
		}
		
	}
	
	private static class RessPop extends GuiSection{
		
		RessPop(GETTER<StationInstance> g){
			
			GRows rr = new GRows(2);
			
			for (int ri = 0; ri < RESOURCES.ALL().size(); ri++) {
				final int k = ri;
				GuiSection row = new GuiSection();
				row.hoverInfoSet(RESOURCES.ALL().get(k).name);
				row.add(RESOURCES.ALL().get(k).icon(), 0, 0);
				INTE in = new INTE() {
					
					@Override
					public int min() {
						return 0;
					}
					
					@Override
					public int max() {
						
						return g.get().crates.size();
					}
					
					@Override
					public int get() {
						return g.get().tally(RESOURCES.ALL().get(k)).crates();
					}
					
					@Override
					public void set(int t) {
						g.get().allocate(RESOURCES.ALL().get(k), t);
					}
				};
				
				GSliderInt sl = new GSliderInt(in, 100, true);
				row.addRightC(8, sl);
				row.pad(4, 2);
				rr.add(row);
			}
			

			
			add(new GScrollRows(rr.rows(), 650).view());
			addRelBody(8, DIR.N, new GStat() {

				@Override
				public void update(GText text) {
					int am = 0;
					for (RESOURCE r : RESOURCES.ALL())
						am += g.get().tally(r).crates();
					GFORMAT.iofk(text, am, g.get().crates.size());
				}
				
			});
			addRelBody(8, DIR.N, new GHeader(Dic.¤¤Resources));
		}
		
		
	}
	
}
