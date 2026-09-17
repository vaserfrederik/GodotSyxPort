package settlement.room.military.artillery;

import settlement.main.SETT;
import settlement.room.main.ROOMS;
import snake2d.util.sprite.SPRITE;
import view.tool.PLACABLE;
import view.tool.PlacableFixed;

public final class Placer extends PlacableFixed {
	
	private final ROOM_ARTILLERY blue;
	private final PlacableFixed p;
	
	public Placer(ROOM_ARTILLERY blue, ROOMS r) {
		this.blue = blue;
		p = r.placement.placer.createItemPlacer(blue, 0);
		
	}
	
	@Override
	public int width() {
		return p.width();
	}
	
	@Override
	public void place(int tx, int ty, int rx, int ry) {
		p.place(tx, ty, rx, ry);
		SETT.ROOMS().construction.construct(tx, ty);
		ArtilleryInstance ins = blue.get(tx, ty);
		if (ins != null) {
			ins.muster(true);
			ins.fireAtWill(true);
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
		return blue.info.name;
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

