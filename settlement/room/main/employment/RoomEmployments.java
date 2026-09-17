package settlement.room.main.employment;

import java.io.IOException;
import java.util.Arrays;

import game.GAME;
import game.faction.Faction;
import game.time.TIME;
import init.sprite.UI.UI;
import init.type.WGROUP;
import init.value.GVALUES;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.room.main.ROOMS;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.stats.Induvidual;
import settlement.stats.STATS;
import snake2d.LOG;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.IntegerStack;
import snake2d.util.sets.LIST;
import snake2d.util.sprite.SPRITE;
import util.data.DOUBLE_O;
import util.data.INT;
import util.data.INT_O;
import util.statistics.HISTORY_INT;
import util.statistics.HistoryInt;
import util.text.D;

public final class RoomEmployments {

	private static CharSequence ¤¤allFarms = "Farm Employees";
	private static CharSequence ¤¤allRefiner = "Refiner Employees";
	private static CharSequence ¤¤allMine = "Mine Employees";
	private static CharSequence ¤¤pasture = "Pasture Employees";
	private static CharSequence ¤¤workshop = "Workshop Employees";
	private static CharSequence ¤¤orchard = "Orchard Employees";
	private static CharSequence ¤¤fishery = "Fishery Employees";
	static {
		D.ts(RoomEmployments.class);
	}
	
	private final LIST<RoomEmployment> all;
	private final LIST<RoomEmploymentSimple> allS;
	private int needed = 0;
	private int current = 0;
	private int target = 0;
	private int upI = 0;
	private final RaceGroup[] groups = new RaceGroup[WGROUP.all().size()];
	private int[] searchIS;
	final HistoryInt history = new HistoryInt(32, TIME.days(), true);

	final Employer employer;
	public final RoomEquips equip;
	
	public RoomEmployments(ROOMS rooms) {
		all = new ArrayList<>(RoomEmployment.WORK);
		allS = new ArrayList<>(RoomEmploymentSimple.WORK_ALL);
		RoomEmployment.WORK.clear();
		searchIS = Alloc.ii(all.size());
		
		for (WGROUP g : WGROUP.all()) {
			groups[g.index()] = new RaceGroup(g, this);
		}
		
		equip = new RoomEquips(rooms, this);
		
		employer = new Employer(all);
		
		pushReq("FARM", ¤¤allFarms, UI.icons().l.farm, rooms.FARMS);
		pushReq("REFINER", ¤¤allRefiner, UI.icons().l.refiner, rooms.REFINERS);
		pushReq("MINE", ¤¤allMine, UI.icons().l.mine, rooms.MINES);
		pushReq("PASTURE", ¤¤pasture, UI.icons().l.pasture, rooms.PASTURES);
		pushReq("WORKSHOP", ¤¤workshop, UI.icons().l.workshop, rooms.WORKSHOPS);
		pushReq("ORCHARD", ¤¤orchard, rooms.ORCHARDS.get(0).iconBig(), rooms.ORCHARDS);
		pushReq("FISHERY", ¤¤fishery, UI.icons().l.fish, rooms.FISHERIES);
	}
	
	private static void pushReq(String key, CharSequence name, SPRITE icon, LIST<? extends RoomBlueprintIns<?>> li) {
		GVALUES.FACTION.push("EMPLOYED_" + key, name, icon, new DOUBLE_O<Faction>() {

			@Override
			public double getD(Faction t) {
				int a = 0;
				for (RoomBlueprintIns<?> b : li) {
					a += b.employment().employed();
				}
				return a;
			}
			
		}, false);
	}
	
	
	void changeCurrent(int current, WGROUP g) {
		this.current += current;
		groups[g.index()].change(current);
	}
	
	void changeNeeded(RoomBlueprintIns<?> b, int total) {
		this.needed += total;
	}
	
	void changeTarget(int current, WGROUP g) {
		
		this.target += current;
		groups[g.index()].target += current;
	}

	public void update(double ds) {
		
		groups[upI].update();
		upI++;
		if (upI == groups.length) {
			upI = 0;
			
		}
		
		employer.update();
	}
	
	public void setTargets() {
		employer.updateAll();
	}
	
	public final SAVABLE saver = new SAVABLE() {
		
		@Override
		public void save(FilePutter file) {
			
			history.save(file);
			equip.saver.save(file);
			
			file.i(allS.size());
			for (RoomEmploymentSimple s : allS) {
				file.chars(s.blueprint().key);
				int pos = file.getPosition();
				file.i(0);
				s.save(file);
				int le = file.getPosition()-pos-4;
				file.setAtPosition(pos, le);
			}

		}

		@Override
		public void load(FileGetter file) throws IOException {

			clear();

			history.load(file);
			equip.saver.load(file);
			
			int am = file.i();
			for (int i = 0; i < am; i++) {
				
				String k = file.chars();
				
				RoomBlueprint p = SETT.ROOMS().collection.tryGet(k);
				int skip = file.i();
				if (p == null || p.employment() == null) {
					
					file.setPosition(file.getPosition()+skip);
				}else {
					p.employment().load(file);
				}
			}
			
			for (RoomEmploymentSimple ss : allS) {
				
				if (ss instanceof RoomEmployment) {
					RoomEmployment s = (RoomEmployment) ss;
					for (WGROUP g : WGROUP.all()) {
						changeCurrent(s.employed(g), g);
						changeTarget(s.target.group(g), g);
					}
				}
				changeNeeded(ss.blueprint(), ss.neededWorkers());
				RoomBlueprintIns<?> blue = ss.blueprint();
				if (blue.employment() != null) {
					for (int i = 0; i < blue.instancesSize(); i++) {
						RoomInstance ins = blue.getInstance(i);
						blue.employment().loadadd(ins.employees());
					}
				}
				
			}
			

		}

		@Override
		public void clear() {
			needed = 0;
			current = 0;
			target = 0;
			for (RaceGroup g : groups)
				g.clear();
			Arrays.fill(searchIS, 0);
			for (RoomEmployment e : all)
				e.setPrioOnSkill();
			history.clear();
			for (RoomEmploymentSimple s : allS)
				s.clear();
			equip.saver.clear();
		}
	};

