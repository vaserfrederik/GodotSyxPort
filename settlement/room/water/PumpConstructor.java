package settlement.room.water;

import java.io.IOException;

import game.time.TIME;
import init.sprite.SPRITES;
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
import settlement.room.sprite.RoomSpriteCombo;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.map.MAP_BOOLEAN;
import snake2d.util.sprite.TILE_SHEET;
import util.GUTIL;
import util.colors.GCOLOR;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import util.spritecomposer.ComposerDests;
import util.spritecomposer.ComposerSources;
import util.spritecomposer.ComposerThings.ITileSheet;
import util.spritecomposer.ComposerUtil;
import util.text.D;

final class PumpConstructor extends Furnisher{

	private final ROOM_PUMP blue;
	
	final FurnisherStat workers;
	private final FurnisherItemTile in;
	final static int B_WORK = 4;
	final static int B_CANAL = -1;
	
	final FurnisherItemTile ou;
	
	private static CharSequence ¤¤GroundD = "the back of the pump must be placed close to fresh water in order to function.";
	
	static {
		D.ts(PumpConstructor.class);
	}
	
	protected PumpConstructor(ROOM_PUMP blue, RoomInitData init)
			throws IOException {
		super(init, 1, 1);
		this.blue = blue;
				
		
		workers = new FurnisherStat.FurnisherStatEmployees(this);

		Json js = init.data().json("SPRITES");
		SPump spump = new SPump(init);
		
		
		RoomSprite1x1 sBottom = new RoomSprite1x1(js, "WORKDONG_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteCombo || item.sprite(rx, ry) == spump.pool;
			};
		};
		
