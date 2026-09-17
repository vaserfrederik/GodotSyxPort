package settlement.room.law.court;

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
import settlement.room.sprite.RoomSpriteCombo;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	private final ROOM_COURT blue;
	
	final FurnisherStat prisoners = new FurnisherStat.FurnisherStatI(this, 1);
	final FurnisherStat workers = new FurnisherStat.FurnisherStatI(this);
	final FurnisherStat spectators = new FurnisherStat.FurnisherStatI(this);
	final static int codeWork = 1;
	final static int codeCriminal = 2;
	final static int codeSpectator = 3;
	final static int distance = 4;
	
	
	protected Constructor(ROOM_COURT blue, RoomInitData init)
			throws IOException {
		super(init, 2, 3, 88, 44);
		this.blue = blue;
		Json sp = init.data().json("SPRITES");
		
		station(sp);
		bench(sp);
		
	}
	
	private void station(Json sp) throws IOException {
		
		RoomSprite table = new RoomSpriteCombo(sp, "TABLE_COMBO") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == this;
			};
		};
		RoomSprite carpets = new RoomSpriteCombo(sp, "CARPET_COMBO") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == this;
			};
		};
		RoomSprite candle = new RoomSprite1x1(sp, "TORCH_1X1") {
			
			final RoomSprite1x1 top = new RoomSprite1x1(sp, "GEM_1X1");
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
		RoomSprite pedistal = new RoomSpriteCombo(sp, "STAND_COMBO") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == this;
			};
		};
		
		RoomSprite chair = new RoomSprite1x1(sp, "CHAIR_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == table;
			}
		};
		
		RoomSprite decor = new RoomSprite1x1(sp, "DECOR_A_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
			}
		}.sData(1);
		
		RoomSprite decorC = new RoomSprite1x1(sp, "DECOR_B_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
			}
		}.sData(1);
		
		final FurnisherItemTile xx = new FurnisherItemTile(
				this,
				table, 
				AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile cc = new FurnisherItemTile(
				this,
				carpets, 
				AVAILABILITY.AVOID_PASS, false);
		final FurnisherItemTile ca = new FurnisherItemTile(
				this,
				candle, 
				AVAILABILITY.ROOM_SOLID, true);
		final FurnisherItemTile ii = new FurnisherItemTile(
				this,
				chair, 
				AVAILABILITY.AVOID_PASS, false).setData(codeWork);
		final FurnisherItemTile pp = new FurnisherItemTile(
				this,
				true,
				pedistal, 
				AVAILABILITY.AVOID_PASS, false).setData(codeCriminal);
		final FurnisherItemTile dd = new FurnisherItemTile(
				this,
				decor, 
				AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile dc = new FurnisherItemTile(
				this,
				decorC, 
				AVAILABILITY.ROOM_SOLID, false);
		
		final FurnisherItemTile __ = new FurnisherItemTile(
				this,
				null, 
				AVAILABILITY.ROOM, false);
		
		final FurnisherItemTile _r = new FurnisherItemTile(
				this,
				true,
				null, 
				AVAILABILITY.ROOM, false);

		new FurnisherItem(new FurnisherItemTile[][] {
			{ca,dd,dc,dd,ca}, 
			{_r,__,ii,__,_r}, 
			{xx,xx,xx,xx,xx}, 
			{xx,xx,xx,xx,xx}, 
			{cc,cc,cc,cc,cc}, 
			{__,__,pp,__,__}, 
		}, 1, 1);

		
		flush(3);
		
	}
	
	private void bench(Json sp) throws IOException {
		
		final RoomSprite ssa = new RoomSprite1xN(sp, "BENCH_A_1X1", true);
		final RoomSprite ssb = new RoomSprite1xN(sp, "BENCH_B_1X1", false);
		final RoomSprite ssc = new RoomSprite1xN(sp, "BENCH_C_1X1", false);

		RoomSprite candle = new RoomSprite1x1(sp, "TORCH_1X1");
		
		final FurnisherItemTile ss = new FurnisherItemTile(
				this,
				true,
				ssa, 
				AVAILABILITY.PENALTY4, false).setData(codeSpectator);
		final FurnisherItemTile sc = new FurnisherItemTile(
				this,
				true,
				ssb, 
				AVAILABILITY.PENALTY4, false).setData(codeSpectator);
		final FurnisherItemTile se = new FurnisherItemTile(
				this,
				true,
				ssc, 
				AVAILABILITY.PENALTY4, false).setData(codeSpectator);
		final FurnisherItemTile ca = new FurnisherItemTile(
				this,
				candle, 
				AVAILABILITY.ROOM_SOLID, true);
		
		
		
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ca,ss,se,ca,},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ca,ss,sc,se,ca,},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ca,ss,sc,sc,se,ca,},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ca,ss,sc,sc,sc,se,ca,},
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ca,ss,sc,sc,sc,sc,se,ca,},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ca,ss,sc,sc,sc,sc,sc,se,ca,},
		}, 7);
		
		flush(1);
		
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
	public Room create(TmpArea area, RoomInit init) {
		return new CourtInstance(blue, area, init);
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
//		{0,1,1,1,0,0,0,0},
//		{0,1,1,1,0,0,0,0},
//		{0,1,1,1,1,1,1,0},
//		{0,1,1,1,1,1,1,0},
//		{0,1,1,1,0,0,0,0},
//		{0,1,1,1,0,0,0,0},
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
