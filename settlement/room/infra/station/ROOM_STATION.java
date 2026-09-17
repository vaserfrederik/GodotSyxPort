package settlement.room.infra.station;

import java.io.IOException;

import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.type.HCLASS_RACE;
import settlement.main.SETT;
import settlement.path.finders.SFinderRoomService;
import settlement.room.infra.station.StationTally.Total;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.util.RoomInitData;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_STATION extends RoomBlueprintIns<StationInstance> implements ROOM_EMPLOY_AUTO {

	public final static int MAX_EMPLOYEES = 15;

	final Constructor constructor;
	//final Job cart = new Job(this);
	final Job job = new Job(this);
	
	final Crate crate = new Crate(this);
	private final StationTally.Total[] tallies = new StationTally.Total[RESOURCES.ALL().size()];
	private final int[] ri = Alloc.ii(RESOURCES.ALL().size());
	
	public ROOM_STATION(RoomInitData init, RoomCategorySub cat) throws IOException {
		super(0, init, "_STATION", cat);
		constructor = new Constructor(this, init);
		for (int i = 0; i < tallies.length; i++)
			tallies[i] = new Total(RESOURCES.ALL().get(i));
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
	
	@Override
	protected void saveP(FilePutter saveFile){
		RESOURCES.map().saver().save(ri, saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		
		RESOURCES.map().loader().load(ri, saveFile, 0);
		
		for (Total t : tallies)
			t.clear();
		
		for (int i = 0; i < instancesSize(); i++) {
			StationInstance ins = getInstance(i);
			ins.bamount = new RBITImp();
			ins.bcapacity = new RBITImp();
			ins.tally = new StationTally[RESOURCES.ALL().size()];
			
			for (RESOURCE res : RESOURCES.ALL())
				ins.tally[res.index()] = new StationTally();
			for (COORDINATE c : ins.body()) {
				if (ins.is(c) && crate.get(c.x(), c.y()) != null && crate.get(c.x(), c.y()).resource() != null) {
					ins.tally(crate.resource()).add(crate.resource(), crate, ins);
				}
			}
			
		}
		
		for (Total t : tallies)
			t.clear();
		for (int i = 0; i < instancesSize(); i++) {
			StationInstance ins = getInstance(i);
			for (Total t : tallies)
				t.add(ins.tally(t.res), ins);
			
		}
		
		for (Total t : tallies)
			t.debug();
		
	}
	
	@Override
	protected void clearP() {
		
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}

	@Override
	public boolean autoEmploy(Room r) {
		return ((StationInstance)r).auto;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((StationInstance)r).auto = b; 
	}
	
	public StationTally.Total tally(RESOURCE res){
		return tallies[res.index()];
	}

	private final Coo coo = new Coo();
	
	public COORDINATE reserve(RESOURCE res) {
		if (tally(res).accepting() <= 0)
			return null;
		int oi = ri[res.index()];
		
		for (int i = 0; i < instancesSize(); i++) {
			
			if (oi >= instancesSize())
				oi = 0;
			StationInstance ins = getInstance(oi);
			oi++;
			if (ins.accepting(res)) {
				for (COORDINATE c : ins.body()) {
					if (ins.is(c) && (SETT.ROOMS().fData.tileData.get(c) & Constructor.BIT_DEST) != 0) {
						ins.reserve(res);
						coo.set(c);
						return coo;
					}
				}
				throw new RuntimeException();
			}
			
			
		
		}
		
		int k = 0;
		for (int i = 0; i < instancesSize(); i++) {
			k += getInstance(i).accepting(res) ? 1 : 0;
		}
		
		throw new RuntimeException(res + " " + tally(res).accepting() + " " + k + " " + instancesSize());
	}
	
	public void reserveCancel(RESOURCE res, int tx, int ty) {
		StationInstance ins = get(tx, ty);
		if (ins != null)
			ins.unreserve(res);
	}
	
	public void deliver(RESOURCE res, int am, int tx, int ty) {
		StationInstance ins = get(tx, ty);
		if (ins != null)
			ins.deliver(res, am);
	}
	
	public double workersPerload(int tx, int ty) {
		StationInstance ins = get(tx, ty);
		if (ins == null)
			return MAX_EMPLOYEES;
		double bo = SETT.ROOMS().STOCKPILE.bonus().get(HCLASS_RACE.clP())/SETT.ROOMS().STOCKPILE.bonus().baseValue;
		return MAX_EMPLOYEES/(bo*ins.efficiency()*MAX_EMPLOYEES);
	}



}
