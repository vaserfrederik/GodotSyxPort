package settlement.room.main.furnisher;

import game.GameDisposable;
import settlement.path.AVAILABILITY;
import settlement.room.sprite.RoomSprite;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Alloc;
import snake2d.util.map.MAP_OBJECT;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.INDEXED;

public final class FurnisherItem implements INDEXED, MAP_OBJECT<FurnisherItemTile>{

	final FurnisherItemTile[][] tiles;
	public final double multiplierStats;
	public final double multiplierCosts;
	final int id;
	private final int firstX;
	private final int firstY;
	public final FurnisherItemGroup group;
	final static ArrayList<FurnisherItem> itemsTmp = new ArrayList<>(127);
	static {
		new GameDisposable() {
			
			@Override
			protected void dispose() {
				itemsTmp.clearSloppy();
			}
		};
	}
	public final int area;
	private final int[] brokenResourceAmount;
	public final int rotation;
	final int size;
	private final int reachableTiles;

	FurnisherItem(int size, FurnisherItemTile[][] its, double multiplierStats, double multiplierCosts, FurnisherItemGroup group, int index, int rot) {
		this.size = size;
		this.id = index;
		tiles = its;
		this.multiplierStats = multiplierStats;
		this.multiplierCosts = multiplierCosts;
		this.group = group;
		this.rotation = rot;
		int fx = -1;
		int fy = -1;
		int a = 0;
		for (int y = 0; y < its.length; y++)
			for (int x = 0; x < its[0].length; x++) {
				if ( tiles[y][x] != null && tiles[y][x].sprite() != null) {
					a++;
					if (fx == -1 && fy == -1) {
						fx = x;
						fy = y;
					}
				}
				
				
			}
		if (fx == -1 || fy == -1) {
			for (int y = 0; y < its.length; y++)
				for (int x = 0; x < its[0].length; x++) {
					if ( tiles[y][x] != null) {
						a++;
						if (fx == -1 && fy == -1) {
							fx = x;
							fy = y;
						}
					}
					
					
				}
		}
		if (fx == -1 || fy == -1)
			throw new RuntimeException();
		
		
		area = a;
		firstX = fx;
		firstY = fy;
		
		
		if (group != null) {
			brokenResourceAmount = Alloc.ii(group.blueprint.resources());
			for (int i = 0; i < group.blueprint.resources(); i++) {
				brokenResourceAmount[i] = (int) Math.ceil(group.costs[i]*multiplierCosts/area);
			}
		}else {
			brokenResourceAmount = null;
		}
		
		int re = 0;
		for (int y = 0; y < its.length; y++)
			for (int x = 0; x < its[0].length; x++) {
				FurnisherItemTile t = get(x, y);
				if (t == null)
					continue;
				if (t.availability.player <= AVAILABILITY.ROOM.player)
					re++;
				else {
					for (DIR d : DIR.ORTHO) {
						t = get(x, y, d);
						if (t == null || t.availability.player <= AVAILABILITY.ROOM.player) {
							re++;
							break;
						}
					}
				}
			}
		reachableTiles = re;
		
	}

	public FurnisherItem(FurnisherItemTile[][] its, double multiplierCosts, double multiplierStats) {
		this(0, its, multiplierStats, multiplierCosts, null, 0, 0);
		itemsTmp.add(this);
	}
	
	public FurnisherItem(FurnisherItemTile[][] its, double multiplier) {
		this(its, multiplier, multiplier);
	}

	public int width() {
		return tiles[0].length;
	}

	public int height() {
		return tiles.length;
	}

	public int firstX() {
		return firstX;
	}

	public int firstY() {
		return firstY;
	}

	public FurnisherItemGroup group() {
		return group;
	}
	
//	public double c22ost(int index, int upgrade) {
//		return (group.costs[index]*multiplierCosts)*group.blueprint.blue().upgrades().resMask(upgrade, index);
//	}
	
	public double cost2(int index, int upgrade) {
		return (group.costs[index]*multiplierCosts)*group.blueprint.blue().upgrades().resMask(upgrade, index);
	}
	
	public double costFlat(int index) {
		return group.costs[index]*multiplierCosts;
	}
	
	public double stat(FurnisherStat stat) {
		return group.stats[stat.index()]*multiplierStats;
	}
	
	public int brokenResourceAmount(int index) {
		return brokenResourceAmount[index];
	}
	
	public CharSequence placable(int tx1, int ty1) {
		return null;
	}

	@Override
	public int index() {
		return id;
	}

	@Override
	public FurnisherItemTile get(int tile) {
		throw new RuntimeException();
	}
	
	public int reachableTiles() {
		return reachableTiles;
	}

	@Override
	public FurnisherItemTile get(int tx, int ty) {
		if (tx < 0 || tx >= width())
			return null;
		if (ty < 0 || ty >= height())
			return null;
		return tiles[ty][tx];
	}
	
	public RoomSprite sprite(int tx, int ty) {
		FurnisherItemTile t = get(tx, ty);
		if (t != null)
			return t.sprite();
		return null;
	}
	
	public int variation() {
		return size;
	}

}