package settlement.room.water;

import java.io.IOException;

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
import settlement.room.main.RoomBlueprint;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.util.RoomInitData;
import settlement.room.water.RoomPumpable.ROOM_PUMPABLE;
import settlement.stats.Induvidual;
import settlement.stats.STATS;
import snake2d.util.color.COLOR;
import snake2d.util.map.MAP_OBJECT;
import util.text.D;

public class ROOM_WATER {

	private static CharSequence ¤¤irrigation = "Water Supply";
	static {
		D.ts(ROOM_WATER.class);
	}	
	
	public final ROOM_PUMP pump;
	public final Canal canal;
	public final Drain drain;
	final WSprite sprite;
	final Updater updater = new Updater(this);
	
	public ROOM_WATER(RoomInitData init, RoomCategorySub cat) throws IOException{
		pump = new ROOM_PUMP(init, cat);
		canal = new Canal(init, cat);
		drain = new Drain(init, cat);
		sprite = new WSprite(this, init);
	}
	
	final MAP_OBJECT<RoomPumpable> pumpable = new MAP_OBJECT<RoomPumpable>() {

		@Override
		public RoomPumpable get(int tile) {
			return get(tile%SETT.TWIDTH, tile/SETT.TWIDTH);
		}

		@Override
		public RoomPumpable get(int tx, int ty) {
			RoomBlueprint p = SETT.ROOMS().map.blueprint.get(tx, ty);
			if (p != null && p instanceof ROOM_PUMPABLE)
				return ((ROOM_PUMPABLE) p).pumpable(tx, ty);
			return null;
		}
		
	};
	
	public static void pushBonus(ROOM_PUMPABLE blue, Boostable bo, double from, double to) {
		
		
		BSourceInfo in = new BSourceInfo(¤¤irrigation, UI.icons().s.drop.createColored(COLOR.BLUEISH));
		Booster bos = new BoosterImp(in, from, to, false) {
			
			
			
			@Override
			public double vGet(Induvidual indu) {
				RoomInstance ins = STATS.WORK().EMPLOYED.get(indu);
				if (ins != null && ins.blueprint() == blue) {
					return blue.pumpable(ins.mX(), ins.mY()).irrigation(ins.mX(), ins.mY());
				}
				return 0;
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
							c += e*blue.pumpable(ins.mX(), ins.mY()).irrigation(ins.mX(), ins.mY());
							am += e;
						}
						
						if (am != 0) {
							c /= am;
						}
						
					}
				}
				
				
				return c;
			}
			
			@Override
			public double vGet(FactionNPC f) {
				return 0;
			}
			
			@Override
			public double get(BOOSTABLE_O o) {
				if (o instanceof FactionNPC)
					return 1.0;
				return super.get(o);
			}
			
			@Override
			public double vGet(Faction f) {
				return 0;
			}
			
		};
		bos.add(bo);
	}
	
}
