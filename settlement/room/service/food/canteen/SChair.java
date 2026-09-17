package settlement.room.service.food.canteen;

import init.constant.C;
import init.resources.RESOURCE;
import settlement.main.SETT;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

class SChair {

	public final static int I = 3;
	
	private final ROOM_CANTEEN b;
	private final Coo res = new Coo();
	
	SChair(ROOM_CANTEEN b){
		this.b = b;
	}
	
	public COORDINATE get(int sx, int sy) {
		
		CanteenInstance ins = b.getter.get(sx, sy);
		if (ins == null)
			return null;
		
		
		int tx = ins.tableX;
		int ty = ins.tableY;

		if (tx == -1)
			return null;
		
		int a = ins.body().width()*ins.body().height();
		
		for (int i = 0; i < a; i++) {
			
			if (ins.is(tx, ty)) {
				
				
				if (SETT.ROOMS().fData.tileData.get(tx, ty) == I) {
					if (SETT.ROOMS().data.get(tx, ty) == 0) {
						ins.tableX = (short) tx;
						ins.tableY = (short) ty;
						return ret(ins, tx, ty);
					}
				}
			}
			tx++;
			if (tx >= ins.body().x2()) {
				tx = ins.body().x1();
				ty++;
				if (ty >= ins.body().y2()) {
					ty = ins.body().y1();
				}
			}
		}
		
		ins.tableX = -1;
		ins.tableY = -1;
		return null;
	}
	
	COORDINATE ret(CanteenInstance ins, int x, int y) {
		ins.tableX = (short)x;
		ins.tableY = (short)y;
		SETT.ROOMS().data.set(ins, x, y, 1);
		res.set(x,y);
		return res;
	}

	public DIR set( int tx, int ty, int mealData) {
		CanteenInstance ins = b.getter.get(tx, ty);
		if (ins == null)
			return null;
		if (SETT.ROOMS().fData.tileData.get(tx, ty) != I) {
			return null;
		}
		
		COORDINATE u = SETT.ROOMS().fData.itemX1Y1(tx, ty, Coo.TMP);
		int ux = u.x();
		int uy = u.y();
		
		for (int di = 0; di < DIR.ORTHO.size(); di++) {
			DIR d = DIR.ORTHO.get(di);
			if (ins.is(tx, ty, d) && SETT.ROOMS().fData.tileData.get(tx, ty, d) != I && SETT.ROOMS().fData.tile.get(tx, ty, d) != null) {
				u = SETT.ROOMS().fData.itemX1Y1(tx, ty, d, Coo.TMP);
				if (u != null && u.isSameAs(ux, uy)) {
					SETT.ROOMS().data.set(ins, tx, ty, d, mealData);
					return d;
				}
			}
		}
		
		return null;
	}
	
	void returnTable(int tx, int ty) {
		CanteenInstance ins = b.getter.get(tx, ty);
		if (ins == null)
			return;
		if (SETT.ROOMS().fData.tileData.get(tx, ty) != I) {
			return;
		}
		SETT.ROOMS().data.set(ins, tx, ty, 0);
		COORDINATE u = SETT.ROOMS().fData.itemX1Y1(tx, ty, Coo.TMP);
		int ux = u.x();
		int uy = u.y();
		
		for (int di = 0; di < DIR.ORTHO.size(); di++) {
			DIR d = DIR.ORTHO.get(di);
			if (SETT.ROOMS().fData.tileData.get(tx, ty, d) != I && SETT.ROOMS().fData.tile.get(tx, ty, d) != null) {
				u = SETT.ROOMS().fData.itemX1Y1(tx, ty, d, Coo.TMP);
				if (u != null && u.isSameAs(ux, uy)) {
					SETT.ROOMS().data.set(ins, tx, ty, d, 0);
					if (ins.tableX == -1) {
						ins.tableX = (short) ins.body().x1();
						ins.tableY = (short) ins.body().y1();
					}
					return;
				}
			}
		}
	}
	
	void render(SPRITE_RENDERER r, ShadowBatch s, int rotMask, RenderIterator it, int am, RESOURCE res) {

		rotMask &= 0b01111;
		
		DIR d = DIR.N;
		for (int i = 0; i < DIR.ORTHO.size(); i++) {
			if ((rotMask | d.mask()) == 0b1111)
				break;
			d = d.next(2);
		}
		
		int x1 = C.TILE_SIZEH/2*d.x();
		int y1 = C.TILE_SIZEH/2*d.y();
		
		
		int dd = 0;
		
		while(am-- > 0) {
			int ddd = dd/3;
			int dddd = dd%3;
			int x = -d.x()*ddd + d.y()*(-1 + dddd);
			int y = -d.y()*ddd + d.x()*(-1+dddd);
			am--;
			dd++;
			it.setOff(x1+x*C.TILE_SIZEH/2, y1+y*C.TILE_SIZEH/2);
			b.constructor.renderDish(r, s, res, it, it.ran());
		}
		
	}
	
}
