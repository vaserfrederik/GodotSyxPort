package settlement.room.main.copy;

import init.sprite.SPRITES;
import init.sprite.UI.Icon;
import init.sprite.UI.UI;
import settlement.main.SETT;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.construction.ConstructionInit;
import settlement.room.main.copy.SavedPrints.SavedPrint;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.placement.PLACEMENT;
import settlement.room.main.placement.UtilWallPlacability;
import settlement.room.main.util.RoomState;
import settlement.tilemap.terrain.TBuilding;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.clickable.CLICKABLE;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import util.gui.misc.GButt;
import util.text.Dic;
import view.main.VIEW;
import view.tool.PlacableFixed;
import view.tool.PlacableFixedImp;

public final class SavedPrintsPlacer {

	private SavedPrint print;
	private final BSwap swap;
	private GuiSection sSelect = new GuiSection();
	private boolean w = true;
	
	private final CLICKABLE bOverlay = new GButt.ButtPanel(UI.icons().s.eye.sized(Icon.M)) {
		
		@Override
		protected void clickA() {
			SETT.ROOMS().placement.placer.showOverlay.toggle();
		}
		
		@Override
		protected void renAction() {
			selectedSet(SETT.ROOMS().placement.placer.showOverlay.is());
		}
		
		@Override
		public void hoverInfoGet(GUI_BOX text) {
			text.title(Dic.¤¤Overlay);
			SETT.ROOMS().placement.placer.structure.get();
			if (swap.current().constructor().overlay() != null && swap.current().constructor().overlay().desc != null) {
				text.text(swap.current().constructor().overlay().desc);
			}
		};
	};
	
	private final CLICKABLE bFoundation = new GButt.ButtPanel(UI.icons().m.foundation) {
		
		@Override
		protected void clickA() {
			SETT.ROOMS().placement.placer.showFoundation.toggle();
		}
		
		@Override
		protected void renAction() {
			selectedSet(SETT.ROOMS().placement.placer.showFoundation.is());
		}
		
		@Override
		public void hoverInfoGet(GUI_BOX text) {
			text.title(SETT.OVERLAY().FOUNDATION.name);
			text.text(SETT.OVERLAY().FOUNDATION.desc);
		};
	};
	
	private final ArrayList<CLICKABLE> walls = new ArrayList<CLICKABLE>(
		new GButt.ButtPanel(SPRITES.icons().m.wall) {
			
			private String s = "Include walls";
			@Override
			protected void clickA() {
				w = !w;
				
			};
			@Override
			protected void renAction() {
				selectedSet(w);
			};
			@Override
			public void hoverInfoGet(GUI_BOX text) {
				text.text(s);
			};
		}.setDim(Icon.L+4),
		new GButt.ButtPanel(SPRITES.icons().m.wall) {
		
			@Override
			protected void clickA() {
				VIEW.inters().popup.show(sSelect, this);
				
			};
			@Override
			protected void renAction() {
				activeSet(w);
				if (structure() != null)
					replaceLabel(structure().iconCombo, DIR.C);
			};
		}.setDim(Icon.L+4)
	);
	
	private final ArrayList<CLICKABLE> butts = new ArrayList<CLICKABLE>(walls.size()+1);
	
	SavedPrintsPlacer(BSwap swap){

		this.swap = swap;
		for (TBuilding b : SETT.TERRAIN().BUILDINGS.all()) {
			sSelect.addDown(0, new GButt.ButtPanel(b.iconCombo) {
				
				@Override
				protected void clickA() {
					SETT.ROOMS().placement.placer.structure.set(b);
					VIEW.inters().popup.close();
				}
				
				@Override
				protected void renAction() {
					selectedSet(structure() == b);
				};
				
			}.hoverTitleSet(b.structure.name));
		}
	}
	
	private TBuilding structure() {
		TBuilding structure = SETT.ROOMS().placement.placer.structure.get();
		if (structure == null) {
			SETT.ROOMS().placement.placer.structure.set(SETT.TERRAIN().BUILDINGS.MUD);
			structure = SETT.TERRAIN().BUILDINGS.MUD;
		}
		return structure;
	}
	
	public void place(SavedPrint print) {
		this.print = print;
		swap.init(print.blue);
//		structure = print.structure;
//		if (print.blue.constructor().mustBeIndoors() && structure == null) {
//			structure = SETT.TERRAIN().BUILDINGS.MUD;
//		}
		
		VIEW.s().tools.place(placer);
	}
	
	public void place(SavedPrint print, RoomBlueprint blue) {
		if (blue.getClass() != print.blue.getClass())
			throw new RuntimeException();
		this.print = print;
		swap.init((RoomBlueprintImp) blue);
//		structure = print.structure;
//		if (print.blue.constructor().mustBeIndoors() && structure == null) {
//			structure = SETT.TERRAIN().BUILDINGS.MUD;
//		}
		VIEW.s().tools.place(placer);
	}


	
	private final PlacableFixed placer = new PlacableFixedImp(null, 4, 1) {
		
		private final Coo cTmp = new Coo();
		

		
		@Override
		public void place(final int tx, final int ty, int rrx, int rry) {
			
			TmpArea tmp = SETT.ROOMS().tmpArea(this);
			
			COORDINATE r = getSourceTile(rrx, rry);
			
			int rx = r.x();
			int ry = r.y();
			
			Furnisher furnisher = swap.current().constructor();
			
			if(w && furnisher.mustBeIndoors() && structure() != null && !print.isRoom(rx, ry)) {
				if(print.isWall(rx, ry) && UtilWallPlacability.wallShouldBuild.is(tx, ty)) {
					UtilWallPlacability.wallBuild(tx, ty, structure());
				}else if(print.isRoof(rx, ry) && UtilWallPlacability.openingShouldBuild.is(tx, ty)) {
					UtilWallPlacability.openingBuild(tx, ty, structure());
				}
			}
			
			if (rrx != 0 || rry != 0) {
				tmp.clear();
				return;
			}
			
			
			int w = width();
			int h = height();
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					COORDINATE c = getSourceTile(x, y);
					rx = c.x();
					ry = c.y();
					if (!print.isRoom(rx, ry))
						continue;
					tmp.set(tx+x, ty+y);
				}
			}
			
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					COORDINATE c = getSourceTile(x, y);
					rx = c.x();
					ry = c.y();
					if (!print.isRoom(rx, ry))
						continue;
					FurnisherItem it = print.item(rx, ry, furnisher.blue());
					if (it == null)
						continue;
					c = rotate(-it.firstX(), -it.firstY());
					int dx = c.x();
					int dy = c.y();
					
