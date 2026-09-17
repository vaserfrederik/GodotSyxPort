package settlement.room.infra.importt;

import game.GAME;
import game.faction.FACTIONS;
import init.resources.RESOURCE;
import init.trade.TRADE_TYPE;
import settlement.room.main.job.StorageCrate;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;
import snake2d.util.rnd.RND;
import util.GUTIL;

public final class ImportThingy {

	private final ROOM_IMPORT b;
	private final ImportTally tally;
	
	
	ImportThingy(ROOM_IMPORT imp, ImportTally tally){
		this.b = imp;
		this.tally = tally;
	}
	
	public COORDINATE getReservableSpot(int sx, int sy, RESOURCE res) {

		if (tally.capacity.get(res)-(tally.amount.get(res)) < 0) {
			return null;
		}
			
		
		ImportInstance ins = b.get(sx, sy);
		if (ins == null || ins.resource() != res || reservable(ins) <= 0) {
			ins = null;
			if (b.all().size() == 0)
				return null;
			int r = RND.rInt(b.all().size());
			for (int i = 0; i < b.all().size(); i++) {
				ImportInstance ins2 = b.all().get((i+r)%b.all().size());
				if (ins2.resource() == res && reservable(ins2) > 0) {
					ins = ins2;
					break;
				}
			}
		}
		
		if (ins != null) {
			return getReservableSpot(ins, sx, sy, res);
		}
		return null;
	}
	
	private COORDINATE getReservableSpot(ImportInstance i, int sx, int sy, RESOURCE res) {
		if (!i.is(sx, sy)) {
			sx = i.mX();
			sy = i.mY();
		}
		GUTIL.filler().init(this);
		GUTIL.filler().filler.set(sx, sy);
		DIR dir = DIR.ORTHO.rnd();
		int q = 0;
		while(GUTIL.filler().hasMore()) {
			COORDINATE c = GUTIL.filler().poll();
			if (reservable(res, c) > 0) {
				GUTIL.filler().done();
				return c;
			}
			q++;
			DIR d = dir;
			for (int k = 0; k < DIR.ORTHO.size(); k++) {
				if (i.is(c, d))
					GUTIL.filler().fill(c, d);
				d = d.next(2);
			}
			
		}
		GUTIL.filler().done();
		GAME.Notify("oh no " + res + " " + i.resource() + " " + q + " " + i.area() + " " + i.mX() + " " + i.mY() + " " + reservable(i));
		return null;
	}
	
	private int reservable(ImportInstance ins) {
		return ins.capacity()-ins.amount()-ins.spaceReserved();
	}
	
	private StorageCrate get(RESOURCE res, COORDINATE c) {
		ImportInstance ins = b.get(c.x(), c.y());
		if (ins == null)
			return null;
		
		if (ins.resource() != res)
			return null;
		
		return b.crate.get(c.x(), c.y(), ins, ins.sdata);
	}
	
	public int reservable(RESOURCE r, COORDINATE c) {
		StorageCrate cr = get(r, c);
		if (cr == null)
			return 0;
		return cr.storageReservable();
		
	}

	public void reserve(RESOURCE r, COORDINATE c, int amount) {
		StorageCrate cr = get(r, c);
		if (cr == null)
			throw new RuntimeException();
		cr.storageReserve(amount);;
	}
	
	public int reserved(RESOURCE r, COORDINATE c) {
		StorageCrate cr = get(r, c);
		if (cr == null)
			return 0;
		return cr.storageReserved();
	}
	
	public void finish(RESOURCE r, COORDINATE c, int amount, TRADE_TYPE type) {
		StorageCrate cr = get(r, c);
		if (cr == null)
			throw new RuntimeException();
		cr.storageDeposit(amount);
		FACTIONS.player().res().inc(r, type.rtype, amount);
	}
	
	

	
}
