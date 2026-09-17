package settlement.room.infra.monument;

import java.io.IOException;

import init.constant.C;
import init.resources.RESOURCES;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.TmpArea;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSpriteCombo;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Torch extends ROOM_MONUMENT {

	private final Constructor2 constructor;
//	private final Instance instance;
	
	
	public Torch(RoomInitData init, int index, String key, RoomCategorySub cat) throws IOException {
		super(init, index, key, cat);
		this.constructor = new Constructor2(init);
	}


	@Override
	public Constructor2 constructor() {
		return constructor;
	}

	public final class Constructor2 extends MConstructor {

		public final RoomSprite small;
		public final RoomSprite medium;
		final FurnisherItemTile ss;
		final FurnisherItemTile sm;
		
		protected Constructor2(RoomInitData init)
				throws IOException {
			super(Torch.this, init);
			Json js = init.data().json("SPRITES");
			
			small = new RoomSprite1x1(js, "SMALL_1X1") {
				@Override
				public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
						boolean isCandle) {
					super.render(r, s, data, it, degrade, isCandle);
					int am = (int) ((1.0-SETT.ROOMS().map.get(it.tx(), it.ty()).getDegrade(it.tx(), it.ty()))*4);
					RESOURCES.WOOD().renderLaying(r, it.x(), it.y(), it.ran(), am);
					return false;
				}
				
			};
			medium = new RoomSpriteCombo(js, "COMBO") {
				
				@Override
				public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
						boolean isCandle) {
					super.render(r, s, data, it, degrade, isCandle);
					if ((data & DIR.N.mask()) != 0 && (data & DIR.W.mask()) != 0) {
						int am = (int) ((1.0-SETT.ROOMS().map.get(it.tx(), it.ty()).getDegrade(it.tx(), it.ty()))*8);
						RESOURCES.WOOD().renderLaying(r, it.x()-C.TILE_SIZEH, it.y()-C.TILE_SIZEH, it.ran(), am);
					}
					return false;
				}
				
			};
			
			ss = new FurnisherItemTile(
					this,
					small,
					AVAILABILITY.SOLID,
					false
					);
			sm = new FurnisherItemTile(
					this,
					medium,
					AVAILABILITY.SOLID,
					false
					);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{ss},
			}, 1,1);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{sm,sm},
				{sm,sm},
			}, 4,4);
			
			flush(0);
		}

		@Override
		public Room create(TmpArea area, RoomInit init) {
			
			if (area.body().width() == 2) {
				SETT.LIGHTS().torchBig(area.body().x1(), area.body().y1(), C.TILE_SIZEH);
			}else {
				SETT.LIGHTS().torch(area.body().x1(), area.body().y1(), 0);
			}
			return super.create(area, init);
		}

		
	}



	
}
