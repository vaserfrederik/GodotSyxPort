package settlement.room.food.pasture;

import static settlement.main.SETT.FLOOR;
import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TERRAIN;

import java.io.IOException;

import init.constant.C;
import init.sprite.UI.UI;
import settlement.main.SETT;
import settlement.overlay.Addable;
import settlement.path.AVAILABILITY;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.job.RoomResStorage;
import settlement.room.main.placement.UtilWallPlacability;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSpriteXxX;
import settlement.tilemap.terrain.TerrainDiagonal.Diagonalizer;
import snake2d.Renderer;
import snake2d.util.color.ColorImp;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.map.MAP_BOOLEAN;
import snake2d.util.misc.CLAMP;
import util.colors.GCOLOR;
import util.rendering.RenderData.RenderIterator;
import util.text.D;

final class ConstructorOutdoor extends Constructor{

	private static CharSequence ¤¤Problem = "Must be facing the edge of the room.";
	private static CharSequence ¤¤Problem2 = "Must not be placed in a corner.";
	private static CharSequence ¤¤Problem3 = "Will be blocked by walls";
	static {
		D.ts(ConstructorOutdoor.class);
	}
	
	private ROOM_PASTURE blue;
	

	private final FurnisherItemTile gc;
	final FurnisherItemTile s1;
	final FurnisherItemTile s2;
	final FurnisherItemTile s3;
	
