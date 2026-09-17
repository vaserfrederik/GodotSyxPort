package settlement.room.sprite;

import java.io.IOException;

import init.sprite.game.SheetPair;
import init.sprite.game.SheetType;
import settlement.room.main.furnisher.FurnisherItem;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public class RoomSpriteTex extends RoomSpriteImp{

	
	public RoomSpriteTex(Json sp, String key) throws IOException {
		super(SheetType.sTex, sp, key);
	}
	
	public RoomSpriteTex(RoomSprite other) throws IOException{
		super(other);
	}
	

	@Override
	public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
			boolean isCandle) {
		int ran = it.ran();
		SheetPair sheet = sheetPair(it, ran);
		if (sheet == null)
			return false;
		sheet.d.color(ran).bind();
		ran = ran>>4;
		
		int tile = type().tile(sheet.s, sheet.d, 0, frame(sheet, it), 0);
		
		sheet.s.render(sheet.d, it.x(), it.y(), it, r, tile, ran, degrade);
		COLOR.unbind();
		if (s != null)
			sheet.s.renderShadow(sheet.d, it.x(), it.y(), it, s, tile, ran);
		return false;
		
	}
	
	@Override
	public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
			FurnisherItem item) {
		SheetType.sTex.renderOverlay(
				x, y, r, item.get(rx, ry).availability, 
				0, rotates ? data : -1, item.width() == 1 && item.height() == 1);
	}
	
	@Override
	public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
		return 0;
	}

	@Override
	protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
		return false;
	}

	@Override
	public SheetType type() {
		return SheetType.sTex;
	}


}
