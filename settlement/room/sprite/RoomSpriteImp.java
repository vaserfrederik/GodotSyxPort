package settlement.room.sprite;

import java.io.IOException;

import init.sprite.game.SheetData;
import init.sprite.game.SheetPair;
import init.sprite.game.SheetType;
import init.sprite.game.Sheets;
import settlement.main.SETT;
import settlement.room.main.Room;
import settlement.room.main.furnisher.FurnisherItem;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import snake2d.util.misc.CLAMP;
import util.rendering.RenderData.RenderIterator;

public abstract class RoomSpriteImp implements RoomSprite{

	protected final Sheets[] sheets;
	
	public final boolean rotates;
	private int sData = 0;
	protected double animationSpeed = 1.0;
	
	public RoomSpriteImp(SheetType type, Json json, String key) throws IOException{
		if (json.jsonsIs(key)) {
			Json[] js = json.jsons(key, 1);
			sheets = new Sheets[js.length];
			for (int i = 0; i < js.length; i++) {
				sheets[i] = new Sheets(type, js[i]);
			}
		}else {
			sheets = new Sheets[] {
				new Sheets(type, json.json(key))
			};
		}
		boolean rot = false;
		for (Sheets s : sheets) {
			for (SheetPair ss : s.sheets)
				rot |= ss.s.hasRotation & ss.d.rotates;
		}
		this.rotates = rot;
	}
	
	public RoomSpriteImp(RoomSprite others) throws IOException{
		
		RoomSpriteImp other = (RoomSpriteImp) others;
		if (other.type() != type())
			throw new RuntimeException();
		this.sheets = other.sheets;
		this.rotates = other.rotates;
		this.animationSpeed = other.animationSpeed;
	}
	
	public RoomSpriteImp(SheetType type) throws IOException{
		this.sheets = new Sheets[] {new Sheets(type.dummy(), SheetData.DUMMY)};
		this.rotates = false;
		this.animationSpeed = 1.0;
	}
	
	public Sheets sheet(RenderIterator it) {
		if (sheets.length == 1)
			return sheets[0];
		Room r = SETT.ROOMS().map.get(it.tx(), it.ty());
		if (r == null)
			return sheets[0];
		return sheets[CLAMP.i(r.upgrade(it.tx(), it.ty()), 0, sheets.length-1)];
	}
	
	public SheetPair sheetPair(RenderIterator it, int ran) {
		Sheets a = sheet(it);
		if (a == null)
			return null;
		return a.get(ran);
	}
	
	public SheetPair get(RenderIterator it, int random) {
		Sheets a = sheet(it);
		if (a == null)
			return null;
		return a.get(random);
	}
	
	public int frame(SheetPair a, RenderIterator it) {
		if (a == null)
			return 0;
		return a.d.frame(it.ran(), animationSpeed);
	}

	protected abstract boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item);
	
	@Override
	public void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry,
			FurnisherItem item) {

	}
	
	@Override
	public int sData() {
		return sData;
	}
	
	public RoomSpriteImp sData(int d) {
		sData = d;
		return this;
	}
	
	public abstract SheetType type();

	protected int getData2(RenderIterator it) {
		return SETT.ROOMS().fData.spriteData2.get(it.tile());
	}
	
	public void animate(double speed) {
		this.animationSpeed = speed;
	}
	
}
