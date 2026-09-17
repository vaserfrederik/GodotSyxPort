package settlement.room.food.pasture;

import java.io.IOException;

import settlement.path.AVAILABILITY;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.job.RoomResStorage;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite1x1;
import snake2d.util.datatypes.AREA;
import snake2d.util.file.Json;

final class ConstructorIndoor extends Constructor{

	
	private ROOM_PASTURE blue;

	public static final int STORAGE1 = 100;
	public static final int STORAGE2 = 200;
	public static final int STORAGE3 = 300;
	

	final FurnisherItemTile s1;
	final FurnisherItemTile s2;
	final FurnisherItemTile s3;
	

	
	protected ConstructorIndoor(ROOM_PASTURE blue, RoomInitData init)
			throws IOException {
		super(blue, init);
		this.blue = blue;
		

		Json js = init.data().json("SPRITES");

		s1 = new FurnisherItemTile(this, true, new SpriteDep(js, blue.s1), AVAILABILITY.ROOM_SOLID, false).setData(1);
		s1.setData(STORAGE1);
		s2 = new FurnisherItemTile(this, true, new SpriteDep(js, blue.s2), AVAILABILITY.ROOM_SOLID, false).setData(1);
		s2.setData(STORAGE2);
		s3 = new FurnisherItemTile(this, true, new SpriteDep(js, blue.s3), AVAILABILITY.ROOM_SOLID, false).setData(1);
		s3.setData(STORAGE3);
		
		
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{s1,s2,s3},
		}, 1, 1);
		
		flush(1, 1, 3);
		
		makeAux(js);
		
		
	}


	@Override
	public boolean usesArea() {
		return true;
	}

	@Override
	public boolean mustBeIndoors() {
		return true;
	}
	
	@Override
	public boolean mustBeOutdoors() {
		return false;
	}

	@Override
	public ROOM_PASTURE blue() {
		return blue;
	}


	
	@Override
	public boolean needsIsolation() {
		return true;
	}
	

	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		super.putFloor(tx, ty, upgrade, area);
		
	}
	
	@Override
	public boolean removeFertility() {
		return true;
	}
	
	private static class SpriteDep extends RoomSprite1x1{

		private final RoomResStorage st;
		
		public SpriteDep(Json json, RoomResStorage st) throws IOException {
			super(json, "STORAGE_1X1");
			this.st = st;
		}
		

		@Override
		public boolean render(snake2d.SPRITE_RENDERER r, util.rendering.ShadowBatch s, int data, util.rendering.RenderData.RenderIterator it, double degrade, boolean isCandle) {
			
			boolean ret =super.render(r, s, data, it, degrade, isCandle);
			st.render(r, s, it.tx(), it.ty(), it.x(), it.y(), it.ran());
			
			return ret;
			
		};
		
		
		
	}

	@Override
	protected boolean fenceJoin(FurnisherItemTile gc) {
		return true;
	}
	
}
