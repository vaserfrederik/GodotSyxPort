package settlement.room.main.throne;

import static settlement.main.SETT.FLOOR;
import static settlement.main.SETT.ROOMS;
import static settlement.main.SETT.TERRAIN;
import static settlement.main.SETT.TWIDTH;

import init.sprite.UI.Icon;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.TmpArea;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.datatypes.RECTANGLE;
import snake2d.util.datatypes.Rec;
import util.rendering.RenderData;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

final class Instance extends Room.RoomInstanceImp {

	private static final long serialVersionUID = 1L;
	final int size = Sprite.width(0)*Sprite.height(0);
	private final RECTANGLE body;
	final int rot;
	
	Instance(int x1, int y1, int rot){
		super(SETT.ROOMS(), SETT.ROOMS().THRONE, false);
		
		if (SETT.ROOMS().map.get(THRONE.coo()) instanceof Instance) {
			((Instance)SETT.ROOMS().map.get(THRONE.coo())).remove();
		}
		
		body = new Rec().moveX1Y1(x1, y1).setDim(Sprite.width(rot), Sprite.height(rot));
		
		blueprintI().setInstance(body.cX(), body.cY());
		this.rot = rot;
		
		
		
		for (COORDINATE c : body) {
			setIndex(c.x(), c.y());
			SETT.ROOMS().data.set(this, c, 0);
		}
		SETT.ROOMS().map.init(this);
		
		
		
		DIR td = DIR.ORTHO.getC(rot).perpendicular();
		
		
		
		for (COORDINATE c : body) {
			
			int tx = c.x();
			int ty = c.y();
			SETT.GRASS().current.set(c, 0);
			if (!TERRAIN().get(tx, ty).roofIs())
				TERRAIN().NADA.placeFixed(tx, ty);
			
			int d = 0;
			for (DIR dir : DIR.ORTHO) {
				if (body.holdsPoint(c, dir))
					d |= dir.mask();
			}
			d = DIR.toBoxID(d);
			ROOMS().data.set(this, tx, ty, d);
			
			if (!body.holdsPoint(c, td)) {
				if (!body.holdsPoint(c, td.next(-2)) || !body.holdsPoint(c, td.next(2)))
					candle(tx, ty);
			}
				
		}
		SETT.ROOMS().map.init(this);
	}
	
	@Override
	public boolean is(int tile) {
		return body.holdsPoint(tile%TWIDTH, tile/TWIDTH);
	}
	
	@Override
	public boolean is(int tx, int ty) {
		return body.holdsPoint(tx, ty);
	}

	@Override
	public int area() {
		return size;
	}

	@Override
	public RECTANGLE body() {
		return body;
	}

	@Override
	final public TmpArea remove(int tx, int ty, boolean scatter, Object obj, boolean force) {
		throw new RuntimeException();
	}
	
	void remove() {
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			SETT.LIGHTS().remove(c.x(), c.y());
			FLOOR().clearer.clear(c.x(), c.y());
		}
		TmpArea t = super.delete(body().x1(), body().y1(), this);
		SETT.ROOMS().map.init(t);
		t.clear();
	}
	
	private void candle(int tx, int ty) {
		SETT.LIGHTS().candle(tx, ty, 0);
		ROOMS().data.set(this, tx, ty, ROOMS().data.get(tx, ty) | 0x10);
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		if (it.tx() == body().cX() && it.ty() == body().cY()) {
			blueprintI().sprite.renderThrone(r, shadowBatch, it, rot);
		}else if ((ROOMS().data.get(it.tile()) & 0x010) != 0) {
			blueprintI().sprite.renderTorch(r, shadowBatch, it, rot);
		}
		return false;
	}
	
	@Override
	protected boolean renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		blueprintI().sprite.renderFloor(r, shadowBatch, i);
		return true;
	}
	
	public THRONE blueprintI() {
		return ROOMS().THRONE;
	}
	
	@Override
	protected AVAILABILITY getAvailability(int tile) {
		return AVAILABILITY.ROOM;
	}
	
	@Override
	public int mX() {
		return body.x1();
	}

	@Override
	public int mY() {
		return body.y1();
	}

	@Override
	public ROOM_DEGRADER degrader(int tx, int ty) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public void destroyTile(int tx, int ty) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public boolean destroyTileCan(int tx, int ty) {
		return false;
	}

	@Override
	public CharSequence name(int tx, int ty) {
		return blueprintI().info.name;
	}
	
	@Override
	public Icon icon() {
		return blueprintI().sprite.icon;
	}

	@Override
	public int resAmount(int ri, int upgrade) {
		// TODO Auto-generated method stub
		return 0;
	}


}