package settlement.room.knowledge.school;

import game.boosting.BOOSTABLE_O;
import game.boosting.Boostable;
import init.sprite.UI.Icon;
import init.type.HCLASSES;
import init.type.HCLASS_RACE;
import settlement.entity.humanoid.Humanoid;
import settlement.room.industry.module.IndustryRate;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.industry.module.RoomBoost;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.stats.STATS;
import settlement.stats.colls.StatsEducation;
import settlement.stats.colls.StatsEducation.AgeType;
import settlement.stats.colls.StatsEducation.StatEducation;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import snake2d.util.sets.LinkedList;
import snake2d.util.sets.Stack;
import snake2d.util.sprite.text.Str;
import util.data.GETTER;
import util.data.INT.INTE;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GGrid;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.gui.slider.GSliderInt;
import util.gui.table.GScrollRows;
import util.gui.table.GTableSorter.GTFilter;
import util.gui.table.GTableSorter.GTSort;
import util.info.GFORMAT;
import util.info.INFO;
import util.text.D;
import util.text.Dic;
import view.sett.ui.room.UIRoomBulkApplier;
import view.sett.ui.room.UIRoomModule;

public abstract class RoomEducationHelper{

	private static CharSequence ¤¤daysToEducate = "¤Days to Educate";
	private static CharSequence ¤¤boost = "¤Species Boost";
	static {
		D.ts(RoomEducationHelper.class);
	}
	
	private final Boostable bonus;
	private final IndustryRate rate;
	private final RoomBlueprintIns<?> blue;
	
	public RoomEducationHelper(RoomBlueprintIns<?> blue, RoomBoost... boosts) {
		this.bonus = blue.bonus();
		this.blue = blue;

		
		RoomBoost deg = new RoomBoost() {
			
			INFO info = new INFO(Dic.¤¤Degrade, Dic.¤¤DegradeDesc);
			
			@Override
			public INFO info() {
				return info;
			}
			
			@Override
			public double get(RoomInstance r) {
				return (1.0-r.getDegrade());
			}
		};
		
		this.rate = new IndustryRate() {
			
			private final LIST<RoomBoost> boos = new ArrayList<RoomBoost>(boosts).join(deg);
			
			@Override
			public LIST<RoomBoost> boosts() {
				return boos;
			}
			
			@Override
			public Boostable bonus() {
				return bonus;
			}
		};;
	}
	
	public abstract AgeType type();
	
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new UIRoomModule() {
			
			
			
			@Override
			public void appendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1) {
				
				section.addRelBody(8, DIR.S, new GStat() {
					
					@Override
					public void update(GText text) {
						GFORMAT.f0(text, learningSpeed(get.get()));
					}
					
					@Override
					public void hoverInfoGet(GBox b) {
						b.title(bonus.name);
						b.text(bonus.desc);
						b.NL();
						
						IndustryUtil.hoverProductionRate(b, 1, rate, get.get());
//						
//						double v = IndustryUtil.calcProductionRate(1, rate, get.get());
//						
//						for (RoomBoost bo : rate.boosts()) {
//							b.textLL(bo.info().name);
//							b.tab(6);
//							b.add(b.text().add('*').add(v), 2);
//							b.NL();
//							
//						}
//						
//						bonus.hover(b, HCLASS_RACE.clP(), true);
					}
					
				}.hh(bonus.icon));
				
			}
			
			@Override
			public void appendManageScr(GGrid icons, GGrid text, GuiSection extra) {
				LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();
				
				for (HCLASS_RACE rr : HCLASS_RACE.ALL()) {
					if (rr.race == null)
						continue;
					if (rr.race.bvalue(bonus) == 0)
						continue;
					if (rr.cl == HCLASSES.CITIZEN())
						rows.add(new RaceRow(rr, bonus, type()));
				}
				
				text.add(new GScrollRows(rows, rows.get(0).body().height()*5).view());
			}
			
			@Override
			public void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters,
					LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers) {
				
			}
			@Override
			public void appendButt(GuiSection s, GETTER<RoomInstance> get) {
				
			}
			
			@Override
			public void hover(GBox box, Room room, int rx, int ry) {
				
			}
			

			@Override
			public void problem(Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings, Room room, int rx, int ry) {
				
			}
			
			
			
		});
	}
	
	private static class RaceRow extends GuiSection{
		
		RaceRow(HCLASS_RACE race, Boostable bonus, AgeType type){
			GuiSection s = new GuiSection() {
				@Override
				public void hoverInfoGet(GUI_BOX text) {
					GBox b = (GBox) text;
					
					b.textLL(race.race.info.names);
					b.textL(race.cl.names);
					b.NL();

					type.hoverLimit(text, race);
					b.NL(8);
					
					b.textLL(¤¤boost);
					b.tab(6);
					double am = race.race.bvalue(bonus);
					b.add(GFORMAT.f(b.text(), am));
					b.NL();
					
					b.textLL(¤¤daysToEducate);
					b.tab(6);
					am = type.limit(race)/(bonus.get(race)*type.limitSpeed(race));
					b.add(GFORMAT.f(b.text(), am));
					b.NL();
				}
			};
			
			s.addRightC(0, race.race.appearance().icon);
			s.addRightC(-8, race.cl.iconSmall());
			s.addRightC(4, new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.i(text, (long) Math.ceil(type.limit(race)/(bonus.get(race)*type.limitSpeed(race))));
				}
			});
			
			INTE ii = new INTE() {
				
				@Override
				public int min() {
					return 0;
				}
				
				@Override
				public int max() {
					return StatsEducation.LIMIT_MAX;
				}
				
				@Override
				public int get() {
					return type.limit(race);
				}
				
				@Override
				public void set(int t) {
					type.limitSet(race, t);
				}
			};
			
			s.addRightC(60, new GSliderInt(ii, 120, true, false));
			add(s);
			body().incrW(16);
			
			for (StatEducation ss : STATS.EDUCATION().all) {
				addRightC(8, new GButt.ButtPanel(ss.total.info().icon.resized(Icon.S)) {
					
					@Override
					protected void renAction() {
						selectedSet(STATS.EDUCATION().policy(race) == ss);
					}
					
					@Override
					protected void clickA() {
						STATS.EDUCATION().policySet(race, ss);
					}
					
					@Override
					public void hoverInfoGet(GUI_BOX text) {
						ss.total.hover(text, race.cl, race.race);
					}
				});
			}
			
			pad(4, 1);
		}
		
	}

	public double learningSpeed(Humanoid student, int tx, int ty) {
		RoomInstance ins = blue.get(tx, ty);
		if (ins == null)
			return 0;
		return learningSpeed(ins, student.indu());
	}
	
	public double learningSpeed(RoomInstance ins, BOOSTABLE_O h) {
		
		double d = 1.0;
		for (RoomBoost rr : rate.boosts()) {
			d *= rr.get(ins);
		}
		
		return bonus.get(h)*d;
	}
	
	public double learningSpeed(RoomInstance ins) {
		double ee = ins.employees().employed();
		if (ee == 0)
			return bonus.get(HCLASS_RACE.clP());
		return IndustryUtil.calcProductionRate(1, rate, ins);
	}
	
}
