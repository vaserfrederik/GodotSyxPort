package settlement.room.food.fish;

import java.io.IOException;

import init.constant.C;
import init.settings.S;
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
import settlement.room.sprite.RoomSpriteImp;
import settlement.room.sprite.RoomSpriteXxX;
import snake2d.Renderer;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.ColorImp;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.map.MAP_BOOLEAN;
import snake2d.util.misc.CLAMP;
import util.colors.GCOLOR;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import util.text.D;
import view.tool.PlacableMessages;

final class Constructor extends Furnisher{

	final static int B_WORK = 1;
	final static int B_STORAGE = 2;
	static CharSequence ¤¤problem = "¤Must not be placed on water";
	static CharSequence ¤¤problem2 = "¤Shape must contain more water tiles in order to function";
	private static CharSequence ¤¤deep = "Deep Sea.";
	private static CharSequence ¤¤deepD = "Deep sea access. Increased worker capacity.";
	
	
	static {
		D.ts(Constructor.class);
	}
	
	final FurnisherStat workers = new FurnisherStat(this) {
		
		@Override
		public double get(AREA area, double fromItems) {
			double shallow = 0;
			double deep = 0;
			for (COORDINATE c : area.body()) {
				if (area.is(c) && SETT.TERRAIN().WATER.SHALLOW.is(c)) {
					deep+= SETT.TERRAIN().WATER.fishAmount.get(c);
					if (SETT.TERRAIN().WATER.SHALLOW.is(c))
						shallow++;
					
				}
				
				
			}
			
			deep /= SETT.TERRAIN().WATER.fishAmount.max();
			
			return deep + shallow/64.0; 
		}
		
		@Override
		public GText format(GText t, double value) {
			return GFORMAT.i(t, (int)value);
		}
	};
	final FurnisherStat storage = new FurnisherStat(this) {
		
		@Override
		public double get(AREA area, double fromItems) {
			return fromItems;
		}
		
		@Override
		public GText format(GText t, double value) {
			return GFORMAT.i(t, (int)(value*blue.job.storage.max()));
		}
	};
	
