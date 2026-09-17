package settlement.room.knowledge.laboratory;

import java.io.IOException;

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
import settlement.room.sprite.RoomSpriteImp;
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
			return GFORMAT.i(t, (int)(value*blue.data.knowledgePerStation));
		}
	};

	public static final int WORK = 1;

	private final ROOM_LABORATORY blue;
	final RoomSprite1x1 schair;
	
	protected Constructor(ROOM_LABORATORY blue, RoomInitData init)
			throws IOException {
		super(init, 1, 2, 88, 44);
		this.blue = blue;
		
		Json sj = init.data().json("SPRITES");

		schair = new RoomSprite1x1(sj, "CHAIR_1X1") {

			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry) != this;
			}
		};
		
		final RoomSprite1x1 tablet = new RoomSprite1x1(sj, "TABLE_KNOWLEDGE_ONTOP_1X1");
		
		final RoomSpriteImp sTableWork = new RoomSpriteCombo(sj, "TABLE_COMBO") {
			
			final RoomSprite1x1 ontop = new RoomSprite1x1(sj, "WORK_TABLE_1X1") {
				
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return (item.sprite(rx+d.perpendicular().x()*2, ry+d.perpendicular().y()*2) == schair);
				}
			};
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (blue.job.used(it.tx(), it.ty())) {
					int i = 0;
					for (DIR d : DIR.ORTHO) {
						if (SETT.ROOMS().fData.sprite.is(it.tx(), it.ty(), d, schair)) {
							tablet.render(r, s, i, it, degrade, false);
							break;
						}
						i++;
					}
				}
				
				ontop.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return ontop.getData(tx, ty, rx, ry, item, itemRan);
			}
		};

		final RoomSprite sTableStorage = new RoomSpriteCombo(sTableWork) {
			
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (!SETT.ROOMS().fData.candle.is(it.tile()) && blue.is(it.tx(), it.ty())) {
					
					int f = (blue.data.usedD & 0x0FF);
					int d = (it.ran() & 0x03F);
					
					int m = 4;
					while(f > d && m-- > 0) {
						tablet.renderRandom(r, s, it, it.ran(), degrade);
						it.ranOffset(1, 0);
						f-= d;
					}
					
				}
			}
			
		};
		
		final RoomSprite sShelf = new RoomSprite1x1(sj, "SHELF_1X1") {
			
			final RoomSprite top = new RoomSprite1x1(sj, "SHELF_DECOR_1x1") {
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return item.sprite(rx, ry) == this;
				}
			};
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (blue.job.used(it.tx(), it.ty())) {
					top.render(r, s, getData2(it), it, degrade, false);
				}
				
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == this;
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};
		
		final RoomSprite sExtra = new RoomSprite1x1(sj, "WORK_STANDALONE_1X1");
		
		final FurnisherItemTile __ = null;
		final FurnisherItemTile sh = new FurnisherItemTile(this,true, sShelf, AVAILABILITY.ROOM_SOLID, true);
		final FurnisherItemTile st = new FurnisherItemTile(this,false, sTableStorage, AVAILABILITY.ROOM_SOLID, true);
		final FurnisherItemTile ch = new FurnisherItemTile(this,true, schair, AVAILABILITY.AVOID_PASS, true);
		final FurnisherItemTile ex = new FurnisherItemTile(this,true, sExtra, AVAILABILITY.ROOM_SOLID, true);
		final FurnisherItemTile ww = new FurnisherItemTile(this,false, sTableWork, AVAILABILITY.ROOM_SOLID, false);
		ex.setData(WORK);
		ww.setData(WORK);
		sh.setData(WORK);
		st.setData(WORK);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sh,st,ww,st,ex,}, 
			{__,ch,ch,ch,__,}, 
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sh,st,ww,ww,st,ex,}, 
			{__,ch,ch,ch,ch,__,}, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sh,st,ww,ww,ww,st,ex,}, 
			{__,ch,ch,ch,ch,ch,__,}, 
		}, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sh,st,ww,ww,ww,ww,st,ex,}, 
			{__,ch,ch,ch,ch,ch,ch,__,}, 
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{sh,st,ww,ww,ww,ww,ww,st,ex,}, 
			{__,ch,ch,ch,ch,ch,ch,ch,__,}, 
		}, 9);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,ch,ch,__,}, 
			{sh,st,ww, st,ex,}, 
			{sh,st,ww, st,ex,}, 
			{__,ch,ch,ch,__,}, 
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,ch,ch,ch,__,}, 
			{sh,st,ww,ww,st,ex,}, 
			{sh,st,ww,ww,st,ex,}, 
			{__,ch,ch,ch,ch,__,}, 
		}, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,ch,ch,ch,ch,__,}, 
			{sh,st,ww,ww,ww,st,ex,}, 
			{sh,st,ww,ww,ww,st,ex,}, 
			{__,ch,ch,ch,ch,ch,__,}, 
		}, 14);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,ch,ch,ch,ch,ch,__,}, 
			{sh,st,ww,ww,ww,ww,st,ex,}, 
			{sh,st,ww,ww,ww,ww,st,ex,}, 
			{__,ch,ch,ch,ch,ch,ch,__,}, 
		}, 16);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,ch,ch,ch,ch,ch,ch,ch,__,}, 
			{sh,st,ww,ww,ww,ww,ww,st,ex,}, 
			{sh,st,ww,ww,ww,ww,ww,st,ex,}, 
			{__,ch,ch,ch,ch,ch,ch,ch,__,}, 
		}, 18);
		
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
	public Room create(TmpArea area, RoomInit init) {
		return new LaboratoryInstance(blue, area, init);
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
