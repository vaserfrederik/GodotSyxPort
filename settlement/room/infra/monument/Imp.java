package settlement.room.infra.monument;

import java.io.IOException;

import settlement.main.SETT;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSpriteCombo;
import settlement.room.sprite.RoomSpriteXxX;
import settlement.tilemap.terrain.TFortification;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import view.tool.PlacableMessages;

final class Imp extends ROOM_MONUMENT{

	private final Furnisher constructor;
	
	
	public Imp(RoomInitData init, int tindex, String key, RoomCategorySub cat) throws IOException {
		super(init, tindex, key, cat);
		
		this.constructor = new Constructor(this, init);
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	private static class Constructor extends MConstructor {
		
		Constructor(Imp blue, RoomInitData init)
				throws IOException {
			super(blue, init);
			
			for (Json sData : init.data().jsons("SPRITES")) {

				RoomSprite floor = new RoomSpriteCombo(sData, "FLOOR_COMBO") {
					@Override
					protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
						return item.get(rx, ry) != null;
					}
				};

				
				
				if (sData.has("1x1")) {
					RoomSprite ssmall = new RoomSprite1x1(sData, "1x1") {
						@Override
						public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
							return floor.getData(tx, ty, rx, ry, item, itemRan);
						}
						
						@Override
						public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
								boolean isCandle) {
							floor.render(r, s, getData2(it), it, degrade, false);
							return false;
						}
						
						@Override
						public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it,
								double degrade) {
							animate(1.0 - SETT.ROOMS().map.get(it.tile()).getDegrade(it.tx(), it.ty()));
							super.render(r, s, data, it, degrade, false);
						}
						
					};
					FurnisherItemTile ss = new FurnisherItemTile(
							this,
							false,
							ssmall,
							blue.avail,
							false
							);
					new FurnisherItem(new FurnisherItemTile[][] {
						{ss},
					}, 1,1);
				}
				
				if (sData.has("2x2")) {
					
					RoomSprite sp = new RoomSpriteXxX(sData, "2x2", 2){
						@Override
						public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
							return floor.getData(tx, ty, rx, ry, item, itemRan);
						}
						
						@Override
						public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
								boolean isCandle) {
							floor.render(r, s, getData2(it), it, degrade, false);
							return false;
						}
						
						@Override
						public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it,
								double degrade) {
							animate(1.0 - SETT.ROOMS().map.get(it.tile()).getDegrade(it.tx(), it.ty()));
							super.render(r, s, data, it, degrade, false);
						}
					};
					FurnisherItemTile tt = new FurnisherItemTile(
							this,
							false,
							sp,
							blue.avail,
							false
							);
					new FurnisherItem(new FurnisherItemTile[][] {
						{tt,tt},
						{tt,tt},
					}, 6,2);
				}
				
				if (sData.has("3x3")) {
					
					RoomSprite sp = new RoomSpriteXxX(sData, "3x3", 3) {
						@Override
						public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
							return floor.getData(tx, ty, rx, ry, item, itemRan);
						}
						
						@Override
						public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
								boolean isCandle) {
							floor.render(r, s, getData2(it), it, degrade, false);
							return false;
						}
						
						@Override
						public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it,
								double degrade) {
							animate(1.0 - SETT.ROOMS().map.get(it.tile()).getDegrade(it.tx(), it.ty()));
							super.render(r, s, data, it, degrade, false);
						}
					};
					FurnisherItemTile tt = new FurnisherItemTile(
							this,
							false,
							sp,
							blue.avail,
							false
							);
					new FurnisherItem(new FurnisherItemTile[][] {
						{tt,tt,tt},
						{tt,tt,tt},
						{tt,tt,tt},
					}, 12,4);
				}
				
				flush(3);
				
			}
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
		public void putFloor(int tx, int ty, int upgrade, AREA area) {
			// TODO Auto-generated method stub
			super.putFloor(tx, ty, upgrade, area);
		}
	}

	

}
