package settlement.room.health.asylum;

import java.io.IOException;

import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.LOG;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.misc.CLAMP;
import snake2d.util.rnd.RND;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_ASYLUM extends RoomBlueprintIns<AsylumInstance> implements INDUSTRY_HASER{

	
	final Constructor constructor;
	private int prisonersCurrent;
	private int prisonersMax;
	final Industry consumtion;
	final LIST<Industry> indus;
	
	public ROOM_ASYLUM(RoomInitData init, RoomCategorySub block) throws IOException {
		super(0, init, "_ASYLUM", block);
		
		constructor = new Constructor(this, init);
		consumtion = new Industry(this, init.data(), null);
		

		indus = new ArrayList<>(consumtion);
	}
	
	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}
	
	public int prisoners() {
		return prisonersCurrent;
	}
	
	public int prisonersMax() {
		return prisonersMax;
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	void incPrisoners(int p, int total){
		prisonersCurrent += p;
		prisonersMax += total;
	}


	@Override
	protected void saveP(FilePutter f){
		f.i(prisonersCurrent);
		f.i(prisonersMax);
		IndustryUtil.save(f, indus);
	}
	
	@Override
	protected void loadP(FileGetter f) throws IOException{
		prisonersCurrent = f.i();
		prisonersMax = f.i();
		
		prisonersCurrent = 0;
		for (AsylumInstance i : all())
			prisonersCurrent += i.prisoners();
		IndustryUtil.load(f, indus);
	}
	
	@Override
	protected void clearP() {
		prisonersCurrent = 0;
		prisonersMax = 0;
		IndustryUtil.clear(indus);
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		// TODO Auto-generated method stub
		return null;
	}
	
	public COORDINATE registerPrisoner(Humanoid h) {
		if (prisonersCurrent >= prisonersMax)
			return null;
		if (is(h.tc())) {
			AsylumInstance ins = get(h.tc().x(), h.tc().y());
			if (ins.active() && ins.prisoners() < ins.prisonersMax()) {
				return ins.registerPrisoner(h);
			}
		}
		
		int i = RND.rInt(instancesSize());
		for (int k = 0; k < instancesSize(); k++) {
			AsylumInstance ins = getInstance((k+i)%instancesSize());
			if (ins.active() && ins.prisoners() < ins.prisonersMax()) {
				return ins.registerPrisoner(h);
			}
		}
		
		for (int k = 0; k < instancesSize(); k++) {
			AsylumInstance ins = getInstance((k+i)%instancesSize());
			LOG.ln(ins.active() + " " + ins.prisoners() + " " + ins.prisonersMax());
		}
		
		throw new RuntimeException(prisonersCurrent + " " + prisonersMax );
	}
	
	public void unregisterPrisoner(COORDINATE c) {
		if (is(c) && getter.get(c).active()) {
			getter.get(c).removePrisoner(c.x(), c.y());
		}
	}
	
	public boolean eatFood(COORDINATE cell) {
		if (is(cell)) {
			for (int di = 0; di < DIR.ORTHO.size(); di++) {
				DIR dir = DIR.ORTHO.get(di);
				Food f = Food.init(cell.x()+dir.x(), cell.y()+dir.y());
				if (f != null && f.food() > 0) {
					f.consume();
					return true;
				}
			}
		}
		return false;
	}
	
	public boolean isWithinCell(int nx, int ny, COORDINATE cell) {
		if (is(nx, ny) && is(cell.x(), cell.y())) {
			return constructor.isWithinCell(nx, ny, cell.x(), cell.y());
		}
		return false;
	}
	
	public boolean isDoor(COORDINATE cell) {
		return is(cell) && SETT.ROOMS().fData.tileData.get(cell) == Constructor.CODE_ENTRANCE;
	}
	
	public boolean isreserved(COORDINATE cell) {
		return is(cell) && getter.get(cell).active() && getter.get(cell).isReserved(cell.x(), cell.y());
	}
	
	public double treatmentFactor(COORDINATE cell) {
		AsylumInstance i = get(cell.x(), cell.y());
		return treatmentFactor(i);
	}
	
	double treatmentFactor(AsylumInstance i) {
		if (i != null)
			return CLAMP.d(0.25+0.75*(double)(1.0-i.getDegrade())*i.employees().employed()/i.employees().max(), 0, 1);
		return 0;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}

	@Override
	public LIST<Industry> industries() {
		return indus;
	}	

}
