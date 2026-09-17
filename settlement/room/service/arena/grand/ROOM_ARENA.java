package settlement.room.service.arena.grand;

import java.io.IOException;

import game.time.TIME;
import init.constant.C;
import init.race.RACES;
import init.race.Race;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.law.PUNISHMENT_SERVICE;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.arena.RoomArenaGui;
import settlement.room.service.arena.RoomArenaWork;
import settlement.room.service.arena.pit.ROOM_FIGHTPIT;
import settlement.room.service.module.ROOM_SPECTATOR;
import settlement.room.service.module.RoomServiceAccess;
import settlement.room.service.module.RoomServiceNeed;
import settlement.room.service.module.RoomServiceNeed.ROOM_SERVICE_NEED_HASER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.RECTANGLE;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.rnd.RND;
import snake2d.util.sets.LISTE;
import util.data.BOOLEANO.BOOLEAN_OE;
import util.data.BOOLEANO.BooleanOEImp;
import util.info.INFO;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_ARENA extends RoomBlueprintIns<ArenaInstance> implements ROOM_SERVICE_NEED_HASER, ROOM_SPECTATOR.ROOM_SPECTATOR_HASER, PUNISHMENT_SERVICE {
	
	final RoomServiceNeed data; 
	final Service ser;
	final ArenaConstructor constructor;
	int executions = 0;
	int executionsMax = 0;
	private final BooleanOEImp<Race> permission = new BooleanOEImp<Race>(RACES.all().size(), true);
	
	public ROOM_ARENA(String key, int index, RoomInitData init, RoomCategorySub block) throws IOException {
		super(index, init, key, block);
		constructor = new ArenaConstructor(this, init);
		ser = new Service(this);
		data = new RoomServiceNeed(this, init) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return ser.get(tx, ty); 
			}
			
			@Override
			public boolean isGoodTime() {
				return spec.isOpenNow();
			}
		};
		employment().setShiftStart(ROOM_SPECTATOR.WORK_STARTSD, false);
		permission.info = new INFO(ROOM_FIGHTPIT.¤¤kill, ROOM_FIGHTPIT.¤¤killD);
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
	protected void saveP(FilePutter f){
		data.saver.save(f);
		f.i(executions);
		f.i(executionsMax);
		permission.save(f);
	}
	
	@Override
	protected void loadP(FileGetter f) throws IOException{
		data.saver.load(f);
		executions = f.i();
		executionsMax = f.i();
		permission.load(f);
		
	}
	
	@Override
	protected void clearP() {
		data.saver.clear();
		executions = 0;
		executionsMax = 0;
		permission.clear();
	}
	
	@Override
	public int punishTotal() {
		return executionsMax;
	}

	@Override
	public int punishUsed() {
		return executions;
	}
	
	@Override
	public BOOLEAN_OE<Race> punishEnabled() {
		return permission;
	}

	@Override
	public RoomServiceNeed service() {
		return data;
	}

	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new RoomArenaGui(work));
	}
	
	private final ROOM_SPECTATOR spec = new ROOM_SPECTATOR() {
		
		private Coo coo = new Coo();
		
		@Override
		public RoomServiceAccess service() {
			return ROOM_ARENA.this.service();
		}
		
		@Override
		public COORDINATE lookAt(int sx, int sy) {
			
			
			ArenaInstance ins = getter.get(sx, sy);
			if (ins == null)
				coo.set(sx, sy);
			else {
				coo.set(sx, sy);
				RECTANGLE rec = work.gladiatorArea(sx, sy);
				int w = Math.min(4, rec.width());
				int h = Math.min(4, rec.height());
				int a = w*h;
				int i = (sx+sy)%a;
				coo.set(rec.cX()-w/2+(i%(w)), rec.cY()-h/2+(i/h));
			}
			coo.set(coo.x()*C.TILE_SIZE+C.TILE_SIZEH, coo.y()*C.TILE_SIZE+C.TILE_SIZEH);
			return coo;
		}
		
		@Override
		public boolean is(int sx, int sy) {
			ArenaInstance ins = getter.get(sx, sy);
			return ins != null;
		}
		
		private int activity(int sx, int sy) {
			ArenaInstance ins = getter.get(sx, sy);
			if (ins == null)
				return 0;
			
			int d = (int)TIME.currentSecond()- ins.cheerTime;
			
			if (d > ArenaInstance.CHEER_TIME*8) {
				ins.cheerTime = (int) TIME.currentSecond();
				ins.cheer = false;
				d = 0;
			}
			
			if (d <= ArenaInstance.CHEER_TIME) {
				if (ins.cheer)
					return 1;
				return 2;
				
			}
			return 0;
		}
		
		@Override
		public boolean isActive(int sx, int sy) {
			return true;
		};
		
		@Override
		public boolean shouldCheer(int sx, int sy) {
			return activity(sx, sy) == 1;
		};
		
		@Override
		public boolean shouldBoo(int sx, int sy) {
			return activity(sx, sy) == 2;
		};
		
		@Override
		public COORDINATE getDestination(COORDINATE roomT) {
			coo.set(roomT.x(), roomT.y());
			return coo;
		};
		
		@Override
		public boolean isSpot(int tx, int ty) {
			if (ser.init(tx, ty))
				return true;
			return super.isSpot(tx, ty);
		}
		
		@Override
		public boolean isOpenNow() {
			return TIME.hours().bitCurrent() > 11 || TIME.hours().bitCurrent() < 6;
		};
		
		
	};

	@Override
	public ROOM_SPECTATOR spec() {
		return spec;
	}
	
	public final RoomArenaWork work = new RoomArenaWork() {
		
		private Coo coo = new Coo();
		
		@Override
		public boolean gladiatorInArena(int tx, int ty) {
			ArenaInstance ins = getter.get(tx, ty);
			if (ins != null) {
				return ins.arena.holdsPoint(tx, ty);
			}
			return false;
		}
		
		@Override
		public COORDINATE gladiatorGetSpot(RoomInstance ins) {
			ArenaInstance a = (settlement.room.service.arena.grand.ArenaInstance) ins;
			int w = a.arena.width();
			int h = a.arena.height();
			coo.set(a.arena.x1()+RND.rInt(w), a.arena.y1()+RND.rInt(h));
			return coo;
		}


		@Override
		public void gladiatorDrawMakeSheer(COORDINATE coo) {
			ArenaInstance ins = getter.get(coo);
			if (ins != null) {
				ins.cheerTime = (int) TIME.currentSecond();
				ins.cheer = !RND.oneIn(6);
			}
		}

		@Override
		public RECTANGLE gladiatorArea(int tx, int ty) {
			ArenaInstance ins = getter.get(tx, ty);
			if (ins != null) {
				return ins.arena;
			}
			return null;
		}

		@Override
		public RoomInstance reserveDeath(COORDINATE coo) {
			if (executions >= executionsMax || instancesSize() <= 0)
				return null;
			
			{
				ArenaInstance ins = getter.get(coo);
				if (ins != null && ins.executions < 4) {
					ins.executions ++;
					executions ++;
					return ins;
				}
			}
			int ri = RND.rInt(instancesSize());
			
			for (int i = 0; i < instancesSize(); i++) {
				ArenaInstance ins = getInstance((i+ri)%instancesSize());
				if (ins.active() && ins.employees().employed() > 0 && ins.executions < 4) {
					ins.executions ++;
					executions ++;
					return ins;
				}
			}
			return null;
		}

		@Override
		public void unreserveDeath(int tx, int ty) {
			ArenaInstance ins = getter.get(tx, ty);
			if (ins != null) {
				ins.executions --;
				ins.executions = (byte) Math.max(ins.executions, 0);
				if (ins.active() && ins.employees().employed() > 0) {
					executions --;
				}
			}
		}

		@Override
		public int executions() {
			return executions;
		}

		@Override
		public int executionsMax() {
			return executionsMax;
		}

		@Override
		public int executions(RoomInstance ins) {
			if (ins instanceof ArenaInstance) {
				ArenaInstance i = (ArenaInstance) ins;
				return i.executions;
			}
			return 0;
		}

		@Override
		public int executionsMax(RoomInstance ins) {
			return 4;
		}
	};


}
