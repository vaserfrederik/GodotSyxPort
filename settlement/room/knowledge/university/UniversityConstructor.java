package settlement.room.knowledge.university;

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
import settlement.room.sprite.RoomSpriteCombo;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class UniversityConstructor extends Furnisher{


	public final FurnisherStat students = new FurnisherStat.FurnisherStatI(this, 1);
	public final FurnisherStat quality = new FurnisherStat.FurnisherStatEfficiency(this, students, 1);
	
	private final ROOM_UNIVERSITY blue;
	
	static final int IWORK = 1;
	static final int IWORKE = 2;
	
	
	protected UniversityConstructor(ROOM_UNIVERSITY blue, RoomInitData init)
			throws IOException {
		super(init, 2, 2, 88, 44);
		this.blue = blue;
		
		Json sp = init.data().json("SPRITES");
		

	
		RoomSprite sBench = new RoomSprite1x1(sp, "BENCH_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				for (int i = 0; i < 3; i++) {
					if (item.sprite(rx + d.x()*i, ry + d.y()*i) != null && item.sprite(rx + d.x()*i, ry + d.y()*i).sData() == 2)
						return true;
				}
				return false;
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				type().renderOverlay(
						x, y, r, AVAILABILITY.AVOID_PASS, 
						0, data, true);
			}
		};
		
		RoomSprite sCarpet = new RoomSpriteCombo(sp, "CARPET_COMBO") {
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
			}
		}.sData(1);
		RoomSprite sCarpetCandle = new RoomSpriteCombo(sCarpet) {
			
			final RoomSprite ca = new RoomSprite1x1(sp, "TORCH_1X1");
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return ca.render(r, s, getData2(it), it, degrade, isCandle);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return ca.getData(tx, ty, rx, ry, item, itemRan);
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
			}
		}.sData(1);
		
		RoomSprite podium = new RoomSpriteCombo(sp, "PODIUM_COMBO") {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				data = getData2(it);
				if (data != 0)
					sCarpet.renderBelow(r, s, getData2(it), it, degrade);
				return false;
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				
				int m = 0;
				if (!isC(rx, ry, item))
					return 0;
				for (int di = 0; di < DIR.ORTHO.size(); di++) {
					DIR d = DIR.ORTHO.get(di);
					if (isC(rx+d.x(), ry+d.y(), item))
						m |= d.mask();
				}
				return (byte) m;
			}
			
			private boolean isC(int rx, int ry, FurnisherItem item) {
				for (int di = 0; di < DIR.ORTHO.size(); di++) {
					DIR d = DIR.ORTHO.get(di);
					if (item.sprite(rx+d.x(), ry+d.y()) != this)
						return false;
				}
				return true;
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 2;
			}
			
		}.sData(2);
		
		RoomSprite sShelf = new RoomSprite1x1(sp, "SHELF_1X1") {
			
			final RoomSprite top = new RoomSprite1x1(sp, "SHELF_TOP_1X1");
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				if (item.width() == 1 || item.height() == 1)
					return d.orthoID() == item.rotation;
				return item.sprite(rx, ry) == this && (d.orthoID() == item.rotation || d.perpendicular().orthoID() == item.rotation);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				it.ranOffset(1, 1);
				return top.render(r, s, data, it, degrade, isCandle);
			}
			
		};
		
		final FurnisherItemTile po = new FurnisherItemTile(this,false, podium, AVAILABILITY.AVOID_PASS, false);
		final FurnisherItemTile pc = new FurnisherItemTile(this,false, podium, AVAILABILITY.AVOID_PASS, false);
		pc.setData(IWORKE);
		final FurnisherItemTile bb = new FurnisherItemTile(this,true, sBench, AVAILABILITY.AVOID_PASS, false);
		bb.setData(IWORK);
		final FurnisherItemTile sh = new FurnisherItemTile(this,false, sShelf, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile ca = new FurnisherItemTile(this,false, sCarpetCandle, AVAILABILITY.ROOM_SOLID, true);
		final FurnisherItemTile __ = null;
		final FurnisherItemTile cc = new FurnisherItemTile(this,false, sCarpet, AVAILABILITY.ROOM, false);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{bb,po,pc,po,bb},
			{ca,bb,bb,bb,ca}, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{bb,po,pc,po,bb},
			{bb,po,po,po,bb},
			{cc,bb,bb,bb,cc}, 
			{ca,bb,cc,bb,ca}, 
		}, 9);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{bb,po,po,pc,po,po,bb},
			{bb,po,po,po,po,po,bb},
			{cc,bb,bb,cc,bb,bb,cc}, 
			{ca,bb,bb,cc,bb,bb,ca}, 
		}, 13);
		
		new FurnisherItem(new FurnisherItemTile[][] {

			{bb,bb,po,po,po,po,po,bb,bb,}, 
			{cc,cc,po,po,pc,po,po,cc,cc},
			{bb,bb,po,po,po,po,po,bb,bb,},
			{cc,cc,bb,bb,cc,bb,bb,cc,cc,},
			{ca,cc,bb,bb,cc,bb,bb,cc,ca,},
			{__,__,bb,bb,cc,bb,bb,__,__,},
		}, 21);
		
		flush(1, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ca }, 
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ca, sh }, 
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ca, sh, sh }, 
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ca, sh, sh, sh }, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ca, sh, sh, sh, sh }, 
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ca, sh, }, 
			{ ca, sh, }, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ca, sh, sh, }, 
			{ ca, sh, sh, }, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ca, sh, sh, sh, }, 
			{ ca, sh, sh, sh, }, 
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ca, sh, sh, sh, sh }, 
			{ ca, sh, sh, sh, sh }, 
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ ca, sh, sh, sh, sh, sh }, 
			{ ca, sh, sh, sh, sh, sh }, 
		}, 12);
		
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
		return new UniversityInstance(blue, area, init);
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
//		{0,1,1,1,1,1,1,1},
//		{0,1,1,1,1,1,0,1},
//		{0,1,1,1,1,1,0,1},
//		{0,1,1,1,1,1,0,1},
//		{0,1,1,1,1,1,0,1},
//		{0,1,1,1,1,1,1,1},
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
