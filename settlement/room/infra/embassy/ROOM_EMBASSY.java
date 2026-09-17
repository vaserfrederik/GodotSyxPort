package settlement.room.infra.embassy;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import game.boosting.BOOSTABLES;
import game.boosting.BSourceInfo;
import game.boosting.BoosterImp;
import game.faction.player.Player;
import init.type.HCLASS_RACE;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.consumption.ConsumptionGui;
import settlement.room.industry.module.consumption.ConsumptionJob;
import settlement.room.industry.module.consumption.RoomConsumption;
import settlement.room.industry.module.consumption.RoomConsumption.ROOM_CONSUMPTION_HASER;
import settlement.room.infra.admin.AdminData;
import settlement.room.infra.admin.AdminData.ROOM_ADMIN_HOLDER;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.rnd.RND;
import snake2d.util.sets.LISTE;
import util.data.BOOLEANCoo;
import util.text.D;
import view.sett.ui.room.UIRoomModule;

public final class ROOM_EMBASSY extends RoomBlueprintIns<EmbassyInstance> implements ROOM_ADMIN_HOLDER, ROOM_CONSUMPTION_HASER{
	
	final ConsumptionJob job;
	final Constructor constructor;
	private final RoomConsumption consumption;
	final AdminData data;
	
	final BOOLEANCoo isJob = new BOOLEANCoo() {
		
		@Override
		public boolean is(int tx, int ty) {
			return ROOMS().fData.tileData.get(tx, ty) == Constructor.IWORK;
		}
	};
	
	public ROOM_EMBASSY(RoomInitData init, RoomCategorySub block) throws IOException {
		super(0, init, "_EMBASSY", block);
		
		
		pushBo(init.data(), type, true);
		constructor = new Constructor(this, init);
		
		
		consumption = new RoomConsumption(this, init.data(), bonus);
		consumption.roomBoosts.add(constructor.efficiency);
		
		job = new ConsumptionJob(this, consumption, 45, isJob) {
			
			@Override
			protected void perform(double time, double skill) {
				data.perform(time, skill);
				
			}

			
			@Override
			public boolean jobUseTool() {
				return false;
			}
			
			@Override
			public boolean jobUseHands() {
				return RND.rBoolean();
			}
		};
		
		data = new AdminData(employmentExtra(), init.data(), bonus());
		
		final double max = 10000000;
		final double maxI = 1.0/max;
		new BoosterImp(new BSourceInfo(info.names, iconBig().small), 0, 10000000, false) {
			
			@Override
			public double vGet(Player f) {
				return data.value()*maxI;
			};
			
			@Override
			public double vGet(HCLASS_RACE reg) {
				return data.value()*maxI;
			};
			
			
		}.add(BOOSTABLES.CIVICS().DIPLOMACY);
		
		employment().countInputSet();
	}
	
	@Override
	protected void update(double ds) {
		data.update();
	}
	
	@Override
	public SFinderRoomService service(int tx, int ty) {
		return null;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		data.save(saveFile);
		consumption.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		data.load(saveFile);
		consumption.load(saveFile);
			
	}
	
	@Override
	protected void clearP() {
		data.clear();
		consumption.clear();
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	static CharSequence ¤¤target = "¤The estimated amount of diplomacy points that will be produced.";
	
	static {
		D.ts(ROOM_EMBASSY.class);
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new ConsumptionGui<EmbassyInstance, ROOM_EMBASSY>(this, consumption).make());
		mm.add(new AdminData.Gui<EmbassyInstance, ROOM_EMBASSY>(this, data, consumption, BOOSTABLES.CIVICS().DIPLOMACY.name, ¤¤target).make());

	}
	
//	private final RBITImp rbit = new RBITImp();
//	
//	public RBIT neededRes(int tx, int ty) {
//		rbit.clear();
//		EmbassyInstance ins = getter.get(tx, ty);
//		if (ins != null) {
//			for (int ri = 0; ri < industry.ins().size(); ri++) {
//				if (!ins.res[ri].disabled && ins.res[ri].current + ins.res[ri].reserved < maxRes(ri, ins)/2) {
//					rbit.or(industry.ins().get(ri).resource);
//				}
//			}
//		}
//		return rbit;
//	}
//	
//	public void unreserve(int tx, int ty, RESOURCE res, int am, boolean deliver) {
//		EmbassyInstance ins = getter.get(tx, ty);
//		if (ins != null) {
//			for (int ri = 0; ri < industry.ins().size(); ri++) {
//				if (industry.ins().get(ri).resource == res) {
//					ins.res[ri].reserved -= am;
//					if (ins.res[ri].reserved < 0)
//						ins.res[ri].reserved = 0;
//					if (deliver) {
//						ins.res[ri].current += am;
//					}
//					return;
//				}
//			}
//		}
//	}
//	
//	public void reportResUnreachable(int tx, int ty) {
//		EmbassyInstance ins = getter.get(tx, ty);
//		if (ins != null) {
//			for (int ri = 0; ri < industry.ins().size(); ri++) {
//				if (ins.res[ri].current + ins.res[ri].reserved < maxRes(ri, ins)/2) {
//					ins.res[ri].unreachable = true;
//				}
//			}
//		}
//	}
//	
//	public int reserveAndReturnAmount(int tx, int ty, RESOURCE res, int maxCarry) {
//		EmbassyInstance ins = getter.get(tx, ty);
//		if (ins != null) {
//			for (int ri = 0; ri < industry.ins().size(); ri++) {
//				if (industry.ins().get(ri).resource == res) {
//					ins.res[ri].unreachable = false;
//					int am = maxRes(ri, ins) - (ins.res[ri].current + ins.res[ri].reserved);
//					am = Math.min(am, maxCarry);
//					ins.res[ri].reserved += am;
//					return am;
//				}
//			}
//		}
//		return 0;
//	}
//	
	int maxRes(int ri, EmbassyInstance ins) {
		return (int) (4*Math.ceil(consumption.ins().get(ri).rate*ins.jobs.size()));
	}
//
//	@Override
//	public LIST<Industry> industries() {
//		return indus;
//	}
//
//	@Override
//	public double industryFormatConsumptionRate(GText text, IndustryResource oo, RoomInstance ins) {
//		EmbassyInstance eins = (EmbassyInstance) ins;
//		for (int i = 0; i < industry.ins().size(); i++) {
//			if (oo == industry.ins().get(i) && eins.res[i].disabled) {
//				text.add('0');
//				return 0;
//			}
//		}
//		return INDUSTRY_HASER.super.industryFormatConsumptionRate(text, oo, ins);
//	}
	
	@Override
	public RoomConsumption consumption() {
		return consumption;
	}

	@Override
	public AdminData admin() {
		return data;
	}
	
}
