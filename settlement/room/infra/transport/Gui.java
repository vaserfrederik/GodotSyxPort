


package settlement.room.infra.transport;

import static settlement.room.infra.logistics.MoveDic.¤¤fetch;
import static settlement.room.infra.logistics.MoveDic.¤¤fetchD;

import game.GAME;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.settings.S;
import init.sprite.UI.Icon;
import init.sprite.UI.UI;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.room.infra.logistics.MoveDic;
import settlement.room.infra.logistics.MoveOrderPull;
import settlement.room.infra.logistics.MoveOrderPullUI;
import settlement.room.main.RoomInstance;
import settlement.room.main.employment.RoomEmploymentIns;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
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

class Gui extends UIRoomModuleImp<TransportInstance, ROOM_TRANSPORT> {
	
	private static CharSequence ¤¤delivering = "Delivering";
	private static CharSequence ¤¤stored = "Loaded";
	private static CharSequence ¤¤loaded = "To be loaded";
	private static CharSequence ¤¤preparations = "Preparations:";
	
	static CharSequence ¤¤organise = "¤Loading Cart";
	static CharSequence ¤¤preparing = "¤Preparing Cart";

	private static CharSequence ¤¤prepared = "The cart is being prepared";
	private static CharSequence ¤¤loadedD = "The cart is being loaded";
	
	private static CharSequence ¤¤pNoDest = "There are currently no stations that can accept the selected resource!";
	private static CharSequence ¤¤pNoResource = "No resource has been set!";
	
	private static CharSequence ¤¤efficiency = "Efficiency Estimate";
	private static CharSequence ¤¤efficiencyD = "A station network can be highly efficient compared to warehouses and haulers. But, for that to be true, the volume, employees and distance must be enough.";
	
	private static CharSequence ¤¤bonusD = "Current carry capacity boost affecting all logistics workers.";
	private static CharSequence ¤¤workloadD = "The portion of employees that are active.";
	private static CharSequence ¤¤Fetching = "Fetching";
	private static CharSequence ¤¤FetchingD = "The percentage of employees fetching resources vs preparing the cart. These are as efficient as regular warehouse workers.";
	private static CharSequence ¤¤EmployeeEff = "Loading Efficiency";
	private static CharSequence ¤¤EmployeeEffD = "Exponential efficiency gained from the amount of employees, where {0} is optimal.";
	private static CharSequence ¤¤Distance = "Unloading Staions";
	private static CharSequence ¤¤DistanceD = "{0} tiles is the average distance to each targeted unloading station (longer distances are more effective, and each station uses roughly {1} workers to unload a delivery.).";
	private static CharSequence ¤¤totMoved = "Total Hauling";
	private static CharSequence ¤¤totMovedD = "Total amount of resources hauled 100 tiles per day of this loader. ({0} from fetching, {1} from the cart.)";
	private static CharSequence ¤¤Load = "Hauled Per Employee";
	private static CharSequence ¤¤LoadD = "How many resources are hauled 100 tiles on average per employee (including unloader, and fetching).";
	private static CharSequence ¤¤Result = "Total Efficiency";
	private static CharSequence ¤¤ResultD = "Efficiency compared to an average warehouse worker ({0} resources 100 tiles). Less than 100% indicates you're better off with a warehouse.";
	
	static {
		D.ts(Gui.class);
	}

	
	Gui(ROOM_TRANSPORT s) {
		super(s);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<TransportInstance> g, int x1, int y1) {

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
						selectedSet(g.get().prio);
					}
					
					@Override
					protected void clickA() {
						g.get().prio = !g.get().prio;
					}
					
					@Override
					protected void render(SPRITE_RENDERER r, float ds, boolean isActive, boolean isSelected,
							boolean isHovered) {
						
						super.render(r, ds, isActive, isSelected, isHovered);
						if (g.get().prio && g.get().coolFetch > -1) {
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
						if (g.get().prio && g.get().coolFetch > -1) {
							b.add(b.text().warnify().add(MoveDic.¤¤fetchProblem));
						}
						
					}
					
				};
				
