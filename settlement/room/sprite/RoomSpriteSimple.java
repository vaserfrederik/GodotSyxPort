package settlement.room.sprite;

import init.sprite.SPRITES;
import settlement.room.main.furnisher.FurnisherItem;
import snake2d.SPRITE_RENDERER;
import snake2d.util.sprite.TILE_SHEET;
import util.rendering.RenderData;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public class RoomSpriteSimple extends RoomSprite.Imp {

	private final int tileStart;
	public final int tileEnd;
	private final TILE_SHEET sheet;
	private final int variations;

	public RoomSpriteSimple(TILE_SHEET sheet, int startTile, int variations) {
		this(sheet, startTile, variations, 0, 3);
	}

	private RoomSpriteSimple(TILE_SHEET sheet, int startTile, int variations, int shadHeight, int shadLength) {
		this.sheet = sheet;
		this.tileStart = startTile;
		this.variations = variations;
		this.tileEnd = startTile + variations;
		this.shadowHeight = shadHeight;
		this.shadowDist = shadLength;
	}

	@Override
	public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
			boolean isCandle) {
		int x = it.x();
		int y = it.y();
		data += getTileOffset(it, data);
		int tile = data + tileStart + (it.ran() % variations);
		sheet.render(r, tile, x, y);
		renderDegrade(sheet, r, tile, it, degrade);

		if (shadowHeight > 0 || shadowDist > 0) {
			s.setDistance2Ground(shadowHeight).setHeight(shadowDist);
			sheet.render(s, tile, x, y);
		}

		return false;
	}

	@Override
	public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
			FurnisherItem item) {
		if (item.get(rx, ry) != null && item.get(rx, ry).isBlocker())
			SPRITES.cons().BIG.filled.render(r, 0, x, y);
		else
			SPRITES.cons().BIG.dashedThick.render(r, 0, x, y);
	}

	protected int getTileOffset(RenderData.RenderIterator it, int data) {
		return 0;
	}

	@Override
	public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
		return 0;
	}

}