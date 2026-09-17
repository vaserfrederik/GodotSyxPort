package settlement.room.infra.elderly;

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
import settlement.room.sprite.RoomSpriteBoxN;
import settlement.room.sprite.RoomSpriteCombo;
import settlement.room.sprite.RoomSpriteImp;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class ResthomeConstructor extends Furnisher{


	public final FurnisherStat stations = new FurnisherStat.FurnisherStatI(this, 1);
	public final FurnisherStat quality = new FurnisherStat.FurnisherStatRelative(this, stations);
	
	private final ROOM_RESTHOME blue;
	
	static final int ITABLE = 1;
	static final int ISTAGE = 2;
	static final int ICHAIR = 3;
	
	
	protected ResthomeConstructor(ROOM_RESTHOME blue, RoomInitData init)
			throws IOException {
		super(init, 4, 2, 88, 44);
		this.blue = blue;
		
		Json js = init.data().json("SPRITES");
		final RoomSpriteImp sChair_wall = new RoomSprite1x1(js, "CHAIR_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return d.perpendicular().orthoID() == item.rotation;
			}
		};
		
		final RoomSprite sChair_table = new RoomSprite1x1(sChair_wall) {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.get(rx, ry) != null && item.get(rx, ry).data() == ITABLE;
			}
		};
		
		final RoomSprite1x1 sOnTopDecor = new RoomSprite1x1(js, "ON_TOP_DECOR_1X1");
		final RoomSprite1x1 sTableSingle = new RoomSprite1x1(js, "TABLE_1X1") {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				boolean ret = super.render(r, s, data, it, degrade, isCandle);
				if (!isCandle) {
					sOnTopDecor.renderRandom(r, null, it, it.ran(), degrade);
				}
				return ret;
			}
		};
		
		final RoomSprite1x1 sOnTopCards = new RoomSprite1x1(js, "ON_TOP_CARDS_1X1");
		final RoomSpriteCombo sTable_clean = new RoomSpriteCombo(js, "TABLES_COMBO") {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				if (blue.job.used(it.tx(), it.ty())) {
					for (int i = 0; i < DIR.ORTHO.size(); i++) {
						if (SETT.ROOMS().fData.sprite.get(it.tx(), it.ty(), DIR.ORTHO.get(i)) == sChair_table) {
							sOnTopCards.render(r, s, i, it, degrade, isCandle);
							
						}
					}
				}
				return false;
			}
		};
		
		final RoomSpriteBoxN sStage = new RoomSpriteBoxN(js, "STAGE_COMBO");
		
		final RoomSprite sTable_nick = new RoomSpriteCombo(sTable_clean) {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				if (!isCandle) {
					sOnTopDecor.renderRandom(r, ShadowBatch.DUMMY, it, it.ran(), degrade);
				}
				return false;
			}
		};
		
		final RoomSprite sShelf = new RoomSprite1x1(js, "SHELF_1X1") {
			
			final RoomSprite1x1 ontop = new RoomSprite1x1(js, "SHELF_ON_TOP_1X1");
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return d.orthoID() == item.rotation;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				ontop.render(r, s, data, it, degrade, false);
			}
			
		};
		
		final RoomSprite sNickNack = new RoomSprite1x1(js, "NICKNACK_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return d.orthoID() == item.rotation;
			}
		};
		
		final FurnisherItemTile __ = null;
		{
			final FurnisherItemTile cc = new FurnisherItemTile(this,true, sChair_wall, AVAILABILITY.AVOID_PASS, false);
			cc.setData(ICHAIR);
			final FurnisherItemTile ch = new FurnisherItemTile(this,true, sChair_table, AVAILABILITY.AVOID_PASS, false);
			final FurnisherItemTile ta = new FurnisherItemTile(this,false, sTable_clean, AVAILABILITY.ROOM_SOLID, false);
			ta.setData(ITABLE);
			final FurnisherItemTile tt = new FurnisherItemTile(this,false, sTable_nick, AVAILABILITY.ROOM_SOLID, true);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,cc,tt}, 
			}, 1);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,cc,cc,tt}, 
			}, 2);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,cc,cc,cc,tt}, 
			}, 3);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,cc,cc,cc,cc,tt}, 
			}, 4);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,cc,cc,cc,cc,cc,tt}, 
			}, 5);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,ch,__},
				{tt,ta,tt},
				{tt,ta,tt},
				{__,ch,__},
				
			}, 6);

			new FurnisherItem(new FurnisherItemTile[][] {
				{__,ch,ch,__},
				{tt,ta,ta,tt},
				{tt,ta,ta,tt},
				{__,ch,ch,__},
				
			}, 10);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,ch,ch,ch,__},
				{tt,ta,ta,ta,tt},
				{tt,ta,ta,ta,tt},
				{__,ch,ch,ch,__},
				
			}, 14);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,ch,ch,ch,ch,__},
				{tt,ta,ta,ta,ta,tt},
				{tt,ta,ta,ta,ta,tt},
				{__,ch,ch,ch,ch,__},
				
			}, 18);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{__,ch,ch,ch,ch,ch,__},
				{tt,ta,ta,ta,ta,ta,tt},
				{tt,ta,ta,ta,ta,ta,tt},
				{__,ch,ch,ch,ch,ch,__},
				
			}, 22);
			
			flush(1, 3);
			
		}
		
		{
			final FurnisherItemTile tt = new FurnisherItemTile(this,false, sStage, AVAILABILITY.AVOID_LIKE_FUCK, false);
			tt.setData(ISTAGE);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt},
				{tt,tt},
				
			}, 4);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,tt},
				{tt,tt,tt},
				
			}, 6);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,tt},
				{tt,tt,tt},
				{tt,tt,tt},
				
			}, 9);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{tt,tt,tt,tt},
				{tt,tt,tt,tt},
				{tt,tt,tt,tt},
				{tt,tt,tt,tt},
				
			}, 16);
			
			flush(1, 3);
		}
		
		{
			final FurnisherItemTile sh = new FurnisherItemTile(this,false, sShelf, AVAILABILITY.ROOM_SOLID, false);
			final FurnisherItemTile ni = new FurnisherItemTile(this,false, sNickNack, AVAILABILITY.ROOM_SOLID, false);
			final FurnisherItemTile ta = new FurnisherItemTile(this,false, sTableSingle, AVAILABILITY.ROOM_SOLID, false);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{sh,ta}, 
			}, 1);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{sh,ta,ni}, 
			}, 2);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{sh,sh,ta,ni,}, 
			}, 3);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{ni,sh,sh,ta,ni,}, 
			}, 4);
			
			new FurnisherItem(new FurnisherItemTile[][] {
				{ni,sh,sh,ta,ni,sh}, 
			}, 5);
			new FurnisherItem(new FurnisherItemTile[][] {
				{ni,sh,sh,ta,ni,sh,sh}, 
			}, 6);
			
			flush(3);
		}
		
		FurnisherItemTools.makeUnder(this, js, "CARPET_COMBO");
		
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
		return new ResthomeInstance(blue, area, init);
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
