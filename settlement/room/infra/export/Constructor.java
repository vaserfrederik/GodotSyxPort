package settlement.room.infra.export;

import java.io.IOException;

import init.constant.C;
import init.sprite.SPRITES;
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
import settlement.tilemap.floor.Floors.Floor;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.OPACITY;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.sprite.SPRITE;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	private final ROOM_EXPORT blue;
	final FurnisherStat crates;
	private final Floor floor2;
	

	private static final int ICRATE = 1;
	private static final int ICANDLE = 2;
	
	boolean isCrate(int tx, int ty) {
		return SETT.ROOMS().fData.tileData.get(tx,ty) == ICRATE; 	
	}
	
	protected Constructor(ROOM_EXPORT blue, RoomInitData init)
			throws IOException {
		super(init, 1, 1, 88, 44);
		this.blue = blue;
		floor2 = SETT.FLOOR().map.read("FLOOR2", init.data());
		crates = new FurnisherStat(this, 1) {
			
			@Override
			public double get(AREA area, double fromItems) {
				return fromItems;
			}
			
			@Override
			public GText format(GText t, double value) {
				return GFORMAT.i(t, (int)value);
			}
		};
		
		Json sp = init.data().json("SPRITES");
		
		RoomSprite1x1 sCrate = new RoomSprite1x1(sp, "CRATE_1X1") {
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				renderCrate(r, s, data, it, degrade, isCandle);
				return false;
			};
			
		};
		
		RoomSprite sRoof = new RoomSpriteCombo(sp, "ROOF_COMBO") {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				s.setSoft();
				DIR rot = DIR.ORTHO.get(SETT.ROOMS().fData.item.get(it.tile()).rotation).next(-1);
				it.setOff(rot.x()*C.TILE_SIZEH, rot.y()*C.TILE_SIZEH);
				super.render(r, s, data, it, degrade, isCandle);
				s.setHard();
				return false;
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
			}
		};
		
		RoomSprite sCrateR = new RoomSprite1x1(sp, "CRATE_1X1") {
			
			
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				renderCrate(r, s, data, it, degrade, isCandle);
				return false;
			};
			
			@Override
			public  byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sRoof.getData(tx, ty, rx, ry, item, itemRan);
			};
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				sRoof.render(r, s, getData2(it), it, degrade, rotates);
			};
			
		}.sData(1);
		
		
		
		RoomSprite spriteCrate = new RoomSprite1x1(sCrate) {

			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				renderCrate(r, s, data, it, degrade, isCandle);
				return false;
			};
		};
		
		FurnisherItemTile cc = new FurnisherItemTile(
				this,
				true,
				spriteCrate, 
				AVAILABILITY.ROOM_SOLID, 
				false).setData(ICRATE);
		
		FurnisherItemTile ca = new FurnisherItemTile(
				this,
				false,
				spriteCrate, 
				AVAILABILITY.ROOM_SOLID, 
				true).setData(ICANDLE);
		
		FurnisherItemTile cr = new FurnisherItemTile(
				this,
				true,
				sCrateR, 
				AVAILABILITY.ROOM_SOLID, 
				false).setData(ICRATE);
		
		FurnisherItemTile rr = new FurnisherItemTile(
				this,
				false,
				sCrateR, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		FurnisherItemTile rc = new FurnisherItemTile(
				this,
				false,
				sCrateR, 
				AVAILABILITY.ROOM_SOLID, 
				true).setData(ICANDLE);
		
		FurnisherItemTile __ = new FurnisherItemTile(
				this,
				null, 
				AVAILABILITY.ROOM, 
				false);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__},
			{__,cc,ca,cc,__},
			{__,cc,rr,cr,__},
			{__,cc,rc,cr,__},
			{__,__,__,__,__},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__},
			{__,cc,ca,cc,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rc,cr,__},
			{__,__,__,__,__},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__},
			{__,cc,ca,cc,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rc,cr,__},
			{__,__,__,__,__},
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__},
			{__,cc,ca,cc,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rc,cr,__},
			{__,__,__,__,__},
		}, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__},
			{__,cc,ca,cc,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rc,cr,__},
			{__,__,__,__,__},
		}, 14);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__},
			{__,cc,ca,cc,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rc,cr,__},
			{__,__,__,__,__},
		}, 16);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__},
			{__,cc,ca,cc,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rr,cr,__},
			{__,cc,rc,cr,__},
			{__,__,__,__,__},
		}, 18);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__,__,__,__,__},
			{__,cc,ca,cc,__,cc,ca,cc,__},
			{__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rc,cr,__,cc,rc,cr,__},
			{__,__,__,__,__,__,__,__,__},
		}, 40);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__,__,__,__,__,__,__,__,__},
			{__,cc,ca,cc,__,cc,ca,cc,__,cc,ca,cc,__},
			{__,cc,rr,cr,__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rr,cr,__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rr,cr,__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rr,cr,__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rr,cr,__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rr,cr,__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rr,cr,__,cc,rr,cr,__,cc,rr,cr,__},
			{__,cc,rc,cr,__,cc,rc,cr,__,cc,rc,cr,__},
			{__,__,__,__,__,__,__,__,__,__,__,__,__},
		}, 60);
		
		flush(1);
		
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
	public RoomBlueprintImp blue() {
		// TODO Auto-generated method stub
		return blue;
	}
	
//	private final FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][] {
//		{0,0,0,0,0,0,0,0},
//		{0,1,1,0,0,0,0,0},
//		{0,0,1,1,1,0,0,0},
//		{0,0,0,1,1,1,1,0},
//		{0,0,0,1,1,1,1,0},
//		{0,0,1,1,1,0,0,0},
//		{0,1,1,0,0,0,0,0},
//		{0,0,0,0,0,0,0,0},
//		},
//		miniColor
//	);
//	
//	@Override
//	public COLOR miniColor(int tx, int ty) {
//		return miniC.get(tx, ty);
//	}
	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		for (DIR d : DIR.ALL) {
			if (!area.is(tx, ty, d)) {
				super.putFloor(tx, ty, upgrade, area);
				return;
			}
		}
		floor2.placeFixed(tx, ty);
	}
	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new ExportInstance(blue, area, init);
	}
	
	private void renderCrate(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, boolean isCandle) {

		if (blue().is(it.tile())){
			ExportInstance ins = blue.getter.get(it.tile());
			if (SETT.ROOMS().fData.tileData.get(it.tile()) == ICANDLE) {
				SPRITE ss = ins.resource() == null ? SPRITES.icons().m.cancel : ins.resource().icon();
				OPACITY.O99.bind();
				ss.render(r, it.x(), it.x()+C.TILE_SIZE, it.y(), it.y()+C.TILE_SIZE);
				OPACITY.unbind();
			}else if (SETT.ROOMS().fData.tileData.get(it.tile()) == ICRATE){
				int a = blue.crate(it.tx(), it.ty()).amount();
				
				if (a > 0 && ins.resource() != null) {
					blue.getter.get(it.tile()).resource().renderLaying(r, it.x(), it.y(), it.ran(), a);
				}
			}
		}
				
	};

}
