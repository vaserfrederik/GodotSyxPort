package settlement.room.law.execution;

import java.io.IOException;

import game.time.TIME;
import init.sprite.SPRITES;
import settlement.environment.SettEnvMap.SettEnv;
import settlement.environment.SettEnvMap.SettEnvValue;
import settlement.main.SETT;
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
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	private final ROOM_EXECTUTION blue;
	
	final FurnisherStat prisoners = new FurnisherStat.FurnisherStatI(this, 1);
	
	private final RoomSprite spritePedistal;
	
	protected Constructor(ROOM_EXECTUTION blue, RoomInitData init)
			throws IOException {
		super(init, 4, 1);
		this.blue = blue;

		Json sp = init.data().json("SPRITES");
		
		spritePedistal = new Pedistall(sp);
		gallows(sp);
		chop(sp);
		gibbet(sp);
		cross(sp);
		
	}
	
	private void gallows(Json sp) throws IOException {

		RoomSprite bedge = new PSprite(sp, "GALLOW_BEAM_LEFT_1X1");
		RoomSprite bcentre = new PSprite(sp, "GALLOW_BEAM_CENTRE_1X1");
		RoomSprite bBottom = new PSprite(sp, "GALLOW_BOTTOM_1X1");
		RoomSprite bPillar = new PSprite(sp, "GALLOW_PILLAR_1X1");
		RoomSprite bBox = new RoomSprite1x1(sp, "GALLOW_BOX_1X1");
		
		RoomSprite bNoose = new PSprite(sp, "GALLOW_NOOSE_1X1") {
			
			final int[] animi = new int[] {0,1,2,3,4,5,6,5,4,3,2,1,0,-1,-2,-3,-4,-5,-6,-7,-8,-7,-6,-5,-4,-3,-2,-1};
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				int rot = SETT.ROOMS().fData.item.get(it.tile()).rotation;
				if (!blue.stations.deadORDying(it.tx(), it.ty())) {
					DIR dir = DIR.ORTHO.get(rot);
					
					int ran = it.ran();
					int speed = 4 + (ran&0b111);
					ran = ran >>3;
					double ww = 0.5 + (ran &0x0F)/(double)0x0F;
					ran = ran >>4;
					int i = (ran&0x0FFFF)+(int)(TIME.currentSecond()*speed);
					int d = animi[i%animi.length];
					
					it.setOff((int) (dir.x()*d*ww), (int) (dir.y()*d*ww));
					super.render(r, s, data, it, degrade, isCandle);
					it.setOff(0, 0);
				}
				return false;
			}
		}.sData(1);;
		
		RoomSprite spriteEdge = new Pedistall(sp) {
			

			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				
				bedge.render(r, s, getData2(it), it, degrade, false);
				bPillar.render(r, s, getData2(it), it, degrade, false);

			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				bBottom.render(r, s, getData2(it), it, degrade, false);
				return false;
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return bedge.getData(tx, ty, rx, ry, item, itemRan);
			}
		}.sData(1);
		
		RoomSprite spriteC = new Pedistall(sp) {
			

			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				
				
				bNoose.render(r, s, getData2(it), it, degrade, false);
				bcentre.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				if (!blue.stations.deadORDying(it.tx(), it.ty())) {
					bBox.render(r, s, getData2(it), it, degrade, false);
				}
				return false;
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return bedge.getData(tx, ty, rx, ry, item, itemRan);
			}
		}.sData(1);
		
		final FurnisherItemTile xx = new FurnisherItemTile(
				this,
				spritePedistal, 
				AVAILABILITY.ROOM, false);
		final FurnisherItemTile aa = new FurnisherItemTile(
				this,
				spriteEdge, 
				AVAILABILITY.PENALTY4, false);
		final FurnisherItemTile cc = new FurnisherItemTile(
				this,
				spriteC, 
				AVAILABILITY.PENALTY4, false).setData(ExecutionStation.TYPE_HANG);


		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,xx}, 
			{aa,cc,aa},
			{xx,xx,xx},
		}, 1, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,xx,xx}, 
			{aa,cc,cc,aa},
			{xx,xx,xx,xx},
		}, 1.25, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,xx,xx,xx}, 
			{aa,cc,cc,cc,aa},
			{xx,xx,xx,xx,xx},
		}, 1.5, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,xx,xx,xx,xx}, 
			{aa,cc,cc,cc,cc,aa},
			{xx,xx,xx,xx,xx,xx},
		}, 1.75, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,xx,xx,xx,xx,xx}, 
			{aa,cc,cc,cc,cc,cc,aa},
			{xx,xx,xx,xx,xx,xx,xx},
		}, 2.0, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,xx,xx,xx,xx,xx,xx}, 
			{aa,cc,cc,cc,cc,cc,cc,aa},
			{xx,xx,xx,xx,xx,xx,xx,xx},
		}, 2.25, 6);
		
		
		flush(1);
		
	}
	
	private static class PSprite extends RoomSprite1x1{

		public PSprite(Json json, String key) throws IOException {
			super(json, key);
		}
		
		@Override
		protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
			return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
		}
		
	}
	
	private static class RSprite extends RoomSprite1x1{

		public RSprite(Json json, String key) throws IOException {
			super(json, key);
		}
		
		@Override
		protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
			return d.orthoID() == item.rotation;
		}
		
	}
	
	private void chop(Json sp) throws IOException {
		
		final RoomSprite bchop = new RSprite(sp, "CHOP_TABLE_1X1");
		final RoomSprite bmat = new PSprite(sp, "CHOP_MAT_1X1");
		
		RoomSprite spriteChop = new Pedistal(sp, bchop).sData(1);
		
		RoomSprite spriteKneel = new Pedistal(sp, bmat);
		
		final FurnisherItemTile xx = new FurnisherItemTile(
				this,
				spritePedistal, 
				AVAILABILITY.ROOM, false);
		final FurnisherItemTile mm = new FurnisherItemTile(
				this,
				spriteKneel, 
				AVAILABILITY.AVOID_PASS, false);
		final FurnisherItemTile cc = new FurnisherItemTile(
				this,
				spriteChop, 
				AVAILABILITY.AVOID_PASS, false).setData(ExecutionStation.TYPE_CHOP);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,mm,xx,},
			{xx,cc,xx,},
			{xx,xx,xx,},
		}, 1, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,mm,mm,xx,},
			{xx,cc,cc,xx,},
			{xx,xx,xx,xx,},
		}, 1.25, 2);
	
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,mm,mm,mm,xx,},
			{xx,cc,cc,cc,xx,},
			{xx,xx,xx,xx,xx,},
		}, 1.5, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,mm,mm,mm,mm,xx,},
			{xx,cc,cc,cc,cc,xx,},
			{xx,xx,xx,xx,xx,xx,},
		}, 1.75, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,mm,mm,mm,mm,mm,xx,},
			{xx,cc,cc,cc,cc,cc,xx,},
			{xx,xx,xx,xx,xx,xx,xx,},
		}, 2, 6);
		
		flush(1);
	}
	
	private void gibbet(Json sp) throws IOException {
		
		final RoomSprite bcage = new RSprite(sp, "GIBBET_CAGE_1X1") {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, rotates);
			};
		};
		final RoomSprite bdoor = new PSprite(sp, "GIBBET_DOOR_1X1") {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
				DIR d = rot(data).perpendicular();
				if (!blue.stations.deadORDying(it.tx()+d.x(), it.ty()+d.y())) {
					super.render(r, s, data, it, degrade, isCandle);
				}
				return false;
			};
		};
		
		RoomSprite spriteChop = new Pedistal(sp, bcage);
		
		RoomSprite spriteKneel = new Pedistal(sp, bdoor);
		
		final FurnisherItemTile xx = new FurnisherItemTile(
				this,
				spritePedistal, 
				AVAILABILITY.ROOM, false);
		final FurnisherItemTile mm = new FurnisherItemTile(
				this,
				spriteKneel, 
				AVAILABILITY.AVOID_PASS, false);
		final FurnisherItemTile cc = new FurnisherItemTile(
				this,
				spriteChop, 
				AVAILABILITY.AVOID_PASS, false).setData(ExecutionStation.TYPE_GIBBET);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,mm,xx,},
			{xx,cc,xx,},
			{xx,xx,xx,},
		}, 1, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,mm,mm,xx,},
			{xx,cc,cc,xx,},
			{xx,xx,xx,xx,},
		}, 1.25, 2);
	
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,mm,mm,mm,xx,},
			{xx,cc,cc,cc,xx,},
			{xx,xx,xx,xx,xx,},
		}, 1.5, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,mm,mm,mm,mm,xx,},
			{xx,cc,cc,cc,cc,xx,},
			{xx,xx,xx,xx,xx,xx,},
		}, 1.75, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,mm,mm,mm,mm,mm,xx,},
			{xx,cc,cc,cc,cc,cc,xx,},
			{xx,xx,xx,xx,xx,xx,xx,},
		}, 2, 6);
		
		flush(1);
	}
	
	private void cross(Json sp) throws IOException {
		
		final RoomSprite bcrossR = new Pedistal(sp, new RSprite(sp, "CROSS_R_1X1"));
		final RoomSprite bcrossL = new Pedistal(sp, new RSprite(sp, "CROSS_L_1X1"));
		final RoomSprite bcrossMid = new Pedistal(sp, new RSprite(sp, "CROSS_MID_1X1"));
		
		final RoomSprite bcrossp = new PSprite(sp, "CROSS_POLE_1X1");
		
		RoomSprite bcrossC = new Pedistal(sp, new RSprite(sp, "CROSS_C_1X1")) {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				bcrossp.render(r, s, getData2(it), it, degrade, isCandle);
				return false;
			}
			
		};
		
		final FurnisherItemTile xx = new FurnisherItemTile(
				this,
				spritePedistal, 
				AVAILABILITY.ROOM, false);
		final FurnisherItemTile cc = new FurnisherItemTile(
				this,
				bcrossC, 
				AVAILABILITY.AVOID_PASS, false).setData(ExecutionStation.TYPE_CROSS);
		
		final FurnisherItemTile ll = new FurnisherItemTile(
				this,
				bcrossL, 
				AVAILABILITY.AVOID_PASS, false);
		
		final FurnisherItemTile rr = new FurnisherItemTile(
				this,
				bcrossR, 
				AVAILABILITY.AVOID_PASS, false);
		
		final FurnisherItemTile mm = new FurnisherItemTile(
				this,
				bcrossMid, 
				AVAILABILITY.AVOID_PASS, false);
		
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,xx,},
			{ll,cc,rr,},
			{xx,xx,xx,},
		}, 1, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,xx,xx,xx,},
			{ll,cc,mm,cc,rr,},
			{xx,xx,xx,xx,xx,},
		}, 1.25, 2);
	
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,xx,xx,xx,xx,xx,},
			{ll,cc,mm,cc,mm,cc,rr,},
			{xx,xx,xx,xx,xx,xx,xx,},
		}, 1.5, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,xx,xx,xx,xx,xx,xx,xx,},
			{ll,cc,mm,cc,mm,cc,mm,cc,rr,},
			{xx,xx,xx,xx,xx,xx,xx,xx,xx,},
		}, 1.75, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,xx,xx,xx,xx,xx,xx,xx,xx,xx,},
			{ll,cc,mm,cc,mm,cc,mm,cc,mm,cc,rr,},
			{xx,xx,xx,xx,xx,xx,xx,xx,xx,xx,xx,},
		}, 2, 6);
		
		flush(1);
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
	public boolean mustBeOutdoors() {
		return false;
	}

	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		return blue.instance.place(area);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	
	private static class Pedistall extends RoomSpriteBoxN {
		

		
		public Pedistall(Json json) throws IOException {
			super(json, "PODIUM_BOX");
		}

		@Override
		public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
			super.render(r, s, data, it, degrade, false);
		}
		
		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			return false;
		}

		@Override
		public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
				FurnisherItem item) {
			type().renderOverlay(x, y, r, AVAILABILITY.ROOM, 
					data&0x0F, 0, false);
			SPRITES.cons().fullArrows.render(r, item.rotation, x, y);
		}
		
	}
	
	private static class Pedistal extends RoomSpriteBoxN {
		
		private final RoomSprite top;
		
		public Pedistal(Json json, RoomSprite top) throws IOException {
			super(json, "PODIUM_BOX");
			this.top = top;
		}

		@Override
		public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
			super.render(r, s, data, it, degrade, false);
			top.renderBelow(r, s, data, it, degrade);
		}
		
		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			if (!isCandle) {
				top.render(r, s, getData2(it), it, degrade, false);
			}
			return false;
		}
		
		@Override
		public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
			top.renderAbove(r, s, data, it, degrade);
		}
		
		@Override
		public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			return top.getData(tx, ty, rx, ry, item, itemRan);
		}
		
		@Override
		public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
				FurnisherItem item) {
			type().renderOverlay(x, y, r, AVAILABILITY.ROOM, 
					data&0x0F, 0, false);
			SPRITES.cons().fullArrows.render(r, item.rotation, x, y);
		}
		
	}
	
	@Override
	public void renderExtra(SPRITE_RENDERER r, int x, int y, int tx, int ty, int rx, int ry, FurnisherItem item) {
		
		if (item.get(rx, ry).data() > 0) {
			double v = envValue[SETT.ENV().map.PUNISHMENT.index()];
			double ra = envRadius[SETT.ENV().map.PUNISHMENT.index()];
			
			SETT.ENV().map.PUNISHMENT.addExtraView(v, ra, tx, ty, -1);
			SETT.OVERLAY().envThing(SETT.ENV().map.PUNISHMENT).add();
		}
		
	}

	@Override
	public boolean envValue(SettEnv e, SettEnvValue v, int tx, int ty) {
		if (envRadius[e.index()] == 0)
			return false;
		if (SETT.ROOMS().fData.tileData.get(tx, ty) > 0) {
			return super.envValue(e, v, tx, ty);
		}
		return false;
	}

}
