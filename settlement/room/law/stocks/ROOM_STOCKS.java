package settlement.room.law.stocks;

import java.io.IOException;

import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.law.PUNISHMENT_SERVICE;
import settlement.room.law.stocks.Tile.STATE;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.ROOM_SPECTATOR;
import settlement.room.service.module.RoomServiceAccess;
import settlement.room.service.module.RoomServiceNeed;
import snake2d.LOG;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.rnd.RND;
import snake2d.util.sets.LISTE;
import util.GUTIL;
import util.gui.misc.GBox;
import util.info.GFORMAT;
import util.text.Dic;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_STOCKS extends RoomBlueprintIns<Instance> implements ROOM_SPECTATOR.ROOM_SPECTATOR_HASER, PUNISHMENT_SERVICE{

	final MConstructor constructor;
	final Tile tile = new Tile(this);
	public final RoomServiceNeed data;
	int used;
	int total;
	
	public ROOM_STOCKS(RoomInitData init, RoomCategorySub cat) throws IOException {
		super(0, init, "_STOCKS", cat);
		this.constructor = new MConstructor(this, init);
		
		data = new RoomServiceNeed(this, init) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				Tile t = tile.get(tx, ty);
				if (t != null) {
					return t.service;
				}
				return null;
			}
		};
		
	}
	
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new UIRoomModule() {
			
			@Override
			public void hover(GBox box, Room i, int rx, int ry) {
				
				AREA a = SETT.ROOMS().map.rooma.get(rx, ry);
				int am = 0;
				int aa = 0;
				for (COORDINATE c : a.body()) {
					if (a.is(c) && tile.get(c.x(), c.y()) != null) {
						am ++;
						if (tile.get(c.x(), c.y()).state() == STATE.available)
							aa++;
					}
				}
				
				box.textLL(Dic.¤¤Available);
				box.add(GFORMAT.iofk(box.text(), aa, am));
				
				super.hover(box, i, rx, ry);
			}
		});
	}
	
	@Override
	protected void saveP(FilePutter f) {
		f.i(used);
		f.i(total);
	}

	@Override
	protected void loadP(FileGetter f) throws IOException {
		used = f.i();
		total = f.i();
	}

	@Override
	protected void clearP() {
		used = 0;
		total = 0;
	}

	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return data.finder;
	}
	
	@Override
	public MConstructor constructor() {
		return constructor;
	}

	private final ROOM_SPECTATOR activity = new ROOM_SPECTATOR() {
		
		
		@Override
		public boolean shouldBoo(int sx, int sy) {
			Tile t = tile.get(sx, sy);
			if (t != null && t.state() == STATE.used)
				return  true;
			return false;
		};
		
		@Override
		public boolean isActive(int sx, int sy) {
			Tile t = tile.get(sx, sy);
			if (t != null && t.state() == STATE.used)
				return  true;
			return false;
		};
		
		@Override
		public boolean shouldCheer(int sx, int sy) {
			return false;
		}

		@Override
		public RoomServiceAccess service() {
			return data;
		}

	};

	@Override
	public ROOM_SPECTATOR spec() {
		return activity;
	}

	
	public DIR stockDir(int tx, int ty, DIR d) {
		FurnisherItem it = SETT.ROOMS().fData.item.get(tx, ty);
		if (it == null)
			return d;
		if ((GUTIL.ran2().get(tx, ty) & 1) == 0)
			return DIR.ORTHO.getC(it.rotation-1);
		return DIR.ORTHO.getC(it.rotation+1);
	}
	
	public boolean stockIsReserved(int tx, int ty) {
		Tile t = tile.get(tx, ty);
		if (t != null)
			return t.state() == STATE.reserved || t.state() == STATE.used;
		return constructor.service(tx, ty);
	}
	
	private Coo tmp = new Coo();
	
	public COORDINATE stockReserve() {
		if (used >= total)
			return null;
		
		int am = instancesSize();
		if (am == 0)
			return null;
		int ri = RND.rInt(am);
		
		
		for (int i = 0; i< am; i++) {
			
			Instance ins = getInstance((ri+i)%am);
			if (ins.available <= 0)
				continue;
			
			for (COORDINATE c : ins.body()) {
				Tile t = tile.get(c.x(), c.y());
				if (t != null && t.state() == STATE.available) {
					t.stateSet(STATE.reserved);
					tmp.set(c);
					return tmp;
				}
				LOG.err("nono");
				break;
			}
			
			
		}

		for (int i = 0; i< am; i++) {
			
			Instance ins = getInstance(i);
			
			
			for (COORDINATE c : ins.body()) {
				Tile t = tile.get(c.x(), c.y());
				if (t != null) {
					t.stateSet(STATE.none);
				}
			}
			
			ins.available = 0;
			
			
		}
		used = 0;
		total = 0;
		
		for (int i = 0; i< am; i++) {
			
			Instance ins = getInstance(i);
			
			
			for (COORDINATE c : ins.body()) {
				Tile t = tile.get(c.x(), c.y());
				if (t != null) {
					t.stateSet(STATE.available);
				}
			}
		}
		
		return null;
	}

	public void stockUse(int tx, int ty) {
		Tile t = tile.get(tx, ty);
		if (t != null && t.state() == STATE.reserved) {
			t.stateSet(STATE.used);
		}
	}
	
	public void stockCancel(int tx, int ty) {
		Tile t = tile.get(tx, ty);
		if (t != null) {
			t.stateSet(STATE.available);
		}
	}
	@Override
	public RoomServiceNeed service() {
		return data;
	}


	@Override
	public int punishTotal() {
		return total;
	}


	@Override
	public int punishUsed() {
		return used;
	}


}
