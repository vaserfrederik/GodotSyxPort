package settlement.room.industry.module.consumption;

import game.time.TIME;
import init.resources.RESOURCE;
import init.sprite.UI.UI;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.industry.module.ROOM_IDATA_INSTANCE;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.Hoverable.HOVERABLE;
import snake2d.util.sets.LISTE;
import snake2d.util.sprite.SPRITE;
import util.colors.GCOLOR;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GChart;
import util.gui.misc.GGrid;
import util.gui.misc.GHeader;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.gui.table.GTableSorter.GTFilter;
import util.gui.table.GTableSorter.GTSort;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;
import util.text.DicTime;
import view.sett.ui.room.ModuleIndustry;
import view.sett.ui.room.UIRoomBulkApplier;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

public class ConsumptionGui<A extends RoomInstance, B extends RoomBlueprintIns<A>> extends UIRoomModuleImp<A, B> {
	
	private static CharSequence ¤¤use = "If usage is enabled, the employees produce x{0} as much.";
	private static CharSequence ¤¤ConsumedDay = "¤Consumed today";
	private static CharSequence ¤¤ConsumedNow = "¤Consumed This Year";
	private static CharSequence ¤¤ConsumedYEsterday = "¤Consumed Yesterday";
	private static CharSequence ¤¤ConsumedPrevious = "¤Consumed last year";
	private static CharSequence ¤¤ConsumptionD = "¤Estimation of how many resources are consumed each day.";
	private static CharSequence ¤¤Consumption = "¤Consumption";
	
	private static CharSequence ¤¤Stored = "¤Stored";
	private static CharSequence ¤¤Incoming = "¤Fetchers";
	
	static {
		D.ts(ConsumptionGui.class);
	}
	
	private final RoomConsumption cons;
	private final GChart chart = new GChart();
	
	public ConsumptionGui(B s, RoomConsumption cons) {
		super(s);
		this.cons = cons;
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<A> getter, int x1, int y1) {
		
		GuiSection t = new GuiSection();

		for (IndustryResource rr : cons.ins()) {
			
			
			SPRITE sp = new SPRITE.Imp(48, 64) {
				GText t = new GText(UI.FONT().S, 8);
				@Override
				public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
					t.clear();
					
					RoomInstance ins = getter.get();
					ROOM_IDATA_INSTANCE insi = (ROOM_IDATA_INSTANCE) getter.get();
					
					double am = ins.employees().employed()*IndustryUtil.calcConsumptionRate(rr.rate, ins, cons);	
					if (cons.stored(rr).get(insi) <= 0)
						am *= 1 + cons.boost(rr);
					
					
					GFORMAT.i(t, -(int) am);
					rr.resource.icon().renderC(r, X1, X2, Y1, Y1+32);
					t.adjustWidth();
					t.renderC(r, X1, X2, Y2-32, Y2);
					
					ROOM_IDATA_INSTANCE iins = (ROOM_IDATA_INSTANCE) getter.get();
					if (!iins.getWork().resourceReachable(rr.resource)){
						GCOLOR.T().IBAD.bind();
						UI.icons().s.alert.render(r, X1+4, Y1+4);
						COLOR.unbind();
					}
					
				}
			};
			
			HOVERABLE h;
			
			h = new GButt.ButtPanel(sp) {
				
				@Override
				protected void clickA() {
					
					cons.enabledToggle(rr, (ROOM_IDATA_INSTANCE) getter.get(), getter.get());
				}
				
				@Override
				protected void renAction() {
					selectedSet(cons.enabled(rr, (ROOM_IDATA_INSTANCE) getter.get()));
				}
				
				@Override
				public void hoverInfoGet(GUI_BOX text) {
					GBox b = (GBox) text;
					GText t = b.text();
					t.add(¤¤use);
					t.insert(0, 1 + cons.boost(rr), 2);
					b.add(t);
					b.NL();
					
					b.textLL(¤¤Stored);
					b.tab(6);
					b.add(GFORMAT.i(b.text(), cons.stored(rr).get((ROOM_IDATA_INSTANCE) getter.get())));
					b.NL();
					b.textLL(¤¤Incoming);
					b.tab(6);
					b.add(GFORMAT.i(b.text(), cons.reseved(rr).get((ROOM_IDATA_INSTANCE) getter.get())));
					b.NL();
					
					
					
					b.sep();
					
					
					
					hoverConsumptionIns(text, rr, getter.get(), cons);
					
					
					
				}
				
			};
			
			
			t.addRightC(0, h);
			
		}
		
