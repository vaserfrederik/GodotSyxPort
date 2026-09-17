package settlement.room.main.placement;

import settlement.main.SETT;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemGroup;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.sprite.text.Str;
import util.GUTIL;
import util.text.D;

final class UtilPlacability {

	private final RoomPlacer p;
	private final Str sError = new Str(100);
	
	private static CharSequence ¤¤TooSmall = "¤The area designated is too small.";
	private static CharSequence ¤¤NotEnoughItems = "¤This room plan has insufficient: {0} items. Place more items inside the shape to continue.";
	private static CharSequence ¤¤NotEnough = "¤This room will have insufficient {0}. Either the shape needs to be expanded, or more items need to placed.";
	private static CharSequence ¤¤Disconnected = "¤Area must be connected!";
	private static CharSequence ¤¤BlockingSelf = "¤Items are cutting off room. Make sure the room can be reached from the outside.";

	
	private static CharSequence ¤¤NotInside = "Must be placed inside the designated area. You must expand the area before you can place items.";
	private static CharSequence ¤¤NotBlockOther = "Must not block other item.";
	private static CharSequence ¤¤WillBeBlock = "Must not be blocked by other items.";
	private static CharSequence ¤¤WillBlockRoom = "Area is not connected, or an item is cutting off part of the room.";
	private static CharSequence ¤¤ItemsREached = "Max amount of this item is reached.";
	private static CharSequence ¤¤ItemMustREac = "Item must be reachable.";
	
	static {
		D.ts(UtilPlacability.class);
	}
	
	
	UtilPlacability(RoomPlacer p){
		D.t(this);
		this.p = p;
	}
	
	public FurnisherItemGroup createProblemGroup() {
		for (FurnisherItemGroup g : p.blueprint().constructor().groups()) {
			if (p.resources.groups(g) < g.min) {
				return g;
			}
		}
		return null;
	}
	
	public CharSequence createProblem(AREA instance) {
		sError.clear();

		if (instance.area() < 1)
			return sError.add(¤¤TooSmall);

		
		
		if (p.blueprint().constructor().constructionProblem(instance) != null) {
			return p.blueprint().constructor().constructionProblem(instance);
		}
		
		for (FurnisherItemGroup g : p.blueprint().constructor().groups()) {
			if (p.resources.groups(g) < g.min) {
				return sError.add(¤¤NotEnoughItems).insert(0, g.name());
			}
		}
		

		for (int si = 0; si < p.blueprint().constructor().stats().size(); si++) {
			
			FurnisherStat s = p.blueprint().constructor().stats().get(si);
			if (p.itemStats(s.index()) < s.min) {
				return sError.add(¤¤NotEnough).insert(0, s.name());
			}
		}
		
		if (!checkAccess(instance)) {
			return ¤¤BlockingSelf;
		}
		
		if (isDisconnected(instance)) {
			return ¤¤Disconnected;
		}
		
		GUTIL.filler().init(this);
		fillFirst(instance);
		int a = instance.area();
		while(GUTIL.filler().hasMore()) {
			COORDINATE c = GUTIL.filler().poll();
			
			
			a--;
			for (DIR d : DIR.ORTHO) {
				if (instance.is(c, d)) {
					
					if (SETT.ROOMS().fData.blocking.is(c)) {
						
						if (SETT.ROOMS().fData.blocking.is(c, d))
							GUTIL.filler().fill(c, d);
						
					}else {
						GUTIL.filler().fill(c, d);
					}
						
				}
			}
		}
		
		GUTIL.filler().done();
		if (a > 0) {
			return ¤¤Disconnected;
		}
		
		if (p.autoWalls.is())
			return p.door.createProblem();
		return null;
	}
	
	
	
	private boolean isDisconnected(AREA instance) {
		if (instance.area() == 0)
			return false;
		GUTIL.filler().init(this);
		fillFirst(instance);
		int a = instance.area();
		while(GUTIL.filler().hasMore()) {
			COORDINATE c = GUTIL.filler().poll();
			a--;
			for (DIR d : DIR.ORTHO) {
				if (instance.is(c, d)) {
					
					if (SETT.ROOMS().fData.blocking.is(c)) {
						
						if (SETT.ROOMS().fData.blocking.is(c, d))
							GUTIL.filler().fill(c, d);
						
					}else {
						GUTIL.filler().fill(c, d);
					}
						
				}
			}
		}
		
		GUTIL.filler().done();
		if (a > 0) {
			return true;
		}
		return false;
	}
	
