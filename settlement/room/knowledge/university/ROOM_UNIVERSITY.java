package settlement.room.knowledge.university;

import java.io.IOException;

import game.boosting.BOOSTABLE_O;
import game.time.TIME;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.path.finders.SFinderRoomService;
import settlement.room.knowledge.school.RoomEducationHelper;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.employment.RoomEmploymentSimple.EmployerSimple;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.stats.STATS;
import settlement.stats.colls.StatsEducation.AgeType;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import util.data.BOOLEAN;
import util.text.D;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_UNIVERSITY extends RoomBlueprintIns<UniversityInstance>{

	
	
	final UniversityConstructor constructor;
	public final double learningSpeed;
	final Job job = new Job(this);
	
	private final RoomEducationHelper helper;
	
	private static CharSequence ¤¤bonus = "Learning speed of";
	
	static {
		D.ts(ROOM_UNIVERSITY.class);
	}
	
	public final EmployerSimple emp = new EmployerSimple(employment());
	
	public ROOM_UNIVERSITY(String key, int index, RoomInitData init, RoomCategorySub block) throws IOException {
		super(index, init, key, block);
	

		constructor = new UniversityConstructor(this, init);
		learningSpeed = init.data().d("LEARNING_SPEED", 0, 100);
		clearP();
		pushBo(init.data(), info.name, ¤¤bonus + ": " + info.name, "UNIVERSITY", true);
		helper = new RoomEducationHelper(this, constructor.quality) {
			
			@Override
			public AgeType type() {
				return STATS.EDUCATION().adult;
			}
		};
	}
	
	@Override
	protected void update(double ds) {

	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return null;
	}

	@Override
	protected void saveP(FilePutter f){

	}
	
	@Override
	protected void loadP(FileGetter f) throws IOException{

		
	}
	
	@Override
	protected void clearP() {

	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		helper.appendView(mm);
	}
	
	public BOOLEAN isTime = new BOOLEAN() {

		@Override
		public boolean is() {
			return TIME.days().bitPartOf() > employment().getShiftStart() && TIME.days().bitPartOf() < employment().getShiftStart()+Humanoid.WORK_PER_DAY;
		}
		
	};
	
	public double learningSpeed(RoomInstance i, BOOSTABLE_O h) {
		return helper.learningSpeed(i, h);
	}

	
	public boolean isLecturer(COORDINATE c) {
		return SETT.ROOMS().fData.tileData.get(c) == UniversityConstructor.IWORKE;
	}
	
	public DIR spotDir(COORDINATE c) {
		return DIR.ORTHO.get(SETT.ROOMS().fData.spriteData.get(c) & 0b011);
	}
	
}
