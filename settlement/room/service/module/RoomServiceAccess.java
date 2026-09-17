package settlement.room.service.module;

import game.GameDisposable;
import game.boosting.BValue;
import game.boosting.BoostSpecs;
import init.race.RACES;
import init.race.Race;
import init.type.HCLASS;
import init.type.HCLASSES;
import init.type.HCLASS_RACE;
import init.type.NEED;
import settlement.entity.humanoid.Humanoid;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.RoomInstance;
import settlement.room.main.util.RoomInitData;
import settlement.stats.STATS;
import settlement.stats.service.StatServiceRoom;
import settlement.stats.standing.StatStanding.StandingDef;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.Json;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.INDEXED;
import snake2d.util.sets.LIST;

public abstract class RoomServiceAccess extends RoomService implements INDEXED{

	private static final ArrayListGrower<RoomServiceAccess> all = new ArrayListGrower<>();
	static {
		new GameDisposable() {
			
			@Override
			protected void dispose() {
				all.clear();
			}
		};
	}
	
	public final StandingDef standingDef;
	public final String[] induMore;
	public final BoostSpecs boosts;
	private final int index;
	
	public RoomServiceAccess(RoomBlueprintImp b, RoomInitData data, NEED need) {
		super(b, data, need);
		this.index = all.add(this);
		induMore = data.text().json("SERVICE").texts("MORE");
		Json json = data.data().json("SERVICE");
		standingDef = new StandingDef(json.json("STANDING"));
		boosts = new BoostSpecs(b.info.name, b.icon, false);
		boosts.read(json, BValue.VALUE1);
		usage = json.dTry("USAGE", 0, 1, 1);
	}
	
	public final void reportAccess(Humanoid a, COORDINATE c) {
		reportAccess(a, c.x(), c.y());
	}
	
	public final void reportContent(Humanoid a, ROOM_SERVICER service) {
		RoomInstance ins = (RoomInstance) service;
		stats().setAccess(a, true, service.quality(), 1, ins.upgrade());
	}
	
	public final void reportAccess(Humanoid a, int tx, int ty) {
		ROOM_SERVICER r = (ROOM_SERVICER) room.get(tx, ty);
		if (r == null)
			return;
		RoomInstance ins = (RoomInstance) r;
		stats().setAccess(a, true, r.quality(), stats().proximity(a), ins.upgrade());
	}
	
	public final void reportDistance(Humanoid a) {
		double p = 1.0-(finder.getDistance()-radius/3)/radius;
		p = CLAMP.d(p, 0, 1);
		p = Math.sqrt(p);
		stats().setProximity(a, p);
	}

	public final void clearAccess(Humanoid a) {
		stats().setAccess(a, false, 0, 0, 0);
	}
	
	public final StatServiceRoom stats() {
		return STATS.SERVICE().ROOMS.get(index);
	}
	
	public interface ROOM_SERVICE_ACCESS_HASER extends ROOM_SERVICE_HASER{
		
		@Override
		RoomServiceAccess service();
	}

	public double cityAccess() {
		double d = 0;
		double p = 0;
		for (int ci = 0; ci < HCLASSES.ALLP().size(); ci++) {
			HCLASS c = HCLASSES.ALLP().get(ci);
			for (int ri = 0; ri < RACES.all().size(); ri++) {
				Race r = RACES.all().get(ri);
				if (stats().permission().is(HCLASS_RACE.clP(r, c))) {
					double pp =  STATS.POP().POP.data(c).get(r);
					p += pp;
					d += pp*stats().access().data(c).getD(r);
					
				}
			}
			
		}
		if (p == 0)
			return 0;
		return d/p;
	}
	
	public boolean accessRequest(Humanoid a) {
		return stats().accessRequest(a);
	}
	
	public boolean isGoodTime() {
		return true;
	}
	
	public static LIST<RoomServiceAccess> ALL(){
		return all;
	}
	
	@Override
	public int index() {
		return index;
	}
}
