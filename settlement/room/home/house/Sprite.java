package settlement.room.home.house;

import init.race.home.RaceHomeClass;
import init.sprite.game.Sheets;
import settlement.main.SETT;
import settlement.room.main.Room;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.sprite.RoomSprite;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.rnd.RND;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

abstract class Sprite extends RoomSprite.Imp{

	public final boolean service;
	public final boolean bed;
	public final boolean solid;
	HomeInstance house;
	
	public Sprite(boolean service, boolean bed, boolean solid) {
		this.service = service;
		this.bed = bed;
		this.solid = solid;
	}
	
	public Sprite() {
		this(false, false, true);
	}
	
	
	public DIR dir(int data) {
		return DIR.ORTHO.get(data&0b011);
	}

	static abstract class Rot extends Sprite {
		

		
		Rot(){
			super();
		}
		
		Rot(boolean service, boolean bed, boolean solid){
			super(service, bed, solid);
		}
		
		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			Sheets a = a(house.race().home().clas(house.occupant(0)));
			int ran = it.ran();
			Sprites.render1x1(ran, a, r, s, data, it, degrade);
			return false;
		}
		
		
		@Override
		public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			
			Room room = SETT.ROOMS().map.get(tx, ty);
			
			int r = RND.rInt(4);
			for (int i = 0; i < DIR.ORTHO.size(); i++) {
				int q = (i+r)%4;
				DIR d = DIR.ORTHO.getC(i+r);
				if (!room.isSame(tx, ty, tx+d.x(), ty+d.y())) {
					return (byte) (q);
				}
				
			}
			for (int i = 0; i < DIR.ORTHO.size(); i++) {
				DIR d = DIR.ORTHO.get(i);
				if (house.sprite(tx+d.x(), ty+d.y()) == SETT.ROOMS().HOME.constructor.sp.tabl) {
					return (byte) ((i+2)%4);
				}
			}
			
			return (byte) RND.rInt(4);
		}
		
		abstract Sheets a(RaceHomeClass sp);
		
	}
	
	protected RaceHomeClass sp() {
		return house.race().home().clas(house.occupant(0));
	}
	
}
