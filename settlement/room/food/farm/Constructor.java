package settlement.room.food.farm;

import static settlement.main.SETT.TERRAIN;

import java.io.IOException;

import init.constant.C;
import init.sprite.SPRITES;
import init.sprite.UI.UI;
import init.sprite.game.Sheet;
import init.sprite.game.SheetData;
import init.sprite.game.SheetType;
import settlement.main.SETT;
import settlement.overlay.Addable;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import snake2d.CORE;
import snake2d.Renderer;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.LIST;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.text.D;

final class Constructor extends Furnisher{

	private static CharSequence ¤¤warning = "Fertility for this farm is very low, which will result in low yields. It can be improved by digging water around the farm. Proceed anyway?";
	static {
		D.ts(Constructor.class);
	}
	
	final boolean isIndoors;
	private final ROOM_FARM blue;
	final FurnisherStat fertility = new FurnisherStat(this, 1/10000.0) {

		@Override
		public GText format(GText t, double value) {
			return GFORMAT.perc(t, value);
		}

		@Override
		public double get(AREA area, double fromItems) {
			
			double v = 0;
			for (COORDINATE c : area.body()) {
				
				if (area.is(c)) {
					v += fertility(c.x(), c.y());
					
				}
			}
			return v/area.area();
		}
		
		@Override
		public double max() {
			return isIndoors ? 1.0 : 1.2;
		};
		
		@Override
		public double min() {
			return isIndoors ? 0.9 : 0;
		};
		
	};
	
	final FurnisherStat workers = new FurnisherStat(this, 0.01) {

		@Override
		public GText format(GText t, double value) {
			return GFORMAT.f(t, value);
		}

		@Override
		public double get(AREA area, double fromItems) {
			return area.area()*ROOM_FARM.WORKERPERTILEI;
		}
		
	};
	
	public double fertility(int tx, int ty) {
		if (!isIndoors) {
			return SETT.GROUND().MAP.get(tx, ty).farm;
		}else
			return SETT.TERRAIN().MOUNTAIN.isMountain(tx, ty) ? 1.0 : 0.9;
	}
	
	final FurnisherStat output;
	
	final FurnisherStat irri;
	
	private final LIST<Sheet> sheets;
	
	protected Constructor(ROOM_FARM blue, RoomInitData init)
			throws IOException {
		super(init, 0, 4, 88, 44);
		
		irri = new FurnisherStat.FurnisherStatIrrigation(this, blue);
		
		output = new FurnisherStat.FurnisherStatProduction2(this, blue, 0.01) {
			
			@Override
			protected double getBase(AREA area, double[] acc) {
				double f = 0;
				for (COORDINATE c : area.body()) {
					if (area.is(c)) {
						f += fertility(c.x(), c.y()); 
					}
				}
				return ROOM_FARM.WORKERPERTILEI*f*blue.industries().get(0).outs().get(0).rate;
			}
			
		};
		
		
		
		isIndoors = init.data().bool("INDOORS");
		this.blue = blue;
		
		sheets = SPRITES.GAME().sheets(SheetType.s1x1, "_FARM_DIRT", null);
//		for (Sheet s : sheets) {
//			s.hasRotation = true;
//			s.hasShadow = false;
//		}

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
		int m = 0;
		for (DIR d : DIR.ORTHO)
			if (area.is(tx, ty, d))
				m |= d.mask();
		SETT.ROOMS().fData.spriteData.set(tx, ty, m);
		SETT.FLOOR().clearer.clear(tx, ty);
	}
	
//	@Override
//	public CharSequence placable(int tx, int ty) {
//		if (SETT.MINERALS().amountD.get(tx, ty) > 0)
//			return PLACABLE.E;
//		return super.placable(tx, ty);
//	}
	
	@Override
	public void renderEmbryo(SPRITE_RENDERER r, int mask, RenderIterator it, boolean isFloored, AREA area, boolean active) {
		
		if (isFloored && active) {
			COLOR c = CORE.renderer().colorGet();
			COLOR.unbind();
			renderTill(r, it, area, 0);
			c.bind();
		}
		super.renderEmbryo(r, mask, it, isFloored, area, active);
	}
	
	private final Addable overlay = new Addable(true, false) {
		@Override
		public void renderBelow(Renderer r, RenderIterator it) {
			double d = SETT.GROUND().MAP.get(it.tile()).farm/SETT.GROUND().types.NORMAL.farm;
			d = CLAMP.d(d, 0, 1);
			d*=d;
			renderUnder(d, r, it, false);
			if (!SETT.ROOMS().placement.embryo.is(it.tile()) && TERRAIN().get(it.tile()).clearing().can() && !SETT.TERRAIN().WATER.DEEP.is(it.tile())) {
				double w = SETT.GROUND().MOISTURE_TOT.get(it.tile());
				w = CLAMP.d(w, 0, 1);
				if (w > 0) {
					ColorImp.TMP.interpolate(COLOR.ORANGE100, COLOR.BLUE100, w).bind();;
					int s = (int) (C.TILE_SIZE/4 + w*3*C.TILE_SIZE/4);
					int x1 = it.x() + (C.TILE_SIZE-s)/2;
					int y1 = it.y() + (C.TILE_SIZE-s)/2;
					
					UI.icons().s.drop.render(r, x1, x1 +s, y1, y1+s);
				}
			}
		};
		
	};
	
	@Override
	public Addable overlay() {
		if (!isIndoors)
			return overlay;
		return null;
	}
	

	
	void renderTill(SPRITE_RENDERER r, RenderIterator it, AREA area, double till) {
		
		int d = direction(it, area);
		
		int sheet = 0;
		int rot = 0;
		
		if (area.is(it.tx(), it.ty(), DIR.ORTHO.get(d))) {
			if (area.is(it.tx(), it.ty(), DIR.ORTHO.get(d+2))) {
				sheet = 2;
				rot = d + 2*(it.ran()&1);
			}else {
				sheet = 1;
				rot = d;
			}
		}else if (area.is(it.tx(), it.ty(), DIR.ORTHO.get(d+2))) {
			sheet = 1;
			rot = d +2;
		}else {
			rot = d + 2*(it.ran()&1);
		}
		
		renderTill(r, it, till, sheet, rot);
		
	}
	
	private void renderTill(SPRITE_RENDERER r, RenderIterator it, double till, int t, int rot) {
		
		till = 1.0-till;
		int aa = (int) (till*(sheets.size()/3-1));
		
		t = (int) (3*aa) + t;
		int data = SheetType.s1x1.tile(sheets.get(0), SheetData.DUMMY, 0, it.ran(), rot);
		sheets.get(t).render(SheetData.DUMMY, it.x(), it.y(), it, r, data, it.ran(), 0);
	}
	
	int direction(RenderIterator it, AREA area) {
		return (it.ran(area.body().x1(), area.body().y1())& 1);
	}
	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new FarmInstance(blue, area, init);
	}
	
	@Override
	public CharSequence warning(AREA area) {
		double d = fertility.get(area, 0);
		if (d < 0.5)
			return ¤¤warning;
		return null;
	}

}
