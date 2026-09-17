package settlement.room.main.throne;

import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TERRAIN;

import init.sprite.SPRITES;
import settlement.main.SETT;
import snake2d.SPRITE_RENDERER;
import util.text.D;
import view.tool.PLACABLE;
import view.tool.PlacableFixedImp;
import view.tool.PlacableMessages;

class Placer extends PlacableFixedImp {
	
	private static CharSequence ¤¤name = "Move Throne";
	private static CharSequence ¤¤desc = "Creates a new construction site that will become the new throne room when finished. The throne is the center of your city and subjects that don't have a clear path to it will function poorly.";
	
	private int px = -1,py = -1;
	
	static {
		D.ts(Placer.class);
	}
	
	Placer(THRONE t){
		super(¤¤name, 4,1, ¤¤desc, t.icon());
	}
	
	@Override
	public void place(int tx, int ty, int rx, int ry) {
		if (rx == 0 && ry == 0 && px != tx && py != ty) {
			px = tx;
			py = ty;
			new InstanceConstruction(tx, ty, rot());
		}
	}
	
	@Override
	public CharSequence placable(int tx, int ty, int rx, int ry) {
		if (ROOMS().map.get(tx, ty) instanceof InstanceConstruction)
			return null;
		
		if (ROOMS().map.is(tx, ty))
			return PlacableMessages.¤¤ROOM_BLOCK;
		
		if (!TERRAIN().get(tx, ty).clearing().can() && !TERRAIN().get(tx, ty).roofIs())
			return PlacableMessages.¤¤TERRAIN_BLOCK;
		
		return null;

	}
	
	@Override
	public void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, int rx, int ry, boolean isPlacable, boolean areaIsPlacable) {
		SPRITES.cons().ICO.arrows.get(rot()).render(r, x, y);
	}
	
	@Override
	public int width() {
		return Sprite.width(rot());
	}
	
	@Override
	public int height() {
		return Sprite.height(rot());
	}
	
	@Override
	public PLACABLE getUndo() {
		return SETT.ROOMS().DELETE;
	}
};