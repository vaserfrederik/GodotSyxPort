package settlement.room.food.pasture;

import java.io.IOException;

import game.boosting.BoostSpec;
import game.time.TIME;
import init.resources.RESOURCE;
import init.type.CLIMATE;
import settlement.entity.animal.AnimalSpecies;
import settlement.main.SETT;
import settlement.path.finders.SFinderRoomService;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryRegion;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.industry.module.RoomBoost;
import settlement.room.main.BonusExperience.RoomExperienceBonus;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.job.RoomResStorage;
import settlement.room.main.util.RoomInitData;
import settlement.room.water.RoomIrrigated;
import settlement.room.water.RoomIrrigated.ROOM_IRRIGATED;
import settlement.weather.WeatherMoisture;
import snake2d.util.datatypes.AREA;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import util.data.DOUBLE_O;
import util.gui.misc.GBox;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.info.INFO;
import util.text.Dic;
import view.sett.ui.room.UIRoomModule;
import world.map.regions.Region;
import world.map.regions.RegionInfo;

public final class ROOM_PASTURE extends RoomBlueprintIns<PastureInstance> implements INDUSTRY_HASER, ROOM_EMPLOY_AUTO, ROOM_IRRIGATED{

	public static final String type = "PASTURE";
	final Constructor constructor;
//	final Industry productionData;
	final int jobsPerDay = TIME.getWorkPerDay(JobManager.workTime);
	final double capacityPerDay = 1.0/2.0;
	public final AnimalSpecies species;
	final LIST<Industry> indus;
	
	final double ANIMALS_PER_TILE;
	final static double WORKERS_PER_TILE = 1.0/64;
	private final RoomIrrigated irri;
	public final boolean isIndoors;
	
	final RoomResStorage s1 = new RoomResStorage(3000) {
		@Override
		public RESOURCE resource() {
			if (ins instanceof PastureInstance) {
				PastureInstance p = (PastureInstance) ins;
				if (p.industry().outs().size() > 0) {
					return p.industry().outs().get(0).resource;
				}
				return p.industry().outs().get(0).resource;
			}
			return indus.get(0).outs().get(0).resource;
		}
		
		@Override
		protected boolean is(int tx, int ty) {
			return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.STORAGE1;
		}
	};
	
	final RoomResStorage s2 = new RoomResStorage(3000) {
		@Override
		public RESOURCE resource() {
			if (ins instanceof PastureInstance) {
				PastureInstance p = (PastureInstance) ins;
				if (p.industry().outs().size() > 1) {
					return p.industry().outs().get(1).resource;
				}
				return p.industry().outs().get(0).resource;
			}
			return indus.get(0).outs().get(0).resource;
		}
		
		@Override
		protected boolean is(int tx, int ty) {
			return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.STORAGE2;
		}
	};
	
	final RoomResStorage s3 = new RoomResStorage(3000) {
		@Override
		public RESOURCE resource() {
			if (ins instanceof PastureInstance) {
				PastureInstance p = (PastureInstance) ins;
				if (p.industry().outs().size() > 2) {
					return p.industry().outs().get(2).resource;
				}
				return p.industry().outs().get(0).resource;
			}
			return indus.get(0).outs().get(0).resource;
		}
		
		@Override
		protected boolean is(int tx, int ty) {
			return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.STORAGE3;
		}
	};
	
	final RoomResStorage[] st = new RoomResStorage[] {
		s1,s2,s3
	};
	
