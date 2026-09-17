package settlement.room.main;

import java.io.IOException;

import game.GameDisposable;
import game.audio.AUDIO;
import game.audio.Ambiance;
import game.audio.SoundRace;
import game.boosting.Boostable;
import game.faction.Faction;
import init.sprite.SPRITES;
import init.sprite.UI.Icon;
import init.type.CLIMATE;
import init.value.GVALUES;
import init.value.Lockable;
import settlement.path.finders.SFinderFindable;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.util.RoomInitData;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.misc.ACTION;
import snake2d.util.sets.ArrayListResize;
import util.info.INFO;

public abstract class RoomBlueprintImp extends RoomBlueprint{

	public final Icon icon;
	public final String type;
	public final INFO info;
	public final RoomCategorySub cat;
	protected double degradeRate;
	private final RoomUpgrades upgrades;
	private final int typeIndex;
	public final Lockable<Faction> reqs;
	
	public final Ambiance soundAmbiance;
	public final SoundRace clickSounds;
	
	protected Boostable bonus;

	static ArrayListResize<RoomBlueprintImp> IMPS = new ArrayListResize<>(10, 512);
	static {
		new GameDisposable() {
			@Override
			protected void dispose() {
				IMPS.clear();
			}
		};
	}
	
	protected RoomBlueprintImp(RoomInitData init, int typeIndex, String key, RoomCategorySub cat) throws IOException{
		this(init, typeIndex, key, cat, null);
		
	}

	protected RoomBlueprintImp(RoomInitData init, int typeIndex, String key, RoomCategorySub cat, ACTION wiki) throws IOException{
		super(key);
		init.init(key);
		this.typeIndex = typeIndex;
		if (cat != null)
			cat.add(this);
		this.type = init.type();
		info = new INFO(init.text(), wiki);
		icon = icon(init);
		this.cat = cat;
		if (init.data().has("DEGRADE_RATE"))
			degradeRate = init.data().d("DEGRADE_RATE", 0, 1);
		else
			degradeRate = 0.75;
		upgrades = new RoomUpgrades(this, init);
		IMPS.add(this);		
		reqs = GVALUES.FACTION.LOCK.push("ROOM_" + key, info.name, info.desc, icon);
		reqs.push(init.data());
		soundAmbiance = AUDIO.AMBI().get("ROOM_" + key);
		clickSounds = AUDIO.race("ROOM_CLICK_" + key);
	}
	
	private Icon icon(RoomInitData init) throws IOException{
		return SPRITES.icons().get(init.data());
	}
	

	
	public final Boostable bonus(){
		return bonus;
	}
	
	public Icon iconBig(){
		return icon;
	}

	@Override
	public abstract SFinderFindable service(int tx, int ty);

	
	@Override
	public COLOR miniC(int tx, int ty) {
		return constructor().miniColor(tx, ty);
	}
	
	@Override
	public COLOR miniCPimped(ColorImp origional, int tx, int ty, boolean northern, boolean southern) {
		return constructor().miniColorPimped(origional, tx, ty, northern, southern);
	}
	
	public abstract Furnisher constructor();
	
	
	public boolean isAvailable(CLIMATE c) {
		return true;
	}
	

	
	public double degradeRate() {
		return degradeRate;
	}
	
	public RoomUpgrades upgrades() {
		return upgrades;
	}
	
	public int typeIndex() {
		return typeIndex;
	}
	
	@Override
	public String toString() {
		return "["+index()+"]" + key;
	}
	
}
