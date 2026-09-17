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

public class RoomSpriteXxX extends RoomSpriteImp{

	private int rotation = 0;
	private final SheetType.cXxX type;
	public RoomSpriteXxX(Json json, String key, int size) throws IOException{
		super(size(size), json, key);
		this.type = size(size);
	}
	
	public RoomSpriteXxX(int size) throws IOException{
		super(size(size));
		this.type = size(size);
	}

	private static SheetType.cXxX size(int size) {
		if (size == 2)
			return SheetType.s2x2;
		else if (size == 3)
			return SheetType.s3x3;
		throw new RuntimeException();
	}
	
	@Override
	public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
			boolean isCandle) {

		int t = data&0b0111111;
		int rot = rot(data);
		int dx = type.dx(data);
		int dy = type.dy(data);
		
		it.ranOffset(-dx, -dy);
		int ran = it.ran();
		SheetPair sheet = sheetPair(it, ran);
		if (sheet == null)
			return false;
		
		sheet.d.color(ran).bind();
		
		if (!sheet.d.rotates) {
			rot = (ran >> 9)&0b11;
		}
		
		int tile = type.tile(sheet.s, sheet.d, t, frame(sheet, it), rot);
		
		it.ranOffset(dx, dy);
		sheet.s.render(sheet.d, it.x(), it.y(), it, r, tile, ran, degrade);
		COLOR.unbind();
		sheet.s.renderShadow(sheet.d, it.x(), it.y(), it, s, tile, ran);
		
		return false;
		
		
	}
	
	@Override
	public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
			FurnisherItem item) {
		type.renderOverlay(x, y, r, item.get(rx, ry).availability, 
				data&0b0111111, rotates ? rot(data) : -1, false);
	}
	
	public int rot(int data) {
		return (data>>6)&0b11;
	}
	
	public int setRot(int data, int rot) {
		data &= 0b0011_1111;
		data |= (rot&0b11) << 6;
		return data;
	}

	@Override
	public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
		int dx = type.size-1;
		int dy = type.size-1;
		
		for (int y = 0; y < type.size; y++) {
			if (!joins(tx, ty-y, rx, ry-y, null, item)) {
				dy = y-1;
				break;
			}
		}
		for (int x = 0; x < type.size; x++) {
			if (!joins(tx-x, ty, rx-x, ry, null, item)) {
				dx = x-1;
				break;
			}
		}
		
		int i = dx + type.size*dy;
		
		i |= ((item.rotation+rotation)&0b11) << 6;
		
		return (byte) i;
	}
	
	public RoomSpriteXxX rotate(int rotation) {
		this.rotation = rotation;
		return this;
	}

	@Override
	protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
		return item.sprite(rx, ry) == this;
	}

	@Override
	public SheetType.cXxX type() {
		return type;
	}

}
