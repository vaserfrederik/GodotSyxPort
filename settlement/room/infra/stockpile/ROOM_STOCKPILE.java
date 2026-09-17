package settlement.room.infra.stockpile;

import java.io.IOException;

import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.job.ROOM_RADIUS.ROOM_RADIUSE;
import settlement.room.main.util.RoomInitData;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.misc.CLAMP;
import snake2d.util.rnd.RND;
import snake2d.util.sets.LISTE;
import util.text.D;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_STOCKPILE extends RoomBlueprintIns<StockpileInstance> implements ROOM_RADIUSE, ROOM_EMPLOY_AUTO{

	public static final int MIN_CARRY = 7;
	
	private final StockpileTally tally = new StockpileTally();
	
	final Constructor constructor;
	
	final Crate crate = new Crate(this);
	private static CharSequence ¤¤bname = "¤Carry Capacity";
	private static CharSequence ¤¤bdesc = "¤Carry Capacity of all logistics workers.";
	
	final Organiser org = new Organiser(this);
	
	static {
		D.ts(ROOM_STOCKPILE.class);
	}
	
	public ROOM_STOCKPILE(RoomInitData init, RoomCategorySub cat) throws IOException {
		super(0, init, "_STOCKPILE", cat);
		constructor = new Constructor(this, init);
		pushBo(init.data(), ¤¤bname, ¤¤bdesc, null, false, MIN_CARRY+3);
		
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
		return null;
	}
	
	public StockpileTally tally() {
		return tally;
	}
	
	@Override
	protected void saveP(FilePutter saveFile){
		tally.saver.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		this.tally.clear();
		for (StockpileInstance ins : all()) {
			tally.init(ins);
		}
		tally.saver.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		this.tally.clear();
		tally.saver.clear();
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}
	
	public int crates() {
		int am = 0;
		for (StockpileInstance ins : all())
			am += ins.crates.size();
		return am;
	}


	@Override
	public boolean autoEmploy(Room r) {
		return ((StockpileInstance) r).autoE;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((StockpileInstance) r).autoE = b;
	}
	
	public int carryCap(Humanoid skill) {
		double dam = bonus.get(skill.indu());
		int am = (int) dam;
		am += RND.rFloat() < dam-am ? 1 : 0;
		am = CLAMP.i(am, 1, 100);
		return am;
	}

	public RoomInstance getInstance(int wI, RESOURCE res) {
		StockpileInstance ins = getInstance(wI);
		
		if (ins != null && SETT.ROOMS().STOCKPILE.tally().crates.get(res, ins) > 0) {
			return ins;
		}
		return null;
	}
	
	@Override
	public ROOM_RADIUS_INSTANCE radiusInstance(Room t) {
		return (StockpileInstance) t;
	}

	
}
