package settlement.room.service.barber;

import java.io.IOException;

import init.constant.C;
import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherItemTools;
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

class Constructor extends Furnisher {

	private final ROOM_BARBER blue;
	
	public final static int IWORK = 1;
	
	final FurnisherStat latrines;
	final FurnisherStat workers;
	final FurnisherStat quality;

	protected Constructor(ROOM_BARBER blue, RoomInitData init) throws IOException {
		super(init, 2, 3, 88, 44);
		this.blue = blue;
		
		latrines = new FurnisherStat.FurnisherStatServices(this, blue);
		workers = new FurnisherStat.FurnisherStatEmployeesR(this, latrines, 1);
		quality = new FurnisherStat.FurnisherStatRelative (this, latrines);
		
		Json sp = init.data().json("SPRITES");
		
		RoomSprite sNick = new RoomSprite1x1(sp, "NICKNACK_1X1");
		RoomSprite sCentre = new RoomSprite1x1(sp, "TABLE_CENTRE_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 22;
			}
		};
		RoomSprite sCentreTop = new RoomSprite1x1(sp, "TABLE_CENTRE_TOP_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 22;
			}
		};
		RoomSprite sTable = new RoomSpriteCombo(sp, "TABLE_COMBO") {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (!SETT.ROOMS().fData.candle.is(it.tile())) {
					sNick.render(r, s, getData2(it), it, degrade, false);
				}
			}
			
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sNick.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};
		RoomSprite sTableC = new RoomSpriteCombo(sTable) {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				sCentre.render(r, s, getData2(it), it, degrade, false);
				sCentreTop.render(r, s, getData2(it), it, degrade, false);
				
			}
			
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sCentre.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};
		RoomSprite sChair = new RoomSprite1x1(sp, "CHAIR_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == sTableC;
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
				FSERVICE ss = blue.ll.service(it.tx(), it.ty());
				if (ss == null)
					return false;
				if (ss.findableReservedIs() || ss.findableReservedCanBe())
					return super.render(r, s, data, it, degrade, false);
				return false;
			};
			
		}.sData(22);
		RoomSprite sSeparator = new RoomSprite1x1(sp, "SEPARATOR_1X1") {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				DIR rot = rot(data);
				it.setOff(-rot.x()*C.TILE_SIZEH/2, -rot.y()*C.TILE_SIZEH/2);
				return super.render(r, s, data, it, degrade, isCandle);
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				rx -= d.x();
				ry -= d.y();
				d = d.next(-2);
				rx += d.x();
				ry += d.y();
				return item.sprite(rx, ry) == sTableC;
			}
		};

		
		final FurnisherItemTile tt = new FurnisherItemTile(this, false, sTable, AVAILABILITY.ROOM_SOLID, true);
		final FurnisherItemTile tc = new FurnisherItemTile(this, false, sTableC, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile oo = new FurnisherItemTile(this, true, sChair, AVAILABILITY.AVOID_PASS, false);
		oo.setData(IWORK);
		final FurnisherItemTile __ = new FurnisherItemTile(this, false, sSeparator, AVAILABILITY.ROOM_SOLID, false);
		
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,__,},
			{tc,oo,},
			{tt,__,},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,__,},
			{tc,oo,},
			{tc,oo,},
			{tt,__,},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,__,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tt,__,},
		}, 3);
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,__,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tt,__,},
		}, 4);
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,__,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tt,__,},
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,__,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tt,__,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tt,__,},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,__,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tt,__,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tt,__,},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{tt,__,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tt,__,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tc,oo,},
			{tt,__,},
		}, 10);
		
		
		flush(1, 3);
		
		FurnisherItemTools.makeUnder(this, sp, "CARPET_COMBO");
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
		return new Instance(blue, area, init);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	
	@Override
	public boolean isHeavy() {
		return true;
	}

}
