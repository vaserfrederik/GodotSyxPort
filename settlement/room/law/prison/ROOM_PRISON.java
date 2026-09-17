package settlement.room.law.prison;

import java.io.IOException;

import init.resources.RESOURCES;
import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.Industry;
import settlement.room.law.PUNISHMENT_SERVICE;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.LOG;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.rnd.RND;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_PRISON extends RoomBlueprintIns<PrisonInstance> implements PUNISHMENT_SERVICE{

	final static double WORKER_PER_PRISONER = 1d/4d;
	final Constructor constructor;
	private int prisonersCurrent;
	private int prisonersMax;
	final Industry indu;

	
	public ROOM_PRISON(RoomInitData init, RoomCategorySub block) throws IOException {
		super(0, init, "_PRISON", block);
		
		constructor = new Constructor(this, init);
		indu = new Industry(this, 
				RESOURCES.EDI().makeArray(), new double[RESOURCES.EDI().all().size()], null);
	}
	
	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}
	
	
	@Override
	public int punishTotal() {
		return prisonersMax;
	}
	
	@Override
	public int punishUsed() {
		return prisonersCurrent;
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
	protected void saveP(FilePutter saveFile){
		indu.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		indu.load(saveFile);
		prisonersMax = 0;
		prisonersCurrent = 0;
		for (PrisonInstance ins : all()) {
			prisonersCurrent += ins.prisoners();
			if (ins.active()) {
				prisonersMax += ins.prisonersMax();
			}
		}
	}
	
	@Override
	protected void clearP() {
		indu.clear();
		prisonersCurrent = 0;
		prisonersMax = 0;
	}


	@Override
	public SFinderRoomService service(int tx, int ty) {
		// TODO Auto-generated method stub
		return null;
	}
	
	public COORDINATE registerPrisoner(COORDINATE last, COORDINATE current) {
		if (prisonersCurrent >= prisonersMax)
			return null;
		
		if (SETT.IN_BOUNDS(last)){
			PrisonInstance ins = getter.get(last);
			if (ins != null && ins.active() && ins.prisoners() < ins.prisonersMax()) {
				if (SETT.PATH().comps.superComp.get(current) == SETT.PATH().comps.superComp.get(ins.mX(), ins.mY())){
					return ins.registerPrisoner(last);
				}
			}
		}
		
		int i = RND.rInt(instancesSize());
		for (int k = 0; k < instancesSize(); k++) {
			PrisonInstance ins = getInstance((k+i)%instancesSize());
			if (ins.active() && ins.prisoners() < ins.prisonersMax()) {
				if (SETT.PATH().comps.superComp.get(current) == SETT.PATH().comps.superComp.get(ins.mX(), ins.mY())){
					return ins.registerPrisoner(last);
				}
			}
		}
		for (int k = 0; k < instancesSize(); k++) {
			PrisonInstance ins = getInstance((k+i)%instancesSize());
			if (ins.active() && ins.prisoners() < ins.prisonersMax()) {
				LOG.ln("exists!");
				return null;
			}
		}
		
		throw new RuntimeException();
	}
	
	public void unregisterPrisoner(COORDINATE c) {
		if (is(c)) {
			getter.get(c).removePrisoner(c.x(), c.y());
		}
	}
	
	public FSERVICE getFood(COORDINATE cell) {
		if (is(cell)) {
			for (int di = 0; di < DIR.ORTHO.size(); di++) {
				DIR dir = DIR.ORTHO.get(di);
				FSERVICE f = Food.init(cell.x()+dir.x(), cell.y()+dir.y());
				if (f != null) {
					int dx = cell.x() + dir.perpendicular().x();
					int dy = cell.y() + dir.perpendicular().y();
					if (Latrine.init(dx, dy) != null)
						return f;
				}
			}
		}
		return null;
	}
	
	public FSERVICE getLatrine(COORDINATE cell) {
		if (is(cell)) {
			for (int di = 0; di < DIR.ORTHO.size(); di++) {
				DIR dir = DIR.ORTHO.get(di);
				FSERVICE f = Latrine.init(cell.x()+dir.x(), cell.y()+dir.y());
				if (f != null) {
					int dx = cell.x() + dir.perpendicular().x();
					int dy = cell.y() + dir.perpendicular().y();
					if (Food.init(dx, dy) != null)
						return f;
				}
			}
		}
		return null;
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
		return is(cell) && getter.get(cell).isReserved(cell.x(), cell.y());
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}

}
