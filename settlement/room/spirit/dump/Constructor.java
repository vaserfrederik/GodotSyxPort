package settlement.room.spirit.dump;

import java.io.IOException;

import settlement.main.SETT;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.sprite.TILE_SHEET;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import util.spritecomposer.ComposerDests;
import util.spritecomposer.ComposerSources;
import util.spritecomposer.ComposerThings.ITileSheet;
import util.spritecomposer.ComposerUtil;
import util.text.D;

final class Constructor extends Furnisher{

	private final ROOM_DUMP p;
	private final TILE_SHEET sheet;
	
	final FurnisherStat total = new FurnisherStat(this, 1) {
		
		@Override
		public double get(AREA area, double acc) {
			int a = 0;
			for (COORDINATE c : area.body()) {
				if (area.is(c)) {
					if (!isEdge(c.x(), c.y(), area))
						a++;
				}
			}
			
			return a;
		}
		
		@Override
		public GText format(GText t, double value) {
			return GFORMAT.i(t, (int) value);
		}
	};
	
	protected Constructor(ROOM_DUMP p, RoomInitData init) throws IOException {
		super(init, 0, 1, 384, 108);
		this.p = p;
		
		sheet = new ITileSheet(init.sp(), 384, 64) {
			
			@Override
			protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)  {
				s.house2.init(0, 0, 2, 1, d.s16);
				s.house2.setVar(0).paste(1, true);
				s.house2.setVar(1).paste(1, true);
				return d.s16.saveGame();
			}
		}.get();

	}

	@Override
	public boolean usesArea() {
		return true;
	}
	
	private static CharSequence ¤¤TooThin = "¤Area is too thin at places. Expand the area to at least 3x3 everywhere.";
	
	@Override
	public boolean joinsWithFloor() {
		return true;
	}
	
	static {
		D.ts(Constructor.class);
	}
	
	@Override
	public CharSequence constructionProblem(AREA area) {
		for (COORDINATE c : area.body()){
			
			if (area.is(c)) {
				boolean any = false;
				for (int di = 0; di < DIR.ALLC.size(); di++) {
					DIR d = DIR.ALLC.get(di);
					if (area.is(c, d) && !isEdge(c.x()+d.x(), c.y()+d.y(), area)) {
						any = true;
						break;
					}
				}
				if (!any)
					return ¤¤TooThin;
			}
			
		}
		
		return null;
		
	}
	
	@Override
	public boolean mustBeIndoors() {
		return false;
	}
	
	@Override
	public boolean mustBeOutdoors() {
		return false;
	}

	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new DumpInstance(p, area, init);
	}

	@Override
	public RoomBlueprintImp blue() {
		return p;
	}
	
	@Override
	public void renderTileBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, boolean floored) {
		int m = SETT.ROOMS().fData.spriteData.get(it.tile());
		if ((m & 0b010000) != 0) {
			sheet.render(r, (m&0x0F)+16*(it.ran()&0b011), it.x(), it.y());
		}else if (blue().is(it.tile())){
			floors.get(0).tint.color.bind();
			floors.get(0).sheet.render(r, it.ran()&0x0F, it.x(), it.y());
		}
	}
	
	@Override
	public void doBeforePlanning(int tx, int ty) {
		SETT.ROOMS().fData.spriteData.set(tx, ty, 0);
		super.doBeforePlanning(tx, ty);
	}
	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		if (isEdge(tx, ty, area)) {
			set(tx, ty, area);
			for (int di = 0; di < DIR.ALL.size(); di++) {
				DIR d = DIR.ALL.get(di);
				if (isEdge(tx+d.x(), ty+d.y(), area))
					set(tx+d.x(), ty+d.y(), area);
			}
		}else {
			//super.putFloor(tx, ty, upgrade, area);
			SETT.ROOMS().fData.spriteData.set(tx, ty, 0);
		}
	}
	
	private void set(int tx, int ty, AREA area) {
		int m = 0;
		for (DIR d: DIR.NORTHO) {
			if (joins(tx, ty, d, area) && joins(tx, ty, d.next(-1), area) && joins(tx, ty, d.next(1), area))
				m |= d.mask();
		}
		SETT.ROOMS().fData.spriteData.set(tx, ty, 0b10000|m);
	}
	
	private boolean joins(int tx, int ty, DIR d, AREA area) {
		tx += d.x();
		ty += d.y();
		if (!area.is(tx, ty))
			return true;
		if (isEdge(tx, ty, area))
			return true;
		return false;
	}

	public boolean isEdge(int tx, int ty, AREA area) {
		if (!area.is(tx, ty))
			return false;
		for (DIR d: DIR.ALL)
			if (!area.is(tx, ty, d))
				return true;
		return false;
	}
	
}