	private boolean checkAccess(AREA instance) {
		boolean allBlocked = true;
		for (COORDINATE c : instance.body()) {
			if (instance.is(c)) {
				boolean blocked = SETT.ROOMS().fData.availability.get(c).player < 0 || SETT.PATH().solidity.is(c);
				allBlocked &= blocked;
				if (!blocked) {
					for (DIR d : DIR.ORTHO) {
						if (!instance.is(c, d)) {
							if (SETT.ROOMS().fData.availability.get(c, d).player > 0 && !SETT.PATH().solidity.is(c, d))
								return true;
						}
					}
				}
				
			}
		}
		return allBlocked;
	}
	
	private void fillFirst(AREA instance) {
		for (COORDINATE c : instance.body()) {
			if (instance.is(c)) {
				if (!SETT.ROOMS().fData.blocking.is(c)) {
					GUTIL.filler().fill(c);
					return;
				}
			}
		}
		for (COORDINATE c : instance.body()) {
			if (instance.is(c)) {
				GUTIL.filler().fill(c);
				return;
			}
		}
		throw new RuntimeException();
	}
	

	
	public CharSequence itemPlacable(int tx, int ty, int rx, int ry, FurnisherItem item, AREA a) {
		
		FurnisherItemTile t = item.get(rx, ry);
		
		if (t == null)
			return null;
		
		if (!a.is(tx, ty)) {
			return ¤¤NotInside;
		}
		if (SETT.ROOMS().fData.tile.is(tx, ty))
			return ¤¤WillBeBlock;
		
		
		
		
		
		CharSequence s = t.isPlacable(tx, ty, a, item, rx, ry);
		if (s != null)
			return s;
		
		
		if (t.mustBeReachable) {
			int b = 0;
			for (int i = 0; i < DIR.ORTHO.size(); i++) {
				DIR d = DIR.ORTHO.get(i);
				if (item.get(rx, ry, d) != null && item.get(rx, ry, d).isBlocker()) {
					b++;
				}else if (!a.is(tx, ty, d) || SETT.ROOMS().fData.blocking.is(tx, ty, d)) {
					b++;
				}
			}
			if (b == 4) {
				return ¤¤ItemMustREac;
			}
		}
		
		if (t.isBlocker()) {
			for (int i = 0; i < DIR.ORTHO.size(); i++) {
				DIR d = DIR.ORTHO.get(i);
				if (otherItemWillBeBlocked(tx, ty, item, rx, ry, d, a)) {
					return ¤¤NotBlockOther + " 1";
				}
			}
		}
		
		
		return  null;
	}
	
	private boolean otherItemWillBeBlocked(int tx, int ty, FurnisherItem item, int rx, int ry, DIR dir, AREA a) {
		tx += dir.x();
		ty += dir.y();
		rx += dir.x();
		ry += dir.y();
		
		if (a.is(tx, ty) && SETT.ROOMS().fData.mustReach.is(tx, ty)) {
			int b = 0;
			for (int i = 0; i < DIR.ORTHO.size(); i++) {
				DIR d = DIR.ORTHO.get(i);
				FurnisherItemTile t = item.get(rx, ry, d);
				if (t != null && t.isBlocker())
					b++;
				else if (!a.is(tx, ty, d) || SETT.ROOMS().fData.blocking.is(tx, ty, d))
					b++;
			}
			return b == 4;
		}
		return false;
	}
	

	
	
