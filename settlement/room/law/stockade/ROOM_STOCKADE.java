package settlement.room.law.stockade;

import java.io.IOException;

import init.resources.RESOURCES;
import settlement.main.SETT;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.Industry;
import settlement.room.law.PUNISHMENT_SERVICE;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
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

public final class ROOM_STOCKADE extends RoomBlueprintIns<StockInstance> implements PUNISHMENT_SERVICE{

	final Constructor constructor;
	final Industry indu;
	final static double PRISONER_PER_TILE = 1.0/4.0;
	final Job job;

	int prisoners;
	int prisonersMax;
	
	public ROOM_STOCKADE(RoomInitData data, RoomCategorySub cat) throws IOException {
		super(0, data, "_STOCKADE", cat);
		
		this.constructor = new Constructor(this, data);
		indu = new Industry(this, 
				RESOURCES.EDI().makeArray(), new double[RESOURCES.EDI().all().size()], 
				 null);
		this.job = new Job(this);
	}

	
	@Override
	protected void update(double ds) {

	}
	
	@Override
	public SFinderRoomService service(int tx, int ty) {
		return null;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		indu.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		indu.load(saveFile);
		prisoners = 0;
		prisonersMax = 0;
		for (StockInstance ins : all()) {
			prisoners += ins.prisonersCurrent;
			if (ins.active()) {
				prisonersMax += ins.prisonersMax;
			}
		}
	}
	
	@Override
	protected void clearP() {
		indu.clear();
		prisoners = 0;
		prisonersMax = 0;
	}
	
	@Override
	public boolean degrades() {
		return false;
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}
	
	@Override
	public double degradeRate() {
		return 0;
	}
	
	public RoomInstance registerPrisoner(COORDINATE current) {
		if (prisoners >= prisonersMax)
			return null;
		
		{
			StockInstance ins = getter.get(current);
			if (ins != null && ins.active() && ins.prisonersCurrent < ins.prisonersMax) {
				if (SETT.PATH().comps.superComp.get(current) == SETT.PATH().comps.superComp.get(ins.mX(), ins.mY())){
					ins.prisonersCurrent ++;
					prisoners ++ ;
					return ins;
				}
				
			}
				
		}
		
		
		int i = RND.rInt(instancesSize());
		for (int k = 0; k < instancesSize(); k++) {
			StockInstance ins = getInstance((k+i)%instancesSize());
			if (ins.active() && ins.prisonersCurrent < ins.prisonersMax) {
				if (SETT.PATH().comps.superComp.get(current) == SETT.PATH().comps.superComp.get(ins.mX(), ins.mY())){
					ins.prisonersCurrent ++;
					prisoners ++ ;
					return ins;
				}
			}
		}
		LOG.ln("nopes");
		return null;
	}
	
	public void unregisterPrisoner(COORDINATE c) {
		StockInstance ins = getter.get(c);
		if (ins != null && ins.active()) {
			ins.prisonersCurrent --;
			prisoners --;
		}
	}
	
	public void unregisterPrisoner(int tx, int ty) {
		StockInstance ins = getter.get(tx, ty);
		if (ins != null && ins.active()) {
			ins.prisonersCurrent --;
			prisoners --;
		}
	}
	
	public COORDINATE foodReserve(COORDINATE c) {
		StockInstance ins = getter.get(c);
		if (ins != null) {
			
			int ri = RND.rInt(ins.jobs.size());
			
			for (int i = 0; i < ins.jobs.size(); i++) {
				COORDINATE coo = ins.jobs.get((ri+i)%ins.jobs.size());
				if (job.reserve(coo.x(), coo.y(), Job.IFOOD, true, false))
					return coo;
			}
		}
		return null;
	}
	
	public void foodUse(COORDINATE c, boolean use) {
		job.reserve(c.x(), c.y(), Job.IFOOD, false, use);
	}
	
	public COORDINATE latrineReserve(COORDINATE c) {
		StockInstance ins = getter.get(c);
		if (ins != null) {
			
			int ri = RND.rInt(ins.jobs.size());
			
			for (int i = 0; i < ins.jobs.size(); i++) {
				COORDINATE coo = ins.jobs.get((ri+i)%ins.jobs.size());
				if (job.reserve(coo.x(), coo.y(), Job.ISHIT, true, false))
					return coo;
			}
		}
		return null;
	}
	
	public void latrineUse(COORDINATE c, boolean use) {
		job.reserve(c.x(), c.y(), Job.ISHIT, false, use);
	}
	
	public boolean isWithin(int nx, int ny, COORDINATE cell) {
		StockInstance ins = getter.get(nx, ny);
		if (ins != null && ins.is(cell)) {
			for (int di = 0; di < DIR.ALL.size(); di++) {
				if (!ins.is(cell, DIR.ALL.get(di)))
					return false;
			}
			return true;
		}
		return false;
		
	}


	@Override
	public int punishTotal() {
		return prisonersMax;
	}


	@Override
	public int punishUsed() {
		return prisoners;
	}
	
}