	public ROOM_PASTURE(RoomInitData data, String key, RoomCategorySub cat, int index) throws IOException {
		super(index, data, key, cat);
		
		species = SETT.ANIMALS().map.read(data.data());
		
		ANIMALS_PER_TILE = CLAMP.d(2.5/(species.mass()+10), 0, 1.0/9);
		
		isIndoors = data.data().bool("INDOORS", false);
		
		this.constructor = isIndoors ? new ConstructorIndoor(this, data) : new ConstructorOutdoor(this, data);
		pushBo(data.data(), type, true);
		ArrayListGrower<RoomBoost> bbs = new ArrayListGrower<RoomBoost>();
		bbs.add(constructor.efficiency);
		bbs.add(new RoomBoost() {
				
				INFO info = new INFO(Dic.¤¤Capacity, Dic.¤¤Capacity);
				
				@Override
				public INFO info() {
					return info;
				}
				
				@Override
				public double get(RoomInstance r) {
					return constructor.ferarea.get(r)*ROOM_PASTURE.WORKERS_PER_TILE;
				}
			});
		bbs.add(new RoomBoost() {
				
				INFO info = new INFO(Gui.¤¤Skill, Gui.¤¤SkillD);
				
				@Override
				public INFO info() {
					return info;
				}
				
				@Override
				public double get(RoomInstance r) {
					PastureInstance p = (PastureInstance) r;
					return p.skill();
				}
			});
		bbs.add(new RoomBoost() {
			
			INFO info = new INFO(Gui.¤¤Animals, Gui.¤¤Animals);
			
			@Override
			public INFO info() {
				return info;
			}
			
			@Override
			public double get(RoomInstance r) {
				PastureInstance p = (PastureInstance) r;
				return (double)(p.animalsCurrent)/p.animalsMax;
			}
		});
		bbs.add(new RoomBoost() {
			
			INFO info = new INFO(Gui.¤¤Adults, Gui.¤¤Adults);
			
			@Override
			public INFO info() {
				return info;
			}
			
			@Override
			public double get(RoomInstance r) {
				PastureInstance p = (PastureInstance) r;
				if (p.animalsCurrent <= 0)
					return 0;
				double an = p.animalsCurrent-p.animalsCubs;
				an = CLAMP.d(an, 0, p.animalsCurrent);
				an /= p.animalsCurrent;
				return 0.1 + 0.9*an;
			}
		});
		bbs.add(new RoomBoost() {
			
			INFO info = new INFO(Gui.¤¤Tending, Gui.¤¤Tending);
			
			@Override
			public INFO info() {
				return info;
			}
			
			@Override
			public double get(RoomInstance r) {
				PastureInstance p = (PastureInstance) r;
				if (p.animalsCurrent == 0)
					return 1.0;
				
				return 1.0-(double)(p.animalsToDie)/p.animalsCurrent;
			}
		});
		
		if (isIndoors) {
			bbs.add(new RoomBoost() {
				
				INFO info = new INFO(Dic.¤¤Isolation, Dic.¤¤Isolation);
				
				@Override
				public INFO info() {
					return info;
				}
				
				@Override
				public double get(RoomInstance r) {
					return r.isolation(r.mX(), r.mY());
				}
			});
		}else {
			bbs.add(WeatherMoisture.makeBoost());
		}
		
		RoomBoost[] bos = new RoomBoost[bbs.size()];
		for (int i = 0; i < bos.length; i++)
			bos[i] = bbs.get(i);
		
		
		DOUBLE_O<Region> gg = new DOUBLE_O<Region>() {

			@Override
			public double getD(Region t) {
				return 0.5 + 0.5*RegionInfo.vArea().getAi(t);
			}
			
		};
		
		indus = Industry.createIndustries(this, data, bos, bonus(), gg);
		
		for (Industry i : indus) {
			if (i.outs().size() > 3)
				data.data().error("Can't declare more than 3 output in industry!", "");
			if (i.ins().size() > 0)
				data.data().error("Can't declare inputs to a pasture industry", "");
			
			new IndustryRegion(i, 1.0) {
				
				@Override
				public double occurence(Region reg) {
					return RegionInfo.vArea().getAi(reg);
				}
			};
		}
		
//		productionData = new Industry(this, data.data(), bos, bonus());
//		
//		if (productionData.outs().size() > 3)
//			data.data().error("Can't declare more than 3 output in industry!", "");
		
		//productionData = new Industry(this, null, null, resses, rates, bos, bonus2);
		
		//indus = new ArrayList<>(productionData);
		new RoomExperienceBonus(this, data.data(), bonus());
		
		irri = new RoomIrrigated(this, bonus, 0.5, 1.075) {
			
			@Override
			public double needed(AREA area) {
				return area.area();
			}

			@Override
			protected double irrigation(RoomInstance ins) {
				return ((PastureInstance)ins).water;
			}
		};

	}

	public double slaughterAmount(boolean cub, Industry ins) {
		
		return 8*(cub ? 0.25 : 1);
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
		IndustryUtil.save(saveFile, indus);
	}
	
	@Override
	protected void loadP(FileGetter saveFile) throws IOException{
		IndustryUtil.load(saveFile, indus);
	}
	
	@Override
	protected void clearP() {
		IndustryUtil.clear(indus);
	}
	
	@Override
	public boolean degrades() {
		return false;
	}
	
	public static boolean isGate(int tx, int ty) {
		return SETT.ROOMS().map.blueprint.get(tx, ty) instanceof ROOM_PASTURE && SETT.ROOMS().fData.tile.is(tx, ty);
	}
	
	@Override
	public Furnisher constructor() {
		return constructor;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new Gui(this).make());
	}
	
	@Override
	public LIST<Industry> industries() {
		return indus;
	}
	
	@Override
	public boolean isAvailable(CLIMATE c) {
		for (BoostSpec s : c.boosters.all()) {
			if (s.boostable == bonus() && s.booster.isMul && s.booster.min() == 0)
				return false;
		}
		return true;
	}

	@Override
	public boolean autoEmploy(Room r) {
		return ((PastureInstance) r).auto;
	}

	@Override
	public void autoEmploy(Room r, boolean b) {
		((PastureInstance) r).auto = b;
	}
	
	@Override
	public double degradeRate() {
		return 0;
	}

	@Override
	public double industryFormatProductionRate(GText text, IndustryResource i, RoomInstance ins) {
		double prod = i.rate;
		
		for (RoomBoost bb : indus.get(0).boosts()) {
			prod *= bb.get(ins);
		}
		text.add('+');
		GFORMAT.f(text, prod);
		return prod;
	}
	
	@Override
	public void industryHoverProductionRate(GBox b, IndustryResource i, RoomInstance ins) {
		Gui.industryHoverProductionRate(b, i, ins);
		
	}
	

	@Override
	public double industryFormatProductionRateEmpl(GText text, IndustryResource i, RoomInstance ins) {
		text.clear();
		double prod = i.rate;
		
		for (RoomBoost bb : indus.get(0).boosts()) {
			prod *= bb.get(ins);
		}
		return prod;
	}

	@Override
	public RoomIrrigated irrigation() {
		return irri;
	}
	
}
