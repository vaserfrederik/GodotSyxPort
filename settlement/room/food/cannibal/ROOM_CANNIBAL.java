package settlement.room.food.cannibal;

import java.io.IOException;
import java.util.Arrays;

import game.time.TIME;
import init.race.RACES;
import init.race.Race;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.resources.RES_AMOUNT;
import settlement.main.SETT;
import settlement.path.finders.SFinderRoomService;
import settlement.room.law.PUNISHMENT_SERVICE;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.stats.STATS;
import snake2d.LOG;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.misc.CLAMP;
import snake2d.util.rnd.RND;
import snake2d.util.sets.LISTE;
import util.data.BOOLEANO.BOOLEAN_OE;
import util.data.BOOLEANO.BooleanOEImp;
import util.info.INFO;
import util.text.D;
import view.sett.ui.room.UIRoomModule;

public class ROOM_CANNIBAL extends RoomBlueprintIns<CannibalInstance> implements PUNISHMENT_SERVICE{

	private static CharSequence ¤¤eat = "Slaughter";
	private static CharSequence ¤¤eatD = "Prisoners convicted to be executed can be put to good use in the Cannibal.";
	
	static {
		D.ts(ROOM_CANNIBAL.class);
	}
	
	final Job job;
	final int[] produced = Alloc.ii(RESOURCES.ALL().size());
	private int year = -1;
	private final double[] cannibalism = new double[RACES.all().size()];
	final Constructor constructor;
	
	private Cage cage = new Cage(this);
	private RESOURCE[] resources;
	
	int prisoners;
	
	private final BooleanOEImp<Race> permission = new BooleanOEImp<Race>(RACES.all().size(), true);
	
	public ROOM_CANNIBAL(RoomInitData init, RoomCategorySub cat) throws IOException {
		super(0, init, "_CANNIBAL", cat);
		
		constructor = new Constructor(this, init);
		job = new Job(this);
		
		
		permission.info = new INFO(¤¤eat, ¤¤eatD);
		

	}
	
	RESOURCE[] resources() {
		
		RBITImp m = new RBITImp();
		
		if (resources == null) {
			int am = 0;
			for (Race race : RACES.all()) {
				for (RES_AMOUNT r : race.resources()) {
					if (!m.has(r.resource())) {
						am++;
						m.or(r.resource());
					}
				}
			}
			RESOURCE[] res = new RESOURCE[am];
			m.clear();
			am = 0;
			for (Race race : RACES.all()) {
				for (RES_AMOUNT r : race.resources()) {
					if (!m.has(r.resource())) {
						res[am++] = r.resource();
						m.or(r.resource());
					}
				}
			}
			resources = res;
		}
		
		return resources;
	}
	
	@Override
	protected void update(double ds) {
		if (year != TIME.years().bitsSinceStart()) {
			Arrays.fill(produced, 0);
			year = TIME.years().bitsSinceStart();
		}
		
		for (int ri = 0; ri < RACES.all().size(); ri++) {

			double d = cannibalism[ri];
			if (d < 1)
				d = 1;
			
			cannibalism[ri] -= d*ds/(TIME.years().bitSeconds()*2*cannibalism.length);
			cannibalism[ri] = CLAMP.d(cannibalism[ri], 0, 1.5);
		}
		
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	protected void saveP(FilePutter f){
		f.isE(produced);
		f.i(year);
		f.dsE(cannibalism);
		f.i(prisoners);
		permission.save(f);
	}
	
	@Override
	protected void loadP(FileGetter f) throws IOException{
		f.isE(produced);
		year = f.i();
		f.dsE(cannibalism);
		prisoners = f.i();
		permission.load(f);
	}
	
	@Override
	protected void clearP() {
		year = -1;
		Arrays.fill(produced, 0);
		Arrays.fill(cannibalism, 0);
		permission.clear();
		prisoners = 0;
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		
	}
	
	public void reportCannibal(Race race) {
		cannibalism[race.index] += 100.0/STATS.POP().POP.data().get(null);
	}
	
	public double cannibalism(Race race) {
		return CLAMP.d(cannibalism[race.index], 0, 1);
	}
	
	@Override
	public BOOLEAN_OE<Race> punishEnabled() {
		return permission;
	}
	
	public void setRace(int tx, int ty, Race race) {
		CannibalInstance ins = get(tx, ty);
		if (ins != null) {
			int d = SETT.ROOMS().data.get(tx, ty);
			d = Job.race.set(d, race.index());
			SETT.ROOMS().data.set(ins, tx, ty, d);
		}
			
	}

	@Override
	public int punishTotal() {
		return employment().employedMax();
	}

	@Override
	public int punishUsed() {
		return prisoners;
	}
	
	public Cage getPrisonerCage() {
		if (prisoners >= punishTotal()) {
			return null;
		}
		
		if (instancesSize() == 0)
			return null;
		
		int ri = RND.rInt(instancesSize());
		
		for (int i = 0; i < instancesSize(); i++) {
			CannibalInstance ins = getInstance((i+ri)%instancesSize());
			if (ins.prisoners < ins.employees().max()) {
				for (int ci = 0; ci < ins.cages.size(); ci++) {
					Cage ca = cage(ins.cages.get().x(), ins.cages.get().y());
					if (ca.available())
						return ca;
					ins.cages.inc();
				}
				LOG.err("nono");
				return null;
			}
		}
		
		LOG.err("nono2");
		return null;
		
	}
	
	public Cage getWorkCage(RoomInstance work) {
		
		CannibalInstance ins = (CannibalInstance) work;
		
		if (ins.reservable <= 0)
			return null;
		
		for (int ci = 0; ci < ins.cages.size(); ci++) {
			Cage ca = cage(ins.cages.get().x(), ins.cages.get().y());
			if (ca.canGrab())
				return ca;
			ins.cages.inc();
		}
		LOG.err("nono");
		return null;
	}
	
	public Cage cage(int tx, int ty) {
		return cage.get(tx, ty);
		
	}
	

}
