package settlement.room.military.artillery;

import java.io.IOException;

import init.resources.RESOURCES;
import init.sprite.SPRITES;
import init.sprite.game.SheetPair;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSpriteXxX;
import settlement.tilemap.terrain.TFortification;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import view.tool.PlacableMessages;


abstract class Constructor extends Furnisher{
	
	private final ROOM_ARTILLERY blue;
	final static int SERVICE = 1;

	protected Constructor(RoomInitData init, ROOM_ARTILLERY blue) throws IOException {
		super(init, 1, 0);
		this.blue = blue;
		Json js = init.data().json("SPRITES");
		
		RoomSpriteXxX sArm = new RoomSpriteXxX(js, "ARM_2X2", 2) {
			
			RoomSprite srot = new RoomSpriteXxX(js, "ARM_ROT_2X2", 2);
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				int rot = 0;
				ArtilleryInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null) {
					rot = ins.dirCurrent().id();
				}
				
				data = setRot(data, rot/2);
				
				if ((rot & 1) == 1) {
					return srot.render(r, s, data, it, degrade, isCandle);
				}else {
					return super.render(r, s, data, it, degrade, isCandle);
				}
			}
			
			@Override
			public int frame(SheetPair a, RenderIterator it) {
				ArtilleryInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null) {
					if (ins.isLoaded)
						return 0;
					
				}
				return 1;
			}
			
		};
		
		RoomSpriteXxX sBase = new RoomSpriteXxX(js, "BASE_2X2", 2) {
			
			RoomSprite srot = new RoomSpriteXxX(js, "BASE_ROT_2X2", 2);
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				int rot = 0;
				ArtilleryInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null) {
					rot = ins.dirCurrent().id();
				}
				
				data = setRot(data, rot/2);
				
				if ((rot & 1) == 1) {
					srot.render(r, s, data, it, degrade, isCandle);
				}else {
					super.render(r, s, data, it, degrade, isCandle);
				}
				sArm.render(r, s, data, it, degrade, isCandle);
				return false;
			}
			
			@Override
			public int frame(SheetPair a, RenderIterator it) {
				ArtilleryInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null) {
					return ((int)(ins.progress()*32)&1);
				}
				return 0;
			}
			
		};

		
		RoomSprite sDep = new RoomSprite1x1(js, "STORAGE_1X1") {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				RESOURCES.STONE().renderLaying(r, it.x(), it.y(), it.ran(), 64);
				return false;
			}
		};
		
		RoomSprite sca = new RoomSprite1x1(js, "TORCH_1X1") {
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().ICO.arrows.get(rot(data).orthoID()).render(r, x, y);
			}
			
		};
		
		RoomSprite sDummy = new RoomSprite.Dummy();
		
		
		FurnisherItemTile xx = new FurnisherItemTile(this, false, sBase, AVAILABILITY.SOLID, false);
		FurnisherItemTile ca = new FurnisherItemTile(this, false, sca, AVAILABILITY.SOLID, true);
		FurnisherItemTile dp = new FurnisherItemTile(this, false, sDep, AVAILABILITY.ROOM, false);
		FurnisherItemTile __ = new FurnisherItemTile(this, false, sDummy, AVAILABILITY.ROOM, false);
		FurnisherItemTile ee = new FurnisherItemTile(this, true, sDummy, AVAILABILITY.ROOM, false);
		__.setData(SERVICE);
		ee.setData(SERVICE);
		
		
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{ca,xx,xx,dp},
					{__,xx,xx,__},
					{__,ee,ee,__},
				},
				1.0, 1.0);
		
		flush(1, 3);
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
		return true;
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
		return null;
	}
	
	@Override
	public ROOM_ARTILLERY blue() {
		return blue;
	}
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		
	}

}
