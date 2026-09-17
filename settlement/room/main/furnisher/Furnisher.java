package settlement.room.main.furnisher;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import game.faction.FACTIONS;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.sprite.SPRITES;
import settlement.environment.SettEnvMap.SettEnv;
import settlement.environment.SettEnvMap.SettEnvValue;
import settlement.main.SETT;
import settlement.overlay.Addable;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.main.util.RoomState;
import settlement.tilemap.floor.Floors.Floor;
import settlement.tilemap.terrain.TerrainDiagonal.Diagonalizer;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LIST;
import util.gui.misc.GBox;
import util.info.INFO;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public abstract class Furnisher{
	
	public final static int MAX_RESOURCES = 4;
	
	final ArrayList<FurnisherItemTile> tiles = new ArrayList<>(255);
	final ArrayList<FurnisherItem> allItems = new ArrayList<>(255);
	final ArrayListGrower<FurnisherItemGroup> pgroups = new ArrayListGrower<>();
	final ArrayListGrower<FurnisherItemGroup> ggroups = new ArrayListGrower<>();
	final ArrayList<FurnisherStat> stats;
	private final LIST<RESOURCE> resources;
	private final double[] areaCost;
	protected final LIST<Floor> floors;

	public final COLOR miniColor;
	private final FurnisherMinimapColor colorPimp;
	
	protected static Json[] jsonGroupText;
	protected static Json[] jsonGroupData;
	protected static Json[] jsonStat;
	
	protected final double[] envValue = new double[SETT.ENV().map.all().size()];
	protected final double[] envRadius = new double[SETT.ENV().map.all().size()];
	

	protected Furnisher(RoomInitData init, int items, int stats) throws IOException {
		
		tiles.add((FurnisherItemTile)null);
		allItems.add((FurnisherItem)null);
		if (FurnisherItem.itemsTmp.size() != 0)
			throw new RuntimeException("someone forgot to flush...");
		
		Json data = init.data();
		Json text = init.text();
		
		resources = RESOURCES.map().readMany(data);
		if (resources.size() > MAX_RESOURCES)
			data.error("Too many resources declared. Max is 4", "RESOURCES");
		areaCost = data.ds("AREA_COSTS", resources.size());
		if (data.has(SETT.FLOOR().map.key)) {
			if (data.arrayIs(SETT.FLOOR().map.key)) {
				floors = SETT.FLOOR().map.readManyWarn(SETT.FLOOR().map.key, data);
			}else {
				Floor f = SETT.FLOOR().map.readTry(data);
				if (f == null)
					data.error("no floor named: ", SETT.FLOOR().map.key);
				floors = new ArrayList<>(f);
			}
			
		}else {
			floors = null;
		}
		
		jsonStat = null;
		if (stats > 0) {
			jsonStat = text.jsons("STATS", stats);
			if (stats != jsonStat.length)
				text.error("Invalid amount of stats declared. Should be " + stats + " not " + jsonStat.length, "STATS");
		}
		this.stats = new ArrayList<>(stats);
		
		jsonGroupText = null;
		jsonGroupData = null;
		if (items > 0) {
			jsonGroupData = data.jsons("ITEMS", items);
			jsonGroupText = text.jsons("ITEMS", items);
			if (items != jsonGroupData.length)
				data.error("Invalid amount of items declared. Should be " + items + " not " + jsonGroupData.length, "ITEMS");
		}
		if (items == 0)
			items = 1;
		
		
		

		
		miniColor = new ColorImp(data, "MINI_COLOR");
		colorPimp = new FurnisherMinimapColor(data);
		
		if (data.has("ENVIRONMENT_EMIT")) {
			Json j = data.json("ENVIRONMENT_EMIT");
			for (String k : j.keys()) {
				SettEnv e = SETT.ENV().map.rmap.getWarn(k, j);
				if (e != null) {
					Json jj = j.json(k);
					envValue[e.index()] = jj.d("VALUE", 0, 1);
					envRadius[e.index()] = jj.d("RADIUS", 0, 1);
				}
			}
		}
		
	}
	
	protected Furnisher(RoomInitData init, int items, int stats, int nopA, int nopB) throws IOException {
		this(init, items, stats);
				
	}
	
	public boolean envValue(SettEnv e, SettEnvValue v, int tx, int ty) {
		if (envRadius[e.index()] != 0) {
			v.radius = envRadius[e.index()];
			v.value = envValue[e.index()];
			return true;
		}
		return false;
	}
	
	public boolean envValue(SettEnv e) {
		if (envRadius[e.index()] != 0) {
			return true;
		}
		return false;
	}
	
	public final int resources() {
		return resources.size();
	}
	
	public final RESOURCE resource(int index) {
		return resources.get(index);
	}
	
	public final boolean resourceHas(int index, int upgrade) {
		return blue().upgrades().resMask(upgrade, index) > 0;
	}
	
	public final double areaCost(int index, int upgrade) {
		return areaCost[index]*blue().upgrades().resMask(upgrade, index);
	}
	
	public final double areaCostFlat(int index) {
		return areaCost[index];
	}

	public abstract boolean usesArea();

	public CharSequence placable(int tx, int ty, FurnisherItem item, FurnisherItemTile tile) {
		
		return null;
	}
	
	protected final FurnisherItemGroup flush(int min, int max, int rots) {
		
		if (rots != 0 && rots != 1 && rots != 3)
			throw new RuntimeException("" + rots);
		
		FurnisherItemGroup f = new FurnisherItemGroup(
				this, rots, 
				jsonGroupText[ggroups.size()].text("NAME"), 
				jsonGroupText[ggroups.size()].text("DESC"),
				min,
				max, 
				jsonGroupData[ggroups.size()].ds("COSTS", resources.size()),
				jsonGroupData[ggroups.size()].ds("STATS", stats.size())
				);
		ggroups.add(f);
		return f;
	}
	
	protected final void flushSingle(INFO info) {
		if (jsonGroupText != null)
			throw new RuntimeException(""+jsonGroupText.length);
		new FurnisherItemGroup(
				this, 0, 
				info.name, 
				info.desc,
				0,
				1, 
				new double[0],
				new double[0]
				);
	}
	
	public final FurnisherItemGroup flush(int rots) {
		return flush(0, Integer.MAX_VALUE, rots);
	}
	
	protected final FurnisherItemGroup flush(int min, int rots) {
		return flush(min, Integer.MAX_VALUE, rots);
	}
	
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		Floor floor = floor(upgrade);
		if (floor != null) {
			floor.placeFixed(tx, ty);
		}
	}
	
	public Floor floor(int upgrade) {
		if (floors == null || floors.size() == 0)
			return null;
		upgrade = CLAMP.i(upgrade, 0, floors.size()-1);
		return FACTIONS.player().race().appearance().floors.get(blue(), upgrade, floors.get(upgrade));
	}
	
	public final LIST<FurnisherItemGroup> pgroups() {
		return pgroups;
	}
	
	public final LIST<FurnisherItemGroup> groups() {
		return ggroups;
	}
	
	public final FurnisherItem item(int index) {
		return allItems.get(index);
	}
	
	public final FurnisherItemTile tile(int index) {
		return tiles.get(index);
	}
	
	public final LIST<FurnisherStat> stats(){
		return stats;
	}
	
	public void renderEmbryo(SPRITE_RENDERER r, int mask, RenderIterator it, boolean isFloored, AREA area, boolean active) {
		SPRITES.cons().BIG.dashed.render(r, mask, it.x(), it.y());
//		if (isFloored) {
//			if (mask != 0x0F) {
//				SPRITES.cons().BIG.dashed_hollow.render(r, mask, it.x(), it.y());
//			}
//		}else {
//			SPRITES.cons().BIG.dashed.render(r, mask, it.x(), it.y());
//		}
		
	}
	
	public void renderTileBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, boolean floored) {

	}
	
	public void renderExtra(SPRITE_RENDERER r, int x, int y, int tx, int ty, int rx, int ry, FurnisherItem item) {
		
	}

	public void doBeforePlanning(int tx, int ty) {
		
	}
	
	public boolean removeFertility() {
		return true;
	}
	
	public boolean removeTerrain(int tx, int ty) {
		return !SETT.TERRAIN().NADA.is(tx, ty);
	}
	
	public final COLOR miniColor(int tx, int ty) {
		if (colorPimp != null)
			return colorPimp.get(tx, ty);
		return miniColor;
	}
	
	public final COLOR miniColorPimped(ColorImp origional, int tx, int ty, boolean northern, boolean southern) {
		for (DIR d : DIR.ORTHO) {
			Room r2 = ROOMS().map.get(tx, ty, d);
			if (r2 == null || !r2.isSame(tx+d.x(), ty+d.y(), tx, ty))
				return origional.shadeSelf(0.8);
		}
		return origional;
	}
	
	public boolean mustBeIndoors() {
		return true;
	}
	
	public Addable overlay() {
		return null;
	}
	
	public boolean isHeavy() {
		return false;
	}
	
	public boolean needsIsolation() {
		return mustBeIndoors() && blue().degradeRate() > 0;
	}
	
	public boolean needFlooring() {
		return true;
	}
	
	public boolean mustBeOutdoors() {
		return false;
	}
	
	public abstract Room create(TmpArea area, RoomInit init);
	
	public abstract RoomBlueprintImp blue();
	
	public boolean canBeCopied() {
		return true;
	}

	public void doAfterConstructionInited() {
		
	}
	
	public FurnisherItem secretReplacementItem(int rot, FurnisherItem origional) {
		return null;
	}
	
	public CharSequence warning(AREA area) {
		return null;
	}
	
	public CharSequence constructionProblem(AREA area) {
		return null;
	}

	public void placeInfo(GBox box, FurnisherItem item, int x1, int y1) {
	
		
	}

	public boolean joinsWithFloor() {
		// TODO Auto-generated method stub
		return false;
	}
	
	public boolean isSpecialAreaPlacable() {
		return false;
	}
	
	public boolean growsGrass(int tx, int ty) {
		return false;
	}
	
	public Diagonalizer dia(int tx, int ty) {
		return null;
	}

	public RoomState getConstructionState() {
		return null;
	}
}
