package settlement.room.home.house;

import java.io.IOException;

import init.constant.C;
import init.race.home.RaceHomeClass;
import init.sprite.SPRITES;
import init.sprite.game.SheetPair;
import init.sprite.game.SheetType;
import init.sprite.game.Sheets;
import settlement.main.SETT;
import settlement.room.home.house.Sprite.Rot;
import settlement.room.main.Room;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.sprite.RoomSprite;
import settlement.tilemap.floor.Floors.Floor;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.datatypes.Rec;
import snake2d.util.file.Json;
import snake2d.util.rnd.RND;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

class Sprites {

	private final SpriteCarpet carpet = new SpriteCarpet();
	
	public static final int renderAbsolute = 1;
	public final Sprite staU;
	public final Sprite staD;

	final SpriteConfigs sp;
	
	Sprites(Json json) throws IOException{
		
		Json js = json.json("SPRITES");
		
		staU = new Sprite() {

			private final Sheets sheets = new Sheets(SheetType.s1x1, js.json("UP_1X1"));
			
			{
				sDataSet(renderAbsolute);
			}
			

			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				if (house.upgrade() < 2)
					return false;
				int ran = it.ran();
				render1x1(ran, sheets, r, s, data, it, degrade);

				return false;
			}

			@Override
			public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {

				HomeInstance room = SETT.ROOMS().HOME.getter.get(tx, ty);
				int r = RND.rInt(4);
				
				for (int i = 0; i < DIR.ORTHO.size(); i++) {
					DIR d = DIR.ORTHO.getC(i+r);
					if (room.serviceX() == tx + d.x() && room.serviceY() == ty+d.y() && !room.isSame(tx, ty, tx-d.x(), ty-d.y()) ) {
						return (byte)d.orthoID();
					}
					
				}
				

				for (int i = 0; i < DIR.ORTHO.size(); i++) {
					DIR d = DIR.ORTHO.getC(i+r);
					if (room.sprite(tx+d.x(), ty+d.y()) == null && room.isSame(tx, ty, tx+d.x(), ty+d.y())) {
						return (byte)d.orthoID();
					}
					
				}


				for (int i = 0; i < DIR.ORTHO.size(); i++) {
					DIR d = DIR.ORTHO.getC(i+r);
					if (!room.isSame(tx, ty, tx+d.x(), ty+d.y()) ) {
						return (byte)d.orthoID();
					}
					
				}
				return (byte) RND.rInt(4);
			}
		};
		staD = new Sprite() {
			{
				sDataSet(renderAbsolute);
			}
			private final Sheets sheets = new Sheets(SheetType.s1x1, js.json("DOWN_1X1"));
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (house.upgrade() == 0)
					return;
				int ran = it.ran();
				render1x1(ran, sheets, r, s, data, it, degrade);
			};
			
			@Override
			public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return staU.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		
		