	final FurnisherStat production;
	
	
	final FurnisherStat efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers) {
		@Override
		public double get(AREA area, double[] fromItems) {

			double w = workers.get(area, fromItems);
			
			
			
			if (w <= 0)
				w = 1;
			double i = fromItems[index];
			
			
			return CLAMP.d(0.5 + mul*0.5*i/w, 0, 1);
		}
	};
	
	final FurnisherStat deepBoost = new FurnisherStat(this, ¤¤deep, ¤¤deepD, 0) {

		@Override
		public GText format(GText t, double value) {
			return GFORMAT.f0(t, value, 1);
		}

		@Override
		public double get(AREA area, double acc) {
			double deep = 0;
			for (COORDINATE c : area.body()) {
				if (area.is(c))
					deep+= SETT.TERRAIN().WATER.fishAmount.get(c);
			}
			return deep/SETT.TERRAIN().WATER.fishAmount.max(); 
		}
	};
	private final ROOM_FISHERY blue;
	final RoomSpriteCombo sEdge;
	final RoomSprite1x1 sMisc;
	
	
	protected Constructor(RoomInitData init, ROOM_FISHERY blue)
			throws IOException {
		super(init, 2, 5);
		this.blue = blue;
		
		production = new FurnisherStat.FurnisherStatProduction2(this, blue) {
			
			@Override
			protected double getBase(AREA area, double[] acc) {

				
				return efficiency.get(area, acc)*(int)workers.get(area, acc);
			}
		};
		
		
		Json sp = init.data().json("SPRITES");
		
		final RoomSpriteImp sStorageBottom = new RoomSprite1x1(sp, "STORAGE_BOTTOM_1X1") {
			
			final RoomSpriteImp sStorageTop =  new RoomSprite1x1(sp, "STORAGE_TOP_1X1");
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				Room ro = SETT.ROOMS().map.get(it.tile());
				if (ro != null && ro instanceof FishInstance) {
					((FishInstance)ro).blueprintI().job.storage.render(r, s, it.tx(), it.ty(), it.x(), it.y(), it.ran());
				}
				return false;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				sStorageTop.render(r, s, getData2(it), it, degrade, rotates);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sStorageTop.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		
		final RoomSprite1x1 miscTop = new RoomSprite1x1(sp, "MISC_TOP_1X1");
		
		final RoomSprite1x1 sCandle = new RoomSprite1x1(sp, "CANDLE_1X1") {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (!SETT.ROOMS().fData.candle.is(it.tile())) {
					miscTop.render(r, s, getData2(it), it, degrade, false);
				}
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return miscTop.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		
		
		
		final RoomSpriteImp auxEdge = new RoomSprite1xN(sp, "AUX_EDGE_1X1", false);
		final RoomSpriteImp auxMid = new RoomSprite1xN(sp, "AUX_MID_1X1", true);
		
		final RoomSpriteXxX auxBig = new RoomSpriteXxX(sp, "AUX_BIG_2X2", 2);
		
		final RoomSprite1x1 work = new RoomSprite1x1(sp, "WORKTABLE_1X1") {
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (blue.is(it.tile()) && Job.working(SETT.ROOMS().data.get(it.tile()))) {
					int dx = 0;
					int dy = 0;
					if (rotates) {
						dx += rot(data).x()*8;
						dy += rot(data).y()*8;
					}
					blue.productionData.outs().get(0).resource.renderLaying(r, it.x()+dx, it.y()+dy, it.ran(), 1);
				}
				
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.get(rx, ry) != null && item.get(rx-d.x()*2, ry+d.y()*2) == null;
			}
		};
		
		final RoomSprite1x1 misc = new RoomSprite1x1(sp, "MISC_1X1") {
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (blue.is(it.tile()) && Job.working(SETT.ROOMS().data.get(it.tile()))) {
					blue.productionData.outs().get(0).resource.renderLaying(r, it.x(), it.y(), it.ran(), 1);
				}
				
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.get(rx, ry) != null && item.get(rx-d.x()*2, ry+d.y()*2) == null;
			}
		};
		
		sEdge = new RoomSpriteCombo(sp, "EDGE_COMBO");
		sMisc = new RoomSprite1x1(sp, "MISC_GROUND_1X1");
		//final RoomSpriteRot.Random misc = new RoomSpriteRot.Random(SINGLETYPE.BARREL, SINGLETYPE.BUCKET, SINGLETYPE.CRATE);
		
		
		final FurnisherItemTile ss = new Aux(this, true, sStorageBottom, AVAILABILITY.ROOM_SOLID, false).setData(B_STORAGE);
		
		final FurnisherItemTile cc = new Aux(this, false, sCandle, AVAILABILITY.ROOM_SOLID, true);
		
		final FurnisherItemTile ms = new Aux(this, false, misc, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile m1 = new Aux(this, false, auxEdge, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile m2 = new Aux(this, false, auxMid, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile ml = new Aux(this, false, auxBig, AVAILABILITY.ROOM_SOLID, false);
		
		final FurnisherItemTile ww = new Aux(this, true, work, AVAILABILITY.ROOM_SOLID, false).setData(B_WORK);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss},
			{cc,ms},
		}, 6, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ms},
			{ss,ss,cc},
		}, 8, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ss,cc},
			{ss,ss,ss,ms},
		}, 10, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ss,ss,cc},
			{ss,ss,ss,ss,ms},
		}, 12, 8);
		

		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ss,ss,ss,cc},
			{ss,ss,ss,ss,ss,ms},
		}, 14, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ss,ss,ss,ss,cc},
			{ss,ss,ss,ss,ss,ss,ms},
		}, 16, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ss,ss,ss,ss,ss,cc},
			{ss,ss,ss,ss,ss,ss,ss,ms},
		}, 18, 14);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ss,ss,ss,ss,ss,ss,cc},
			{ss,ss,ss,ss,ss,ss,ss,ss,ms},
		}, 20, 16);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ss,ss,ss,ss,ss,ss,ss,cc},
			{ss,ss,ss,ss,ss,ss,ss,ss,ss,ms},
		}, 22, 18);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ss,ss,ss,ss,ss,ss,ss,ss,cc},
			{ss,ss,ss,ss,ss,ss,ss,ss,ss,ss,ms},
		}, 24, 20);
		
		flush(1, 1, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ms},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ms,cc},
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
			{ms,ms,ms},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ms,ww,cc,ms},
			{ms,m1,m1,ww},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,ml,ml,ms,cc},
			{ww,ml,ml,ww,ms},
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,ml,ml,ww,ms,cc},
			{ww,ml,ml,m1,m1,ms},
		}, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ww,ml,ml,ww,ms,cc,ww},
			{ww,ml,ml,m1,m2,m1,ms},
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
			if (SETT.TERRAIN().WATER.SHALLOW.is(tx, ty)) {
				return ¤¤problem;
			}
			return null;
		}
		
		
	}
	
	public final Addable FISH = new Addable(null, null, null, null, false, true) {
		
		@Override
		public boolean render(Renderer r, RenderIterator it) {
			
			if (SETT.TERRAIN().WATER.SHALLOW.is(it.tile()) && !SETT.ROOMS().map.is(it.tile()) && !SETT.ROOMS().placement.embryo.is(it.tile())) {
				
				double d = SETT.TERRAIN().WATER.fishAmount.get(it.tile())/(double)SETT.TERRAIN().WATER.fishAmount.max();
				if (d == 0)
					return false;
				ColorImp.TMP.interpolate(GCOLOR.MAP().OVERLAY_BAD, GCOLOR.MAP().OVERLAY_GOOD, d);
				ColorImp.TMP.bind();
				UI.icons().s.fish.renderScaled(r, it.x(), it.y(), C.SCALE);
				
				if (S.get().developer && SETT.TERRAIN().WATER.deepSeaFishSpot.is(it.tile())) {
					UI.icons().s.cancel.render(r, it.x(), it.y());
				}
				
			}
			return false;
		}
	};
	
	@Override
	public Addable overlay() {
		return FISH;
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
	public Room create(TmpArea area, RoomInit init) {
		return new FishInstance(blue, area, init);
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
				if (area.is(tx, ty, d) || SETT.TERRAIN().WATER.DEEP.is(tx, ty, d) || SETT.TERRAIN().WATER.BRIDGE.is(tx, ty, d)) {
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
	public CharSequence constructionProblem(AREA area) {
		if (workers.get(area, 0) < 1.0)
			return ¤¤problem2;
		return super.constructionProblem(area);
	}
	
	@Override
	public CharSequence placable(int tx, int ty, FurnisherItem item, FurnisherItemTile tile) {
		if (SETT.TERRAIN().WATER.DEEP.is(tx, ty))
			return PlacableMessages.¤¤TERRAIN_BLOCK;
		return super.placable(tx, ty, item, tile);
	}
	
	@Override
	public boolean removeTerrain(int tx, int ty) {
		return  !SETT.TERRAIN().WATER.SHALLOW.is(tx, ty) && !SETT.TERRAIN().NADA.is(tx, ty);
	}
	
	@Override
	public boolean canBeCopied() {
		return false;
	}


}
