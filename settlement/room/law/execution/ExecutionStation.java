package settlement.room.law.execution;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import game.GAME;
import init.type.CAUSE_LEAVES;
import settlement.entity.ENTITY;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.room.main.ROOMA;
import settlement.room.main.Room;
import settlement.room.main.util.RoomAreaWrapper;
import settlement.room.main.util.RoomBits;
import settlement.stats.STATS;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.rnd.RND;
import snake2d.util.sets.ArrayCooShort;

public final class ExecutionStation{

	public static final int TYPE_CHOP = 1;
	public static final int TYPE_HANG = 2;
	public static final int TYPE_GIBBET = 3;
	public static final int TYPE_CROSS = 4;
	
	private final ROOM_EXECTUTION b;
	
	private RoomAreaWrapper aa = new RoomAreaWrapper();
	private final Coo coo = new Coo();
	private final RoomBits bState = new BB(coo, 		new Bits(0x000000FF));
	private final RoomBits bServices = new BB(coo, 		new Bits(0x0000FF00));
	private final Client client = new Client();
	private final Guard guard = new Guard();
	
	public final static int services = 8;
	

	final static int STATE_UNINITED = 0;
	final static int STATE_RESERVABLE = 1;
	final static int STATE_RESERVED = 2;
	final static int STATE_USED = 3;
	final static int STATE_EXECUTING = 4;
	final static int STATE_DEAD = 5;
	
	private ArrayCooShort available = new ArrayCooShort(128);
	private int total = 0;
	

	
	ExecutionStation(ROOM_EXECTUTION b) {
		this.b = b;
	}
	
	public Client exectuionReserve() {
		
		if (available.getI() == 0)
			return null;
		
		int m = available.getI();
		if (m == 0)
			return null;
		
		available.set(RND.rInt(m));
		int x = available.get().x();
		int y = available.get().y();
		
		if (!isInit(x, y) || bState.get() != STATE_RESERVABLE) {
			throw new RuntimeException();
		}
		bState.set(aa.area(), STATE_RESERVED);
		
		available.swap(available.getI(), m-1);
		available.set(m-1);
		
		return client;
	}
	
	public int total() {
		return total;
	}
	
	public int available() {
		return available.getI();
	}
	
	public boolean deadORDying(int tx, int ty) {
		if (isInit(tx, ty))
			return bState.get() >= STATE_USED;
		return false;
	}
	
	void save(FilePutter f) {
		f.object(available);
		f.i(total);
	}

	void load(FileGetter f) throws IOException {
		available = (ArrayCooShort) f.object();
		total = f.i();
	}

	void clear() {
		available = new ArrayCooShort(128);
		total = 0;
	}
	
	void init(int tx, int ty) {
		if (isInit(tx, ty)) {
			total++;
			available.get().set(tx, ty);
			bState.set(aa.area(), STATE_RESERVABLE);
		}
	}
	
	void dispose(int tx, int ty) {
		if (isInit(tx, ty)) {
			total--;
			int m = available.getI();
			for (int i = 0; i < m; i++) {
				if (available.set(i).isSameAs(tx, ty)) {
					available.swap(i, m-1);
					available.set(m-1);
					break;
				}
			}
			bState.set(aa.area(), STATE_UNINITED);
		}
	}

	private boolean isInit(int tx, int ty) {
		Room r = ROOMS().map.get(tx, ty);
		if (r !=  SETT.ROOMS().EXECUTION.instance)
			return false;
		
		
		
		int c = SETT.ROOMS().fData.tileData.get(tx, ty);
		if (c <= 0)
			return false;
		aa.done();
		aa.init(r, tx, ty);
		coo.set(tx, ty);
		return true;
	}
	
	public int type(int tx, int ty) {
		return SETT.ROOMS().fData.tileData.get(tx, ty);
	}
	
	public FSERVICE service(int tx, int ty) {
		if (isInit(tx, ty))
			return service;
		return null;
	}
	
	public Client client(int tx, int ty) {
		if (isInit(tx, ty))
			return client;
		return null;
	}
	
	public Guard guard(int tx, int ty) {
		if (isInit(tx, ty))
			return guard;
		return null;
	}

	public class Client {
		

		
		public boolean clientReserved() {
			return bState.get() >= STATE_RESERVED;
		}
		
		public boolean clientPresent() {
			return bState.get() > STATE_RESERVED;
		}
		
		
		public void clientUse() {
			bState.set(aa.area(), STATE_USED);
		}
		
