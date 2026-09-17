package settlement.room.infra.inn;

import java.io.IOException;

import game.tourism.Review;
import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderFindable;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomService;
import settlement.room.service.module.RoomService.ROOM_SERVICE_HASER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_INN extends RoomBlueprintIns<InnInstance> implements ROOM_EMPLOY_AUTO, ROOM_SERVICE_HASER{

	final Constructor constructor;
	final ABed bed;
	final RoomService service;
	
	public ROOM_INN(RoomInitData init, RoomCategorySub block) throws IOException {
		super(0, init, "_INN", block);
		bed = new ABed(this);
		constructor = new Constructor(this, init);
		service = new RoomService(this, init, null) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				if (bed.init(tx, ty) != null)
					return bed.service;
				return null;
			}
		};
	}
	
	@Override
	protected void update(double ds) {
		
		
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		service.saver.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		service.saver.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		service.saver.clear();
	}
	
	@Override
	public boolean autoEmploy(Room r) {
		return ((InnInstance)r).auto;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((InnInstance)r).auto = b;
	}

	@Override
	public SFinderFindable service(int tx, int ty) {
		if (bed.init(tx, ty) != null)
			return service.finder;
		return null;
	}
	
	public DIR sleepDir(int tx, int ty) {
		for (DIR d : DIR.ORTHO) {
			if (SETT.ROOMS().fData.tileData.is(tx, ty, d, Constructor.IHEAD))
				return d;
		}
		return DIR.C;
	}
	
	public void setReview(int tx, int ty, Review rev) {
		InnInstance ins = get(tx, ty);
		Review f = ins.reviews[ins.reviews.length-1];
		for (int i = ins.reviews.length-1; i > 0; i--)
			ins.reviews[i] = ins.reviews[i-1];
		ins.reviews[0] = f;
		f.copyOther(rev);
		ins.earnings += rev.credits;
	}

	@Override
	public RoomService service() {
		return service;
	}

}
