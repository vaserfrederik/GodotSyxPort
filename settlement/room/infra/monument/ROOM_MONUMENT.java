package settlement.room.infra.monument;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import game.GameDisposable;
import game.battle.div.Div;
import game.boosting.BValue;
import game.boosting.BoostSpecs;
import game.faction.npc.FactionNPC;
import game.faction.player.Player;
import init.race.bio.Opinion;
import init.type.HCLASS_RACE;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.ROOMA;
import settlement.room.main.ROOMS;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.RoomSingleton;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.util.RoomInitData;
import settlement.stats.Induvidual;
import settlement.stats.STATS;
import settlement.stats.standing.StatStanding.StandingDef;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.map.MAP_DOUBLE;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.Bitsmap2D;
import snake2d.util.sets.LISTE;
import util.gui.misc.GBox;
import util.info.GFORMAT;
import util.text.Dic;
import view.sett.ui.room.UIRoomModule;
import world.map.regions.Region;

public abstract class ROOM_MONUMENT extends RoomBlueprintImp{

	public final Bitsmap2D mapData = new Bitsmap2D(0, 4, SETT.TILE_BOUNDS);
	public final Bitsmap2D mapUpgrade  = new Bitsmap2D(0, 2, SETT.TILE_BOUNDS);
	
	final AVAILABILITY avail;
	private final int MAX_VALUE;
	protected final RoomSingleton instance;
	private int area;
	private int degrade;
	private int upgrade;
	public final StandingDef defaultStanding;
	public final StandingDef defaultStandingUp;
	public final Opinion opinion;
	public final int monumentIndex;
	public final BoostSpecs boosts;
	
	private static int ii = 0;
	static {
		new GameDisposable() {
			
			@Override
			protected void dispose() {
				ii = 0;
			}
		};
	}
	
	
	
	protected ROOM_MONUMENT(RoomInitData init, int tindex, String key, RoomCategorySub cat) throws IOException {
		super(init, tindex, key, cat);
		this.instance = new Instance(init.m, this);
		defaultStanding = new StandingDef(init.data());
		if (init.data().has("STANDING_UPGRADE"))
			defaultStandingUp = new StandingDef(init.data().json("STANDING_UPGRADE"));
		else
			defaultStandingUp = null;
		
		
		opinion = new Opinion();
		opinion.read(init.text());
		monumentIndex = ii++;
		MAX_VALUE = init.data().i("MAX_VALUE");
		boosts = new BoostSpecs(info.names, icon, false);
		avail = init.data().bool("SOLID", true) ? AVAILABILITY.SOLID : AVAILABILITY.ROOM;
		BValue v = new BValue() {
			
			@Override
			public double vGet(FactionNPC f) {
				return 0;
			}
			
			@Override
			public double vGet(Player f) {
				return vGet(HCLASS_RACE.clP());
			}
			
			@Override
			public double vGet(HCLASS_RACE t) {
				return STATS.ACCESS().MONUMENTS.ALL().get(tindex).data(t.cl).getD(t.race);
			}
			
			@Override
			public double vGet(Div div) {
				return STATS.ACCESS().MONUMENTS.ALL().get(tindex).div().getD(div);
			}
			
			@Override
			public double vGet(Induvidual indu) {
				return STATS.ACCESS().MONUMENTS.ALL().get(tindex).indu().getD(indu);
			}
			
			@Override
			public double vGet(Region reg) {
				return vGet(reg.faction());
			}
		};
		boosts.read("FULFILLMENT_BONUS", init.data(), v);
		
	}

	
	@Override
	protected void save(FilePutter f) {
		f.i(area);
		f.i(degrade);
		f.i(upgrade);
	}

	@Override
	protected void load(FileGetter f) throws IOException {
		area = f.i();
		degrade = f.i();
		upgrade = f.i();
	}

	@Override
	protected void clear() {
		area = 0;
		degrade = 0;
		upgrade = 0;
	}
	
	public int area() {
		return area;
	}
	
	public double degrade() {
		if (area == 0)
			return 0;
		return (double)degrade/area;
	}
	
