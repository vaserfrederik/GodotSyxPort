package settlement.room.main.util;

import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.THINGS;

import game.GAME;
import game.faction.FResources.RTYPE;
import init.sprite.SPRITES;
import init.sprite.UI.Icons.S.IconS;
import init.sprite.UI.UI;
import settlement.main.SETT;
import settlement.room.main.ROOMS;
import settlement.room.main.Room;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.military.artillery.ArtilleryInstance;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Alloc;
import snake2d.util.misc.CLAMP;
import snake2d.util.rnd.RND;
import util.text.D;
import view.tool.PLACER_TYPE;
import view.tool.PlacableMulti;

public final class Deleter extends PlacableMulti{
	
	private static CharSequence ¤¤name = "Dismantle Room";
	private static CharSequence ¤¤desc = "Dismantle room, recovering some of the resources used to build it. Cannot be undone.";
	static {
		D.ts(Deleter.class);
	}
	
	public Deleter(ROOMS m){
		super(¤¤name, ¤¤desc, SPRITES.icons().m.cancel.resized(IconS.L).twin(UI.icons().m.ceiling, DIR.NE, 1));
	}
	
	@Override
	public CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE t) {
		return canRemove(tx, ty) ? null : "";
	}

	public static boolean canRemove(int tx, int ty) {
		if (ROOMS().THRONE.is(tx, ty))
			return false;
		Room r = ROOMS().map.get(tx, ty);
		if (r == null)
			return false;
		if (r instanceof ArtilleryInstance) {
			ArtilleryInstance i = (ArtilleryInstance) r;
			return i.army() == GAME.ARMIES().player();
		}
		return true;
	}
	
	@Override
	public void place(int tx, int ty, AREA a, PLACER_TYPE t) {
		
		if (ROOMS().THRONE.is(tx, ty))
			return;
		Room r = ROOMS().map.get(tx, ty);
		if (r == null)
			return;
		
		if (r instanceof ArtilleryInstance) {
			ArtilleryInstance i = (ArtilleryInstance) r;
			if (i.army() != GAME.ARMIES().player())
				return;
		}
		boolean rem = SETT.ROOMS().construction.isser.is(tx, ty);
		TmpArea ar = r.remove(tx, ty, true, this, false);
		if (!rem)
			ar.setRemoveFloor();
		if (ar != null)
			ar.clear();
	}
	
	@Override
	public boolean canBePlacedAs(PLACER_TYPE t) {
		return t == PLACER_TYPE.BRUSH || t == PLACER_TYPE.SQUARE;
	}
	
	@Override
	public boolean expandsTo(int fromX, int fromY, int toX, int toY) {
		if (ROOMS().THRONE.is(fromX, fromY))
			return false;
		
		return ROOMS().map.is(fromX, fromY) && ROOMS().map.get(fromX, fromY).isSame(fromX, fromY, toX, toY);
	}
	
	
	private final static double[] amountsd = new double[Furnisher.MAX_RESOURCES];
	private final static int[] amountsi = Alloc.ii(Furnisher.MAX_RESOURCES);
	
	private static int[] getResources(AREA r,  Furnisher furnisher, int upgrade, double degrade) {
		for (int i = 0; i < amountsd.length; i++) {
			amountsd[i] = 0;
			amountsi[i] = 0;
		}
		for (COORDINATE c : r.body()) {
			
			if (!r.is(c))
				continue;
			
			if (ROOMS().fData.isMaster.is(c)) {
				FurnisherItem it = ROOMS().fData.item.get(c);
				for (int i = 0; i < furnisher.resources(); i++) {
					amountsd[i] += it.cost2(i, upgrade);
				}
			}
		}
		
		degrade = CLAMP.d(1.0-degrade, 0, 1);
		for (int i = 0; i < furnisher.resources(); i++) {
			amountsd[i] += Math.ceil(r.area()*furnisher.areaCost(i, upgrade));
			double mm = amountsd[i]*0.75 * degrade;
			amountsi[i] = (int) mm;
			if (amountsd[i]-amountsi[i] > RND.rFloat())
				amountsi[i] ++;
			
			
		}
		return amountsi;
	}
	
	public static void scatterMaterials(AREA r, Furnisher furnisher, int upgrade, double degrade) {
		
		getResources(r, furnisher, upgrade, degrade);
		
		
		int resAll = 0;
		int resPiles = 0;
		for (int i = 0; i < furnisher.resources(); i++) {
			resAll += amountsi[i];
			resPiles += Math.ceil(amountsi[i]/32.0);
		}
		
		if (resPiles == 0)
			return;
		
		double resPerPile = (double)resAll/r.area();
		double am = 0;
		
		for (COORDINATE c : r.body()) {
			if (!r.is(c))
				continue;
			am += resPerPile;
			if (am >= 1) {
				
				int di = RND.rInt(furnisher.resources());
				for (int i = 0; i < furnisher.resources() && am >= 1; i++) {
					int ri = (di+i)%furnisher.resources();
					if (amountsi[ri] > 0) {
						
						int a = CLAMP.i(amountsi[ri], 0, (int) am);
						THINGS().resources.create(c, furnisher.resource(ri), a);
						GAME.player().res().inc(furnisher.resource(ri), RTYPE.CONSTRUCTION, a);
						am -= a;
						amountsi[ri] -= a;						
					}
				}
				
				
			}
		}
		
	}
	
	
}
