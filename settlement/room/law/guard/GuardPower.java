package settlement.room.law.guard;

import java.io.IOException;

import game.GAME;
import game.battle.div.Div;
import game.battle.util.DIV_SPEC;
import game.faction.FACTIONS;
import game.faction.Faction;
import init.race.Race;
import init.type.HTYPES;
import settlement.main.SETT;
import settlement.stats.STATS;
import settlement.stats.colls.StatsBattle.StatTraining;
import settlement.stats.equip.EquipBattle;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.misc.CLAMP;

public class GuardPower implements SAVABLE{

	
	private Div d;
	private final DIV_SPEC spec = new DIV_SPEC() {
		
		@Override
		public Race race() {
			return d.race();
		}
		
		@Override
		public double training(StatTraining tr) {
			return tr.stat.div().getD(d);
		}
		
		@Override
		public double equip(EquipBattle e) {
			return CLAMP.d((double)e.stat().div().get(d)/STATS.POP().pop(HTYPES.SOLDIER(), d), 0, 1);
		}
		
		@Override
		public int men() {
			return STATS.POP().pop(HTYPES.GUARD(), d);
		}
		
		@Override
		public Faction faction() {
			return FACTIONS.player();
		}
		
		@Override
		public double experience() {
			return STATS.BATTLE().COMBAT_EXPERIENCE.div().getD(d);
		}

		@Override
		public CharSequence name() {
			// TODO Auto-generated method stub
			return null;
		}

		@Override
		public int bannerI() {
			// TODO Auto-generated method stub
			return 0;
		}
	};
	
	private double res = 0;
	private double resD = 0;
	private int upI = -1;
	private int DI = 0;
	
	public double get() {
		if (upI == GAME.updateI())
			return res;
		upI = GAME.updateI();
		if (DI >= GAME.ARMIES().player().divisions().size()) {
			DI = 0;
			res = resD;
			resD = 0;
		}
		
		Div d = GAME.ARMIES().player().divisions().get(DI);
		if (SETT.ROOMS().GUARD.activeDuty.is(d) && STATS.POP().pop(HTYPES.GUARD(), d) > 0) {
			this.d = d;
			resD += GAME.battle().power.get(spec);
		}
		DI++;
		
		return res;
	}

	@Override
	public void save(FilePutter file) {
		file.d(res);
	}

	@Override
	public void load(FileGetter file) throws IOException {
		res = file.d();
	}

	@Override
	public void clear() {
		res = 0;
		resD = 0;
	}
	
}