		t.addRelBody(30, DIR.E, ModuleIndustry.makeFetch(getter));
		
		section.addRelBody(8, DIR.S, t);
		
		
	}
	
	
	@Override
	protected void appendMain(GGrid r, GGrid text, GuiSection sExtra) {

		for (IndustryResource rr : cons.ins()) {
			
			GStat s = new GStat() {
				@Override
				public void update(GText text) {
					int am = rr.history().getPeriodSum(-(int)TIME.years().bitConversion(TIME.days()), 0);
					GFORMAT.iIncr(text, -am);
				}
			};
			
			r.add(new GHeader.HeaderHorizontal(rr.resource.icon(), s) {
				@Override
				public void hoverInfoGet(GUI_BOX text) {

					hoverConsumption(text, rr, cons, chart);
					
					
				}
			});
			
		}

		
	}
	
	public static void hoverConsumptionIns(GUI_BOX text, IndustryResource i, RoomInstance ins, RoomConsumptionAbs cons) {
		
		GBox b = (GBox) text;
		b.title(i.resource.name);

		ROOM_IDATA_INSTANCE iins = (ROOM_IDATA_INSTANCE) ins;
		if (!iins.getWork().resourceReachable(i.resource)) {
			b.error(Dic.¤¤Unreachable);
			b.NL();
		}
		
		ROOM_IDATA_INSTANCE p = (ROOM_IDATA_INSTANCE) ins;
		
		
		b.NL(8);
		b.textLL(¤¤ConsumedDay);
		b.tab(7);
		b.add(GFORMAT.i(b.text(), (int)i.day.getD(p)));
		b.NL();

		b.textLL(¤¤ConsumedYEsterday);
		b.tab(7);
		b.add(GFORMAT.i(b.text(), i.dayPrev.get(p)));
		
		b.NL(0);
		b.textLL(¤¤ConsumedNow);
		b.tab(7);
		b.add(GFORMAT.i(b.text(), i.year.get(p)));
		b.NL();

		b.textLL(¤¤ConsumedPrevious);
		b.tab(7);
		b.add(GFORMAT.i(b.text(), i.yearPrev.get(p)));

		b.NL(8);
		
		b.textLL(¤¤ConsumptionD);
		b.NL();
		
		IndustryUtil.hoverConsumptionRate(text, i.rate, ins, cons);
	}
	
	public static void hoverConsumption(GUI_BOX text, IndustryResource rr, RoomConsumptionAbs cons, GChart chart) {
		
		GBox b = (GBox) text;
		
		RESOURCE res = rr.resource;
		
		b.title(res.name);
		b.add(text.text().add(¤¤Consumption).s().add('(').add(Dic.¤¤Total).add(')'));
		b.NL(4);
		
		b.textLL(¤¤ConsumedDay);
		b.tab(7);
		b.add(GFORMAT.i(b.text(), rr.history().get(0)));
		b.NL();
		
		b.textLL(¤¤ConsumedYEsterday);
		b.tab(7);
		b.add(GFORMAT.i(b.text(), rr.history().get(1)));
		b.NL();
		
		int yearStart = (int) (-TIME.days().bitOfYear()*TIME.years().bitConversion(TIME.years()));
		
		b.textLL(¤¤ConsumedNow);
		b.tab(7);
		b.add(GFORMAT.i(b.text(), rr.history().getPeriodSum(-yearStart, 0)));
		b.NL();
		

		
		b.textLL(¤¤ConsumedPrevious);
		b.tab(7);
		b.add(GFORMAT.i(b.text(), rr.history().getPeriodSum(-(int)TIME.years().bitConversion(TIME.days())-yearStart, -(int)yearStart)));
		b.NL();
		
		b.NL(8);
		b.textLL(DicTime.¤¤Days);
		chart.clear();
		chart.add(rr.history());
		text.NL();
		text.add(chart.sprite);
	}
	
	@Override
	protected void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts,
			LISTE<UIRoomBulkApplier> appliers) {
		
	}
	
	@Override
	protected void hover(GBox box, A ins) {
		super.hover(box, ins);
		int t = 0;
		for (IndustryResource i : cons.ins()) {
			
			box.tab(t * 3);
			box.add(i.resource.icon().small);
//			GText te = box.text();
//			GFORMAT.i(te, -(int)cons.consumptionRate(i, ins));
//			//indu.industryFormatConsumptionRate(te, i, (RoomInstance) room);
//			box.add(te);
			t++;
			if (t == 3) {
				t = 0;
				box.NL();
			}
		}
		
		box.NL(8);
	}
	
	

}
