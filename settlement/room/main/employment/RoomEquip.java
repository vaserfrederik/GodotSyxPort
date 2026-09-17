package settlement.room.main.employment;

import java.io.IOException;

import game.GAME;
import game.GameDisposable;
import game.boosting.BOOSTING;
import game.boosting.BSourceInfo;
import game.boosting.BoostSpec;
import game.boosting.BoostSpecs;
import game.boosting.Boostable;
import game.boosting.BoostableCat;
import game.boosting.BoosterImp;
import game.faction.FACTIONS;
import game.faction.Faction;
import game.faction.npc.FactionNPC;
import game.faction.player.Player;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.sprite.UI.Icon;
import init.type.HCLASS_RACE;
import settlement.room.main.ROOMS;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.RoomInstance;
import settlement.stats.Induvidual;
import settlement.stats.STATS;
import snake2d.SPRITE_RENDERER;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.Json;
import snake2d.util.file.SAVABLE;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.INDEXED;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import snake2d.util.sprite.SPRITE;
import snake2d.util.sprite.text.Str;
import util.data.INT.INTE;
import util.info.INFO;
import util.text.D;

public class RoomEquip implements INDEXED{

	private final ArrayListGrower<RoomEmploymentSimple> rooms = new ArrayListGrower<>();
	final ArrayList<Target> targets;
	private final int[] currents;
	private int total;
	private final int index;
	public final double degradePerDay;
	private final int defaultTarget;
	public final RESOURCE resource;
	public final BoostSpecs boosts;
	public final int maxAm;
	private static CharSequence ¤¤equipment = "Equipment";
	private static CharSequence ¤¤equipmentD = "Equipment boosts the efficiency of rooms. When equipped, each {0} degrades with a rate of {1} % per day.";
	
	private BoostSpec[] boostMap;
	
	public final INFO info;
	
	static {
		D.ts(RoomEquip.class);
	}
	
	final SAVABLE saver = new SAVABLE() {
		
		@Override
		public void save(FilePutter file) {
			file.i(targets.size());
			for (int i = 0; i < targets.size(); i++) {
				file.i(targets.get(i).get());
			}
		}
		
		@Override
		public void load(FileGetter file) throws IOException {
			clear();
			int am = file.i();
			
			if (am != targets.size()) {
				for (int i = 0; i < am; i++) {
					file.i();
				}
				clear();
			}else {
				for (int i = 0; i < targets.size(); i++) {
					targets.get(i).set(file.i());
				}
			}
			
			
		}
		
		@Override
		public void clear() {
			total = 0;
			for (int i = 0; i < targets.size(); i++) {
				targets.get(i).set(defaultTarget);
				currents[i] = 0;
			}
		}
	};
	
	RoomEquip(String kkey, LISTE<RoomEquip> all, RoomEmployments emps, Json data, ROOMS ROOMS, BoostableCat cat) {
		targets = new ArrayList<>(emps.ALLS().size());
		while(targets.hasRoom())
			targets.add(new Target());
		currents = Alloc.ii(emps.ALLS().size());
		boostMap = new BoostSpec[emps.ALLS().size()];
		
		
		index = all.add(this);
		resource = RESOURCES.map().read(data);
		degradePerDay = data.d("WEAR_PER_DAY", 0, 1);
		defaultTarget = data.i("DEFAULT_TARGET");
		info = new INFO(¤¤equipment + ": " + resource.names, ""+Str.TMP.clear().add(¤¤equipmentD).insert(0, resource.name).insert(1, degradePerDay*100, 2));
		boosts = new BoostSpecs(resource.names, resource.icon(), true);
		double add = data.d("BOOST_MAX_VALUE");
		
		int ams[] = Alloc.ii(emps.ALLS().size());
		ROOMS.collection. new KJson("EQUIP_AMOUNTS", data) {
			
			@Override
			protected void process(RoomBlueprint bp, Json j, String key, boolean isWeak) {
				if (bp instanceof RoomBlueprintImp) {
					RoomBlueprintImp room = (RoomBlueprintImp) bp;
					if (room.bonus() == null || room.employment() == null) {
						GAME.WarnLight(data.errorGet("Not a valid boostable room " + key, key));
						return;
					}
					int am = j.i(key, 0, 100);
					ams[room.employment().eindex()] = am;
					
//					rooms.add(room.employment());
//					
//					
//					targets.get(room.employment().eindex()).init(kkey, am, room, resource, cat);
//					
//					targets.get(room.employment().eindex()).max = am;
//					targets.get(room.employment().eindex()).set(defaultTarget);
					
					
				}
				
			}
		};
		
		for (RoomEmploymentSimple e : emps.ALLS()) {
			int am = ams[e.eindex()];
			if (am > 0) {
				rooms.add(e);
				
				targets.get(e.eindex()).init(kkey, am, e.blueprint(), resource, cat);
				
				targets.get(e.eindex()).max = am;
				targets.get(e.eindex()).set(defaultTarget);
			}
			
		}
		
		
		int m = 0;
		
		for (RoomEmploymentSimple e : rooms) {
			m = Math.max(m, targets.get(e.eindex()).max);
		}
		
		maxAm = m;
		
		boolean mul = data.bool("BOOST_MUL", false);
//		double cost = ROOMS.industries.vanillaRate(resource.tr());
//		
//		cost *= maxAm*degradePerDay/add;
//		if (cost > 1) {
//			GAME.Notify(kkey + " is useless!");
//		}
//		cost = 1-cost;
		
		for (RoomEmploymentSimple e : rooms) {
			m = targets.get(e.eindex()).max;
			final double to = Math.ceil(add*100*targets.get(e.eindex()).max)/(maxAm*100.0);
			BoosterImp bo = new BoosterImp(new BSourceInfo(resource.names, resource.icon()), to, mul) {
				
				@Override
				public double vGet(Faction f) {
					return 0;
				}
				
				@Override
				public double vGet(Player f) {
					return RoomEquip.this.value(e);
				}
				
				@Override
				public double vGet(FactionNPC f) {
					return 0;
				}

				@Override
				public double vGet(HCLASS_RACE popTime) {
					return RoomEquip.this.value(e);
				}
				
				@Override
				public double vGet(Induvidual indu) {
					RoomInstance ins = STATS.WORK().EMPLOYED.get(indu);
					if (ins == null || ins.blueprint() != e.blueprint())
						return 0;
					
					double t = ins.employees().toolsTargetMax(RoomEquip.this);
					if (t == 0)
						return 0;
					double tt = ins.employees().tools(RoomEquip.this);
					tt = CLAMP.d(tt, 0, ins.employees().toolsTarget(RoomEquip.this));
					return CLAMP.d(tt/t, 0, 1);
				}
				
			};
			
			BoostSpec s = boosts.push(bo, e.blueprint().bonus());
			boostMap[e.eindex()] = s;
		}
	}
	
