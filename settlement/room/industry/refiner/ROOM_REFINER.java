package settlement.room.industry.refiner;

import java.io.IOException;

import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.industry.module.RoomBoost;
import settlement.room.main.BonusExperience.RoomExperienceBonus;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public class ROOM_REFINER extends RoomBlueprintIns<RefinerInstance> implements INDUSTRY_HASER{

	public final static String type = "REFINER";
	final Job job;
	final LIST<Industry> indus;
	final Constructor constructor;
	
	public ROOM_REFINER(RoomInitData init, String key, int index, RoomCategorySub cat) throws IOException {
		super(index, init, key, cat);
		
		

		
		constructor = new Constructor(init, this);
		pushBo(init.data(), type, true);
		indus = Industry.createIndustries(this, init, new RoomBoost[] {constructor.efficiency}, bonus());
		job = new Job(this, init.data().i("STORAGE", 8, 500));
		new RoomExperienceBonus(this, init.data(), bonus());
		employment().countInputSet();
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
		IndustryUtil.save(saveFile, indus);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		IndustryUtil.load(saveFile, indus);
	}
	
	@Override
	protected void clearP() {
		IndustryUtil.clear(indus);
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

}
