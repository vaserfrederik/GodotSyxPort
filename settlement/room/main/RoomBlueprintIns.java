package settlement.room.main;

import static settlement.main.SETT.IN_BOUNDS;
import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TWIDTH;

import java.io.IOException;
import java.util.Arrays;

import game.GameDisposable;
import game.boosting.BOOSTABLES;
import game.boosting.BOOSTING;
import game.boosting.Boostable;
import game.faction.Faction;
import init.type.CLIMATE;
import init.type.CLIMATES;
import init.value.GVALUES;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.employment.RoomEmployment;
import settlement.room.main.employment.RoomEmploymentSimple;
import settlement.room.main.util.RoomInitData;
import snake2d.LOG;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.Json;
import snake2d.util.map.MAP_OBJECT;
import snake2d.util.misc.ACTION;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.ArrayListResize;
import snake2d.util.sets.LIST;
import snake2d.util.sprite.text.Str;
import util.data.DOUBLE_O;
import util.text.D;
import util.text.Dic;

public abstract class RoomBlueprintIns<T extends RoomInstance> extends RoomBlueprintImp{

	static ArrayListGrower<RoomBlueprintIns<?>> INS = new ArrayListGrower<RoomBlueprintIns<?>>();
	static {
		new GameDisposable() {
			@Override
			protected void dispose() {
				INS.clear();
			}
		};
	}
	
	private final ArrayListResize<T> all = new ArrayListResize<T>(128);
	
	private int totalArea = 0;
	int upgrades = 0;
	int averageDegrade = 0;
	private final RoomEmploymentSimple employment;
	int roomNameI = 1;
	private long[] stats = new long[32];
	private static long statL = 1000;
	private static CharSequence ¤¤Desc = "¤Production speed of: {0}";
	
	
	static {
		D.ts(RoomBlueprintIns.class);
	}

	protected RoomBlueprintIns(int typeIndex, RoomInitData data, String key, RoomCategorySub cat, ACTION wiki) throws IOException{
		super(data, typeIndex, key, cat, wiki);
		if (data.data().has("WORK"))
			employment = new RoomEmployment(this, data);
		else if (data.data().has("EMPLOYMENT")){
			employment = new RoomEmploymentSimple("EMPLOYMENT", this, data);
		}else {
			employment = null;
		}
		INS.add(this);
		String vKey = ("ROOM_" + key).replace("__", "_");
		
		GVALUES.FACTION.push(vKey + "_AMOUNT", Dic.¤¤Amount + ": " + info.names, iconBig(), new DOUBLE_O<Faction>() {

			@Override
			public double getD(Faction t) {
				return instancesSize();
			}
			
		}, false);
		
		GVALUES.FACTION.push(vKey + "_AREA", Dic.¤¤Area + ": " + info.names, iconBig(), new DOUBLE_O<Faction>() {
	
			@Override
			public double getD(Faction t) {
				return totalArea;
			}
			
		}, false);
	}
	
	protected RoomBlueprintIns(int typeIndex, RoomInitData data, String key, RoomCategorySub cat) throws IOException{
		this(typeIndex, data, key, cat, null);
	}

	protected Boostable pushBo(Json json, CharSequence name, CharSequence desc, String type, boolean upgrades) {
		return pushBo(json, name, desc, type, upgrades, 1.0);
	}
	
	protected Boostable pushBo(Json json, CharSequence name, CharSequence desc, String type, boolean upgrades, double value) {
		if (bonus != null)
			throw new RuntimeException();
		bonus = BOOSTING.push(key, value, name, desc, icon, BOOSTABLES.ROOMS());

		if (json.has("BONUS")) {
			json = json.json("BONUS");
			CLIMATES.pushBonuses(json, bonus);
		}
		if (upgrades)
			this.upgrades().pushBonus(this, bonus);
		
		return bonus;
	}
	
	protected Boostable pushBo(Json json, String type, boolean upgrades) {
		String desc = "" + new Str(¤¤Desc).insert(0, info.names);
		return pushBo(json, info.names, desc, type, upgrades);
	}
	
