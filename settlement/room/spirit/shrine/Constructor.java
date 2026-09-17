package settlement.room.spirit.shrine;

import java.io.IOException;

import init.constant.C;
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
import settlement.room.sprite.RoomSpriteBoxN;
import settlement.room.sprite.RoomSpriteTex;
import settlement.room.sprite.RoomSpriteXxX;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	private final ROOM_SHRINE blue;
	
	final FurnisherStat services = new FurnisherStat.FurnisherStatI(this);
	

	
	static final int codeService = 1;
	static final int codeFire = 2;

//	protected Constructor(ROOM_SHRINE blue, RoomInitData init)
//			throws IOException {
//		super(init, 1, 1);
//		this.blue = blue;
//		
//		Json sp = init.data().json("SPRITES");
//		
//	
//		final RoomSprite s2 = new RoomSpriteBoxN(sp, "INNER_BOX")  {
//			@Override
//			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
//				return item.sprite(rx, ry) instanceof RoomSpriteBoxN;
//			};
//			
//			@Override
//			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
//				super.render(r, s, data, it, degrade, false);
//			}
//			
//			@Override
//			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
//					boolean isCandle) {
//				return false;
//			}
//		};
//		
//		final RoomSprite s3 = new RoomSpriteBoxN(sp, "INNER_BOX")  {
//			@Override
//			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
//				return item.sprite(rx, ry) == this || (item.sprite(rx, ry) != null &&  item.sprite(rx, ry).sData() == 99);
//			};
//			
//			@Override
//			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
//				super.render(r, s, data, it, degrade, false);
//			}
//			
//			@Override
//			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
//					boolean isCandle) {
//				return false;
//			}
//		};
//		
//		final RoomSprite sInscript = new RoomSpriteTex(sp, "ALTAR_FLOOR_TEXTURE") {
//			
//			RoomSprite sc1 = new RoomSpriteTex(sp, "ALTAR_FLOOR_SCRIBBLE1_TEXTURE");
//			RoomSprite sc2 = new RoomSpriteTex(sp, "ALTAR_FLOOR_SCRIBBLE2_TEXTURE");
//			
//			@Override
//			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
//					boolean isCandle) {
//				return false;
//			}
//			
//			
//			
//			@Override
//			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
//				
//				it.ranOffset(1, 0);
//				super.render(r, s, data, it, degrade, rotates);
//				it.ranOffset(2, 0);
//				sc1.render(r, s, 0, it, degrade, false);
//				it.ranOffset(3, 0);
//				sc2.render(r, s, 0, it, degrade, false);
//			}
//		};
//		
//		final RoomSprite sInscriptA = new RoomSpriteBoxN(s2) {
//
//			@Override
//			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
//					boolean isCandle) {
//				return false;
//			}
//			
//			
//			
//			@Override
//			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
//				super.render(r, s, data, it, degrade, rotates);
//				sInscript.renderBelow(r, s, data, it, degrade);
//				
//			}
//		};
//		
//		final RoomSprite sAltarTorch = new RoomSpriteBoxN(s2)   {
//			
//			final RoomSprite1x1 torch = new RoomSprite1x1(sp, "TORCH_1X1");
//			
//			@Override
//			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
//				return item.sprite(rx, ry) instanceof RoomSpriteBoxN;
//			};
//			
//			@Override
//			public  byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
//				return torch.getData(tx, ty, rx, ry, item, itemRan);
//			};
//			@Override
//			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
//				super.render(r, s, data, it, degrade, false);
//			}
//			
//			@Override
//			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
//					boolean isCandle) {
//				torch.render(r, s, getData2(it), it, degrade, false);
//				return false;
//			}
//		};
//		final RoomSprite sAltar = new RoomSpriteBoxN(sp, "ALTAR_BOX")   {
//			
//			
//			@Override
//			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
//				return item.sprite(rx, ry) != null &&  item.sprite(rx, ry).sData() == 99;
//			};
//			
//			@Override
//			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
//				s2.renderBelow(r, s, 0x0F, it, degrade);
//				sInscript.renderBelow(r, s, 0, it, degrade);
//			};
//		}.sData(99);
//
//		
//		
//		final RoomSprite sAltarRelief = new RoomSpriteXxX(sp, "EMBLEM_2X2", 2)   {
//			
//			@Override
//			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
//				sAltar.render(r, s, 0x0F, it, degrade, isCandle);
//				super.render(r, s, data, it, degrade, isCandle);
//				return false;
//			};
//			
//			@Override
//			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
//				s2.renderBelow(r, s, 0x0F, it, degrade);
//				sInscript.renderBelow(r, s, 0, it, degrade);
//			};
//			
//		}.sData(99);
//		RoomSprite1x1 sAltarReliefSmall = new RoomSprite1x1(sp, "EMBLEM_1X1") {
//			@Override
//			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
//				return d.orthoID() == item.rotation;
//			}
//		};
//		final RoomSprite sAltarReliefSmall1 = new ASmall(sInscriptA, sAltar, sAltarReliefSmall, -2).sData(99);
//		final RoomSprite sAltarReliefSmall2 = new ASmall(sInscriptA, sAltar, sAltarReliefSmall, 2).sData(99);
//		
//		final FurnisherItemTile c1 = new FurnisherItemTile(
//				this,
//				sAltarReliefSmall1,
//				AVAILABILITY.SOLID, 
//				false);
//		
//		final FurnisherItemTile c2 = new FurnisherItemTile(
//				this,
//				sAltarReliefSmall2,
//				AVAILABILITY.SOLID, 
//				false);
//		
//		final FurnisherItemTile cc = new FurnisherItemTile(
//				this,
//				sAltarRelief,
//				AVAILABILITY.SOLID, 
//				false);
//		final FurnisherItemTile tt = new FurnisherItemTile(
//				this,
//				sAltarTorch,
//				AVAILABILITY.SOLID, 
//				false).setData(codeFire);
//		final FurnisherItemTile aa = new FurnisherItemTile(
//				this,
//				sAltar,
//				AVAILABILITY.SOLID, 
//				false);
//		
//		final FurnisherItemTile oo = new FurnisherItemTile(
//				this,
//				s2,
//				AVAILABILITY.ROOM, 
//				false);
//		
//		final FurnisherItemTile o2 = new FurnisherItemTile(
//				this,
//				s3,
//				AVAILABILITY.ROOM, 
//				false);
//		
//		final FurnisherItemTile __ = new FurnisherItemTile(
//				this,
//				null,
//				AVAILABILITY.ROOM, 
//				false);
//		
//		final FurnisherItemTile _x = new FurnisherItemTile(
//				this,
//				sInscript,
//				AVAILABILITY.ROOM, 
//				false);
//		
//		final FurnisherItemTile xx = new FurnisherItemTile(
//				this,
//				sInscriptA,
//				AVAILABILITY.ROOM, 
//				false);
//		
//		make(new FurnisherItemTile[][] {
//
//			{_x,_x,__,__,_x,_x},
//			{oo,oo,oo,oo,oo,oo},
//			{oo,xx,aa,aa,xx,oo},
//			{oo,xx,c1,c2,xx,oo},
//			{oo,xx,aa,aa,xx,oo},
//			{tt,oo,oo,oo,oo,tt},
//
//		});
//		
//		make(new FurnisherItemTile[][] {
//
//			{_x,_x,__,__,__,__,_x,_x},
//			{_x,oo,oo,oo,oo,oo,oo,_x},
//			{__,oo,xx,aa,aa,xx,oo,__},
//			{__,oo,xx,c1,c2,xx,oo,__},
//			{__,oo,xx,aa,aa,xx,oo,__},
//			{_x,tt,oo,oo,oo,oo,tt,_x},
//			{_x,_x,__,__,__,__,_x,_x},
//
//		});
//		
//		make(new FurnisherItemTile[][] {
//
//			{_x,_x,_x,__,__,__,__,_x,_x,_x},
//			{_x,oo,oo,oo,oo,oo,oo,oo,oo,_x},
//			{_x,oo,xx,aa,aa,aa,aa,xx,oo,_x},
//			{__,oo,xx,aa,cc,cc,aa,xx,oo,__},
//			{__,oo,xx,aa,cc,cc,aa,xx,oo,__},
//			{_x,oo,xx,aa,aa,aa,aa,xx,oo,_x},
//			{_x,tt,oo,oo,oo,oo,oo,oo,tt,_x},
//			{_x,_x,_x,__,__,__,__,_x,_x,_x},
//
//		});
//		
//		make(new FurnisherItemTile[][] {
//
//			{_x,_x,_x,_x,__,__,__,__,_x,_x,_x,_x},
//			{_x,oo,oo,oo,oo,oo,oo,oo,oo,oo,oo,_x},
//			{_x,oo,oo,oo,oo,oo,oo,oo,oo,oo,oo,_x},
//			{_x,oo,oo,xx,aa,aa,aa,aa,xx,oo,oo,_x},
//			{__,oo,oo,xx,aa,cc,cc,aa,xx,oo,oo,__},
//			{__,oo,oo,xx,aa,cc,cc,aa,xx,oo,oo,__},
//			{_x,oo,oo,xx,aa,aa,aa,aa,xx,oo,oo,_x},
//			{_x,oo,tt,oo,oo,oo,oo,oo,oo,tt,oo,_x},
//			{_x,oo,oo,oo,oo,oo,oo,oo,oo,oo,oo,_x},
//			{_x,_x,_x,_x,__,__,__,__,_x,_x,_x,_x},
//
//		});
//		
//		flush(1, 3);
//		
//	}
	
	protected Constructor(ROOM_SHRINE blue, RoomInitData init)
			throws IOException {
		super(init, 1, 1);
		this.blue = blue;
		
		Json sp = init.data().json("SPRITES");
		
		final RoomSprite sClear = RoomSprite.DUMMY;
		
		final RoomSprite sScribble = new RoomSpriteTex(sp, "ALTAR_FLOOR_TEXTURE") {
		
			RoomSprite sc1 = new RoomSpriteTex(sp, "ALTAR_FLOOR_SCRIBBLE1_TEXTURE");
			RoomSprite sc2 = new RoomSpriteTex(sp, "ALTAR_FLOOR_SCRIBBLE2_TEXTURE");
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
			
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				
				it.ranOffset(1, 0);
				super.render(r, s, data, it, degrade, rotates);
				it.ranOffset(2, 0);
				sc1.render(r, s, 0, it, degrade, false);
				it.ranOffset(3, 0);
				sc2.render(r, s, 0, it, degrade, false);
			}
		};
		
		final RoomSprite sStairs1 = new SStairs(1, sp);
		
		final RoomSprite sAltarNormal = new RoomSpriteBoxN(sp, "ALTAR_BOX")   {
		
		
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null &&  item.sprite(rx, ry).sData() == 99;
			};
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				sStairs1.renderBelow(r, s, 0x0F, it, degrade);
				sScribble.renderBelow(r, s, 0, it, degrade);
			};
		}.sData(99);
		
		final FurnisherItemTile aa = new FurnisherItemTile(
				this,
				sAltarNormal,
				AVAILABILITY.SOLID, 
				false);
		
		final FurnisherItemTile __ = new FurnisherItemTile(
				this,
				sClear,
				AVAILABILITY.ROOM, 
				false);
		
		final FurnisherItemTile _x = new FurnisherItemTile(
				this,
				sScribble,
				AVAILABILITY.ROOM, 
				false);
		
		final FurnisherItemTile oo = new FurnisherItemTile(
				this,
				sStairs1,
				AVAILABILITY.ROOM, 
				false);
		
		{
			
			

			
			final RoomSprite sAltarTorch = new SStairs(1, sp)   {
			
				final RoomSprite1x1 torch = new RoomSprite1x1(sp, "TORCH_1X1");
				
				@Override
				public  byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
					return torch.getData(tx, ty, rx, ry, item, itemRan);
				};
				
				@Override
				public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
						boolean isCandle) {
					torch.render(r, s, getData2(it), it, degrade, false);
					return false;
				}
			}.sData(1);
			
			final RoomSprite sAltarScrib = new SScribbleAltar(2, sp, sStairs1);
			
			RoomSprite1x1 sAltarReliefSmall = new RoomSprite1x1(sp, "EMBLEM_1X1") {
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return d.orthoID() == item.rotation;
				}
			};
			
			final RoomSprite sAltarReliefSmall1 = new ASmall(new SScribbleAltar(1, sp, sAltarNormal), sAltarNormal, sAltarReliefSmall, -2).sData(99);
			final RoomSprite sAltarReliefSmall2 = new ASmall(new SScribbleAltar(1, sp, sAltarNormal), sAltarNormal, sAltarReliefSmall, 2).sData(99);
			
			final FurnisherItemTile tt = new FurnisherItemTile(
					this,
					sAltarTorch,
					AVAILABILITY.SOLID, 
					false).setData(codeFire);
			
			final FurnisherItemTile xx = new FurnisherItemTile(
					this,
					sAltarScrib,
					AVAILABILITY.ROOM, 
					false);
			
			final FurnisherItemTile c1 = new FurnisherItemTile(
					this,
					sAltarReliefSmall1,
					AVAILABILITY.SOLID, 
					false);
			
			final FurnisherItemTile c2 = new FurnisherItemTile(
					this,
					sAltarReliefSmall2,
					AVAILABILITY.SOLID, 
					false);
			
			make(new FurnisherItemTile[][] {

				{_x,_x,__,__,_x,_x},
				{oo,oo,oo,oo,oo,oo},
				{oo,xx,aa,aa,xx,oo},
				{oo,tt,c1,c2,tt,oo},
				{oo,xx,aa,aa,xx,oo},
				{oo,oo,oo,oo,oo,oo},

			});
			
			make(new FurnisherItemTile[][] {

				{_x,_x,__,__,_x,_x},
				{oo,oo,oo,oo,oo,oo},
				{oo,xx,aa,aa,xx,oo},
				{oo,tt,c1,c2,tt,oo},
				{oo,xx,aa,aa,xx,oo},
				{oo,oo,oo,oo,oo,oo},
				{_x,_x,__,__,_x,_x},

			});
			
			make(new FurnisherItemTile[][] {

				{_x,_x,__,__,__,__,_x,_x},
				{_x,oo,oo,oo,oo,oo,oo,_x},
				{__,oo,xx,aa,aa,xx,oo,__},
				{__,tt,xx,c1,c2,xx,tt,__},
				{__,oo,xx,aa,aa,xx,oo,__},
				{_x,oo,oo,oo,oo,oo,oo,_x},
				{_x,_x,__,__,__,__,_x,_x},

			});
			
		}
		
		{
			
			final RoomSprite sAltarTorch = new SStairs(1, sp)   {
				
				final RoomSprite1x1 torch = new RoomSprite1x1(sp, "TORCH_1X1");
				
				@Override
				public  byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
					return torch.getData(tx, ty, rx, ry, item, itemRan);
				};
				
				@Override
				public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
						boolean isCandle) {
					torch.render(r, s, getData2(it), it, degrade, false);
					return false;
				}
			}.sData(1);

			
			final RoomSprite sAltarRelief = new RoomSpriteXxX(sp, "EMBLEM_2X2", 2)   {
				
				@Override
				public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
					sAltarNormal.render(r, s, 0x0F, it, degrade, isCandle);
					super.render(r, s, data, it, degrade, isCandle);
					return false;
				};
				
			}.sData(99);

			final FurnisherItemTile tt = new FurnisherItemTile(
					this,
					sAltarTorch,
					AVAILABILITY.SOLID, 
					false).setData(codeFire);
			
			final FurnisherItemTile o2 = new FurnisherItemTile(
					this,
					new SStairs(2, sp),
					AVAILABILITY.ROOM, 
					false);
			
			final FurnisherItemTile xx = new FurnisherItemTile(
					this,
					new SScribbleAltar(2, sp, sStairs1),
					AVAILABILITY.ROOM, 
					false);
			final FurnisherItemTile cc = new FurnisherItemTile(
				this,
				sAltarRelief,
				AVAILABILITY.SOLID, 
				false);
			make(new FurnisherItemTile[][] {
			
				{_x,_x,_x,__,__,__,__,_x,_x,_x},
				{_x,oo,oo,oo,oo,oo,oo,oo,oo,_x},
				{_x,oo,xx,aa,aa,aa,aa,xx,oo,_x},
				{__,oo,xx,aa,cc,cc,aa,xx,oo,__},
				{__,oo,xx,aa,cc,cc,aa,xx,oo,__},
				{_x,oo,xx,aa,aa,aa,aa,xx,oo,_x},
				{_x,tt,oo,oo,oo,oo,oo,oo,tt,_x},
				{_x,_x,_x,__,__,__,__,_x,_x,_x},
			});
					
			make(new FurnisherItemTile[][] {
				{_x,_x,_x,_x,__,__,__,__,_x,_x,_x,_x},
				{_x,oo,oo,oo,oo,oo,oo,oo,oo,oo,oo,_x},
				{_x,oo,o2,o2,o2,o2,o2,o2,o2,o2,oo,_x},
				{_x,oo,o2,xx,aa,aa,aa,aa,xx,o2,oo,_x},
				{__,oo,o2,xx,aa,cc,cc,aa,xx,o2,oo,__},
				{__,oo,o2,xx,aa,cc,cc,aa,xx,o2,oo,__},
				{_x,oo,o2,xx,aa,aa,aa,aa,xx,o2,oo,_x},
				{_x,oo,o2,o2,o2,o2,o2,o2,o2,o2,oo,_x},
				{_x,tt,oo,oo,oo,oo,oo,oo,oo,oo,tt,_x},
				{_x,_x,_x,_x,__,__,__,__,_x,_x,_x,_x},
			});			
		}
		
		
		
		flush(1, 3);
		
	}
	
	private void make(FurnisherItemTile[][] ttt) {
		
		int am = 0;
		for (FurnisherItemTile[] tt : ttt)
			for (FurnisherItemTile t : tt)
				if (t.availability.player > 0)
					am++;
		double cost = am*0.75;
		new FurnisherItem(ttt, cost, am);
	}

	private static class ASmall extends RoomSpriteBoxN {

		private final RoomSprite1x1 sAltarReliefSmall;
		private final RoomSprite s2;
		private int off;
		public ASmall(RoomSprite s2, RoomSprite saltar, RoomSprite1x1 sAltarReliefSmall, int off) throws IOException {
			super(saltar);
			this.sAltarReliefSmall = sAltarReliefSmall;
			this.s2 = s2;
			this.off = off;
		}
		
		@Override
		protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
			return item.sprite(rx, ry) != null &&  item.sprite(rx, ry).sData() == 99;
		};
		
		@Override
		public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
			s2.renderBelow(r, s, 0x0F, it, degrade);
		};
		
		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
			super.render(r, s, data, it, degrade, isCandle);
			DIR dd = sAltarReliefSmall.rot(getData2(it)).next(-off);
			it.setOff(dd.x()*C.TILE_SIZEH, dd.y()*C.TILE_SIZEH);
			sAltarReliefSmall.render(r, s, getData2(it), it, degrade, false);
			return false;
		};
		
		@Override
		public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			return sAltarReliefSmall.getData(tx, ty, rx, ry, item, itemRan);
		};
		
	}
	
	private static class SStairs extends RoomSpriteBoxN{
		
		public SStairs(int level, Json sp) throws IOException {
			super(sp, "INNER_BOX");
			sData(level);
		}
		
		@Override
		protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
			return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() >= sData();
		}
		
		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			return false;
		}
		
		@Override
		public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
			if (sData() > 1)
				super.render(r, s, 0x0F, it, degrade, false);
			super.render(r, s, data, it, degrade, false);
		}
		
	}
	
	private static class SScribbleAltar extends RoomSpriteTex{
		
		RoomSprite sc1;
		RoomSprite sc2;
		final RoomSprite sAltarNormal;
		
		public SScribbleAltar(int level, Json sp, RoomSprite sAltarNormal) throws IOException {
			super(sp, "ALTAR_FLOOR_TEXTURE");
			sData(level);
			sc1 = new RoomSpriteTex(sp, "ALTAR_FLOOR_SCRIBBLE1_TEXTURE");
			sc2 = new RoomSpriteTex(sp, "ALTAR_FLOOR_SCRIBBLE2_TEXTURE");
			this.sAltarNormal = sAltarNormal;
		}
		
		
		
		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			return false;
		}
		
		
		
		@Override
		public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
			
			sAltarNormal.renderBelow(r, s, 0x0F, it, degrade);
			it.ranOffset(1, 0);
			super.render(r, s, data, it, degrade, false);
			it.ranOffset(2, 0);
			sc1.render(r, s, 0, it, degrade, false);
			it.ranOffset(3, 0);
			sc2.render(r, s, 0, it, degrade, false);
		}
		
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
		return new ShrineInstance(blue, area, init);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}

}
