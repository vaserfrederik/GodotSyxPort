package settlement.room.food.orchard;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import init.constant.C;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.sprite.UI.UI;
import settlement.main.SETT;
import settlement.overlay.Addable;
import settlement.path.AVAILABILITY;
import settlement.room.food.orchard.OTile.STATE;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.job.RoomResStorage;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSpriteCombo;
import settlement.room.sprite.RoomSpriteXxX;
import snake2d.Renderer;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.ColorImp;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.misc.CLAMP;
import util.colors.GCOLOR;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import util.text.D;

final class Constructor extends Furnisher{

	private static CharSequence ¤¤warning = "Fertility for this orchard is very low, which will result in low yields. This can be improved by irrigation. Proceed anyway?";
	static {
		D.ts(Constructor.class);
	}
	
	final static int TREE = 1;
	final static int STORAGE = 2;
	
	final boolean isIndoors;
	private final ROOM_ORCHARD blue;
	final FurnisherStat fertility = new FurnisherStat(this, 1/10000.0) {

		@Override
		public GText format(GText t, double value) {
			return GFORMAT.perc(t, value);
		}

		@Override
		public double get(AREA area, double fromItems) {
			if (mustBeIndoors())
				return 1;
			double v = 0;
			for (COORDINATE c : area.body()) {
				
				if (area.is(c)) {
					v += fertility(c.x(), c.y());
				}
			}
			return CLAMP.d(v/area.area(), 0, 1);
		}
		
	};
	final FurnisherStat workers = new FurnisherStat(this, 0.01) {

		@Override
		public GText format(GText t, double value) {
			return GFORMAT.f(t, value);
		}

		@Override
		public double get(AREA area, double fromItems) {
			return 4*fromItems/ROOM_ORCHARD.TILES_PER_WORKER;
		}
		
	};
	
	final RoomSpriteCombo sEdge;
	final FurnisherStat irri;
	final FurnisherStat output;
	
	
	
	final RoomResStorage storage = new RoomResStorage(3000) {
		@Override
		public RESOURCE resource() {
			if (ins instanceof Instance) {
				return ((Instance) ins).industry().outs().get(0).resource;
			}
			return RESOURCES.ALL().get(0);
		}
		
		@Override
		protected boolean is(int tx, int ty) {
			return SETT.ROOMS().fData.tileData.get(tx, ty) == STORAGE;
		}
	};
	
	protected Constructor(ROOM_ORCHARD blue, RoomInitData init)
			throws IOException {
		super(init, 2, 4);
		
		irri = new FurnisherStat.FurnisherStatIrrigation(this, blue);
		output = new FurnisherStat.FurnisherStatProduction2(this, blue, 0.01) {
			
			@Override
			protected double getBase(AREA area, double[] acc) {
				double f = area.area();
				if (!isIndoors) {
					f = 0;
					for (COORDINATE c : area.body()) {
						if (area.is(c)) {
							f += fertility(c.x(), c.y());
						}
					}
				}
				f /= area.area();
				f = CLAMP.d(f, 0, 1);
				return f*workers.get(area, acc[workers.index()]);
			}
			
		};
		
		
		
		isIndoors = init.data().bool("INDOORS");
		this.blue = blue;

		Json sp = init.data().json("SPRITES");
		
		RoomSprite1x1 sfruit = new RoomSprite1x1(sp, "FRUIT_1X1");
		RoomSprite1x1 ssmall = new RoomSprite1x1(sp, "TREE_1X1");
		
		RoomSpriteXxX tree = new RoomSpriteXxX(sp, "TREE_2X2", 2) {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				
				if (type().dx(data) == 0 || type().dy(data) == 0) {
					return false;
				}
				OTile t = blue.tile.get(it.tx(), it.ty());
				if (t != null) {
					STATE state =  t.state();
					if (state == t.ISAPLING) {
						SETT.TERRAIN().BUSH.render(it, r, s, it.x()-C.TILE_SIZEH, it.y()-C.TILE_SIZEH, it.ran());
					}else if (state == t.ISMALL) {
						it.setOff(-C.TILE_SIZEH, -C.TILE_SIZEH);
						ssmall.render(r, s, data, it, degrade, isCandle);
					}
				}else {
					SETT.TERRAIN().BUSH.render(it, r, s, it.x()-C.TILE_SIZEH, it.y()-C.TILE_SIZEH, it.ran());
				}
				
				return false;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {

				OTile t = blue.tile.get(it.tx(), it.ty());
				if (t == null)
					return;
				STATE state =  t.state();
				if (state != t.IBIG && state != t.IDEAD) {
					//t.renderDebug(r, it);
					return;
				}
				
				degrade = state.deadAmount();
				super.render(r, s, data, it, degrade, false);
				int dx = C.TILE_SIZEH/2 - C.TILE_SIZEH*type().dx(data) + it.oX();
				int dy = C.TILE_SIZEH/2 - C.TILE_SIZEH*type().dy(data) + it.oY();
				it.setOff(dx, dy);
				
				Instance ins = blue.getter.get(it.tile());
				double a = 4*state.fruitAmount()*fertility.get(ins)*ins.skill()*blue.time.fruit();
				int am = (int) a;
				
				for (int i = 0; i < am; i++) {
					it.ranOffset(i, 0);
					sfruit.render(r, s, data, it, degrade, false);
				}
				
				//t.renderDebug(r, it);
					
				
			}
		};
		
		sEdge = new RoomSpriteCombo(sp, "EDGE_COMBO");
		
		RoomSprite st = new RoomSprite1x1(sp, "STORAGE_1X1") {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				boolean ret = super.render(r, s, data, it, degrade, isCandle);
				storage.render(r, s, it.tx(), it.ty(), it.x(), it.y(), it.ran());
				return ret;
			}
			
		};
		