	public HISTORY_INT hEmployed() {
		return history;
	}
	
	public void setWork(Humanoid h) {
		Induvidual i = h.indu();
		
		if (!i.hType().isWorks())
			throw new RuntimeException();
		
		groups[WGROUP.get(i).index()].setWork(h, searchIS);
	}
	
	public boolean hasWork(Humanoid h) {
		Induvidual i = h.indu();
		
		if (!i.hType().isWorks())
			return false;
		
		return groups[WGROUP.get(i).index()].hasWork(h);
	}
	
	public LIST<RoomEmployment> ALL(){
		return all;
	}
	
	public LIST<RoomEmploymentSimple> ALLS(){
		return allS;
	}

	public INT NEEDED = new INT() {

		@Override
		public int get() {
			return needed;
		}

		@Override
		public int min() {
			return 0;
		}

		@Override
		public int max() {
			return needed;
		}

	};

	public INT_O<WGROUP> CURRENT = new INT_O<WGROUP>() {

		@Override
		public int get(WGROUP t) {
			if (t == null)
				return current;
			return groups[t.index()].current;
		}
		
		@Override
		public int min(WGROUP t) {
			return 0;
		};
		@Override
		public int max(WGROUP t) {
			return Integer.MAX_VALUE;
		};

	};
	
	public INT_O<WGROUP> TARGET = new INT_O<WGROUP>() {

		@Override
		public int get(WGROUP t) {
			if (t == null)
				return target;
			return groups[t.index()].target;
		}
		@Override
		public int min(WGROUP t) {
			return 0;
		};
		@Override
		public int max(WGROUP t) {
			return Integer.MAX_VALUE;
		};
	};
	

	private static class RaceGroup implements SAVABLE {

		int target;
		int current;
		final IntegerStack possibles;
		private final RoomEmployments es;
		private WGROUP group;

		public RaceGroup(WGROUP group, RoomEmployments es) {
			this.es = es;
			possibles = new IntegerStack(es.all.size());
			this.group = group;
		}

		void change(int current) {
			this.current += current;
		}

		boolean setWork(Humanoid i, int[] searchI) {

				//can I ruing an ongoing work plan here?
			
			RoomInstance old = STATS.WORK().EMPLOYED.get(i);
			
			if (old != null && old.blueprintI().employment() instanceof RoomEmployment) {
				RoomEmployment ee = (RoomEmployment) old.blueprintI().employment();
				if (ee.employed(group) > ee.target.group(group)) {
					STATS.WORK().EMPLOYED.set(i, null);
					
				}else if (old.employees().isOverstaffed()) {
					STATS.WORK().EMPLOYED.set(i, null);
				}
			}

			
			
			if (STATS.WORK().EMPLOYED.get(i) != null)
				return true;

			if (current >= target) {
				return false;
			}
			
			while(!possibles.isEmpty()) {
				RoomEmployment e = es.all.get(possibles.pop());
				if (e.employed() < e.neededWorkers() && e.employed(group) < e.target.group(group)) {
					possibles.push(e.index());
					return setWork(i,e, searchI);
				}
			}
			
			return false;
			
		}
		
		boolean hasWork(Humanoid i) {
			return current < target;
		}
		
		private boolean setWork(Humanoid i, RoomEmployment e, int[] searchI) {
			
			int am = e.blueprint().instancesSize();
			
			for (int k = 0; k < am; k++) {
				if (searchI[e.index()] >= am)
					searchI[e.index()] = 0;
				RoomInstance ins = e.blueprint().getInstance(searchI[e.index()]);
				if (ins.active() && ins.employees().employed() < ins.employees().target()) {
					STATS.WORK().EMPLOYED.set(i, ins);
					return true;
				}
				searchI[e.index()] ++;
				
			}
			
			GAME.Notify("oh no!" + e.blueprint().info.name + " " + i.race().info.name + " " + e.target.group(group) + " " + e.employed(group) + " " + e.employed() + " " + e.neededWorkers());
			for (int ii = 0; ii < am; ii++) {
				RoomInstance ins = e.blueprint().getInstance(ii);
				LOG.ln(ins.employees().employed()  + "  " + ins.employees().target());
			}
			
			return false;
			
		}

		void update() {
			possibles.clear();
			for (RoomEmployment p : SETT.ROOMS().employment.all) {
				if (p.employed() < p.neededWorkers() && p.employed(group) < p.target.group(group)) {
					possibles.push(p.index());
				}
			}
		}

		@Override
		public void save(FilePutter file) {
			file.i(current);
			file.i(target);
		}

		@Override
		public void load(FileGetter file) throws IOException {
			current = file.i();
			target = file.i();
			possibles.clear();
		}

		@Override
		public void clear() {
			current = 0;
			target = 0;
			possibles.clear();
			
		}

	}
	

	
}
