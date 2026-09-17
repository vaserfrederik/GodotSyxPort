package settlement.room.food.pasture;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import init.sprite.SPRITES;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.industry.module.IndustryResource;
import settlement.room.main.ROOMA;
import settlement.room.main.Room;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite1xN;
import settlement.room.sprite.RoomSpriteCombo;
import settlement.room.sprite.RoomSpriteImp;
import settlement.room.sprite.RoomSpriteXxX;
import settlement.tilemap.terrain.TerrainDiagonal.Diagonalizer;
import snake2d.CORE;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
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
import util.text.Dic;

abstract class Constructor extends Furnisher{

	
	private static CharSequence ¤¤TooThin = "¤Area is too thin at places. Expand the area to at least 3x3.";
	
	static {
		D.ts(Constructor.class);
	}
	
	private ROOM_PASTURE blue;
	final FurnisherStat workers = new FurnisherStat(this, 1) {
		
		@Override
		public double get(AREA area, double fromItems) {
			return ROOM_PASTURE.WORKERS_PER_TILE*ferarea.get(area, fromItems);
		}
		
		@Override
		public GText format(GText t, double value) {
			return GFORMAT.f(t, value, 1);
		}
	};
	
	public static final int STORAGE1 = 100;
	public static final int STORAGE2 = 200;
	public static final int STORAGE3 = 300;
	
	final FurnisherStat ferarea;
	final FurnisherStat efficiency;
	final FurnisherStat irri;
	
	private final RoomSpriteCombo fence;
	private final RoomSpriteCombo fenceDia;
	
	protected Constructor(ROOM_PASTURE blue, RoomInitData init)
			throws IOException {
		super(init, 2, 4, 88, 44);
		this.blue = blue;
		
		irri = new FurnisherStat.FurnisherStatIrrigation(this, blue);
		efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers);
		