				p.body.setDim(48);
				s.addRightC(0, p);
				
				

			
			}
			
			s.addRelBody(8, DIR.E, new MoveOrderPullUI(g, g, null, TransportInstance.ORDERS));
			
			GuiSection pop = new UIPickerRes(true) {
				
				@Override
				protected void select(RESOURCE r, int li) {
					g.get().data.resourceSet(r, g.get());
				}
				
				@Override
				protected RESOURCE getResource() {
					return g.get().data.resource();
				}
			};
			
			SPRITE sp = new SPRITE.Imp(Icon.M) {
				
				@Override
				public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
					RESOURCE res = g.get().data.resource();
					if (res == null)
						UI.icons().m.cancel.render(r, X1, X2, Y1, Y2);
					else
						res.icon().render(r, X1, X2, Y1, Y2);
				}
			};
			
			s.addRelBody(8, DIR.S, new GHeader(Dic.¤¤Resource));
			
			GButt.ButtPanel b = new GButt.ButtPanel(sp) {
				
				@Override
				protected void clickA() {
					VIEW.inters().popup.show(pop, this, true);
				}
				
			};
			b.body.setDim(48);
			
			s.addRelBody(2, DIR.S, b);
			
			section.addRelBody(8, DIR.S, s);	
			
		}
		
		{
			GuiSection s = new GuiSection();
			final double dd = 100;
			s.addDown(0, new GStat() {
				
				@Override
				public void update(GText text) {
					TransportInstance i = g.get();
					
					double bonus = bonus(i);
					double workload = i.employees().efficiency();
					double eff = i.efficiency();
					double workersPerLoad = 1.0/(bonus*workload*eff);
					workersPerLoad += i.stationWorkers;
					double fetching = 1.0-i.fetchTime;
					double warhousefetch = bonus*800*SETT.ROOMS().STOCKPILE.bonus().baseValue/dd;
					double moved = i.distance*fetching*ROOM_TRANSPORT.MAX_LOAD/(workersPerLoad*dd) + (1-fetching)*warhousefetch;
					
					GFORMAT.perc(text, moved/warhousefetch);
					
				}
				
				@Override
				public void hoverInfoGet(GBox b) {
					b.title(¤¤efficiency);
					b.text(¤¤efficiencyD);
					b.sep();
					TransportInstance i = g.get();
					int tab = 7;
					
					double bonus = bonus(i);
					double warhousefetch = 800*bonus*SETT.ROOMS().STOCKPILE.bonus().baseValue/dd;
					
					{
						b.textLL(SETT.ROOMS().STOCKPILE.bonus().name);
						b.tab(tab);
						b.add(GFORMAT.perc(b.text(), bonus -1));
						b.NL();
						b.text(¤¤bonusD);
						b.NL(2);
					}
					double workload = i.employees().efficiency();
					{
						b.textLL(RoomEmploymentIns.¤¤Workload);
						b.tab(tab);
						b.add(GFORMAT.perc(b.text(), workload));
						b.NL();
						b.text(¤¤workloadD);
						b.NL(2);
					}
					double fetching = 1.0-i.fetchTime;
					{
						b.textLL(¤¤Fetching);
						b.tab(tab);
						b.add(GFORMAT.perc(b.text(), i.fetchTime));
						b.NL();
						b.text(¤¤FetchingD);
						b.NL(2);
					}
					double eff = i.efficiency();
					{
						b.textLL(¤¤EmployeeEff);
						b.tab(tab);
						b.add(GFORMAT.perc(b.text(), eff));
						b.NL();
						GText t = b.text();
						t.add(¤¤EmployeeEffD).insert(0, ROOM_TRANSPORT.MAX_EMPLOYEES);
						b.add(t);
						b.NL(2);
					}
					
					
					double dist = i.distance;
					double stationWorkers = i.stationWorkers;
					{
						b.textLL(¤¤Distance);
						b.tab(tab);
						b.add(GFORMAT.perc(b.text(), dist/(dd*i.stationWorkers)));
						b.NL();
						GText t = b.text();
						t.add(¤¤DistanceD).insert(0, (int)dist);
						t.insert(1, i.stationWorkers, 1);
						b.text(t);
						b.NL(2);
					}
					
					double perDayFetch = workload*warhousefetch * (1-fetching);
					double perDayCart = workload*bonus*ROOM_TRANSPORT.MAX_LOAD*eff*fetching*dist/100.0;
					{
						b.textLL(¤¤totMoved);
						b.tab(tab);
						b.add(GFORMAT.i(b.text(), (int)(perDayFetch+perDayCart)*i.employees().employed()));
						b.NL();
						GText t = b.text();
						t.add(¤¤totMovedD);
						t.insert(0, (int) perDayFetch*i.employees().employed());
						t.insert(1, (int) perDayCart*i.employees().employed());
						b.add(t);
						b.NL(2);
					}
					
					
					
					double workersPerLoad = 1.0/(bonus*workload*eff);
					workersPerLoad += stationWorkers;
					
					double moved = dist*fetching*ROOM_TRANSPORT.MAX_LOAD/(workersPerLoad*100) + (1-fetching)*warhousefetch;
					{
						b.textLL(¤¤Load);
						b.tab(tab);
						b.add(GFORMAT.i(b.text(), (int)moved));
						b.NL();
						b.text(¤¤LoadD);
						b.NL(2);
					}
					
					{
						b.sep();
						b.textLL(¤¤Result);
						b.tab(tab);
						double d = moved/warhousefetch;
						b.add(GFORMAT.perc(b.text(), d));
						b.NL();
						GText t = b.text();
						t.add(¤¤ResultD).insert(0, warhousefetch, 1);
						b.add(t);
						b.NL(6);
					}
					
					
					
					

				};
				
			}.increase().hh(Dic.¤¤Efficiency, 200));
			
			s.addDown(16, new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.perc(text, g.get().data.prepD());
					
				}
			}.hh(¤¤preparations, 200));
			
			s.addDown(2, new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.i(text, g.get().data.unloaded());
					
				}
			}.hh(¤¤loaded, 200));
			
			s.addDown(2, new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.iofkInv(text, g.get().data.stored(), ROOM_TRANSPORT.MAX_LOAD);
					
				}
			}.hh(¤¤stored, 200));
			
			s.addDown(2, new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.i(text, g.get().data.delivering());
					
				}
			}.hh(¤¤delivering, 200));
			
			if (S.get().developer) {
				s.addDown(8, new GStat() {
					
					@Override
					public void update(GText text) {
						GFORMAT.i(text, g.get().data.unloadedSpots());
						
					}
				}.hh(dCrate, 200));
			}

			s.addDown(16, new GStat() {
				
				@Override
				public void update(GText text) {
					if (g.get().data.prepD() >= 1) {
						text.add(¤¤prepared);
					}else {
						text.add(¤¤loadedD);
					}
					text.setMaxWidth(300);
					text.setMultipleLines(true);
				}
			});
			
			section.addRelBody(8, DIR.S, s);
		}
		
	}
	
	double bonusCache = 1;
	int bonusI = -10;
	
	private double bonus(TransportInstance i) {
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
	protected void hover(GBox box, TransportInstance i) {
		super.hover(box, i);
		box.sep();
		if (i.resource() != null) {
			box.add(i.resource().icon());
			box.add(GFORMAT.iofkInv(box.text(), i.data.stored(), ROOM_TRANSPORT.MAX_LOAD));
		}
	}
	
	private final String dCrate = "crates to fetch to";
	

	@Override
	protected void problem(TransportInstance i, Stack<Str> free, LISTE<CharSequence> errors,
			LISTE<CharSequence> warnings) {
		if (i.employees().target() == 0)
			return;
		
		{

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
			
			if (i.fetching() && i.coolFetch > -1 && (has && !ok)) {
				errors.add(MoveDic.¤¤pullProblem);
			}
		}

		if (i.resource() == null) {
			errors.add(¤¤pNoResource);
		}else if (i.stationProblem) {
			warnings.add(¤¤pNoDest);
		}
		
		super.problem(i, free, errors, warnings);
	}
	
	@Override
	protected void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts,
			LISTE<UIRoomBulkApplier> appliers) {
		// TODO Auto-generated method stub
		super.appendTableFilters(filters, sorts, appliers);
		
		for (RESOURCE res : RESOURCES.ALL()) {
			filters.add(new GTFilter<RoomInstance>(res.names) {
				
				@Override
				public boolean passes(RoomInstance h) {
					TransportInstance i = (TransportInstance) h;
					if (i.data.resource() == res)
						return true;
					return false;
				}
			});
		}
	}
	
}
