package settlement.room.main.furnisher;

import java.io.IOException;

import init.sprite.SPRITES;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSpriteCombo;
import settlement.tilemap.floor.Floors.Floor;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public final class FurnisherItemTools {

	private FurnisherItemTools() {
		// TODO Auto-generated constructor stub
	}
		
	public static FurnisherItemTile makeFloorTile(Furnisher f, Floor floor) {
		
		;
		
		RoomSprite ca = new RoomSprite.Imp() {
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				SETT.FLOOR().renderOntop(it, floor,  getRes(it.tx(), it.ty())); 
				super.renderBelow(r, s, data, it, degrade);
			}
			
			@Override
			public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
					FurnisherItem item) {
				int res = getRes(tx, ty);
				SPRITES.cons().BIG.dashedThick.render(r, res, x, y);
			}
			
			private int getRes(int tx, int ty) {
				int res = 0;
				
				if (SETT.ROOMS().placement.factory.is(tx, ty)) {
					for (int di = 0; di < 4; di++) {
						DIR d = DIR.ORTHO.get(di);
						int dx = tx + d.x();
						int dy = ty + d.y();
						if (SETT.ROOMS().placement.factory.is(dx, dy) && is(dx, dy))
							res |= d.mask();
						
					}
				}else {
					Room r = SETT.ROOMS().map.get(tx, ty);
					if (r == null)
						return 0;
					for (int di = 0; di < 4; di++) {
						DIR d = DIR.ORTHO.get(di);
						int dx = tx + d.x();
						int dy = ty + d.y();
						if (r.isSame(tx, ty, dx, dy) && is(dx, dy))
							res |= d.mask();
						
					}
				}
				return res;
			}
			
			private boolean is(int dx, int dy) {
				FurnisherItemTile t = f.tiles.get(SETT.ROOMS().fData.tIndex.get(dx, dy));
				return (t != null && t.sprite() == this);
			}
			
			
			@Override
			public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				// TODO Auto-generated method stub
				return 0;
			}
			
		};
		
		return new FurnisherItemTile(f, false, ca, AVAILABILITY.ROOM, false);
	}
	
	
	public static void makeFloor(Furnisher f, Floor floor) {
		
		FurnisherItemTile cc = makeFloorTile(f, floor);

		makeArea(f, cc);
		
	}
	
	public static FurnisherItemTile makeUnder(Furnisher f, Json j, String key) throws IOException {
		
		RoomSpriteCombo ca = new RoomSpriteCombo(j, key) {
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
		FurnisherItemTile cc = new FurnisherItemTile(f, false, ca, AVAILABILITY.ROOM, false);
		
		makeArea(f, cc);
		return cc;
	}
	
	public static FurnisherItemTile makeUnder(Furnisher f, Json j, String key, RoomSprite above) throws IOException {
		
		RoomSpriteCombo ca = new RoomSpriteCombo(j, key) {
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return above.render(r, s, data, it, degrade, isCandle);
			}
			
			@Override
			public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				above.renderAbove(r, s, data, it, degrade);
			}
			
		};
		FurnisherItemTile cc = new FurnisherItemTile(f, false, ca, AVAILABILITY.ROOM, false);
		
		makeArea(f, cc);
		return cc;
	}
	
	public static void makeArea(Furnisher f, FurnisherItemTile cc) {
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc}
		}, 1);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc}
		}, 2);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc}
		}, 3);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc,cc}
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc,cc,cc}
		}, 5);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc,cc,cc,cc}
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc},
			{cc,cc},
		}, 4);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc},
			{cc,cc,cc},
		}, 6);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc,cc},
			{cc,cc,cc,cc},
		}, 8);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc,cc,cc},
			{cc,cc,cc,cc,cc},
		}, 10);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc,cc,cc,cc,},
			{cc,cc,cc,cc,cc,cc,},
		}, 12);

		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc},
			{cc,cc,cc},
			{cc,cc,cc},
		}, 9);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc,cc},
			{cc,cc,cc,cc},
			{cc,cc,cc,cc},
		}, 12);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc,cc,cc},
			{cc,cc,cc,cc,cc},
			{cc,cc,cc,cc,cc},
		}, 15);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc,cc,cc,cc,},
			{cc,cc,cc,cc,cc,cc,},
			{cc,cc,cc,cc,cc,cc,},
		}, 18);
		
		new FurnisherItem(new FurnisherItemTile[][] {
			{cc,cc,cc,cc,cc,cc,cc,},
			{cc,cc,cc,cc,cc,cc,cc,},
			{cc,cc,cc,cc,cc,cc,cc,},
		}, 21);
		
		f.flush(1);
	}
	

	

	
}