		ferarea = new FurnisherStat(this, 1) {
			
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
					f += fertility(c.x(), c.y());
				}
				return f;
			}
			
			@Override
			public GText format(GText t, double value) {
				double am = 0;
				
				for (IndustryResource o : blue.industries().get(0).outs())
					am += o.rate;
				//am *= blue.bonus().get(POP_CL.clP(null, HCLASSES.CITIZEN()));
				return GFORMAT.f(t, (ROOM_PASTURE.WORKERS_PER_TILE*value*am), 1);
			}
		};
		
		Json js = init.data().json("SPRITES");
		
		
		fence = new RoomSpriteCombo(js, "FENCE_COMBO");
		fenceDia = new RoomSpriteCombo(js, "FENCE_D_COMBO");
		

		
		
	}

	protected void makeAux(Json js) throws IOException {
		
		final RoomSpriteImp auxEdge = new RoomSprite1xN(js, "AUX_EDGE_1X1", false) {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
		};
		final RoomSpriteImp auxMid = new RoomSprite1xN(js, "AUX_MID_1X1", true) {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
		};
		final RoomSpriteXxX auxBig = new RoomSpriteXxX(js, "AUX_BIG_2X2", 2) {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
		};
		
		final FurnisherItemTile m1 = new FurnisherItemTile(this, false, auxEdge, AVAILABILITY.ROOM, false) {
			@Override
			public CharSequence isPlacable(int tx, int ty, MAP_BOOLEAN roomIs, FurnisherItem it, int rx, int ry) {
				for (int i = 0; i < DIR.ALL.size(); i++) {
					if (!roomIs.is(tx, ty, DIR.ALL.get(i)))
						return Dic.empty;
				}
				return super.isPlacable(tx, ty, roomIs, it, rx, ry);
				
			};
		};
		final FurnisherItemTile m2 = new FurnisherItemTile(this, false, auxMid, AVAILABILITY.ROOM, false) {
			@Override
			public CharSequence isPlacable(int tx, int ty, MAP_BOOLEAN roomIs, FurnisherItem it, int rx, int ry) {
				for (int i = 0; i < DIR.ALL.size(); i++) {
					if (!roomIs.is(tx, ty, DIR.ALL.get(i)))
						return Dic.empty;
				}
				return super.isPlacable(tx, ty, roomIs, it, rx, ry);
				
			};
		};
		final FurnisherItemTile ml = new FurnisherItemTile(this, false, auxBig, AVAILABILITY.ROOM, false) {
			@Override
			public CharSequence isPlacable(int tx, int ty, MAP_BOOLEAN roomIs, FurnisherItem it, int rx, int ry) {
				for (int i = 0; i < DIR.ALL.size(); i++) {
					if (!roomIs.is(tx, ty, DIR.ALL.get(i)))
						return Dic.empty;
				}
				return super.isPlacable(tx, ty, roomIs, it, rx, ry);
				
			};
		};
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{m1,m1},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{m1,m2,m1},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{m1,m2,m2,m1},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ml,ml},
			{ml,ml},
		}, 4);
		
		
		flush(3);
	}
	

	@Override
	public boolean usesArea() {
		return true;
	}

	@Override
	public ROOM_PASTURE blue() {
		return blue;
	}

	private double fertility(int tx, int ty) {
		if (mustBeIndoors()) {
			if (SETT.TERRAIN().MOUNTAIN.isMountain(tx, ty))
				return 1.5;
			return 1.0;
		}
		double f = (SETT.GROUND().MAP.get(tx, ty).farm-0.1);
		f *= f;
		return 0.4 + 0.6*f;
	}
	

	
	private final ColorImp color = new ColorImp();
	
	@Override
	public void renderEmbryo(SPRITE_RENDERER r, int mask, RenderIterator it, boolean isFloored, AREA area, boolean active) {
		double f = SETT.GROUND().MAP.get(it.tile()).farm;
		COLOR col = CORE.renderer().colorGet();
		
		if (active) {
			color.interpolate(GCOLOR.MAP().SOSO, GCOLOR.MAP().BETTER, 0.75 + 0.25*f);
			color.bind();
		}
		Room room = SETT.ROOMS().map.get(it.tile());
		
		if (isFloored) {
			COLOR.unbind();
			renderFence(r, ShadowBatch.DUMMY, it, 0);
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
		col.bind();
		super.renderEmbryo(r, mask, it, isFloored, area, active);
	}
	

	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new PastureInstance(blue, area, init);
	}

	
	@Override
	public CharSequence constructionProblem(AREA area) {
		for (COORDINATE c : area.body()){
			if (area.is(c)) {
				boolean ok = false;
				for (DIR d : DIR.ALL) {
					if (isFull(c, area, d)) {
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
	
	private boolean isFull(COORDINATE c, AREA a, DIR d) {
		int tx = c.x()+d.x();
		int ty = c.y()+d.y();
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
			renderFence(r, s, it, 0);
	}
	
	public boolean fenceJoin(ROOMA ii, int tx, int ty) {
		if (!ii.is(tx, ty))
			return false;
		if (!fenceJoin(SETT.ROOMS().fData.tile.get(tx, ty)))
			return false;
		
		for (int di = 0; di < DIR.ALL.size(); di++) {
			DIR d = DIR.ALL.get(di);
			if (!ii.is(tx, ty, d) && !SETT.TERRAIN().get(tx, ty, d).isMassiveWall())
				return true;
		}
		return false;
	}
	
	protected abstract boolean fenceJoin(FurnisherItemTile gc);
	
	public boolean isFence(ROOMA ii, int tx, int ty) {
		if (!ii.is(tx, ty))
			return false;
		if (SETT.ROOMS().fData.tile.get(tx, ty) != null)
			return false;
		for (int di = 0; di < DIR.ORTHO.size(); di++) {
			if (!ii.is(tx, ty, DIR.ORTHO.get(di)))
				return true;
		}
		return false;
	}
	
	public void renderFence(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, double degrade) {
		ROOMA ii = SETT.ROOMS().map.rooma.get(it.tx(), it.ty());
		if (ii == null)
			return;
		if (!fenceJoin(ii, it.tx(), it.ty()))
			return;
		
		int m = 0;
		
		for (DIR d : DIR.ORTHO) {
			if (fenceJoin(ii, it.tx()+d.x(), it.ty()+d.y()) || SETT.TERRAIN().get(it.tx()+d.x(), it.ty()+d.y()).isMassiveWall())
				m|= d.mask();			
		}
		if (m != 0x0F) {
			if (SETT.ROOMS().fData.spriteData.get(it.tile()) == 1) {
				fenceDia.render(r, s, m, it, degrade, false);
			}else {
				fence.render(r, s, m, it, degrade, false);
			}
		}

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
	
}
