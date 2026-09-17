package settlement.room.service.breeder;

import java.io.IOException;

import game.VERSION;
import init.race.RACES;
import init.race.Race;
import init.type.HCLASSES;
import settlement.entity.ENTETIES;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.util.RoomInitData;
import settlement.stats.POP;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import util.gui.misc.GText;
import util.info.GFORMAT;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_BREEDER extends RoomBlueprintIns<BreederInstance> implements INDUSTRY_HASER, ROOM_EMPLOY_AUTO {
	
	public final String type = "NURSERY";
	final BreederConstructor constructor;
	final Industry productionData;
	final double PRODUCTION_SPEED_DAY;
	public final Race race;
	int limitTotal = ENTETIES.MAX;
	int limitSpecies = ENTETIES.MAX;
	final LIST<Industry> indus;
	
	final Station station = new Station(this);
	boolean prosecute = false;
	
	public ROOM_BREEDER(int index, RoomInitData init, RoomCategorySub block, String key) throws IOException {
		super(index, init, key, block);
		constructor = new BreederConstructor(this, init);
		pushBo(init.data(), type, true);
		productionData = new Industry(this, init.data(), bonus());
		
		productionData.roomBoosts.add(constructor.coziness);
		
		if (productionData.ins().size() == 0) {
			init.data().error("Nurseries must have an in-resource (food)", "INDUSTRY");
		}
		PRODUCTION_SPEED_DAY = 1.0/init.data().i("INCUBATION_DAYS", 0, Byte.MAX_VALUE);
		race = RACES.map().read("RACE", init.data());
		
		indus = new ArrayList<>(productionData);
		employment().countInputSet();
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
		return null;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		productionData.save(saveFile);
		saveFile.i(limitTotal);
		saveFile.i(limitSpecies);
		saveFile.bool(prosecute);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		productionData.load(saveFile);
		limitTotal = saveFile.i();
		limitSpecies = saveFile.i();
		if (!VERSION.versionIsBefore(71, 15))
			prosecute = saveFile.bool();
	}
	
	@Override
	protected void clearP() {
		productionData.clear();
		limitTotal = ENTETIES.MAX;
		limitSpecies = ENTETIES.MAX;
		prosecute = false;
	}
	
	public boolean canWork() {
		return POP.next(HCLASSES.CITIZEN(), race) < limitSpecies && POP.next(null, null) < limitTotal;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}

	@Override
	public LIST<Industry> industries() {
		return indus;
	}

	@Override
	public boolean autoEmploy(Room r) {
		BreederInstance i = (BreederInstance) r;
		return i.auto;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		BreederInstance i = (BreederInstance) r;
		i.auto = b;
	}


}
