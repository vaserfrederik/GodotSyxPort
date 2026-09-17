package settlement.room.infra.janitor;

import java.io.IOException;

import init.resources.RESOURCES;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSpriteCombo;
import snake2d.SPRITE_RENDERER;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	private final ROOM_JANITOR blue;
	final FurnisherStat workers = new FurnisherStat.FurnisherStatI(this);
	final FurnisherStat efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers);
	final FurnisherItemTile ta;
	
	
	protected Constructor(ROOM_JANITOR blue, RoomInitData init)
			throws IOException {
		super(init, 2, 2, 88, 44);
		this.blue = blue;
		
		Json sp = init.data().json("SPRITES");
		
		RoomSprite res = new RoomSpriteCombo(sp, "TABLE_COMBO") {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (SETT.ROOMS().fData.candle.is(it.tile()))
					return;
				JanitorInstance ins = blue.getter.get(it.tile());
				if (ins != null) {
					int ri = (int) (ins.tableRes >> (8*((it.tx()+it.ty())%8)));
					ri &= 0x0FF;
					if (ri != 0)
						RESOURCES.ALL().get(ri-1).renderLaying(r, it.x(), it.y(), it.ran(), ins.bits.resAm(RESOURCES.ALL().getC(ri-1)));
				}
			}
			
		};
		
		RoomSprite table = new RoomSpriteCombo(res) {
			final RoomSprite1x1 top = new RoomSprite1x1(sp, "TABLE_MISC_1X1");
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				top.renderRandom(r, s, it, it.ran(), degrade);
			}
			
		};
		
		final RoomSprite1x1 top = new RoomSprite1x1(sp, "MISC_1X1");
		
		RoomSprite nickGround = new RoomSprite1x1(sp, "STORAGE_1X1") {
			
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				if (!isCandle) {
					top.renderRandom(r, s, it, it.ran(), degrade);
				}
				return false;
				
			}
			
		};
		
		final FurnisherItemTile tc = new FurnisherItemTile(
				this,
				table,
				AVAILABILITY.ROOM_SOLID, 
				true);		
		
		ta = new FurnisherItemTile(
				this,
				true,
				res,
				AVAILABILITY.ROOM_SOLID, 
				false).setData(1);
		
		final FurnisherItemTile ng = new FurnisherItemTile(
				this,
				nickGround,
				AVAILABILITY.ROOM_SOLID, 
				true);
		
		final FurnisherItemTile nn = new FurnisherItemTile(
				this,
				top,
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tc,ta,ta,ta,tc},
		}, 1);
		
		flush(1, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{nn,ng,},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{nn,ng,nn},
		}, 1.5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{nn,ng,ng,nn},
		}, 2);
		
		flush(1, 3);
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
	public Room create(TmpArea area, RoomInit init) {
		return new JanitorInstance(blue, area, init);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	
	@Override
	public boolean isHeavy() {
		return true;
	}
	
//	private final FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][] {
//		{0,0,0,0,0,0,0,0},
//		{0,1,0,0,0,0,0,0},
//		{0,1,0,0,0,0,0,0},
//		{0,1,1,1,1,1,1,0},
//		{0,1,1,1,1,1,1,0},
//		{0,1,1,1,1,1,1,0},
//		{0,0,0,0,0,0,0,0},
//		{0,0,0,0,0,0,0,0},
//		},
//		miniColor
//	);
//	
//	@Override
//	public COLOR miniColor(int tx, int ty) {
//		return miniC.get(tx, ty);
//	}
	

}
