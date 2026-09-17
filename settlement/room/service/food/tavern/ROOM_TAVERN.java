package settlement.room.service.food.tavern;

import java.io.IOException;

import init.race.Race;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.resources.ResG;
import init.resources.ResGDrink;
import init.type.NEEDS;
import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.food.eatery.RoomDistribution;
import settlement.room.service.module.RoomServiceAccess;
import settlement.room.service.module.RoomServiceAccess.ROOM_SERVICE_ACCESS_HASER;
import settlement.stats.colls.StatsFood;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import util.text.D;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_TAVERN extends RoomBlueprintIns<TavernInstance> implements ROOM_SERVICE_ACCESS_HASER, ROOM_EMPLOY_AUTO{
	
	private static CharSequence ¤¤food = "drinks";
	static {
		D.ts(ROOM_TAVERN.class);
	}

	public final RoomServiceAccess serviceData;
	final RoomDistribution dist;
	final Constructor constructor;
	
	public ROOM_TAVERN(String key, int index, RoomInitData data, RoomCategorySub cat) throws IOException {
		super(index, data, key, cat);
		constructor = new Constructor(this, data);
		serviceData = new RoomServiceAccess(this, data, NEEDS.TYPES().THIRST) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return dist.service(tx, ty);
			}
			
		};
		RBITImp bits = new RBITImp();
		for (ResGDrink g : RESOURCES.DRINKS().all()) {
			if (g.serve)
				bits.or(g.resource);
		}
		
		
		dist = new RoomDistribution(this, this, RESOURCES.DRINKS().res(), bits, StatsFood.MAX_RATIONS) {
			
			@Override
			protected boolean isPref(RESOURCE r, Race race) {
				return race.pref().drinkMask.has(r);
			}
			
			@Override
			protected boolean isDeposit(int tx, int ty) {
				return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.ITABLE;
			}
			
			@Override
			protected boolean isCrate(int tx, int ty) {
				return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.ISTORAGE;
				
			}
		};
	}

	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		dist.appendView(mm, ¤¤food);
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return serviceData.finder;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		serviceData.saver.save(saveFile);
		dist.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		serviceData.saver.load(saveFile);
		dist.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		serviceData.saver.clear();
		dist.clear();
	}
	
	@Override
	public RoomServiceAccess service() {
		return serviceData;
	}
	
	@Override
	public boolean autoEmploy(Room r) {
		return ((TavernInstance)r).auto;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((TavernInstance)r).auto = b;
	}
	
	public int consume(LIST<ResG> prefs, int amount, int tx, int ty) {
		return dist.consume(prefs, amount, tx, ty);
	}
	
}