					FurnisherItem it2 = it.group.item(it.variation(), (it.rotation + rot())%it.group.rotations());
					r = getSourceItemOff(it2, x+tx+dx, y+ty+dy);
					int x1 = r.x();
					int y1 = r.y();
					
					
					SETT.ROOMS().fData.itemSet(x1, y1, it2, tmp.room());
				}
			}
			
			ConstructionInit init = new ConstructionInit(0, furnisher, structure(), 0, RoomState.DUMMY);
			SETT.ROOMS().construction.createClean(tmp, init);;
		}
		
		@Override
		public CharSequence placable(int tx, int ty, int rx, int ry) {
			COORDINATE c = getSourceTile(rx, ry);
			if (!print.isRoom(c.x(), c.y()))
				return null;
			Furnisher furnisher = swap.current().constructor();
			CharSequence s = PLACEMENT.placable(tx, ty, furnisher.blue(), true);
			if (s != null)
				return s;
			return furnisher.placable(tx, ty, SETT.ROOMS().fData.item.get(c), SETT.ROOMS().fData.tile.get(c));
		}
		
		@Override
		public int width() {
			int wi = (rot() & 1) == 1 ? print.height : print.width;
			return wi;
		}
		
		@Override
		public int height() {
			int h = (rot() & 1) == 0 ? print.height : print.width;
			return h;
		}
		
		private COORDINATE getSourceTile(int rx, int ry) {
			switch (rot()) {
			case 0:
				cTmp.set(rx, ry);
				break;
			case 1:
				cTmp.ySet(print.height-rx-1);
				cTmp.xSet(ry);
				break;
			case 2:
				cTmp.ySet(print.height-ry-1);
				cTmp.xSet(print.width-rx-1);
				break;
			case 3:
				cTmp.ySet(rx);
				cTmp.xSet(print.width-ry-1);
				break;
			default:
				throw new RuntimeException();
			}
			return cTmp;
		}
		
		private COORDINATE getSourceItemOff(FurnisherItem i, int rx, int ry) {
			
			switch (rot()) {
			case 0:
				cTmp.set(rx, ry);
				break;
			case 1:
				cTmp.ySet(ry);
				cTmp.xSet(rx-i.width()+1);
				break;
			case 2:
				cTmp.ySet(ry-i.height()+1);
				cTmp.xSet(rx-i.width()+1);
				break;
			case 3:
				cTmp.ySet(ry-i.height()+1);
				cTmp.xSet(rx);
				break;
			default:
				throw new RuntimeException();
			}
			return cTmp;
		}
		
		private COORDINATE rotate(int rx, int ry) {
			for (int i = 0; i < rot(); i++) {
				int newX = -ry;
				int newY = rx;
				rx = newX;
				ry = newY;
			}
			cTmp.set(rx, ry);
			return cTmp;
		}
		
		@Override
		public void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, int rx, int ry, boolean isPlacable, boolean areaIsPlacable) {
			COORDINATE cr = getSourceTile(rx, ry);
			rx = cr.x();
			ry = cr.y();
			Furnisher furnisher = swap.current().constructor();
			if (print.isRoom(rx, ry)) {
				
				if (print.isSoldid(rx, ry))
					SPRITES.cons().BIG.filled.render(r, mask, x, y);
				else
					SPRITES.cons().BIG.dashed.render(r, mask, x, y);
			}else if(w && furnisher.mustBeIndoors() && structure() != null) {
				if(print.isWall(rx, ry) && UtilWallPlacability.wallShouldBuild.is(tx, ty)) {
					SPRITES.cons().BIG.filled.render(r, 0, x, y);
				}else if(print.isRoof(rx, ry) && UtilWallPlacability.openingShouldBuild.is(tx, ty)) {
					SPRITES.cons().BIG.dashed_hollow.render(r, 0, x, y);
				}
			}
		};
		
		@Override
		public void updateRegardless(view.subview.GameWindow window) {
			if (swap.current().constructor().overlay() != null && SETT.ROOMS().placement.placer.showOverlay.is()) {
				swap.current().constructor().overlay().add();
			}
			if (swap.current().constructor().isHeavy() && SETT.ROOMS().placement.placer.showFoundation.is()) {
				SETT.OVERLAY().FOUNDATION.add();
			}
			
		};
		
		@Override
		public LIST<CLICKABLE> getAdditionalButt() {
			Furnisher furnisher = swap.current().constructor();
			butts.clearSloppy();
			if (furnisher.mustBeIndoors()) {
				butts.add(walls);
			}
			if (furnisher.overlay() != null)
				butts.add(bOverlay);
			if (furnisher.isHeavy())
				butts.add(bFoundation);
			return swap.wrap(butts);
		}
		
		@Override
		public CharSequence name() {
			Furnisher furnisher = swap.current().constructor();
			return furnisher.blue().info.name;
		}
	};

	
}
