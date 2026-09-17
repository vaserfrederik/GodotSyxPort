package settlement.room.sprite;

import java.io.IOException;

import init.sprite.game.SheetPair;
import init.sprite.game.SheetType;
import settlement.room.main.furnisher.FurnisherItem;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.GUTIL;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public class RoomSprite1x1 extends RoomSpriteImp{

	
	public RoomSprite1x1(Json json, String key) throws IOException{
		super(SheetType.s1x1, json, key);
	}
	
	public RoomSprite1x1(RoomSprite other) throws IOException{
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
		
		int tile = type().tile(sheet.s, sheet.d, 0, frame(sheet, it), this.rotates ? (data&0x03) : ran&0b11);
		
		sheet.s.render(sheet.d, it.x(), it.y(), it, r, tile, ran, degrade);
		COLOR.unbind();
		if (s != null)
			sheet.s.renderShadow(sheet.d, it.x(), it.y(), it, s, tile, ran);
		return false;
		
	}

	public void renderRandom(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, int ran, double degrade) {
		
		
		SheetPair sheet = sheetPair(it, ran);
		if (sheet == null)
			return;
		sheet.d.color(ran).bind();
		
		int tile = type().tile(sheet.s, sheet.d, 0, frame(sheet, it), -1);
		
		
		sheet.s.render(sheet.d, it.x(), it.y(), it, r, tile, ran, degrade);
		COLOR.unbind();
		sheet.s.renderShadow(sheet.d, it.x(), it.y(), it, s, tile, ran);
	}
	
	public void renderRandom(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, int ran, double degrade, COLOR col) {
		
		
		SheetPair sheet = sheetPair(it, ran);
		if (sheet == null)
			return;
		col.bind();
		
		int tile = type().tile(sheet.s, sheet.d, 0, frame(sheet, it), -1);
		
		
		sheet.s.render(sheet.d, it.x(), it.y(), it, r, tile, ran, degrade);
		COLOR.unbind();
		sheet.s.renderShadow(sheet.d, it.x(), it.y(), it, s, tile, ran);
	}
	
	@Override
	public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
			FurnisherItem item) {
		SheetType.s1x1.renderOverlay(
				x, y, r, item.get(rx, ry).availability, 
				0, rotates ? data : -1, item.width() == 1 && item.height() == 1);
	}
	
	@Override
	public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
		int ri = GUTIL.ran2().get(tx, ty)&0b011;
		for (int i = 0; i < DIR.ORTHO.size(); i++) {
			int rr = (ri+i)%DIR.ORTHO.size();
			DIR d = DIR.ORTHO.get(rr);
			if (joins(tx+d.x(), ty+d.y(), rx+d.x(), ry+d.y(), d, item))
				return (byte) rr;
		}
		return (byte) item.rotation;
	}

	public int getRot(int data) {
		return data&0x03;
	}
	
	public DIR rot(int data) {
		return DIR.ORTHO.get(data&0x03);
	}
	
	@Override
	protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
		return DIR.ORTHO.get(item.rotation) == d;
	}

	@Override
	public SheetType.c1X1 type() {
		return SheetType.s1x1;
	}

	@Override
	public int rotation(int data, FurnisherItem item) {
		if (rotates)
			return getRot(data)+1;
		return 0;
	}
	
}
