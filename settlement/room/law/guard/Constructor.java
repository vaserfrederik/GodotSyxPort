package settlement.room.law.guard;

import java.io.IOException;

import init.sprite.SPRITES;
import settlement.environment.SettEnvMap.SettEnv;
import settlement.environment.SettEnvMap.SettEnvValue;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSpriteBoxN;
import settlement.room.sprite.RoomSpriteCombo;
import settlement.tilemap.terrain.TFortification;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.rnd.RND;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import view.tool.PlacableMessages;

final class Constructor extends Furnisher{

	private final ROOM_GUARD blue;
	static final int codeLight = 3;
	static final int codeStand = 4;
	private final FurnisherItemTile xx;
	private final FurnisherItemTile gg;
	
	final FurnisherStat guards = new FurnisherStat(this) {
		
		@Override
		public double get(AREA area, double acc) {
			return Math.ceil(acc);
		}
		
		@Override
		public GText format(GText t, double value) {
			GFORMAT.i(t, (int) (value));
			return t;
		}
	};
	
	protected Constructor(ROOM_GUARD blue, RoomInitData init)
			throws IOException {
		super(init, 2, 1, 88, 44);
		this.blue = blue;
		
		Json js = init.data().json("SPRITES");
		


		
		
		RoomSprite sPodium = new RoomSpriteBoxN(js, "FLOOR_BOX") {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
		};
		
		RoomSprite top = new RoomSpriteCombo(js, "CARPET_COMBO") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 3;
			}
		};
		
		RoomSprite sCarpeted = new RoomSpriteBoxN(sPodium) {
			

			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				
				return top.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}

			
		}.sData(3);
		
		RoomSprite sCarpetedRot = new RoomSpriteBoxN(sPodium) {
			
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return top.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item) {
				super.renderPlaceholder(r, x, y, data, tx, ty, rx, ry, item);
				DIR d = DIR.ORTHO.getC(item.rotation+1);
				SPRITES.cons().fullArrows.render(r, d.orthoID(), x, y);
			};

			
		}.sData(3);

		RoomSprite sbraiser = new RoomSpriteBoxN(sPodium) {
			
			RoomSprite sBottom = new RoomSpriteCombo(js, "TORCH_BOTTOM_COMBO") {
				
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 2;
				}
				
			};
			
			RoomSprite top = new RoomSpriteCombo(js, "TORCH_COMBO") {
				
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 2;
				}
				
			};
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return top.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
				sBottom.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}

			
		}.sData(2);
		
		
		

		RoomSprite sFence = new RoomSpriteBoxN(sPodium) {
			
			RoomSprite top = new RoomSpriteCombo(js, "FENCE_COMBO") {
				
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
				}
				
			};
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return top.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		}.sData(1);
				
		
		xx = new FurnisherItemTile(
				this,
				sFence,
				AVAILABILITY.SOLID, 
				false);
		
		FurnisherItemTile op = new FurnisherItemTile(
				this,
				sPodium,
				AVAILABILITY.ROOM, 
				false);
		
		FurnisherItemTile __ = new FurnisherItemTile(
				this,
				sCarpeted,
				AVAILABILITY.ROOM, 
				false).setData(codeStand);
		
		gg = new FurnisherItemTile(
				this,
				sCarpetedRot,
				AVAILABILITY.ROOM, 
				false).setData(codeStand);
		
		FurnisherItemTile i1 = new FurnisherItemTile(
				this,
				sbraiser,
				AVAILABILITY.SOLID, 
				false).setData(codeLight);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,op,op,}, 
			{xx,gg,op,},
			{xx,op,op},
		}, 9, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,op,op,}, 
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,op,op},
		}, 12, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,op,op,}, 
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,op,op},
		}, 15, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,op,op,}, 
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,op,op},
		}, 18, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,op,op,}, 
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,op,op},
		}, 21, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,op,op,}, 
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,op,op},
		}, 24, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,op,op,}, 
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,op,op},
		}, 27, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,op,op,}, 
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,gg,op,},
			{xx,op,op},
		}, 30, 8);
		
		
		flush(1, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] { 
			{op,op,op},
			{__,i1,__},
			{op,xx,op},
		}, 9, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,op,xx,xx}, 
			{xx,__,op,__,xx},
			{op,__,i1,__,op},
			{xx,__,op,__,xx},
			{xx,xx,op,xx,xx},
		}, 25, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,op,op,xx,xx}, 
			{xx,op,__,__,op,xx},
			{op,__,i1,i1,__,op},
			{op,__,i1,i1,__,op},
			{xx,op,__,__,op,xx},
			{xx,xx,op,op,xx,xx},
		}, 36, 9);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{xx,xx,xx,op,op,xx,xx,xx}, 
			{xx,__,__,__,__,__,__,xx},
			{xx,__,op,op,op,op,__,xx},
			{op,__,op,i1,i1,op,__,op},
			{op,__,op,i1,i1,op,__,op},
			{xx,__,op,op,op,op,__,xx},
			{xx,__,__,__,__,__,__,xx},
			{xx,xx,xx,op,op,xx,xx,xx},
		}, 64, 18);
		
		flush(1, 3);
	}
	
	@Override
	public void renderExtra(SPRITE_RENDERER r, int x, int y, int tx, int ty, int rx, int ry, FurnisherItem item) {
		
		
		if (item.get(rx, ry) == gg) {
			DIR d = DIR.ORTHO.getC(item.rotation + 1);
			int approvedDirs = d.bit | d.next(-1).bit | d.next(1).bit;
			SETT.ENV().map.GUARD.addExtraView(2.0/15.0, 1.0, tx, ty, approvedDirs);
			SETT.OVERLAY().envThing(SETT.ENV().map.GUARD).add();
		}else if (item.get(rx, ry).data() == codeStand) {
			SETT.ENV().map.GUARD.addExtraView(1.0/15.0, 0.75, tx, ty, -1);
			SETT.OVERLAY().envThing(SETT.ENV().map.GUARD).add();
		}
		
	}
	
	@Override
	public boolean envValue(SettEnv e) {
		return super.envValue(e);
	}
	
	@Override
	public boolean envValue(SettEnv e, SettEnvValue v, int tx, int ty) {
		if (envRadius[e.index()] == 0)
			return false;
		if (SETT.ROOMS().fData.tileData.get(tx, ty) == codeStand) {
			GuardInstance ins = blue.get(tx, ty);
			if (ins != null && ins.eff > 0) {
				if (SETT.ROOMS().fData.tile.get(tx, ty) == gg) {
					DIR d = DIR.ORTHO.getC(SETT.ROOMS().fData.item.get(tx, ty).rotation + 1);
					v.approvedDirs = d.bit | d.next(-1).bit | d.next(1).bit;
					v.value =  2.0/15.0;
					v.radius = ins.eff*1.0;
				}else {
					v.radius = ins.eff*0.75;
					v.value = 1.0/15.0;
				}
				return true;
			}else {
				v.value = 0;
				v.radius = 0;
				return false;
			}
			
				
			
			
		}
		return false;
	}
	
	public DIR gaurdDir(int tx, int ty) {
		if (SETT.ROOMS().fData.tileData.get(tx, ty) == codeStand) {
			
			if (SETT.ROOMS().fData.tile.get(tx, ty) == gg) {
				DIR d = DIR.ORTHO.getC(SETT.ROOMS().fData.item.get(tx, ty).rotation + 1);
				return d;
			}else {
				RoomInstance ins = blue.get(tx, ty);
				return DIR.get(ins.body().cX(), ins.body().cY(),tx, ty).next(-1 + RND.rInt(3));

			}
		}
		return DIR.N;
	}
	
	@Override
	public boolean removeTerrain(int tx, int ty) {
		if (SETT.TERRAIN().get(tx, ty) instanceof TFortification.Normal && SETT.PATH().availability.get(tx, ty).player >= 0)
			return false;
		return super.removeTerrain(tx, ty);
	}
	
	@Override
	public CharSequence placable(int tx, int ty, FurnisherItem item, FurnisherItemTile tile) {
		if (SETT.TERRAIN().get(tx, ty) instanceof TFortification.Normal && SETT.PATH().availability.get(tx, ty).player < 0)
			return PlacableMessages.¤¤STRUCTURE_BLOCK;
		return super.placable(tx, ty, item, tile);
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
		return new GuardInstance(blue, area, init);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		super.putFloor(tx, ty, upgrade, area);
	}


	


	
	

}
