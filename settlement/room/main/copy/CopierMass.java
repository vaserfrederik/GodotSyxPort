package settlement.room.main.copy;

import game.faction.FACTIONS;
import settlement.main.SETT;
import settlement.room.main.ROOMA;
import settlement.room.main.Room;
import settlement.room.main.TmpArea;
import settlement.room.main.construction.ConstructionInit;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.placement.PLACEMENT;
import settlement.room.main.util.RoomAreaWrapper;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;

public final class CopierMass {
	
	CopierMass(){

	}

	private static RoomAreaWrapper wrap = new RoomAreaWrapper();
	
	public boolean isPlacable(int sx, int sy, int dx, int dy) {
		
		Room r = SETT.ROOMS().map.get(sx, sy);
		if (r == null)
			return false;
		
		if (!canCopy(sx, sy))
			return false;
		
		Furnisher c = r.constructor();
		
		if (c.placable(dx, dy, SETT.ROOMS().fData.item.get(sx, sy), SETT.ROOMS().fData.tile.get(sx, sy)) != null)
			return false;

		if (PLACEMENT.placable(dx, dy, c.blue(), true) != null)
			return false;
			
		FurnisherItem it = SETT.ROOMS().fData.item.get(sx, sy);  
		
		if (it == null)
			return true;
		
		FurnisherItemTile tile = SETT.ROOMS().fData.tile.get(sx, sy);  
		
		if (tile.mustBeReachable) {
			int bi = 0;
			for (DIR d : DIR.ORTHO) {
				if (SETT.PLACA().solidityWill.is(dx, dy, d))
					bi++;
			}
			if (bi == 4)
				return false;
			
		}
		
		return true;
	}
	
	public boolean canCopy(int tx, int ty) {
		Room r = SETT.ROOMS().map.get(tx, ty);
		if (r == null)
			return false;
		if (r.blueprint() == SETT.ROOMS().THRONE)
			return false;
		if (r.constructor() == null)
			return false;
		if (!r.constructor().blue().reqs.passes(FACTIONS.player()))
			return false;
		return (r.constructor().canBeCopied());
	}
	
	public void copy(int rx, int ry, int destCX, int destCY, int rot) {
		
		Room room = SETT.ROOMS().map.get(rx, ry);
		
		if (room == null || room.constructor() == null || room.constructor().blue() == null)
			return;
		
		wrap.done();
		ROOMA r = wrap.init(room, rx, ry);
		
		for (COORDINATE c : r.body()) {
			if (r.is(c)) {
				int dx = c.x()-r.body().cX();
				int dy = c.y()-r.body().cY();
				for (int i = 0; i < rot; i++) {
					int k = dx;
					dx = -dy;
					dy = k;
				}
				int x = (int) (destCX+dx);
				int y = (int) (destCY+dy);
				if (!isPlacable(c.x(), c.y(), x, y)) {
					return;
				}
				
			}
		}
		
		TmpArea tmp = SETT.ROOMS().tmpArea(this);
		
		for (COORDINATE c : r.body()) {
			if (r.is(c)) {
				int dx = c.x()-r.body().cX();
				int dy = c.y()-r.body().cY();
				for (int i = 0; i < rot; i++) {
					int k = dx;
					dx = -dy;
					dy = k;
				}
				int x = (int) (destCX+dx);
				int y = (int) (destCY+dy);
				tmp.set(x, y);
			}
		}
		
		for (COORDINATE c : r.body()) {
			if (r.is(c)) {
				FurnisherItem it = SETT.ROOMS().fData.item.get(c);
				if (it == null)
					continue;
				if (!SETT.ROOMS().fData.isMaster.is(c))
					continue;
				
				COORDINATE ul = SETT.ROOMS().fData.itemX1Y1(c.x(), c.y(), Coo.TMP);
				int scx = ul.x() + it.width()/2;
				int scy = ul.y() + it.height()/2;
				
				int dx = scx- r.body().cX();
				int dy = scy- r.body().cY();
				
				for (int i = 0; i < rot; i++) {
					int k = dx;
					dx = -dy;
					dy = k;
				}
				
				it = it.group.item(it.variation(), (rot+it.rotation)%it.group.rotations());
				int x = destCX +dx - it.width()/2;
				int y = destCY +dy - it.height()/2;
				

				
				SETT.ROOMS().fData.itemSet(x+deltaX(it, rot), y+deltaY(it, rot), it, tmp.room());
				
			}
		}
		
		ConstructionInit init = new ConstructionInit(room, r.mX(), r.mY(), false);
		
		
		SETT.ROOMS().construction.createClean(tmp, init);
	}
	
	private int deltaX(FurnisherItem it, int rot) {
		if ((it.width() & 1) == 0) {
			if (rot == 1)
				return 1;
			if (rot == 2)
				return 1;
		}
		return 0;
	}
	
	private int deltaY(FurnisherItem it, int rot) {
		if ((it.height() & 1) == 0) {
			if (rot == 2)
				return 1;
			if (rot == 3)
				return 1;
		}
		return 0;
	}

	
}
