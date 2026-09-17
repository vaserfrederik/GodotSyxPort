package settlement.room.main.employment;

import java.io.IOException;
import java.util.Arrays;

import game.GameDisposable;
import game.audio.AUDIO;
import game.audio.SoundRace;
import game.faction.Faction;
import init.race.RACES;
import init.race.Race;
import init.type.HTYPE;
import init.type.HTYPES;
import init.type.WGROUP;
import init.value.GVALUES;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.util.RoomInitData;
import settlement.stats.STATS;
import snake2d.LOG;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.Json;
import snake2d.util.rnd.RND;
import snake2d.util.sets.ArrayListResize;
import snake2d.util.sets.LIST;
import util.data.DOUBLE_O;
import util.text.Dic;

public class RoomEmploymentSimple {

	private boolean countInput = false;
	private final RoomBlueprintIns<?> p;
	protected int workersNeeded = 0;
	private int employedTot = 0;
	private int[] employed = Alloc.ii(HTYPES.ALL().size()*RACES.all().size());
	
	private int employedMax = 0;
	private int[] workersNeededPreferred = Alloc.ii(WGROUP.all().size());
	
	public final CharSequence title;
	public final CharSequence verb;
	private double hourStart;
	private final SoundRace sound;
	private boolean shift;
	public final double accidentsPerYear;
	public final double defaultFullfillment;
	public final double healthFactor;
	private double fill = 1.0;
	private final int eindex;
	public final int largeWorkforce;
	private int efficiency = 0;
	private int proximity = 0;
	private int fetch = 0;
	
	
	static ArrayListResize<RoomEmploymentSimple> WORK_ALL = new ArrayListResize<>(10, 512);
	static {
		new GameDisposable() {
			@Override
			protected void dispose() {
				WORK_ALL.clear();
			}
		};
	}
	
	public RoomEmploymentSimple(String key, RoomBlueprintIns<?> p, RoomInitData init) {
		this.p = p;
		Json data = init.data().json(key);
		Json text = init.text().json(key);
		this.title = text.text("TITLE");
		this.verb = text.text("VERB");
		this.hourStart = data.d("SHIFT_OFFSET", 0, 0.99);
		shift = data.has("NIGHT_SHIFT") && data.bool("NIGHT_SHIFT");
		this.sound = AUDIO.race("ROOM_WORK_" + p.key);
		if (data.has("FULFILLMENT"))
			defaultFullfillment = data.d("FULFILLMENT", -100000, 100000);
		else
			defaultFullfillment = 0.5;
		accidentsPerYear = data.dTry("ACCIDENTS_PER_YEAR", 0, 10000, 0)/2.0;
		healthFactor = data.dTry("HEALTH_FACTOR", 0, 1.0, 1);
		largeWorkforce = data.i("LARGE_WORKFORCE", 10, 10000, 1000);
		eindex =  WORK_ALL.add(this);
		
		GVALUES.FACTION.push("EMPLOYED_" + p.key(), Dic.¤¤Employees + ": " + p.info.names, p.iconBig(), new DOUBLE_O<Faction>() {

			@Override
			public double getD(Faction t) {
				return employed();
			}
			
		}, false);
	}
	
	public void countInputSet() {
		countInput = true;
	}
	
	public boolean countInput() {
		return countInput;
	}
	
	public int eindex() {
		return eindex;
	}
	
	public int employed() {
		return employedTot;
	}
	
	public int employed(WGROUP g) {
		if (g == null)
			return employedTot;
		return employed(g.type, g.race);
	}
	
	public int employed(HTYPE t, Race r) {
		return employed[r.index()*HTYPES.ALL().size() +t.index()];
	}
	
	public int employedMax() {
		return employedMax;
	}
	
	public final int neededWorkers() {
		return workersNeeded;
	}
	
	public final int neededWorkers(WGROUP t) {
		return workersNeededPreferred[t.index];
	}
	
	public SoundRace sound() {
		return sound;
	}
	
