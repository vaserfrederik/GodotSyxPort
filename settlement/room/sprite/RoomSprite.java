package settlement.room.sprite;

import init.sprite.SPRITES;
import settlement.main.SETT;
import settlement.room.main.furnisher.FurnisherItem;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.OPACITY;
import snake2d.util.sprite.TILE_SHEET;
import util.rendering.RenderData;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public interface RoomSprite{
	
	public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderData.RenderIterator it, double degrade, boolean isCandle);
	
	public default void renderBroken(SPRITE_RENDERER r, ShadowBatch s, int x, int y, RenderData.RenderIterator it, FurnisherItem item) {
		for (int i = 0; i < item.group().blueprint.resources(); i++) {
			int a = item.brokenResourceAmount(i);
			if (a > 0) {
				item.group().blueprint.resource(i).renderDebris(r, s, x, y, it.ran()>>i, a);
			}
		}
	}
	
//	public default void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, FurnisherItemTile item) {
//		SPRITES.cons().BIG.outline_dashed_small.render(r, 0, x, y);
//	}
	public default void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item) {
		SPRITES.cons().BIG.dashed_hollow.render(r, 0, x, y);
	}
	public default void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderData.RenderIterator it, double degrade) {
		
	}
	public default void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderData.RenderIterator it, double degrade) {
		
	}
	public default int rotation(int data, FurnisherItem item) {
		if (item.group().rotations() > 2)
			return item.rotation + 1;
		return 0;
	}
	
	
	public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan);
	
	public default byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
		return 0;
	}
	
	public int sData();
	
	class Dummy implements RoomSprite {

		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch shadowBatch, int data, RenderData.RenderIterator it, double degrade, boolean isCandle) {
			return false;
		}

		@Override
		public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			// TODO Auto-generated method stub
			return 0;
		}

		@Override
		public int sData() {
			return 0;
		}
		
	}
	
	public static final Dummy DUMMY = new Dummy();
	
	public abstract class Imp implements RoomSprite {
		
		protected int shadowDist = 3,shadowHeight=0;
		private int sData = 0;
		
		public Imp(){
			
		}
		
		public Imp setShadow(int height, int heightOverGround) {
			this.shadowDist = height;
			this.shadowHeight = heightOverGround;
			return this;
		}
		
		@Override
		public int sData() {
			return sData;
		}
		
		public Imp sDataSet(int s) {
			this.sData = s;
			return this;
		}
		
		protected int getData2(RenderIterator it) {
			return SETT.ROOMS().fData.spriteData2.get(it.tile());
		}
		
		public void renderDegrade(TILE_SHEET sheet, SPRITE_RENDERER r, int tile, RenderData.RenderIterator it, double degrade) {
			if (degrade > 0.05) {
				OPACITY.O99.bind();
				sheet.renderTextured(SETT.ROOMS().util.filth.texture(degrade, it.ran()), tile, it.x(), it.y());
				OPACITY.unbind();
			}
		}
		
	}
	
	
}
