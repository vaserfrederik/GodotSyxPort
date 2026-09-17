package settlement.room.service.arena.grand;

import java.io.IOException;

import init.sprite.SPRITES;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite1x1;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.datatypes.RECTANGLE;
import snake2d.util.file.Json;
import snake2d.util.map.MAP_BOOLEAN;
import snake2d.util.misc.CLAMP;
import util.colors.GCOLOR;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import util.text.D;
import view.main.VIEW;

final class ArenaConstructor extends Furnisher{

	private final int minDim = 24;
	private static CharSequence ¤¤notRec = "The blueprint must be in the shape of a rectangle";
	private static CharSequence ¤¤small = "This is too small to be considered a grand arena. Minimum dimensions are 24 x 24 tiles.";
	private static CharSequence ¤¤onEdge = "Entrance must be placed on the edge of the room.";
	private static CharSequence ¤¤onEdgeC = "Entrance must be placed in the center of an edge of the room.";
	
	static {
		D.ts(ArenaConstructor.class);
	}
	
	
	private final ROOM_ARENA blue;
	final FurnisherStat workers;
	final FurnisherStat spectators;
	public final CUtil util;

	private double ri = 0;
	private int cGlad = 0;
	private int cSpec = 0;
	private boolean err = false;
	
	private final RoomSprite1x1 csprite;
	
	protected ArenaConstructor(ROOM_ARENA blue, RoomInitData init)
			throws IOException {
		super(init, 1, 2);
		
		Json sp = init.data().json("SPRITES");
		util = new CUtil(this, sp);
		
		csprite = new RoomSprite1x1(sp, "CONSTRUCT_1X1");
		
		this.blue = blue;

		
		workers = new FurnisherStat(this) {

			@Override
			public GText format(GText t, double value) {
				return GFORMAT.i(t, (long) value);
			}

			@Override
			public double get(AREA area, double acc) {
				init(area);
				return cGlad;
			}
			
		};
		spectators = new FurnisherStat(this) {

			@Override
			public GText format(GText t, double value) {
				GFORMAT.i(t, (int)Math.ceil(value));
				t.s();
				t.add('(');
				GFORMAT.i(t, (int)Math.ceil(value*blue.service().totalMultiplier()));
				t.add(')');
				return t;
			}

			@Override
			public double get(AREA area, double acc) {
				init(area);
				return cSpec;
			}
			
		};
		
		
		FurnisherItemTile ee = new FurnisherItemTile(
				this,
				true,
				RoomSprite1x1.DUMMY,
				AVAILABILITY.SOLID, 
				false) {
			
			@Override
			public CharSequence isPlacable(int tx, int ty, MAP_BOOLEAN roomIs, FurnisherItem it, int rx,
					int ry) {

				if (util.getLevel(tx, ty) != 0)
					return ¤¤onEdge;
				if (!util.canBeEntrance(tx, ty))
					return ¤¤onEdgeC;

				return null;
			}
			
		};
		

		
		new FurnisherItem(new FurnisherItemTile[][] {
			{ee},

		}, 1);
		flush(4, 0);
		
		
		
	}

	private void init(AREA area) {
		if (ri != VIEW.renderSecond()) {
			err = false;
			ri = VIEW.renderSecond();
			cSpec = 0;
			cGlad = 0;
			if (area.body().width() < minDim || area.body().height()<minDim)
				err = true;
			for (COORDINATE c : area.body()) {
				if (!area.is(c)) {
					err = true;
					break;
				}
				FurnisherItemTile it = util.get(c.x(), c.y(), area);
				if (it == util.iSeat1 || it == util.iSeat2)
					cSpec ++;
				if (it == util.iArena)
					cGlad ++;
			}
			
			cSpec -= 10;
			cGlad /= 6;
			
			if (err) {
				cSpec = 0;
				cGlad = 0;
			}
			
			if (cGlad > 0) {
				cGlad = CLAMP.i(cSpec/40, 1, cGlad);
			}
			
			cSpec = Math.max(cSpec, 0);
		}
		
		
	}
	
	
	
	@Override
	public boolean usesArea() {
		return true;
	}

	@Override
	public boolean mustBeIndoors() {
		return false;
	}
	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		RECTANGLE aa = util.init(area);
		ArenaInstance a =  new ArenaInstance(blue, area, init, aa);
		for (COORDINATE c : a.body()) {
			if (a.is(c) && util.tile(c.x(), c.y()) == util.iTorch) {
				SETT.LIGHTS().candle(c.x(), c.y(), 0);
			}
		}
		return a;
	}
	
	@Override
	public CharSequence constructionProblem(AREA area) {
		if (area.body().width() < minDim || area.body().height()<minDim)
			return ¤¤small;
		for (COORDINATE c : area.body()) {
			if (!area.is(c))
				return ¤¤notRec;
		}
		return super.constructionProblem(area);
	}
	
	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {
		FurnisherItemTile it = util.get(tx, ty);
		
		if (it == util.iRim || it == util.iArena)
			super.putFloor(tx, ty, upgrade, area);
			
	}
	
	@Override
	public void renderTileBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, boolean floored) {
		if (floored && util.get(it.tx(), it.ty()) != util.iArena) {
			csprite.render(r, s, 0, it, 0, false);
		}
		super.renderTileBelow(r, s, it, floored);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}
	

	@Override
	public void renderEmbryo(SPRITE_RENDERER r, int mask, RenderIterator it, boolean isFloored, AREA area, boolean active) {
		init(area);
		
		if (err) {
			if (active)
				GCOLOR.MAP().BAD.bind();
			super.renderEmbryo(r, mask, it, isFloored, area, active);
		}else {
			
			FurnisherItemTile tile = util.get(it.tx(), it.ty(), area);
			int m = 0;
			for (DIR d : DIR.ORTHO) {
				if (util.get(it.tx()+d.x(), it.ty()+d.y(), area).availability == tile.availability)
					m|= d.mask();
			}
			
			if (tile.availability.player < 0)
				SPRITES.cons().BIG.solid.render(r, m, it.x(), it.y());
			else
				SPRITES.cons().BIG.outline.render(r, m, it.x(), it.y());
//			int ii = util.getLevel(it.tx(), it.ty());
//			Str.TMP.clear().add(ii);
//			UI.FONT().S.render(r, Str.TMP, it.x(), it.y(), 4);
		}

		
	}
	


	
}
