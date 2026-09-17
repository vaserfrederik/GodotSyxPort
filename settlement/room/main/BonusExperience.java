package settlement.room.main;

import java.io.IOException;
import java.util.Arrays;

import game.GameDisposable;
import game.boosting.BSourceInfo;
import game.boosting.BValue;
import game.boosting.Boostable;
import game.boosting.BoosterValue;
import game.faction.npc.FactionNPC;
import game.faction.player.Player;
import game.time.TIME;
import init.sprite.UI.UI;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.Json;
import snake2d.util.file.SAVABLE;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LIST;
import snake2d.util.sprite.text.Str;
import util.text.D;
import util.updating.IUpdater;
import view.ui.message.MessageText;

public final class BonusExperience implements SAVABLE{

	private static CharSequence ¤¤name = "¤Experience";
	private static CharSequence ¤¤mGainedTitle = "¤Experience Gained";
	
	private static CharSequence ¤¤mGainedBody = "¤We now employ over {0} in our {1}, and as a result, this combined experience is boosting performance. Boosting will continue to increase up until {2} employees.";
	
	private static CharSequence ¤¤mLostTitle = "¤Experience Lost";
	
	
	private static CharSequence ¤¤mLostBody = "¤Since the employees of our {0} have plummeted, performance boosts from experience has been lost.";
	
	static {
		D.ts(BonusExperience.class);
	}

	
	private final int[] currents;
	private byte[] sent; 

	
	BonusExperience() {

		currents = Alloc.ii(all.size());
		sent = Alloc.bb(all.size());
		
		
	}
	
	
	private final IUpdater up = new IUpdater(all.size(), TIME.secondsPerDay()) {
		
		@Override
		protected void update(int index, double timeSinceLast) {
			
			RoomExperienceBonus bo = all.get(index);
			
			int am = bo.blue.employment().employed();
			
			if (currents[index] < bo.minEmployed && am >= bo.minEmployed && (sent[index] & 1) == 0) {
				MessageText m = new MessageText(¤¤mGainedTitle);
				Str s = Str.TMP;
				s.clear();
				s.add(¤¤mGainedBody);
				s.insert(0, bo.minEmployed);
				s.insert(1, bo.blue.info.names);
				s.insert(2, bo.maxEmployed);
				m.paragraph(s);
				m.send();
				sent[index] |= 1;
			}else if (currents[index] >= bo.minEmployed && am < bo.minEmployed && (sent[index] & 2) == 0) {
				MessageText m = new MessageText(¤¤mLostTitle);
				
				Str s = Str.TMP;
				s.clear();
				s.add(¤¤mLostBody);
				s.insert(0, bo.blue.info.names);
				
				m.paragraph(s);
				m.send();
				sent[index] |= 2;
			}
			currents[index] = am;
			
			
			
		}
	};
	
	void update(double ds) {
		up.update(ds);
	}

	@Override
	public void save(FilePutter file) {
		file.isE(currents);
		file.bsE(sent);
		up.save(file);
		
	}

	@Override
	public void load(FileGetter file) throws IOException {
		file.isE(currents);
		file.bsE(sent);
		up.load(file);
	}

	@Override
	public void clear() {
		Arrays.fill(currents, 0);
		Arrays.fill(sent, (byte)0);
	}

	private static final ArrayListGrower<RoomExperienceBonus> all =  new ArrayListGrower<>();
	static {
		new GameDisposable() {
			
			@Override
			protected void dispose() {
				all.clear();
			}
		};
	}
	
	public LIST<RoomExperienceBonus> ALL(){
		return all;
	}
	
	
	public static class RoomExperienceBonus {
		
		
		
		public final double bonus;
		public final int maxEmployed;
		public final int minEmployed;
		private final double ie;
		public final RoomBlueprintImp blue;
		public final Boostable boostable;
		
		public RoomExperienceBonus(RoomBlueprintImp blue, Json data, Boostable boostable){
			all.add(this);
			this.blue = blue;
			this.boostable = boostable;
			
			
			int ma = 1000;
			double bo = 1.0;
			
			if (data.has("EXPERIENCE_BONUS")) {
				data = data.json("EXPERIENCE_BONUS");
				ma = data.i("MAX_EMPLOYEES", 50, Integer.MAX_VALUE);
				bo = data.d("BONUS");
			}
			
			maxEmployed = ma;
			minEmployed =  50;
			bonus = bo;
			ie = 1.0/(maxEmployed-minEmployed);
			BValue v = new BValue.BValueFaction(boostable) {
				
				@Override
				public double vGet(Player f) {
					return CLAMP.d((blue.employment().employed()-minEmployed)*ie, 0, 1.0);
				}

				@Override
				public double vGet(FactionNPC f) {
					return 0;
				}
			};
			
			BoosterValue bos = new BoosterValue(v, new BSourceInfo(¤¤name, UI.icons().s.clock), 0, bonus, false);
			
			bos.add(boostable);
			
		}
		
	}

}
