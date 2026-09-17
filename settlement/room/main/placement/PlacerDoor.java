package settlement.room.main.placement;

import static settlement.main.SETT.ROOMS;

import game.GAME;
import init.constant.C;
import init.sprite.SPRITES;
import init.sprite.UI.UICons;
import settlement.job.Job;
import settlement.main.SETT;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.construction.ConstructionData;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.tilemap.terrain.TBuilding;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.map.MAP_BOOLEAN;
import snake2d.util.sprite.SPRITE;
import util.rendering.RenderData;
import util.text.D;
import view.tool.PLACABLE;
import view.tool.PLACER_TYPE;
import view.tool.PlacableMulti;

final class PlacerDoor {

	protected final Room.RoomInstanceImp a;
	private final UtilHistory history;

	final UICons cWall = SPRITES.cons().BIG.filled;
	final UICons cDoor = SPRITES.cons().BIG.outline;
	
	private static CharSequence ¤¤name = "¤Place Doorway";
	private static CharSequence ¤¤shrink = "¤Remove Doorway";
	private static CharSequence ¤¤cp = "Room will be blocked! Place doorways so that the room can be entered from the outside";

	static {
		D.ts(PlacerDoor.class);
	}

	final PlacableMulti undo = new PlacableMulti(¤¤shrink) {

		private SPRITE icon = new SPRITE.Twin(SPRITES.icons().m.wall_opening, SPRITES.icons().m.anti);

		@Override
		public void place(int tx, int ty, AREA a, PLACER_TYPE t) {
			if (removeWithoutHistory(tx, ty))
				history.placeDoor(tx, ty, -1);
		}

		@Override
		public CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE t) {
			if (!isEdge.is(tx, ty))
				return E;
			
			if (!UtilWallPlacability.wallCanBe.is(tx, ty))
				return E;
			
			if (!isOpening.is(tx, ty)) {
				return E;
			}
			return null;
		}

