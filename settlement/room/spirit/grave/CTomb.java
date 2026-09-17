package settlement.room.spirit.grave;

import java.io.IOException;

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
import settlement.room.sprite.RoomSprite1xN;
import settlement.room.sprite.RoomSpriteXxX;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.file.Json;
import snake2d.util.misc.CLAMP;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class CTomb extends Furnisher{

	private final ROOM_TOMB blue;

	final FurnisherStat workers;
	final FurnisherStat services;
	final FurnisherStat respekk;
	
	final RoomSprite sHead;
	
	protected CTomb(ROOM_TOMB blue, RoomInitData init)
			throws IOException {
		super(init, 2, 3, 88, 44);
		this.blue = blue;
		
		workers = new FurnisherStat.FurnisherStatEmployees(this, 0);
		services = new FurnisherStat.FurnisherStatI(this);
		respekk = new FurnisherStat(this) {
			
			@Override
			public double get(AREA area, double fromItems) {
				fromItems /= area.area();
				fromItems*= 2;
				return CLAMP.d(fromItems, 0, 1.0);
			}
			
			@Override
			public GText format(GText t, double value) {
				return GFORMAT.perc(t, value);
			}
		};
		Json sData = init.data().json("SPRITES");
		
		sHead = new RoomSprite1xN(sData, "HEAD_BOTTOM_1X1", false) {
			final RoomSprite1xN lid = new RoomSprite1xN(sData, "HEAD_TOP_1X1", false);
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				
				super.render(r, s, data, it, degrade, isCandle);
				if (blue.is(it.tile())) {
					int x = it.tx()+offX(data);
					int y = it.ty()+offY(data);
					if (Grave.isUsed(x, y)) {
						lid.render(r, s, getData2(it), it, degrade, isCandle);
					}
				}
				return false;
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return lid.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		
		final RoomSprite sFoot = new RoomSprite1xN(sData, "TAIL_BOTTOM_1X1", true) {
			final RoomSprite1xN lid = new RoomSprite1xN(sData, "TAIL_TOP_1X1", true);
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				
				super.render(r, s, data, it, degrade, isCandle);
				if (blue.is(it.tile())) {
					if (Grave.isUsed(it.tx(), it.ty())) {
						lid.render(r, s, getData2(it), it, degrade, isCandle);
					}
				}
				return false;
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return lid.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};
		
		final RoomSprite sStone = new RoomSprite1x1(sData, "STONE_1X1");
		
		
		
		RoomSprite sSmall = new RoomSprite1x1(sData, "MONUMENT_1x1");
		RoomSprite sMedium = new RoomSpriteXxX(sData, "MONUMENT_2x2", 2);
		RoomSprite  sLarge = new RoomSpriteXxX(sData, "MONUMENT_3x3", 3);
		
		FurnisherItemTile ss = new FurnisherItemTile(
				this,
				false,
				sSmall,
				AVAILABILITY.SOLID, 
				false
				);
		FurnisherItemTile sm = new FurnisherItemTile(
				this,
				false,
				sMedium,
				AVAILABILITY.SOLID, 
				false
				);
		FurnisherItemTile sl = new FurnisherItemTile(
				this,
				false,
				sLarge,
				AVAILABILITY.SOLID, 
				false
				);
		
	
		
		final FurnisherItemTile h1 = new FurnisherItemTile(
				this,
				false,
				sHead,
				AVAILABILITY.SOLID, 
				false);
		
		final FurnisherItemTile t1 = new FurnisherItemTile(
				this,
				true,
				sFoot, 
				AVAILABILITY.SOLID, 
				false).setData(Grave.ITEM_MARK);
		final FurnisherItemTile st = new FurnisherItemTile(
				this,
				false,
				sStone, 
				AVAILABILITY.SOLID, true);
		final FurnisherItemTile __ = new FurnisherItemTile(
				this,
				false,
				null, 
				AVAILABILITY.ROOM, false);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,h1,st,}, 
			{__,t1,__},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,h1,h1,st,}, 
			{__,t1,t1,__},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,h1,h1,st,h1,st}, 
			{__,t1,t1,__,t1,__},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,h1,h1,st,h1,h1,st}, 
			{__,t1,t1,__,t1,t1,st},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,h1,h1,st,h1,h1,st,h1,st}, 
			{__,t1,t1,__,t1,t1,__,t1,__},
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,h1,h1,st,h1,h1,st,h1,h1,st}, 
			{__,t1,t1,__,t1,t1,__,t1,t1,__},
		}, 6);
		
		flush(1,3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sm,sm},
			{sm,sm},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sl,sl,sl},
			{sl,sl,sl},
			{sl,sl,sl},
		}, 9);
		
		flush(3);

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
		return new GraveInstance(blue, area, init);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}


}
