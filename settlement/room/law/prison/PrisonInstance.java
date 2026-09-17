package settlement.room.law.prison;

import game.GAME;
import init.constant.C;
import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCES;
import init.resources.ResGEat;
import init.type.CAUSE_LEAVES;
import settlement.entity.ENTITY;
import settlement.entity.humanoid.HEvent;
import settlement.entity.humanoid.Humanoid;
import settlement.entity.humanoid.ai.types.prisoner.AIModule_Prisoner;
import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import settlement.stats.STATS;
import snake2d.Renderer;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.misc.CLAMP;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class PrisonInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE {

	private static final long serialVersionUID = 1L;
	final Jobs jobs;

	
	private short prisoners = 0;
	
	private static final Bits bprisoners = new Bits(0b01111);
	private final short[] cellsXY;
	private short cellI = 0;
	final RBITImp fetch = new RBITImp();
	private long[] productionData;
	float riotChance = 1;
	boolean hasWarned = false;
	
	protected PrisonInstance(ROOM_PRISON b, TmpArea area, RoomInit init) {
		super(b, area, init);
		
		int cells = 0;
		for (COORDINATE c : body()) {
			if (is(c)) {
				candle(c.x(), c.y());
				if (SETT.ROOMS().fData.tileData.get(c) == Constructor.CODE_ENTRANCE) {
					cells++;
				}
			}
		}
		
		cellsXY = new short[cells*2];
		cells = 0;
		for (COORDINATE c : body()) {
			if (is(c)) {
				if (SETT.ROOMS().fData.tileData.get(c) == Constructor.CODE_ENTRANCE) {
					cellsXY[cells++] = (short) c.x();
					cellsXY[cells++] = (short) c.y();
				}
			}
		}
		productionData = blueprintI().indu.makeData();
		jobs = new Jobs(this);
		
		int am = (int)Math.ceil(b.constructor.guards.get(this));
		employees().maxSet(am);
		employees().neededSet(am);
		
		for (ResGEat e : RESOURCES.EDI().all())
			if (e.serve)
				fetch.or(e.resource);
		activate();
		jobs.setAlwaysNew();
		
	}
	
	@Override
	protected void loadFix() {
		productionData = blueprintI().indu.makeDataFix(productionData);
		jobs.setAlwaysNew();
		jobs.resNotFound.clear();
	}
	
	void candle(int tx, int ty) {
		
		if (SETT.LIGHTS().is(tx, ty)) {
			SETT.LIGHTS().remove(tx, ty);
			FurnisherItem it = SETT.ROOMS().fData.item.get(tx, ty);
					
			for (DIR d : DIR.ORTHO) {
				if (SETT.ROOMS().fData.item.is(tx, ty, d, it)) {
					SETT.LIGHTS().candle(tx, ty, d.x()*(C.TILE_SIZEH-4), d.y()*(C.TILE_SIZEH-4));
					return;
				}
			}
		}
	}
	
	public int prisoners() {
		return prisoners;
	}
	
	public int prisonersMax() {
		return blueprintI().constructor.PRISONERS_PER_CELL*cellsXY.length/2;
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}

	@Override
	protected void activateAction() {
		blueprintI().incPrisoners(prisoners, prisonersMax());
	}

	@Override
	protected void deactivateAction() {
		for (ENTITY e : SETT.ENTITIES().getAllEnts()) {
			if (e != null && e instanceof Humanoid) {
				Humanoid h = (Humanoid) e;
				HEvent.Handler.removeRoom(h, this);
			}
		}
		
		for (int i = 0; i < cellsXY.length; i+=2) {
			cellI += 2;
			if (cellI >= cellsXY.length)
				cellI = 0;
			int tx = cellsXY[cellI];
			int ty = cellsXY[cellI+1];
			int data = SETT.ROOMS().data.get(tx, ty);
			data = bprisoners.set(data, 0);
			SETT.ROOMS().data.set(this, tx, ty, data);
		}
		
		blueprintI().incPrisoners(-prisoners, -prisonersMax());
		prisoners = 0;
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		blueprintI().indu.updateRoom(this);
		jobs.searchAgain();
		if (day && prisoners() > 0) {
			
			float prev = riotChance;
			
			double v = (double)employees().employed()/employees().max() - 1;
			
			
			RBIT.RBITImp.tmp.clearSet(fetch);
			RBIT.RBITImp.tmp.xor(jobs.resNotFound);
			if (RBIT.RBITImp.tmp.isClear())
				v -= 1;
			
			
			v /= 4;
			
			if (v == 0) {
				riotChance = 1;
				return;
			}
				
			
			riotChance += v;
			riotChance = (float) CLAMP.d(riotChance, 0, 1);
			if (riotChance < 0.5 && riotChance < prev && !hasWarned) {
				Gui.mWarn(this);
				hasWarned = true;
			}else if (riotChance <= 0) {
				Gui.m(this);
				hasWarned = false;
				riotChance = 1;
				for (ENTITY e : SETT.ENTITIES().getAllEnts()) {
					if (e instanceof Humanoid) {
						Humanoid a = (Humanoid) e;
						if (AIModule_Prisoner.isPrisoner(a, this)) {
							STATS.LAW().escapeInc();
							a.kill(false, CAUSE_LEAVES.OTHER());
						}		
					}
				}
				
				
			}
		}
		

	}
	
	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}
	
	@Override
	protected void dispose() {
		
		
	}

	@Override
	public ROOM_PRISON blueprintI() {
		return (ROOM_PRISON) blueprint();
	}
	
	COORDINATE registerPrisoner(COORDINATE c) {
		if (prisoners >= prisonersMax())
			throw new RuntimeException();
		if (!active())
			throw new RuntimeException();
		
		if (is(c) && SETT.ROOMS().fData.tileData.get(c) == Constructor.CODE_ENTRANCE) {

			int tx = c.x();
			int ty = c.y();
			int data = SETT.ROOMS().data.get(tx, ty);
			if (bprisoners.get(data) < blueprintI().constructor.PRISONERS_PER_CELL) {
				incPrisoner(tx, ty, 1);
				Coo.TMP.set(tx, ty);
				return Coo.TMP;
			}
		}
		
		int pris = 0;
		
		for (int i = 0; i < cellsXY.length; i+=2) {
			cellI += 2;
			if (cellI >= cellsXY.length)
				cellI = 0;
			int tx = cellsXY[cellI];
			int ty = cellsXY[cellI+1];
			int data = SETT.ROOMS().data.get(tx, ty);
			pris += bprisoners.get(data);
			if (bprisoners.get(data) < blueprintI().constructor.PRISONERS_PER_CELL) {
				incPrisoner(tx, ty, 1);
				Coo.TMP.set(tx, ty);
				return Coo.TMP;
			}
		}
		
		throw new RuntimeException(pris + " " + prisoners + " " + prisonersMax());
	}
	
	void incPrisoner(int tx, int ty, int am) {
		int data = SETT.ROOMS().data.get(tx, ty);
		if (am < 0) {
			if (bprisoners.get(data) + am < 0 || prisoners < 0) {
				return;
			}
		}else if (am > 0) {
			if (bprisoners.get(data) + am > blueprintI().constructor.PRISONERS_PER_CELL || prisoners > prisonersMax()) {
				GAME.Error("prison " + (prisoners > prisonersMax()));
				return;
			}
		}
		prisoners += am;
		data = bprisoners.inc(data, am);
		SETT.ROOMS().data.set(this, tx, ty, data);
		blueprintI().incPrisoners(am, 0);
	}
	
	void removePrisoner(int tx, int ty) {
		if (!is(tx, ty))
			return;
		if (SETT.ROOMS().fData.tileData.get(tx, ty) != Constructor.CODE_ENTRANCE)
			return;
		int data = SETT.ROOMS().data.get(tx, ty);
		if (bprisoners.get(data) == 0) {
			return;
		}
		incPrisoner(tx, ty, -1);
	}
	
	boolean isReserved(int tx, int ty) {
		if (SETT.ROOMS().fData.tileData.get(tx, ty) != Constructor.CODE_ENTRANCE)
			return false;
		int data = SETT.ROOMS().data.get(tx, ty);
		if (bprisoners.get(data) == 0) {
			return false;
		}
		return true;
	}
	
	static class Jobs extends JobPositions<PrisonInstance> {

		private static final long serialVersionUID = 1L;

		public Jobs(PrisonInstance ins) {
			super(ins);
			setAlwaysNew();
		}

		@Override
		protected SETT_JOB get(int tx, int ty) {
			if (Food.init(tx, ty) != null) {
				return Food.init(tx, ty);
			}
			if (Latrine.init(tx, ty) != null)
				return Latrine.init(tx, ty);
			if (Cell.init(tx, ty) != null)
				return Cell.init(tx, ty);
			return null;
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			if (Food.init(tx, ty) != null)
				return true;
			if (Latrine.init(tx, ty) != null)
				return true;
			if (Cell.init(tx, ty) != null)
				return true;
			return false;
			
			
		}
	}

	@Override
	public long[] productionData() {
		return productionData;
	}

	@Override
	public Industry industry() {
		return blueprintI().indu;
	}

	@Override
	public int industryI() {
		return 0;
	}

}
