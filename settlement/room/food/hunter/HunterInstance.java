package settlement.room.food.hunter;

import static settlement.main.SETT.PATH;

import game.time.TIME;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.util.RESOURCE_TILE;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.sets.ArrayCooShort;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

final class HunterInstance extends RoomInstance implements ROOM_PRODUCER_INSTANCE {


	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	final ArrayCooShort coos;
	private long[] pData;
	private short industry = 0;
	
	double dSkill;
	int iSkill;
	private float skill = 1;
	float produce;
	
	HunterInstance(ROOM_HUNTER blue, TmpArea area, RoomInit init) {
		super(blue, area, init);
		pData = industry().makeData();
		
		int am = 0;
		
		for (COORDINATE c : body()) {
			if (is(c) && blue.tile.init(c.x(), c.y(), this) != null) {
				am++;
			}
		}
		
		coos = new ArrayCooShort(am);
		am = 0;
		for (COORDINATE c : body()) {
			if (is(c) && blue.tile.init(c.x(), c.y(), this) != null) {
				coos.set(am++).set(c);
			}
		}
		employees().maxSet(am);
		employees().neededSet(am);
		activate();
		
	}
	
	@Override
	protected void loadFix() {
		super.loadFix();
		industry %= blueprintI().indus.size();
		pData = industry().makeDataFix(pData);
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
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
		
		industry().updateRoom(this);
		
		if (!PATH().finders.entryPoints.anyHas(mX(), mY()))
			return;
		
		if (iSkill > 0) {
			skill = (float) (dSkill/iSkill);
			dSkill = 0;
			iSkill = 0;
		}
		
		produce += TIME.secondsPerDayI()*updateInterval*employees().employed()*skill;
	}

	@Override
	protected void dispose() {
		
		
	}
	
	@Override
	public ROOM_HUNTER blueprintI() {
		return (ROOM_HUNTER)blueprint();
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		return null;
	}

	@Override
	public long[] productionData() {
		return pData;
	}


	@Override
	public int industryI() {
		return industry;
	}
	
	@Override
	public Industry industry() {
		return blueprintI().indus.get(industry);
	}
	
	@Override
	public void setIndustry(int i) {
		
		if (i == industry)
			return;
		
		Industry in = blueprintI().industries().get(i);
		if (in == null)
			return;
		pData = in.makeData();
		industry = (byte) i;
		iSkill = 0;
		dSkill = 0;
		skill = 0;
		produce = 0;
		
	}

	@Override
	public JOB_MANAGER getWork() {
		return null;
	}

}
