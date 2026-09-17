package settlement.room.spirit.temple;

import java.io.IOException;

import settlement.main.SETT;
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
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSprite1xN;
import settlement.room.sprite.RoomSpriteBoxN;
import settlement.room.sprite.RoomSpriteImp;
import settlement.room.sprite.RoomSpriteXxX;
import settlement.tilemap.floor.Floors.Floor;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.misc.CLAMP;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class TempleConstructor extends Furnisher{

	private final ROOM_TEMPLE blue;
	final FurnisherStat priests;
	final FurnisherStat worshippers;
	final FurnisherStat decor;
	final FurnisherStat grandure;
	final FurnisherStat space;
	
//	private final RoomSpriteBox sFloor;
//	private final RoomSpriteBox sAltar;
	private final FurnisherItemTile ca;
	final FurnisherItemTile ap;
	final FurnisherItemTile al;
	final FurnisherItemTile es;
	final FurnisherItemTile wo;
	
	protected TempleConstructor(ROOM_TEMPLE blue, RoomInitData init)
			throws IOException {
		super(init, 5, 5, 304, 240);
		this.blue = blue;
		
		priests = new FurnisherStat.FurnisherStatEmployees(this);
		worshippers = new FurnisherStat.FurnisherStatServices(this, blue);
		decor = new FurnisherStat.FurnisherStatRelative(this, worshippers, 0.7);
		grandure = new FurnisherStat(this) {
			
			@Override
			public double get(AREA area, double acc) {
				double d = 1.5*area.area()/Room.MAX_SIZE;
				return Math.pow(CLAMP.d(d, 0, 1), 0.5);
			}
			
			@Override
			public GText format(GText t, double value) {
				GFORMAT.perc(t, value);
				return t;
			}
		};
		space = new FurnisherStat(this) {
			
			@Override
			public double get(AREA area, double acc) {
				return acc;
			}
			
			@Override
			public double get(AREA area, double[] fromItems) {
				double p = fromItems[priests.index()];
				if (p == 0)
					return 1;
				
				double d = area.area()/(p*38);
				return Math.pow(CLAMP.d(d, 0, 1), 0.5);
			};
			
			@Override
			public GText format(GText t, double value) {
				GFORMAT.perc(t, value);
				return t;
			}
		};
		
		Floor path = SETT.FLOOR().map.read("FLOOR_PATH", init.data());
		
		Json sj = init.data().json("SPRITES");
		
		RoomSpriteBoxN sPedistal = new RoomSpriteBoxN(sj, "PEDISTAL_BOX") {
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
		};
		RoomSpriteBoxN sAltar = new RoomSpriteBoxN(sj, "ALTAR_BOX"){
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.get(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
			};
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				sPedistal.renderBelow(r, s, 0x0F, it, degrade);
			}
		};
		sAltar.sData(1);
		
		RoomSpriteImp sEmblemS = new RoomSpriteBoxN(sAltar) {
			RoomSpriteImp sEmblem = new RoomSprite1x1(sj, "EMBLEM_1X1");
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.get(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
			};
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				sEmblem.render(r, s, getData2(it), it, degrade, isCandle);
				if (blue.altar.get(it.tx(), it.ty()) != null)
					blue.altar.render(r, s, it);
				return false;
			}
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sEmblem.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		sEmblemS.sData(1);
		
		RoomSpriteImp sEmblemL = new RoomSpriteXxX(sj, "EMBLEM_2X2", 2);
		
		
		RoomSpriteImp sB0 = new RoomSprite1xN(sj, "NICHE_A_1X1", true);
		RoomSpriteImp sB1 = new RoomSprite1xN(sj, "NICHE_B_1X1", false);
		RoomSpriteImp sB2 = new RoomSprite1xN(sj, "NICHE_C_1X1", false);
		RoomSpriteImp sB3 = new RoomSprite1xN(sj, "NICHE_D_1X1", false);
		
		RoomSpriteImp sCS = new RoomSprite1x1(sj, "TORCH_1X1");
		
		RoomSpriteImp sCa = new RoomSpriteBoxN(sPedistal) {

			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				sCS.render(r, s, getData2(it), it, degrade, true);
				return false;
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sCS.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		
		RoomSpriteImp sSA = new RoomSprite1xN(sj, "COFFIN_A_1X1", false);
		RoomSpriteImp sSB = new RoomSprite1xN(sSA, true);
		RoomSpriteImp sSC = new RoomSprite1xN(sSA, false);
		
		wo = new FurnisherItemTile(
				this,
				false,
				sPedistal,
				AVAILABILITY.ROOM, 
				false);
		
		final FurnisherItemTile __ = new FurnisherItemTile(
				this,
				sPedistal,
				AVAILABILITY.ROOM, 
				false);
		
		es = new FurnisherItemTile(
				this,
				sEmblemS,
				AVAILABILITY.AVOID_LIKE_FUCK, 
				false);
		
		final FurnisherItemTile eb = new FurnisherItemTile(
				this,
				sEmblemL,
				AVAILABILITY.ROOM, 
				false);
		
		al = new FurnisherItemTile(
				this,
				sAltar,
				AVAILABILITY.AVOID_LIKE_FUCK, 
				false);
		
		ap = new FurnisherItemTile(
				this,
				sAltar,
				AVAILABILITY.AVOID_LIKE_FUCK, 
				false);
		
		final FurnisherItemTile b0 = new FurnisherItemTile(
				this,
				sB0,
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		final FurnisherItemTile b1 = new FurnisherItemTile(
				this,
				sB1,
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		final FurnisherItemTile b2 = new FurnisherItemTile(
				this,
				sB2,
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		final FurnisherItemTile b3 = new FurnisherItemTile(
				this,
				sB3,
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		ca = new FurnisherItemTile(
				this,
				sCa,
				AVAILABILITY.ROOM_SOLID, 
				true);
		
		final FurnisherItemTile sA = new FurnisherItemTile(
				this,
				sSA,
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		final FurnisherItemTile sB = new FurnisherItemTile(
				this,
				sSB,
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		final FurnisherItemTile sC = new FurnisherItemTile(
				this,
				sSC,
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		final FurnisherItemTile cs = new FurnisherItemTile(
				this,
				sCS,
				AVAILABILITY.ROOM_SOLID, 
				true);
		
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,wo,wo,__,wo,wo,__},
			{__,__,al,ap,al,__,__},
			{wo,ca,al,es,al,ca,wo},
			{__,__,al,ap,al,__,__},
			{__,wo,wo,__,wo,wo,__},
		}, 1, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{wo,wo,wo,__,wo,__,wo,wo,wo},
			{wo,__,al,ap,al,ap,al,__,wo},
			{__,ca,al,es,al,es,al,ca,__},
			{wo,__,al,ap,al,ap,al,__,wo},
			{wo,wo,wo,__,wo,__,wo,wo,wo},
		}, 1.8, 1.8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{wo,wo,wo,__,wo,__,wo,__,wo,wo,wo},
			{wo,__,al,ap,al,ap,al,ap,al,__,wo},
			{wo,ca,al,es,al,es,al,es,al,ca,wo},
			{wo,__,al,ap,al,ap,al,ap,al,__,wo},
			{wo,wo,wo,__,wo,__,wo,__,wo,wo,wo},
		}, 2.2, 2.2);
		
		flush(3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sB,sC}, 
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sB,sA,sC}, 
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sB,sA,sA,sC}, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{b3,b1,b0,b1,b3}, 
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{b3,b1,b2,b0,b1,b2,b3}, 
		}, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{b3,b1,b2,b1,b0,b1,b2,b1,b3}, 
		}, 9);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{b3,b1,b2,b1,b1,b0,b1,b1,b2,b1,b3}, 
		}, 11);
		
		flush(3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cs}, 
		}, 1);
		
		flush(3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{eb,eb},
			{eb,eb},
		}, 1);
		
		flush(3);
		
		FurnisherItemTools.makeFloor(this, path);
		
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
		TempleInstance ins = new TempleInstance(blue, area, init);
		for (COORDINATE c : ins.body()) {
			if (ins.is(c) && SETT.ROOMS().fData.tile.get(c) == ca) {
				SETT.LIGHTS().candle(c.x(), c.y(), 0);
			}
		}
		return ins;
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
//		{0,0,0,1,1,0,0,0},
//		{0,0,1,0,0,1,0,0},
//		{0,1,0,0,0,0,1,0},
//		{0,1,1,1,1,1,1,0},
//		{0,1,0,1,1,0,1,0},
//		{0,0,0,1,1,0,0,0},
//		{0,0,0,0,0,0,0,0},
//		},
//		miniColor
//	);
//	
//	@Override
//	public COLOR miniColor(int tx, int ty) {
//		return miniC.get(tx, ty);
//	}
//	
//	@Override
//	public COLOR miniColorPimped(ColorImp origional, int tx, int ty, boolean northern, boolean southern) {
//		// TODO Auto-generated method stub
//		return super.miniColorPimped(origional, tx, ty, northern, southern);
//	}
	
	
	


}
