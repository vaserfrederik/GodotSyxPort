package settlement.room.service.nursery;

import java.io.IOException;

import game.VERSION;
import game.boosting.Boostable;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.IndustryRate;
import settlement.room.industry.module.RoomBoost;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomService;
import settlement.room.service.module.RoomService.ROOM_SERVICE_HASER;
import settlement.stats.STATS;
import settlement.stats.service.StatServiceChild;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.rnd.RND;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_NURSERY extends RoomBlueprintIns<NurseryInstance> implements ROOM_SERVICE_HASER{
	
	public final String type = "NURSERY";
	final NurseryConstructor constructor;
	final NurseryStation ss;
	final RoomService service;
	
	public final IndustryRate rate;
	
	public static final double playTime = 120;
	public final double ChildPErE = 10.0;
	
	
	public ROOM_NURSERY(int index, RoomInitData init, RoomCategorySub block, String key) throws IOException {
		super(index, init, key, block);
		

		ss = new NurseryStation(this);
		constructor = new NurseryConstructor(this, init);
		pushBo(init.data(), type, true);
		
		service = new RoomService(this, init, null) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return ss.service(tx, ty);
			}
			
			@Override
			public double totalMultiplier() {
				return 1;
			}
		};
		
		rate = new IndustryRate() {
			
			private final ArrayList<RoomBoost> boos = new ArrayList<RoomBoost>(constructor.coziness);
			
			@Override
			public LIST<RoomBoost> boosts() {
				return boos;
			}
			
			@Override
			public Boostable bonus() {
				return bonus;
			}
		};
		
		
	}
	
	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
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
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		if (VERSION.versionIsBefore(71, 38)) {
			service.loadFix(this);
			
		}else {
			service.saver.load(saveFile);
		}
	}
	
	@Override
	protected void clearP() {
		service.saver.clear();
	}


	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}

	@Override
	public RoomService service() {
		return service;
	}



	
	public FSERVICE getOther(COORDINATE c) {
		
		NurseryInstance ins = get(c.x(), c.y());
		if (ins != null) {
			
			COORDINATE t = ins.getWork().get(RND.rInt(ins.getWork().size()));
			FSERVICE s = ss.service(t.x(), t.y());
			if (s.findableReservedCanBe())
				return s;
			
		}
		return null;
		
	}
	
	public StatServiceChild stat() {
		return STATS.SERVICE().nurseries.get(typeIndex());
	}
	

}
