package settlement.room.infra.admin;

import java.io.IOException;

import game.GAME;
import game.boosting.BOOSTING;
import game.boosting.BSourceInfo;
import game.boosting.BValue;
import game.boosting.Boostable;
import game.boosting.BoosterValue;
import game.faction.npc.FactionNPC;
import game.faction.player.Player;
import game.time.TIME;
import init.settings.S;
import settlement.entity.humanoid.Humanoid;
import settlement.room.industry.module.IndustryRate;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.employment.RoomEmployment;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.Json;
import snake2d.util.file.SAVABLE;
import snake2d.util.gui.GuiSection;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.LISTE;
import snake2d.util.sprite.text.Str;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GChart;
import util.gui.misc.GGrid;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.gui.table.GTableSorter.GTFilter;
import util.gui.table.GTableSorter.GTSort;
import util.info.GFORMAT;
import util.statistics.HistoryInt;
import util.text.D;
import util.text.Dic;
import view.sett.ui.room.UIRoomBulkApplier;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

public class AdminData implements SAVABLE {
	
	private static CharSequence ¤¤degradeD = "Degrade per year. Each worker can only produce and maintain so much {0}. Without worker effort, the value will start to degrade with time.";
	private static CharSequence ¤¤TargetD = "¤Estimation of how much {0} will be produced.";
	
	static {
		D.ts(AdminData.class);
	}
	public final HistoryInt utilizedHistory = new HistoryInt(64, TIME.days(), true);
	public final Boostable target;
	
	public final double knowledgePerStation;
	private final double degradeValue;
	private final double workSpeed;
	private final double workValue;
	private final RoomEmployment emps;
	final Boostable boost;
	private int stations;
	private double prev = 0;
	private double dayGain = 0;
	private int day = -1;
	
	public byte usedD = 0;
	private double progress = 0;
	private double workProg = 1;
	
	private double skill;
	private double skillAmount;
	private int skillUpI = -1;
	
	
	
	public AdminData(RoomEmployment emps, Json json, Boostable boost){
		this.emps = emps;
		this.boost = boost;
		knowledgePerStation = json.d("VALUE_PER_WORKER", 0, 100000);
		double degrade = json.d("VALUE_DEGRADE_PER_YEAR", 0, 10);
		degradeValue = degrade/TIME.years().bitConversion(TIME.days());
		workSpeed = json.d("VALUE_WORK_SPEED", 0, 1000);
	
		double work = knowledgePerStation*degrade;
		work /= TIME.years().bitSeconds();
		work *= Humanoid.WORK_PER_DAYI;
		
		workValue = work;
		
		if (json.has(BOOSTING.MAP().key)) {
			target = BOOSTING.MAP().read(json);
		}else
			target = null;
		
		
		
		if (target != null) {
			final double max = knowledgePerStation*1000000;
			final double maxI = 1.0/max;
			
			BValue v = new BValue.BValuePlayerOnly() {
				
				@Override
				public double vGet(Player f) {
					return value()*maxI;
				}

				@Override
				public double vGet(FactionNPC f) {
					return 0;
				}
			};

			new BoosterValue(v, new BSourceInfo(emps.blueprint().info.names, emps.blueprint().iconBig().small), 0, max, false).add(target);
		}
	}
	
	@Override
	public void save(FilePutter file) {
		utilizedHistory.save(file);
		file.d(prev);
		file.d(dayGain);
		file.i(day);
		file.d(skill);
		file.d(skillAmount);
		file.i(skillUpI);
		file.i(stations);
	}

	@Override
	public void load(FileGetter file) throws IOException {
		utilizedHistory.load(file);
		prev = file.d();
		dayGain = file.d();
		day = file.i();
		skill = file.d();
		skillAmount = file.d();
		skillUpI = file.i();
		stations = file.i();
		setProgress();
	}

	@Override
	public void clear() {
		utilizedHistory.clear();
		prev = 0;
		dayGain = 0;
		day = 0;
		stations = 0;
	}
	
	public void incStations(int am) {
		this.stations += am;
	}
	
	public void perform(double time, double skill){

		double progSpeed = workProg;
		
		progSpeed = (progSpeed)*workSpeed;
		double am = time*skill*progSpeed*workValue;
		dayGain += am;
		this.skill += skill*emps.proximity();
		
		skillAmount ++;
		
		int emp = emps.employed();
		
		if (skillAmount > emp) {
			skillUpI = GAME.updateI();
			skillAmount /= 2;
			this.skill /= 2;
			
		}
		
		utilizedHistory.set((int)value());
		setProgress();
	}
	
	public void inc(double am){

		dayGain += am;
		utilizedHistory.set((int)value());
	}
	
	public void update() {
		if (day != TIME.days().bitsSinceStart()) {
			day = TIME.days().bitsSinceStart();
			prev += dayGain;
			prev *=(1-degradeValue);
			dayGain = 0;
			utilizedHistory.set((int)value());
		}
	}
	
