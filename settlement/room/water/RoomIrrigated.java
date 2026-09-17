package settlement.room.water;

import game.GAME;
import game.boosting.BOOSTABLE_O;
import game.boosting.BSourceInfo;
import game.boosting.Boostable;
import game.boosting.Booster;
import game.boosting.BoosterImp;
import game.faction.Faction;
import game.faction.npc.FactionNPC;
import game.faction.player.Player;
import init.sprite.UI.UI;
import init.type.HCLASS_RACE;
import settlement.main.SETT;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.stats.Induvidual;
import settlement.stats.STATS;
import settlement.tilemap.ground.Ground;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.misc.CLAMP;

public abstract class RoomIrrigated {

	
	public final double from;
	public final double to;
	
	public RoomIrrigated(ROOM_IRRIGATED blue, Boostable bo, double from, double to) {

		this.from = from;
		this.to = to;
		BSourceInfo in = new BSourceInfo(Ground.¤¤moisture, UI.icons().s.drop.createColored(COLOR.BLUEISH));
		Booster bos = new BoosterImp(in, from, to, true) {
			
			
			
			@Override
			public double vGet(Induvidual indu) {
				RoomInstance ins = STATS.WORK().EMPLOYED.get(indu);
				if (ins != null && ins.blueprint() == blue) {
					return CLAMP.d(irrigation(ins) / needed(ins), 0, 1);
				}
				return 1;
			}
			
			private int ci = -120;
			private double c = 0;
			

			
			@Override
			public double vGet(Player f) {
				return vGet(HCLASS_RACE.clP());
			}
			
			@Override
			public double vGet(HCLASS_RACE popTime) {
				if (blue instanceof RoomBlueprintIns<?>) {
					RoomBlueprintIns<?> p = (RoomBlueprintIns<?>) blue;
					if (Math.abs(GAME.updateI()-ci) >= 120){
						ci = GAME.updateI();
						c = 0;
						int am = 0;
						for (int i = 0; i < p.instancesSize(); i++) {
							RoomInstance ins = p.getInstance(i);
							int e = ins.employees().employed();
							c += e*CLAMP.d(irrigation(ins) / needed(ins), 0, 1);
							am += e;
						}
						
						if (am != 0) {
							c /= am;
						}else {
							c = 1.0;
						}
						
					}
				}
				
				
				return c;
			}
			
			@Override
			public double vGet(FactionNPC f) {
				return 1.0;
			}
			
			
			@Override
			public double vGet(Faction f) {
				return 0;
			}
			
			@Override
			public double get(BOOSTABLE_O o) {
				if (o instanceof FactionNPC)
					return 1.0;
				return super.get(o);
			}
			
		};
		bos.add(bo);
	}
	
	protected abstract double irrigation(RoomInstance ins);
	
	public interface ROOM_IRRIGATED {
		
		public RoomIrrigated irrigation();
		
	}
	
	public double prospectFlat(AREA area) {
		double n = needed(area);
		if (n == 0)
			return 0;
		
		double w = 0;
		
		for (COORDINATE c : area.body()) {
			if (area.is(c)) {
				w += SETT.GROUND().MOISTURE_TOT.get(c);
			}
		}
		return w/n;
	}
	
	public static double rawValue(AREA area) {
		double w = 0;
		
		for (COORDINATE c : area.body()) {
			if (area.is(c)) {
				w += SETT.GROUND().MOISTURE_TOT.get(c);
			}
		}
		return w/area.area();
	}
	
	public double valueProspect(AREA area) {
		double n = prospectFlat(area);		
		return CLAMP.d(from + (to-from)*n, 0, 1);
	}
	
	public double needed(AREA area) {
		return area.area();
	}
	
	public double current(RoomInstance ins) {
		return CLAMP.d(irrigation(ins)/needed(ins), 0, 1);
	}
}