		FurnisherItemTile ss = new FurnisherItemTile(this, st, AVAILABILITY.ROOM_SOLID, false);
		ss.setData(STORAGE);
		FurnisherItemTile tt = new FurnisherItemTile(this, tree, AVAILABILITY.AVOID_PASS, false);
		tt.setData(TREE);
		FurnisherItemTile __ = new FurnisherItemTile(this, RoomSprite.DUMMY, AVAILABILITY.ROOM, false);
		__.setData(TREE+2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__},
			{__,ss,ss,__},
			{__,ss,ss,__},
			{__,__,__,__},
		}, 1);
		
		flush(1, 1, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
		}, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
			{__,tt,tt,__},
			{__,tt,tt,__},
			{__,__,__,__},
		}, 8);
		
		flush(3);
		
		

	}

	@Override
	public boolean usesArea() {
		return true;
	}

	@Override
	public boolean mustBeIndoors() {
		return isIndoors;
	}
	
	@Override
	public boolean mustBeOutdoors() {
		return !isIndoors;
	}


	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		
		if (SETT.ROOMS().fData.tileData.get(tx, ty) != TREE)
			SETT.GRASS().grow(tx, ty, 16);
		
		
		int m = 0;
		for (DIR d : DIR.ORTHO) {
			if (area.is(tx, ty, d)) {
				m |= d.mask();
			}
		}
		if (m != 0x0F)
			SETT.ROOMS().fData.spriteData2.set(tx, ty, m);
		SETT.FLOOR().clearer.clear(tx, ty);
		super.putFloor(tx, ty, upgrade, area);
	}
	
	private final Addable overlay = new Addable(true, false) {
		@Override
		public void renderBelow(Renderer r, RenderIterator it) {
			double d = fertility(it.tx(), it.ty()); 
			d*=d;
			renderUnder(d, r, it, false);
			if (!SETT.ROOMS().placement.embryo.is(it.tile())) {
				double w = SETT.GROUND().MOISTURE_TOT.get(it.tile());
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
//		
//		@Override
//		public boolean render(Renderer r, RenderIterator it) {
//			if (!SETT.ROOMS().placement.embryo.is(it.tile())) {
//				double w = SETT.GROUND().MOISTURE_TOT.get(it.tile());
//				w = CLAMP.d(w, 0, 1);
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
	
	public double fertility(int tx, int ty) {
		double d = SETT.GROUND().MAP.get(tx, ty).farm/SETT.GROUND().types.NORMAL.farm;
		if (SETT.GROUND().types.FOREST.is(tx, ty))
			d = 1.0;
		else
			d*=0.5;
		d = CLAMP.d(d, 0, 1);
		return d;
	}
	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new Instance(blue, area, init);
	}
	
	@Override
	public CharSequence warning(AREA area) {
		double d = fertility.get(area, 0);
		if (d < 0.5)
			return ¤¤warning;
		return null;
	}

	@Override
	public void doBeforePlanning(int tx, int ty) {
		// TODO Auto-generated method stub
		super.doBeforePlanning(tx, ty);
	}
	
	@Override
	public boolean removeFertility() {
		return true;
	}
	
	@Override
	public boolean growsGrass(int tx, int ty) {
		return ROOMS().fData.item.get(tx, ty) == null;
	}
	

}
