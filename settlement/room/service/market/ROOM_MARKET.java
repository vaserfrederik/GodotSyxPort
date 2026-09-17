package settlement.room.service.market;

import java.io.IOException;

import game.GAME;
import init.race.RACES;
import init.race.Race;
import init.race.RaceResources.RaceResource;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import init.resources.ResG;
import init.type.HCLASSES;
import init.type.NEEDS;
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
import settlement.stats.STATS;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.misc.ACTION;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LISTE;
import util.text.D;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_MARKET extends RoomBlueprintIns<MarketInstance> implements ROOM_EMPLOY_AUTO, ROOM_SERVICE_ACCESS_HASER{

	private static CharSequence ¤¤food = "wares";
	static {
		D.ts(ROOM_MARKET.class);
	}
	final Constructor constructor;
	final RoomServiceAccess service;
	RoomDistribution dist;
	
	public ROOM_MARKET(String key, int index, RoomInitData data, RoomCategorySub cat) throws IOException {
		super(index, data, key, cat);
		
		constructor = new Constructor(this, data);
		
		service = new RoomServiceAccess(this, data, NEEDS.TYPES().SHOPPING) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return dist.service(tx, ty);
			}

		};
		
		GAME.addOnInit(new ACTION() {
			
			@Override
			public void exe() {
				ArrayListGrower<RESOURCE> ress = new ArrayListGrower<RESOURCE>();
				RBITImp bits = new RBITImp();
				
				for (RaceResource r : RACES.res().ALL) {
					ress.add(r.res);
					bits.or(r.res.bit);
				}
				
				dist = new RoomDistribution(ROOM_MARKET.this, ROOM_MARKET.this, ress, bits, 1) {
					
					@Override
					protected boolean isPref(RESOURCE r, Race race) {
						return race.home().clas(HCLASSES.CITIZEN()).amount(r) > 0;
					}
					
					@Override
					protected boolean isDeposit(int tx, int ty) {
						return constructor.isStore(tx, ty);
					}
					
					@Override
					protected boolean isCrate(int tx, int ty) {
						return constructor.isCrate(tx, ty);
					}
				};
			}
		});
		
		
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
		service.saver.save(saveFile);
		dist.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		service.saver.load(saveFile);
		dist.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		service.saver.clear();
		dist.clear();
	}
	
	public long totalFood() {
		return dist.tStored.total.get();
	}
	
	public long amount(ResG e) {
		return dist.stored(e.resource).total.get();
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		dist.appendView(mm, ¤¤food);
	}

	@Override
	public boolean autoEmploy(Room r) {
		return ((MarketInstance) r).autoE;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((MarketInstance) r).autoE = b;
	}

	@Override
	public RoomServiceAccess service() {
		return service;
	}
	
	
	public int buy(RaceResource res, int amount, int tx, int ty) {
	
		return dist.consume(res.res, amount, tx, ty);
	}
	
	@Override
	public boolean registersEnvironment() {
		return true;
	}
	
}