	public double upgrade() {
		if (area == 0)
			return 0;
		if (upgrades().max() == 0)
			return 1;
		return (double)(upgrade+area)/(area*(upgrades().max()+1));
	}
	
	@Override
	public Room get(int tx, int ty) {
		if (ROOMS().map.get(tx, ty) == instance)
			return instance;
		return null;
	}

	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return null;
	}
	
	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(new UIRoomModule() {
			@Override
			public void hover(GBox box, Room room, int rx, int ry) {
				box.NL();
				if (upgrades().max() > 0) {
					box.NL();
					box.text(Dic.¤¤Upgrade);
					box.tab(6);
					box.add(GFORMAT.iofkInv(box.text(), room.upgrade(rx, ry), upgrades().max()));
					box.NL();
				}
				box.text(Dic.¤¤Degrade);
				box.tab(6);
				box.add(GFORMAT.percInv(box.text(), room.getDegrade(rx, ry)));
				box.NL();
			}
		});
		
	}

	final static class Instance extends RoomSingleton{

		
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		Instance(ROOMS m, RoomBlueprint p){
			super(m, p);
			
		}
		
		protected Object readResolve() {
			  return blueprintI().instance;
		}

		@Override
		public ROOM_MONUMENT blueprintI() {
			return (ROOM_MONUMENT) blueprint();
		}
		
		@Override
		protected void addAction(ROOMA ins) {
			blueprintI().area += ins.area();
			blueprintI().area = CLAMP.i(blueprintI().area, 0, SETT.TAREA);
			blueprintI().degrade += ins.area()*getDegrade(ins.mX(), ins.mY());
			blueprintI().upgrade += ins.area()*upgrade(ins.mX(), ins.mY());
			
		}
		
		@Override
		protected void removeAction(ROOMA ins) {
			blueprintI().area -= ins.area();
			blueprintI().area = CLAMP.i(blueprintI().area, 0, SETT.TAREA);
			blueprintI().degrade -= ins.area()*getDegrade(ins.mX(), ins.mY());
			blueprintI().upgrade -= ins.area()*upgrade(ins.mX(), ins.mY());
			super.removeAction(ins);
		}
		
		@Override
		protected void degradeChange(int mx, int my, double oldD, double newD, boolean realDegradeChange) {
			blueprintI().degrade -= area(mx, my)*oldD;
			super.degradeChange(mx, my, oldD, newD, realDegradeChange);
			blueprintI().degrade += area(mx, my)*oldD;
			if (realDegradeChange)
				SETT.ENV().map.MONUMENT.changeDegrade(mx, my);
		}
		
		@Override
		public int upgrade(int tx, int ty) {
			return CLAMP.i(SETT.ROOMS().extraBit.get(mX(tx, ty), mY(tx, ty)), 0, blueprintI().upgrades().max());
		}
		
		@Override
		public void upgradeSet(int tx, int ty, int upgrade) {
			if (upgrade == upgrade(tx, ty))
				return;
			blueprintI().upgrade -= area(tx, ty)*upgrade(tx, ty);
			int up = CLAMP.i(upgrade, 0, blueprintI().upgrades().max());
			SETT.ROOMS().extraBit.set(tx, ty, up);
			blueprintI().upgrade += area(tx, ty)*upgrade(tx, ty);
			ROOMA a = SETT.ROOMS().map.rooma.get(tx, ty);
			for (COORDINATE c : a.body()){
				if (a.is(c))
					SETT.MAINTENANCE().setChanged(c.x(), c.y());
			}
			SETT.ENV().map.MONUMENT.changeUpgrade(tx, ty);
			
		}

	}
	
	public int[] radius = new int[]{
		5,10,15
	};
	
	public double radius(FurnisherItem it) {
		if (it == null)
			return 0;
		return radius[it.width()-1];
	}
	
	public int maxEnv() {
		return MAX_VALUE;
	}
	
	public final MAP_DOUBLE envValue = new MAP_DOUBLE() {
		
		@Override
		public double get(int tx, int ty) {
			return (double)mapData.get(tx, ty)/MAX_VALUE;
		}
		
		@Override
		public double get(int tile) {
			return (double)mapData.get(tile)/MAX_VALUE;
		}
	};
	
}
