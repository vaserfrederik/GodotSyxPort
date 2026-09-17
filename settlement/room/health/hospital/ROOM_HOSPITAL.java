package settlement.room.health.hospital;

import java.io.IOException;

import game.faction.Faction;
import init.value.GVALUES;
import init.value.Lockable;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.util.FSERVICE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryResource;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import settlement.room.service.module.RoomService;
import settlement.room.service.module.RoomService.ROOM_SERVICE_HASER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import util.gui.misc.GBox;
import util.gui.misc.GText;
import util.info.GFORMAT;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_HOSPITAL extends RoomBlueprintIns<HospitalInstance> implements ROOM_SERVICE_HASER, INDUSTRY_HASER{

	final RoomService service;
	final Constructor constructor;
	final Industry consumtion;
	final LIST<Industry> indus;
	final ArrayListGrower<Lockable<Faction>> resLocks = new ArrayListGrower<Lockable<Faction>>();

	public ROOM_HOSPITAL(RoomInitData init, RoomCategorySub block) throws IOException {
		super(0, init, "_HOSPITAL", block);

		service = new RoomService(this, init, null) {
			
			@Override
			public FSERVICE service(int tx, int ty) {
				return Bed.service(tx, ty);
			}
			
			@Override
			public double totalMultiplier() {
				return 1;
			}
		};
				
		constructor = new Constructor(this,init);
		consumtion = new Industry(this, init.data(), null){
			
			@Override
			public double consumptionRate(RoomInstance ins, Humanoid h, IndustryResource oo) {
				HospitalInstance ii = (HospitalInstance) ins;
				double n = ii.service().load()*ii.service().total();
				if (!ii.fetch[oo.index()])
					n = 0;
				if (ii.employees().employed() == 0)
					return 0;
				return n/ii.employees().employed();
			}
			
		};
		indus = new ArrayList<>(consumtion);
		
		employment().countInputSet();
		
		for (IndustryResource i : consumtion.ins()) {
			resLocks.add(GVALUES.FACTION.LOCK.push("ROOM_HOSPITAL_USE_" + i.resource.key, info.name + ": " + i.resource.name, i.resource.desc, i.resource.icon()));
		}
		
		
	}
	
	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	public SFinderRoomService service(int tx, int ty) {
		return service.finder;
	}
	
	@Override
	public RoomService service() {
		return service;
	}

	@Override
	protected void saveP(FilePutter file){
		consumtion.save(file);
		service.saver.save(file);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		consumtion.load(saveFile);
		service.saver.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		consumtion.clear();
		service.saver.clear();
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}


	@Override
	public LIST<Industry> industries() {
		return indus;
	}
	
	public DIR layCoo(int tx, int ty) {
		return DIR.ORTHO.get(SETT.ROOMS().fData.spriteData.get(tx, ty)&0b011);
	}
	
	public double recoverRate(int tx, int ty) {
		double bo = get(tx, ty).quality();
		if (Bed.res1(tx, ty)) {
			bo += 1;
		}
		if (Bed.res2(tx, ty)) {
			bo += 1;
		}
		return 1 - 0.8/(bo);
	}

	@Override
	public double industryFormatConsumptionRate(GText text, IndustryResource i, RoomInstance ins) {
		
		HospitalInstance ii = (HospitalInstance) ins;
		double n = ii.service().load()*ii.service().total();
		if (!ii.fetch[i.index()])
			n = 0;
		GFORMAT.f0(text, -n);
		return n;
	}
	
	@Override
	public void industryHoverConsumptionRate(GBox b, IndustryResource i, RoomInstance ins) {
		
	}

}
