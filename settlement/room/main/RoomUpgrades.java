package settlement.room.main;

import java.util.Arrays;

import game.GAME;
import game.boosting.BOOSTABLE_O;
import game.boosting.BSourceInfo;
import game.boosting.Boostable;
import game.boosting.Booster;
import game.boosting.BoosterImp;
import game.faction.Faction;
import game.faction.npc.FactionNPC;
import game.faction.player.Player;
import init.sprite.UI.Icon;
import init.sprite.UI.UI;
import init.type.HCLASS_RACE;
import init.value.GVALUES;
import init.value.Lockable;
import settlement.room.main.util.RoomInitData;
import settlement.stats.Induvidual;
import settlement.stats.STATS;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.color.OPACITY;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sprite.SPRITE;
import util.info.GFORMAT;
import util.text.Dic;

public final class RoomUpgrades {

	private final int upgrades;
	private final double[][] masks;
	private final double[] boosts;
	public final ArrayListGrower<Lockable<Faction>> reqs = new ArrayListGrower<>();
	private final CharSequence[] texts;
	private final static COLOR ORANGE100 = new ColorImp(127,100,0);
	RoomUpgrades(RoomBlueprintImp blue, RoomInitData init) {
		
		double ai = 0;
		if (init.data().has("UPGRADES")) {
			Json[] jj = init.data().jsons("UPGRADES", 1);
			masks = new double[jj.length][];
			boosts = new double[jj.length];
			
			upgrades = jj.length;
			int i = 0;
			int ll = 0;
			for (Json j : jj) {
				double[] mask = j.ds("RESOURCE_MASK");
				
				ll = Math.max(ll, mask.length);
				double b = j.d("BOOST");
				masks[i] = mask;
				
				
				boosts[i] = b;
				if (j.has("AI")) {
					ai = Math.max(ai, j.d("AI", 0, 10000));
				}else {
					ai = Math.max(ai, b*0.5);
				}
					
				i++;
			}
			
			
			for (i = 0; i < boosts.length; i++) {
				if (masks[i].length < ll) {
					double[] nn = new double[ll];
					Arrays.fill(nn, 1);
					for (int k = 0; k < masks[i].length; k++)
						nn[k] = masks[i][k];
					masks[i] = nn;
				}
			}
			texts = init.text().textsTry("UPGRADES");
			//LOG.ln(blue.key + " " + max());
			
		}else {
			upgrades = 1;
			masks = new double[][] {
				{1}
			};
			boosts = new double[] {
				1
			};
			texts = new CharSequence[0];
		}

		for (int i = 1; i <= max(); i++) {
			
			final int upAm = i;

			
			SPRITE icon = new SPRITE.Imp(Icon.L, Icon.L) {
				
				@Override
				public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
					
					
					blue.icon.render(r, X1, X2, Y1, Y2);
					int size = Icon.S*(X2-X1)/Icon.L;
					
					UI.icons().s.chevron(DIR.N);
					
					COLOR.BLACK.bind();
					OPACITY.O66.bind();
					int sh = size/8;
					for (int j = 0; j < upAm; j++)
						UI.icons().s.chevron(DIR.N).render(r, X1+sh, X1+size+sh, Y1+sh+j*size/2, Y1+sh+j*size/2+size);
					OPACITY.unbind();
					COLOR.unbind();
					//GCOLOR.T().bronzeGold((upAm-1)/(upgrades-1)).bind();
					
					ORANGE100.bind();
					for (int j = 0; j < upAm; j++)
						UI.icons().s.chevron(DIR.N).render(r, X1, X1+size, Y1+j*size/2, Y1+j*size/2+size);
					COLOR.unbind();
					COLOR.unbind();
					size= Icon.M*(X2-X1)/(Icon.L*2);
					
					for (int ri = 0; ri < blue.constructor().resources(); ri++) {
						if (masks[upAm][ri] > 0 && masks[upAm-1][ri] == 0) {
							blue.constructor().resource(ri).icon().render(r, X2-size, X2, Y2-size, Y2);
							Y2 -= size*0.75;
						}
					}

						
				}
			};
			
			reqs.add(GVALUES.FACTION.LOCK.push("ROOM_" + init.key() + "_UPGRADE_" + i, blue.info.name + " (" + Dic.¤¤Upgrade + " " + GFORMAT.toNumeral(i) + ")", "", icon));
		}
	}
	
	public void pushBonus(RoomBlueprintIns<?> blue, Boostable bo) {
		
		if (max() <= 0)
			return;
		
		double from = boost(0);
		double to = boost(max());
		BSourceInfo in = new BSourceInfo(Dic.¤¤Upgrade, UI.icons().s.chevron(DIR.N));
		Booster bos = new BoosterImp(in, from, to, false) {
			
			@Override
			public double get(BOOSTABLE_O o) {
				if (o instanceof FactionNPC) {
					return 0;
				}
				return o.boostableValue(this);

//				double d = o.boostableValue(this);
//				d = CLAMP.d(d, 0, RoomUpgrades.this.max());
//				int di = (int) d;
//				d -= di;
//				double res = RoomUpgrades.this.boost(di)*(1.0-d);
//				if (di < RoomUpgrades.this.max())
//					res += RoomUpgrades.this.boost(di+1)*d;
//				return res;
			}
			
			@Override
			public double vGet(Induvidual indu) {
				return get(STATS.WORK().EMPLOYED.get(indu));
			}
			
			private int ci = -120;
			private double c = 0;
			

			
			@Override
			public double vGet(Player f) {
				return vGet(HCLASS_RACE.clP());
			}
			
			private double get(RoomInstance ins) {
				if (ins != null && ins.blueprint() == blue) {
					return ins.blueprintI().upgrades().boost(ins.upgrade());
				}
				return 0;
			}
			
			@Override
			public double vGet(HCLASS_RACE popTime) {
				if (Math.abs(GAME.updateI()-ci) >= 120){
					ci = GAME.updateI();
					c = 0;
					int am = 0;
					for (int i = 0; i < blue.instancesSize(); i++) {
						RoomInstance ins = blue.getInstance(i);
						int e = ins.employees().employed();
						c += e*get(ins);
						am += e;
					}
					
					if (am != 0) {
						c /= am;
					}
					
				}
				
				return c;
			}
			
			@Override
			public double vGet(FactionNPC f) {
				
//				double d = f.bonus.get(blue.index());
//				double eff = aiEff*d;
//				return eff;
				return 0;
			}
			
			
			@Override
			public double vGet(Faction f) {
				return 0;
			}
			
		};
		bos.add(bo);
	}
	
	public int max() {
		return upgrades-1;
	}
	
	public double resMask(int upgrade, int ri) {
		upgrade = CLAMP.i(upgrade, 0, max());
		ri = CLAMP.i(ri, 0, masks[upgrade].length-1);
		return masks[upgrade][ri];
	}
	
	public double boost(int upgrade) {
		return boosts[CLAMP.i(upgrade, 0, boosts.length-1)];
	}
	
	public double upD(RoomInstance room) {
		return (1.0+room.upgrade())/(max()+1.0);
	}
	
	public Lockable<Faction> requires(int upgrade){
		return reqs.get(upgrade-1);
	}
	
	public CharSequence desc(int upgrade) {
		if (upgrade > 0 && upgrade-1 < texts.length) {
			return texts[upgrade-1];
		}
		return null;
	}
	
}
