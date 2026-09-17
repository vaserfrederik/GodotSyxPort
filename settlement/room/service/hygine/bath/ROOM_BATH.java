package settlement.room.service.hygine.bath;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryResource;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomServiceNeed;
import settlement.room.service.module.RoomServiceNeed.ROOM_SERVICE_NEED_HASER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import util.gui.misc.GBox;
import util.gui.misc.GText;
import util.info.GFORMAT;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_BATH extends RoomBlueprintIns<BathInstance> implements ROOM_SERVICE_NEED_HASER, INDUSTRY_HASER, ROOM_EMPLOY_AUTO{

	final RoomServiceNeed data;
	final Constructor constructor;
	final CharSequence sHeating;
	final CharSequence sHeatingDesc;
	final CharSequence sHeatingProblem;
	final CharSequence sWaterProblem;
	final Industry consumtion;
	final LIST<Industry> indus;

	public ROOM_BATH(String key, int index, RoomInitData init, RoomCategorySub block) throws IOException {
		super(index, init, key, block);
		
		data = new RoomServiceNeed(this, init) {
			@Override
			public FSERVICE service(int tx, int ty) {
				return Bath.init(tx, ty, ROOM_BATH.this);
			}

		};
		
		constructor = new Constructor(this,init);
		sHeating = init.text().text("HEATING");
		sHeatingDesc = init.text().text("HEATING_DESC");
		sHeatingProblem = init.text().text("HEATING_PROBLEM");
		sWaterProblem = init.text().text("WATER_PROBLEM");
		consumtion = new Industry(this, init.data(), null) {
			@Override
			public double consumptionRate(RoomInstance ins, Humanoid h, IndustryResource oo) {
				BathInstance i = (BathInstance) ins;
				if (ins.employees().employed() == 0)
					return 0;
				return oo.rate*i.service.total()/ins.employees().employed();
			}
		};
		
		indus = new ArrayList<>(consumtion);
		employment().countInputSet();
	}


	public Bath bath(int tx, int ty) {
		return Bath.init(tx, ty, this);
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
		return data.finder;
	}
	
	@Override
	public RoomServiceNeed service() {
		return data;
	}
	
	public static boolean isPool(int tx, int ty) {
		Room r = SETT.ROOMS().map.get(tx, ty);
		if (r != null && r instanceof BathInstance) {
			int d = ROOMS().data.get(tx, ty);
			return (d & Bits.BITS) == Bits.POOL && (d & 1) == 1;
		}
		return false;
	}
	
	public boolean isBench(int tx, int ty) {
		if (is(tx, ty)) {
			int d = ROOMS().data.get(tx, ty);
			return (d & Bits.BITS) == Bits.BENCH;
		}
		return false;
	}
	
	public DIR getBenchDir(int tx, int ty) {
		if (!isBench(tx, ty))
			throw new RuntimeException();
		for (DIR d : DIR.ORTHO) {
			int da = ROOMS().data.get(tx, ty, d);
			if (da == Bits.BENCH_TAIL)
				return d;
		}
		throw new RuntimeException();
	}

	@Override
	protected void saveP(FilePutter file){
		data.saver.save(file);
		consumtion.save(file);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		data.saver.load(saveFile);
		consumtion.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		this.data.saver.clear();
		consumtion.clear();
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
		return ((BathInstance)r).auto;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((BathInstance)r).auto = b;
	}

	
	@Override
	public double industryFormatConsumptionRate(GText text, IndustryResource i, RoomInstance ins) {
		BathInstance uu = (BathInstance) ins;
		double n = i.rate*uu.service.total(); 
		GFORMAT.f0(text, -n);
		return n;
	}
	
	@Override
	public void industryHoverConsumptionRate(GBox b, IndustryResource i, RoomInstance ins) {
		
	}
	
}
