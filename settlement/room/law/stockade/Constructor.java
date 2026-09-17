package settlement.room.law.stockade;

import java.io.IOException;

import init.constant.C;
import init.sprite.SPRITES;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.ROOMA;
import settlement.room.main.Room;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.placement.UtilWallPlacability;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.map.MAP_BOOLEAN;
import util.GUTIL;
import util.colors.GCOLOR;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import util.text.D;

final class Constructor extends Furnisher{

	private static CharSequence ¤¤Problem = "Must be facing the edge of the room.";
	private static CharSequence ¤¤Problem3 = "Will be blocked by walls.";
	static {
		D.ts(Constructor.class);
	}
	
	private ROOM_STOCKADE blue;
	final FurnisherStat workers = new FurnisherStat(this) {
		
		@Override
		public double get(AREA area, double fromItems) {
			return 0.1*prisoners.get(area, fromItems);
		}
		
		@Override
		public GText format(GText t, double value) {
			return GFORMAT.f(t, value, 1);
		}
	};
	
	final FurnisherStat prisoners = new FurnisherStat(this, 1) {
		
		@Override
		public double get(AREA area, double fromItems) {
			
			double f = 0;
			outer: for (COORDINATE c : area.body()) {
				if (!area.is(c))
					continue;
				for (DIR d : DIR.ALL) {
					if (!area.is(c, d)) {
						continue outer;
					}
				}
				f ++;
			}
			return f*ROOM_STOCKADE.PRISONER_PER_TILE;
		}
		
		@Override
		public GText format(GText t, double value) {
			return GFORMAT.f(t, value, 1);
		}
	};
	
	private final FurnisherItemTile oo;
	
	private final RoomSprite fence;
	private final RoomSprite fenceDia;
	
	private final RoomSprite sFood;
	private final RoomSprite sShit;
	private final RoomSprite sStand;
	
	
	protected Constructor(ROOM_STOCKADE blue, RoomInitData init)
			throws IOException {
		super(init, 1, 2);
		this.blue = blue;
		
		Json js = init.data().json("SPRITES");
		
		fence = new RoomSprite1x1(js, "WALL_1X1");
		fenceDia = new RoomSprite1x1(js, "WALL_CORNER_1X1"); 
		
		RoomSprite sOpening = new RoomSprite1x1(js, "OPENING_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				tx -= d.x();
				ty -= d.y();
				ROOMA aa = SETT.ROOMS().map.rooma.get(tx, ty);
				return aa != null && !aa.is(tx, ty, d);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				DIR d = rot(data);
				d = d.perpendicular();
				it.setOff(C.SCALE*3*d.x(), C.SCALE*3*d.y());
				return super.render(r, s, data, it, degrade, isCandle);
			}
			
		};

		oo = new FurnisherItemTile(this, sOpening, AVAILABILITY.ROOM, false) {
			
			@Override
			public CharSequence isPlacable(int tx, int ty, MAP_BOOLEAN roomIs, FurnisherItem it, int rx, int ry) {
				if (SETT.ROOMS().placement.embryo.is(tx, ty)) {
					if (SETT.ROOMS().placement.placer.autoWalls.is()) {
						for (DIR d : DIR.ORTHO) {
							if (it.get(rx, ry, d) == null && UtilWallPlacability.wallCanBe.is(tx, ty, d) && SETT.ROOMS().placement.placer.placerDoor.isPlacable(tx+d.x(), ty+d.y(), null, null) == null) {
								return ¤¤Problem3;
							}
						}
					}
					
				}
				
				for (int di = 0; di < DIR.ORTHO.size(); di++) {
					DIR dd = DIR.ORTHO.get(di);
					if (!roomIs.is(tx, ty, dd) && !SETT.PATH().solidity.is(tx, ty, dd)) {
						if (roomIs.is(tx, ty, dd.next(2)) && !roomIs.is(tx, ty, dd.next(1))){
							if (roomIs.is(tx, ty, dd.next(-2)) && !roomIs.is(tx, ty, dd.next(-1))){
								return null;
							}
						}
					}
				}
				
				return ¤¤Problem;
			};
		};
		
