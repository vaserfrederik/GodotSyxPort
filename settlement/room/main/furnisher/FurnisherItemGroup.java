package settlement.room.main.furnisher;

import snake2d.util.sets.INDEXED;

public final class FurnisherItemGroup implements INDEXED{

	final FurnisherItem[][] items;
	public final CharSequence name;
	public final CharSequence desc;
	public final int max;
	public final int min;
	public final Furnisher blueprint;
	final double[] costs;
	final double[] stats;
	private final int index;
	
	FurnisherItemGroup(Furnisher b, int rots, CharSequence name, CharSequence desc, int min, int max, double[] costs, double[]stats) {
		if (rots >= 4)
			throw new RuntimeException();
		
		this.blueprint = b;
		if (FurnisherItem.itemsTmp.size() == 0)
			throw new RuntimeException("No items declared");
		this.costs = costs;
		this.stats = stats;
		this.name = name;
		this.desc = desc;
		this.items = new FurnisherItem[FurnisherItem.itemsTmp.size()][rots + 1];
		index = b.pgroups.add(this);
		this.max = max;
		this.min = min;
		for (int s = 0; s < FurnisherItem.itemsTmp.size(); s++) {
			FurnisherItem copy = FurnisherItem.itemsTmp.get(s);
			FurnisherItem item = new FurnisherItem(s, copy.tiles, copy.multiplierStats, copy.multiplierCosts, this, b.allItems.size(), 0);
			b.allItems.add(item);
			
			this.items[s][0] = item;
			for (int r = 1; r <= rots; r++)
				this.items[s][r] = getRot(s, item, r);
		}
		FurnisherItem.itemsTmp.clear();
	}

	public CharSequence name() {
		return name;
	}
	
	public CharSequence desc() {
		return desc;
	}
	
	private FurnisherItem getRot(int size, FurnisherItem other, int rot) {

		FurnisherItemTile[][] its = other.tiles.clone();

		int r = rot;
		while (rot > 0) {
			its = rotate(its);
			rot--;
		}
		FurnisherItem item = new FurnisherItem(size, its, other.multiplierStats, other.multiplierCosts, this, blueprint.allItems.size(), r);

		blueprint.allItems.add(item);
		return item;
	}
	
	private FurnisherItemTile[][] rotate(FurnisherItemTile[][] l) {
		final int M = l.length;
		final int N = l[0].length;
		FurnisherItemTile[][] ret = new FurnisherItemTile[N][M];
		for (int r = 0; r < M; r++) {
			for (int c = 0; c < N; c++) {
				ret[c][M - 1 - r] = l[r][c];
			}
		}
		return ret;
	}
	
	public FurnisherItem item(int size, int rot) {
		return items[size][rot];
	}
	
	public int size() {
		return items.length;
	}
	
	public int rotations() {
		return items[0].length;
	}

	@Override
	public int index() {
		return index;
	}
	
	public double cost(int ri, int upgrade) {
		return costs[ri]*blueprint.blue().upgrades().resMask(upgrade, ri);
	}
	
	public double stat(int si) {
		return stats[si];
	}
	
}