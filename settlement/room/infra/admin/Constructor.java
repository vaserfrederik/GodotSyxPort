package settlement.room.infra.admin;

import java.io.IOException;

import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
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
import settlement.room.sprite.RoomSpriteImp;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.GUTIL;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	public final FurnisherStat workers = new FurnisherStat.FurnisherStatEmployees(this);
	public final FurnisherStat stations = new FurnisherStat(this) {
		
		@Override
		public double get(AREA area, double fromItems) {
			return fromItems;
		}
		
		@Override
		public GText format(GText t, double value) {
			return GFORMAT.i(t, (int)(value*blue.data.knowledgePerStation));
		}
	};
	public final FurnisherStat efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers);
	
	private final ROOM_ADMIN blue;
	final FurnisherItemTile ww;
	static final int ICHAIR = 3;
	
	protected Constructor(ROOM_ADMIN blue, RoomInitData init)
			throws IOException {
		super(init, 3, 3, 88, 44);
		this.blue = blue;
		
		final Json sp = init.data().json("SPRITES");
		
		final RoomSprite sMisc = new RoomSprite1x1(sp, "MISC_1X1");
		
		final RoomSprite sTable = new RoomSpriteCombo(sp, "TABLE_COMBO") {

			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (SETT.ROOMS().fData.candle.is(it.tile()))
					return;
				sMisc.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sMisc.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		final RoomSprite sTMid = new RoomSpriteCombo(sTable) {

			final RoomSprite1x1 idle = new RoomSprite1x1(sp, "ADMIN_ONTOP_1X1") {
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return d.orthoID() == item.rotation;
				}
				
			};
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				data = getData2(it);
				Room ro = SETT.ROOMS().map.get(it.tile());
				if (ro instanceof AdminInstance && !SETT.ROOMS().fData.candle.is(it.tile())) {
					long ran = ((0l | GUTIL.ran2().get(it.tx(), it.ty())) << 32) | GUTIL.ran2().get(it.tx()+1, it.ty());
					int am = (blue.data.usedD>>4) & 0x0F;
					am -= (it.ran()&0b11);
					DIR d = idle.rot(getData2(it));
					for (int i = 0; i < am; i++) {
						int dd = (int) ((ran&7)*7);
						DIR dd2 = d.next(2);
						int d2 = (int) ((ran >> 3)&1);
						d2 *= 4;
						ran = ran >> 4;
						
						it.setOff(-d.x()*dd+dd2.x()*d2, -d.y()*dd+dd2.y()*d2);
						idle.render(r, s, data, it, degrade, false);
					}
				}
				
				
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return idle.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		final RoomSprite sPaper = new RoomSpriteCombo(sTable) {
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				Room ro = SETT.ROOMS().map.get(it.tile());
				if (ro instanceof AdminInstance && !SETT.ROOMS().fData.candle.is(it.tile())) {
					AdminInstance ins = (AdminInstance)ro;
					int am = (int) (16.0*blue.consumption().stored(blue.consumption().ins().get(0)).get(ins)/(ins.jobs.size()));
					
					
					if (am > 0) {
						blue.consumption().ins().get(0).resource.renderLaying(r, it.x(), it.y(), GUTIL.ran2().get(it.tile()), am);
					}
				}
			}
			
		};
		final RoomSprite sWork = new RoomSpriteCombo(sTable) {

			final RoomSprite idle = new RoomSprite1x1(sp, "WORK_UNUSED_1X1") {
				@Override
				protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
					return item.get(rx, ry) != null && item.get(rx, ry).data() == ICHAIR;
				}
			};
			final RoomSprite active = new RoomSprite1x1(sp, "WORK_USED_1X1");
			final RoomSpriteImp ontop = new RoomSprite1x1(sp, "WORK_USED_TOP_1X1");
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {

				if(!blue.is(it.tile()))
					return;
				SETT_JOB j = blue.job.get(it.tx(), it.ty());
				
				if(!j.jobReservedIs(null)) {
					;//idle.render(r, s, getData2(it), it, degrade, false);
				}else {
					ontop.animate(blue.job.used(it.tx(), it.ty()) ? 1 : 0);
					active.render(r, s, getData2(it), it, degrade, false);
					ontop.render(r, s, getData2(it), it, degrade, false);
				}
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return idle.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		
		final RoomSprite sShelfSingle = new RoomSprite1x1(sp, "SHELF_1X1") {
			
			final RoomSpriteImp ontop = new RoomSprite1x1(sp, "SHELF_TOP_1X1");

			@Override
			public void renderAbove(SPRITE_RENDERER re, ShadowBatch s, int data, RenderIterator it, double degrade) {
				Room r = SETT.ROOMS().map.get(it.tile());
				if (r instanceof AdminInstance) {
					int f = (blue.data.usedD&0x0FF);
					if (f >= (it.ran() & 0x0FF)) {
						ontop.render(re, s, data, it, degrade, false);
					}
				}
				
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				if (item.width() == 1 || item.height() == 1)
					return d.orthoID() == item.rotation;
				if (d.orthoID() == item.rotation || d.perpendicular().orthoID() == item.rotation)
					return item.sprite(rx, ry) == this;
				return false;
			}

		};

		
		final RoomSprite sStool = new RoomSprite1x1(sp, "STOOL_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteCombo;
			}
		};
		
		final FurnisherItemTile ss = new FurnisherItemTile(this,true, sShelfSingle, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile mm = new FurnisherItemTile(this,false, sTMid, AVAILABILITY.ROOM_SOLID, false);
		ww = new FurnisherItemTile(this,false, sWork, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile tt = new FurnisherItemTile(this,false, sPaper, AVAILABILITY.ROOM_SOLID, true);
		final FurnisherItemTile in = new FurnisherItemTile(this,true, sStool, AVAILABILITY.AVOID_PASS, true);
		in.setData(ICHAIR);
		final FurnisherItemTile __ = null;
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ mm, mm, mm, },
			{ tt, ww, tt, },
			{ __, in, __, }, 
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ mm, mm, mm, mm, },
			{ tt, ww, ww, tt, },
			{ __, in, in, __, }, 
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ mm, mm, mm, mm, mm, },
			{ tt, ww, ww, ww, tt, },
			{ __, in, in, in, __, }, 
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ mm, mm, mm, mm, mm, mm, },
			{ tt, ww, ww, ww, ww, tt, },
			{ __, in, in, in, in, __, }, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ mm, mm, mm, mm, mm, mm, mm, },
			{ tt, ww, ww, ww, ww, ww, tt, },
			{ __, in, in, in, in, in, __, }, 
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ mm, mm, mm, mm, mm, mm, mm, mm, },
			{ tt, ww, ww, ww, ww, ww, ww, tt, },
			{ __, in, in, in, in, in, in, __, }, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ mm, mm, mm, mm, mm, mm, mm, mm, mm, },
			{ tt, ww, ww, ww, ww, ww, ww, ww, tt, },
			{ __, in, in, in, in, in, in, in, __, }, 
		}, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ __, in, __, }, 
			{ tt, ww, tt, },
			{ mm, mm, mm, },
			{ tt, ww, tt, },
			{ __, in, __, }, 
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ __, in, in, __, }, 
			{ tt, ww, ww, tt, },
			{ mm, mm, mm, mm, },
			{ tt, ww, ww, tt, },
			{ __, in, in, __, }, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ __, in, in, in, __, }, 
			{ tt, ww, ww, ww, tt, },
			{ mm, mm, mm, mm, mm, },
			{ tt, ww, ww, ww, tt, },
			{ __, in, in, in, __, }, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ __, in, in, in, in, __, },
			{ tt, ww, ww, ww, ww, tt, },
			{ mm, mm, mm, mm, mm, mm, },
			{ tt, ww, ww, ww, ww, tt, },
			{ __, in, in, in, in, __, }, 
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ __, in, in, in, in, in, __, }, 
			{ tt, ww, ww, ww, ww, ww, tt, },
			{ mm, mm, mm, mm, mm, mm, mm, },
			{ tt, ww, ww, ww, ww, ww, tt, },
			{ __, in, in, in, in, in, __, }, 
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ __, in, in, in, in, in, in, __, }, 
			{ tt, ww, ww, ww, ww, ww, ww, tt, },
			{ mm, mm, mm, mm, mm, mm, mm, mm, },
			{ tt, ww, ww, ww, ww, ww, ww, tt, },
			{ __, in, in, in, in, in, in, __, }, 
		}, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ __, in, in, in, in, in, in, in, __, }, 
			{ tt, ww, ww, ww, ww, ww, ww, ww, tt, },
			{ mm, mm, mm, mm, mm, mm, mm, mm, mm, },
			{ tt, ww, ww, ww, ww, ww, ww, ww, tt, },
			{ __, in, in, in, in, in, in, in, __, }, 
		}, 14);
		
		flush(1, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ tt, ss }, 
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ tt, ss, ss, }, 
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ tt, ss, ss, ss, }, 
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ tt, ss, ss, ss, ss, }, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ tt, ss, ss, ss, ss, ss, }, 
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ tt, ss }, 
			{ tt, ss }, 
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ tt, ss, ss, }, 
			{ tt, ss, ss, }, 
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ tt, ss, ss, ss, }, 
			{ tt, ss, ss, ss, }, 
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ tt, ss, ss, ss, ss, }, 
			{ tt, ss, ss, ss, ss, }, 
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ tt, ss, ss, ss, ss, ss, }, 
			{ tt, ss, ss, ss, ss, ss, }, 
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
		return new AdminInstance(blue, area, init);
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
