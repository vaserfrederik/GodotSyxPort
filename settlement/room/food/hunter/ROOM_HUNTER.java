package settlement.room.food.hunter;

import java.io.IOException;

import game.time.TIME;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.industry.module.RoomBoost;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.misc.CLAMP;
import snake2d.util.rnd.RND;
import snake2d.util.sets.ArrayCooShort;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import snake2d.util.sprite.text.Str;
import util.info.INFO;
import util.text.D;
import util.text.Dic;
import view.sett.ui.room.UIRoomModule;

public class ROOM_HUNTER extends RoomBlueprintIns<HunterInstance> implements INDUSTRY_HASER{

	public static final String type = "HUNTER";
	
	final Constructor constructor;
	final LIST<Industry> indus;
	final Tile tile;
	
	public double luck = 1.0;
	private int year = -1;
	
	public final int MAX_EMPLOYED;
	
	private static CharSequence ¤¤emp = "The more employees you have, the less efficient this industry will become. The max amount for this room is {0}. Employees after that point will decrease the output gradually."; 
	private static CharSequence ¤¤luck = "Luck";
	private static CharSequence ¤¤luckD = "How lucky your hunters are. Changes annually";
		
	
	static {
		D.ts(ROOM_HUNTER.class);
	}
	
	public final RoomBoost bEmployed;
	
	public ROOM_HUNTER(int index, RoomInitData init, String key, RoomCategorySub cat) throws IOException {
		super(index, init, key, cat);
		
		constructor = new Constructor(this, init);
		pushBo(init.data(), type, true);
		MAX_EMPLOYED = init.data().i("MAX_EMPLOYED", 1, 10000);
		
		

		bEmployed = new RoomBoost() {
			INFO info = new INFO(Dic.¤¤Employees, "" + Str.TMP.clear().add(¤¤emp).insert(0, MAX_EMPLOYED));
			
			@Override
			public INFO info() {
				return info;
			}
			
			@Override
			public double get(RoomInstance r) {
				return eBonus(0);
			}
		};
		
		RoomBoost bluck = new RoomBoost() {
			INFO info = new INFO(¤¤luck, ¤¤luckD);
			@Override
			public INFO info() {
				return info;
			}
			
			@Override
			public double get(RoomInstance r) {
				return luck;
			}
			
			@Override
			public double min() {
				return 0.6;
			}
			
			@Override
			public double max() {
				return 1.4;
			}
		};

		indus = Industry.createIndustries(this, init, new RoomBoost[] {bEmployed, bluck, constructor.efficiency}, bonus());
		for (Industry i : indus)
			i.isOnlyRoomDoNotUse = true;
		tile = new Tile(this);
	}
	
	public double eBonus(int delta) {
		double emp = employment().employed()+delta;
		if (emp < MAX_EMPLOYED)
			return 1.0;
		double d = 1 + (emp-MAX_EMPLOYED)/(MAX_EMPLOYED*4);
		return 1.0/d;
	}
	
	@Override
	protected void update(double ds) {
		if (year != TIME.years().bitsSinceStart()) {
			luck = RND.rFloat1(0.4);
			year = TIME.years().bitsSinceStart();
		}
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		IndustryUtil.save(saveFile, indus);
		saveFile.d(luck);
		saveFile.i(year);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		IndustryUtil.load(saveFile, indus);
		luck = saveFile.d();
		year = saveFile.i();
	}
	
	@Override
	protected void clearP() {
		IndustryUtil.clear(indus);
		luck = 1;
		year = -1;
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		
	}

	@Override
	public LIST<Industry> industries() {
		return indus;
	}
	
	public void resetGore(COORDINATE c) {
		tile.reset(getter.get(c), c);
	}
	
	public void gore(COORDINATE c) {
		tile.gore(getter.get(c), c);
	}

	public COORDINATE reserveWork(RoomInstance inss, Humanoid h) {
		
		COORDINATE start = h.tc();
		
		
		HunterInstance ins = (HunterInstance) inss;
		
		for (DIR d : DIR.ORTHO) {
			Tile j = tile.init(start.x()+d.x(), start.y()+d.y(), ins);
			if (j != null && j.reserved.get() == 0 )
				return clean(j);
		}
		
		
		ArrayCooShort coos = ins.coos;
		for (int i = 0; i < coos.size(); i++) {
			coos.inc();
			Tile j = tile.init(coos.get().x(), coos.get().y(), ins);
			if (j.reserved.get() == 0)
				return clean(j);
		}
		
		return null;
	}
	
	private COORDINATE clean(Tile j) {
		
//		Cadaver ca = SETT.THINGS().cadavers.tGet.get(j.coo);
//		if (ca != null)
//			ca.remove();
		
		
		return j.coo;
	}
	
	public void reportSkill(RoomInstance inss, Humanoid h) {
		HunterInstance ins = (HunterInstance) inss;
		ins.dSkill += IndustryUtil.roomBonus(ins, indus.get(0))*bonus.get(h.indu());
		ins.iSkill ++;
	}
	
	public boolean work(RoomInstance inss, COORDINATE work, Humanoid h, boolean cadaver) {
		
		HunterInstance ins = getter.get(work);
		if (ins == null)
			return false;
		
		
		if (ins.produce > 1) {
			double mm = 1 + ins.produce/10;
			mm = CLAMP.d(mm, 0, ins.produce);
			ins.produce -= mm;
			DIR dir = storeDir(work);
			for (IndustryResource o : ins.industry().outs()) {
				int am = o.inc(ins, mm*o.rate);
				if (am > 0) {
					SETT.THINGS().resources.createPrecise(work.x()+dir.x(), work.y()+dir.y(), ins.industry().outs().get(0).resource, am);
				}
			}

			
			
		}
		Tile j = tile.init(work.x(), work.y(), ins);
		j.cadaver.set(ins, cadaver ? 1 : 0);
		
		return true;
		
	}
	
	public void workFinish(COORDINATE work) {
		
		HunterInstance ins = getter.get(work);
		if (ins == null)
			return;
		
		Tile j = tile.init(work.x(), work.y(), ins);
		if (j != null) {
			j.cadaver.set(ins, 0);
			j.reserved.set(ins, 0);
		}
		
		
	}
	
	
	private DIR storeDir(COORDINATE c) {
		for (DIR d : DIR.ORTHO) {
			if (SETT.ROOMS().fData.tile.get(c, d) == constructor.rr)
				return d;
		}
		return DIR.C;
	}

}
