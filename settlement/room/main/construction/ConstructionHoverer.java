package settlement.room.main.construction;

import static settlement.room.main.construction.ConstructionData.dBroken;
import static settlement.room.main.construction.ConstructionData.dConstructed;
import static settlement.room.main.construction.ConstructionData.dFloored;
import static settlement.room.main.construction.ConstructionData.dResAllocated;
import static settlement.room.main.construction.ConstructionData.dResourceNeeded;
import static settlement.room.main.construction.ConstructionData.dWorkAmount;

import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.settings.S;
import init.sprite.UI.Icon;
import init.sprite.UI.UI;
import settlement.job.Job;
import settlement.main.SETT;
import settlement.room.main.Room;
import settlement.room.main.furnisher.FurnisherItem;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.Alloc;
import snake2d.util.misc.CLAMP;
import util.gui.misc.GBox;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.text.D;
import view.main.VIEW;
import view.sett.ui.room.UIRoomModule;

final class ConstructionHoverer extends UIRoomModule {

	
	private static CharSequence ¤¤prog = "Construction";
	private static CharSequence ¤¤clear = "Cleared";
	private static CharSequence ¤¤Mat = "Materials";
	private static CharSequence ¤¤Act = "Dormant. Activate to commence work";
	private static CharSequence ¤¤Broken = "The room is broken. You can order your minions to repair it with the repair or activate job tool.";
	private static CharSequence ¤¤resources = "¤This construction needs {0} to complete, which is unobtainable in your city.";
	
	static {
		D.ts(ConstructionHoverer.class);
	}
	private final int[] resNeeded = Alloc.ii(RESOURCES.ALL().size());
	private final int[] resAllocated = Alloc.ii(RESOURCES.ALL().size());

	public ConstructionHoverer() {
		// TODO Auto-generated constructor stub
	}
	