		sp = new SpriteConfigs();
	}

	

	
	public final Sprite bedN = new Sprite() {

		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			
			int ran = it.ran();
			Sheets a = sp().bedTop.get(house);
			render1x1(ran, a, r, s, data, it, degrade);

			return false;
		}

		@Override
		public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			
			HomeInstance house = SETT.ROOMS().HOME.getter.get(tx, ty);
			for (int i = 0; i < DIR.ORTHO.size(); i++) {
				DIR d = DIR.ORTHO.get(i);
				if (house.sprite(tx+d.x(), ty+d.y()) == bedS) {
					return (byte) (i);
				}
			}
			return 0;
		}
	};

	public final Sprite bedS = new Sprite(true, true, true) {

		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			
			DIR dir = DIR.ORTHO.get((data & 0b011));
			it.ranOffset(dir.x(), dir.y());
			int ran = it.ran();
			Sheets a = sp().bedBottom.get(house);
			render1x1(ran, a, r, s, data, it, degrade);
			return false;
		}

		@Override
		public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			HomeInstance house = SETT.ROOMS().HOME.getter.get(tx, ty);
			for (int i = 0; i < DIR.ORTHO.size(); i++) {
				DIR d = DIR.ORTHO.get(i);
				if (house.sprite(tx+d.x(), ty+d.y()) == bedN) {
					return (byte) (i);
				}
			}
			return 0;
		}
	};

	static void render1x1(int ran, Sheets a, SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
		if (a != null) {
			SheetPair sh = a.get(ran);
			if (sh != null) {
				sh.d.color(ran).bind();
				ran = ran >> 4;
				int i = SheetType.s1x1.tile(sh.s, sh.d, 0, sh.d.frame(ran, 1.0), (data & 0b011));
				ran = ran >> 4;
				sh.s.render(sh.d, it.x(), it.y(), it, r, i, ran, 0);
				COLOR.unbind();
				sh.s.renderShadow(sh.d, it.x(), it.y(), it, s, i, ran);
			}
		}
	}
	
	public final Sprite nSta = new Sprite() {

		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			Sheets a = sp().nightStand.get(house);
			render1x1(it.ran(), a, r, s, data, it, degrade);

			return false;
		}

		@Override
		public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			return 0;
		}
	};
	
	public final Sprite tabl = new Sprite() {

		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			Sheets a = sp().table.get(house);
			if (a != null) {
				int vv = (data >> 4)&0x0F;
				SheetPair sh = a.get(vv);
				if (sh != null) {
					sh.d.color(vv).bind();
					int ran = it.ran();
					int t = SheetType.sCombo.tile(sh.s, sh.d, data&0x0F, sh.d.frame(ran, 1.0), 0);
					sh.s.render(sh.d, it.x(), it.y(), it, r, t, ran, 0);
					COLOR.unbind();
					sh.s.renderShadow(sh.d, it.x(), it.y(), it, s, t, ran);
				}
			}

			return false;
		}
		
		
		@Override
		public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			HomeInstance house = SETT.ROOMS().HOME.getter.get(tx, ty);
			int m = 0;
			for (DIR d : DIR.ORTHO) {
				if (house.sprite(tx+d.x(), ty+d.y()) == this)
					m |= d.mask();
			}
			return (byte) (m | ((itemRan&0x0F) << 4));
		}
	};

	public final Sprite stor = new Rot() {

		@Override
		Sheets a(RaceHomeClass it) {
			return it.storage.get(house);
		}

	};

	public final Sprite chai = new Rot(true, false, true) {

		@Override
		Sheets a(RaceHomeClass it) {
			return it.chair.get(house);
		}
		
		@Override
		public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			
			HomeInstance house = SETT.ROOMS().HOME.getter.get(tx, ty);
			

			int r = RND.rInt(4);
			for (int i = 0; i < DIR.ORTHO.size(); i++) {
				DIR d = DIR.ORTHO.getC(i+r);
				if (!house.isSame(tx, ty, tx+d.x(), ty+d.y())) {
					return (byte) ((i+2)%4);
				}
				
			}
			for (int i = 0; i < DIR.ORTHO.size(); i++) {
				DIR d = DIR.ORTHO.get(i);
				if (house.sprite(tx+d.x(), ty+d.y()) == SETT.ROOMS().HOME.constructor.sp.tabl) {
					return (byte) (i);
				}
			}
			
			return (byte) RND.rInt(4);
		}
	};

	private final Sprite nic1Top = new Rot() {

		@Override
		Sheets a(RaceHomeClass it) {
			return it.nickTop1.get(house);
		}

	};
	
	public final Sprite nic1 = new Rot() {

		@Override
		Sheets a(RaceHomeClass it) {
			return it.nick1.get(house);
		}
		
		@Override
		public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
			nic1Top.house = house;
			nic1Top.render(r, s, data, it, degrade, false);
		};

	};

	public final Sprite nic2 = new Rot() {

		@Override
		Sheets a(RaceHomeClass it) {
			return it.nick2.get(house);
		}

	};

	public final Sprite _mat = new Rot(false, false, false) {

		@Override
		Sheets a(RaceHomeClass it) {
			return it.mat.get(house);
		}
	};

	public final RoomSprite theDummy = new RoomSprite.Dummy() {

		@Override
		public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
			HomeInstance h = SETT.ROOMS().HOME.getter.get(it.tile());
			if (h != null) {
				s.setSoft();
				s.setDistance2Ground(0).setHeight(0);
				COLOR.BLACK.render(s, it.x(), it.x() + C.TILE_SIZE, it.y(), it.y() + C.TILE_SIZE);
				s.setPrev();
				
				
				if (h.occupants() > 0) {
					Sprite sp = h.sprite(it.tx(), it.ty());
					if (sp != null) {
						sp.house = h;
						sp.renderAbove(r, s, data, it, degrade);
					}
				}
			}
		}

		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch shadowBatch, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			HomeInstance h = SETT.ROOMS().HOME.getter.get(it.tile());
			if (h != null) {
				if (h.occupants() > 0) {
					
					RaceHomeClass stage = h.race().home().clas(h.occupant(0));
					Floor f = stage.floor(h);
					if (f != SETT.FLOOR().getter.get(it.tx(), it.ty())) {
						if (stage.floor(h) == null)
							SETT.FLOOR().clearer.clear(it.tx(), it.ty());
						else
							stage.floor(h).placeFixed(it.tx(), it.ty());
					}
					
					Sprite s = h.sprite(it.tx(), it.ty());
					if (s != null) {
						s.house = h;
						s.render(r, shadowBatch, data, it, degrade, isCandle);
					}
//					shadowBatch.setSoft();
//					shadowBatch.setHeight(16).setDistance2Ground(4);
//					COLOR.BLACK.render(shadowBatch, it.x(), it.x() + C.TILE_SIZE, it.y(), it.y() + C.TILE_SIZE);
//					shadowBatch.setPrev();
				}else {
					Sprite s = h.sprite(it.tx(), it.ty());
					if (s != null && s.sData() == renderAbsolute) {
						s.house = h;
						s.render(r, shadowBatch, data, it, degrade, isCandle);
					}
					if (SETT.FLOOR().getter.get(it.tx(), it.ty()) != SETT.ROOMS().HOME.constructor.flooring)
						SETT.ROOMS().HOME.constructor.flooring.placeFixed(it.tx(), it.ty());
				}
			}

			return false;

		}

		@Override
		public void renderBelow(SPRITE_RENDERER r, ShadowBatch shadow, int data, RenderIterator it, double degrade) {
			HomeInstance h = SETT.ROOMS().HOME.getter.get(it.tile());
			if (h != null) {
				if (h.occupants() > 0) {
					
					Sprite s =  h.sprite(it.tx(), it.ty());
					if (s != null) {
						s.house = h;
						s.renderBelow(r, shadow, data, it, degrade);
					}

					renderCarpet(r, shadow, data, it, degrade, h);
				}else {
					Sprite s = h.sprite(it.tx(), it.ty());
					if (s != null && s.sData() == renderAbsolute) {
						s.house = h;
						s.renderBelow(r, shadow, data, it, degrade);
					}
				}
			}
		}

		private void renderCarpet(SPRITE_RENDERER r, ShadowBatch shadow, int data, RenderIterator it, double degrade, HomeInstance h) {

			DIR dd = h.dir();
			
			Room room = SETT.ROOMS().map.get(it.tile());
			int rx = it.tx()-room.x1(it.tx(), it.ty());
			int ry = it.ty()-room.y1(it.tx(), it.ty());
			int c = carpet.get(rx, ry, h.it());
			if (c == 0)
				return;
			
			Sheets a =  h.race().home().clas( h.occupant(0)).carpet.get(h);
			if (a == null)
				return;
			
			int ran = it.ran(room.x1(it.tx(), it.ty())+c, room.y1(it.tx(), it.ty()));
			SheetPair ts = a.get(ran);
			
			
			int t = 0;
			for (DIR d : DIR.ORTHO) {
				if (carpet.get(rx, ry, d, h.it()) == c)
					t |= d.mask();
			}
			
			
			ran = ran >> 4;
			t = SheetType.sCombo.tile(ts.s, ts.d, t, ts.d.frame(ran, 1.0), 0);
			
			int dx = C.TILE_SIZEH / 2 + (ran & C.T_MASK) / 2;
			if (dd.x() > 0)
				dx = 0;
			else if (dd.x() < 0) {
				dx = C.TILE_SIZEH + C.TILE_SIZEH / 2;
			}
			ran = ran >> C.T_SCROLL;
			int dy = C.TILE_SIZEH / 2 + (ran & C.T_MASK) / 2;
			if (dd.y() > 0)
				dy = 0;
			else if (dd.y() < 0) {
				dy = C.TILE_SIZEH + C.TILE_SIZEH / 2;
			}
			ts.s.render(ts.d, it.x() - dx, it.y() - dy, it, r, t, ran, 0);

		}
		
		@Override
		public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item) {
			if (item.get(rx, ry).data() == 2)
				return;
				
			int m = 0;
			for (DIR d : DIR.ORTHO)
				if (item.is(rx, ry, d))
					m |= d.mask();
			SPRITES.cons().BIG.outline.render(r, m, x, y);
		};

		private final Rec tmp = new Rec();

		@Override
		public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			byte m = 0;

			int w = item.group().item(0, item.rotation).width();
			int h = item.group().item(0, item.rotation).height();
			tmp.setDim(w, h);

			tmp.moveX1Y1(w * (rx / w), h * (ry / h));

			for (DIR d : DIR.ORTHO) {
				if (tmp.holdsPoint(rx, ry, d))
					m |= d.mask();
			}
			return m;
		}

	};


	final class SpriteConfigs {

		final SpriteConfig[][] sprites = new SpriteConfig[4][];

		public SpriteConfigs() {
			sprites[0] = mirror(new SpriteConfig[] {
				new SpriteConfig(new Sprite[][] { 
					{ bedN, bedS, nic2 }, 
					{ nSta, null, nic1 }, 
					{ staU, _mat, staD }, }),
				new SpriteConfig(new Sprite[][] { 
					{ nic2, bedS, bedN }, 
					{ nic1, null, nSta }, 
					{ staD, _mat, staU }, }),
				new SpriteConfig(new Sprite[][] { 
					{ bedN, nSta, nic2 }, 
					{ bedS, null, nic1 }, 
					{ staU, _mat, staD }, }),
				new SpriteConfig(new Sprite[][] { 
					{ nic2, nSta, bedN }, 
					{ nic1, null, bedS }, 
					{ staU, _mat, staD }, }),
				new SpriteConfig(new Sprite[][] { 
					{ nSta, stor, bedN }, 
					{ chai, null, bedS }, 
					{ staU, _mat, staD }, }),
				new SpriteConfig(new Sprite[][] { 
					{ bedN, nSta, stor }, 
					{ bedS, null, chai }, 
					{ staD, _mat, staU }, }),

			});
			
			sprites[1] = mirror(new SpriteConfig[] {
				new SpriteConfig(new Sprite[][] { 
					{ bedN, nSta, bedN }, 
					{ bedS, null, bedS }, 
					{ chai, null, nic1 }, 
					{ tabl, null, nic2 }, 
					{ staU, _mat, staD }, }),
				new SpriteConfig(new Sprite[][] { 
					{ bedN, nSta, bedN }, 
					{ bedS, null, bedS }, 
					{ nic1, null, chai }, 
					{ nic2, null, stor }, 
					{ staU, _mat, staD }, }),
				new SpriteConfig(new Sprite[][] { 
					{ bedN, nSta, bedN }, 
					{ bedS, null, bedS }, 
					{ chai, null, nic2 }, 
					{ stor, null, nic1 }, 
					{ staU, _mat, staD }, }),
				new SpriteConfig(new Sprite[][] { 
					{ bedN, nSta, bedN }, 
					{ bedS, null, bedS }, 
					{ nic1, null, nic2 }, 
					{ nic1, null, nic2 }, 
					{ staU, _mat, staD }, }),
				new SpriteConfig(new Sprite[][] { 
					{ bedN, nSta, bedN }, 
					{ bedS, null, bedS }, 
					{ nic1, null, stor }, 
					{ chai, null, chai }, 
					{ staU, _mat, staD }, }),
				new SpriteConfig(new Sprite[][] { 
					{ bedN, nSta, stor }, 
					{ bedS, null, nic1 }, 
					{ staD, null, chai }, 
					{ bedN, null, tabl }, 
					{ bedS, _mat, staU }, }),
				new SpriteConfig(new Sprite[][] { 
					{ nic1, nSta, bedN }, 
					{ null, null, bedS }, 
					{ chai, null, bedS }, 
					{ tabl, null, bedN }, 
					{ staD, _mat, staU }, }),
				new SpriteConfig(new Sprite[][] { 
					{ bedN, stor, tabl }, 
					{ bedS, null, nic1 }, 
					{ bedS, null, chai }, 
					{ bedN, null, null }, 
					{ staD, _mat, staU }, }),
				new SpriteConfig(new Sprite[][] { 
					{ tabl, stor, bedN }, 
					{ nic2, null, bedS }, 
					{ nSta, null, bedS }, 
					{ chai, null, bedN }, 
					{ staD, _mat, staU }, }),
				new SpriteConfig(new Sprite[][] { 
					{ bedN, nSta, nic1 }, 
					{ bedS, null, nic2 }, 
					{ bedS, null, chai }, 
					{ bedN, null, tabl }, 
					{ staD, _mat, staU }, }),
				new SpriteConfig(new Sprite[][] { 
					{ nic1, nSta, bedN }, 
					{ chai, null, bedS }, 
					{ nic2, null, bedS }, 
					{ tabl, null, bedN }, 
					{ staD, _mat, staU }, }),
				new SpriteConfig(new Sprite[][] { 
					{ bedN, stor, tabl }, 
					{ bedS, null, nSta }, 
					{ bedS, null, nic2 }, 
					{ bedN, null, chai }, 
					{ staD, _mat, staU }, }),
				new SpriteConfig(new Sprite[][] { 
					{ tabl, stor, bedN }, 
					{ nic2, null, bedS }, 
					{ nSta, null, bedS }, 
					{ chai, null, bedN }, 
					{ staD, _mat, staU }, }),
			});

			sprites[2] = mirror(new SpriteConfig[] {
				new SpriteConfig(new Sprite[][] {
					{ tabl, nic1, nic1, nic1, nSta },
					{ bedN, bedS, null, bedS, bedN },
					{ bedN, bedS, null, bedS, bedN },
					{ nic2, null, null, null, stor },
					{ nic2, null, null, null, nic2 },
					{ stor, staD, _mat, staU, tabl }, }),
				new SpriteConfig(new Sprite[][] {
					{ nSta, nic2, stor, nic2, nic2 },
					{ bedN, bedS, null, bedS, bedN },
					{ bedN, bedS, null, bedS, bedN },
					{ chai, null, null, null, chai },
					{ nic1, null, null, null, nic2 },
					{ nic1, staD, _mat, staU, tabl }, }),
				new SpriteConfig(new Sprite[][] {
					{ nSta, nic2, nic2, nic2, nic1 },
					{ bedN, bedS, null, bedS, bedN },
					{ bedN, bedS, null, bedS, bedN },
					{ stor, null, null, null, nic1 },
					{ chai, null, null, null, chai },
					{ tabl, staD, _mat, staU, nSta }, }),
				new SpriteConfig(new Sprite[][] {
					{ nSta, tabl, nic1, nic1, nSta },
					{ bedN, bedS, null, bedS, bedN },
					{ bedN, bedS, null, bedS, bedN },
					{ chai, null, null, null, chai },
					{ nic2, null, null, null, nic2 },
					{ stor, staD, _mat, staU, stor }, }),
				new SpriteConfig(new Sprite[][] {
					{ stor, nic2, nic2, null, stor },
					{ bedN, bedS, null, bedS, bedN },
					{ bedN, bedS, null, bedS, bedN },
					{ nSta, null, null, null, nSta },
					{ chai, null, null, null, chai },
					{ nic1, staD, _mat, staU, tabl }, }),
				
				
				new SpriteConfig(new Sprite[][] {
					{ tabl, stor, nic1, nic1, nic1 },
					{ chai, null, null, null, chai },
					{ bedN, bedS, null, bedS, bedN },
					{ bedN, bedS, null, bedS, bedN },
					{ nSta, null, null, null, nSta },
					{ tabl, staU, _mat, staD, nic2 }, }),
				new SpriteConfig(new Sprite[][] {
					{ nic1, chai, chai, nic1, tabl },
					{ nSta, null, null, null, stor },
					{ bedN, bedS, null, bedS, bedN },
					{ bedN, bedS, null, bedS, bedN },
					{ nic1, null, null, null, nSta },
					{ tabl, staU, _mat, staD, nic1 }, }),
				new SpriteConfig(new Sprite[][] {
					{ nic2, nic1, nic1, nic2, nSta },
					{ nSta, null, null, null, chai },
					{ bedN, bedS, null, bedS, bedN },
					{ bedN, bedS, null, bedS, bedN },
					{ chai, null, null, null, stor },
					{ tabl, staU, _mat, staD, stor }, }),
				new SpriteConfig(new Sprite[][] {
					{ nic2, chai, chai, stor, tabl },
					{ stor, null, null, null, nSta },
					{ bedN, bedS, null, bedS, bedN },
					{ bedN, bedS, null, bedS, bedN },
					{ nSta, null, null, null, nic1 },
					{ nic1, staU, _mat, staD, nic1 }, }),
				new SpriteConfig(new Sprite[][] {
					{ stor, stor, nic2, nic2, nic2 },
					{ nic1, null, null, null, chai },
					{ bedN, bedS, null, bedS, bedN },
					{ bedN, bedS, null, bedS, bedN },
					{ chai, null, null, null, tabl },
					{ nic1, staU, _mat, staD, nic1 }, }),
				
				new SpriteConfig(new Sprite[][] {
					{ nSta, bedN, bedN, bedN, nSta },
					{ nic1, bedS, bedS, bedS, nic2 },
					{ nic1, null, null, null, stor },
					{ nic2, null, null, null, bedN },
					{ stor, null, null, null, bedS },
					{ tabl, staU, _mat, staD, nic2 }, }),
				new SpriteConfig(new Sprite[][] {
					{ nSta, bedN, bedN, bedN, tabl },
					{ nic2, bedS, bedS, bedS, nic1 },
					{ nic2, null, null, null, chai },
					{ stor, null, null, null, bedN },
					{ chai, null, null, null, bedS },
					{ nic1, staU, _mat, staD, nSta }, }),
				new SpriteConfig(new Sprite[][] {
					{ stor, bedN, bedN, bedN, nSta },
					{ nic2, bedS, bedS, bedS, nic1 },
					{ chai, null, null, null, nic2 },
					{ chai, null, null, null, bedN },
					{ nic1, null, null, null, bedS },
					{ nSta, staU, _mat, staD, tabl }, }),
				new SpriteConfig(new Sprite[][] {
					{ nSta, bedN, bedN, bedN, nic2 },
					{ nic2, bedS, bedS, bedS, stor },
					{ nic1, null, null, null, chai },
					{ nic1, null, null, null, bedN },
					{ chai, null, null, null, bedS },
					{ tabl, staU, _mat, staD, nSta }, }),
				new SpriteConfig(new Sprite[][] {
					{ nic2, bedN, bedN, bedN, nSta },
					{ tabl, bedS, bedS, bedS, nic1 },
					{ chai, null, null, null, nic2 },
					{ chai, null, null, null, bedN },
					{ tabl, null, null, null, bedS },
					{ nSta, staU, _mat, staD, stor }, }),
				
				
				new SpriteConfig(new Sprite[][] {
					{ nSta, bedN, bedN, nSta, bedN },
					{ nic2, bedS, bedS, null, bedS },
					{ nic1, null, null, null, chai },
					{ nic1, null, null, null, bedS },
					{ chai, null, null, null, bedN },
					{ stor, staU, _mat, staD, nic2 }, }),
				new SpriteConfig(new Sprite[][] {
					{ tabl, bedN, bedN, nic2, bedN },
					{ nSta, bedS, bedS, null, bedS },
					{ chai, null, null, null, nic2 },
					{ chai, null, null, null, bedS },
					{ nic1, null, null, null, bedN },
					{ nic1, staU, _mat, staD, nSta }, }),
				new SpriteConfig(new Sprite[][] {
					{ nic1, bedN, bedN, nic2, bedN },
					{ nic1, bedS, bedS, null, bedS },
					{ chai, null, null, null, tabl },
					{ stor, null, null, null, bedS },
					{ chai, null, null, null, bedN },
					{ nSta, staU, _mat, staD, nSta }, }),
				new SpriteConfig(new Sprite[][] {
					{ stor, bedN, bedN, nic2, bedN },
					{ nic2, bedS, bedS, null, bedS },
					{ chai, null, null, null, nSta },
					{ chai, null, null, null, bedS },
					{ nSta, null, null, null, bedN },
					{ tabl, staU, _mat, staD, nic1 }, }),
				new SpriteConfig(new Sprite[][] {
					{ tabl, bedN, bedN, nSta, bedN },
					{ nic2, bedS, bedS, null, bedS },
					{ chai, null, null, null, stor },
					{ nic2, null, null, null, bedS },
					{ chai, null, null, null, bedN },
					{ nic1, staU, _mat, staD, nSta }, }),
				
			
			});
			
			
			

		}
		
		private SpriteConfig[] mirror(SpriteConfig[] o) {
			
			SpriteConfig[] res = new SpriteConfig[o.length*2];
			
			for (int i = 0; i < o.length; i++) {
				res[i] = o[i];
				Sprite[][] org = o[i].spri;
				
				int h = org.length;
				int w = org[0].length;
				
				Sprite[][] nn = new Sprite[h][w];
				
				for (int y = 0; y < h; y++) {
					for (int x = 0; x < w; x++) {
						nn[y][w-x-1] = org[y][x];
					}
				}
				
				res[i+o.length] = new SpriteConfig(nn);
			}
			
			if (res.length > 0x0FF)
				throw new RuntimeException();
			
			return res;
			
		}
	}
	

		

}
