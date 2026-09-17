package settlement.room.sprite;

import java.io.IOException;

import init.sprite.game.SheetPair;
import init.sprite.game.SheetType;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.rnd.RND;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public class RoomSpriteBoxN extends RoomSpriteImp{

	public RoomSpriteBoxN(Json json, String key) throws IOException{
		super(SheetType.sBox, json, key);
	}
	
	public RoomSpriteBoxN(RoomSprite other) throws IOException{
		super(other);
	}
	
	@Override
	public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
			boolean isCandle) {

		int k = (data>>5)&0b0111;
		int rot = (data >> 4) & 1;
		SheetPair sheet = sheetPair(it, k);
		if (sheet == null)
			return false;
		sheet.d.color(k).bind();
		int ran = it.ran();
		
		int tile = type().tile(sheet.s, sheet.d, data&0x0F, frame(sheet, it), rot);
		
		sheet.s.render(sheet.d, it.x(), it.y(), it, r, tile, ran, degrade);
		COLOR.unbind();
		if (s != null)
			sheet.s.renderShadow(sheet.d, it.x(), it.y(), it, s, tile, ran);
		return false;
		
		
	}
	
	@Override
	public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
			FurnisherItem item) {
		type().renderOverlay(x, y, r, item.get(rx, ry).availability, 
				data&0x0F, 0, false);
	}
	
	public int rotMask(int data) {
		return (data&0x0F);
	}

	@Override
	public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
		int m = 0;
		int ri = RND.rInt(DIR.ORTHO.size());
		for (int i = 0; i < DIR.ORTHO.size(); i++) {
			int rr = (ri+i)%DIR.ORTHO.size();
			DIR d = DIR.ORTHO.get(rr);
			if (joins(tx+d.x(), ty+d.y(), rx+d.x(), ry+d.y(), d, item))
				m |= d.mask();
		}
		m |= 0b00010000*(item.rotation&1);
		return (byte) (m | (itemRan<<5));
	}

	@Override
	protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
		FurnisherItemTile t = item.get(rx, ry);
		return t != null && t.sprite != null && t.sprite instanceof RoomSpriteBoxN;
	}

	@Override
	public SheetType type() {
		return SheetType.sBox;
	}

}
