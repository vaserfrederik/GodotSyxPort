package settlement.room.service.food.canteen;

import java.io.IOException;
import java.util.Arrays;

import game.GAME;
import game.faction.FResources.RTYPE;
import init.resources.Meal;
import init.resources.RESOURCES;
import init.resources.ResG;
import init.type.NEEDS;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.Industry;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomServiceAccess;
import settlement.room.service.module.RoomServiceAccess.ROOM_SERVICE_ACCESS_HASER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.rnd.RND;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_CANTEEN extends RoomBlueprintIns<CanteenInstance> implements ROOM_EMPLOY_AUTO, ROOM_SERVICE_ACCESS_HASER, INDUSTRY_HASER{

	final Constructor constructor;
	public final Industry industryFuel;
	final RoomServiceAccess service;
	final long[] amounts = new long[RESOURCES.EDI().all().size()];
	long total;
	final SService food = new SService(this);
	final SWork job = new SWork(this);
	final SChair chair = new SChair(this);
	final LIST<Industry> indus;
	
	
	public ROOM_CANTEEN(String key, int index, RoomInitData data, RoomCategorySub cat) throws IOException {
		super(index, data, key, cat);
		
		constructor = new Constructor(this, data);
		industryFuel = new Industry(this, data.data(), null);
		service = new RoomServiceAccess(this, data, NEEDS.TYPES().HUNGER) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return food.get(tx, ty);
			}
		};
		employment().countInputSet();
		indus = new ArrayList<>(industryFuel);
		
	}


	@Override
	protected void update(double ds) {
		
	}

	
	@Override
	public Furnisher constructor() {
		return constructor;
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return service.finder;
	}
	
	

	
	@Override
	protected void saveP(FilePutter saveFile){
		industryFuel.save(saveFile);
		service.saver.save(saveFile);
		
		saveFile.l(total);
		saveFile.lsE(amounts);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		industryFuel.load(saveFile);
		service.saver.load(saveFile);
		total = saveFile.l();
		saveFile.lsE(amounts);
	}
	
	@Override
	protected void clearP() {
		industryFuel.clear();
		service.saver.clear();
		total = 0;
		Arrays.fill(amounts, 0l);
	}
	
	public long totalFood() {
		return total;
	}
	
	public long amount(ResG e) {
		return amounts[e.index()];
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}

	@Override
	public boolean autoEmploy(Room r) {
		return ((CanteenInstance) r).autoE;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((CanteenInstance) r).autoE = b;
	}

	@Override
	public RoomServiceAccess service() {
		return service;
	}
	

	public int grab(LIST<ResG> prefs, int amount, int tx, int ty) {
		
		ResG pref = prefs.rnd();
		CanteenInstance ins = getter.get(tx, ty);
		if (ins == null)
			return Meal.make(pref, 0, 0);
		FSERVICE f = food.get(tx, ty);
		if (f == null)
			return Meal.make(pref, 0, 0);
		f.consume();
		
		
		if (amount > ins.amountTotal()-ins.serviceReserved()+1) {
			amount = ins.amountTotal()-ins.serviceReserved()+1;
		}
		int am = amount;
		
		int ipref = 0;
		int iopref = 0;
		
		
		
		if (am < 0) {
			GAME.Notify("here! " + am + " " + tx + " " + ty);
			return Meal.make(pref, 0, 0);
		}
		
		ResG ee = null;
		
		if (ins.amount(pref) > 0) {
			ee = pref;
			int a = Math.min(ins.amount(pref), am);
			am -= a;
			ipref += a;
			ins.consume(pref, a, tx, ty);
			GAME.player().res().inc(pref.resource, RTYPE.CONSUMED, -a);
			
			
		}
		
		if (am > 0) {
			int ri = RND.rInt(prefs.size());
			for (int i = 0; i < prefs.size() && am > 0; i++) {
				ResG g = prefs.getC(i + ri);
				if (ins.amount(g) > 0) {
					if (ee == null)
						ee = g;
					int a = Math.min(ins.amount(g), am);
					am -= a;
					iopref += a;
					ins.consume(g, a, tx, ty);
					GAME.player().res().inc(g.resource, RTYPE.CONSUMED, -a);
				}
			}
		}
		
		if (am > 0) {
			int ri = RND.rInt(RESOURCES.EDI().all().size());
			for (int i = 0; i < RESOURCES.EDI().all().size() && am > 0; i++) {
				ResG g = RESOURCES.EDI().all().getC(i + ri);
				if (ins.amount(g) > 0) {
					if (ee == null)
						ee = g;
					int a = Math.min(ins.amount(g), am);
					am -= a;
					ins.consume(g, a, tx, ty);
					GAME.player().res().inc(g.resource, RTYPE.CONSUMED, -a);
				}
			}
		}
		
		if (ee == null)
			ee = RESOURCES.EDI().all().rnd();
		
		amount -= am;
		int pt = ipref + iopref;
		double pv = 0;
		if (pt > 0)
			pv = (ipref + 0.25*iopref)/pt;
		return Meal.make(ee, amount, pv);
		
	}
	
	public COORDINATE getChair(int tx, int ty) {
		return chair.get(tx, ty);
	}
	
	public DIR setChair(int tx, int ty, int mealData) {
		return chair.set(tx, ty, mealData);
	}
	
	public void returnChair(int tx, int ty) {
		chair.returnTable(tx, ty);
	}


	@Override
	public LIST<Industry> industries() {
		return indus;
	}
	
}