	@Override
	public void hover(GBox box, Room r, int rx, int ry) {
		
		ConstructionInstance k = (ConstructionInstance) r;
		box.clear();
		box.add(k.constructor().blue().icon);
		box.textLL(k.name(rx, ry));
		box.NL(8);
		
		if (!k.active) {
			if (k.broken) {
				box.add(box.text().errorify().add(¤¤Broken));
			}else
				box.add(box.text().errorify().add(¤¤Act));
			
		}

		for (int i = 0; i < k.blueprint.resources(); i++) {
			resNeeded[i] = 0;
			resAllocated[i] = 0;
		}
		
		int clearNeeded = 0;
		int floorNeeded = 0;
		int structuresNeeded = 0;
		int structureResources = 0;
		int itemNeeded = 0;
		int itemTotal = 0;
		
		for (COORDINATE c : k.body()) {
			if (!k.is(c))
				continue;
			if (k.needsClear(c))
				clearNeeded ++;
			if (k.structureI >= 0 && !SETT.TERRAIN().CAVE.is(c) && !SETT.TERRAIN().MOUNTAIN.isMountain(c.x(), c.y()) && !SETT.TERRAIN().BUILDINGS.all().get(k.structureI).roof.is(c)) {
				structuresNeeded++;
				structureResources += dWorkAmount.get(c);
			}
			if (dFloored.is(c, 0))
				floorNeeded ++;
			FurnisherItem it = SETT.ROOMS().fData.item.get(c);
			if (it != null)
				itemTotal ++;
			if (it != null && (dConstructed.is(c, 0) || dBroken.is(c, 1))) {
				itemNeeded ++;
//				for (int i = 0; i < k.blueprint.resources(); i++) {
//					resNeeded[i] += it.cost(i);
//				}
			}
			if (SETT.ROOMS().fData.isMaster.is(c)) {
				
//				for (int i = 0; i < k.blueprint.resources(); i++) {
//					resNeeded[i] += it.cost(i);
//					if (dConstructed.is(c, 1) && dBroken.is(c, 0)) {
//						resAllocated[i] += it.cost(i);
//					}
//				}
				
				
			}
			
			
			int am = dResAllocated.get(c);
			for (int i = 0; i < k.blueprint.resources(); i++) {
				int b =  dResourceNeeded[i].get(c);
				resNeeded[i] += b;
				if (am > 0) {
					int a = CLAMP.i(am, 0, b);
					resAllocated[i] += a;
					am -= a;
				}
			}
		}


		box.NL();
		box.textLL(¤¤clear);
		box.tab(5);
		box.add(GFORMAT.perc(box.text(), (k.area()-clearNeeded)/(double)k.area(), 1));
		
		box.NL();
		box.textLL(¤¤Mat);
		box.tab(5);
		
		if (k.structureI >= 0) {
			RESOURCE sRes = SETT.TERRAIN().BUILDINGS.all().get(k.structureI).structure.resource;
			int sResA = SETT.TERRAIN().BUILDINGS.all().get(k.structureI).structure.resAmount;
			int kkkk = -1;
			for (int i = 0; i < k.blueprint.resources(); i++) {
				if (k.blueprint.resource(i) == sRes) {
					kkkk = i;
					resAllocated[i] += structureResources; 
					resAllocated[i] += (k.area()-structuresNeeded)*sResA;
					resNeeded[i] += k.area()*sResA;
				}
				
			}
			if (kkkk == -1 && sRes != null && structuresNeeded > 0) {
				box.setResource(sRes,structureResources + (k.area()-structuresNeeded)*sResA, k.area()*sResA);
			}
		}

		for (int i = 0; i < k.blueprint.resources(); i++) {
			if (resNeeded[i] > 0) {
				RESOURCE res = k.blueprint.resource(i);
				box.setResource(res, resAllocated[i], resNeeded[i]);
			}
			
		}
		box.NL();
		
		for (int i = 0; i < k.blueprint.resources(); i++) {
			if (resNeeded[i] > 0 && resAllocated[i] < resNeeded[i] && !SETT.PATH().finders.resource.has(rx, ry, k.blueprint.resource(i).bit)) {
				RESOURCE res = k.blueprint.resource(i);
				GText t = box.text();
				t.add(¤¤resources);
				t.insert(0, res.names);
				t.errorify();
				if (res.specialHelpText != null) {
					t.s().add(res.specialHelpText);
				}
				box.add(res.icon().big);
				box.add(t);
				box.NL();
				
				
			}
			
		}
		box.NL();

		
		
		box.NL();
		box.textLL(¤¤prog);
		box.tab(5);
		double total = k.area() + itemTotal;
		double p = floorNeeded + k.builtNeeded;
		double t = (total-p)/total;
		box.add(GFORMAT.perc(box.text(), t));

		box.NL();
		

		
		if (S.get().developer) {
			box.NL();
			k.debug(box);
			box.NL();box.NL();
			box.add(box.text().add("nClear: ").add(clearNeeded));
			box.add(box.text().add("nFloor: ").add(floorNeeded));
			box.add(box.text().add("nStruc: ").add(structuresNeeded));
			box.add(box.text().add("nItem: ").add(itemNeeded).add('/').add(itemTotal));
			
			Job j = SETT.JOBS().getter.get(VIEW.s().getWindow().tile());
			if (j != null) {
				box.NL(8);
				j.hover(box);
			}
			
		}
	}
	
	private final GText t = new GText(UI.FONT().M, 16);
	
	void renderButt(ConstructionInstance k, SPRITE_RENDERER r, int x1, int cy) {
		
		k.icon().renderCY(r, x1, cy);

		
		int clearNeeded = 0;
		int floorNeeded = 0;
		int structNeeded = 0;
		int itemTotal = 0;
		
		for (COORDINATE c : k.body()) {
			if (!k.is(c))
				continue;
			if (k.needsClear(c))
				clearNeeded ++;
			if (k.structureI >= 0 && !SETT.TERRAIN().CAVE.is(c) && !SETT.TERRAIN().MOUNTAIN.isMountain(c.x(), c.y()) && !SETT.TERRAIN().BUILDINGS.all().get(k.structureI).roof.is(c)) {
				structNeeded ++;
			}
			if (dFloored.is(c, 0)) {
				floorNeeded ++;
			}
			FurnisherItem it = SETT.ROOMS().fData.item.get(c);
			if (it != null)
				itemTotal ++;
			
			
			

		}



		double total = k.area() + k.area() + itemTotal;
		double p = floorNeeded + k.builtNeeded + clearNeeded;
		if (k.structureI >= 0) {
			total += k.area();
			p += structNeeded;
		}
		double prog = (total-p)/total;
		
		
		t.clear();
		GFORMAT.percGood(t, prog);
		
		if (!k.active)
			t.errorify();
		
		t.renderCY(r, x1+Icon.L+8, cy);
		
		
	}
	
}
