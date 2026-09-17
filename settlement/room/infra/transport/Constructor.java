package settlement.room.infra.transport;

import java.io.IOException;

import game.GAME;
import init.constant.C;
import init.resources.RESOURCE;
import init.sprite.SPRITES;
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
import settlement.tilemap.floor.Floors.Floor;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Constructor extends Furnisher{
	
	private final ROOM_TRANSPORT blue;
	final FurnisherStat crates;
	private final Floor floor2;
	
	final FurnisherItemTile an;
	final FurnisherItemTile ww;
	
	protected Constructor(ROOM_TRANSPORT blue, RoomInitData init)
			throws IOException {
		super(init, 1, 1);
		

		this.blue = blue;
		floor2 = SETT.FLOOR().map.read("FLOOR2", init.data());
		
		crates = new FurnisherStat(this, 1) {
			
			@Override
			public double get(AREA area, double fromItems) {
				return fromItems;
			}
			
			@Override
			public GText format(GText t, double value) {
				return GFORMAT.i(t, (int)value);
			}
		};
		
		Json sp = init.data().json("SPRITES");
		
		RoomSprite marker = new RoomSprite1x1(sp, "GROUND_THING_1X1") {
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			};
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) == this;
			}
			
		};
		
		RoomSprite sLoad = new RoomSprite.Imp() {

			@Override
			public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return 0;
			}
			
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				TransportInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null && ins.resource() != null) {
					int am = blue.job.bamount.get(it.tx(), it.ty());
					if (am > 0 )
						ins.data.resource().renderLaying(r, it.x(), it.y(), it.ran(), am);
				}
			}


			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				// TODO Auto-generated method stub
				return false;
			}
		};
		
		RoomSprite scart = new RoomSprite.Imp() {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				
				RESOURCE res = null;
				int am = 0;
				
				TransportInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null) {
					res = ins.data.resource();
					am = ins.data.stored();
					if (am > 0 || ins.data.cartVisible()) {
						int iii = SETT.ROOMS().fData.tileData.get(it.tile())-1;
						int dx = 0;
						int dy = 0;
						if ((iii & 1) == 1) {
							DIR dd = DIR.ORTHO.get(data);
							dx = dd.x()*C.TILE_SIZEH;
							dy = dd.y()*C.TILE_SIZEH;
						}
						SETT.HALFENTS().transports.sprite.renderBelow(r, s, data*2, it.x()+dx+C.TILE_SIZEH, it.y()+dy+C.TILE_SIZEH, 0, it.ran(), degrade, res, (double)am/ROOM_TRANSPORT.MAX_LOAD);
					}
				}
				
				return false;
			}
			
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				TransportInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null && ins.data.cartVisible()) {
					int iii = SETT.ROOMS().fData.tileData.get(it.tile())-1;
					int dx = 0;
					int dy = 0;
					if ((iii & 1) == 1) {
						DIR dd = DIR.ORTHO.get(data);
						dx = dd.x()*C.TILE_SIZEH;
						dy = dd.y()*C.TILE_SIZEH;
					}
					SETT.HALFENTS().transports.sprite.render(r, s, data*2, it.x()+dx+C.TILE_SIZEH, it.y()+dy+C.TILE_SIZEH, degrade, false);
					
				}
			}
			
			@Override
			public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return (byte) item.rotation;
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().ICO.arrows.get(item.rotation).render(r, x, y);
			}
		};
		
		RoomSprite dummy = new RoomSprite() {
			
			@Override
			public int sData() {
				// TODO Auto-generated method stub
				return 0;
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				// TODO Auto-generated method stub
				return false;
			}
			
			@Override
			public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				// TODO Auto-generated method stub
				return 0;
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().ICO.arrows.get(item.rotation).render(r, x, y);
			}
		};
		
		RoomSprite animal = new RoomSprite.Imp() {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				TransportInstance ins = blue.get(it.tx(), it.ty());
				if (ins != null && ins.data.oxVisible()) {
					double mov = (GAME.intervals().get05()+it.ran()) & 0x0FF;
					mov /= 0x0FF;
					SETT.ANIMALS().renderCaravan(r, s, mov, it.x()+C.TILE_SIZEH, it.y()+C.TILE_SIZEH, null, 0, false, data*2, it.ran());
				}
				
				return false;
			}
			
			@Override
			public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return (byte) item.rotation;
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				SPRITES.cons().ICO.arrows.get(item.rotation).render(r, x, y);
			}
		};
		
		RoomSprite post = new RoomSprite1x1(sp, "TORCH_1X1");
		
		FurnisherItemTile pp = new FurnisherItemTile(
				this,
				false,
				post, 
				AVAILABILITY.ROOM_SOLID, 
				true);
		
		FurnisherItemTile mm = new FurnisherItemTile(
				this,
				false,
				marker, 
				AVAILABILITY.ROOM, 
				false);

		ww = new FurnisherItemTile(
				this,
				false,
				sLoad, 
				AVAILABILITY.ROOM_SOLID, 
				false);
		
		FurnisherItemTile xx = new FurnisherItemTile(
				this,
				false,
				dummy, 
				AVAILABILITY.ROOM_SOLID, 
				false).setData(1);
		
		FurnisherItemTile __ = new FurnisherItemTile(
				this,
				false,
				new RoomSprite.Dummy(), 
				AVAILABILITY.ROOM, 
				false);
		
		an = new FurnisherItemTile(
				this,
				false,
				animal, 
				AVAILABILITY.ROOM_SOLID, 
				false).setData(1);
		
		FurnisherItemTile c0 = new FurnisherItemTile(
				this,
				false,
				scart, 
				AVAILABILITY.ROOM_SOLID, 
				false).setData(1);
		
		FurnisherItemTile c1 = new FurnisherItemTile(
				this,
				false,
				scart, 
				AVAILABILITY.ROOM_SOLID, 
				false).setData(2);
		
		FurnisherItemTile c2 = new FurnisherItemTile(
				this,
				false,
				scart, 
				AVAILABILITY.ROOM_SOLID, 
				false).setData(3);
		
		FurnisherItemTile c3 = new FurnisherItemTile(
				this,
				false,
				scart, 
				AVAILABILITY.ROOM_SOLID, 
				false).setData(4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{pp,__,__,__,pp},
			{__,__,__,__,__},
			{mm,ww,an,ww,mm},
			{mm,ww,c0,ww,mm},
			{__,ww,xx,ww,__},
			{mm,ww,c1,ww,mm},
			{mm,ww,c2,ww,mm},
			{__,ww,xx,ww,__},
			{mm,ww,c3,ww,mm},
			{mm,ww,ww,ww,mm},
			{__,__,__,__,__},
			{pp,__,__,__,pp},
		}, 1);
				
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
		if (SETT.ROOMS().fData.tile.get(tx, ty) != null && SETT.ROOMS().fData.tile.get(tx, ty).data() > 0)
			floor2.placeFixed(tx, ty);
		else
			super.putFloor(tx, ty, upgrade, area);
	}

	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new TransportInstance(blue, area, init);
		
	}
}
