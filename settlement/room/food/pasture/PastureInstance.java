package settlement.room.food.pasture;

import static settlement.main.SETT.ENTITIES;
import static settlement.main.SETT.THINGS;
import static settlement.main.SETT.TWIDTH;

import game.GAME;
import game.faction.FACTIONS;
import game.faction.FResources.RTYPE;
import init.constant.C;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.type.HCLASSES;
import init.type.HCLASS_RACE;
import settlement.entity.ENTITY;
import settlement.entity.animal.Animal;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.util.RESOURCE_TILE;
import settlement.path.AVAILABILITY;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.IndustryUtil;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.industry.module.RoomBoost;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.job.RoomResStorage;
import settlement.room.main.util.RoomInit;
import settlement.thing.ThingsCadavers.Cadaver;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.misc.CLAMP;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

public final class PastureInstance extends RoomInstance implements JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE {

	final static double WORKERPERANIMAL = 0.15;
	private static final long serialVersionUID = 1L;
	
	private final short depX,depY;

	final short animalsMax;
	short animalsCurrent;
	short animalsCubs = 0;
	short animalsToFetch;
	
	boolean missingLivestock = false;
	boolean searchForLivestock = true;
	
	boolean auto = false;
	private long[] productionData;
	
	private float skillPrev;
	private float skill;
	float prodPrev;
	int work;
	final int workMax;
	float animalsToDie = 0;
	
	float water;
	private short waterCount = 0;
	float waterN;
	
	private short industry = 0;
	
	PastureInstance(ROOM_PASTURE p, TmpArea area, RoomInit init) {
		super(p, area, init);

		
		int dx = mX();
		int dy = mY();
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			if (blueprintI().s2.get(c.x(), c.y(), this) != null) {
				dx = c.x();
				dy = c.y();
				break;
			}
			water += SETT.GROUND().MOISTURE_TOT.get(c.x(), c.y());
		};
		
		depX = (short) dx;
		depY = (short) dy;
		animalsMax = (short) Math.ceil(blueprintI().constructor.ferarea.get(this)*p.ANIMALS_PER_TILE);
		animalsToFetch = animalsMax;
		
