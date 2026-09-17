package settlement.room.home.house;

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
import settlement.room.main.util.RoomState;
import settlement.tilemap.floor.Floors.Floor;
import snake2d.LOG;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;

final class HomeContructor extends Furnisher{

	private final ROOM_HOME blue;
	
	final FurnisherStat occupants = new FurnisherStat.FurnisherStatI(this, 1);
	
	public final int[][] maxOccupants = new int[][] {
		{3,4,5},
		{5,7,9},
		{10,14,18},
	};
	
	static final int entrance = 2;
	
	public final FurnisherItemTile tOpening;
	public final Sprites sp;
	public final Floor flooring;
	protected HomeContructor(RoomInitData init, ROOM_HOME blue)
			throws IOException {
		super(init, 3, 1);
		
		flooring = floors.get(0);
		sp = new Sprites(init.data());
		
		
		
		
		this.blue = blue;
		final FurnisherItemTile ee = new FurnisherItemTile(this, true, sp.theDummy, AVAILABILITY.ROOM, false);
		ee.setData(entrance);
		ee.noWalls = true;
		tOpening = ee;
		final FurnisherItemTile __ = new FurnisherItemTile(this, false, sp.theDummy, AVAILABILITY.ROOM, false);
		__.setData(1);
		final FurnisherItemTile xx = new FurnisherItemTile(this, false, sp.theDummy, AVAILABILITY.NOT_ACCESSIBLE, false);
		
		LOG.ln();
		
		create(new FurnisherItemTile[][] {
			 {xx,xx,xx},
			 {xx,__,xx},
			 {xx,ee,xx},
		}, 9);
		
		create(new FurnisherItemTile[][] {
			{xx,xx,xx},
			{xx,__,xx},
			{xx,__,xx},
			{xx,__,xx},
			{xx,ee,xx},
		}, 15);
		create(new FurnisherItemTile[][] {
			 {xx,xx,xx,xx,xx},
			 {xx,__,__,__,xx},
			 {xx,__,__,__,xx},
			 {xx,__,__,__,xx},
			 {xx,__,__,__,xx},
			 {xx,xx,ee,xx,xx},
		}, 30);
		
	}
	
	private void create(FurnisherItemTile[][] tt, int am) {
		am++;
		new FurnisherItem(tt, 1);
		for (int i = 2; i < am; i++) {
			FurnisherItemTile[][] tn = new FurnisherItemTile[tt.length][tt[0].length*i];
			for (int y = 0; y < tt.length; y++) {
				for (int x = 0; x < tn[0].length; x++) {
					tn[y][x] = tt[y][x%tt[0].length];
				}
			}
			new FurnisherItem(tn, i);
		}
		flush(3);
	}

	@Override
	public boolean mustBeIndoors() {
		return true;
	}

	@Override
	public boolean mustBeOutdoors() {
		return false;
	}

	@Override
	public boolean usesArea() {
		return false;
	}
	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		
		HomeInstance i = new HomeInstance(blue, area);
		
		for (COORDINATE c : i.body()) {
			for (int di = 0; di < DIR.ALL.size(); di++) {
				int x = c.x() + DIR.ALL.get(di).x();
				int y = c.y() + DIR.ALL.get(di).y();
				SETT.TERRAIN().get(x, y).placeFixed(x, y);
			}
			
		}
		return i;
	}
	
	@Override
	public FurnisherItem secretReplacementItem(int rot, FurnisherItem it) {
		return it.group.item(0, rot);
	}
	
	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	
	@Override
	public boolean needsIsolation() {
		return true;
	}
	
	@Override
	public RoomState getConstructionState() {
		return new HomeInstance.State(null);
	}
	

}
