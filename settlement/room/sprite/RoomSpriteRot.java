package settlement.room.sprite;

import init.constant.C;
import settlement.room.main.furnisher.FurnisherItem;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.sets.LIST;
import snake2d.util.sprite.SPRITE;
import snake2d.util.sprite.TILE_SHEET;
import util.rendering.RenderData;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public class RoomSpriteRot extends RoomSprite.Imp {

	private final static int ROT = 4; 
	
	public final int tileEnd;
	public final int tilestart;
	//private final TILE_SHEET sheet;
	private final LIST<SPRITE> blue;
	
	private final TILE_SHEET sheet;

	
	public RoomSpriteRot(TILE_SHEET sheet, int startTile, int variations, LIST<SPRITE> blueprint) {
		this.sheet = sheet;
		this.blue = blueprint;
		this.tileEnd = startTile+variations*ROT;
		this.tilestart = startTile;
		shadowDist = 3;
		shadowHeight = 0;
	}
	
	@Override
	public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
		int x = it.x();
		int y = it.y();
		return render(r, s, data, x, y, it.ran(), it, degrade, isCandle);
				
	}
	
	public boolean renderRandom(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, double degrade, boolean off) {
		
		int x = it.x();
		int y = it.y();
		int ran = it.ran()&Integer.MAX_VALUE;
		if (off) {
			x += -C.SCALE*2 + (ran&Integer.MAX_VALUE)%(C.SCALE*4);
			ran = ran >> 4;
			y += -C.SCALE*2 + (ran&Integer.MAX_VALUE)%(C.SCALE*4);
			ran = ran >> 4;
		}
		
		int data = ran & 0b011;
		ran = ran >> 2;
		
		return render(r, s, data, x, y, ran, it, degrade, false);
				
	}
	

	protected final boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, int x, int y, int ran, RenderIterator it, double degrade, boolean isCandle) {
		
		
		ran &= Integer.MAX_VALUE;
		int variations = (tileEnd-tilestart)/4;
		int tile = (ran % variations);
		tile += (data & 0b011)*variations;
		tile += tilestart;
		tile+=getTileOffset(it, data);
		sheet.render(r, tile, x, y);
		ran = ran >> 4;
		renderDegrade(sheet, r, tile, it, degrade);
		
		if (s != null && (shadowHeight > 0 || shadowDist > 0)) {
			s.setDistance2Ground(shadowHeight).setHeight(shadowDist);
			sheet.render(s, tile, x, y);
		}
		
		return false;
		
		
	}
	
	protected static int getRot(int data) {
		return (data & 0b011);
	}
	
	protected int getTileOffset(RenderData.RenderIterator it, int data) {
		return 0;
	}
	
	@Override
	public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
			FurnisherItem item) {
		int tile = (data & 0b011);
		blue.get(tile).render(r, x, y);
	}
	
	@Override
	public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
		
		int r = 0;
		
		for (int i = 0; i < DIR.ORTHO.size(); i++) {
			DIR d = DIR.ORTHO.get(i);
			
			int dx = rx+d.x();
			int dy = ry+d.y();

			if ((dy < 0 || dy >= item.height())) {
				if (joinsWith(null, true, item.rotation, d, rx, ry, item)) {
					r = i;
					break;
				}
			}else if ((dx < 0 || dx >= item.width()) && joinsWith(null, true, item.rotation, d, rx, ry, item)) {
				if (joinsWith(null, true, item.rotation, d, rx, ry, item)) {
					r = i;
					break;
				}
			}else if (joinsWith(item.sprite(dx, dy), false, item.rotation, d, rx, ry, item)) {
				r = i;
				break;
			}
		}
		
		return (byte) r;
	}
	
	protected boolean joinsWith(RoomSprite s, boolean outof, int dir, DIR test, int rx, int ry, FurnisherItem item) {
		return test == DIR.ORTHO.get(dir);
	}
	
//	public static class Random extends RoomSprite.Imp{
//
//		private final RoomSpriteRot[] vars;
//		private final RoomSpriteRot candle = new RoomSpriteRot(SINGLETYPE.TABLE_WOOD);
//		
//		public Random(RoomSpriteRot... vars) {
//			this.vars = vars;
//		}
//		
//		public Random(SINGLETYPE...sprites) {
//			vars = new RoomSpriteRot[sprites.length];
//			for (int i = 0; i < vars.length; i++) {
//				vars[i] = new RoomSpriteRot(sprites[i]) {
//					@Override
//					protected boolean joinsWith(RoomSprite s, boolean outof, int dir, DIR test, int rx, int ry, FurnisherItem item) {
//						return Random.this.joinsWith(s, outof);
//					};
//				};
//			}
//		}
//		
//		
//		protected boolean joinsWith(RoomSprite sprite, boolean outof){
//			return false;
//		}
//		
//		@Override
//		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
//			if (isCandle) {
//				return candle.render(r, s, data, it, degrade, isCandle);
//			}else {
//				int var = it.ran()%vars.length;
//				return vars[var].render(r, s, data, it, degrade, isCandle);
//			}
//			
//		}
//		
//		@Override
//		public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, FurnisherItemTile item) {
//			vars[0].renderPlaceholder(r, x, y, data&0x03, tx, ty, item);
//		}
//		
//		@Override
//		public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
//			int r = RND.rInt(vars.length);
//			return vars[r].getData(tx, ty, rx, ry, item, itemRan);
//		}
//		
//	}
	
}