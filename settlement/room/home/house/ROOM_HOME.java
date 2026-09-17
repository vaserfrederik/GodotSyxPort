package settlement.room.home.house;

import java.io.IOException;
import java.util.Arrays;

import init.type.HGROUP;
import init.type.HGROUP.HTypeBits;
import settlement.main.SETT;
import settlement.path.finders.SFinderFindable;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.thing.pointlight.LOS;
import snake2d.util.color.COLOR;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.map.MAP_OBJECT;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public class ROOM_HOME extends RoomBlueprintImp{

	private final int[] total = Alloc.ii(HGROUP.all().size());
	private final int[] used = Alloc.ii(HGROUP.all().size());
	
	private int totalT;
	private int usedT;
	
	final HomeContructor constructor;
	public final OddHome odd = new OddHome();

	public ROOM_HOME(RoomInitData init, RoomCategorySub cat) throws IOException {
		super(init, 0, "_HOME", cat);
		constructor = new HomeContructor(init, this);
	}


	
	@Override
	protected void update(double ds) {
		
	}

	@Override
	protected void save(FilePutter file) {
		odd.saver.save(file);
		HGROUP.MAP().saver().save(total, file);
		HGROUP.MAP().saver().save(used, file);
	}

	@Override
	protected void load(FileGetter file) throws IOException {
		odd.saver.load(file);
		HGROUP.MAP().loader().load(total, file, 0);
		HGROUP.MAP().loader().load(used, file, 0);
		
		totalT = 0;
		usedT = 0;
		for (HGROUP t : HGROUP.all()) {
			totalT += total[t.index()];
			usedT += used[t.index()];
		}
	}

	@Override
	protected void clear() {
		odd.saver.clear();
		Arrays.fill(total, 0);
		Arrays.fill(used, 0);
		totalT = 0;
		usedT = 0;
	}

	
	void report(int used, int total, HTypeBits s) {
		for (int i = 0; i < HGROUP.all().size(); i++) {
			
			if (s.is(i)) {
				HGROUP t = HGROUP.all().get(i);
				this.usedT += used;
				this.totalT += total;
				this.used[t.index()] += used;
				this.total[t.index()] += total;
			}
			
		}
		
		
	}
	
	public int total(HGROUP t) {
		if (t == null)
			return totalT;
		return total[t.index()];
	}
	
	public int used(HGROUP t) {
		if (t == null)
			return usedT;
		return used[t.index()];
	}
	
	@Override
	public SFinderFindable service(int tx, int ty) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public COLOR miniC(int tx, int ty) {
		return constructor.miniColor;
	}

	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	public MAP_OBJECT<HomeInstance> getter = new MAP_OBJECT<HomeInstance>() {
		
		@Override
		public HomeInstance get(int tx, int ty) {
			if (SETT.ROOMS().map.blueprint.get(tx, ty) == ROOM_HOME.this) {
				return (HomeInstance) SETT.ROOMS().map.get(tx, ty); 
			}
			return null;
		}
		
		@Override
		public HomeInstance get(int tile) {
			if (SETT.ROOMS().map.blueprint.get(tile) == ROOM_HOME.this) {
				return (HomeInstance) SETT.ROOMS().map.get(tile); 
			}
			return null;
		}
	};
	
	public MAP_OBJECT<HomeInstance> service = new MAP_OBJECT<HomeInstance>() {
		
		@Override
		public HomeInstance get(int tx, int ty) {
			if (SETT.ROOMS().map.blueprint.get(tx, ty) == ROOM_HOME.this) {
				HomeInstance h = (HomeInstance) SETT.ROOMS().map.get(tx, ty); 
				if (tx == h.serviceX() && ty == h.serviceY())
					return h;
			}
			return null;
		}
		
		@Override
		public HomeInstance get(int tile) {
			return get(tile % SETT.TWIDTH, tile/SETT.TWIDTH);
		}
	};
	
	private final LOS los = new LOS() {
		
		@Override
		public boolean passesToOtherFromThis(int fx, int fy, int tx, int ty) {	
			if (SETT.ROOMS().fData.tile.get(fx, fy) == constructor.tOpening)
				return true;
			return getter.get(fx, fy).is(tx, ty);
		}
		
		@Override
		public boolean passesFromOtherToThis(int fx, int fy, int tx, int ty) {
			if (SETT.ROOMS().fData.tile.get(tx, ty) == constructor.tOpening)
				return true;
			return getter.get(tx, ty).is(fx, fy);
		}
		
		@Override
		public boolean blocksEnv(int tx, int ty) {
			return false;
		}

		@Override
		public boolean isLightBlocker(int tx, int ty) {
			return false;
		}
	};
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new HomeHoverer());
		super.appendView(mm);
	}
	
	@Override
	public LOS LOS(int tx, int ty) {
		return los;
	}
	
	
	

	

}
