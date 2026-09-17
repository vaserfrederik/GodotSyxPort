package settlement.room.main.placement;

import game.GAME;
import game.faction.FResources.RTYPE;
import settlement.main.SETT;
import settlement.room.main.MapDataF;
import settlement.room.main.construction.ConstructionData;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemGroup;
import settlement.room.main.furnisher.FurnisherStat;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.Alloc;

final class UtilStats {
	
	private double[] stats = new double[8];
	private double[] statsR = new double[8];
	private double[] needed = new double[8];
	private double[] allocated = new double[8];
	private double[] resRemoved = new double[8];
	private int[] placedGroups = Alloc.ii(16);
	private final RoomPlacer p;
	int walls;
	int items;
	private int tick = -1;
	UtilStats(RoomPlacer p) {
		this.p = p;
	}
	
	public int needed(int ri) {
		update();
		return (int) Math.ceil(needed[ri]);
	}
	
	public int allocated(int ri) {
		update();
		return (int) Math.round(allocated[ri]);
	}
	
	public double stat(int si) {
		update();
		return statsR[si];
	}
	
	public int groups(FurnisherItemGroup g) {
		update();
		return placedGroups[g.index()];
	}
	
	void removeTile(int tx, int ty) {
		SETT.FLOOR().clearer.clear(tx, ty);
		for (int i = 0; i < p.blueprint().constructor().resources(); i++) {
			resRemoved[i] += p.blueprint().constructor().areaCost(i, p.instance.upgrade())*0.75;
			if (resRemoved[i] >= 1) {
				SETT.THINGS().resources.create(tx,  ty, p.blueprint().constructor().resource(i), (int) resRemoved[i]);
				GAME.player().res().inc(p.blueprint().constructor().resource(i), RTYPE.CONSTRUCTION, (int) resRemoved[i]);
				resRemoved[i] -= (int)resRemoved[i];
			}
		}
		
	}
	
	void removeItem(int tx, int ty, FurnisherItem it) {
		for (int i = 0; i < p.blueprint().constructor().resources(); i++) {
			resRemoved[i] += it.cost2(i, p.instance.upgrade())*0.75;
			if (resRemoved[i] >= 1) {
				SETT.THINGS().resources.create(tx,  ty, p.blueprint().constructor().resource(i), (int) resRemoved[i]);
				GAME.player().res().inc(p.blueprint().constructor().resource(i), RTYPE.CONSTRUCTION, (int) resRemoved[i]);
				resRemoved[i] -= (int)resRemoved[i];
			}
		}
	}
	
	void updatee() {
		tick = GAME.updateI()-1;
		update();
	}
	
	private void update() {
		if (tick == GAME.updateI())
			return;
		
		tick = GAME.updateI();
		items = 0;
		if (p.blueprint() == null)
			return;
		Furnisher b = p.blueprint().constructor();
		if (b == null) {
			GAME.Notify("? " + p.blueprint().info.name);
		}
		MapDataF d = SETT.ROOMS().fData;
		AREA a = p.instance;
		
		for (int i = 0; i < needed.length; i++) {
			needed[i] = 0;
			stats[i] = 0;
			allocated[i] = 0;
			statsR[i] = 0;
		}
		for (int i = 0; i < placedGroups.length; i++)
			placedGroups[i] = 0;
		
		walls = p.autoWalls.is() ? p.door.getWalls() : 0;
		int floored = 0;
		for (COORDINATE c : a.body()) {
			if (!a.is(c))
				continue;
			if (ConstructionData.dFloored.is(c, 1))
				floored ++;
			
			if (d.isMaster.is(c)) {
				items++;
				FurnisherItem it = d.item.get(c);
				if (it == null) {
					System.err.println(p.blueprint().info.name + " " + d.itemIndexx.get(c));
				}
				
				
				placedGroups[it.group().index()]++;
				for (int i = 0; i < b.resources(); i++) {
					needed[i] += it.cost2(i, p.instance.upgrade());
					if (ConstructionData.dConstructed.is(c, 1) && ConstructionData.dBroken.is(c, 0))
						allocated[i] += it.cost2(i, p.instance.upgrade());
				}
				for (FurnisherStat s : b.stats()) {
					stats[s.index()] += it.stat(s);
				}	
			}
		}
		
		for (int i = 0; i < b.resources(); i++) {
			
			allocated[i] = (int) Math.ceil(allocated[i]);
			
			needed[i] += Math.ceil(a.area()*b.areaCost(i, p.instance.upgrade()));
			
			allocated[i] += Math.ceil(floored*b.areaCost(i, p.instance.upgrade()));
		}
		for (FurnisherStat s : b.stats()) {
			statsR[s.index()] = s.get(a, stats);
		}
	}

	public void clear() {
		for (int i = 0; i < needed.length; i++) {
			needed[i] = 0;
			stats[i] = 0;
			allocated[i] = 0;
			resRemoved[i] = 0;
			statsR[i] = 0;
		}
		for (int i = 0; i < placedGroups.length; i++)
			placedGroups[i] = 0;
		items = 0;
	}
	
	public double statIncr(FurnisherItem it, FurnisherStat s) {
		double pp = stats[s.index()];
		double old = statsR[s.index()];
		stats[s.index()] += it.stat(s);
		double n = s.get(p.instance, stats);
		stats[s.index()] = pp;
		return n - old;
	}
	
}