	public Target target(RoomEmploymentSimple e) {
		return targets.get(e.eindex());
	}
	
	public int targetI(RoomEmploymentSimple e) {
		return targets.get(e.eindex()).get()*e.employed();
	}

	public int current(RoomEmploymentSimple e) {
		return currents[e.eindex()];
	}
	
	public double value(RoomEmploymentSimple e) {
		double tt = targetI(e);
		if (tt == 0)
			return 0;
		double c = (double)currents[e.eindex()]/tt;
		c = CLAMP.d(c, 0, targets.get(e.eindex()).getD());
		
		return c;
	}
	
	public int currentTotal() {
		return total;
	}
	
	public int neededTotal() {
		int am = 0;
		for (int i = 0; i < rooms.size(); i++) {
			am += targetI(rooms.get(i));
		}
		return am;
	}
	
	public LIST<RoomEmploymentSimple> rooms(){
		return rooms;
	}
	
	public BoostSpec boost(RoomEmploymentSimple e) {
		return boostMap[e.eindex()];
	}
	
	public boolean has(RoomEmploymentSimple e) {
		return targets.get(e.eindex()).max() > 0;
	}
	
	void count(RoomEmploymentSimple e, int am) {
		currents[e.eindex()] += am;
		total += am;
	}

	@Override
	public int index() {
		return index;
	}
	
	public static class Target implements INTE {

		private Boostable maxLevel;
		int max;
		int i;
		public RoomBlueprintImp blue;
		static final ArrayListGrower<Target> boos = new ArrayListGrower<>();
		static {
			new GameDisposable() {
				
				@Override
				protected void dispose() {
					boos.clear();
				}
			};
		}
		
		Target(){
			
		}
		
		public void init(String kkey, int am, RoomBlueprintImp bp, RESOURCE resource, BoostableCat cat) {
			this.max = am;
			this.blue = bp;
			String key = "LEVEL_" + kkey + "_" + bp.key;
			String name = resource.names + "(" + bp.info.names + ")";
			SPRITE s = new SPRITE.Imp(Icon.L, Icon.S) {
				
				@Override
				public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
					
					int dim = Y2-Y1;
					if (dim <= Icon.S)
						bp.icon.render(r, X1, X2, Y1, Y2);
					else {
						bp.icon.render(r, X1, X1+dim, Y1, Y2);
						resource.icon().render(r, X1+dim, X1+dim+dim, Y1, Y2);
					}
					
				}
			}; 
			
			maxLevel = BOOSTING.push(key, 0, name, name, s, cat);
			boos.add(this);
			
		}


		
		@Override
		public int get() {
			return CLAMP.i(i, 0, availableMax());
		}

		@Override
		public int min() {
			return 0;
		}

		@Override
		public int max() {
			return max;
		}
		
		public int availableMax() {
			return (int) (maxLevel == null ? 0 : maxLevel.get(FACTIONS.player()));
		}

		@Override
		public void set(int t) {
			i = t;
		}
		
		public Boostable boost() {
			return maxLevel;
		}
		
	}
	
	
}
