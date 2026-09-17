package settlement.room.sprite;

import java.io.IOException;

import init.sprite.game.SheetType;
import settlement.room.main.furnisher.FurnisherItem;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public class RoomSprite1xN extends RoomSprite1x1 {

	private final boolean master;

	public RoomSprite1xN(Json json, String key, boolean master) throws IOException {
		super(json, key);
		this.master = master;
	}

	public RoomSprite1xN(RoomSprite other, boolean master) throws IOException {
		super(other);
		this.master = master;
	}

	@Override
	public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
			boolean isCandle) {

		it.ranOffset(offX(data), offY(data));
		return super.render(r, s, getRot(data), it, degrade, isCandle);

	}

	@Override
	public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
			FurnisherItem item) {

		DIR d = rot(data);
		int m = d.mask();
		if ((data & 0b0100) != 0) {
			m |= d.perpendicular().mask();
		}
		SheetType.sCombo.renderOverlay(x, y, r, item.get(rx, ry).availability, m, 0, false);
	}

	@Override
	public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
		int m = Math.max(item.width(), item.height());
		int res = -1;

		for (int dist = 1; dist < m; dist++) {
			for (int di = 0; di < DIR.ORTHO.size(); di++) {
				DIR d = DIR.ORTHO.get(di);

				if (isMaster(rx + d.x() * dist, ry + d.y() * dist, item)) {
					if (res != -1) {
						res |= 0b0100;
						return (byte) res;
					}
					res = (di | (dist << 3));
				}
			}
		}

		if (res != -1)
			return (byte) res;
		
		for (int dist = 1; dist < m; dist++) {
			for (int di = 0; di < DIR.ORTHO.size(); di++) {
				DIR d = DIR.ORTHO.get(di);

				if (joins(rx + d.x() * dist, ry + d.y() * dist, item, !master)) {
					if (res != -1) {
						res |= 0b0100;
						return (byte) res;
					}
					res = (di | (dist << 3));
				}
			}
		}
		
		
		if (res != -1)
			return (byte) res;
		return (byte) item.rotation;
	}

	protected boolean isMaster(int rx, int ry, FurnisherItem item) {
		RoomSprite s = item.sprite(rx, ry);
		if (s == null)
			return false;
		if (s instanceof RoomSprite1xN) {
			return master != ((RoomSprite1xN) s).master;
		}
		return false;
	}

	protected final boolean joins(int rx, int ry, FurnisherItem item, boolean master) {
		RoomSprite s = item.sprite(rx, ry);
		if (s == null)
			return false;
		if (s instanceof RoomSprite1xN) {
			return master != ((RoomSprite1xN) s).master;
		}
		return false;
	}

	@Override
	protected final boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
		RoomSprite s = item.sprite(rx, ry);
		if (s == null)
			return false;
		if (s instanceof RoomSprite1xN) {
			return master != ((RoomSprite1xN) s).master;
		}
		return false;
	}

	public int offX(int data) {
		if (master)
			return 0;
		DIR d = rot(data);
		return d.x() * ((data >> 3) & 0b011111);
	}

	public int offY(int data) {
		if (master)
			return 0;
		DIR d = rot(data);
		return d.y() * ((data >> 3) & 0b011111);
	}

}
