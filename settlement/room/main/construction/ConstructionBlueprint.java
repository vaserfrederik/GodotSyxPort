package settlement.room.main.construction;

import static settlement.main.SETT.GRASS;
import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TERRAIN;

import java.io.IOException;

import settlement.job.Job;
import settlement.main.SETT;
import settlement.path.finders.SFinderFindable;
import settlement.room.main.ROOMS;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.TmpArea;
import settlement.tilemap.terrain.Terrain.TerrainTile;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Rec;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.ArrayListResize;
import snake2d.util.sets.LISTE;
import view.sett.IDebugPanelSett;
import view.sett.ui.room.UIRoomModule;
import view.tool.PLACABLE;
import view.tool.PLACER_TYPE;
import view.tool.PlacableMulti;

final class ConstructionBlueprint extends RoomBlueprint {

	final ConstructionHoverer hoverer = new ConstructionHoverer();
	ArrayListResize<ConstructionInstance> all = new ArrayListResize<>(256);

	public ConstructionBlueprint(ROOMS r) {
		super("_CONSTRUCTION");
		PLACABLE q = new PlacableMulti("Finish room") {
			
			@Override
			public void place(int tx, int ty, AREA a, PLACER_TYPE t) {
				if (ROOMS().map.is(tx, ty))
					construct(tx, ty);
				else {
					Job j = SETT.JOBS().getter.get(tx, ty);
					if (j != null) {
						TerrainTile tt = j.becomes(tx, ty);
						SETT.JOBS().clearer.set(tx, ty);
						tt.placeFixed(tx, ty);
					}
				}
			}
			
			@Override
			public CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE t) {
				return (is(tx, ty) || SETT.JOBS().getter.get(tx, ty) != null) ? null : "";
			}
			
			@Override
			public boolean expandsTo(int fromX, int fromY, int toX, int toY) {
				if (is(fromX, fromY)) {
					if (is(toX, toY))
						return true;
					if (SETT.JOBS().getter.get(toX, toY) != null)
						return true;
				}
				return false;
				
			}
		};
		IDebugPanelSett.add(q);

	}

	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub

	}

	@Override
	public SFinderFindable service(int tx, int ty) {
		return null;
	}

	public ConstructionInstance create(TmpArea area, ConstructionInit init) {
		ConstructionInstance ins = new ConstructionInstance(this, area, init);
		if (SETT.ROOMS().map.get(ins.mX(), ins.mY()) == ins)
			all.add(ins);
		return ins;
	}
	
	void remove(ConstructionInstance ins) {
		all.remove(ins);
	}
	
	@Override
	public ConstructionInstance get(int tx, int ty) {
		Room r = SETT.ROOMS().map.get(tx, ty);
		if (r != null && r instanceof ConstructionInstance)
			return (ConstructionInstance) r;
		return null;
	}

	@Override
	public COLOR miniC(int tx, int ty) {
		return get(tx, ty).blueprint.miniColor(tx, ty);
	}
	
	@Override
	public COLOR miniCPimped(ColorImp origional, int tx, int ty, boolean northern, boolean southern) {
		return get(tx, ty).blueprint.miniColorPimped(origional, tx, ty, northern, southern);
	}

	@Override
	protected void save(FilePutter saveFile) {
		saveFile.object(all);

	}

	@SuppressWarnings("unchecked")
	@Override
	protected void load(FileGetter saveFile) throws IOException {
		all.clear();
		Object a = saveFile.object(true);
		if (a != null) {
			ArrayListResize<ConstructionInstance> li = (ArrayListResize<ConstructionInstance>) a;
			for (ConstructionInstance i : li) {
				if (!i.constructing)
					all.add(i);
			}
		}else
			clear();
	}

	@Override
	protected void clear() {
		all.clear();
	}

	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		mm.add(hoverer);
	}
	
	private Rec rec = new Rec();
	public void construct(int tx, int ty) {
		ConstructionInstance r = get(tx, ty);
		if (r == null)
			return;
		if (r.mX() != tx || r.mY() != ty)
			return;
		rec.set(r.body());
		for (COORDINATE c : rec) {

			if (!r.is(c))
				continue;
			r.jobClear(c.x(), c.y());
			if (r.blueprint.removeFertility())
				GRASS().current.set(c.x(), c.y(), 0);
			if (!TERRAIN().CAVE.is(c) && r.structureI != -1 && !TERRAIN().BUILDINGS.all().get(r.structureI).roof.is(c))
				TERRAIN().BUILDINGS.all().get(r.structureI).roof.placeFixed(c.x(), c.y());
			if (!TERRAIN().get(c).clearing().isStructure() && r.blueprint.removeTerrain(c.x(), c.y()))
				TERRAIN().NADA.placeFixed(c.x(), c.y());
			
			r.blueprint.putFloor(c.x(), c.y(), r.upgrade(), r);
		}
		r.finish();
		
	}


}
