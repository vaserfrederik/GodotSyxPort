package settlement.room.service.hearth;

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
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	private final ROOM_HEARTH blue;
	
	final FurnisherStat services = new FurnisherStat.FurnisherStatI(this);
	

	
	static final int codeService = 1;
	static final int codeFire = 2;

	protected Constructor(ROOM_HEARTH blue, RoomInitData init)
			throws IOException {
		super(init, 1, 1, 88, 44);
		this.blue = blue;
		
		Json sp = init.data().json("SPRITES");
		
		final RoomSprite sBench = new RoomSprite1x1(sp, "BENCH_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				
				
				DIR d2 = DIR.ORTHO.getC(item.rotation+1);
				
				if (d2.x()*d.x() == 0 && d2.y()*d.y() == 0)
					return false;
				
				if (item.get(rx+-d.x()*4, ry-d.y()*4) == null)
					return true;
				return false;	
			}
			
		};
		
		final RoomSprite sHearth = new RoomSpriteCombo(sp, "HEARTH_COMBO");
		
		final RoomSprite sFire = new RoomSpriteCombo(sHearth) {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				
			}
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, false);
				if (blue.is(it.tile())) {
					HearthInstance ins = blue.getter.get(it.tile());
					
					RESOURCES.WOOD().renderLaying(r, it.x(), it.y(), it.ran(), 5);
					
					SETT.LIGHTS().hide(it.tx(), it.ty(), ins.used == 0);
					
					
				}
				return false;
			}
		};
		
		
		final FurnisherItemTile ff = new FurnisherItemTile(
				this,
				sFire,
				AVAILABILITY.SOLID, 
				false).setData(codeFire);
		final FurnisherItemTile fe = new FurnisherItemTile(
				this,
				sHearth,
				AVAILABILITY.SOLID, 
				false);
		final FurnisherItemTile bb = new FurnisherItemTile(
				this,
				false,
				sBench, 
				AVAILABILITY.PENALTY4, 
				false).setData(codeService);
		
		
		final FurnisherItemTile __ = new FurnisherItemTile(
				this,
				null, 
				AVAILABILITY.ROOM, false);
		
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,bb,__,bb,__}, 
			{__,bb,ff,bb,__},
			{__,bb,__,bb,__},
		}, 6, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,bb,__,bb,__}, 
			{__,bb,__,bb,__}, 
			{__,bb,ff,bb,__},
			{__,bb,__,bb,__},
			{__,bb,__,bb,__}, 
		}, 10, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,bb,__,bb,__}, 
			{__,bb,__,bb,__}, 
			{__,bb,fe,bb,__}, 
			{__,bb,ff,bb,__},
			{__,bb,fe,bb,__},
			{__,bb,__,bb,__}, 
			{__,bb,__,bb,__}, 
		}, 14, 14);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,bb,bb,__,bb,bb,__}, 
			{__,bb,bb,fe,bb,bb,__}, 
			{__,bb,bb,ff,bb,bb,__}, 
			{__,bb,bb,fe,bb,bb,__},
			{__,bb,bb,ff,bb,bb,__},
			{__,bb,bb,fe,bb,bb,__}, 
			{__,bb,bb,__,bb,bb,__}, 
		}, 28, 28);
		
		
		flush(1, 3);
		
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
	public Room create(TmpArea area, RoomInit init) {
		return new HearthInstance(blue, area, init);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}

	
//	private final FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][] {
//		{0,0,0,0,0,0,0,0},
//		{0,0,0,0,0,0,0,0},
//		{0,0,0,1,1,0,0,0},
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
