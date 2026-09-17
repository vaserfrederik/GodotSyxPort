package settlement.room.main.placement;

import static settlement.main.SETT.ROOMS;

import init.sprite.SPRITES;
import settlement.main.SETT;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.construction.ConstructionInit;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemGroup;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.tilemap.terrain.TBuilding;
import snake2d.CORE;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.datatypes.RECTANGLE;
import snake2d.util.datatypes.Rec;
import snake2d.util.file.Alloc;
import snake2d.util.sprite.SPRITE;
import util.gui.misc.GBox;
import util.text.D;
import view.tool.PLACABLE;
import view.tool.PLACER_TYPE;
import view.tool.PlacableFixed;
import view.tool.PlacableMessages;
import view.tool.PlacableMulti;

class PlacerItemSingle extends PlacableFixed{


	protected final RoomPlacer embryo;
	private RoomBlueprintImp blueprint;
	protected FurnisherItemGroup group;
	protected final UtilStats res;
	protected final Instance area;
	private int upgrade;
	
	private int[] sizes;

	private static CharSequence ¤¤undo = "¤Remove Item";
	static {
		D.ts(PlacerItemSingle.class);
	}
	
	private final PlacableMulti undo = new PlacableMulti(¤¤undo) {
		
		@Override
		public void place(int tx, int ty, AREA a, PLACER_TYPE t) {
			Room r = SETT.ROOMS().map.get(tx, ty);
			if (r != null && r.constructor() == blueprint.constructor())
				r.remove(tx, ty, true, this, false).clear();
		}
		
		@Override
		public CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE t) {
			Room r = SETT.ROOMS().map.get(tx, ty);
			if (r != null && r.constructor() == blueprint.constructor())
				return null;
			return E;
		}
		
