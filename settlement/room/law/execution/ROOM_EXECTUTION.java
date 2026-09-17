package settlement.room.law.execution;

import java.io.IOException;

import init.constant.C;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.law.PUNISHMENT_SERVICE;
import settlement.room.law.execution.ExecutionStation.Client;
import settlement.room.law.execution.ExecutionStation.Guard;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.ROOM_ACTIVITY;
import settlement.room.service.module.ROOM_ACTIVITY.ROOM_ACTIVITY_HASER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_EXECTUTION extends RoomBlueprintImp implements PUNISHMENT_SERVICE, ROOM_ACTIVITY_HASER{
	
	final SFinderRoomService data; 
	final Constructor constructor;
	
	public final ExecutionStation stations = new ExecutionStation(this);
	final ExecutionSingle instance;
	
	public ROOM_EXECTUTION(RoomInitData init, RoomCategorySub block) throws IOException {
		super(init, 0, "_EXECUTION", block);
		
		instance = new ExecutionSingle(init.m, this);
		constructor = new Constructor(this, init);
		data = new SFinderRoomService("Execution") {

			@Override
			public FSERVICE get(int tx, int ty) {
				return stations.service(tx, ty); 
			}

		};
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
	public int punishUsed() {
		return stations.total() -stations.available();
	}
	
	@Override
	public int punishTotal() {
		return stations.total();
	}

	@Override
	protected void save(FilePutter f) {
		stations.save(f);
	}

	@Override
	protected void load(FileGetter f) throws IOException {
		stations.load(f);
	}

	@Override
	protected void clear() {
		stations.clear();
	}


	@Override
	public SFinderRoomService service(int tx, int ty) {
		return data;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this));
	}

	private final ROOM_ACTIVITY spec = new ROOM_ACTIVITY() {
		
		private Coo coo = new Coo();
		
		@Override
		public SFinderRoomService finder() {
			return ROOM_EXECTUTION.this.data;
		}
		
		@Override
		public COORDINATE lookAt(int sx, int sy) {
			coo.set(sx*C.TILE_SIZE+C.TILE_SIZEH, sy*C.TILE_SIZE+C.TILE_SIZEH);
			return coo;
		}
		
		@Override
		public boolean is(int sx, int sy) {
			return ROOM_EXECTUTION.this.is(sx, sy);
		}
		
		@Override
		public boolean shouldCheer(int sx, int sy) {
			Client s = stations.client(sx, sy);
			if (s != null) {
				return (s.clientBeingExecuted());
			}
			return false;
		};
		
		@Override
		public boolean shouldBoo(int sx, int sy) {
			Guard s = stations.guard(sx, sy);
			if (s != null) {
				return (s.shouldExecute());
			}
			return false;
		};
		
		@Override
		public boolean isActive(int sx, int sy) {
			Client s = stations.client(sx, sy);
			return s != null && s.clientPresent();
		};
		
//		@Override
//		public boolean isSpot(int tx, int ty) {
//			return stations.client(tx, ty) != null;
//			
//		};
	};


	@Override
	public ROOM_ACTIVITY spec() {
		return spec;
	}

	
	
}
