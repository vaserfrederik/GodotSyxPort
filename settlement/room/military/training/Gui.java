package settlement.room.military.training;

import init.type.HCLASS_RACE;
import settlement.entity.ENTETIES;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.main.RoomInstance;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GuiSection;
import util.data.DOUBLE;
import util.data.GETTER;
import util.data.INT.INTE;
import util.gui.misc.GBox;
import util.gui.misc.GGrid;
import util.gui.misc.GHeader;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.gui.slider.GGaugeMutable;
import util.gui.slider.GSliderInt;
import util.info.GFORMAT;
import util.text.D;
import view.sett.ui.room.UIRoomModule;

final class Gui extends UIRoomModule{
	
	private static CharSequence ¤¤Limit = "¤Recruits limit";
	private static CharSequence ¤¤LimitD = "¤The number of recruits that you allow to train simultaneously.";
	
	private static CharSequence ¤¤speed = "¤Training Speed";
	private static CharSequence ¤¤speedD = "¤The speed at which subjects are trained.";
	private static CharSequence ¤¤maxLevel = "¤Days to reach max level: ";
	
	static {
		D.ts(Gui.class);
	}

	private final ROOM_M_TRAINER<?> blueprint;
	
	protected Gui(ROOM_M_TRAINER<?> blueprint) {
		this.blueprint = blueprint;
	}
	
	@Override
	public void appendPanel(GuiSection section, GETTER<RoomInstance> getter, int x1, int y1) {
		
		INTE t = new INTE() {
			
			@Override
			public int min() {
				// TODO Auto-generated method stub
				return 0;
			}
			
			@Override
			public int max() {
				return getter.get().employees().max();
			}
			
			@Override
			public int get() {
				return getter.get().employees().target();
			}
			
			@Override
			public void set(int t) {
				getter.get().employees().neededSet(t);
			}
		};
		
		GGaugeMutable m = new GGaugeMutable(t, 220) {
			@Override
			protected int setInfo(DOUBLE d, GText text) {
				GFORMAT.i(text, t.get());
				return 48;
			}
		};
		m.hoverInfoSet(¤¤LimitD);
		
		section.addRelBody(8, DIR.S, new GHeader(¤¤Limit).hoverInfoSet(¤¤LimitD));
		section.addRelBody(4, DIR.S, m);
		
		GStat ss = new GStat() {
			
			@Override
			public void update(GText text) {
				
				GFORMAT.perc(text, get());
			}
			
			double get() {
				return IndustryUtil.calcProductionRate(1, null, blueprint.bonus(), getter.get());
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				b.title(¤¤speed);
				b.text(¤¤speedD);
				
				IndustryUtil.hoverProductionRate(b, 1, null, blueprint.bonus(), getter.get());	

				b.NL(8);
				b.textLL(¤¤maxLevel);

				double d = get();
				int am = (int) Math.ceil(blueprint.TRAINING_DAYS/d);
				
				b.add(GFORMAT.i(b.text(), am));
			}
		};
		
		section.addRelBody(8, DIR.S, ss.hv(¤¤speed));
		
	}
	
	@Override
	public void appendManageScr(GGrid icons, GGrid text, GuiSection extra) {
		
//		icons.add(new GStat() {
//
//			@Override
//			public void update(GText text) {
//				int emp = blueprint.employment().employed();
//				if (emp == 0)
//					return;
//				GFORMAT.perc(text, emp*blueprint.employment().accidentsPerYear / (BOOSTABLES.CIVICS().ACCIDENT.get(POP_CL.clP())), 2);
//				
//			}
//		}.hh(SPRITES.icons().s.death).hoverTitleSet(Dic.¤¤AccidentRate).hoverInfoSet(Dic.¤¤AccidentRateD));
		
		GuiSection s = new GuiSection();
		
		INTE t = new INTE() {
			
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
				return blueprint.trainingLimit;
			}
			
			@Override
			public void set(int t) {
				blueprint.trainingLimit = t;
			}
		};
		
		s.addRelBody(0, DIR.S, new GHeader(¤¤Limit).hoverInfoSet(¤¤LimitD));
		s.addRelBody(0, DIR.S, new GSliderInt(t, 200, true));
		
		GStat ss = new GStat() {
			
			@Override
			public void update(GText text) {
				
				GFORMAT.perc(text, get());
			}
			
			double get() {
				double d = blueprint.bonus().get(HCLASS_RACE.clP(null, null));
				return d;
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				b.title(¤¤speed);
				b.text(¤¤speedD);
				b.NL(4);
				blueprint.bonus().hover(b, HCLASS_RACE.clP(null, null), true);
				

				b.NL(8);
				b.textLL(¤¤maxLevel);

				double d = get();
				int am = (int) Math.ceil(blueprint.TRAINING_DAYS/d);
				
				b.add(GFORMAT.i(b.text(), am));
			}
		};
		
		s.addRelBody(8, DIR.S, ss.hv(¤¤speed));
		
		text.section.addRelBody(0, DIR.S, s);
		
		super.appendManageScr(icons, text, extra);
	}
	

}