		RoomSprite1x1 sMisc = new RoomSprite1x1(js, "MISC_1X1") {
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().BIG.filled.render(r, 0, x, y);
			}
		};
		
		RoomSprite[] sWork = new RoomSprite1x1[3];
		
		String[] ss = new String[] {
			"A",
			"B",
			"C",
		};
		
		for (int i = 0; i < 3; i++) {
			final int up = i;
			sWork[i] = new RoomSprite1x1(js, "WORK_" + ss[i] + "_1X1") {
				
				@Override
				public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
					return sBottom.getData(tx, ty, rx, ry, item, itemRan);
				}
				
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return item.sprite(rx, ry) instanceof RoomSpriteCombo || item.sprite(rx, ry) == spump.pool;
				};
				
				@Override
				public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
						boolean isCandle) {
					
					animationSpeed = 0;
					PumpInstance ins = blue.get(it.tx(), it.ty());
					
					if (ins != null) {
						if (ins.upgrade() > up) {
							animationSpeed = ins.aniSpeed();
						}else {
							if (blue.job.working(SETT.ROOMS().data.get(it.tile()))) {
								animationSpeed = 1.0;
							}
						}
					}
					
					return super.render(r, s, data, it, degrade, isCandle);
				}
				
				@Override
				public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
					sBottom.render(r, s, getData2(it), it, degrade, false);
				}
				
				@Override
				public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
						FurnisherItem item) {
					SPRITES.cons().BIG.filled.render(r, 0, x, y);
				}
			};
		}
		
		RoomSpriteCombo sBody = new RoomSpriteCombo(js, "FRAME_COMBO") {
			
			final RoomSprite1x1 top = new RoomSprite1x1(js, "FRAME_TOP_1X1");
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				
				if (!SETT.ROOMS().fData.candle.is(it.tile()) && (GUTIL.ran2().get(it.tile()) & 0b11) == 0) {
					top.render(r, s, getData2(it), it, degrade, false);
				}
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};
		

		RoomSprite1x1 sPipe = new RoomSprite1x1(js, "PIPE_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteCombo;
			};
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().BIG.filled.render(r, 0, x, y);
			}
		};
		
		RoomSprite1x1 sPipeIn = new RoomSprite1x1(js, "PIPE_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteCombo;
			};
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				DIR dir = DIR.ORTHO.get(data);
				SPRITES.cons().ICO.arrows.get(dir.orthoID()).render(r, x, y);
			}
		};

		RoomSprite s__ = null;
		
		final FurnisherItemTile w1 = new FurnisherItemTile(this, false, sWork[0], AVAILABILITY.SOLID, false).setData(B_WORK);
		final FurnisherItemTile w2 = new FurnisherItemTile(this, false, sWork[1], AVAILABILITY.SOLID, false).setData(B_WORK+1);
		final FurnisherItemTile w3 = new FurnisherItemTile(this, false, sWork[2], AVAILABILITY.SOLID, false).setData(B_WORK+2);		
		
		ou = new FurnisherItemTile(this, false, spump.sp, AVAILABILITY.SOLID, false).setData(B_CANAL);
		final FurnisherItemTile pi = new FurnisherItemTile(this, false, sPipe, AVAILABILITY.SOLID, false);
		in = new FurnisherItemTile(this, false, sPipeIn, AVAILABILITY.SOLID, false) {
			@Override
			public CharSequence isPlacable(int tx, int ty, MAP_BOOLEAN roomIs, FurnisherItem it, int rx, int ry) {
				if (!SETT.TERRAIN().WATER.groundWater.is(tx, ty)) {
					return ¤¤GroundD;
				}
				return null;
			}
		};
		
		final FurnisherItemTile bo = new FurnisherItemTile(this, false, sBody, AVAILABILITY.SOLID, false);
		final FurnisherItemTile b1 = new FurnisherItemTile(this, false, spump.sprite(0, js), AVAILABILITY.SOLID, false);
		final FurnisherItemTile b2 = new FurnisherItemTile(this, false, spump.sprite(1, js), AVAILABILITY.SOLID, false);
		final FurnisherItemTile b3 = new FurnisherItemTile(this, false, spump.sprite(2, js), AVAILABILITY.SOLID, false);
		final FurnisherItemTile oo = new FurnisherItemTile(this, false, spump.pool, AVAILABILITY.SOLID, false);
		final FurnisherItemTile mm = new FurnisherItemTile(this, false, sMisc, AVAILABILITY.SOLID, false);
		
		final FurnisherItemTile __ = new FurnisherItemTile(this, false, s__, AVAILABILITY.ROOM, false);
	
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,mm,pi,ou,pi,mm,__},
			{__,w2,oo,oo,oo,w2,__},
			{__,w1,oo,oo,oo,w1,__},
			{__,w3,b1,b2,b3,w3,__},
			{__,mm,in,in,in,mm,__},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,mm,pi,ou,pi,mm,__},
			{__,w2,bo,bo,bo,w2,__},
			{__,w1,bo,bo,bo,w1,__},
			{__,w3,oo,oo,oo,w3,__},
			{__,w2,oo,oo,oo,w2,__},
			{__,w1,oo,oo,oo,w1,__},
			{__,w3,b1,b2,b3,w3,__},
			{__,mm,in,in,in,mm,__},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,mm,pi,ou,pi,__,__},
			{__,w2,bo,bo,bo,w2,__},
			{__,w1,b1,b2,b3,w1,__},
			{__,w3,bo,bo,bo,w3,__},
			{__,w2,oo,oo,oo,w2,__},
			{__,w1,oo,oo,oo,w1,__},
			{__,w3,oo,oo,oo,w3,__},
			{__,w2,oo,oo,oo,w2,__},
			{__,w1,oo,oo,oo,w1,__},
			{__,w3,b1,b2,b3,w3,__},
			{__,mm,in,in,in,mm,__},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,mm,pi,ou,pi,mm,__},
			{__,w2,bo,bo,bo,w2,__},
			{__,w1,b1,b2,b3,w1,__},
			{__,w3,bo,bo,bo,w3,__},
			{__,w2,oo,oo,oo,w2,__},
			{__,w1,oo,oo,oo,w1,__},
			{__,w3,oo,oo,oo,w3,__},
			{__,w2,oo,oo,oo,w2,__},
			{__,w1,oo,oo,oo,w1,__},
			{__,w3,oo,oo,oo,w3,__},
			{__,w2,bo,bo,bo,w2,__},
			{__,w1,b1,b2,b3,w1,__},
			{__,w3,bo,bo,bo,w3,__},
			{__,mm,in,in,in,mm,__},
		}, 4);
		
		
		
		flush(1, 3);
	}

	@Override
	public CharSequence placable(int tx, int ty, FurnisherItem item, FurnisherItemTile tile) {
		if (tile == in) {
			if (!SETT.TERRAIN().WATER.groundWater.is(tx, ty)) {
				return ¤¤GroundD;
			}
			return null;
		}
		return super.placable(tx, ty, item, tile);
	}
	
	public boolean isJob(int tx, int ty) {
		PumpInstance ins = blue.get(tx, ty);
		
		if (ins != null) {
			
			int dd = SETT.ROOMS().fData.tileData.get(tx, ty)-B_WORK;
			return ins.upgrade() <= dd;
		}
		return false;
	}
	
	@Override
	public boolean usesArea() {
		return false;
	}

	@Override
	public boolean mustBeIndoors() {
		return false;
	}

	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new PumpInstance(blue, area, init);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}

	private final Addable overlay = new Addable(true, false) {
		
		@Override
		public void renderBelow(snake2d.Renderer r, RenderIterator it) {
			COLOR c = COLOR.WHITE05;
			if (SETT.JOBS().getter.get(it.tile()) == null) {
				if (SETT.TERRAIN().WATER.groundWater.is(it.tile())) {
					c = GCOLOR.MAP().OVERLAY_GOOD;
				}
			}
			
			renderUnder(c, r, it);
		};
		
	};
	
	@Override
	public Addable overlay() {
		return overlay;
	}
	
	
	final class SPump {
		
		private final TILE_SHEET sheet;
		
		private final TILE_SHEET rim;
		private final TILE_SHEET stencil;
		private final TILE_SHEET outlet;
		
		private final static int animations = 5;
		private final static int shadow = animations*2*4;
		
		SPump(RoomInitData init) throws IOException{
			sheet = new ITileSheet(init.gSprite.get("PUMP"), 220, 76) {
				
				@Override
				protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
					s.full2.init(0, 0, animations, 2, 1, 2, d.s16);
					for (int r = 0; r < 2; r++) {
						for (int i = 0; i < animations; i++) {
							s.full2.setVar(i+animations*r).setSkip(1, 0);
							s.full2.paste(3, true);
						}
						for (int i = 0; i < animations; i++) {
							s.full2.setVar(i+animations*r).setSkip(1, 1);
							s.full2.paste(3, true);
						}
					}
					return d.s16.saveGame();
					
				}
			}.get();
			
			rim = new ITileSheet() {
				
				@Override
				protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
					s.full.init(0, s.full2.body().y2(), 1, 1, 3, 3, d.s16);
					s.full.paste(true);
					return d.s16.saveGame();
				}
			}.get();
			
			stencil = new ITileSheet() {
				
				@Override
				protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
					s.full.init(0, s.full.body().y2(), 1, 1, 3, 3, d.s16);
					s.full.paste(true);
					return d.s16.saveGame();
				}
			}.get();
			
			outlet = new ITileSheet() {
				
				@Override
				protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
					s.full2.init(s.full.body().x2(), s.full.body().y1(), 1, 1, 1, 1, d.s16);
					s.full2.paste(3, true);
					return d.s16.saveGame();
				}
			}.get();
		}
		
		public RoomSprite sprite(int ii, Json js) throws IOException {
			return new RoomSpriteCombo(js, "FRAME_COMBO") {
	
				
				
				@Override
				public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
					return (byte) ((item.rotation-1)&3);
				}
				
				@Override
				public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
					
					int rot = getData2(it);
					
					DIR d = DIR.ORTHO.get(rot);
					int aniI = GUTIL.ran2().get(it.tx()-d.x()+d.x()*ii, it.ty()-d.y()+d.y()*ii);
					
					int frames = (animations-1)*2;
					int ani =  (aniI + (int) (TIME.currentSecond()*6.0))%frames;
					if (ani >= animations)
						ani = frames-ani;
					int tile = 0;
					if (ii == 0) {
						tile = rot + ani*4;
					}else if (ii == 1) {
						tile = rot + animations*4 + ani*4;
					}else {
						rot = (rot+2)%4;
						ani = animations-ani-1;
						tile = rot + ani*4;
					}
					sheet.render(r, tile, it.x(), it.y());
					s.setHeight(14).setDistance2Ground(0);
					sheet.render(s, tile+shadow, it.x(), it.y());
				}
				
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return super.joins(tx, ty, rx, ry, d, item) || item.sprite(rx+d.x(), ry+d.y()) == pool;
				}
				
			};
		}
		
		public final RoomSprite pool = new RoomSprite() {
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				
				int m = 0;
				for (int di = 0; di < DIR.ORTHO.size(); di++) {
					DIR d = DIR.ORTHO.get(di);
					if (item.sprite(rx+d.x(), ry+d.y()) == this) {
						m |= d.mask();
					}
				}
				SPRITES.cons().BIG.outline.render(r, m, x, y);
			}

			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				s.setHeight(16);
				s.setDistance2Ground(0);
				rim.render(s, data, it.x(), it.y());
				int op = 10;
				PumpInstance p = SETT.ROOMS().WATER.pump.get(it.tx(), it.ty());
				if (p != null) {
					op += 60*p.value/p.valueMax;
				}
				SETT.ROOMS().WATER.sprite.renderWater(r, it, stencil, data, op);
				return false;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				int h = 4;
				PumpInstance p = SETT.ROOMS().WATER.pump.get(it.tx(), it.ty());
				if (p != null) {
					h = (int) (8 - 7*p.value/p.valueMax);
					h = Math.max(h, 0);
				}
				s.setHeight(h);
				s.setDistance2Ground(0);
				rim.render(r, data, it.x(), it.y());
				rim.render(s, data, it.x(), it.y());
			};

			@Override
			public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				int x = 0;
				int y = 0;
				if (item.sprite(rx+1, ry) != this)
					x = 2;
				else if (item.sprite(rx-1, ry) == this)
					x = 1;
				if (item.sprite(rx, ry+1) != this)
					y = 2;
				else if (item.sprite(rx, ry-1) == this)
					y = 1;
				return (byte) (y*3 + x);
			}

			@Override
			public int sData() {
				return 0;
			}
		};
		
		public final RoomSprite sp = new RoomSprite() {
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				SETT.ROOMS().WATER.sprite.renderBelow(r, s, it, DIR.ORTHO.get(data).perpendicular().mask(), false);
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				DIR dir = DIR.ORTHO.get(data);
				SPRITES.cons().ICO.arrows.get(dir.orthoID()).render(r, x, y);
			}

			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				int flow = blue.is(it.tile()) ? DIR.ORTHO.get(data).mask() : 0;
				SETT.ROOMS().WATER.sprite.render(r, s, it,  DIR.ORTHO.get(data).perpendicular().mask(), flow, false);
				return false;
			}

			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				outlet.render(r, data, it.x(), it.y());
			};
			
			@Override
			public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return (byte) item.rotation;
			}

			@Override
			public int sData() {
				return 0;
			}
		};
		
	}





}
