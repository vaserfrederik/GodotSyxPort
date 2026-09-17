package settlement.room.industry.refiner;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import init.constant.C;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.RoomInstance;
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
import snake2d.util.color.OPACITY;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.sprite.SPRITE;
import util.GUTIL;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{

	final FurnisherStat workers;
	final FurnisherStat efficiency;
	final FurnisherStat output;
	private final ROOM_REFINER blue;
	
	final static int B_STORAGE = 2;
//	final static int B_FETCH = 3;
	final static int B_WORK = 3;
	
	protected Constructor(RoomInitData init, ROOM_REFINER blue)
			throws IOException {
		super(init, 3, 3, 88, 44);
		this.blue = blue;
		

		workers = new FurnisherStat.FurnisherStatEmployees(this) {
			@Override
			public double get(AREA area, double acc) {
				int am = 0;
				for (COORDINATE c : area.body()) {
					if (area.is(c) && (SETT.ROOMS().fData.tileData.get(c) == B_WORK))
						am++;
				}
				return am;
			}
		};
		efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers, 1);
		output = new FurnisherStat.FurnisherStatProduction2(this, blue) {
			@Override
			protected double getBase(AREA area, double[] fromItems) {
				return workers.get(area, fromItems)*efficiency.get(area, fromItems);
			}
		};
		
		Json js = init.data().json("SPRITES");
		
		final RoomSprite sMachineTop = new RoomSpriteCombo(js, "MAIN_MACHINE_COMBO_TOP");
		
		final RoomSprite sMachine = new RoomSpriteCombo(js, "MAIN_MACHINE_COMBO") {
			final RoomSprite1x1 top2 = new RoomSprite1x1(js, "MAIN_MACHINE_COMBO_TOP_MACHINE");
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				sMachineTop.render(r, s, getData2(it), it, degrade, isCandle);
				return false;
			}

			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if ((it.ran() & 0b0011) == 0 && !SETT.ROOMS().fData.candle.is(it.tile())) {
					top2.animate(aniSpeed(it));
					top2.render(r, s, 0, it, degrade, rotates);
				}
				super.renderAbove(r, s, data, it, degrade);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sMachineTop.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		
		final RoomSprite sWork = new RoomSprite1x1(js, "WORK_1X1") {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				animationSpeed = 0;
				if (blue.is(it.tile())) {
					int d = ROOMS().data.get(it.tile());
					if (blue.job.FETCH.working(d)) {
						animationSpeed = 1;
					}
					
				}
				return super.render(r, s, data, it, degrade, isCandle);
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteCombo;
			}
			
		};
		
		final RoomSprite sFecth = new RoomSprite1x1(js, "STORAGE_IN_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteCombo;

			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				RoomInstance ins = SETT.ROOMS().map.instance.get(it.tile());
				if (ins != null && blue.job.FETCH.get(it.tx(), it.ty(), ins) != null)
					blue.job.FETCH.renderNeighs(ins, r, s, it.x(), it.y(), it.ran());
				

			}
		};
		
		RoomSprite sMisc = new RoomSprite1x1(js, "NICKNACK_BOTTOM_1X1") {
			
			private RoomSprite1x1 top = new RoomSprite1x1(js, "NICKNACK_TOP_1X1");
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteCombo;
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				if ((GUTIL.ran2().get(it.tile()) & 1) == 1)
					top.render(r, s, data, it, degrade, isCandle);
				return false;
			}
			
		};
		
		RoomSprite sMech = new RoomSprite1x1(js, "MACHINE_1X1") {
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) instanceof RoomSpriteCombo;
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				animationSpeed = aniSpeed(it);
				return super.render(r, s, data, it, degrade, isCandle);
			}
			
		};
		
		
		final RoomSprite sStorageTop = new RoomSprite1x1(js, "CONVEYOR_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				animationSpeed = aniSpeed(it);
				return super.render(r, s, data, it, degrade, isCandle);
			}
			
		};
		
		RoomSprite sStorage = new RoomSprite1xN(js, "STORAGE_1X1", false) {
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				if (blue.is(it.tile()))
					blue.job.storage.render(r, s, it.tx(), it.ty(), it.x(), it.y(), it.ran());
			}
			
			@Override
			protected boolean isMaster(int rx, int ry, FurnisherItem item) {
				return item.sprite(rx, ry) == sStorageTop;
			}
			
		};
		
		final RoomSprite sStorageEnd = new RoomSpriteCombo(sMachine) {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				super.render(r, s, data, it, degrade, isCandle);
				sMachineTop.render(r, s, getData2(it), it, degrade, isCandle);
				RefinerInstance ins = blue.get(it.tx(), it.ty());
				if (!isCandle && ins != null) {
					SPRITE i = ins.industry().outs().get(0).resource.icon();
					OPACITY.O99.bind();
					i.render(r, it.x()+8, it.x()+C.TILE_SIZE-8, it.y()+8, it.y()+C.TILE_SIZE-8);
					OPACITY.unbind();
				}
				return false;
			}

			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sMachineTop.getData(tx, ty, rx, ry, item, itemRan);
			}
		};
		

		
		
		final FurnisherItemTile me = new FurnisherItemTile(this, false,sMachine, AVAILABILITY.ROOM_SOLID, true);
		final FurnisherItemTile mm = new FurnisherItemTile(this, false,sMisc, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile ma = new FurnisherItemTile(this, false,sMech, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile ff = new FurnisherItemTile(this, true, sFecth, AVAILABILITY.ROOM_SOLID, false).setData(B_WORK);;
		final FurnisherItemTile ww = new FurnisherItemTile(this, true, sWork, AVAILABILITY.ROOM_SOLID, false).setData(B_WORK);
		
		final FurnisherItemTile st = new FurnisherItemTile(this, false,sStorageTop, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile sm = new FurnisherItemTile(this, true,sStorage, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile sb = new FurnisherItemTile(this, true,sStorage, AVAILABILITY.ROOM_SOLID, false);
		final FurnisherItemTile se = new FurnisherItemTile(this, false,sStorageEnd, AVAILABILITY.ROOM_SOLID, true);
		sm.setData(B_STORAGE);
		sb.setData(B_STORAGE);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,me},
			{ww,me},
			{ff,me},
			{mm,me},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,me},
			{ww,me},
			{ff,me},
			{ww,me},
			{mm,me},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,me},
			{ww,me},
			{ff,me},
			{ww,me},
			{ww,me},
			{ff,me},
			{mm,me},
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,me},
			{ww,me},
			{ff,me},
			{ww,me},
			{ww,me},
			{ff,me},
			{ww,me},
			{mm,me},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,me,me,mm},
			{ww,me,me,ww},
			{ff,me,me,ff},
			{mm,me,me,mm},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,me,me,mm},
			{ww,me,me,ww},
			{ff,me,me,ff},
			{ww,me,me,ww},
			{mm,me,me,mm},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,me,me,mm},
			{ww,me,me,ww},
			{ff,me,me,ff},
			{ww,me,me,ww},
			{ww,me,me,ww},
			{ff,me,me,ff},
			{mm,me,me,mm},
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,me,me,mm},
			{ww,me,me,ww},
			{ff,me,me,ff},
			{ww,me,me,ww},
			{ww,me,me,ww},
			{ff,me,me,ff},
			{ww,me,me,ww},
			{mm,me,me,mm},
		}, 12);
		
		flush(1, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,sm,sb,se},
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,sm,sm,sb,se},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,sm,sm,sm, sb,se},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,sm,sm,sm, sm, sb,se},
		}, 5);
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,sm,sm,sm,sm,sm,sb,se},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,sm,sm,sm,sm,sm,sm,sb,se},
		}, 7);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,sm,sb,se},
			{st,sm,sb,se},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,sm,sm,sb,se},
			{st,sm,sm,sb,se},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,sm,sm,sm, sb,se},
			{st,sm,sm,sm, sb,se},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,sm,sm,sm, sm,sb,se},
			{st,sm,sm,sm, sm,sb,se},
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,sm,sm,sm,sm,sm,sb,se},
			{st,sm,sm,sm,sm,sm,sb,se},
		}, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{st,sm,sm,sm,sm,sm,sm,sb,se},
			{st,sm,sm,sm,sm,sm,sm,sb,se},
		}, 14);
		
		flush(1, 1, 3);
		
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,me,ma},
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,me,me,ma},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{me,me,me,me},
			{ma,mm,ma,mm},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{me,me,me,me,me},
			{mm,ma,mm,ma,mm},
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{mm,ma,mm},
			{me,me,me},
			{me,me,me},
			{mm,ma,mm},
		}, 12);
		
		flush(3);
		
	}

	private double aniSpeed(RenderIterator it) {
		if (blue.is(it.tile())) {
			RefinerInstance ins = (RefinerInstance) blue.get(it.tile());
			return ins.WI / (double)ins.employees().max();
		}
		return 0;
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
		return new RefinerInstance(blue, area, init);
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
//		{0,1,0,1,1,0,1,0},
//		{0,0,1,0,0,1,0,0},
//		{0,1,0,1,1,0,1,0},
//		{0,1,0,1,1,0,1,0},
//		{0,0,1,0,0,1,0,0},
//		{0,1,0,1,1,0,1,0},
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