		workMax = (int) (blueprintI().constructor.ferarea.get(this)*ROOM_PASTURE.WORKERS_PER_TILE*blueprintI().jobsPerDay);
		skillPrev = (float) blueprintI().bonus().get(HCLASS_RACE.clP(null, HCLASSES.CITIZEN()));
		double work = blueprintI().constructor.workers.get(this);
		employees().maxSet((int) Math.ceil(work)*2);
		employees().neededSet((int) Math.ceil(work));
		productionData = industry().makeData();
		activate();
		
		
	}

	@Override
	protected void loadFix() {
		industry = (short) CLAMP.i(industry, 0, blueprintI().indus.size()-1);
		productionData = industry().makeDataFix(productionData);
	}
	
	@Override
	public void updateTileDay(int tx, int ty) {
		waterCount++;
		waterN += SETT.GROUND().MOISTURE_TOT.get(tx, ty);
		if (waterCount >= area()) {
			
			water = waterN;
			waterCount = 0;
			waterN = 0;
		}
		
		super.updateTileDay(tx, ty);
	}
	
	@Override
	protected void updateAction(double updateInterval, boolean day) {
		
		if (day) {

			int needed = neededWork(animalsCurrent);
			
			if (needed > 0) {
				
				double dskill = skill/work;
				if (work == 0)
					skill = 1;
				skillPrev = (float) dskill;
				
				double produce = 1;
				for (RoomBoost rr : blueprintI().indus.get(0).boosts()) {
					produce *= rr.get(this);
				}
				
				produce(produce);
				
				double toDie = CLAMP.d(1.0 - work/(double)needed, 0, 1)*animalsCurrent;
				int death = (int) Math.min(toDie, animalsToDie);
				if (death >= 1) {
					int kill = CLAMP.i((int)death, 0, animalsCurrent);
					kill(kill);
				}
				animalsToDie = (float) toDie;
			}else {
				skillPrev = (float) blueprintI().bonus().get(HCLASS_RACE.clP(null, HCLASSES.CITIZEN()));
			}
			
			animalsToFetch = (short) (animalsMax - animalsCurrent);
			work = 0;
			skill = 0;
			
		}
		
		searchForLivestock = true;
		
		industry().updateRoom(this);
	}
	
	public int kill(int killAmount) {
		int kill = 0;
		if (killAmount > 0) {
			for (ENTITY e : ENTITIES().fillTiles(body())){
				if (is(e.physics.tileC())) {
					if (e instanceof Animal) {
						Animal a = (Animal) e;
						if (a.domesticated()) {
							Cadaver c = a.slaugher();
							if (c != null)
								c.makeSkelleton();
							
							kill++;
							if (kill == killAmount) {
								break;
							}
						}
					}
				}
			}
		}
		return kill;
	}
	
	boolean needsWork() {
		if (work >= workMax)
			return false;
		if (animalsCurrent >= animalsMax)
			return work < workMax;
		int l = animalsCurrent + 2 + (animalsMax-animalsCurrent-animalsToFetch);
		int needed = neededWork(l);
		return work < needed;
	}
	
	int neededWork(int animals) {
		double dAnimals = (double)CLAMP.d(animals, 0, animalsMax)/animalsMax;
		int needed = (int) (dAnimals*workMax);
		return needed;
	}
	
	boolean hasLivestockFetch() {
		return animalsToFetch > 0;
	}
	
	boolean consumeALivestockFetch() {
		if (animalsToFetch > 0) {
			animalsToFetch --;
			return true;
		}
		return false;
	}
	
	void work(Humanoid skill, RESOURCE r, COORDINATE coo) {
		
		if (r == RESOURCES.LIVESTOCK()) {
			
			if (animalsCurrent < animalsMax) {
				FACTIONS.player().res().inc(r, RTYPE.PRODUCED, -1);
				Animal a = new Animal(coo.x()*C.TILE_SIZE+C.TILE_SIZEH, coo.y()*C.TILE_SIZE+C.TILE_SIZEH, blueprintI().species, null);
				if (a != null && !a.isRemoved()) {
					a.domesticate();
					animalsCubs ++;
					animalsCurrent++;
				}
			}else {
				SETT.THINGS().resources.create(coo, r, 1);
			}
		}else {
			work ++;
			this.skill += industry().bonus().get(skill.indu());
			SETT.GRASS().currentI.increment(coo, 1);
		}
		
	}
	
	private void produce(double produce) {
		
		if (produce < 0 || !Double.isFinite(produce))
			return;
		int i = 0;
		for (IndustryResource r : industry().outs()) {
			int am = r.inc(this, produce*r.rate);
			
			RoomResStorage s = dStorage(blueprintI().st[i++]);
			while(am-- > 0 && s.hasRoom()) {
				s.deposit();
			}
			if (am > 0) {
				GAME.player().res().inc(r.resource, RTYPE.SPOILAGE, -am);
			}
		}
	}

	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		boolean ret = super.render(r, shadowBatch, it);
		blueprintI().constructor.renderFence(r, shadowBatch, it, 0);
		return ret;
	}

	@Override
	protected void activateAction() {
		// TODO Auto-generated method stub

	}

	@Override
	protected void deactivateAction() {
		// TODO Auto-generated method stub

	}

	private RoomResStorage dStorage(RoomResStorage s) {
		if (s ==blueprintI().s2)
			return blueprintI().s2.get(depX, depY, this);
		for (int k = 0; k < DIR.ORTHO.size(); k++) {
			int dx = depX + DIR.ORTHO.get(k).x();
			int dy = depY + DIR.ORTHO.get(k).y();
			RoomResStorage d = s.get(dx, dy, this);
			if (d != null)
				return d;
		}
		throw new RuntimeException();
	}
	
	@Override
	protected void dispose() {
		
		for (ENTITY e : ENTITIES().fillTiles(body())){
			if (is(e.physics.tileC())) {
				if (e instanceof Animal) {
					Animal a = (Animal) e;
					if (a.domesticated()) {
						SETT.THINGS().resources.create(e.physics.tileC(), RESOURCES.LIVESTOCK(), 1);
						a.helloMyNameIsInigoMontoyaYouKilledMyFatherPrepareToDie();
						
					
					}
				}
			}
		}
		
		blueprintI().s2.get(depX, depY, this).dispose();
		dStorage(blueprintI().s1).dispose();
		dStorage(blueprintI().s3).dispose();
		
	}
	
	void slaughterAll() {
		double produce = 0;
		for (ENTITY e : ENTITIES().fillTiles(body())){
			if (is(e.physics.tileC())) {
				if (e instanceof Animal) {
					Animal a = (Animal) e;
					boolean cub = a.cub();
					if (a.domesticated()) {
						Cadaver c = a.slaugher();
						if (c != null)
							c.makeSkelleton();
						
						produce += blueprintI().slaughterAmount(cub, industry());
					}
				}
			}
		}
		
		int i = 0;
		for (IndustryResource r : industry().outs()) {
			if (r.resource == RESOURCES.LIVESTOCK())
				continue;
			int am = r.inc(this, produce*r.rate);
			RoomResStorage s = dStorage(blueprintI().st[i++]);
			while(am-- > 0 && s.hasRoom()) {
				s.deposit();
			}
			if (am > 0) {
				THINGS().resources.create(s.x(), s.y(), r.resource, am);
			}
		}
		
		if (animalsCurrent != 0)
			GAME.Notify("" + animalsCurrent);
		animalsCurrent = 0;
		animalsCubs = 0;
		animalsToFetch = animalsMax;
		work = 0;
		skill = 0;
		animalsToDie = 0;
		
	}
	
	public void removeAnimal(boolean cub) {
		animalsCurrent --;
		if (cub && animalsCubs > 0)
			animalsCubs--;
		if (animalsCurrent < 0) {
			GAME.Notify("werid!");
			animalsCurrent = 0;
		}
	}
	
	public void reportAdult() {
		if (animalsCubs > 0)
			animalsCubs--;
	}

	@Override
	public JOB_MANAGER getWork() {
		return JobManager.init(this);
	}

	@Override
	public ROOM_PASTURE blueprintI() {
		return (ROOM_PASTURE) blueprint();
	}

	@Override
	protected AVAILABILITY getAvailability(int tile) {
		int tx = tile%SETT.TWIDTH;
		int ty = tile/SETT.TWIDTH;
		if (blueprintI().constructor.isFence(this, tx, ty))
			return blueprintI().isIndoors ? AVAILABILITY.AVOID_PASS : AVAILABILITY.SOLID;
		return super.getAvailability(tile);
	}

	@Override
	public void destroyTile(int tx, int ty) {
		super.destroyTile(tx, ty);
	}

	@Override
	public boolean destroyTileCan(int tx, int ty) {
		return getAvailability(tx+ty*TWIDTH).player < 0;
	}

	@Override
	public ROOM_DEGRADER degrader(int tx, int ty) {
		return null;
	}
	
	@Override
	public RESOURCE_TILE resourceTile(int tx, int ty) {
		RESOURCE_TILE t = blueprintI().s1.get(tx, ty, this);
		if (t != null)
			return t;
		t = blueprintI().s2.get(tx, ty, this);
		if (t != null)
			return t;
		return blueprintI().s3.get(tx, ty, this);

	}
	
	@Override
	public long[] productionData() {
		return productionData;
	}


	
	double skill() {
		if (work == 0)
			return skillPrev;
		if (work < 5) {
			double d = work/5.0;
			return skillPrev*(1-d) + (skill*d)/work;
		}
		return skill/work;
	}
	
	public int animalsCurrent() {
		return animalsCurrent;
	}

	@Override
	public double productionRate(RoomInstance ins, Humanoid h, Industry in, IndustryResource oo) {
		if (employees().employed() == 0)
			return 0;
		return oo.rate*IndustryUtil.roomBonus(this, in)/employees().employed();
	}
	
	@Override
	public void setIndustry(int i) {
		blueprintI().s2.get(depX, depY, this).dispose();
		dStorage(blueprintI().s1).dispose();
		dStorage(blueprintI().s3).dispose();
		Industry in = blueprintI().industries().get(i);
		if (in == null)
			return;
		productionData = in.makeData();
		industry = (short) i;
	}

	@Override
	public Industry industry() {
		return blueprintI().industries().getC(industryI());
	}

	@Override
	public int industryI() {
		return industry;
	}
}
