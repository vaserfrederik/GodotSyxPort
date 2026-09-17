package settlement.room.infra.station;

import java.io.IOException;

import game.GAME;
import init.constant.C;
import init.resources.RESOURCE;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.infra.transport.ROOM_TRANSPORT;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import settlement.tilemap.floor.Floors.Floor;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.GUTIL;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{
	
	private final ROOM_STATION blue;
	private final Floor floor2;
	
	public static final double RAND = 		1.0/0x0FF;
	public static final int BIT_WORK = 		0b00001;
	public static final int BIT_CRATE = 	0b00010;
	private static final int BIT_FLOOR = 	0b00100;
	public static final int BIT_DEST = 		0b01000;
	
	protected Constructor(ROOM_STATION blue, RoomInitData init)
			throws IOException {
		super(init, 1, 0);
		

		this.blue = blue;
		floor2 = SETT.FLOOR().map.read("FLOOR2", init.data());

		
		Json sp = init.data().json("SPRITES");
		
		final RoomSprite sWork = new RoomSprite1x1(sp, "WORK_1X1") {
			
			final RoomSprite top = new RoomSprite1x1(sp, "WORK_TOP_1X1");		
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				StationInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null) {
					if (ins.prepD() + (GUTIL.ran2().get(it.tile())&0x0FF)*RAND >= 1)
						return;
				}
				top.render(r, s, getData2(it), it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return top.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};
		
		final RoomSprite sBasin = new RoomSprite1x1(sp, "ANIMAL_TOP_1X1") {
			
			final RoomSprite water = new RoomSprite1x1(sp, "ANIMAL_BOTTOM_1X1");
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				StationInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null) {
					if (ins.prepD() + (GUTIL.ran2().get(it.tile())&0x0FF)*RAND >= 1)
						water.render(r, s, data, it, degrade, false);
				}
				
			}			
			
		};
		
		final RoomSprite sAnimal = new RoomSprite1x1(sp, "ROOF_MID_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == this;
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				StationInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null) {
					if (ins.prepD() + (GUTIL.ran2().get(it.tile())&0x0FF)*RAND >= 1) {
						DIR d = DIR.ORTHO.get(SETT.ROOMS().fData.item.get(it.tile()).rotation);
						
						double mov = (GAME.intervals().get05()+it.ran()) & 0x0FF;
						mov /= 0x0FF;
						SETT.ANIMALS().renderCaravan(r, s, mov, it.x()+C.TILE_SIZEH+d.x()*C.TILE_SIZEH, it.y()+C.TILE_SIZEH+d.y()*C.TILE_SIZEH, null, 0, false, d.id(), it.ran());
					}
					
				}
				return false;
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				s.setSoft();
				super.render(r, s, data, it, degrade, false);
				s.setPrev();
			}
			
		};
		
		final RoomSprite sAnimalEdge = new RoomSprite1x1(sp, "ROOF_EDGE_1X1") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == sAnimal;
			}
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				s.setSoft();
				super.render(r, s, data, it, degrade, false);
				s.setPrev();
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
		};
		
		
		final RoomSprite spriteCrate = new RoomSprite1x1(sp, "CRATE_BOTTOM_1X1") {
			
			final RoomSprite top = new RoomSprite1x1(sp, "CRATE_TOP_1X1");
			
			@Override
			public boolean render(snake2d.SPRITE_RENDERER r, ShadowBatch s, int data, util.rendering.RenderData.RenderIterator it, double degrade, boolean isCandle) {
				top.render(r, s, data, it, degrade, rotates);
				return false;
				
			};
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
				
				Crate c = blue.crate;
				
				if (c.get(it.tx(), it.ty()) != null && c.resource() != null) {
					double am = c.stored.get();
					RESOURCE res = c.resource();
					if (am > 0 && res != null) {
						res.renderLayingRel(r, it.x(), it.y(), it.ran(), am/ROOM_TRANSPORT.MAX_LOAD);
					}
				}
			}
		};
		
		RoomSprite post = new RoomSprite1x1(sp, "TORCH_1X1");
		
		FurnisherItemTile pp = new FurnisherItemTile(
				this,
				false,
				post, 
				AVAILABILITY.ROOM_SOLID, 
				true);
		pp.setData(BIT_DEST);
		
		FurnisherItemTile dd = new FurnisherItemTile(
				this,
				false,
				post, 
				AVAILABILITY.ROOM_SOLID, 
				true);
		dd.setData(BIT_DEST);
		
		FurnisherItemTile st = new FurnisherItemTile(
				this,
				false,
				spriteCrate, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		st.setData(BIT_WORK | BIT_CRATE);
		
		FurnisherItemTile ww = new FurnisherItemTile(
				this,
				false,
				sWork, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		ww.setData(BIT_WORK);
		
		FurnisherItemTile __ = new FurnisherItemTile(
				this,
				false,
				new RoomSprite.Dummy(), 
				AVAILABILITY.ROOM, 
				false);
		
		FurnisherItemTile an = new FurnisherItemTile(
				this,
				false,
				sAnimal, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		an.setData(BIT_FLOOR);
		
		FurnisherItemTile ae = new FurnisherItemTile(
				this,
				false,
				sAnimalEdge, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		ae.setData(BIT_FLOOR);
		
		FurnisherItemTile af = new FurnisherItemTile(
				this,
				false,
				sBasin, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		af.setData(BIT_FLOOR | BIT_WORK);
		
		FurnisherItemTile in = new FurnisherItemTile(
				this,
				false,
				new RoomSprite.Dummy(), 
				AVAILABILITY.ROOM, 
				false).setData(1);
		in.setData(BIT_FLOOR);
		
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__,__,__},
			{__,in,af,af,af,in,__},
			{__,ae,an,an,an,ae,__},
			{__,st,__,ww,pp,ww,__,},
			{__,st,__,ww,in,ww,__,},
			{__,st,__,ww,in,ww,__,},
			{__,st,__,ww,in,ww,__,},
			{__,st,__,ww,in,ww,__,},
			{__,st,__,ww,in,ww,__,},
			{__,st,__,ww,in,ww,__,},
			{__,st,__,ww,in,ww,__,},
			{__,__,__,__,__,__,__,},
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__,__,__,__},
			{__,in,af,af,af,af,in,__},
			{__,ae,an,an,an,an,ae,__},
			{__,st,st,__,ww,pp,ww,__,},
			{__,st,st,__,ww,in,ww,__,},
			{__,st,st,__,ww,in,ww,__,},
			{__,st,st,__,ww,in,ww,__,},
			{__,st,st,__,ww,in,ww,__,},
			{__,st,st,__,ww,in,ww,__,},
			{__,st,st,__,ww,in,ww,__,},
			{__,st,st,__,ww,in,ww,__,},
			{__,__,__,__,__,__,__,__,},
		}, 1.15);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{__,__,__,__,__,__,__,__,__,__,__},
			{__,in,af,af,af,af,af,af,af,in,__},
			{__,ae,an,an,an,an,an,an,an,ae,__},
			{__,st,st,__,ww,pp,ww,__,st,st,__},
			{__,st,st,__,ww,in,ww,__,st,st,__},
			{__,st,st,__,ww,in,ww,__,st,st,__},
			{__,st,st,__,ww,in,ww,__,st,st,__},
			{__,st,st,__,ww,in,ww,__,st,st,__},
			{__,st,st,__,ww,in,ww,__,st,st,__},
			{__,st,st,__,ww,in,ww,__,st,st,__},
			{__,st,st,__,ww,in,ww,__,st,st,__},
			{__,__,__,__,__,__,__,__,__,__,__},
		}, 1.6);
				
		flush(3);
		
	}

	@Override
	public boolean usesArea() {
		return false;
	}

	@Override
	public boolean mustBeIndoors() {
		return false;
	}

	@Override
	public RoomBlueprintImp blue() {
		// TODO Auto-generated method stub
		return blue;
	}
	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		if (SETT.ROOMS().fData.tile.get(tx, ty) != null && (SETT.ROOMS().fData.tile.get(tx, ty).data() & BIT_FLOOR) != 0)
			floor2.placeFixed(tx, ty);
		else
			super.putFloor(tx, ty, upgrade, area);
	}

	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new StationInstance(blue, area, init);
		
	}
	
	
}
