package settlement.room.law.police;

import java.io.IOException;

import game.GAME;
import game.battle.div.Div;
import game.boosting.BSourceInfo;
import game.boosting.BValue;
import game.boosting.BoostSpecs;
import game.faction.npc.FactionNPC;
import game.faction.player.Player;
import init.race.RACES;
import init.race.Race;
import init.type.HCLASS;
import init.type.HCLASSES;
import init.type.HCLASS_RACE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.stats.Induvidual;
import settlement.stats.POP;
import settlement.stats.STATS;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LISTE;
import util.data.BOOLEAN.BOOLEANImp;
import util.data.BOOLEAN.BOOLEAN_MUTABLE;
import view.sett.ui.room.UIRoomModule;
import world.map.regions.Region;

public final class ROOM_POLICE extends RoomBlueprintIns<PoliceInstance>{

	final PoliceConstructor constructor;
	
	private final ArrayListGrower<BOOLEANImp> access = new ArrayListGrower<BOOLEANImp>();
	
	public final PoliceWork work = new PoliceWork(this);
	
	public final BoostSpecs spec;
			
	
	public ROOM_POLICE(RoomInitData init, RoomCategorySub block) throws IOException {
		super(0, init, "_POLICE", block);
		
		for (int i = 0; i < HCLASS_RACE.ALL().size(); i++)
			access.add(new BOOLEANImp(HCLASS_RACE.ALL().get(i).cl == HCLASSES.SLAVE()));
		
		constructor = new PoliceConstructor(this, init);
		
		BValue v = new BValue() {
			
			private int[] upIs = Alloc.ii(HCLASS_RACE.ALL().size());
			private double[] vv = new double[HCLASS_RACE.ALL().size()];
			
			@Override
			public double vGet(FactionNPC f) {
				return 0;
			}
			
			@Override
			public double vGet(Player f) {
				return vGet(HCLASS_RACE.clP());
			}
			
			
			@Override
			public double vGet(HCLASS_RACE cl) {
				
				if (upIs[cl.index] != GAME.updateI()) {
					upIs[cl.index] = GAME.updateI();
					if (cl.cl == null) {
						double pop = 0;
						double v = 0;
						for (int ci = 0; ci < HCLASSES.ALLP().size(); ci++) {
							double p = POP.pop(HCLASSES.ALLP().get(ci), cl.race);
							pop += p;
							v += p*vGet(HCLASS_RACE.clP(cl.race, HCLASSES.ALLP().get(ci)));
						}
						if (pop == 0)
							vv[cl.index] = 0;
						else
							vv[cl.index] = v/pop;
					}else if (cl.race == null) {
						double pop = 0;
						double v = 0;
						for (int ri = 0; ri < RACES.all().size(); ri++) {
							double p = POP.pop(cl.cl, RACES.all().get(ri));
							pop += p;
							v += p*vGet(HCLASS_RACE.clP(RACES.all().get(ri), cl.cl));
						}
						if (pop == 0)
							vv[cl.index] = 0;
						else
							vv[cl.index] = v/pop;
					}else {
						if (!access.get(cl.index()).is()) {
							vv[cl.index] = 0;
						}else {
							vv[cl.index()] = value();
						}
					}
					
					
					
				}
				
				return vv[cl.index];
			}
			
			@Override
			public double vGet(Div div) {
				return vGet(HCLASS_RACE.clP(div.race(), HCLASSES.CITIZEN()));
			}
			
			@Override
			public double vGet(Induvidual indu) {
				return vGet(indu.popCL());
			}
			
			@Override
			public double vGet(Region reg) {
				return 0;
			}
		};
		
		spec = new BoostSpecs(new BSourceInfo(info.name, icon), true);
		spec.read(init.data(), v);
		
	}
	
	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}
	

	@Override
	public Furnisher constructor() {
		return constructor;
	}

	@Override
	protected void saveP(FilePutter f){
		HCLASS_RACE.MAP().saver().save(access, f);
	}
	
	@Override
	protected void loadP(FileGetter f) throws IOException{
		HCLASS_RACE.MAP().loader().load(access, f);
	}
	
	@Override
	protected void clearP() {
		for (BOOLEANImp b : access)
			b.set(false);
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return null;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}
	
	
	public BOOLEAN_MUTABLE access(HCLASS_RACE g) {
		return access.get(g.index());
	}
	
	public double value() {
		
		double pop = 0;
		for (HCLASS_RACE r : HCLASS_RACE.ALL()) {
			if (access.get(r.index).is())
				pop += STATS.POP().POP.data(r.cl).get(r.race);
		}
		
		if (pop == 0)
			return employment().employed() > 0 ? 1 :0;
		
		return Math.sqrt(CLAMP.d(employment().employed()/pop, 0, 1));
	}
	
	public double value(HCLASS cl, Race race) {
		
		if (!access.get(HCLASS_RACE.clP(race, cl).index).is())
			return 0;
		
		double pop = 0;
		for (HCLASS_RACE r : HCLASS_RACE.ALL()) {
			if (access.get(r.index).is())
				pop += STATS.POP().POP.data(r.cl).get(r.race);
		}
		
		if (pop == 0)
			return employment().employed() > 0 ? 1 :0;
		
		return Math.sqrt(CLAMP.d(employment().employed()/pop, 0, 1));
	}

}