		public void clientCancel() {
			if(SETT.THINGS().corpses.tGet.get(coo) != null)
				bState.set(aa.area(), STATE_DEAD);
			else
				bState.set(aa.area(), STATE_RESERVABLE);
		}
		
		public boolean clientBeingExecuted() {
			return bState.get() == STATE_EXECUTING;
		}
		
		public DIR clientDir() {
			return DIR.ORTHO.get(SETT.ROOMS().fData.item.get(coo).rotation);
		}
		
		public COORDINATE coo() {
			return coo;
		}
	}
	
	public class Guard {
		
		public boolean active() {
			if (bState.get() >= STATE_RESERVABLE && bState.get() < STATE_DEAD)
				return true;
			return false;
		}
		
		public boolean shouldExecute() {
			return bState.get() == STATE_USED;
		}
		
		public boolean workExecute() {
			if (bState.get() == STATE_USED) {
				bState.set(aa.area(), STATE_EXECUTING);
				
				if (type(coo.x(), coo.y()) == TYPE_CHOP) {
					for (ENTITY e : SETT.ENTITIES().getAtTile(coo.x(), coo.y())) {
						if (e instanceof Humanoid) {
							Humanoid a = (Humanoid) e;
							STATS.NEEDS().INJURIES.COUNT.indu().incD(a.indu(), 0.2+RND.rFloat());
							SETT.THINGS().gore.cloud(a, a.race().appearance().colors.blood);
							SETT.THINGS().gore.flesh(a, a.race().appearance().colors.blood);
							if (STATS.NEEDS().INJURIES.COUNT.indu().getD(a.indu()) > 0.75) {
								GAME.count().EXECUTIONS.inc(1);
								STATS.NEEDS().INJURIES.COUNT.indu().setD(a.indu(), 1.0);
								a.kill(false, CAUSE_LEAVES.EXECUTED());
								client.clientCancel();
							}
							return true;
						}
					}
				}
				
				
			}
			return false;
		}
		
		public COORDINATE coo() {
			return coo;
		}
		
	}
	
	private final FSERVICE service = new FSERVICE() {
		
		
		@Override
		public int y() {
			return coo.y();
		}
		
		@Override
		public int x() {
			return coo.x();
		}
		
		@Override
		public boolean findableReservedIs() {
			return bServices.get() > 0;
		}
		
		@Override
		public boolean findableReservedCanBe() {
			return bState.get() == STATE_RESERVABLE && bServices.get() < services;
		}
		
		@Override
		public void findableReserveCancel() {
			bServices.inc(aa.area(), 1);
		}
		
		@Override
		public void findableReserve() {
			bServices.inc(aa.area(), -1);
		}
		
		@Override
		public void consume() {
			bServices.inc(aa.area(), 1);
		}
	};
	
	private final class BB extends RoomBits {

		public BB(COORDINATE coo, Bits bits) {
			super(coo, bits);
		}
		
		@Override
		public void set(ROOMA r, int t) {
			boolean av = bState.get() == STATE_RESERVABLE;
			boolean requested = bState.get() == STATE_RESERVED;
			if (service.findableReservedCanBe())
				b.data.report(service, -1);
			super.set(r, t);
			if (service.findableReservedCanBe())
				b.data.report(service, 1);
			if (!av && bState.get() == STATE_RESERVABLE) {
				if (available.getI()-1 >= available.size()) {
					ArrayCooShort nn = new ArrayCooShort(available.size()+128);
					for (int i = available.getI()-1; i >= 0; i--) {
						nn.set(i).set(available.set(i));
					}
					available = nn;
				}
				available.get().set(coo.x(), coo.y());
				available.inc();
			}
			if (!requested && bState.get() == STATE_RESERVED) {
				int tx = coo.x();
				int ty = coo.y();
				SETT.ROOMS().GUARD.reporter.reportExecution(tx, ty);
				isInit(tx, ty);
			}
			
		}
		
	}

	public void update(int tx, int ty) {
		if (isInit(tx, ty)) {
			if (bState.get() == STATE_DEAD) {
				if (SETT.THINGS().corpses.tGet.get(tx, ty) == null)
					bState.set(aa.area(), STATE_RESERVABLE);
			}
			
			
		}
		
	}

	int sevices(int tx, int ty) {
		if (service(tx, ty) != null) {
			if (bState.get() == STATE_DEAD) {
				return services-bServices.get();
			}
		}
		return 0;
	}
	
	int state(int tx, int ty) {
		if (service(tx, ty) != null) {
			return bState.get();
		}
		return 0;
	}
	
}
