package settlement.room.industry.mine;

import java.io.IOException;

import game.boosting.Boostable;
import init.resources.Minable;
import init.resources.RESOURCES;
import init.type.TERRAINS;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryRegion;
import settlement.room.main.BonusExperience.RoomExperienceBonus;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import util.gui.misc.GBox;
import util.text.D;
import view.sett.ui.room.UIRoomModule;
import world.map.regions.Region;

public final class ROOM_MINE extends RoomBlueprintIns<MineInstance> implements INDUSTRY_HASER{

	public final static String type = "MINE";
	int rawNeeded = 2;
	final Job job;

	
	final Industry productionData;
	final Constructor constructor;
	public final Minable minable;
	
//	private final RoomBoost employed;
	final LIST<Industry> indus;
	
	public ROOM_MINE(RoomInitData init, String key, int index, RoomCategorySub cat) throws IOException {
		super(index, init, key, cat);
		
		minable = RESOURCES.minables().read(init.data());
		constructor = new Constructor(init, this);
		Boostable skill = pushBo(init.data(), type, true);
		{D.t(this);}
		
//		CharSequence out = D.g("OutputD", "When building the mine, the density of the {0} determines the output. The highest density is applied when you have but 1 worker. This value will move down to the average density the more people you employ in the mine.");
		
//		employed = new RoomBoost() {
//			
//			private final INFO info = new INFO(
//					D.g("Output"),
//					new Str(out).insert(0, minable.name)
//					);
//			
//			@Override
//			public INFO info() {
//				return info;
//			}
//			
//			@Override
//			public double get(RoomInstance r) {
//				MineInstance m = (MineInstance) r;
//				
//				double h = m.outputMax;
//				double a = constructor.deposits.get(r);
//				double d = h-a;
//				
//				double e = r.employees().hardTarget();
//				double em = r.employees().max();
//				if (e == 0)
//					return h;
//				return (h + h - (e/em)*d)/2;
//				
//			}
//		};
//		productionData = new Industry(this, null, 0, minable.resource, init.data().d("YEILD_WORKER_DAILY", 0, 1000), new RoomBoost[] {constructor.efficiency}, skill);
//		
		productionData = new Industry(this, minable.resource, init.data().d("YEILD_WORKER_DAILY", 0, 1000), skill);
		productionData.roomBoosts.add(constructor.efficiency);
		productionData.roomBoosts.add(constructor.deposits);
		
		new IndustryRegion(productionData, minable.occurence) {
			
			@Override
			public double occurence(Region reg) {
				double d = 0;
				for (int ti = 0; ti < TERRAINS.ALL().size(); ti++) {
					d += minable.terrain(TERRAINS.ALL().get(ti))*reg.info.terrain(TERRAINS.ALL().getC(index));
				}
				return d;
			}
		};
		
		
		job = new Job(this, init.data().i("STORAGE", 4, 500));
		indus = new ArrayList<>(productionData);
		
		new RoomExperienceBonus(this, init.data(), skill);
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}

	
	
	@Override
	protected void update(double ds) {
		
	}
	
	@Override
	public SFinderRoomService service(int tx, int ty) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	protected void saveP(FilePutter saveFile){
		productionData.save(saveFile);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		productionData.load(saveFile);
	}
	
	@Override
	protected void clearP() {
		productionData.clear();
	}
	
	@Override
	public boolean makesDudesDirty() {
		return true;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(constructor.deposits.applier(this));
		mm.add(constructor.efficiency.applier(this));
//		mm.add(new UIRoomModule() {
//			
//
//			
//			@Override
//			public void appendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1) {
//				section.addRelBody(4, DIR.S, new GStat() {
//					@Override
//					public void update(GText text) {
//						GFORMAT.perc(text, employed.get((RoomInstance) get.get()));
//					}
//				}.hh(employed.info()));
//			}
//			
//			
//		});
		mm.add(new UIRoomModule() {
			@Override
			public void hover(GBox box, Room i, int rx, int ry) {
				box.NL();
				box.add(box.text().add(((MineInstance)(i)).workage));
			}
		});
	}
	
	@Override
	public LIST<Industry> industries() {
		return indus;
	}


}
