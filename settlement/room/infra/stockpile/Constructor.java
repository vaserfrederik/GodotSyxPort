package settlement.room.infra.stockpile;

import java.io.IOException;

import init.resources.RESOURCE;
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
import settlement.room.main.job.StorageCrate;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.file.Json;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	final FurnisherStat storage = new FurnisherStat(this) {
		
		@Override
		public double get(AREA area, double fromItems) {
			return fromItems*(blue.upgrades().boost(0)-1);
		}
		
		@Override
		public GText format(GText t, double value) {
			return GFORMAT.i(t, (int)value);
		}
	};
	private final ROOM_STOCKPILE blue;
	
	private final FurnisherItemTile cr;
	
	boolean isCrate(int tx, int ty) {
		return SETT.ROOMS().fData.tile.is(tx, ty, cr);
	}
	
	protected Constructor(ROOM_STOCKPILE blue, RoomInitData init)
			throws IOException {
		super(init, 1, 1, 88, 44);
		this.blue = blue;
		
		final Json sp = init.data().json("SPRITES");
		if (false) {
			//add crate count stat as well as storage.
		}
		final RoomSprite spriteCrate = new RoomSprite1x1(sp, "CRATE_BOTTOM_1X1") {
			
			final RoomSprite top = new RoomSprite1x1(sp, "CRATE_TOP_1X1");
			final RoomSprite topf = new RoomSprite1x1(sp, "CRATE_TOP_FOOD_1X1");
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				RoomSprite top = this.top;
				if (SETT.ROOMS().STOCKPILE.is(it.tile())) {
					StockpileInstance ins = blue.getter.get(it.tx(), it.ty());
					StorageCrate cr = blue.crate.get(it.tx(), it.ty(),ins, ins.sdata);
					RESOURCE res = cr.resource();
					if (res != null && RESOURCES.EDI().is(res)) {
						top = topf;
					}
				}
				top.render(r, s, data, it, degrade, rotates);
			}
			
			@Override
			public boolean render(snake2d.SPRITE_RENDERER r, ShadowBatch s, int data, util.rendering.RenderData.RenderIterator it, double degrade, boolean isCandle) {
				super.render(r, s, data, it, degrade, false);
				if (SETT.ROOMS().STOCKPILE.is(it.tile())) {
					StockpileInstance ins = blue.getter.get(it.tx(), it.ty());
					StorageCrate cr =blue.crate.get(it.tx(), it.ty(),ins, ins.sdata);
					RESOURCE res = cr.resource();
					if (res != null) {
						double a = cr.amount();
						res.renderLayingRel(r, it.x(), it.y(), it.ran(), a/blue.upgrades().boost(SETT.ROOMS().STOCKPILE.getter.get(it.tx(), it.ty()).upgrade()));
					}
				}
				return false;
				
			};
		};
		final RoomSprite spriteMisc = new RoomSprite1x1(sp, "MISC_1X1");
		
		cr = new FurnisherItemTile(
				this,
				true,
				spriteCrate, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		final FurnisherItemTile mm = new FurnisherItemTile(
				this,
				false,
				spriteMisc, 
				AVAILABILITY.ROOM_SOLID, 
				true);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,mm},
		}, 2, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,cr,mm},
		}, 3,2);

		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,cr,cr,mm},
		}, 4,3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,cr,cr,cr,mm},
		}, 5,4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,cr,cr,cr,cr,mm},
		}, 6,5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,cr,cr,cr,cr,cr,mm},
		}, 7,6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,cr},
			{mm,mm},
		}, 4,2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,cr},
			{cr,cr},
			{mm,mm},
		}, 6,4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,cr},
			{cr,cr},
			{cr,cr},
			{mm,mm},
		}, 8,6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,cr},
			{cr,cr},
			{cr,cr},
			{cr,cr},
			{mm,mm},
		}, 10,8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,cr},
			{cr,cr},
			{cr,cr},
			{cr,cr},
			{cr,cr},
			{mm,mm},
		}, 12,10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cr,cr},
			{cr,cr},
			{cr,cr},
			{cr,cr},
			{cr,cr},
			{cr,cr},
			{mm,mm},
		}, 14,12);
	
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
	public RoomBlueprintImp blue() {
		return blue;
	}
	
	@Override
	public boolean isHeavy() {
		return true;
	}
	
//	private final FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][] {
//		{0,0,0,0,0,0,0,0},
//		{0,0,0,0,0,0,0,0},
//		{0,1,1,1,1,1,1,0},
//		{0,0,0,0,0,0,0,0},
//		{0,0,0,0,0,0,0,0},
//		{0,0,0,0,0,0,0,0},
//		{0,1,1,1,1,1,1,0},
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
	public Room create(TmpArea area, RoomInit init) {
		return new StockpileInstance(blue, area, init);
	}

}
