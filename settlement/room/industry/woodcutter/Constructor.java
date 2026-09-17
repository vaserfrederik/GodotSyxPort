package settlement.room.industry.woodcutter;

import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TERRAIN;

import java.io.IOException;

import init.constant.C;
import init.sprite.UI.UI;
import settlement.main.SETT;
import settlement.overlay.Addable;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSprite1xN;
import settlement.room.sprite.RoomSpriteCombo;
import settlement.room.sprite.RoomSpriteXxX;
import snake2d.Renderer;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.ColorImp;
import snake2d.util.color.OPACITY;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.map.MAP_BOOLEAN;
import snake2d.util.misc.CLAMP;
import snake2d.util.sprite.SPRITE;
import util.colors.GCOLOR;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	final static int B_WORK = 1;
	final static int B_STORAGE = 2;
	
	
	final FurnisherStat workers;
	final FurnisherStat output;
	final FurnisherStat irri;
	final RoomSpriteCombo sedge;
	
	final FurnisherStat efficiency;
	private final ROOM_WOODCUTTER blue;

	protected Constructor(RoomInitData init, ROOM_WOODCUTTER blue)
			throws IOException {
		super(init, 2, 4, 88, 44);
		this.blue = blue;
		
		Json sp = init.data().json("SPRITES");
		
		
		
		workers = new FurnisherStat(this, 1) {
			
			@Override
			public double get(AREA area, double fromItems) {
				double am = 0;
				for (COORDINATE c : area.body()) {
					if (area.is(c))
						am += fertility(c.x(), c.y());
				}
				return Math.floor(am/25.0); 
			}
			
			@Override
			public GText format(GText t, double value) {
				return GFORMAT.i(t, (int)value);
			}
		};
		efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers);
		
		irri = new FurnisherStat.FurnisherStatIrrigation(this, blue);
		
		output = new FurnisherStat.FurnisherStatProduction2(this, blue) {
			@Override
			protected double getBase(AREA area, double[] fromItems) {
				
				return workers.get(area, fromItems)*efficiency.get(area, fromItems);
			}
		};
		
		sedge = new RoomSpriteCombo(sp, "EDGE_COMBO");
		
		final RoomSprite sStorageTop = new RoomSprite1xN(sp, "STORAGE_1X1_TOP", true) {
			
			@Override
			public void renderAbove(SPRITE_RENDERER ren, ShadowBatch shadowBatch, int data, RenderIterator it, double degrade) {
				Room r = SETT.ROOMS().map.get(it.tile());
				if (r != null && r instanceof Instance) {
					((Instance)r).blueprintI().job.storage.render(ren, shadowBatch, it.tx(), it.ty(), it.x(), it.y(), it.ran());
				}
			};
			
		};
		final RoomSprite sStorageMid = new RoomSprite1xN(sp, "STORAGE_1X1_MID", false) {
			
			@Override
			public void renderAbove(SPRITE_RENDERER ren, ShadowBatch shadowBatch, int data, RenderIterator it, double degrade) {
				Room r = SETT.ROOMS().map.get(it.tile());
				if (r != null && r instanceof Instance) {
					((Instance)r).blueprintI().job.storage.render(ren, shadowBatch, it.tx(), it.ty(), it.x(), it.y(), it.ran());
				}
			};
			
		};
		final RoomSprite sStorageEnd = new RoomSprite1xN(sp, "STORAGE_1X1_END", false) {
			
			@Override
			public void renderAbove(SPRITE_RENDERER ren, ShadowBatch shadowBatch, int data, RenderIterator it, double degrade) {
				Room r = SETT.ROOMS().map.get(it.tile());
				if (r != null && r instanceof Instance) {
					((Instance)r).blueprintI().job.storage.render(ren, shadowBatch, it.tx(), it.ty(), it.x(), it.y(), it.ran());
				}
			};
			
		};
		
		final RoomSprite sStorageSingle = new RoomSprite1x1(sp, "SLAB_1X1") {
			
			@Override
			public void renderAbove(SPRITE_RENDERER ren, ShadowBatch shadowBatch, int data, RenderIterator it, double degrade) {
				Room r = SETT.ROOMS().map.get(it.tile());
				if (r != null && r instanceof Instance) {
					((Instance)r).blueprintI().job.storage.render(ren, shadowBatch, it.tx(), it.ty(), it.x(), it.y(), it.ran());
				}
			};
			
		};
		
		final RoomSprite sSlab = new RoomSprite1x1(sStorageSingle) {
			
			
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				if (!isCandle) {
					SPRITE i = blue.indus.get(0).outs().get(0).resource.icon();
					OPACITY.O99.bind();
					i.render(r, it.x()+8, it.x()+C.TILE_SIZE-8, it.y()+8, it.y()+C.TILE_SIZE-8);
					OPACITY.unbind();
				}
				return false;
			}
			
		};
		
		final RoomSprite sAuxSmall = new RoomSprite1x1(sp, "AUX_1X1") {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				animationSpeed = aniSpeed(it.tile());
				return super.render(r, s, data, it, degrade, isCandle);
			}
		};
		
		final RoomSprite sAuxMediumA = new RoomSprite1xN(sp, "AUX_MEDIUM_A_1X1", true) {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				animationSpeed = aniSpeed(it.tile());
				return super.render(r, s, data, it, degrade, isCandle);
			}
		};
		
		final RoomSprite sAuxMediumB = new RoomSprite1xN(sp, "AUX_MEDIUM_B_1X1", false) {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				animationSpeed = aniSpeed(it.tile());
				return super.render(r, s, data, it, degrade, isCandle);
			}
		};
		
		final RoomSprite sAuxBig = new RoomSpriteXxX(sp, "AUX_BIG_2X2", 2) {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				animationSpeed = aniSpeed(it.tile());
				return super.render(r, s, data, it, degrade, isCandle);
			}
		};
		
		final RoomSprite sWork = new RoomSprite1x1(sp, "WORK_1X1") {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				animationSpeed = 0;
				Room ro = SETT.ROOMS().map.get(it.tile());
				
				if (r != null && ro instanceof Instance && Job.working(SETT.ROOMS().data.get(it.tile()))) {
					animationSpeed = 1.0;
				}
				return super.render(r, s, data, it, degrade, isCandle);
			}
		};
		
		
		
		final FurnisherItemTile ss = new Aux(this, true, sStorageSingle, AVAILABILITY.ROOM_SOLID, false).setData(B_STORAGE);
		final FurnisherItemTile sA = new Aux(this, true, sStorageTop,  AVAILABILITY.ROOM_SOLID, false).setData(B_STORAGE);
		final FurnisherItemTile sm = new Aux(this, true, sStorageMid, AVAILABILITY.ROOM_SOLID, false).setData(B_STORAGE);
		final FurnisherItemTile sc = new Aux(this, true, sStorageEnd,  AVAILABILITY.ROOM_SOLID, false).setData(B_STORAGE);
		
		final FurnisherItemTile cc = new Aux(this, false, sSlab, AVAILABILITY.ROOM_SOLID, true);
		
		final FurnisherItemTile ms = new Aux(this, false, sAuxSmall, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile mA = new Aux(this, false, sAuxMediumA, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile mB = new Aux(this, false, sAuxMediumB, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile ml = new Aux(this, false, sAuxBig, AVAILABILITY.ROOM_SOLID, false);
		
		final FurnisherItemTile ww = new Aux(this, true, sWork, AVAILABILITY.ROOM_SOLID, false).setData(B_WORK);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,cc},
			{ss,cc},
		}, 6, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sA,sc,cc},
			{sA,sc,cc},
		}, 8, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sA,sm,sc,cc},
			{sA,sm,sc,cc},
		}, 10, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sA,sm,sm,sc,cc},
			{sA,sm,sm,sc,cc},
		}, 12, 8);
		

		new FurnisherItem(new FurnisherItemTile[][] {
			{sA,sm,sm,sm,sc,cc},
			{sA,sm,sm,sm,sc,cc},
		}, 14, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sA,sm,sm,sm,sm,sc,cc},
			{sA,sm,sm,sm,sm,sc,cc},
		}, 16, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sA,sm,sm,sm,sm,sm,sc,cc},
			{sA,sm,sm,sm,sm,sm,sc,cc},
		}, 18, 14);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sA,sm,sm,sm,sm,sm,sm,sc,cc},
			{sA,sm,sm,sm,sm,sm,sm,sc,cc},
		}, 20, 16);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sA,sm,sm,sm,sm,sm,sm,sm,sc,cc},
			{sA,sm,sm,sm,sm,sm,sm,sm,sc,cc},
		}, 22, 18);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sA,sm,sm,sm,sm,sm,sm,sm,sm,sc,cc},
			{sA,sm,sm,sm,sm,sm,sm,sm,sm,sc,cc},
		}, 24, 20);
		
		flush(1, 1, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ms},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ms,ww},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ms,ww,cc},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ms,ww,cc,ms},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ms,ww},
			{cc,ms},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ms,ww,cc},
			{ww,ms,ms},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ms,ww,cc,ms},
			{ms,mA,mB,ww},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,ml,ml,ww,cc},
			{ww,ml,ml,ww,ms},
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,ml,ml,mA,ww,cc},
			{ww,ml,ml,mB,ww,ms},
		}, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,ml,ml,mA,ww,cc,mA},
			{ww,ml,ml,mB,ww,ms,mB},
		}, 14);
		
		flush(3);
		
		
		
	}
	
	private class Aux extends FurnisherItemTile {

		public Aux(Furnisher p, boolean mustBeReachable, RoomSprite sprite, AVAILABILITY availability,
				boolean canGoCandle) {
			super(p, mustBeReachable, sprite, availability, canGoCandle);
		}
		
		@Override
		public CharSequence isPlacable(int tx, int ty, MAP_BOOLEAN roomIs, FurnisherItem it, int rx, int ry) {
			return null;
		}
		
		
	}
	
	@Override
	public CharSequence constructionProblem(AREA area) {
		return null;
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
		return true;
	}

	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new Instance(blue, area, init);
		
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	
//	private final FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][] {
//		{0,0,0,0,0,0,0,0},
//		{0,1,0,0,1,0,0,0},
//		{0,1,1,0,1,1,0,0},
//		{0,1,1,1,1,1,1,1},
//		{0,1,1,1,1,1,1,1},
//		{0,1,1,0,1,0,0,0},
//		{0,1,0,0,0,0,0,0},
//		{0,0,0,0,0,0,0,0},
//		},
//		miniColor
//	);
//	
//	@Override
//	public COLOR miniColor(int tx, int ty) {
//		return miniC.get(tx, ty);
//	}
	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		
		if (SETT.ROOMS().fData.item.get(tx, ty) == null) {
			int m = 0;
			for (DIR d : DIR.ORTHO) {
				if (area.is(tx, ty, d)) {
					m |= d.mask();
				}
			}
			SETT.ROOMS().fData.spriteData.set(tx, ty, m);
		}
		SETT.FLOOR().clearer.clear(tx, ty);
	}
	
	@Override
	public boolean removeFertility() {
		return false;
	}
	
	@Override
	public boolean removeTerrain(int tx, int ty) {
		if (SETT.FLOOR().getter.get(tx, ty) != null)
			return true;
		if (SETT.ROOMS().fData.item.get(tx, ty) != null)
			return true;
		if (SETT.TERRAIN().TREES.isTree(tx, ty))
			return false;
		return super.removeTerrain(tx, ty);
	}
	
	@Override
	public boolean canBeCopied() {
		return true;
	}
	
	private double aniSpeed(int tile) {
		Room r = SETT.ROOMS().map.get(tile);
		if (r != null && r instanceof Instance) {
			Instance ins = (Instance)r;
			return (double)ins.workage / ins.employees().max();

		}
		return 0;
	}

	
	private double fertility(int tx, int ty) {
		if (SETT.GROUND().types.FOREST.is(tx, ty))
			return 1.0;
		else
			return SETT.GROUND().MAP.get(tx, ty).vegitation*0.25;
	}
	
	private final Addable overlay = new Addable(true, true) {
		@Override
		public void renderBelow(Renderer r, RenderIterator it) {
			renderUnder(fertility(it.tx(),it.ty()), r, it, false);
			if (!SETT.ROOMS().placement.embryo.is(it.tile()) && TERRAIN().get(it.tile()).clearing().can()) {
				double w = SETT.GROUND().MOISTURE_TOT.get(it.tile())*2.0;
				w = CLAMP.d(w, 0, 1);
				w*=w;
				if (w > 0) {
					ColorImp.TMP.interpolate(GCOLOR.MAP().OVERLAY_BAD, GCOLOR.MAP().OVERLAY_GOOD, w).bind();;
					int s = (int) (C.TILE_SIZE/4 + w*3*C.TILE_SIZE/4);
					int x1 = it.x() + (C.TILE_SIZE-s)/2;
					int y1 = it.y() + (C.TILE_SIZE-s)/2;
					
					UI.icons().s.drop.render(r, x1, x1 +s, y1, y1+s);
				}
			}
		};
		
//		@Override
//		public boolean render(Renderer r, RenderIterator it) {
//			if (!SETT.ROOMS().placement.embryo.is(it.tile()) && TERRAIN().get(it.tile()).clearing().can()) {
//				double w = SETT.GROUND().MOISTURE_TOT.get(it.tile())*2.0;
//				w = CLAMP.d(w, 0, 1);
//				w*=w;
//				if (w > 0) {
//					ColorImp.TMP.interpolate(GCOLOR.MAP().OVERLAY_BAD, GCOLOR.MAP().OVERLAY_GOOD, w).bind();;
//					int s = (int) (C.TILE_SIZE/4 + w*3*C.TILE_SIZE/4);
//					int x1 = it.x() + (C.TILE_SIZE-s)/2;
//					int y1 = it.y() + (C.TILE_SIZE-s)/2;
//					
//					UI.icons().s.drop.render(r, x1, x1 +s, y1, y1+s);
//					return true;
//				}
//			}
//			return false;
//		};
	};
	
	@Override
	public Addable overlay() {
		return overlay;
	}
	
	@Override
	public boolean growsGrass(int tx, int ty) {
		return ROOMS().fData.item.get(tx, ty) == null;
	}
}