		@Override
		public boolean expandsTo(int fromX, int fromY, int toX, int toY) {
			Room r = SETT.ROOMS().map.get(fromX, fromY);
			if (r != null  && r.constructor() == blueprint.constructor() && r.isSame(fromX, fromY, toX, toY))
				return true;
			return false;
		};
	};
	
	public PlacerItemSingle(RoomPlacer embryo) {
		this.embryo = embryo;
		this.res = embryo.resources;
		this.area = embryo.instance;
	}
	
	public void set(RoomBlueprintImp b, int group, int upgrade) {
		if (SETT.ROOMS() != null) {
			if (sizes == null) {
				sizes = Alloc.ii(SETT.ROOMS().AMOUNT_OF_BLUEPRINTS);
			}
			if (this.blueprint != null) {
				sizes[this.blueprint.index()] = size();
			}
			
		}
		
		this.blueprint = b;
		this.group = b.constructor().pgroups().getC(group);
		this.upgrade = upgrade;
		if (sizes != null) {
			sizeSet(sizes[b.index()]);
		}
	}
	
	@Override
	public CharSequence name() {
		return group.name();
	}

	@Override
	public void place(int tx, int ty, int rx, int ry) {
		FurnisherItem it = group.item(size(), rot());
		
		if (rx == 0 && ry == 0) {

			TBuilding s = blueprint.constructor().mustBeIndoors() ? embryo.structure.get() : null;
			
		
			if (s != null && embryo.autoWalls.is()) {
				embryo.instance.clear(blueprint);
				for (int y = 0; y < it.height(); y++) {
					for (int x = 0; x < it.width(); x++) {
						if (it.get(x, y) != null)
							embryo.instance.set(tx+x, ty+y);
					}
				}
				
				embryo.door.build(s);	
				embryo.instance.clear(blueprint);
				
				for (int y = 0; y < it.height(); y++) {
					for (int x = 0; x < it.width(); x++) {
						
						if (it.get(x, y) != null && it.get(x, y).mustBeReachable) {
							for (int di = 0; di < DIR.ORTHO.size(); di++) {
								int dx = tx+x+DIR.ORTHO.get(di).x();
								int dy = ty+y+DIR.ORTHO.get(di).y();	
								if (UtilWallPlacability.wallisReal.is(dx, dy)) {
									UtilWallPlacability.openingBuild(dx, dy, s);
								}
							}
							
						}
					}
				}
			}
			
			FurnisherItem secret = blueprint.constructor().secretReplacementItem(rot(), it);
			
			if (secret != null) {
				for (int y = 0; y < it.height(); y+=secret.height()) {
					for (int x = 0; x < it.width(); x+=secret.width()) {
						place(secret, tx+x, ty+y, s);
					}
				}
				
				
			}else {
				place(it, tx, ty, s);
			}
			
		}
	}
	
	private void place(FurnisherItem it, int tx, int ty, TBuilding s) {
		
		TmpArea tmp = SETT.ROOMS().tmpArea(this);
		
		
		
		for (int y = 0; y < it.height(); y++) {
			for (int x = 0; x < it.width(); x++) {
				
				if (it.get(x, y) != null) {
					tmp.set(tx+x, ty+y);
				}
			}
		}
		
		
		
		SETT.ROOMS().fData.itemSet(tx, ty, it, tmp.room());
		
		ConstructionInit init = new ConstructionInit(0, blueprint.constructor(), s, 0, blueprint.constructor().getConstructionState());
		
		SETT.ROOMS().construction.createClean(tmp, init);
	}
	

	@Override
	public void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, int rx, int ry, boolean isPlacable, boolean areaIsPlacable) {
		FurnisherItem it = group.item(size(), rot());
		
		FurnisherItemTile t = it.get(rx, ry);
		if (t != null) {
			
			if (t.mustBeReachable) {
				SPRITES.cons().BIG.filled.render(r, 0, x, y);
				COLOR c = CORE.renderer().colorGet();
				COLOR.unbind();
				
				int ri = -1;
				
				if (t.sprite() != null) {
					int d = t.sprite().getData(tx, ty, rx, ry, it, 0);
					ri = t.sprite.rotation(d, it)-1;
				}
				
				if (ri < 0)
					SPRITES.cons().ICO.arrows_inward.render(r, x, y);
				else
					SPRITES.cons().ICO.arrows_inwards.get(ri).render(r, x, y);
				c.bind();
			}else if (t.sprite() != null) {
				int d = t.sprite().getData(tx, ty, rx, ry, it, 0);
				
				

				t.sprite().renderPlaceholder(r, x, y, d, tx, ty, rx, ry, it);

			}else {
				SPRITES.cons().BIG.dashed.render(r, mask, x, y);
			}
		}
		group.blueprint.renderExtra(r, x, y, tx, ty, rx, ry, it);
		
		
		if (blueprint.constructor().mustBeIndoors() && ROOMS().placement.placer.autoWalls.is()) {
			ROOMS().placement.placer.door.renderWall(r, it, tx, ty, rx, ry, x, y);
		}
	}

	@Override
	public int width() {
		return group.item(size(), rot()).width();
	}

	@Override
	public int height() {
		return group.item(size(), rot()).height();
	}

	@Override
	public CharSequence placableWhole(int tx1, int ty1) {
		itemAreaCurrent = null;
		
		
		FurnisherItem it = group.item(size(), rot());
		CharSequence s = it.placable(tx1, ty1);
		if (s != null)
			return s;
		
		itemAreaCurrent = itemArea.set(group.item(size(), rot()), tx1, ty1);
		
		return null;
	}
	
	@Override
	public CharSequence placable(int tx, int ty, int rx, int ry) {
		FurnisherItem it = group.item(size(), rot());
		if (it.get(rx, ry) == null)
			return null;

		
		CharSequence s = PLACEMENT.placable(tx, ty, blueprint, true);
		if (s != null)
			return s;
		s = group.blueprint.placable(tx, ty, it, it == null ? null : it.get(rx, ry));
		if (s != null)
			return s;
		
		if (it.get(rx, ry).mustBeReachable) {
			if (SETT.PLACA().willBeBlocked(tx, ty, rx, ry, it))
				return PlacableMessages.¤¤BLOCKED_WILL;
		}
		
		if (it.get(rx, ry).isBlocker()) {
			if (SETT.PLACA().willBlock.is(tx, ty))
				return PlacableMessages.¤¤BLOCK_WILL;
		}
		
		
		
		
		return it.get(rx, ry).isPlacable(tx, ty, embryo.instance, it, rx, ry);
	}
	
	@Override
	public void placeInfo(GBox box, int x1, int y1) {
		box.add(box.text().add(width()).add('x').add(height()));
		box.NL();
		for (int i = 0; i < group.blueprint.resources(); i++) {
			if (group.item(size(), rot()).cost2(i, upgrade) > 0) {
				box.setResource(group.blueprint.resource(i), Math.ceil(group.item(size(), rot()).cost2(i, upgrade)));
				box.space();
			}
		}
		

		if (blueprint.constructor().mustBeIndoors() && ROOMS().placement.placer.autoWalls.is() && embryo.structure.get() != null) {
			FurnisherItem it = group.item(size(), rot());
			int roofs = 0;
			int walls = 0;
			for (int y = -1; y <= it.height(); y++) {
				for (int x = -1; x <= it.width(); x++) {
					
					if (it.get(x, y) != null) {
						roofs++;
					}else if (UtilWallPlacability.wallCanBe.is(x1+x, y1+y)) {
						boolean roof = false;
						for (DIR d : DIR.ORTHO) {
							roof |= it.get(x, y, d) != null && it.get(x, y, d).mustBeReachable;
						}
						
						for (DIR d : DIR.ALL) {
							if (it.get(x, y, d) != null) {
								if (roof)
									roofs++;
								else
									walls++;
								break;
							}
						}
					}
				}
			}
			
			int am = roofs*SETT.JOBS().build_structure.get(embryo.structure.get().structure.index()).ceiling.resAmount();
			am += walls*SETT.JOBS().build_structure.get(embryo.structure.get().structure.index()).wall.resAmount();
			box.setResource(embryo.structure.get().structure.resource, am);
			box.space();
			
		}
		
		for (FurnisherStat s : group.blueprint.stats()) {
			double am = group.item(size(), rot()).stat(s);
			if (am != 0) {
				box.NL();
				box.add(box.text().lablify().add(s.name()));
				box.tab(7);
				box.add(s.format(box.text(), am));
			}
		}
		
		box.NL(8);
		group.blueprint.placeInfo(box, group.item(size(), rot()), x1, y1);
	}
	
	@Override
	public void hoverDesc(GBox box) {
		box.title(group.name);
		box.text(group.desc);
		box.NL();
		for (int i = 0; i < group.blueprint.resources(); i++) {
			if (group.item(0, 0).cost2(i, upgrade) > 0) {
				box.setResource(group.blueprint.resource(i), Math.ceil(group.item(0, 0).cost2(i, upgrade)));
				box.space();
			}
		}
		
		for (FurnisherStat s : group.blueprint.stats()) {
			if (group.item(0, 0).stat(s) > 0) {
				box.NL();
				box.add(box.text().lablify().add(s.name()));
				box.add(s.format(box.text(), group.item(0, 0).stat(s)));
			}
		}
		
	}

	@Override
	public PLACABLE getUndo() {
		return undo;
	}

	@Override
	public int rotations() {
		return group.rotations();
	}

	@Override
	public int sizes() {
		return group.size();
	}

	@Override
	public SPRITE getIcon() {
		// TODO Auto-generated method stub
		return null;
	}
	
	private final Area itemArea = new Area();
	AREA itemAreaCurrent = null;
	
	final class Area implements AREA {

		private final Rec area = new Rec();
		private int size = 0;
		private FurnisherItem item;
		
		AREA set(FurnisherItem item, int x1, int y1) {
			this.item = item;
			area.setDim(item.width(), item.height());
			area.moveX1Y1(x1, y1);
			size = area.width()*area.height();
			return this;
		}
		
		@Override
		public RECTANGLE body() {
			return area;
		}

		@Override
		public boolean is(int tile) {
		
			return false;
		}

		@Override
		public boolean is(int tx, int ty) {
			return area.holdsPoint(tx, ty) && item.get(tx-body().x1(), ty-body().y1()) != null;
		}

		@Override
		public int area() {
			return size;
		}
		
		
	}

}
