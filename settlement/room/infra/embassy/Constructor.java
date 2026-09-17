package settlement.room.infra.embassy;

import java.io.IOException;

import init.sprite.SPRITES;
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
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSpriteCombo;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	public final FurnisherStat workers = new FurnisherStat.FurnisherStatEmployees(this);
	public final FurnisherStat efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers);
	private final ROOM_EMBASSY blue;
	static final int IWORK = 3;
	

	
	
	protected Constructor(ROOM_EMBASSY blue, RoomInitData init)
			throws IOException {
		super(init, 3, 2);
		this.blue = blue;
		
		final Json sp = init.data().json("SPRITES");
		
		
		
		final RoomSprite sDecor = new RoomSprite1x1(sp, "DECOR_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				
				if (d.orthoID() == item.rotation || d.perpendicular().orthoID() == item.rotation) {
					if (item.get(rx, ry) != null && item.get(rx, ry).sprite instanceof RoomSpriteCombo)
						return true;
				}
				
				return false;
			}
		};
		final RoomSprite sTable = new RoomSpriteCombo(sp, "TABLE_COMBO");
		
		final RoomSprite sTableWork = new RoomSpriteCombo(sTable) {
			
			final RoomSprite top = new RoomSprite1x1(sp, "TABLE_TOP_1X1") {
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					if (d.orthoID() == item.rotation || d.perpendicular().orthoID() == item.rotation) {
						if ( d.orthoID() == item.rotation && item.get(rx, ry) == null)
							return true;
						if (item.get(rx, ry) != null && item.get(rx, ry).sprite instanceof RoomSpriteCombo)
							return true;
					}
					
					return false;
				}
			};
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (SETT.ROOMS().fData.candle.is(it.tile()))
					return;
				top.render(r, s, getData2(it), it, degrade, false);
			};
			

			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		
		final RoomSprite sTableDec = new RoomSpriteCombo(sTable) {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (SETT.ROOMS().fData.candle.is(it.tile()))
					return;
				sDecor.render(r, s, getData2(it), it, degrade, false);
			};
			

			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sDecor.getData(tx, ty, rx, ry, item, itemRan);
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				super.renderPlaceholder(r, x, y, data, tx, ty, rx, ry, item);
				if (item.width() == 1 || item.height() == 1);
					SPRITES.cons().ICO.arrows.get(item.rotation);
			}
		};
		
		final RoomSprite sShelf = new RoomSprite1x1(sp, "SHELF_1X1") {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (SETT.ROOMS().fData.candle.is(it.tile()))
					return;
				sDecor.render(r, s, data, it, degrade, false);
			};
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				if (item.width() > 1 && item.height() > 1) {
					if ((DIR.ORTHO.get(item.rotation).x() * d.x() != 0 || DIR.ORTHO.get(item.rotation).y() * d.y() != 0) && item.sprite(rx, ry) == this)
						return true;
					return false;
				}
				return DIR.ORTHO.get(item.rotation) == d;
			}

		};
		
		final RoomSprite sRes = new RoomSprite1x1(sp, "TABLE_1X1") {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				EmbassyInstance ins = blue.getter.get(it.tx(), it.ty());
				if (ins != null && blue.consumption().ins().size() == 0) {
					long iran = it.bigRan();
					int ri = (int) ((iran & 15)%blue.consumption().ins().size());
					iran = iran >> 4;
					double dam = (iran & 0xFF)/(double)0x0FF;
					
					
					int am = 8*blue.consumption().stored(blue.consumption().ins().get(ri)).get(ins)/Constructor.this.blue.maxRes(ri, ins);
					am *= 0.5+dam;
					if (am > 0) {
						blue.consumption().ins().get(ri).resource.renderLaying(r, it.x(), it.y(), it.ran(), am);
					}
				}
			};

		};
		
		final RoomSprite sStool = new RoomSprite1x1(sp, "STOOL_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteCombo;
			}
		};

		final FurnisherItemTile rr = new FurnisherItemTile(this,false, sRes, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile ww = new FurnisherItemTile(this,false, sTableWork, AVAILABILITY.ROOM_SOLID, true);
		final FurnisherItemTile dd = new FurnisherItemTile(this,false, sTableDec, AVAILABILITY.ROOM_SOLID, true);
		final FurnisherItemTile ch = new FurnisherItemTile(this,true, sStool, AVAILABILITY.AVOID_PASS, false);
		final FurnisherItemTile sh = new FurnisherItemTile(this,false, sShelf, AVAILABILITY.AVOID_PASS, true);
		ww.setData(IWORK);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,ch,rr,},
			{ dd,ww,dd,},
		}, 1);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,ch,ch,rr,},
			{ dd,ww,ww,dd,},
		}, 2);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,ch,ch,ch,rr,},
			{ dd,ww,ww,ww,dd,},
		}, 3);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,ch,ch,ch,ch,rr,},
			{ dd,ww,ww,ww,ww,dd,},
		}, 4);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,ch,ch,ch,ch,ch,rr,},
			{ dd,ww,ww,ww,ww,ww,dd,},
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,ch,rr,},
			{ dd,ww,dd,},
			{ dd,ww,dd,},
			{ rr,ch,rr,},
		}, 2);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,ch,ch,rr,},
			{ dd,ww,ww,dd,},
			{ dd,ww,ww,dd,},
			{ rr,ch,ch,rr,},
		}, 4);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,ch,ch,ch,rr,},
			{ dd,ww,ww,ww,dd,},
			{ dd,ww,ww,ww,dd,},
			{ rr,ch,ch,ch,rr,},
		},6);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,ch,ch,ch,ch,rr,},
			{ dd,ww,ww,ww,ww,dd,},
			{ dd,ww,ww,ww,ww,dd,},
			{ rr,ch,ch,ch,ch,rr,},
		}, 8);
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,ch,ch,ch,ch,ch,rr,},
			{ dd,ww,ww,ww,ww,ww,dd,},
			{ dd,ww,ww,ww,ww,ww,dd,},
			{ rr,ch,ch,ch,ch,ch,rr,},
		}, 10);
		
		flush(3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,sh,},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,sh,sh,},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,sh,sh,sh,},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,sh,sh,sh,sh,sh,},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,sh,},
			{ rr,sh,},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,sh,sh,},
			{ rr,sh,sh,},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,sh,sh,sh,},
			{ rr,sh,sh,sh,},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,sh,sh,sh,sh,},
			{ rr,sh,sh,sh,sh,},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ rr,sh,sh,sh,sh,sh,},
			{ rr,sh,sh,sh,sh,sh,},
		}, 10);
		
		flush(3);
		
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
		return new EmbassyInstance(blue, area, init);
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
