package settlement.room.tests;

import settlement.main.SETT;
import settlement.room.main.Room;
import settlement.room.military.artillery.ArtilleryInstance;
import snake2d.util.sprite.SPRITE;
import view.tool.PLACABLE;
import view.tool.PlacableFixed;

final class ArtilleryTest extends PlacableFixed{

	private final PlacableFixed p;
	
	public ArtilleryTest(PlacableFixed p) {
		
		this.p = p;
		
	}
	
	@Override
	public int width() {
		return p.width();
	}
	
	@Override
	public void place(int tx, int ty, int rx, int ry) {
		p.place(tx, ty, rx, ry);
		Room r = SETT.ROOMS().map.get(tx, ty);
		if (r instanceof ArtilleryInstance) {
			((ArtilleryInstance)r).setEnemy();
		}
	}
	
	@Override
	public CharSequence placable(int tx, int ty, int rx, int ry) {
		return p.placable(tx, ty, rx, ry);
	}
	
	@Override
	public int height() {
		return p.height();
	}

	@Override
	public SPRITE getIcon() {
		return p.getIcon();
	}

	@Override
	public CharSequence name() {
		return "enemy artillery " + p.name();
	}

	@Override
	public PLACABLE getUndo() {
		return null;
	}

	@Override
	public int rotations() {
		return p.rotations();
	}

	@Override
	public int sizes() {
		return p.sizes();
	}

	@Override
	public CharSequence placableWhole(int tx1, int ty1) {
		return p.placableWhole(tx1, ty1);
	}
	
	@Override
	public void rotSet(int rot) {
		p.rotSet(rot);
	};
	
	@Override
	public int rot() {
		return p.rot();
	};
	
}