		@Override
		public SPRITE getIcon() {
			return icon;
		}

	};
	
	final PlacableMulti placer = new PlacableMulti(¤¤name) {
		
		@Override
		public void place(int tx, int ty, AREA area, PLACER_TYPE type) {
			if (placeWithoutHistory(tx, ty)) {
				history.placeDoor(tx, ty, 1);
			}
		}
		
		@Override
		public CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type) {
			if (!isEdge.is(tx, ty))
				return E;
			if (!UtilWallPlacability.wallCanBe.is(tx, ty))
				return E;
			if (isOpening.is(tx, ty))
				return E;
			return null;
		}
		
		@Override
		public void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, AREA area,
				PLACER_TYPE type, boolean isPlacable, boolean areaIsPlacable) {
			cDoor.render(r, mask, x, y);
		}
		
		@Override
		public PLACABLE getUndo() {
			return undo;
		}

		@Override
		public SPRITE getIcon() {
			return SPRITES.icons().m.wall_opening;
		}
	};

	public boolean isOpening(int tx, int ty) {
		if (!isEdge.is(tx, ty))
			return false;
		if (!UtilWallPlacability.wallCanBe.is(tx, ty))
			return false;
		return true;
	}
	
	public PlacerDoor(RoomPlacer embryo) {
		this.a = embryo.instance;
		this.history = embryo.history;

	}

	boolean placeWithoutHistory(int tx, int ty) {
		
		if (!isEdge.is(tx, ty))
			return false;
		if (!UtilWallPlacability.wallCanBe.is(tx, ty))
			return false;
		if (isOpening.is(tx, ty))
			return false;
		
		for (int i = 0; i < DIR.ALL.size(); i++) {
			DIR d = DIR.ALL.get(i);
			int dx = tx + d.x();
			int dy = ty + d.y();
			if (!a.is(dx, dy))
				continue;
			
			int m = ConstructionData.dWall.get(dx, dy);
			
			if (d.isOrtho())
				m |= d.mask();
			else
				m |=(d.mask()<<4);
			
			ConstructionData.dWall.set(a, dx, dy, m);
		}
		return true;
	}

	boolean removeWithoutHistory(int tx, int ty) {
		
		if (!isEdge.is(tx, ty))
			return false;
		
		if (!UtilWallPlacability.wallCanBe.is(tx, ty))
			return false;
		
		if (!isOpening.is(tx, ty))
			return false;
		
		for (int i = 0; i < DIR.ALL.size(); i++) {
			DIR d = DIR.ALL.get(i);
			int dx = tx + d.x();
			int dy = ty + d.y();
			if (!a.is(dx, dy))
				continue;
			
			int m = ConstructionData.dWall.get(dx, dy);
			
			if (d.isOrtho())
				m &= ~d.mask();
			else
				m &= ~(d.mask()<<4);
			
			ConstructionData.dWall.set(a, dx, dy, m&0x0FF);
		}
		return true;
	}

	


	private int tick;
	private int walls;
	private int openings;
	private int mountains;
	
	public void init(int tx, int ty) {
		for (int i = 0; i < DIR.ALL.size(); i++) {
			DIR d = DIR.ALL.get(i);
			
			if (isEdge.is(tx, ty, d)) {
				if (UtilWallPlacability.openingIsReal.is(tx, ty, d)) {
					placeWithoutHistory(tx+d.x(), ty+d.y());
				}
			}
			
		}
		
	}
	
	private void update() {
		if (tick == GAME.updateI()) {
			return;

		}
		
		tick = GAME.updateI();
		walls = 0;
		openings = 0;
		mountains = 0;
		
		boolean over = Job.overwrite;
		Job.overwrite = false;
		
		for (int y = a.body().y1()-1; y <= a.body().y2(); y++) {
			for (int x = a.body().x1()-1; x <= a.body().x2(); x++) {
				if (!isEdge.is(x, y))
					continue;
				
				if (!UtilWallPlacability.wallisReal.is(x, y)) {
					if (isOpening.is(x, y)) {
						if (SETT.TERRAIN().CAVE.is(x, y))
							;
						else
							openings++;
					}else {
						if (SETT.TERRAIN().CAVE.is(x, y))
							mountains++;
					}
						
					continue;
				}else if (UtilWallPlacability.wallShouldBuild.is(x, y)) {
					if (SETT.TERRAIN().CAVE.is(x, y))
						mountains++;
					else if (isOpening.is(x, y))
						openings++;
					else
						walls++;
							
				}
			}
		}
		
		Job.overwrite = over;
		
	}
	
	public int getWalls() {
		update();
		return walls;
	}
	
	public int getOpenings() {
		update();
		return openings;
	}
	
	public int getMountains() {
		update();
		return mountains;
	}
	
	CharSequence createProblem() {
		for (COORDINATE c : a.body()) {
			if (!a.is(c))
				continue;
			
			if (SETT.ROOMS().fData.tile.get(c) != null && SETT.ROOMS().fData.tile.get(c).isBlocker())
				continue;
			
			int m = ConstructionData.dWall.get(c) &0x0F;
			if (m == 0)
				continue;
			for (int i = 0; i < DIR.ORTHO.size(); i++) {
				DIR d = DIR.ORTHO.get(i);
				int dx = c.x() + d.x();
				int dy = c.y() + d.y();
				if (!SETT.IN_BOUNDS(dx, dy))
					continue;
				if (!a.is(dx, dy)) {
					if (UtilWallPlacability.wallCanBe.is(dx, dy) && (d.perpendicular().mask() & m) != 0)
						return null;
					else if (SETT.ROOMS().map.is(dx, dy) && SETT.PATH().getAvailability(dx, dy).player > 0)
						return null;
				}
			}
			
		}
		return ¤¤cp;
	}

	public void renderTmpPlaceArea(SPRITE_RENDERER r, int x, int y, int tx, int ty, AREA area) {
		
		if (a.is(tx, ty))
			return;
		
		

		for (int i = 0; i < DIR.ALL.size(); i++) {
			DIR d = DIR.ALL.get(i);
			int dx = tx + d.x();
			int dy = ty + d.y();
			
			
			if (!UtilWallPlacability.wallShouldBuild.is(dx, dy))
				continue;
			
			
			
			if (area.is(dx, dy))
				continue;
			
			if (isEdge.is(dx, dy))
				continue;

			cWall.render(r, 0, x + C.TILE_SIZE * d.x(), y + C.TILE_SIZE * d.y());
		}
	}
	
	public void renderWall(SPRITE_RENDERER r, RenderData.RenderIterator it) {
		
		for (int i = 0; i < DIR.ALL.size(); i++) {
			DIR d = DIR.ALL.get(i);
			int dx = it.tx() + d.x();
			int dy = it.ty() + d.y();
			if (a.is(dx, dy))
				continue;
			if (UtilWallPlacability.wallisReal.is(dx, dy)) {
				if (isOpening.is(dx, dy))
					cDoor.render(r, 0, it.x() + C.TILE_SIZE * d.x(), it.y() + C.TILE_SIZE * d.y());
				continue;
			}
			
			if (!UtilWallPlacability.wallShouldBuild.is(dx, dy)) {
				continue;
			}
			
			if (isOpening.is(dx, dy))
				cDoor.render(r, 0, it.x() + C.TILE_SIZE * d.x(), it.y() + C.TILE_SIZE * d.y());
			else
				cWall.render(r, 0, it.x() + C.TILE_SIZE * d.x(), it.y() + C.TILE_SIZE * d.y());
		}
	}

	public void renderWall(SPRITE_RENDERER r, FurnisherItem a, int tx, int ty, int rx, int ry, int x, int y) {
		if (!a.is(rx, ry) || a.get(rx, ry).mustBeReachable)
			return;
		
		for (int i = 0; i < DIR.ALL.size(); i++) {
			DIR d = DIR.ALL.get(i);
			int dx = tx + d.x();
			int dy = ty + d.y();
			
			if (a.is(rx + d.x(), ry + d.y()))
				continue;
			
			if (UtilWallPlacability.wallisReal.is(dx, dy)) {
				continue;
			}
				
			if (!UtilWallPlacability.wallCanBe.is(dx, dy))
				continue;
			
			if (!UtilWallPlacability.wallShouldBuild.is(dx, dy)) {
				continue;
			}
			
			getType(a, rx + d.x(), ry + d.y()).render(r, 0, x + C.TILE_SIZE * d.x(), y + C.TILE_SIZE * d.y());
		}
	}
	

	
	private UICons getType(FurnisherItem a, int rx, int ry) {
		for (int i = 0; i < DIR.ORTHO.size(); i++) {
			DIR d = DIR.ORTHO.get(i);
			if (a.get(rx, ry, d) != null && a.get(rx, ry, d).mustBeReachable)
				return cDoor;
		}
		return cWall;
	}



	public void build(TBuilding structure) {

		for (COORDINATE c : a.body()) {
			if (!a.is(c)) {
				continue;
			}
			FurnisherItemTile tile = ROOMS().fData.tile.get(c);
			if (tile != null && tile.noWalls) {
				continue;
			}

			for (int i = 0; i < DIR.ALL.size(); i++) {
				DIR d = DIR.ALL.get(i);		
				int dx = c.x() + d.x();
				int dy = c.y() + d.y();
				build(structure, dx, dy);
				
			}
		}
	}
	
	private void build(TBuilding structure, int tx, int ty) {
		if (isOpening.is(tx, ty)) {
			UtilWallPlacability.openingBuild(tx, ty, structure);
		}else if(UtilWallPlacability.wallShouldBuild.is(tx, ty)) {
			if (isOpening.is(tx, ty)) {
				UtilWallPlacability.openingBuild(tx, ty, structure);
			}else {
				UtilWallPlacability.wallBuild(tx, ty, structure);
			}
		}
	}
	
	private final MAP_BOOLEAN isolationMap = new MAP_BOOLEAN() {

		@Override
		public boolean is(int tile) {
			throw new RuntimeException();
		}

		@Override
		public boolean is(int tx, int ty) {
			return UtilWallPlacability.wallCanBe.is(tx, ty) && !isOpening.is(tx, ty);
		}
		
	};
	
	public double isolation(RoomBlueprint blue, AREA area, boolean wallOn) {
		return SETT.ROOMS().isolation.getProspect(blue, area, wallOn ? isolationMap : UtilWallPlacability.wallisReal);
	}


	private final MAP_BOOLEAN isEdge = new MAP_BOOLEAN() {
		
		@Override
		public boolean is(int tx, int ty) {
			if (a.is(tx, ty))
				return false;
			for (int i = 0; i < DIR.ALL.size(); i++) {
				DIR d = DIR.ALL.get(i);
				if (a.is(tx, ty, d)) {
					return true;
				}
			}
			return false;
		}
		
		@Override
		public boolean is(int tile) {
			// TODO Auto-generated method stub
			return false;
		}
	};
	
	private MAP_BOOLEAN isOpening = new MAP_BOOLEAN() {
		
		@Override
		public boolean is(int tx, int ty) {
			
			for (int i = 0; i < DIR.ALL.size(); i++) {
				DIR d = DIR.ALL.get(i);
				int dx = tx + d.x();
				int dy = ty + d.y();
				if (a.is(dx, dy)) {
					int m = d.isOrtho() ? d.mask() : (d.mask()<<4);
					return (ConstructionData.dWall.get(dx, dy) & m) != 0;
				}
			}
			return false;
		}
		
		@Override
		public boolean is(int tile) {
			// TODO Auto-generated method stub
			return false;
		}
	};



	



}