	protected ConstructorOutdoor(ROOM_PASTURE blue, RoomInitData init)
			throws IOException {
		super(blue, init);
		this.blue = blue;

		Json js = init.data().json("SPRITES");
		
		RoomSprite sbelow = new RoomSprite1x1(js, "STORAGE_1X1");
		
		RoomSprite sprite = new RoomSpriteXxX(js, "GATE_TOP_3X3", 3) {
			@Override
			public void renderAbove(snake2d.SPRITE_RENDERER r, util.rendering.ShadowBatch s, int data, util.rendering.RenderData.RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			};
			@Override
			public boolean render(snake2d.SPRITE_RENDERER r, util.rendering.ShadowBatch s, int data, util.rendering.RenderData.RenderIterator it, double degrade, boolean isCandle) {
				return false;
				
			};
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sbelow.getData(tx, ty, rx, ry, item, itemRan);
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteXxX;
			}
		};
		

		
		FurnisherItemTile gl = new FurnisherItemTile(this, sprite, AVAILABILITY.ROOM_SOLID, false).setData(5);
		gc = new FurnisherItemTile(this, sprite, AVAILABILITY.ROOM, false) {
			
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
				
				for (DIR d : DIR.ORTHO) {
					if (!roomIs.is(tx, ty, d))
						return null;
					
				}
				return ¤¤Problem;
				
//				for (DIR d : DIR.ORTHO) {
//					if (!roomIs.is(tx, ty, d) && !SETT.PATH().solidity.is(tx, ty, d) && SETT.ROOMS().placement.placer.placerDoor.isPlacable(tx+d.x(), ty+d.y(), null, null) == null)
//						return null;
//					
//				}
//				return ¤¤Problem;
			};
		}.setData(5);
		FurnisherItemTile du = new FurnisherItemTile(this, sprite, AVAILABILITY.ROOM, false) {
			@Override
			public CharSequence isPlacable(int tx, int ty, MAP_BOOLEAN roomIs, FurnisherItem it, int rx, int ry) {
				
				
				for (DIR d : DIR.ORTHO) {
					if (it.get(rx, ry, d) != null) {
						continue;
					}
					if (!p(tx+d.x(), ty+d.y(), roomIs))
						return ¤¤Problem2;
					
				}
				
				
				
				return super.isPlacable(tx, ty, roomIs, it, rx, ry);
				
			};
			
			private boolean p(int tx, int ty, MAP_BOOLEAN roomIs) {
				for (int i = 0; i < DIR.ORTHO.size(); i++) {
					if (!roomIs.is(tx, ty, DIR.ORTHO.get(i)))
						return false;
				}
				return true;
			}
		};
		s1 = new FurnisherItemTile(this, new SpriteDep(js, sbelow, blue.s1), AVAILABILITY.ROOM_SOLID, false).setData(1);
		s1.setData(STORAGE1);
		s2 = new FurnisherItemTile(this, new SpriteDep(js, sbelow, blue.s2), AVAILABILITY.ROOM_SOLID, false).setData(1);
		s2.setData(STORAGE2);
		s3 = new FurnisherItemTile(this, new SpriteDep(js, sbelow, blue.s3), AVAILABILITY.ROOM_SOLID, false).setData(1);
		s3.setData(STORAGE3);
		
		
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{gl,gc,gl},
			{du,du,du},
			{s1,s2,s3},
		}, 1, 1);
		
		flush(1, 1, 3);
		
		makeAux(js);
		
	}

	@Override
	public boolean mustBeIndoors() {
		return false;
	}
	
	@Override
	public boolean mustBeOutdoors() {
		return true;
	}

	private final Addable overlay = new Addable(true, true) {
		@Override
		public void renderBelow(Renderer r, RenderIterator it) {
			double d = fertility(it.tx(), it.ty());
			d*=d;
			renderUnder(d, r, it, false);
			if (!SETT.ROOMS().placement.embryo.is(it.tile()) && TERRAIN().get(it.tile()).clearing().can()) {
				double w = SETT.GROUND().MOISTURE_TOT.get(it.tile())*2.0;
				w = CLAMP.d(w, 0, 1);
				if (w > 0) {
					ColorImp.TMP.interpolate(GCOLOR.MAP().OVERLAY_BAD, GCOLOR.MAP().OVERLAY_GOOD, w).bind();;
					int s = (int) (C.TILE_SIZE/4 + w*3*C.TILE_SIZE/4);
					int x1 = it.x() + (C.TILE_SIZE-s)/2;
					int y1 = it.y() + (C.TILE_SIZE-s)/2;
					
					UI.icons().s.drop.render(r, x1, x1 +s, y1, y1+s);
				}
			}
		};
		
		private double fertility(int tx, int ty) {
			if (mustBeIndoors()) {
				if (SETT.TERRAIN().MOUNTAIN.isMountain(tx, ty))
					return 1.0;
				return 0.5;
			}
			double f = (SETT.GROUND().MAP.get(tx, ty).farm-0.1);
			f *= f;
			return 0.4 + 0.6*f;
		}
		

	};
	
	@Override
	public Addable overlay() {
		return overlay;
	}
	
	@Override
	public boolean needsIsolation() {
		return false;
	}
	

	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		FurnisherItemTile t = SETT.ROOMS().fData.tile.get(tx, ty);
		if (t != null) {
			super.putFloor(tx, ty, upgrade, area);
		}else {
			FLOOR().clearer.clear(tx, ty);
		}
		
	}
	
	@Override
	public boolean removeFertility() {
		return false;
	}


	@Override
	protected boolean fenceJoin(FurnisherItemTile gc) {
		return gc != this.gc;
	}

	private final Diagonalizer dia = new Diagonalizer() {
		
		@Override
		public void setDia(int tx, int ty, boolean dia) {
			SETT.ROOMS().fData.spriteData.set(tx, ty, dia ? 1 :0);
		}
		
		@Override
		public boolean getDia(int tx, int ty) {
			return SETT.ROOMS().fData.spriteData.get(tx, ty) == 1;
		}
	};
	
	@Override
	public Diagonalizer dia(int tx, int ty) {
		if (blue.is(tx, ty) && fenceJoin(blue.get(tx, ty), tx, ty))
			return dia;
		return null;
	}
	
	@Override
	public boolean growsGrass(int tx, int ty) {
		return ROOMS().fData.item.get(tx, ty) == null;
	}
	
	private static class SpriteDep extends RoomSpriteXxX{

		private final RoomSprite below;
		private final RoomResStorage st;
		
		public SpriteDep(Json json, RoomSprite below, RoomResStorage st) throws IOException {
			super(json, "GATE_TOP_3X3", 3);
			this.below = below;
			this.st = st;
		}
		
		@Override
		public void renderAbove(snake2d.SPRITE_RENDERER r, util.rendering.ShadowBatch s, int data, util.rendering.RenderData.RenderIterator it, double degrade) {
			super.render(r, s, data, it, degrade, false);
		};
		@Override
		public boolean render(snake2d.SPRITE_RENDERER r, util.rendering.ShadowBatch s, int data, util.rendering.RenderData.RenderIterator it, double degrade, boolean isCandle) {
			
			boolean ret = below.render(r, s, getData2(it), it, degrade, isCandle);
			st.render(r, s, it.tx(), it.ty(), it.x(), it.y(), it.ran());
			
			return ret;
			
		};
		
		@Override
		public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			return below.getData(tx, ty, rx, ry, item, itemRan);
		}
		
		@Override
		protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
			return item.sprite(rx, ry) instanceof RoomSpriteXxX;
		}
		
	}
}
