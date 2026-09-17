package settlement.room.infra.hauler;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import init.constant.C;
import init.sprite.SPRITES;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSpriteCombo;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.OPACITY;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.sprite.SPRITE;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	protected Constructor(RoomInitData init)
			throws IOException {
		super(init, 1, 0, 88, 44);
	
		Json sp = init.data().json("SPRITES");
		
		RoomSprite spriteCrate = new RoomSprite1x1(sp, "CRATE_1X1") {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				HaulerInstance ins = blue().get(it.tx(), it.ty());
				if (ins!= null && blue().crate.get(it.tx(), it.ty(), ins, ins.sdata).resource() != null) {
					
					 blue().crate.resource().renderLaying(r, it.x(), it.y(), it.ran(), blue().crate.amount());
				}
				return false;
			}
		};
		RoomSprite spriteCrateRes = new RoomSprite1x1(spriteCrate) {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
			
				HaulerInstance ins = ROOMS().HAULER.getter.get(it.tile());
				if (ins == null)
					return false;
				
				SPRITE i = ins.resource() == null ? SPRITES.icons().m.cancel : ins.resource().icon();
				OPACITY.O99.bind();
				i.render(r, it.x(), it.x()+C.TILE_SIZE, it.y(), it.y()+C.TILE_SIZE);
				OPACITY.unbind();
				
				return false;
			}
		};
		
		RoomSpriteCombo spriteFence = new RoomSpriteCombo(sp, "FENCE_COMBO") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return false;
			}
		};
		
		
		FurnisherItemTile tt = new FurnisherItemTile(this, false, spriteCrateRes, AVAILABILITY.AVOID_PASS, false);
		
		FurnisherItemTile ff = new FurnisherItemTile(this, false, spriteFence, AVAILABILITY.AVOID_PASS, false);
		FurnisherItemTile __ = new FurnisherItemTile(this, false, spriteCrate, AVAILABILITY.ROOM, false).setData(1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,},
			{__,},
		}, 1);
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,ff},
			{__,__},
		}, 2);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ff,__,ff},
			{__,tt,__},
			{ff,__,ff},
		}, 4);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ff,__,__,__,ff},
			{__,__,tt,__,__},
			{ff,__,__,__,ff},
		}, 11);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ff,__,__,__,ff},
			{__,__,__,__,__},
			{__,__,tt,__,__},
			{__,__,__,__,__},
			{ff,__,__,__,ff},
		}, 20);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ff,__,__,__,__,__,ff},
			{__,__,__,__,__,__,__},
			{__,__,__,__,__,__,__},
			{__,__,__,tt,__,__,__},
			{__,__,__,__,__,__,__},
			{__,__,__,__,__,__,__},
			{ff,__,__,__,__,__,ff},
		}, 44);
		
		flush(3);
	}

	@Override
	public boolean usesArea() {
		return false;
	}
	
	@Override
	public boolean mustBeIndoors() {
		return false;
	}
	
	@Override
	public boolean mustBeOutdoors() {
		return false;
	}

	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new HaulerInstance(blue(), area, init);
	}

	@Override
	public ROOM_HAULER blue() {
		return SETT.ROOMS().HAULER;
	}

}
