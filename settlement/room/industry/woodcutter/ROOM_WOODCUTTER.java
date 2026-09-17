package settlement.room.industry.woodcutter;

import java.io.IOException;

import init.type.TERRAINS;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryRegion;
import settlement.room.main.BonusExperience.RoomExperienceBonus;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.water.RoomIrrigated;
import settlement.room.water.RoomIrrigated.ROOM_IRRIGATED;
import settlement.weather.WeatherMoisture;
import snake2d.util.datatypes.AREA;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;
import world.map.regions.Region;

public class ROOM_WOODCUTTER extends RoomBlueprintIns<Instance> implements INDUSTRY_HASER, ROOM_IRRIGATED{

	final Job job;
	final Industry productionData;
	final Constructor constructor;
	final LIST<Industry> indus;
	final RoomIrrigated irrigation;
	
	public ROOM_WOODCUTTER(RoomInitData init, RoomCategorySub cat) throws IOException {
		super(0, init, "_WOODCUTTER", cat);
		
		
		constructor = new Constructor(init, this);
		pushBo(init.data(), null, true);
				
		
		productionData = new Industry(this, init.data(), bonus());
		productionData.roomBoosts.add(constructor.efficiency);
		
		productionData.roomBoosts.add(WeatherMoisture.makeBoost());
		new IndustryRegion(productionData, 1.0) {
			
			@Override
			public double occurence(Region reg) {
				return reg.info.terrain(TERRAINS.FOREST());
			}
		};
		
		job = new Job(this, init.data().i("STORAGE", 8, 500));
		indus = new ArrayList<>(productionData);
		
		
		new RoomExperienceBonus(this, init.data(), bonus());
		
		irrigation = new RoomIrrigated(this, bonus, 0.75, 1.05) {
			
			@Override
			public double needed(AREA area) {
				return area.area();
			}

			@Override
			protected double irrigation(RoomInstance ins) {
				return ((Instance)ins).irri;
			}
			
		};
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}

	
	
	@Override
	protected void update(double ds) {
		
	}
	
	@Override
	public SFinderRoomService service(int tx, int ty) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		productionData.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		productionData.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		productionData.clear();
	}
	
	@Override
	public boolean makesDudesDirty() {
		return true;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
	
	}
	
	@Override
	public LIST<Industry> industries() {
		return indus;
	}

	@Override
	public RoomIrrigated irrigation() {
		return irrigation;
	}

}
