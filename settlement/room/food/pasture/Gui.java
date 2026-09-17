package settlement.room.food.pasture;

import init.resources.RESOURCES;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.industry.module.RoomBoost;
import settlement.room.main.RoomInstance;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.misc.ACTION;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.LISTE;
import snake2d.util.sets.Stack;
import snake2d.util.sprite.text.Str;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GGrid;
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

class Gui extends UIRoomModuleImp<PastureInstance, ROOM_PASTURE> {

	static CharSequence ¤¤Animals = "¤Animals";
	static CharSequence ¤¤Adults = "¤Adult Animals";
	static CharSequence ¤¤Tending = "¤Tending";
	static CharSequence ¤¤Skill = "¤Skill";
	static CharSequence ¤¤SkillD = "¤Skill that gets put into the tending, multiplying the output.";
	static CharSequence ¤¤BaseRate = "¤Base Rate";
	
	
	static CharSequence ¤¤DailyWork = "¤Daily Tending";
	static CharSequence ¤¤DailyWorkD = "¤Daily Tending is the amount of work needed to keep this pasture functioning. The amount of work needed depends on the amount of animals. If the workers fail to do the tending, animals will start to die. Resets each day.";
	static CharSequence ¤¤SlaughterAll = "¤Slaughter all";
	static CharSequence ¤¤SlaughterAllDesc = "¤Slaughter all animals and immediately receive some produce?";
	
	static CharSequence ¤¤ProdExp = "¤Production of the current day is based on the work of the previous day. The workers must tend to the animals each day. Failing to tend to the animals for one day will result in lower produce. Failing two days in a row results in livestock dying.";
	
	static {
		D.ts(Gui.class);
	}
	
	Gui(ROOM_PASTURE s) {

		super(s);


	}
	
	@Override
	public void hover(GBox box, PastureInstance i) {
		super.hover(box, i);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<PastureInstance> getter, int x1, int y1) {
		
		
		GuiSection s = new GuiSection();
		
		s.addRightC(32, new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.iofkInv(text, getter.get().animalsCurrent, getter.get().animalsMax);
				
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				b.textLL(¤¤Adults);
				b.tab(6);
				b.add(GFORMAT.i(b.text(), getter.get().animalsCurrent-getter.get().animalsCubs));
			};
			
		}.hv(¤¤Animals));
		
		s.addRightC(32, new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.iofkInv(text, CLAMP.i(getter.get().work, 0, getter.get().workMax), getter.get().neededWork(getter.get().animalsCurrent()));
//				text.s().add('(').add(getter.get().neededWork(getter.get().animalsCurrent+1));
//				text.s().add('(').add(getter.get().animalsToDie);
			}
		}.hv(¤¤DailyWork, ¤¤DailyWorkD));
		
		s.addRightC(32, new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.perc(text, getter.get().skill());
				
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				b.title(¤¤Skill);
				b.text(¤¤SkillD);
				b.NL(8);
				IndustryUtil.hoverBoosts(b, 1, null, getter.get().blueprintI().indus.get(0).bonus(), getter.get(), 1);
			};
			
		}.hv(¤¤Skill));
		
		section.addRelBody(8, DIR.S, s);
		
		RENDEROBJ b = new GButt.ButtPanel(¤¤SlaughterAll) {
			private final ACTION yes = new ACTION() {
				
				@Override
				public void exe() {
					getter.get().slaughterAll();
				}
			};
			@Override
			protected void clickA() {
				VIEW.inters().yesNo.activate(¤¤SlaughterAllDesc, yes, ACTION.NOP, true);
			}
			
			@Override
			public void hoverInfoGet(GUI_BOX text) {
				GBox b = (GBox) text;
				double produce = blueprint.slaughterAmount(false, getter.get().industry())*(getter.get().animalsCurrent-getter.get().animalsCubs);
				produce += blueprint.slaughterAmount(true, getter.get().industry())*(getter.get().animalsCubs);
				
				for (IndustryResource r : getter.get().industry().outs()) {
					if (r.resource == RESOURCES.LIVESTOCK())
						continue;
					double am = produce*r.rate;
					
					b.add(r.resource.icon());
					b.text(r.resource.name);
					b.tab(7);
					b.add(GFORMAT.f0(b.text(), am));
					b.NL();
				}
			};
			
		}.hoverInfoSet(¤¤SlaughterAllDesc);
		
		section.addRelBody(8, DIR.S, b);
		
		
	}
	
	@Override
	protected void problem(PastureInstance i, Stack<Str> free, LISTE<CharSequence> errors,
			LISTE<CharSequence> warnings) {
//		if (i.constructor().mustBeIndoors() && i.isolation(i.mX(), i.mY()) < 1) {
//			warnings.add(Dic.¤¤Isolationl)
//		}
		super.problem(i, free, errors, warnings);
	}
	
	@Override
	protected void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts,
			LISTE<UIRoomBulkApplier> appliers) {
		
	}

	public static void industryHoverProductionRate(GBox b, IndustryResource i, RoomInstance ins) {
		PastureInstance ii = (PastureInstance) ins;
		b.NL(8);
		b.text(¤¤ProdExp);
		b.NL(8);
		b.textLL(Dic.¤¤Multipliers);
		b.NL();
		
		b.text(¤¤BaseRate);
		b.tab(6);
		b.add(GFORMAT.f(b.text(), i.rate));
		b.NL();
		
		double prod = i.rate;
		
		for (RoomBoost bb : ii.blueprintI().indus.get(0).boosts()) {
			b.text(bb.info().name);
			b.tab(6);
			b.add(GFORMAT.f1(b.text(), bb.get(ii)));
			b.NL();
			prod *= bb.get(ii);
		}
		
		b.NL(8);
		

		b.textL(Dic.¤¤Total);
		b.tab(6);
		b.add(GFORMAT.f(b.text(), prod));
		b.NL();
		
		
	}



}