	public CharSequence itemProblem(int x1, int y1, FurnisherItemGroup group, FurnisherItem item, AREA a) {
		

		if (p.resources.groups(group) >= group.max) {
			return ¤¤ItemsREached;
		}
		
		for (int y = 0; y < item.height(); y++) {
			for (int x = 0; x < item.width(); x++) {
				int tx = x+x1;
				int ty = y+y1;
				CharSequence s = itemPlacable(tx, ty, x, y, item, a);
				if (s != null)
					return s;
			}
		}
				
		
		GUTIL.filler().init(this);
		
		for (int y = 0; y < item.height(); y++) {
			for (int x = 0; x < item.width(); x++) {
				FurnisherItemTile t = item.get(x, y);
				if (t != null && t.isBlocker()) {
					int tx = x+x1;
					int ty = y+y1;
					GUTIL.filler().closer.set(tx, ty);
				}
			}
		}
		
		for (int y = -1; y <= item.height(); y++) {
			for (int x = -1; x <= item.width(); x++) {
				if (x == -1 || x == item.width() || y ==-1 || y == item.height()) {
					
					if ((x == -1 && y == -1))
						continue;
					if ((x == -1 && y == item.height()))
						continue;
					if ((x == item.width() && y == -1))
						continue;
					if ((x == item.width() && y == item.height()))
						continue;
					
					int tx = x+x1;
					int ty = y+y1;
					if (checkIfOtherTileBlocked(tx, ty, a) || checkIfOtherItemBlocked(tx, ty, a)) {
						GUTIL.filler().done();
						return ¤¤NotBlockOther + " 2" + " " + checkIfOtherTileBlocked(tx, ty, a) + " " + checkIfOtherItemBlocked(tx, ty, a);
					}
				}
			}
		}
		
		boolean first = false;
		int area = 0;
		for (COORDINATE c : a.body()) {
			if (!a.is(c))
				continue;
			if (!GUTIL.filler().isser.is(c.x(), c.y()) && !isBlockerTile(c.x(), c.y(), a)) {
				area ++;
				if (!first) {
					GUTIL.filler().filler.set(c);
					first = true;
				}
			}
		}
		
		while(GUTIL.filler().hasMore()) {
			COORDINATE c = GUTIL.filler().poll();
			area--;
			for (DIR d: DIR.ORTHO) {
				int dx = c.x() + d.x();
				int dy = c.y() + d.y();
				if (a.is(dx, dy) && !isBlockerTile(dx, dy, a)) {
					GUTIL.filler().fill(dx, dy);
				}
			}
		}
		
		GUTIL.filler().done();
		
		if (area != 0) {
			return ¤¤WillBlockRoom;
		}

		return null;
		
	}
	
	
	private boolean checkIfOtherTileBlocked(int tx, int ty, AREA a) {
		if (!a.is(tx, ty))
			return false;
		FurnisherItemTile t = SETT.ROOMS().fData.tile.get(tx, ty);
		if (t == null || !t.mustBeReachable)
			return false;
		
		COORDINATE c = SETT.ROOMS().fData.itemMaster(tx, ty, Coo.TMP);
		int mx = c.x();
		int my = c.y();
		
		for (int di = 0; di < DIR.ORTHO.size(); di++) {
			DIR d = DIR.ORTHO.get(di);
			int dx = tx+d.x();
			int dy = ty+d.y();
			if (!a.is(dx, dy))
				continue;
			if (GUTIL.filler().isser.is(dx, dy))
				continue;
			t = SETT.ROOMS().fData.tile.get(dx, dy);
			if (t == null)
				return false;
			if (t.isBlocker())
				continue;
			c = SETT.ROOMS().fData.itemMaster(dx, dy, Coo.TMP);
			if (c.x() == mx && c.y() == my)
				continue;
			
			if (t.isNotBlocker())
				return false;
	
		}
		return true;
		
	}
	
	private boolean checkIfOtherItemBlocked(int tx, int ty, AREA a) {
		if (!a.is(tx, ty))
			return false;
		FurnisherItem item = SETT.ROOMS().fData.item.get(tx, ty);
		if (item == null)
			return false;
		
		COORDINATE c = SETT.ROOMS().fData.itemMaster(tx, ty, Coo.TMP);
		
		tx = c.x()-item.firstX();
		ty = c.y()-item.firstY();
		
		for (int y = 0; y < item.height(); y++) {
			for (int x = 0; x < item.width(); x++) {
				if (x == 0 || x == item.width()-1 || y == 0 || y == item.height()-1) {
					int dx = x+tx;
					int dy = y+ty;
					if (!isBlockedTile(dx, dy, a)) {
						return false;
					}
				}
			}
		}
		return true;
		
	}
	
	private boolean isBlockerTile(int tx, int ty, AREA a) {
		if (!a.is(tx, ty))
			return true;
		if (GUTIL.filler().isser.is(tx, ty))
			return true;
		FurnisherItemTile t = SETT.ROOMS().fData.tile.get(tx, ty);
		return t != null && t.isBlocker();
	}
	
	private boolean isBlockedTile(int tx, int ty, AREA a) {
		for (int di = 0; di < DIR.ORTHO.size(); di++) {
			DIR d = DIR.ORTHO.get(di);
			int dx = tx+d.x();
			int dy = ty+d.y();
			if (!isBlockerTile(dx, dy, a))
				return false;
		}
		return true;
	}
	

	
}
