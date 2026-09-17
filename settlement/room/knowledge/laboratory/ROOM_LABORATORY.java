package settlement.room.knowledge.laboratory;

import java.io.IOException;

import settlement.main.SETT;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.consumption.ConsumptionGui;
import settlement.room.industry.module.consumption.ConsumptionJob;
import settlement.room.industry.module.consumption.RoomConsumption;
import settlement.room.industry.module.consumption.RoomConsumption.ROOM_CONSUMPTION_HASER;
import settlement.room.infra.admin.AdminData;
import settlement.room.infra.admin.AdminData.ROOM_ADMIN_HOLDER;
import settlement.room.main.BonusExperience.RoomExperienceBonus;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import util.data.BOOLEANCoo;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_LABORATORY extends RoomBlueprintIns<LaboratoryInstance> implements ROOM_ADMIN_HOLDER, ROOM_CONSUMPTION_HASER{

	public final static String type = "LABORATORY";
	public final AdminData data;
	private final RoomConsumption consumption;
	final ConsumptionJob job;
	final Constructor constructor;
	final BOOLEANCoo isJob = new BOOLEANCoo() {
		
		@Override
		public boolean is(int tx, int ty) {
			return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.WORK;
		}
	};
	public ROOM_LABORATORY(String key, int index, RoomInitData init, RoomCategorySub block) throws IOException {
		super(index, init, key, block);
		
		pushBo(init.data(), type, true);
		consumption = new RoomConsumption(this, init.data(), bonus);
		
		data = new AdminData(employmentExtra(), init.data(), bonus());
		job = new ConsumptionJob(this, consumption, 45, isJob) {
			
			@Override
			protected void perform(double time, double skill) {
				data.perform(time, skill);
				
			}
			
			@Override
			public DIR jobStandDir() {
				for (int di = 0; di < DIR.ORTHO.size(); di++) {
					if (ins.is(coo, DIR.ORTHO.get(di)) && SETT.ROOMS().fData.sprite.is(coo, DIR.ORTHO.get(di), constructor.schair))
						return DIR.ORTHO.get(di);
				}
				return null;
			}
			
			@Override
			public boolean jobUseTool() {
				return false;
			}
			
			@Override
			public boolean jobUseHands() {
				return false;
			}
		};
		

		constructor = new Constructor(this, init);
		
		new RoomExperienceBonus(this, init.data(), bonus);
		employment().countInputSet();
		
	}
	
	@Override
	protected void update(double ds) {
		data.update();
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return null;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		data.save(saveFile);
		consumption.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		data.load(saveFile);
		consumption.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		this.data.clear();
		consumption.clear();
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		
		mm.add(new ConsumptionGui<LaboratoryInstance, ROOM_LABORATORY>(this, consumption).make());
		mm.add(new AdminData.Gui<LaboratoryInstance, ROOM_LABORATORY>(this, data, consumption).make());
		
	}
//
//	public void knowledgeAdd(int i) {
//		data.inc(i);
//		
//	}
//
//	public double knowledgePerStation() {
//		return data.knowledgePerStation;
//	}

	@Override
	public AdminData admin() {
		return data;
	}

	@Override
	public RoomConsumption consumption() {
		return consumption;
	}
	

	
}
