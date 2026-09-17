package settlement.room.military.training.archery;

import java.io.IOException;

import init.constant.C;
import settlement.main.SETT;
import settlement.room.main.Room;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.military.training.ROOM_M_TRAINER;
import settlement.stats.STATS;
import settlement.thing.projectiles.Trajectory;
import snake2d.util.datatypes.DIR;

public final class ROOM_ARCHERY extends ROOM_M_TRAINER<ArcheryInstance> {
	
	final Constructor constructor;
	final ArcheryThing thing = new ArcheryThing(this);
	private final Trajectory[] trajs = new Trajectory[4];
	
	
	public ROOM_ARCHERY(int typeIndex, RoomInitData data, String key) throws IOException {
		super(typeIndex, data, key);
		
		constructor = new Constructor(this, data) {

			@Override
			public Room create(TmpArea area, RoomInit init) {
				return new ArcheryInstance(ROOM_ARCHERY.this, area, init);
			}
			
			@Override
			public boolean isHeavy() {
				return true;
			}
			
		};
	
		
		
		{
			double dist = constructor.item(1).height()-1;
			dist *= C.TILE_SIZE;
			int i = 0;
			for (DIR d : DIR.ORTHO) {
				Trajectory t = new Trajectory();
				t.calcLow(0, 0, 0, (int)(d.x()*dist), (int)(d.y()*dist), 45, 40*C.TILE_SIZE);
				trajs[i++] = t;
			}
		}
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}

	public DIR faceCoo(int tx, int ty) {
		FurnisherItem t = SETT.ROOMS().fData.item.get(tx, ty);
		if (t != null)
			return DIR.ORTHO.get(SETT.ROOMS().fData.item.get(tx, ty).rotation);
		return DIR.C;
	}
	
	public void fireArrow(int tx, int ty, int x, int y) {
		if (is(tx, ty)) {
			FurnisherItem it = SETT.ROOMS().fData.item.get(tx, ty);
			if (it != null) {
				Trajectory t = trajs[it.rotation];
				SETT.PROJS().launchDummy(x, y, 0, t, STATS.EQUIP().RANGED().get(0).projectile, 0, null);
			}
		}
	}

}