	public double value() {
		double vv = (prev+dayGain)*(1-degradeValue);
		vv = Math.max(prev, vv);
//		long v = (long) (vv/knowledgePerStation);
		
		
//		if (v > 1000000) {
//			v = 1000*(v / 1000);
//		}else if (v > 100000) {
//			v = 100*(v / 100);
//		}else if (v > 1000) {
//			v = 10*(v / 10);
//		}else if (v > 500)
//			v = 5*(v / 5);
//		if (vv > 500)
//			vv = v*knowledgePerStation;
		return vv;
	}

	
	public double projection() {
		return skill()*emps.employed()*knowledgePerStation*emps.totEff()/emps.proximity();
	}
	
	public double perEmployee() {
		if (emps.employed() == 0)
			return knowledgePerStation;
		return skill()*knowledgePerStation*emps.totEff()/emps.proximity();
	}
	
	public double skill() {
		if (skillAmount == 0)
			return 1;
		return this.skill/skillAmount;
	}
	
	private void setProgress() {
		progress = value()/(skill()*(emps.neededWorkers())*knowledgePerStation);
		progress = CLAMP.d(progress, 0, 1);
		workProg = 1-progress*progress;
		usedD = (byte) (progress*255);

	}
	
	public static class Gui<A extends RoomInstance, B extends RoomBlueprintIns<A>> extends UIRoomModuleImp<A, B> {

		private final GChart chart = new GChart();
		private final AdminData data;
		private final IndustryRate indu;
		private final CharSequence name;
		private final CharSequence targetD;
		
		public Gui(B s, AdminData data, IndustryRate out, CharSequence name, CharSequence targetD) {
			super(s);
			this.data = data;
			this.indu = out;
			this.name = name;
			this.targetD = targetD;
		}
		
		public Gui(B s, AdminData data, IndustryRate out) {
			super(s);
			this.data = data;
			this.indu = out;
			this.name = data.target.name;
			this.targetD = "" + Str.TMP.clear().add(¤¤TargetD).insert(0, name);
		}

		@Override
		protected void appendPanel(GuiSection section, GGrid grid, GETTER<A> getter, int x1, int y1) {
			
			section.addRelBody(8, DIR.S, new GStat() {
				
				@Override
				public void update(GText text) {
					double p = getter.get().employees().employed()*IndustryUtil.calcProductionRate(data.knowledgePerStation, indu, data.boost, getter.get());
					GFORMAT.f0(text, p);
				}
				@Override
				public void hoverInfoGet(GBox b) {
					
					b.text(targetD);
					b.NL(8);
					IndustryUtil.hoverProductionRate(b, data.knowledgePerStation, indu, data.boost, getter.get());
				};
				
			}.hv(Dic.¤¤Target));
			
		}
		
		@Override
		protected void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts,
				LISTE<UIRoomBulkApplier> appliers) {
			
		}
		
		@Override
		protected void hover(GBox box, A i) {
			super.hover(box, i);
			box.NL(8);
			box.textLL(name);
			box.add(GFORMAT.i(box.text(), (int)data.value()));
		}


		@Override
		protected void appendMain(GGrid r, GGrid text, GuiSection sExtra) {
			
			GuiSection ss = new GuiSection();
			
			ss.add(new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.f0(text, data.value(), 2);
					
				}
				
				@Override
				public void hoverInfoGet(GBox b) {
					chart.clear();
					chart.add(data.utilizedHistory);
					b.add(chart);
				};
				
			}.hh(name));
			ss.addDown(2, new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.perc(text, data.degradeValue*TIME.years().bitConversion(TIME.days()));
					
				}
				
				@Override
				public void hoverInfoGet(GBox b) {
					
					GText t = b.text();
					t.add(¤¤degradeD);
					t.insert(0, name);
					b.add(t);
				};
				
			}.hh(Dic.¤¤Degrade));
			
			ss.addDown(2, new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.f0(text, data.projection(), 2);
					
				}
				
				@Override
				public void hoverInfoGet(GBox b) {
					b.text(targetD);
					b.add(GFORMAT.i(b.text(), (int)data.projection()));
				};
				
			}.hh(Dic.¤¤Target));
			
			if (S.get().developer) {
				ss.addDown(2, new GButt.ButtPanel("++") {
					@Override
					protected void clickA() {
						data.cheatAdd(50);
						super.clickA();
					}
				});
				ss.addDown(0, new GButt.ButtPanel("--") {
					@Override
					protected void clickA() {
						data.clear();
						super.clickA();
					}
				});
			}
			
			ss.body().incrW(64);
			text.add(ss);
		}

	}
	
	public void cheatAdd(int baseUnits) {
		inc(TIME.secondsPerDay()*baseUnits*workValue);
	}
	
	public interface ROOM_ADMIN_HOLDER {
		public AdminData admin();
	}

}