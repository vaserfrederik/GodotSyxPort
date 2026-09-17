package settlement.room.spirit.temple;

import java.io.IOException;

import game.GAME;
import game.time.TIME;
import init.race.Race;
import init.religion.RELIGIONS;
import init.religion.Religion;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.type.NEEDS;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomService;
import settlement.room.service.module.RoomService.ROOM_SERVICE_HASER;
import settlement.room.spirit.temple.TempleAltar.Prisoner;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public class ROOM_TEMPLE extends RoomBlueprintIns<TempleInstance> implements ROOM_SERVICE_HASER{

	int consumed = 0;
	private int year = TIME.years().bitsSinceStart();
	final TempleConstructor constructor;
	final RoomService service;
	final Service serviceTile;
	final TempleJob job;
	final TempleAltar altar;
	public final RESOURCE resource;
	public final double STIME;
	private double searchCooloff = 0;
	public static final String TYPE = "TEMPLE";
	
	public final Religion religion;
	
	public ROOM_TEMPLE(int typeIndex, RoomInitData data, String key, RoomCategorySub cat) throws IOException {
		super(typeIndex, data, key, cat);
		constructor = new TempleConstructor(this, data);
		serviceTile = new Service(this);
		service = new RoomService(this, data, NEEDS.TYPES().TEMPLE) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return serviceTile.get(tx, ty);
			}
			
		};
		religion = RELIGIONS.MAP().read(data.data());
		switch(data.data().value("SACRIFICE_TYPE")) {
			case "RESOURCE":
				resource = RESOURCES.map().read("SACRIFICE_RESOURCE", data.data());
				job = new TempleJob.Resources(this, resource);
				altar = new TempleAltar.Resource(this, resource);
				break;
			case "ANIMAL":
				resource = RESOURCES.LIVESTOCK();
				job = new TempleJob.Resources(this, resource);
				altar = new TempleAltar.Animal(this);
				break;
			case "HUMAN":
				resource = null;
				job = new TempleJob.None(this);
				altar = new TempleAltar.Prisoner(this);
				break;
			default:
				resource = null;
				job = null;
				altar = null;
				data.data().error(data.data().value("SACRIFICE_TYPE") + " is not a sacrifice type. Pick from RESOURCES,", "SACRIFICE_TYPE");
				break;
		}
		
		STIME = data.data().d("SACRIFICE_TIME", 0, 10);

	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}

	@Override
	protected void saveP(FilePutter f) {
		service.saver.save(f);
		f.i(consumed);
	}

	@Override
	protected void loadP(FileGetter f) throws IOException {
		service.saver.load(f);
		consumed = f.i();
	}

	@Override
	protected void clearP() {
		service.saver.clear();
		consumed = 0;
		year = TIME.years().bitsSinceStart();
	}

	@Override
	protected void update(double ds) {
		if (year != TIME.years().bitsSinceStart()) {
			consumed = 0;
			year = TIME.years().bitsSinceStart();
		}
		searchCooloff -= ds;
		if (searchCooloff < 0)
			searchCooloff = 0;
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return service.finder;
	}

	@Override
	public Furnisher constructor() {
		return constructor;
	}

	@Override
	public RoomService service() {
		return service;
	}
	
	int si = 0;
	
	public COORDINATE sacrificeReserve(Race race) {
		if (!(altar instanceof TempleAltar.Prisoner)) {
			return null;
		}
		if (searchCooloff > 0)
			return null;
		
		for (int i = 0; i < instancesSize(); i++) {
			si %= instancesSize();
			TempleInstance ins = getInstance(si);
			si++;
			if (ins.sacrificesRequired > 0) {
				int old = ins.jobs.getI();
				for (int j = 0; j < ins.jobs.size(); j++) {
					if (job.get(ins.jobs.set(j).x(), ins.jobs.get().y()) != null) {
						TempleAltar.Prisoner p = (Prisoner) altar.get(job.faceCoo().x(), job.faceCoo().y());
						if (p.sacrificeReservable()) {
							ins.jobs.set(old);
							p.sacrificeReserve(race);
							return p.coo();
						}
					}
					
				}
				ins.jobs.set(old);
				GAME.Notify("Weird!");
			}
		}
		
		searchCooloff = 60;
		return null;
		
	}
	
	public boolean sacrifices() {
		if (!(altar instanceof TempleAltar.Prisoner)) {
			return false;
		}
		return true;
	}
	
	public boolean sacrificeReserved(COORDINATE coo) {
		if (altar.get(coo.x(), coo.y())== null)
			return false;
		if (!(altar instanceof TempleAltar.Prisoner)) {
			return false;
		}
		TempleAltar.Prisoner p = (Prisoner) altar;
		return p.sacrificeReserved();
	}
	
	public void sacrificeUnreserve(COORDINATE coo) {
		if (altar.get(coo.x(), coo.y())== null)
			return;
		if (!(altar instanceof TempleAltar.Prisoner)) {
			return;
		}
		TempleAltar.Prisoner p = (Prisoner) altar;
		p.sacrificeUnreserve();
	}
	
	public void sacrificeSetReady(COORDINATE coo) {
		if (altar.get(coo.x(), coo.y())== null)
			return;
		if (!(altar instanceof TempleAltar.Prisoner)) {
			return;
		}
		TempleAltar.Prisoner p = (Prisoner) altar;
		p.sacrificeReady();
	}
	
	public double sacrificeKillAmount(COORDINATE coo) {
		if (altar.get(coo.x(), coo.y())== null)
			return 0;
		if (!(altar instanceof TempleAltar.Prisoner)) {
			return 0;
		}
		TempleAltar.Prisoner p = (Prisoner) altar;
		return p.sacrificeKillAmount();
	}

	
}
