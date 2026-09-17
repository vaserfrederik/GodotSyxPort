package settlement.room.infra.hauler;

import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TILE_BOUNDS;

import java.io.IOException;

import game.faction.FACTIONS;
import game.faction.FResources.RTYPE;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.resources.STOCKPILE;
import settlement.misc.util.RESOURCE_TILE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.job.ROOM_RADIUS.ROOM_RADIUSE;
import settlement.room.main.util.RoomInitData;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_HAULER extends RoomBlueprintIns<HaulerInstance> implements ROOM_RADIUSE, ROOM_EMPLOY_AUTO{

	private final Furnisher constructor;
	final Crate crate = new Crate(this);
	public final HaulerTally tally = new HaulerTally();
	
	
	public ROOM_HAULER(RoomInitData init, RoomCategorySub cat) throws IOException {
		super(0, init, "_HAULER", cat);
		constructor = new Constructor(init);
	}

	@Override
	protected void update(double ds) {


	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return null;
	}

	@Override
	protected void saveP(FilePutter saveFile){

	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		this.tally.clear();
		for (HaulerInstance ins : all()) {
			tally.init(ins);
		}
	}
	
	@Override
	protected void clearP() {
		this.tally.clear();
	}
	
	@Override
	public boolean degrades() {
		return false;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}

	@Override
	public boolean autoEmploy(Room r) {
		return ((HaulerInstance)r).auto;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((HaulerInstance)r).auto = b;
	}

	@Override
	public ROOM_RADIUS_INSTANCE radiusInstance(Room t) {
		return ((HaulerInstance) t);
	}
	
	public void removeFromEverywhere(STOCKPILE.StockpileImp stock, RTYPE record) {
		double[] res = new double[RESOURCES.ALL().size()];
		for (RESOURCE r : RESOURCES.ALL()) {
			double d = stock.get(r) /(1.0 + tally.amountReservable.get(r));
			d = CLAMP.d(d, 0, 1);
			res[r.index()] = d;
		}
		
		for (COORDINATE c : TILE_BOUNDS) {
			Room r = ROOMS().STOCKPILE.get(c.x(), c.y());
			if (r == null)
				continue;
			RESOURCE_TILE cr = (RESOURCE_TILE) r.storage(c.x(), c.y());
			if (cr != null && cr.resource() != null && stock.get(cr.resource()) > 0 && res[cr.resource().index()] > 0) {
				int a = (int) Math.ceil(res[cr.resource().index()]*cr.reservable());
				stock.inc(cr.resource(), -a);
				for (int i = 0; i < a; i++) {
					cr.findableReserve();
					cr.resourcePickup();
				}
				FACTIONS.player().res().inc(cr.resource(), record, -a);
			}
		}
	}
	

}
