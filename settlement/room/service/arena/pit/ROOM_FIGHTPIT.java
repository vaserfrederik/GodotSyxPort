package settlement.room.service.arena.pit;

import java.io.IOException;

import game.time.TIME;
import init.constant.C;
import init.race.RACES;
import init.race.Race;
import settlement.main.SETT;
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
import settlement.room.service.module.ROOM_SPECTATOR;
import settlement.room.service.module.RoomServiceAccess;
import settlement.room.service.module.RoomServiceNeed;
import settlement.room.service.module.RoomServiceNeed.ROOM_SERVICE_NEED_HASER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.RECTANGLE;
import snake2d.util.datatypes.Rec;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.rnd.RND;
import snake2d.util.sets.LISTE;
import util.data.BOOLEANO.BOOLEAN_OE;
import util.data.BOOLEANO.BooleanOEImp;
import util.info.INFO;
import util.text.D;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_FIGHTPIT extends RoomBlueprintIns<ArenaInstance> implements ROOM_SERVICE_NEED_HASER, ROOM_SPECTATOR.ROOM_SPECTATOR_HASER, PUNISHMENT_SERVICE{

	public static CharSequence ¤¤kill = "Strength and Honor";
	public static CharSequence ¤¤killD = "Enable prisoners condemned to die to do so on the sands of the arena.";
	
	static {
		D.ts(ROOM_FIGHTPIT.class);
	}
	
	final RoomServiceNeed data; 
	final Service ser;
	final static int EXECUTIONS = 1;
	final ArenaConstructor constructor;

	private int gladiators = 0;
	private int gladiatorMax = 0;
	
	private final BooleanOEImp<Race> permission = new BooleanOEImp<Race>(RACES.all().size(), true);
	
	public ROOM_FIGHTPIT(String key, int index, RoomInitData init, RoomCategorySub block) throws IOException {
		super(index, init, key, block);
		ser = new Service(this);
		data = new RoomServiceNeed(this, init) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return ser.get(tx, ty); 
			}

		};
		constructor = new ArenaConstructor(this, init);
		employment().setShiftStart(ROOM_SPECTATOR.WORK_STARTSD, false);
		permission.info = new INFO(¤¤kill, ¤¤killD);
	}
	
	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new RoomArenaGui(work));
	}
	
	void incG(int g, int m) {
		gladiators += g;
		gladiatorMax += m;
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
	protected void saveP(FilePutter saveFile){
		data.saver.save(saveFile);
		saveFile.i(gladiators);
		saveFile.i(gladiatorMax);
		permission.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		data.saver.load(saveFile);
		gladiators = saveFile.i();
		gladiatorMax = saveFile.i();
		
		gladiators = 0;
		for (int i = 0; i < instancesSize(); i++) {
			ArenaInstance ins = getInstance(i);
			gladiators += ins.gladiators;
		}
		permission.load(saveFile);
		
	}
	
	@Override
	protected void clearP() {
		data.saver.clear();
		gladiators = 0;
		gladiatorMax = 0;
		permission.clear();
	}
	
	@Override
	public RoomServiceNeed service() {
		return data;
	}
	
	private final ROOM_SPECTATOR spec = new ROOM_SPECTATOR() {
		
		private Coo coo = new Coo();
		
		@Override
		public RoomServiceAccess service() {
			return ROOM_FIGHTPIT.this.service();
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
		}

		@Override
		public boolean isActive(int sx, int sy) {
			ArenaInstance ins = getter.get(sx, sy);
			return ins != null && ins.employees().employed() > 0;
		};
		
		
	};

	@Override
	public ROOM_SPECTATOR spec() {
		return spec;
	}
	
	private final Coo coo = new Coo();
	private final Rec aArea = new Rec();
	
	public static boolean gamesAreHeld() {
		return TIME.hours().bitCurrent() >= 8;
	}
	
	@Override
	public int punishTotal() {
		return gladiatorMax;
	}

	@Override
	public int punishUsed() {
		return gladiators;
	}
	
	@Override
	public BOOLEAN_OE<Race> punishEnabled() {
		return permission;
	}
	
	public final RoomArenaWork work = new RoomArenaWork() {
		
		@Override
		public void unreserveDeath(int tx, int ty) {
			ArenaInstance ins = getter.get(tx, ty);
			if (ins != null)
				ins.reserveGladiator(-1);
		}
		
		@Override
		public RoomInstance reserveDeath(COORDINATE coo) {
			if (gladiatorMax == 0 || gladiators >= gladiatorMax)
				return null;
			
			{
				ArenaInstance ins = getter.get(coo);
				if (ins != null && ins.gladiatorsNeeded() > 0) {
					ins.reserveGladiator(1);
					return ins;
				}
			}
			
			int ri = RND.rInt(instancesSize());
			
			for (int i = 0; i < instancesSize(); i++) {
				ArenaInstance ins = getInstance((i+ri)%instancesSize());
				if (ins.gladiatorsNeeded() > 0) {
					ins.reserveGladiator(1);
					return ins;
				}
			}
			return null;
		}
		
		@Override
		public boolean gladiatorInArena(int tx, int ty) {
			ArenaInstance ins = getter.get(tx, ty);
			return ins != null && SETT.ROOMS().fData.tileData.get(tx, ty) == ArenaConstructor.ARENA;
		}
		
		@Override
		public COORDINATE gladiatorGetSpot(RoomInstance i) {
			ArenaInstance ins = (ArenaInstance) i;
			int w = ins.body().width()-ins.ax*2;
			int h = ins.body().height()-ins.ay*2;
			coo.set(ins.body().x1()+ins.ax+RND.rInt(w), ins.body().y1()+ins.ay+RND.rInt(h));
			if (!gladiatorInArena(coo.x(), coo.y()))
				throw new RuntimeException("" + coo);
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
			if (ins == null)
				return null;
			aArea.setDim(ins.body().width()-ins.ax*2, ins.body().height()-ins.ay*2);
			aArea.moveX1Y1(ins.body().x1()+ins.ax, ins.body().y1()+ins.ay);
			return aArea;
		}

		@Override
		public int executions(RoomInstance ins) {
			if (ins instanceof ArenaInstance) {
				ArenaInstance i = (ArenaInstance) ins;
				return i.gladiators;
			}
			return 0;
		}

		@Override
		public int executionsMax(RoomInstance ins) {
			return EXECUTIONS;
		}

		@Override
		public int executions() {
			return gladiators;
		}

		@Override
		public int executionsMax() {
			return gladiatorMax;
		}
	};







}
