package settlement.room.service.breeder;

import init.type.HCLASSES;
import settlement.entity.ENTETIES;
import settlement.room.industry.module.IndustryUtil;
import settlement.stats.POP;
import settlement.stats.STATS;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.LISTE;
import snake2d.util.sets.Stack;
import snake2d.util.sprite.text.Str;
import util.data.GETTER;
import util.data.INT.INTE;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GGrid;
import util.gui.misc.GHeader;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.gui.slider.GSliderInt;
import util.info.GFORMAT;
import util.text.D;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<BreederInstance, ROOM_BREEDER> {

	private static CharSequence ¤¤limitGlobal = "Global limit";
	private static CharSequence ¤¤limit = "Class limit";
	
	private static CharSequence ¤¤limitGlobalD = "A limit for when to stop producing children, based on total population of all classes and species.";
	private static CharSequence ¤¤limitD = "A limit for when to stop producing children, based on population of current class and species.";
	
	private static CharSequence ¤¤limitGlobalProb = "Your total population of all races exceeds the limit set.";
	private static CharSequence ¤¤limitProb = "Your population of citizens of the current species exceeds the limit set.";
	
	private static CharSequence ¤¤population = "Population";
	private static CharSequence ¤¤incoming = "Incoming";
	private static CharSequence ¤¤total = "Total";
	private static CharSequence ¤¤toBreed = "To Breed";
	static {
		D.ts(Gui.class);
	}
	
	Gui(ROOM_BREEDER s) {
		super(s);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<BreederInstance> getter, int x1, int y1) {
		
		RENDEROBJ rr = new GStat() {
			
			@Override
			public void update(GText text) {
				double n = IndustryUtil.calcProductionRate(blueprint.PRODUCTION_SPEED_DAY, blueprint.productionData, getter.get());
				GFORMAT.fRel(text, n, blueprint.PRODUCTION_SPEED_DAY);
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				IndustryUtil.hoverProductionRate(b, blueprint.PRODUCTION_SPEED_DAY, blueprint.productionData, getter.get());
			};
			
		}.hh(blueprint.race.appearance().iconBig);
		
		section.addRelBody(8, DIR.S, rr);
		
		rr = new GButt.ButtPanel(STATS.MULTIPLIERS().PROSECUTION.name) {
			
			@Override
			protected void clickA() {
				blueprint.prosecute = !blueprint.prosecute;
			}
			
			@Override
			protected void renAction() {
				selectedSet(blueprint.prosecute);
				super.renAction();
			}
			
		};
		section.addRelBody(8, DIR.S, rr);

		
	}
	
	@Override
	protected void appendMain(GGrid icons, GGrid r, GuiSection sExtra) {
		
		{
			GuiSection s = new GuiSection() {
				
				@Override
				public void hoverInfoGet(GUI_BOX text) {
					GBox b = (GBox) text;
					b.title(¤¤limitGlobal);
					b.text(¤¤limitGlobalD);
					b.NL(8);
					
					b.textLL(¤¤population);
					b.tab(7);
					b.add(GFORMAT.i(b.text(), POP.tot()));
					b.NL();
					
					b.textLL(¤¤incoming);
					b.tab(7);
					b.add(GFORMAT.i(b.text(), POP.next()-POP.tot()));
					b.NL();
					
					b.textLL(¤¤total);
					b.tab(7);
					b.add(GFORMAT.i(b.text(), POP.next()));
					b.NL();
					
					b.textLL(¤¤toBreed);
					b.tab(7);
					b.add(GFORMAT.i(b.text(), blueprint.limitTotal-POP.next()));
					b.NL();
					
				}
				
			};
			
			s.add(new GHeader(¤¤limitGlobal));
			
			INTE in = new INTE() {
				
				@Override
				public int min() {
					return 0;
				}
				
				@Override
				public int max() {
					return ENTETIES.MAX;
				}
				
				@Override
				public int get() {
					return CLAMP.i(blueprint.limitTotal, min(), max());
				}
				
				@Override
				public void set(int t) {
					CLAMP.i(t, min(), max());
					blueprint.limitTotal = t;
				}
			};
			s.addRelBody(2, DIR.S, new GSliderInt(in, 200, true));
			
			r.section.addRelBody(0, DIR.S, s);
			
		}
		
		{
			GuiSection s = new GuiSection() {
				
				@Override
				public void hoverInfoGet(GUI_BOX text) {
					GBox b = (GBox) text;
					b.title(¤¤limit);
					b.text(¤¤limitD);
					b.NL(8);
					
					b.textLL(¤¤population);
					b.tab(7);
					b.add(GFORMAT.i(b.text(), POP.tot(HCLASSES.CITIZEN(), blueprint.race)));
					b.NL();
					
					b.textLL(¤¤incoming);
					b.tab(7);
					b.add(GFORMAT.i(b.text(), POP.next(HCLASSES.CITIZEN(), blueprint.race)-POP.tot(HCLASSES.CITIZEN(), blueprint.race)));
					b.NL();
					
					b.textLL(¤¤total);
					b.tab(7);
					b.add(GFORMAT.i(b.text(), POP.next(HCLASSES.CITIZEN(), blueprint.race)));
					b.NL();
					
					b.textLL(¤¤toBreed);
					b.tab(7);
					b.add(GFORMAT.i(b.text(), blueprint.limitSpecies-POP.next(HCLASSES.CITIZEN(), blueprint.race)));
					b.NL();
					
				}
				
			};
			
			s.add(new GHeader(¤¤limit));
			
			INTE in = new INTE() {
				
				@Override
				public int min() {
					return 0;
				}
				
				@Override
				public int max() {
					return ENTETIES.MAX;
				}
				
				@Override
				public int get() {
					return CLAMP.i(blueprint.limitSpecies, min(), max());
				}
				
				@Override
				public void set(int t) {
					CLAMP.i(t, min(), max());
					blueprint.limitSpecies = t;
				}
			};
			s.addRelBody(2, DIR.S, new GSliderInt(in, 200, true));
			
			r.section.addRelBody(0, DIR.S, s);
			
		}
		
		
		
	}
	
	@Override
	protected void hover(GBox box, BreederInstance i) {

		
	}
	
	@Override
	protected void problem(BreederInstance i, Stack<Str> free, LISTE<CharSequence> errors,
			LISTE<CharSequence> warnings) {

		if (POP.next(HCLASSES.CITIZEN(), blueprint.race) >= blueprint.limitSpecies) {
			errors.add(¤¤limitProb);
		}
		
		if (POP.next(null, null) >= blueprint.limitTotal) {
			errors.add(¤¤limitGlobalProb);
		}
			
		super.problem(i, free, errors, warnings);
	}

}
