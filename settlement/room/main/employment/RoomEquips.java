package settlement.room.main.employment;

import java.io.IOException;

import game.boosting.Boostable;
import game.boosting.BoostableCat;
import init.paths.PATH;
import init.paths.PATHS;
import init.sprite.UI.UI;
import settlement.room.main.ROOMS;
import settlement.room.main.employment.RoomEquip.Target;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.Json;
import snake2d.util.file.SAVABLE;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LIST;
import util.text.Dic;

public final class RoomEquips {

	public final LIST<RoomEquip> ALL;
	private final ArrayList<ArrayListGrower<RoomEquip>> perRoom;
	
	RoomEquips(ROOMS rooms, RoomEmployments emps) {
		
		BoostableCat cat = new BoostableCat("EQUIP_", Dic.¤¤Equipment, Dic.¤¤Equipment, BoostableCat.TYPE_CRAP, UI.icons().s.house);
		ArrayListGrower<RoomEquip> all = new ArrayListGrower<>();
		
		PATH p = PATHS.INIT().getFolder("resource").getFolder("work");
		
		for (String k : p.getFiles()) {
			
			new RoomEquip(k, all, emps, new Json(p.gets(k)), rooms, cat);
			
		}
		
		this.ALL = all;
		
		perRoom = new ArrayList<ArrayListGrower<RoomEquip>>(emps.ALLS().size());
		while(perRoom.hasRoom())
			perRoom.add(new ArrayListGrower<RoomEquip>());
		
		
		for (RoomEquip t : all) {
			for (RoomEmploymentSimple e : emps.ALLS()) {
				if (t.target(e).max > 0)
					perRoom.get(e.eindex()).add(t);
			}
		}
		
		
	}

	public Target boostToTarget(Boostable bo) {
		
		if (bo.index() >= Target.boos.get(0).boost().index() && bo.index() < Target.boos.get(Target.boos.size()-1).boost().index()) {
			return Target.boos.get(bo.index()-Target.boos.get(0).boost().index());
		}
		return null;
	}
	
	public LIST<RoomEquip> get(RoomEmploymentSimple e){
		return perRoom.get(e.eindex());
	}
	
	final SAVABLE saver = new SAVABLE() {
		
		@Override
		public void save(FilePutter file) {
			file.i(ALL.size());
			for (RoomEquip t : ALL)
				t.saver.save(file);
		}
		
		@Override
		public void load(FileGetter file) throws IOException {
			int am = file.i();
			
			if (am != ALL.size()) {
				for (int i = 0; i < am; i++) {
					ALL.get(0).saver.load(file);
				}
				clear();
			}else {
				for (RoomEquip t : ALL)
					t.saver.load(file);
			}
			
		}
		
		@Override
		public void clear() {
			for (RoomEquip t : ALL)
				t.saver.clear();
		}
	};
	
	
}
