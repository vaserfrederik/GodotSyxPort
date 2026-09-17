package settlement.room.home.chamber;

import static settlement.main.SETT.ROOMS;

import init.type.HCLASSES;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.room.home.HOME;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobIterator;
import settlement.room.main.util.RoomInit;
import settlement.stats.STATS;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

public final class ChamberInstance extends RoomInstance implements JOBMANAGER_HASER, HOME{

	private static final long serialVersionUID = 1L;
	final JobIterator jobs;
	final COORDINATE serviceCoo;
	final byte sleepDir;
	private int occupant = 0;
	boolean fetching;
	private boolean workingWarn = false;
	private boolean working;
	
	protected ChamberInstance(ROOM_CHAMBER b, TmpArea area, RoomInit init) {
		super(b, area, init);
		jobs = new JobIterator(this) {

			private static final long serialVersionUID = 1L;

			@Override
			protected SETT_JOB init(int tx, int ty) {
				return blueprintI().work.get(tx, ty);
			}
		};
		
		jobs.setAlwaysNewJob();
		
		COORDINATE s = null;
		
		for (COORDINATE c : body()) {
			if (is(c) && ROOMS().fData.tile.get(c) == blueprintI().constructor.bb) {
				s = new Coo(c);
			}
		}
		
		if (s == null)
			throw new RuntimeException();
		
		serviceCoo = s;
		
		sleepDir = (byte) DIR.S.next(2*ROOMS().fData.item.get(s).rotation).id();

		employees().maxSet(4);
		employees().neededSet(4);
		activate();
		
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		if (occupant != 0)
			it.lit();
		return super.render(r, shadowBatch, it);
	}

	@Override
	protected void activateAction() {
		
	}

	@Override
	protected void deactivateAction() {
		
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		boolean warn = workingWarn;
		workingWarn = employees().employed() < 4;
		
		if (working && warn && workingWarn) {
			
			if (occupant() != null) {
				STATS.HOME().GETTER.set(occupant(), null);
			}
			remove();
			working = false;
			add();
		}else if (!working) {
			remove();
			working = true;
			add();
		}
		
		jobs.searchAgain();
	}
	
	@Override
	public JOB_MANAGER getWork() {
		return jobs;
	}
	
	@Override
	protected void dispose() {
		remove();
		if (occupant() != null) {
			STATS.HOME().dump(occupant());
			STATS.HOME().GETTER.set(occupant(), null);
		}
	}

	@Override
	public ROOM_CHAMBER blueprintI() {
		return (ROOM_CHAMBER) blueprint();
	}

	@Override
	public HOME vacate(Humanoid h) {
		remove();
		occupant = 0;
		add();
		return this;
	}

	@Override
	public HOME occupy(Humanoid h) {
		remove();
		occupant = h.id();
		add();
		return this;
	}

	public Humanoid occupant() {
		if (occupant != 0)
			return (Humanoid) SETT.ENTITIES().getByID(occupant);
		return null;
	}
	
	@Override
	public Humanoid occupant(int oi) {
		if (oi == 0)
			return occupant();
		return null;
	}
	
	@Override
	public int occupants() {
		return occupant() != null ? 1 :0;
	}
	

	@Override
	public int serviceX() {
		return serviceCoo.x();
	}
	
	@Override
	public int serviceY() {
		return serviceCoo.y();
	}
	
//	@Override
//	public HTypeBits availability() {
//		if (occupant() != null)
//			return null;
//		return EGroupBitsImp.specific(HOMET.ALL().get(0));
//	}
//	
	@Override
	public ROOM_DEGRADER degrader(int tx, int ty) {
		return null;
	}
	
	@Override
	public double getDegrade() {
		if (working)
			return 0;
		return 0.5;
	}
	
	private void remove() {
//		if (occupant() == null)
//			SETT.PATH().comps.data.home.reportAbsence(serviceCoo.x(), serviceCoo.y(), availability());
//		SETT.ROOMS().HOMES.report(occupant() != null ? -1 : 0, -1, EGroupBitsImp.specific(HOMET.ALL().get(0)));
	}
	
	private void add() {
//		if (occupant() == null)
//			SETT.PATH().comps.data.home.reportPresence(serviceCoo.x(), serviceCoo.y(), availability());
//		SETT.ROOMS().HOMES.report(occupant() != null ? 1 : 0, 1,  EGroupBitsImp.specific(HOMET.ALL().get(0)));
	}

	@Override
	public int occupantsMax() {
		return 1;
	}

	@Override
	public int resourceAm(int ri) {
		if (occupant() == null)
			return 0;
		return STATS.HOME().current(occupant(), ri);
	}


	@Override
	public double isolation() {
		return isolation(mX(), mY());
	}

	@Override
	public boolean canOccupy(Humanoid h) {
		return h.indu().clas() == HCLASSES.NOBLE() && occupant() == null;
	}

	@Override
	public CharSequence typeName(int tx, int ty) {
		return blueprintI().info.name;
	}
	

}
