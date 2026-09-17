package settlement.room.law.police;

import game.audio.SoundRace;
import game.time.TIME;
import init.resources.RBIT;
import init.resources.RESOURCE;
import init.type.CAUSE_LEAVES;
import settlement.entity.ENTITY;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.room.main.RoomInstance;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.util.RoomBits;
import settlement.stats.STATS;
import snake2d.PathTile;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.rnd.RND;
import util.GUTIL;

public final class PoliceWork {

	private final ROOM_POLICE b;
	private PoliceInstance ins;
	private final Coo coo = new Coo();
	
	private final RoomBits reserved = new RoomBits(coo, new Bits(0b0001));
	private final RoomBits used = new RoomBits(coo, new Bits(0b0010));
	
	private final RoomBits clientTime = new RoomBits(coo, new Bits(0b11110000)) {
		
		@Override
		public void set(settlement.room.main.ROOMA r, int t) {
			if (get() > 0)
				ins.prisoners--;
			if (t == 0) {
				ENTITY e = SETT.ENTITIES().getAtTileSingle(coo.x(), coo.y());
				if (e != null && e instanceof Humanoid) {
					Humanoid h = (Humanoid) e;
					if (RND.oneIn(5))
						h.kill(false, CAUSE_LEAVES.PUNISHED());
					h.interrupt();
				}
			}
			super.set(r, t);
			if (get() > 0)
				ins.prisoners++;
			
		};
		
	};
	
	PoliceWork(ROOM_POLICE b){
		this.b = b;
	}
	
	private boolean init(int tx, int ty) {
		PoliceInstance ins = SETT.ROOMS().POLICE.get(tx, ty);
		if (ins == null)
			return false;
		int c = SETT.ROOMS().fData.tileData.get(tx, ty);
		if (c == 0)
			return false;
		this.ins = ins;
		this.coo.set(tx, ty);
		return true;
	}

	public SETT_JOB job(int tx, int ty) {
		if (init(tx, ty))
			return job;
		return null;
	}
	
	public Humanoid clientToFetch(int tx, int ty) {
		
		
		if (!init(tx, ty))
			return null;
		
		if (ins.prisoners >= ins.prisonersMax())
			return null;
		if (clientTime.get() > 0)
			return null;
		if (SETT.ENTITIES().hasAtTile(coo.x(), coo.y()))
			return null;
		if ((SETT.ROOMS().fData.tileData.get(tx, ty) & PoliceConstructor.bitService) != PoliceConstructor.bitService)
			return null;
		
		for (int i = 0; i < 10; i++) {
			Humanoid h = pollVictim();
			if (!validVictim(h) || !b.access(h.indu().popCL()).is())
				continue;
			return h;
		}
		return null;
	}
	
	private Humanoid[] queue = new Humanoid[256];
	private int hi = queue.length;
	
	private Humanoid pollVictim() {
		
		if (hi >= queue.length) {
			
			hi = 0;
			GUTIL.flooder().init(this);
			
			ENTITY[] es = SETT.ENTITIES().getAllEnts();
			for (int i = 0; i < es.length; i++) {
				ENTITY e = es[i];
				if (validVictim(e))
					GUTIL.flooder().pushSmaller(i%SETT.TWIDTH, i/SETT.TWIDTH, RND.rFloat());
				
			}
			hi = queue.length-1;
			while(hi >= 0 && GUTIL.flooder().hasMore()) {
				PathTile t = GUTIL.flooder().pollSmallest();
				int i = t.x()+t.y()*SETT.TWIDTH;
				ENTITY e = es[i];
				queue[hi] = (Humanoid) e;
				hi--;
			}
			hi++;
			GUTIL.flooder().done();
		}
		return queue[hi++];
		
	}
	
	private boolean validVictim(ENTITY e) {
		if (e == null || !(e instanceof Humanoid))
			return false;
		Humanoid h = (Humanoid) e;
		
		if (h.isRemoved())
			return false;
		
		RoomInstance ins = STATS.WORK().EMPLOYED.get(h);
		if (ins != null && (ins.blueprintI() == SETT.ROOMS().GUARD || ins.blueprintI() == b))
			return false;
		if (b.is(h.tc()))
			return false;
		return true;
	}
	
	public Humanoid client(int tx, int ty) {
		if (!init(tx, ty)) {
			return null;
		}

		if ((SETT.ROOMS().fData.tileData.get(tx, ty) & PoliceConstructor.bitService) != PoliceConstructor.bitService)
			return null;

		if (clientTime.get() == 0)
			return null;
		
		ENTITY e = SETT.ENTITIES().getAtTileSingle(tx, ty);
		if (e == null)
			return null;
		
		if (e instanceof Humanoid)
			return (Humanoid) e;
		return null;
		
	}
	
	public boolean isLay(int tx, int ty) {
		return SETT.ROOMS().fData.tileData.get(tx, ty) == PoliceConstructor.bitBed;
	}
	
	public DIR victimDir(int tx, int ty) {
		FurnisherItem f = SETT.ROOMS().fData.item.get(tx, ty);
		if (f == null)
			return DIR.NE;
		return DIR.ORTHO.get(f.rotation);
	}
	
	public void deliverClient(int tx, int ty) {
		if (!init(tx, ty))
			return;
		clientTime.set(ins, 2+RND.rInt(2));
	}
	
	public void dispose(int tx, int ty) {
		if (!init(tx, ty))
			return;
		clientTime.set(ins, 0);
	}
	
	public void update(int tx, int ty) {
		if (!init(tx, ty))
			return;
		clientTime.inc(ins, -1);
	}
	
	private final SETT_JOB job = new SETT_JOB() {
	
		@Override
		public void jobReserve(RESOURCE r) {
			reserved.set(ins, 1);	
		}
	
		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return reserved.get() == 1;
		}
	
		@Override
		public void jobReserveCancel(RESOURCE r) {
			used.set(ins, 0);
			reserved.set(ins, 0);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return reserved.get() == 0;
		}
	
		@Override
		public RBIT jobResourceBitToFetch() {
			return null;
		}
	
		@Override
		public double jobPerformTime(Humanoid a) {
			return 45;
		}
	
		@Override
		public void jobStartPerforming() {
			used.set(ins, 1);
		}
	
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
			jobReserveCancel(r);
			if (clientTime.get() == 0) {
				
				ENTITY e = SETT.ENTITIES().getAtTileSingle(coo.x(), coo.y());
				if (e != null && e instanceof Humanoid) {
					Humanoid h = (Humanoid) e;
					h.interrupt();
				}
			}
			return null;
		}
	
		@Override
		public COORDINATE jobCoo() {
			return coo;
		}
	
		@Override
		public CharSequence jobName() {
			return ins.blueprintI().employment().verb;
		}
	
		@Override
		public boolean jobUseTool() {
			return false;
		}
		
		@Override
		public boolean jobUseHands() {
			return ((coo.x()+coo.y()*SETT.TWIDTH + TIME.hours().bitsSinceStart()) & 1) == 1; 
		}
		
		@Override
		public SoundRace jobSound() {
			return ins.blueprintI().employment().sound();
		}
	
	};



	
}
