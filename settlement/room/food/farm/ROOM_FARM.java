package settlement.room.food.farm;

import java.io.IOException;

import game.time.TIME;
import init.resources.Growable;
import init.resources.RESOURCES;
import init.type.TERRAINS;
import settlement.misc.util.RESOURCE_TILE;
import settlement.misc.util.TILE_STORAGE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryRegion;
import settlement.room.industry.module.RoomBoost;
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
import world.map.regions.RegionInfo;

public class ROOM_FARM extends RoomBlueprintIns<FarmInstance> implements INDUSTRY_HASER, ROOM_IRRIGATED{

	static double WORKERPERTILE = TIME.workSeconds()/(Tile.WORK_TIME+TIME.workSecondsWalkNext());
	static double WORKERPERTILEI = 1.0/WORKERPERTILE;
	
	
	public final Growable crop;
	public final static String type = "FARM";
	
	final Constructor constructor;
	
	final Industry productionData;
	
	final LIST<Industry> indus;

	final Tile tile;
	final Time time;
	
	final double yearMul = TIME.years().bitSeconds()/(16*TIME.secondsPerDay());
	private final RoomIrrigated irri;

	public ROOM_FARM(RoomInitData data, String key, RoomCategorySub cat, int index) throws IOException {
		super(index, data, key, cat);
		crop = RESOURCES.growable().MAP.read(data.data());
		
		constructor = new Constructor(this, data);
		pushBo(data.data(), type, true);
		
		RoomBoost mo = WeatherMoisture.makeBoost();
		
		productionData = new Industry(this, data.data(), bonus());
		productionData.roomBoosts.add(constructor.fertility);
		productionData.roomBoosts.add(mo);
		
		new IndustryRegion(productionData, 1.0) {
			
			@Override
			public double occurence(Region reg) {
				if (constructor().mustBeOutdoors())
					return RegionInfo.vFer().getAi(reg);
				return reg.info.terrain(TERRAINS.MOUNTAIN());
			}
		};
		indus = new ArrayList<>(productionData);

		
		time = new Time(this);
		tile = new Tile(this);
		new RoomExperienceBonus(this, data.data(), bonus());
		
		double ibonus = 1;
		{
			double period = TIME.years().bitConversion(TIME.days())-1;
			double degrade = 1-productionData.outs().get(0).resource.degradeSpeed()/(2*period);
			double consumption = 1;
			
			double res = 0;
			
			for (int i = 0; i < period; i++) {
				res += consumption;
				res /= degrade;
			}
			
			ibonus = res/(period*consumption);
			ibonus = ((int)(ibonus*100)/100.0);
			ibonus += 0.05;
		}
		
		
		irri = new RoomIrrigated(this,  bonus(), 0.05, ibonus) {
			
			@Override
			public double needed(AREA area) {
				return area.area();
			}
			
			@Override
			protected double irrigation(RoomInstance ins) {
				return ((FarmInstance)ins).irri;
			}
		};
		
		degradeRate = 0;
	}
	
	Tile tile(int tx, int ty) {
		return tile.get(tx, ty);
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
	public boolean degrades() {
		return false;
	}

	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}

	@Override
	public LIST<Industry> industries() {
		return indus;
	}



	public boolean isGoodDayForEvent() {
		return time.dayI() == time.dayEvent;
	}
	
	@Override
	public boolean industryIgnoreUI() {
		return true;
	}
	
	public boolean shouldReportWorkFailure() {
		return (time.dayI() != time.dayOffWork && time.dayI() != time.dayHarvest);
	}

	@Override
	public RoomIrrigated irrigation() {
		return irri;
	}
	
	public double fer() {
		return getStat(constructor.fertility.index());
	}

	public RESOURCE_TILE toStore(int tx, int ty) {
		FarmInstance ins = get(tx, ty);
		if (ins == null)
			return null;
		
		return ins.getResTile();
	}
	
	public TILE_STORAGE toStoreTo(int tx, int ty) {
		FarmInstance ins = get(tx, ty);
		if (ins == null)
			return null;
		return ins.getStoreTile();
	}
	


	
}
