package settlement.room.knowledge.library;

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
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSpriteCombo;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	public final FurnisherStat workers = new FurnisherStat(this) {
		
		@Override
		public double get(AREA area, double fromItems) {
			return fromItems;
		}
		
		@Override
		public GText format(GText t, double value) {
			return GFORMAT.i(t, (int)value);
		}
	};
	
	
	public final FurnisherStat knowledge = new FurnisherStat(this) {
		
		@Override
		public double get(AREA area, double fromItems) {
			return fromItems;
		}
		
		@Override
		public GText format(GText t, double value) {
			return GFORMAT.f0(t, value*blue.data.knowledgePerStation);
		}
		
		@Override
		public double get(AREA area, double[] fromItems) {
			return super.get(area, fromItems)*efficiency.get(area, fromItems);
		};
	};
	
	public final FurnisherStat efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers);
	
	private final ROOM_LIBRARY blue;
	final FurnisherItemTile ww;
	final RoomSprite sStool;
	
	protected Constructor(ROOM_LIBRARY blue, RoomInitData init)
			throws IOException {
		super(init, 2, 3, 88, 44);
		this.blue = blue;
		
		Json sj = init.data().json("SPRITES");
		

		final RoomSprite1x1 sUsed = new RoomSprite1x1(sj, "WORK_USED_1x1");
		
		final RoomSprite sWork = new RoomSpriteCombo(sj, "TABLE_COMBO") {

			private final RoomSprite1x1 available = new RoomSprite1x1(sj, "WORK_UNUSED_1x1");
			private final RoomSprite1x1 dec = new RoomSprite1x1(sj, "TABLE_DECOR_1x1");
			

			@Override
			public  void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (blue.is(it.tile())) {
					it.ranOffset(1, 0);
					RoomSprite1x1 sp = null;
					if (blue.job.used(it.tx(), it.ty())){
						sp = sUsed;
					}else if(blue.consumption().ins().size() > 0 && blue.consumption().stored(blue.consumption().ins().get(0)).get(blue.get(it.tx(), it.ty())) > 0) {
						sp = available;
					}
					
					for (int i = 0; i < DIR.ORTHO.size(); i++) {
						if (SETT.ROOMS().fData.sprite.is(it.tx(), it.ty(), DIR.ORTHO.get(i), sStool)) {
							if (sp != null)
								sp.render(r, s, i, it, degrade, false);
							if ((it.ran() & 0b011) == 1) {
								it.ranOffset(1, 0);
								dec.render(r, s, (i+2)%4, it, degrade, false);
							}
							break;
						}
					}
					
				}
			};
		};
		
		sStool = new RoomSprite1x1(sj, "CHAIR_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == sWork;
			}
		};
		
		final RoomSprite sShelf = new RoomSprite1x1(sj, "SHELF_1X1") {

			private final RoomSprite1x1 ontop = new RoomSprite1x1(sj, "SHELF_DECOR_1x1");
			
			@Override
			public  void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (blue.is(it.tile())) {
					int f = blue.data.usedD & 0x0FF;
					if (f > (it.ran() & 0x0FF)) {
						it.ranOffset(1, 0);
						ontop.render(r, s, data, it, degrade, false);
					}
				}
			};
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				if (item.width() > 2 && item.height() > 2) {
					if ((DIR.ORTHO.get(item.rotation).x() * d.x() != 0 || DIR.ORTHO.get(item.rotation).y() * d.y() != 0) && item.sprite(rx, ry) == this)
						return true;
					return false;
				}
				return DIR.ORTHO.get(item.rotation) == d;
			}
		};

		final RoomSprite sTorch = new RoomSprite1x1(sj, "TORCH_1x1") {
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (!SETT.ROOMS().fData.candle.is(it.tile()))
					sUsed.renderRandom(r, s, it, it.ran(), degrade);
			}
		};
		final RoomSprite sNick = new RoomSprite1x1(sj, "DECOR_1x1");
		
		final FurnisherItemTile ss = new FurnisherItemTile(this,false, sShelf, AVAILABILITY.ROOM_SOLID, false);
		ww = new FurnisherItemTile(this,false, sWork, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile st = new FurnisherItemTile(this,true, sStool, AVAILABILITY.AVOID_PASS, false);
		final FurnisherItemTile ca = new FurnisherItemTile(this,false, sTorch, AVAILABILITY.ROOM_SOLID, true);
		final FurnisherItemTile ni = new FurnisherItemTile(this,false, sNick, AVAILABILITY.ROOM_SOLID, false);
		
		final FurnisherItemTile __ = null;
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss, ss, ww, ca,},
			{__, __, st, __,}, 
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ss,ww,ww,ca,},
			{__,__,__,st,st,__,}, 
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ss,ss,ww,ww,ww,ca,},
			{__,__,__,__,st,st,st,__,}, 
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ss,ss,ss,ww,ww,ww,ww,ca,},
			{__,__,__,__,__,st,st,st,st,__,}, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ss,ss,ss,ss,ss,ss,ww,ww,ww,ww,ww,ca,},
			{__,__,__,__,__,__,st,st,st,st,st,__,}, 
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__, __, st, __,}, 
			{ss, ss, ww, ni,},
			{ss, ss, ww, ca,},
			{__, __, st, __,}, 
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,st,st,__,}, 
			{ss,ss,ss,ww,ww,ni,},
			{ss,ss,ss,ww,ww,ca,},
			{__,__,__,st,st,__,}, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,st,st,st,__,}, 
			{ss,ss,ss,ss,ww,ww,ww,ni,},
			{ss,ss,ss,ss,ww,ww,ww,ca,},
			{__,__,__,__,st,st,st,__,}, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__,st,st,st,st,__,}, 
			{ss,ss,ss,ss,ss,ww,ww,ww,ww,ni,},
			{ss,ss,ss,ss,ss,ww,ww,ww,ww,ca,},
			{__,__,__,__,__,st,st,st,st,__,}, 
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__,__,st,st,st,st,st,__,}, 
			{ss,ss,ss,ss,ss,ss,ww,ww,ww,ww,ww,ca,},
			{ss,ss,ss,ss,ss,ss,ww,ww,ww,ww,ww,ca,},
			{__,__,__,__,__,__,st,st,st,st,st,__,}, 
		}, 10);
		
		
		flush(1, 3);
		
		FurnisherItemTools.makeUnder(this, sj, "CARPET_COMBO");
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
		return new LibraryInstance(blue, area, init);
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