	void register(RoomEmploymentIns ins, int delta) {
		workersNeeded += delta*ins.hardTarget();
		for (int hi = 0; hi < WGROUP.all().size(); hi++) {
			WGROUP g = WGROUP.all().get(hi);
			workersNeededPreferred[g.index] += ins.preffered().is(g) ? delta*ins.hardTarget() : 0;
		}
		
		efficiency += delta*ins.efficiency()*100*ins.hardTarget();
		efficiency = Math.max(0, efficiency);
		proximity += delta*ins.proximity()*100*ins.hardTarget();
		proximity = Math.max(0, proximity);
		fetch += delta*ins.fetchProximity()*100*ins.hardTarget();
		fetch = Math.max(0, fetch);
		employedMax += delta*ins.max();
		for (RoomEquip t : tools()) {
			t.count(this, delta*ins.tools(t));
		}
		
		if (delta > 0 && workersNeeded < 0)
			throw new RuntimeException(blueprint().info.name + " " + workersNeeded + " " + delta);
	}
	
//	void fixFuckup() {
//		workersNeeded = 0;
//		efficiency = 0;
//	}
	
	void employ(Humanoid h, int delta) {
		employedTot += delta;
		employed[h.race().index()*HTYPES.ALL().size() + h.indu().hType().index()] += delta;
	}
	
	void loadadd(RoomEmploymentIns ins) {
		ins.add();
		employedTot += ins.employed();
	}
	
	public double getFill() {
		return fill;
	}
	
	public LIST<RoomEquip> tools(){
		return SETT.ROOMS().employment.equip.get(this);
	}
	
	public double efficiency() {
		if (p.instancesSize() == 0)
			return 1;
		return efficiency/(100.0*workersNeeded);
	}
	
	public double proximity() {
		if (p.instancesSize() == 0 || workersNeeded <= 0)
			return 1;
		return proximity/(100.0*workersNeeded);
	}
	
	public double fetch() {
		if (p.instancesSize() == 0 || workersNeeded <= 0)
			return 1;
		return fetch/(100.0*workersNeeded);
	}
	
	public double totEff() {
		return efficiency()*proximity()*fetch();
	}
	
	public RoomBlueprintIns<?> blueprint(){
		return p;
	}

	void save(FilePutter file) {
		file.isE(employed);
	}

	void load(FileGetter file) throws IOException {
		file.isE(employed);
	}

	void clear() {
		Arrays.fill(employed, 0);
		employedTot = 0;
		employedMax = 0;
		workersNeeded = 0;
		efficiency = 0;
		proximity = 0;
		fetch = 0;
		Arrays.fill(workersNeededPreferred, 0);
	}
	
	public double getShiftStart() {
		return hourStart;
	}
	
	public void setShiftStart(double start, boolean nights) {
		this.hourStart = start;
		this.shift = nights;
	}
	
	public boolean worksNights() {
		return shift;
	}
	
	public static class EmployerSimple {
		
		private final RoomEmploymentSimple si;
		
		public EmployerSimple(RoomEmploymentSimple si) {
			this.si = si;
		}
		
		public boolean employ(Humanoid h) {

			RoomInstance ins = STATS.WORK().EMPLOYED.get(h);
			if (ins != null && ins.blueprintI() == si.blueprint()) {
				if (ins.blueprintI() == si.blueprint()) {
					if (ins.employees().isOverstaffed()) {
						STATS.WORK().EMPLOYED.set(h, null);
						ins = null;
					}else
						return true;
				}else {
					STATS.WORK().EMPLOYED.set(h, null);
				}
			}
			
			if (si.neededWorkers() > si.employed()) {
				if (si.blueprint().instancesSize() <= 0)
					throw new RuntimeException(si.blueprint().key + " " + si.neededWorkers() + " " + si.employed());
				int i = RND.rInt(si.blueprint().instancesSize());
				for (int k = 0; k < si.blueprint().instancesSize(); k++) {
					RoomInstance in = si.blueprint().getInstance((i+k)%si.blueprint().instancesSize());
					if (in.active() && in.employees().employed() < in.employees().target()) {
						STATS.WORK().EMPLOYED.set(h, in);
						return true;
					}
				}
				LOG.err("no!" + " " + si.neededWorkers() + " " + si.employed() + " " + si.blueprint().instancesSize());
				for (int k = 0; k < si.blueprint().instancesSize(); k++) {
					RoomInstance in = si.blueprint().getInstance((i+k)%si.blueprint().instancesSize());
					LOG.err(k + " " + in.active() + " " + in.employees().employed() + " " + in.employees().target() + " " + in.employees().hardTarget() + " " + si.getFill() + " " + i);
				}
				
			}
			return false;
			
		}
		
		public int employable() {
			return si.neededWorkers()-si.employed();
		}
		
	}
	
}
