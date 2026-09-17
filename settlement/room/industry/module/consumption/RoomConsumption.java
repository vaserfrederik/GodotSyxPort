package settlement.room.industry.module.consumption;

import game.boosting.BOOSTABLES;
import game.boosting.BOOSTING;
import game.boosting.Boostable;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.sprite.SPRITES;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.ROOM_IDATA_INSTANCE;
import settlement.room.industry.module.RoomBoost;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.RoomInstance;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sprite.SPRITE;
import util.data.DataOSimple;
import util.data.INT_O.INT_OE;
import util.info.INFO;
import util.text.Dic;

public class RoomConsumption extends RoomConsumptionAbs {

	public final ArrayListGrower<ExtraInfo> extra = new ArrayListGrower<ExtraInfo>();

	
	public RoomConsumption(RoomBlueprintImp blue, Json json, Boostable bonus) {
		super(blue, bonus);
		
		if (json.has("CONSUMPTION")) {
			json = json.json("CONSUMPTION");
			
			for (String k : json.keys()) {
				RESOURCE res = RESOURCES.map().get(k, json);
				Json j = json.json(k);
				double rate = j.d("RATE", 0, 10000);
				
				new IndustryResourceIn(data, res, rate, rate, rate);
				extra.add(new ExtraInfo(j, data));
			}
		}
		
		INFO info = new INFO(Dic.¤¤Resources, "");
		
		double m = 1;
		for (IndustryResource r : allIns) {
			m += boost(r);
		}
		double max = m;
		
		roomBoosts.add(new RoomBoost() {
			
			@Override
			public INFO info() {
				return info;
			}
			
			@Override
			public double get(RoomInstance r) {
				double m = 1;
				for (IndustryResource res : allIns) {
					if (stored(res).get((ROOM_IDATA_INSTANCE) r) > 0)
						m += boost(res);
				}
				return m;
			}
			
			@Override
			public double max() {
				return max;
			}
			
			@Override
			public double min() {
				return 1;
			}
		});
		SPRITE icon = SPRITES.icons().l.star.twin(blue.iconBig(), DIR.C, 1);
		
		conBonus = BOOSTING.push("CONSUMPTION_" + blue.key, 1, Dic.¤¤ConsumptionRate + ": " + blue.info.name, Dic.¤¤ConsumptionRate + ": " + blue.info.name, icon, BOOSTABLES.CONSUMPTION());
	}
	
	public boolean enabled(IndustryResource res, ROOM_IDATA_INSTANCE ins) {
		return extra.get(res.index()).enabled.get(ins) == 1;
		
	}
	
	public void enabledToggle(IndustryResource res, ROOM_IDATA_INSTANCE ins, RoomInstance i) {
		extra.get(res.index()).enabled.set(ins, (extra.get(res.index()).enabled.get(ins)+1)&1);
		ins.getWork().resetResourceSearch();
		if (!enabled(res, ins)) {
			releaseResources(i, ins);
		}
	}
	
	public double boost(IndustryResource res) {
		return extra.get(res.index()).boost;
	}
	

	
	public INT_OE<ROOM_IDATA_INSTANCE> stored(IndustryResource res) {
		return extra.get(res.index()).amount;
	}
	
	public INT_OE<ROOM_IDATA_INSTANCE> reseved(IndustryResource res) {
		return extra.get(res.index()).reserved;
	}
	
	public boolean shouldFecth(IndustryResource r, ROOM_IDATA_INSTANCE ins, RoomInstance emp) {
		
		if (!enabled(r, ins))
			return false;
		if (!ins.getWork().resourceShouldSearch(r.resource))
			return false;
		
		double am = extra.get(r.index()).amount.get(ins);
		double res = extra.get(r.index()).reserved.get(ins);
		double min = Math.ceil(emp.employees().employed()*r.rate);
		return am + res < min;
		
		
	}
	
	static class ExtraInfo {
		
		public final double boost;
		public final INT_OE<ROOM_IDATA_INSTANCE> enabled;
		public final INT_OE<ROOM_IDATA_INSTANCE> amount;
		public final INT_OE<ROOM_IDATA_INSTANCE> reserved;
		ExtraInfo(Json j, DataOSimple<ROOM_IDATA_INSTANCE> data){
			boost = j.d("BONUS", 0, 1000);
			enabled = data.new DataBit();
			amount = data.new DataInt();
			reserved = data.new DataShort();
		}
		
	}
	
	public interface ROOM_CONSUMPTION_HASER {
		public RoomConsumption consumption();
	}
	
	public void releaseResources(RoomInstance ins, ROOM_IDATA_INSTANCE insc) {
		
		double stations = 0;
		
		for (COORDINATE c : ins.body()) {
			if (ins.is(c) && insc.getWork().getJob(c) != null) {
				stations++;
			}
		}
		
		if (stations > 0) {
			
			for (IndustryResource res : ins()) {
				
				int delta = (int) Math.ceil(stored(res).get(insc)/stations);
				
				for (COORDINATE c : ins.body()) {
					if (ins.is(c) && insc.getWork().getJob(c) != null) {
						int am = Math.min(delta, stored(res).get(insc));
						if (am > 0) {
							for (int di = 0; di < DIR.ALL.size(); di++) {
								int dx = c.x() + DIR.ALL.get(di).x();
								int dy = c.y() + DIR.ALL.get(di).y();
								if (!SETT.PATH().solidity.is(dx, dy)) {
									SETT.THINGS().resources.create(dx, dy, res.resource, am);
									stored(res).inc(insc, -am);
									break;
								}
							}
						}
					}
				}
			}
		}
		for (IndustryResource res : ins()) {
			int am = stored(res).get(insc);
			if (am > 0) {
				SETT.THINGS().resources.create(ins.mX(), ins.mY(), res.resource, am);
			}
			stored(res).set(insc, 0);
		}
		
	}
	
	
	@Override
	public double consumptionRate(RoomInstance ins, Humanoid h, IndustryResource res) {
		ROOM_IDATA_INSTANCE insi = (ROOM_IDATA_INSTANCE) ins;
		if (stored(res).get(insi) > 0) {
			return super.consumptionRate(ins, h, res);
		}
		return 0;
	}
	
}