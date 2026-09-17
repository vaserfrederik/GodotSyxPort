package settlement.room.industry.module.consumption;



import java.io.IOException;

import game.GAME;
import game.boosting.BOOSTABLE_O;
import game.boosting.Boostable;
import game.faction.FResources.RTYPE;
import game.time.TIME;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.entity.humanoid.Humanoid;
import settlement.room.industry.module.IndustryRate;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.industry.module.ROOM_IDATA_INSTANCE;
import settlement.room.industry.module.RoomBoost;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.RoomInstance;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LIST;
import util.data.DataOSimple;
import util.data.INT_O.INT_OE;
import util.statistics.HistoryInt;

public class RoomConsumptionAbs implements SAVABLE, IndustryRate {

	
	protected final ArrayListGrower<IndustryResource> allIns = new ArrayListGrower<IndustryResource>();
	protected final ArrayListGrower<IndustryResource> allRes = new ArrayListGrower<IndustryResource>();
	protected IndustryResourceIn[] inMap = new IndustryResourceIn[RESOURCES.ALL().size()];	
	
	private final Boostable bonus;
	public final ArrayListGrower<RoomBoost> roomBoosts = new ArrayListGrower<RoomBoost>();
	public final RoomBlueprintImp blue;
	protected final INT_OE<ROOM_IDATA_INSTANCE> pday;	
	public Boostable conBonus = null;
	
	protected final DataOSimple<ROOM_IDATA_INSTANCE> data = new DataOSimple<ROOM_IDATA_INSTANCE>() {

		@Override
		protected long[] data(ROOM_IDATA_INSTANCE t) {
			return t.productionData();
		}
	
	};
	
	public RoomConsumptionAbs(RoomBlueprintImp blue, Boostable bonus) {
		
		
		
		pday = data .new DataNibble();
		
		this.blue = blue;
		this.bonus = bonus;
		
	}
	
	public IndustryResource in(RESOURCE res) {
		return inMap[res.index()];
	}
	
	public void updateRoom(ROOM_IDATA_INSTANCE r) {
		
		if (pday.get(r) != (TIME.days().bitCurrent()&0x0F)) {
			pday.set(r,  (TIME.days().bitCurrent()&0x0F));
			boolean year = TIME.days().bitsSinceStart()%TIME.years().bitConversion(TIME.days()) == 0;
			for (IndustryResource i : allRes) {
				int v = (int) i.day.getD(r);
				i.dayPrev.set(r, v);
				i.day.incD(r, -v);
				if (year) {
					i.yearPrev.set(r, i.year.get(r));
					i.year.set(r, 0);
				}	
			}
		}
	}
	
	public LIST<IndustryResource> ins(){
		return allIns;
	}
	
	@Override
	public Boostable bonus() {
		return bonus;
	}
	
	@Override
	public LIST<RoomBoost> boosts(){
		return roomBoosts;
	}

	public long[] makeData() {
		return new long[data.longCount()];
	}
	
	public long[] makeDataFix(long[] old) {
		if (old.length != data.longCount())
			return new long[data.longCount()];
		return old;
	}
	
	@Override
	public void save(FilePutter file) {
		file.i(allRes.size());
		for (IndustryResource r : allRes)
			r.save(file);
	}

	@Override
	public void load(FileGetter file) throws IOException {
		int am = file.i();
		if (am != allRes.size()) {
			HistoryInt history = new HistoryInt(48, TIME.days(), false);
			for (int i = 0; i < am; i++)
				history.load(file);
			clear();
		}else {
			for (IndustryResource r : allRes)
				r.load(file);
		}
		
	}

	@Override
	public void clear() {
		for (IndustryResource r : allRes)
			r.clear();
	}

	protected final class IndustryResourceIn extends IndustryResource{
		
		
		public IndustryResourceIn(DataOSimple<ROOM_IDATA_INSTANCE> data, RESOURCE res, double rate, double AI, double AIRate) {
			super(data, allIns.size(), res, rate, AI, AIRate);
			allIns.add(this);
			inMap[resource.index()] = this;
			allRes.add(this);
		}
		
		@Override
		public int inc(ROOM_IDATA_INSTANCE r, double amount, boolean record) {
			int old = (int) day.getD(r);
			day.incD(r, amount);
			int now = (int) day.getD(r);
			int d = now-old;
			if (record)
				GAME.player().res().inc(resource, RTYPE.PRODUCED, -d);
			year.inc(r, d);
			history.inc(d);
			return d;
		}
		
		@Override
		protected double getEffort(Humanoid skill, ROOM_IDATA_INSTANCE r, double workSeconds) {
			return IndustryUtil.calcConsumptionRate(rateSeconds*workSeconds, skill, (RoomInstance)r, RoomConsumptionAbs.this);
		}

	}
	
	public double consumptionRate(RoomInstance ins, Humanoid h, IndustryResource oo) {
		return ins.employees().totEfficiency()*IndustryUtil.calcConsumptionRate(oo.rate, h, ins, this);
	}
	
//	public double consumptionRate(IndustryResource i, RoomInstance ins) {
//		return ins.employees().employed()*ins.employees().totEfficiency()*IndustryUtil.calcConsumptionRate(i.rate, this, ins, i.resource);	
//	}
	

	public double conBonus(BOOSTABLE_O bo) {
		if (conBonus == null)
			return 1;
		return conBonus.get(bo);
	}
	
	

}
