package settlement.room.service.module;

import java.io.IOException;

import game.audio.AUDIO;
import game.audio.SoundRace;
import game.time.TIME;
import init.type.HCLASS_RACE;
import init.type.NEED;
import init.type.NEEDS;
import init.type.NEED_E;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderFindable;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.util.RoomInitData;
import settlement.stats.STATS;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.Json;
import snake2d.util.file.SAVABLE;

public abstract class RoomService  {

	private int available = 0; 
	private int total = 0;
	private double load,loadLast;
	private int day;
	
	public final int radius;
	final RoomBlueprintImp room;
	
	public final SFinderRoomService finder;
	
	public final NEED need;
	public SoundRace usageSound;
	public double usage = 1;
	
	public final CharSequence verb;
	
	public RoomService(RoomBlueprintImp b, RoomInitData data, NEED need) {
		
		verb =  data.text().json("SERVICE").text("VERB");
		this.need = need;
		Json jd = data.data().json("SERVICE");
		usageSound = AUDIO.race("ROOM_SERVICE_" + b.key);

		this.room = b;
		
		radius = jd.has("RADIUS") ? jd.i("RADIUS", 0, 50000) : 150;
		
		finder = new SFinderRoomService(b.info.name) {
			
			@Override
			public FSERVICE get(int tx, int ty) {
				return service(tx, ty);
			}
		};
		
		day = -1;
	}

	public final SAVABLE saver = new SAVABLE() {
		
		@Override
		public void save(FilePutter file) {
			file.i(available);
			file.i(total);
			file.d(load);
			file.d(loadLast);
			
		}

		@Override
		public void load(FileGetter file) throws IOException {
			available = file.i();
			total = file.i();
			load = file.d();
			loadLast = file.d();
		}

		@Override
		public void clear() {
			available = 0;
			total = 0;
			load = 0;
			loadLast = 0;
		}
	};
	
	public double load() {
		if (total == 0)
			return 1;
		if (day != TIME.days().bitsSinceStart()) {
			loadLast = load;
			load = 0;
			day =  TIME.days().bitsSinceStart();
		}
		return loadLast;
	}
	
	public void loadFix(RoomBlueprintIns<?> blue) {
		total = 0 ;
		available = 0;
		
		for (int i = 0; i < blue.instancesSize(); i++) {
			
			RoomInstance ins = blue.getInstance(i);
			ROOM_SERVICER ss = (ROOM_SERVICER) ins;
			
			available += ss.service().available();
			total += ss.service().total();
			
			
			
		}
	}
	
	public int available(){
		return available;
	}
	
	public int total() {
		return total;
	}
	
	void increServices(int total, int available) {
		
		if (this.total == 0) {
			load = 1;
			loadLast = 1;
		}else {
			double d = 1.0 - this.available/(double)this.total;
			if (d > load)
				load = d;
			if (d > loadLast)
				loadLast = d;
		}
		
		
		this.available += available;
		this.total += total;
		
	}
	
	public RoomBlueprintImp room(){
		return room;
	}
	
	public double totalMultiplier() {
		
		
		if (need != null) {
			
			double ne = usage/STATS.SERVICE().needTot(need);
			
			if (need instanceof NEED_E) {
				return 1.0/(ne*need.rate.get(HCLASS_RACE.clP(null, null)));
			}else {
				double tot = 0;
				for (int ni = 0; ni < NEEDS.ALLSIMPLE().size(); ni++) {
					NEED o = NEEDS.ALLSIMPLE().get(ni);
					tot += o.rate.get(HCLASS_RACE.clP(null, null));
				}
				if (STATS.SERVICE().needTot(need) == 0)
					ne = usage;
				return 1.0/(ne*TIME.servicePerDay()*0.5*need.rate.get(HCLASS_RACE.clP(null, null))/tot);
				
			}
			
		}
		return 1;
	}
	
	public interface ROOM_SERVICE_HASER extends RoomFinderHaser{
		RoomService service();
		@Override
		public default SFinderFindable finder() {
			return service().finder;
		}
		
		@Override
		public default int radius() {
			return service().radius;
		}
	}

	public abstract FSERVICE service(int tx, int ty);

	public SFinderFindable finder() {
		return finder;
	}

	public int radius() {
		return radius;
	}
	

}
