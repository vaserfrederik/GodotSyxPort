package settlement.room.knowledge.school;

import java.io.IOException;

import settlement.entity.humanoid.Humanoid;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryResource;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomService;
import settlement.room.service.module.RoomService.ROOM_SERVICE_HASER;
import settlement.stats.STATS;
import settlement.stats.colls.StatsEducation.AgeType;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import util.gui.misc.GText;
import util.info.GFORMAT;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_SCHOOL extends RoomBlueprintIns<SchoolInstance> implements INDUSTRY_HASER, ROOM_SERVICE_HASER{

	final Industry industry;
	final SchoolConstructor constructor;
	final RoomService service;
	final SchoolStation station = new SchoolStation(this);
	final LIST<Industry> indus;

	private final RoomEducationHelper helper;
	
	public ROOM_SCHOOL(String key, int index, RoomInitData init, RoomCategorySub block) throws IOException {
		super(index, init, key, block);
		service = new RoomService(this, init, null) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return station.service(tx, ty);
			}
		};
		pushBo(init.data(), type, true);
		constructor = new SchoolConstructor(this, init);
		
		helper = new RoomEducationHelper(this, constructor.quality) {

			@Override
			public AgeType type() {
				return STATS.EDUCATION().child;
			}
			
		};

		industry = new Industry(this, init.data(), null) {
			
			@Override
			public double consumptionRate(RoomInstance ins, Humanoid h, IndustryResource oo) {
				if (ins.employees().employed() == 0)
					return 0;
				double d = oo.rate*service.load()*service.total()/ins.employees().employed();
				return d;
			}
			
		};
		employment().countInputSet();

		indus = new ArrayList<>(industry);
	}
	
	@Override
	protected void update(double ds) {

	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return service.finder;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		service.saver.save(saveFile);
		industry.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		service.saver.load(saveFile);
		industry.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		service.saver.clear();
		industry.clear();
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		helper.appendView(mm);
	}

	@Override
	public LIST<Industry> industries() {
		return indus;
	}

	@Override
	public RoomService service() {
		return service;
	}
	
	
//	public StatServiceChild stat() {
//		return STATS.SERVICE().schools.get(typeIndex());
//	}
//	
	public DIR childDir(int sx, int sy) {
		return station.serviceDir(sx, sy);
	}
	
	public double learningSpeed(Humanoid student, int tx, int ty) {
		return helper.learningSpeed(student, tx, ty);
	}
	
	@Override
	public double industryFormatConsumptionRate(GText text, IndustryResource i, RoomInstance ins) {
		SchoolInstance sc = (SchoolInstance) ins;
		
		double d = i.rate*sc.service().load()*sc.service().total();
		GFORMAT.f0(text, -d);
		return d;
	}
	
}
