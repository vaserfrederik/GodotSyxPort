package settlement.room.spirit.grave;

import init.race.RACES;
import init.race.Race;
import init.type.CAUSE_LEAVE;
import init.type.CAUSE_LEAVES;
import init.type.HTYPE;
import init.type.HTYPES;
import settlement.stats.STATS;
import settlement.thing.ThingsCorpses.Corpse;
import snake2d.util.bit.BitsLong;
import snake2d.util.misc.CLAMP;

public final class GraveInfo {

	private static final GraveInfo self = new GraveInfo();

	private BitsLong gender = 	new BitsLong	(0x000000000000000Fl);
	private BitsLong race = new BitsLong		(0x0000000000FF0000l);
	private BitsLong type = new BitsLong		(0x00000000FF000000l);
	private BitsLong cause = new BitsLong		(0x000000FF00000000l);
	private BitsLong age = new BitsLong			(0x00FFFF0000000000l);
	private BitsLong has = new BitsLong			(0x8000000000000000l);
	
	private GraveInstance ins;
	private int id;
	private long dataD;
	private int nameD;
	
	private GraveInfo() {

		if (race.mask < RACES.all().size())
			throw new RuntimeException();
		if (type.mask < HTYPES.ALL().size())
			throw new RuntimeException();
		if (cause.mask < CAUSE_LEAVES.ALL().size())
			throw new RuntimeException();
	}
	
	static GraveInfo get(GraveInstance instance, int id) {
		self.ins = instance;
		self.id = id;
		self.dataD = instance.datas[id];
		self.nameD = instance.names[id];
		return self;
	}
	
	public CharSequence name() {
		return STATS.APPEARANCE().name(race(), type(), gender.get(dataD),nameD);
	}

	boolean hasBody() {
		return has.get(dataD) > 0;
	}
	
	public Race race() {
		return RACES.all().get((int) race.get(dataD));
	}
	
	public HTYPE type() {
		return HTYPES.ALL().get((int)type.get(dataD));
	}
	
	public CAUSE_LEAVE cause() {
		return CAUSE_LEAVES.ALL().get((int)cause.get(dataD));
	}
	
	void clear() {
		dataD = has.set(dataD, 0);
		ins.datas[id] = dataD;
	}
	
	public int years() {
		return (int) age.get(dataD);
	}
	
	void bury(Corpse c) {
		
		dataD = has.set(dataD, 1);
		dataD = gender.set(dataD, STATS.APPEARANCE().gender.get(c.indu()));
		nameD = STATS.APPEARANCE().nameData.get(c.indu());
		dataD = type.set(dataD, c.indu().hType().index()); 
		dataD = race.set(dataD, c.indu().race().index);
		dataD = cause.set(dataD, c.cause().index());
		int a =  (int) Math.ceil(STATS.POP().age.years.getD(c.indu()));
		a = CLAMP.i(a, 0, (int) age.mask);
		dataD = age.set(dataD, a);
		ins.datas[id] = dataD;
		ins.names[id] = nameD;
	}
	
	
}
