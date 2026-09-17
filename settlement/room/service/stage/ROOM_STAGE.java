package settlement.room.service.stage;

import java.io.IOException;

import game.time.TIME;
import init.constant.C;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.ROOM_SPECTATOR;
import settlement.room.service.module.RoomServiceAccess;
import settlement.room.service.module.RoomServiceNeed;
import settlement.room.service.module.RoomServiceNeed.ROOM_SERVICE_NEED_HASER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.rnd.RND;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_STAGE extends RoomBlueprintIns<StageInstance> implements ROOM_SERVICE_NEED_HASER, ROOM_SPECTATOR.ROOM_SPECTATOR_HASER{

	final RoomServiceNeed data; 
	
	final StageConstructor constructor;
	final Centre work;
	
	public ROOM_STAGE(String key, int index, RoomInitData init, RoomCategorySub block) throws IOException {
		super(index, init, key, block);
		work = new Centre(this);
		data = new RoomServiceNeed(this, init) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return work.service(tx, ty); 
			}

		};
		constructor = new StageConstructor(this, init);
		employment().setShiftStart(ROOM_SPECTATOR.WORK_STARTSD, false);
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
	public SFinderRoomService finder() {
		return data.finder;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		data.saver.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		data.saver.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		data.saver.clear();
	}
	
	@Override
	public RoomServiceNeed service() {
		return data;
	}

	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		
	}
	
	private final ROOM_SPECTATOR spec = new ROOM_SPECTATOR() {
		
		private Coo coo = new Coo();
		
		private final byte[] acts = Alloc.bb(64);
		
		{
			
			
			for(int i = 0; i < 10; i++) {
				acts[RND.rInt(acts.length)] = (byte) (1 + RND.rInt(2));
			}
		}
		
		@Override
		public RoomServiceAccess service() {
			return ROOM_STAGE.this.service();
		}
		
		@Override
		public COORDINATE lookAt(int sx, int sy) {
			coo.set(sx*C.TILE_SIZE+C.TILE_SIZEH, sy*C.TILE_SIZE+C.TILE_SIZEH);
			return coo;
		}
		
		@Override
		public boolean is(int sx, int sy) {
			StageInstance ins = getter.get(sx, sy);
			return ins != null;
		}
		
		private int activity(int sx, int sy) {
			StageInstance ins = getter.get(sx, sy);
			if (ins == null)
				return 0;
			int s = ins.off;
			
			s += (int) (acts.length*TIME.currentSecond()/TIME.secondsPerDay());
			s %= acts.length;
			return acts[s];
		}
		
		@Override
		public boolean shouldCheer(int sx, int sy) {
			return activity(sx, sy) == 1;
		};
		
		@Override
		public boolean shouldBoo(int sx, int sy) {
			return activity(sx, sy) == 2;
		}

		@Override
		public boolean isActive(int sx, int sy) {
			return true;
		};
	};

	@Override
	public ROOM_SPECTATOR spec() {
		return spec;
	}

}
