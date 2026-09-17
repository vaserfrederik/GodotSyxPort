package settlement.room.water;

import java.io.IOException;

import game.time.TIME;
import init.constant.C;
import init.sprite.SPRITES;
import settlement.main.SETT;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.tilemap.ground.GroundType;
import snake2d.CORE;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.color.OPACITY;
import snake2d.util.color.OpacityImp;
import snake2d.util.datatypes.DIR;
import snake2d.util.map.MAP_BOOLEAN;
import snake2d.util.sprite.TILE_SHEET;
import snake2d.util.sprite.TextureCoords;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import util.spritecomposer.ComposerDests;
import util.spritecomposer.ComposerSources;
import util.spritecomposer.ComposerThings.ITileSheet;
import util.spritecomposer.ComposerUtil;

final class WSprite {

	private final ROOM_WATER w;
	private final ColorImp col = new ColorImp(20, 40, 100);
	private final TILE_SHEET stencil;
	private final TILE_SHEET sroad;
	private final TILE_SHEET edge;
	private final TILE_SHEET sbridge;
	
	WSprite(ROOM_WATER w, RoomInitData init) throws IOException{
		this.w = w;

		edge = new ITileSheet(init.gSprite.get("CANAL"), 288, 172) {
			
			@Override
			protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
				s.house.init(0, 0, 2, 2, d.s16);
				s.house.setVar(0).paste(true);
				s.house.setVar(0).pasteRotated(1, true);
				s.house.setVar(1).paste(true);
				s.house.setVar(1).pasteRotated(1, true);
				return d.s16.saveGame();
			}
		}.get();
		stencil = new ITileSheet() {

			@Override
			protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
				s.house.setVar(2).paste(true);
				return d.s16.saveGame();
			}
			
			
		}.get();
		sroad = new ITileSheet() {

			@Override
			protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
				s.house.setVar(3).paste(true);
				return d.s16.saveGame();
			}
			
			
		}.get();
		sbridge = new ITileSheet() {

			@Override
			protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
				s.singles.init(0, s.house.body().y2(), 1, 1, 2, 1, d.s16).paste(3, true);
				return d.s16.saveGame();
			}
			
			
		}.get();
	}
	
	public void render(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, int edgeMask, int flowMask, boolean bridge) {
		boolean bb = false;
		for (DIR d : DIR.ORTHO) {
			if (willConnectable.is(it.tx(), it.ty(), d))
				edgeMask |= d.mask();
			else if (bridge && !bb && !SETT.PATH().solidity.is(it.tx(), it.ty(), d) && SETT.FLOOR().getter.is(it.tx(), it.ty(), d)) {
				d = d.perpendicular();
				if (!SETT.PATH().solidity.is(it.tx(), it.ty(), d) && SETT.FLOOR().getter.is(it.tx(), it.ty(), d)) {
					bb = true;
				}
			}
		}
		int steI = edgeMask;
		edgeMask = edgeMask+16*(it.ran()&3);
		edge.render(r, edgeMask, it.x(), it.y());

		TextureCoords ste = bb ? sroad.getTexture(steI) : stencil.getTexture(steI);
		CORE.renderer().setMaxDepth(it.x(), it.x()+C.TILE_SIZE, it.y(), it.y()+C.TILE_SIZE, ste, CORE.renderer().getDepth()+1);
		
		{
			GroundType g = SETT.GROUND().MAP.get(it.tile());
			ColorImp.TMP.set(g.col(it.tile()));
			ColorImp.TMP.shadeSelf(0.8);
			ColorImp.TMP.bind();
			stencil.renderTextured(SETT.GROUND().getTexture(it.tile(), it.ran()), steI, it.x(), it.y());
			COLOR.unbind();
		}

		
		
		if (flowMask != 0){
			int ms = 0;
			int am = 0;
			for (DIR d : DIR.ORTHO) {
				if ((d.mask() & flowMask) != 0) {
					am++;
				}
				int dx = it.tx()+d.x();
				int dy = it.ty()+d.y();
				if (!w.pumpable.is(dx, dy) || w.pumpable.get(dx, dy).dirmask(dx, dy) != 0)
					ms |= d.mask();
			}
			
			
			int op = 255/(am+1);
			OpacityImp.TMP.set(op);
			OpacityImp.TMP.bind();
			col.bind();
			
			double dd = (TIME.currentSecond()*12);
			ms &= steI;
			
			
			
//			m |= DIR.W.mask();
			
			for (DIR d : DIR.ORTHO) {
				if ((d.mask() & flowMask) != 0) {
					TextureCoords tex = SPRITES.textures().dis_small.get(it.tx()*C.T_PIXELS+dd*-d.x(), it.ty()*C.T_PIXELS+dd*-d.y());
					stencil.renderTextured(tex, ms, it.x(), it.y());
					//CORE.renderer().renderSprite(it.x(), it.x()+C.TILE_SIZE, it.y(), it.y()+C.TILE_SIZE, tex);
				}
			}
			
		
			
			OPACITY.unbind();
			COLOR.unbind();
		}
		
		if (bb) {
			sbridge.render(r, it.ran()&7, it.x(), it.y());
		}
	}
	
	public void renderWater(SPRITE_RENDERER r, RenderIterator it, TILE_SHEET stencil, int tm, int op) {



		{
			GroundType g = SETT.GROUND().MAP.get(it.tile());
			ColorImp.TMP.set(g.col(it.tile()));
			ColorImp.TMP.shadeSelf(0.8);
			ColorImp.TMP.bind();
			stencil.renderTextured(SETT.GROUND().getTexture(it.tile(), it.ran()), tm, it.x(), it.y());
			COLOR.unbind();
		}

		OpacityImp.TMP.set(op);
		OpacityImp.TMP.bind();
		col.bind();
		
		double dd = (TIME.currentSecond()*12);
	
		for (DIR d : DIR.ORTHO) {
			TextureCoords tex = SPRITES.textures().dis_small.get(it.tx()*C.T_PIXELS+dd*-d.x(), it.ty()*C.T_PIXELS+dd*-d.y());
			stencil.renderTextured(tex, tm, it.x(), it.y());
		}
		
	
		
		OPACITY.unbind();
		COLOR.unbind();
	}
	
	public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, int edgeMask, boolean bridge) {
		
		boolean bb = false;
		for (DIR d : DIR.ORTHO) {
			if (willConnectable.is(it.tx(), it.ty(), d)) {
				edgeMask |= d.mask();
				
			}
			else if (bridge && !bb && !SETT.PATH().solidity.is(it.tx(), it.ty(), d) && SETT.FLOOR().getter.is(it.tx(), it.ty(), d)) {
				d = d.perpendicular();
				if (!SETT.PATH().solidity.is(it.tx(), it.ty(), d) && SETT.FLOOR().getter.is(it.tx(), it.ty(), d)) {
					bb = true;
				}
			}
		}
		edgeMask = edgeMask+16*(it.ran()&3);
		
		s.setHeight(2).setDistance2Ground(0);
		edge.render(s, edgeMask, it.x(), it.y());
		
		if (bb) {
			sbridge.render(s, it.ran()&7, it.x(), it.y());
		}
		
	


	}
	

	public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int tx, int ty) {
		int data = 0;
		for (DIR d : DIR.ORTHO) {
			if (willConnectable.is(tx, ty, d))
				data |= d.mask();
		}
		SPRITES.cons().BIG.outline.render(r, data, x, y);
	}
	
	public MAP_BOOLEAN willConnectable = new MAP_BOOLEAN() {
		
		@Override
		public boolean is(int tx, int ty) {
			RoomBlueprintImp b = SETT.ROOMS().map.blueprintImp.get(tx, ty);
			if (b == w.canal || b == w.drain)
				return true;
			else if (b == w.pump) {
				return w.pump.isCanalConnection(tx, ty);
			}
			return false;
		}
		
		@Override
		public boolean is(int tile) {
			throw new RuntimeException();
		}
	};
	
	public static class RSprite implements RoomSprite {

		private final RoomBlueprintImp blue;
		private final RoomPumpable pump;
		private boolean bridge;
		RSprite(RoomBlueprintImp blue, RoomPumpable pump, boolean bridge){
			this.blue = blue;
			this.pump = pump;
			this.bridge = bridge;
		}
		
		@Override
		public int sData() {
			return 0;
		}
		
		@Override
		public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
				FurnisherItem item) {
			SETT.ROOMS().WATER.sprite.renderPlaceholder(r, x, y, tx, ty);
		}
		
		@Override
		public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
			
			data = 0;
			for (DIR d : DIR.ORTHO) {
				if (SETT.ROOMS().WATER.sprite.willConnectable.is(it.tx(), it.ty(), d))
					data |= d.mask();
			}
			SETT.ROOMS().WATER.sprite.renderBelow(r, s, it, data, bridge);
			
		}
		
		@Override
		public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
				boolean isCandle) {
			int edge = 0;
			for (DIR d : DIR.ORTHO) {
				if (SETT.ROOMS().WATER.sprite.willConnectable.is(it.tx(), it.ty(), d))
					data |= d.mask();
			}
			int flow = blue.is(it.tile()) ? pump.dirmask(it.tx(), it.ty()) : 0;
			
			SETT.ROOMS().WATER.sprite.render(r, s, it, edge, flow, bridge);
			
			return false;
		}
		
		@Override
		public byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
			return 0;
		}
	}
	
	
}
