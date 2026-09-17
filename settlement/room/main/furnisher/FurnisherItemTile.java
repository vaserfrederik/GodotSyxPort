package settlement.room.main.furnisher;

import settlement.path.AVAILABILITY;
import settlement.room.sprite.RoomSprite;
import snake2d.util.map.MAP_BOOLEAN;
import snake2d.util.sets.INDEXED;

public class FurnisherItemTile implements INDEXED{

	private final int index;
	public final boolean canGoCandle;
	public final RoomSprite sprite;
	public final AVAILABILITY availability;
	public final boolean mustBeReachable;
	public boolean noWalls;
	private int data;

	public FurnisherItemTile(Furnisher p, boolean mustBeReachable, RoomSprite sprite, AVAILABILITY availability,	boolean canGoCandle) {
		this.index = p.tiles.add(this);
		this.canGoCandle = canGoCandle;
		this.sprite = sprite;
		this.availability = availability;
		this.mustBeReachable = mustBeReachable;
	}

	public FurnisherItemTile(Furnisher p, RoomSprite sprite, AVAILABILITY availability, boolean canGoCandle) {
		this(p, false, sprite, availability, canGoCandle);
	}

	public boolean isBlocker() {
		return availability.player < 0 || availability.from > 0;
	}
	
	public boolean isNotBlocker() {
		return !isBlocker() && availability.player <= AVAILABILITY.ROOM.player;
	}
	
	public CharSequence isPlacable(int tx, int ty, MAP_BOOLEAN roomIs, FurnisherItem it, int rx, int ry) {
		
		return null;
	}

	@Override
	public int index() {
		return index;
	}
	
	public FurnisherItemTile setData(int data) {
		this.data = data;
		return this;
	}
	
	public int data() {
		return data;
	}
	
	public RoomSprite sprite() {
		return sprite;
	}
	
}