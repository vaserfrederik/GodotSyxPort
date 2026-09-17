package settlement.room.service.speaker;

import java.io.IOException;

import init.constant.C;
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
import settlement.room.sprite.RoomSpriteCombo;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class SpeakerConstructor extends Furnisher{

	private final ROOM_SPEAKER blue;
	final FurnisherStat workers;
	final FurnisherStat spectators;
	
	protected SpeakerConstructor(ROOM_SPEAKER blue, RoomInitData init)
			throws IOException {
		super(init, 1, 2, 88, 44);
		this.blue = blue;
		workers = new FurnisherStat.FurnisherStatEmployees(this);
		spectators = new FurnisherStat.FurnisherStatServices(this, blue);
		
		Json sp = init.data().json("SPRITES");
		
		RoomSprite sSprite = new RoomSpriteCombo(sp, "CENTER_COMBO");
		
		RoomSprite sFrame = new RoomSpriteCombo(sp, "FRAME_COMBO") {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				if ((data & 0x0F) == 0x0F) {
					for (DIR d : DIR.NORTHO) {
						it.setOff(d.x()*C.TILE_SIZEH, d.y()*C.TILE_SIZEH);
						d = d.perpendicular();
						int m = d.next(-1).mask() | d.next(1).mask();
						
						sSprite.render(r, s, m, it, degrade, isCandle);
					}
				}
				return false;
				
			};
		};
		
		
		FurnisherItemTile bb = new FurnisherItemTile(
				this,
				sFrame,
				AVAILABILITY.PENALTY4, 
				false);
		final FurnisherItemTile b1 = new FurnisherItemTile(
				this,
				sFrame,
				AVAILABILITY.ROOM, 
				false);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{b1,b1,b1,}, 
			{b1,bb,b1,},
			{b1,b1,b1,}, 
		}, 1);
		
		flush(1, 0);
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
		return new SpeakerInstance(blue, area, init);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
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