	@SuppressWarnings("unchecked")
	protected void removeInstance(RoomInstance rem) {
		totalArea -= rem.area();
		if (degrades())
			averageDegrade -= (int)Math.ceil((100*rem.getDegrade()));
		upgrades -= rem.upgrade()*rem.area();
		all.removeOrdered((T)rem);
		for (int i = 0; i < constructor().stats().size(); i++) {
			this.stats[i] -= (long)(rem.stat(i)*statL);
		}
			
	}
	
	@SuppressWarnings("unchecked")
	protected void addInstance(RoomInstance t) {
		totalArea += t.area();
		if (degrades())
			averageDegrade += (int)Math.ceil((100*t.getDegrade()));
		upgrades += t.upgrade()*t.area();
		all.add((T) t);
		for (int i = 0; i < constructor().stats().size(); i++) {
			this.stats[i] += (long)(t.stat(i)*statL);
		}
	}

	@Override
	protected final void save(FilePutter saveFile) {
		
		saveFile.object(all);
		saveFile.i(roomNameI);
		saveFile.i(totalArea);
		saveFile.i(averageDegrade);
		saveFile.i(upgrades);
		saveFile.ls(stats);
		int pos = saveFile.getPosition();
		saveFile.i(0);
		saveP(saveFile);
		saveFile.setAtPosition(pos, saveFile.getPosition()-pos-4);
	}
	
	@SuppressWarnings("unchecked")
	@Override
	protected final void load(FileGetter saveFile) throws IOException{
		all.clear();
		Object a = saveFile.object(true);
		roomNameI = saveFile.i();
		totalArea = saveFile.i();
		averageDegrade = saveFile.i();
		upgrades = saveFile.i();
		saveFile.ls(stats);

		if (a != null)
			all.add((ArrayListResize<T>)a);
		else
			clear();
		
		int le = saveFile.i();
		int pos = saveFile.getPosition();
		loadP(saveFile);
		if (saveFile.getPosition()-le != pos) {
			LOG.ln("room save corrupt in pLoad: " + key);
			saveFile.setPosition(pos+le);
			clearP();;
		}
		
		
	}
	
	@Override
	protected void clear() {
		roomNameI = 1;
		totalArea = 0;
		averageDegrade = 0;
		upgrades = 0;
		Arrays.fill(stats, 0);
		all.clear();
		clearP();
		return;
		
	}
	
	protected abstract void saveP(FilePutter f);
	
	protected abstract void loadP(FileGetter f) throws IOException;
	
	protected abstract void clearP();
	
	@SuppressWarnings("unchecked")
	@Override
	public final T get(int tx, int ty) {
		Room r = ROOMS().map.get(tx, ty);
		if (r != null && r.blueprint() == this)
			return (T) ROOMS().map.get(tx, ty);
		return null;
	}
	
	public final MAP_OBJECT<T> getter = new MAP_OBJECT<T>() {

		@SuppressWarnings("unchecked")
		@Override
		public T get(int tile) {
			Room r = ROOMS().map.get(tile);
			if (r != null && r.blueprint() == RoomBlueprintIns.this)
				return (T) r;
			return null;
		}

		@Override
		public T get(int tx, int ty) {
			if (IN_BOUNDS(tx, ty))
				return get(tx+ty*TWIDTH);
			return null;
		}
	
	
	};
	
	public final T getInstance(int nr) {
		return all.get(nr);
	}
	
	public final int instancesSize() {
		return all.size();
	}
	
	public final LIST<T> all(){
		return all;
	}
	
	public final RoomEmployment employmentExtra() {
		if (employment instanceof RoomEmployment)
			return (RoomEmployment) employment;
		return null;
	}
	
	@Override
	public final RoomEmploymentSimple employment() {
		return employment;
	}
	
	public boolean degrades() {
		return true;
	}
	
	public final int totalArea() {
		return totalArea;
	}
	
	public double degradeAverage() {
		if (instancesSize() == 0)
			return 0;
		return averageDegrade/(100.0*instancesSize());
	}
	
	public double averageUpgrade() {
		if (totalArea == 0)
			return 0;
		return (double)upgrades/totalArea;
	}
	

	
	@Override
	public boolean isAvailable(CLIMATE c) {
		return true;
	}
	
	public double getStat(int statIndex) {
		if (instancesSize() == 0) {
			this.stats[statIndex] = 0;
			return 0;
		}
		return this.stats[statIndex]/((double)statL*instancesSize());
	}
	
}
