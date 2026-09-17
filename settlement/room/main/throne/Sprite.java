package settlement.room.main.throne;

import java.io.IOException;

import game.GAME;
import init.constant.C;
import init.sprite.UI.Icon;
import init.sprite.UI.UI;
import settlement.main.SETT;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite;
import settlement.room.sprite.RoomSprite1x1;
import snake2d.Renderer;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.file.Json;
import snake2d.util.sprite.TILE_SHEET;
import util.rendering.RenderData;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import util.spritecomposer.ComposerDests;
import util.spritecomposer.ComposerSources;
import util.spritecomposer.ComposerThings;
import util.spritecomposer.ComposerUtil;

final class Sprite {
	
	final COLOR miniC;
	public final Icon icon;
	private final TILE_SHEET sfloor;
	private final TILE_SHEET sthrone;
	final static int WIDTH = 5;
	final static int HEIGHT = 3;
	private final RoomSprite candle;
	private int shadow = 5*4;
	
	
	Sprite(RoomInitData init) throws IOException{
		miniC = new ColorImp(init.data(), "MINI_COLOR");
		icon = UI.icons().get(init.data());
		
		Json sp = init.data().json("SPRITES");
		
		candle = new RoomSprite1x1(sp, "TORCH_1X1");
		
		sfloor = new ComposerThings.ITileSheet(init.sp(), 264, 240) {
			
			@Override
			protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
				s.full.init(0, 0, 1, 4, 3, 3, d.s16);
				s.full.paste(true);
				s.full.setVar(1).paste(true);
				s.full.setVar(2).paste(true);
				s.full.setVar(3).paste(true);
				return d.s16.saveGame();
			}
		}.get();
		
		sthrone = new ComposerThings.ITileSheet() {
			
			@Override
			protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d) {
				s.singles.init(s.full.body().x2(), s.full.body().y1(), 1, 5, 1, 1, d.s24);
				s.singles.setVar(0).paste(3, true);
				s.singles.setVar(1).paste(3, true);
				s.singles.setVar(2).paste(3, true);
				s.singles.setVar(3).paste(3, true);
				s.singles.setVar(4).paste(3, true);
				s.singles.init(s.singles.body().x2(), s.singles.body().y1(), 1, 5, 1, 1, d.s24);
				s.singles.setVar(0).paste(3, true);
				s.singles.setVar(1).paste(3, true);
				s.singles.setVar(2).paste(3, true);
				s.singles.setVar(3).paste(3, true);
				s.singles.setVar(4).paste(3, true);
				
				return d.s24.saveGame();
			}
		}.get();
		
	}
	
	static int width(int rot) {
		return (rot & 1) == 0 ? Sprite.WIDTH : Sprite.HEIGHT;
	}
	
	static int height(int rot) {
		return (rot & 1) == 1 ? Sprite.WIDTH : Sprite.HEIGHT;
	}
	
	void renderFloor(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		double t = GAME.player().level().current().index();
		t /= GAME.player().level().all().size();
		t *= 3;
		int d = SETT.ROOMS().data.get(it.tile()) & 0x0F;
		d += (int)t*9;
		sfloor.render(r, d, it.x(), it.y());
	}
	
	void renderThrone(Renderer r, ShadowBatch shadowBatch, RenderIterator it, int rot) {
		
		double t = GAME.player().level().current().index();
		t /= GAME.player().level().all().size();
		t *= 5;
		
		int x = it.x()-4*C.SCALE;
		int y = it.y()-4*C.SCALE;
		int tile = rot;
		tile += (int)t*4;
		
		sthrone.render(r, tile, x, y);
		shadowBatch.setDistance2Ground(0).setHeight(12);
		sthrone.render(shadowBatch, shadow+tile, x, y);
		
	}
	
	void renderTorch(Renderer r, ShadowBatch shadowBatch, RenderIterator it, int rot) {
		candle.render(r, shadowBatch, 0, it, 0, true);
	}
}