		sFood = new RoomSprite1x1(js, "FOOD_1X1") {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				if (blue.is(it.tile()) && blue.job.food(it.tx(), it.ty()) > 0)
					return super.render(r, s, data, it, degrade, isCandle);
				return false;
			}
			
		};
		
		sShit = new RoomSprite1x1(js, "LATRINE_EMPTY_1X1") {
			
			final RoomSprite1x1 full = new RoomSprite1x1(js, "LATRINE_FULL_1X1");
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				if (blue.is(it.tile()) && blue.job.shit(it.tx(), it.ty()) > 0)
					return full.render(r, s, data, it, degrade, isCandle);
				return super.render(r, s, data, it, degrade, isCandle);
			}
			
		};
		
		sStand = new RoomSprite1x1(js, "MISC_1X1");
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{oo,oo,oo},
		}, 1, 1);

		flush(1, 3);
		
	}


	@Override
	public boolean usesArea() {
		return true;
	}

	@Override
	public boolean mustBeIndoors() {
		return false;
	}
	
	@Override
	public boolean mustBeOutdoors() {
		return false;
	}

	@Override
	public ROOM_STOCKADE blue() {
		return blue;
	}

	@Override
	public void renderEmbryo(SPRITE_RENDERER r, int mask, RenderIterator it, boolean isFloored, AREA area, boolean active) {
		
		if (active) {
			GCOLOR.MAP().BETTER.bind();
		}
		
		Room room = SETT.ROOMS().map.get(it.tile());
		
		if (isFloored) {
			COLOR.unbind();
			renderFence(r, ShadowBatch.DUMMY, it, 1.0, false);
			return;
		}
		
		if (mask != 0x0F) {
			SPRITES.cons().BIG.filled.render(r, mask, it.x(), it.y());
			return;
		}
		for (DIR d : DIR.NORTHO) {
			if (!room.isSame(it.tx(), it.ty(), it.tx()+d.x(), it.ty()+d.y())) {
				SPRITES.cons().BIG.filled.render(r, 0x0F, it.x(), it.y());
				return;
			}
		}
		GCOLOR.MAP().BETTER.bind();
		super.renderEmbryo(r, mask, it, isFloored, area, active);
	}
	
	@Override
	public boolean removeFertility() {
		return true;
	}
	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		GUTIL.coos().set(0);
		for (COORDINATE c : area.body()) {
			if (area.is(c) && !isFence(area, c.x(), c.y()) && SETT.ROOMS().fData.item.get(c) == null) {
				GUTIL.coos().get().set(c);
				GUTIL.coos().inc();
			}
		};
		
		int am = GUTIL.coos().getI();
		am = (int) Math.ceil(am/5.0);
		GUTIL.coos().shuffle(GUTIL.coos().getI());
		
		GUTIL.coos().set(0);
		
		{
			int a = (int) Math.ceil(am*0.25);
			for (int i = 0; i < a; i++) {
				SETT.ROOMS().data.set(area, GUTIL.coos().get(), Job.IFOOD);
				GUTIL.coos().inc();
			}
		}
		{
			int a = (int) Math.ceil(am*0.25);
			for (int i = 0; i < a; i++) {
				SETT.ROOMS().data.set(area, GUTIL.coos().get(), Job.ISHIT);
				GUTIL.coos().inc();
			}
		}
		{
			int a = (int) Math.ceil(am*0.5);
			for (int i = 0; i < a; i++) {
				SETT.ROOMS().data.set(area, GUTIL.coos().get(), Job.ISTAND);
				GUTIL.coos().inc();
			}
		}
		return new StockInstance(blue, area, init);
	}
	
	private static CharSequence ¤¤TooThin = "¤Area is too thin at places. Expand the area to at least 3x3.";
	
	static {
		D.ts(Constructor.class);
	}
	
	@Override
	public CharSequence constructionProblem(AREA area) {
		for (COORDINATE c : area.body()){
			if (area.is(c)) {
				boolean ok = false;
				for (DIR d : DIR.ALL) {
					if (isFull(c.x(), c.y(), area, d)) {
						ok = true;
						break;
					}	
				}
				if (!ok) {
					GUTIL.filler().done();
					return ¤¤TooThin;
				}
				
				
			}
			
		}
		return null;
		
	}
	
	private boolean isFull(int x, int y, AREA a, DIR d) {
		int tx = x+d.x();
		int ty = y+d.y();
		if (!a.is(tx, ty))
			return false;
		for (int i = 0; i < DIR.ALL.size(); i++) {
			DIR dd = DIR.ALL.get(i);
			if (!a.is(tx, ty, dd))
				return false;
		}
		return true;
	}
	
	@Override
	public void renderTileBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, boolean floored) {
		if (floored)
			renderFence(r, s, it, 0, false);
	}
	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
//		if (SETT.ROOMS().fData.item.get(tx, ty) != null)
//			super.putFloor(tx, ty, upgrade, area);
		super.putFloor(tx, ty, upgrade, area);
	}
	
	public boolean isFence(ROOMA ii, int tx, int ty) {
		if (!ii.is(tx, ty))
			return false;
		if (SETT.ROOMS().fData.tile.get(tx, ty) != null)
			return false;
		for (int di = 0; di < DIR.ALL.size(); di++) {
			if (!ii.is(tx, ty, DIR.ALL.get(di)))
				return true;
		}
		return false;
	}
	
	public void renderFence(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, double degrade, boolean exists) {
		
		
		if (exists) {
			int i = blue.job.type(it.tx(), it.ty());
			if (i == Job.IFOOD) {
				sFood.render(r, s, it.ran(), it, degrade, false);
				return;
			}
			else if (i == Job.ISHIT) {
				sShit.render(r, s, it.ran(), it, degrade, false);
				return;
			}
			else if (i == Job.ISTAND) {
				sStand.render(r, s, it.ran(), it, degrade, false);
				return;
			}

		}
		
		ROOMA ii = SETT.ROOMS().map.rooma.get(it.tx(), it.ty());
		if (ii == null)
			return;
		if (!isFence(ii, it.tx(), it.ty()))
			return;
		for (int di = 0; di < DIR.ORTHO.size(); di++) {
			if (!ii.is(it.tx(), it.ty(), DIR.ORTHO.get(di)))
				fence.render(r, s, di, it, degrade, false);
		}

		
		for (int di = 0; di < DIR.NORTHO.size(); di++) {
			if (!ii.is(it.tx(), it.ty(), DIR.NORTHO.get(di))) {
				if (ii.is(it.tx(), it.ty(), DIR.NORTHO.get(di).next(-1)) == ii.is(it.tx(), it.ty(), DIR.NORTHO.get(di).next(1))) {
					fenceDia.render(r, s, di, it, degrade, false);
				}else if (SETT.ROOMS().fData.tile.get(it.tx(), it.ty(), DIR.NORTHO.get(di).next(1)) != null || SETT.ROOMS().fData.tile.get(it.tx(), it.ty(), DIR.NORTHO.get(di).next(-1)) != null) {
					fenceDia.render(r, s, di, it, degrade, false);
				}
			}
		}
		

	}

}
