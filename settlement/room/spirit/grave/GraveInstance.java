package settlement.room.spirit.grave;

import settlement.misc.job.SETT_JOB;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.JobPositions;
import settlement.room.main.util.RoomInit;
import settlement.room.spirit.dump.ROOM_DUMP;
import settlement.room.spirit.grave.GraveData.GRAVE_DATA_HOLDER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.Alloc;
import snake2d.util.sprite.text.Str;
import view.main.VIEW;

final class GraveInstance extends RoomInstance{

	private int available = 0;
	private final int total;
	
	final static double WORKER_PER_GRAVE = 0.1;
	final Jobs jobs;
	final long[] datas;
	final int[] names;
	
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;

	protected GraveInstance(RoomBlueprintIns<GraveInstance> blueprint, TmpArea area, RoomInit init) {
		super(blueprint, area, init);
		
		int i =0;
		for (COORDINATE c : body()) {
			
			if (is(c) && data().grave(c.x(), c.y()) != null) {
				 data().grave(c.x(), c.y()).init(c.x(), c.y(), i);
				 i++;
			}
		}
		
		datas = new long[i];
		names = Alloc.ii(i);
		jobs = new Jobs(this);
		int w = (int) Math.ceil(WORKER_PER_GRAVE*jobs.size());
		employees().maxSet(w);
		employees().neededSet(w);
		available = jobs.size();
		total = jobs.size();
		activate();
		
	}

	@SuppressWarnings("unchecked")
	@Override
	public RoomBlueprintIns<? extends RoomInstance> blueprintI() {
		return (RoomBlueprintIns<? extends RoomInstance>) blueprint();
	}

	@Override
	protected void updateAction(double updateInterval, boolean day) {
		jobs.searchAgain();
	}
	
	@Override
	protected void activateAction() {
		data().activate(this, available, total);
	}

	@Override
	protected void deactivateAction() {
		data().deactivate(this, available, total);
		data().deactivate(this);
	}
	
	@Override
	public void updateTileDay(int tx, int ty) {
		Grave g = data().grave(tx, ty);
		if (g != null)
			g.updateDay2();
	}
	
	private GraveData data() {
		return ((GRAVE_DATA_HOLDER)blueprint()).graveData();
	}

	@Override
	protected void dispose() {
		data().dispose(this, available, total);
	}
	
	void count(int a) {
		if (active())
			data().deactivate(this, available, total);
		available += a;
		if (active())
			data().activate(this, available, total);
	}

	public int total() {
		return total;
	}
	
	public int available() {
		return available;
	}
	
	private boolean prompt() {
		int time = 0;
		int am = 0;
		for (COORDINATE c : body()) {
			if (is(c)) {
				Grave g = data().grave(c.x(), c.y());
				if (g != null) {
					int t = g.daysTillDecompose(c.x(), c.y());
					if (t > 0) {
						am++;
						if (t > time)
							time = t;
					}
				}
				
			}
		}
		
		if (am > 0) {
			Str.TMP.clear();
			Str.TMP.add(ROOM_DUMP.¤¤RemoveProblem);
			Str.TMP.insert(0, am);
			Str.TMP.insert(1, time);
			VIEW.inters().yesNo.activate(Str.TMP, null, null, false);
			return true;
		}
		return false;
	}
	
	@Override
	protected boolean canRemoveAndRemoveAction(int tx, int ty, boolean scatter, Object obj, boolean force) {
		if (force || !prompt())
			return true;
		return false;
	}
	
	static class Jobs extends JobPositions<GraveInstance> {

		private static final long serialVersionUID = 1L;

		public Jobs(GraveInstance ins) {
			super(ins);
		}

		@Override
		protected boolean isAndInit(int tx, int ty) {
			return ins.data().grave(tx, ty) != null;
		}
		
		@Override
		protected SETT_JOB get(int tx, int ty) {
			if (ins.data().grave(tx, ty) != null)
				return ins.data().grave(tx, ty).job(tx, ty);
			return null;
		}
	}

}
