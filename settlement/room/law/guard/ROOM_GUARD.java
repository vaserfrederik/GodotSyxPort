package settlement.room.law.guard;

import java.io.IOException;

import game.battle.div.Div;
import init.constant.Config;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.employment.RoomEmploymentSimple.EmployerSimple;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.Bitmap1D;
import snake2d.util.sets.LISTE;
import util.data.BOOLEANO.BOOLEAN_OE;
import util.info.INFO;
import util.text.D;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_GUARD extends RoomBlueprintIns<GuardInstance>{

	private static CharSequence ¤¤guard = "Active Duty";
	private static CharSequence ¤¤guardD = "When a division is on active duty, the soldiers will become guards who actively protects your city against crime, and instills order and law.";
	
	static {
		D.ts(ROOM_GUARD.class);
	}
	
	
	public final static int maxRadius = 90;

	final SFinderRoomService finder;
	
	final Constructor constructor;
	final Service service = new Service(this);
	
	public final EmployerSimple emp = new EmployerSimple(employment());
	
	public final Patrols patrols = new Patrols();
	public final GuardPower power = new GuardPower();
	public final CrimeReporter reporter = new CrimeReporter(this); 
	
	private final Bitmap1D guardMode = new Bitmap1D(Config.battle().DIVISIONS_PER_ARMY, false);
	
	
	public ROOM_GUARD(RoomInitData init, RoomCategorySub block) throws IOException {
		super(0, init, "_GUARD", block);
		finder = new SFinderRoomService("Guards") {
			
			@Override
			public FSERVICE get(int tx, int ty) {
				GuardInstance ins = getter.get(tx, ty);
				if (ins != null && ins.body().cX() == tx && ins.body().cY() == ty)
					return service.get(ins);
				return null;
			}
		};
		constructor = new Constructor(this, init);
		
		
		
	}
	
	@Override
	protected void update(double ds) {
		patrols.update(ds);
		
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	public SFinderRoomService service(int tx, int ty) {
		return finder;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		guardMode.save(saveFile);
		power.save(saveFile);
		patrols.save(saveFile);
		reporter.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		guardMode.load(saveFile);
		power.load(saveFile);
		patrols.load(saveFile);
		reporter.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		guardMode.clear();
		power.clear();
		patrols.clear();
		reporter.clear();
	}

	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}
	
	public BOOLEAN_OE<Div> activeDuty = new BOOLEAN_OE<Div>() {
		
		private final INFO info = new INFO(¤¤guard, ¤¤guardD);
		
		@Override
		public boolean is(Div t) {
			return guardMode.get(t.indexArmy());
		}

		@Override
		public BOOLEAN_OE<Div> set(Div t, boolean b) {
			guardMode.set(t.indexArmy(), b);;
			return this;
		}
		
		@Override
		public INFO info() {
			return info;
		};
		
	};
	
	
}
