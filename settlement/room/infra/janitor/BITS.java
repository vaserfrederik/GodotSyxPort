package settlement.room.infra.janitor;

import java.io.Serializable;

import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.main.SETT;
import settlement.room.main.RoomInstance;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.Bitsmap1D;
import util.gui.misc.GBox;

final class BITS implements Serializable {

	/**
	 * 
	 */
	private static final long serialVersionUID = 2L;
	private Bitsmap1D resAms = new Bitsmap1D(0, 5, RESOURCES.ALL().size());
	private final RBITImp bitsAvailable = new RBITImp();
	private final RBITImp bitsHaveEnough = new RBITImp();
	private final RBITImp bitsHaveAny = new RBITImp();
	private final RBITImp bitsTimedOut = new RBITImp();
	private final RBITImp bitsIsFetching = new RBITImp();
	private final RBITImp bitsMissing = new RBITImp();
	Bitsmap1D fetchAms = new Bitsmap1D(0, 5, RESOURCES.ALL().size());

	public int resAm(RESOURCE res) {
		return resAms.get(res.index());
	}

	public RBITImp resHave() {
		return bitsAvailable;
	}


	public void resSetMissing(RBIT resourceMask) {
		bitsMissing.or(resourceMask);
		bitsTimedOut.or(resourceMask);
	}

	public boolean resMissing(RESOURCE res) {
		return bitsMissing.has(res) && !bitsIsFetching.has(res);
	}

	public void resInc(JanitorInstance ins, RESOURCE res, int am) {
		am = CLAMP.i(resAms.get(res.index())+am, 0, resAms.maxValue());
		resAms.set(res.index(), am);

		if (resAms.get(res.index()) > 0) {
			bitsAvailable.or(res);
			bitsMissing.clear(res);
			bitsTimedOut.clear(res);
		} else {
			bitsAvailable.clear(res);
		}
		
		double ma = maxAm(ins, res);
		
		bitsHaveEnough.set(res, resAms.get(res.index()) > ma);
		bitsHaveAny.set(res, resAms.get(res.index()) > 0 && resAms.get(res.index()) + fetchAms.get(res.index())*4 > ma/2);
		bitsIsFetching.set(res, resAms.get(res.index()) + fetchAms.get(res.index())*4 >= ma);
	}

	public int maxAm(JanitorInstance ins, RESOURCE res) {
		int max = ins.employees().employed();
		max = (int) (max*SETT.MAINTENANCE().estimateGlobal(res));
		return CLAMP.i(max, 4, resAms.maxValue()-3);
	}
	
	private static final RBITImp tmp = new RBITImp();

	public RBITImp resMaskFetcher(RoomInstance ins) {
		tmp.clearSet(SETT.PATH().finders.maintenance.mask(ins.mX(), ins.mY()));
		tmp.xor(bitsIsFetching);
		tmp.xor(bitsHaveEnough);
		tmp.xor(bitsTimedOut);
		return tmp;
	}
	
	public RBITImp resMaskFetcherMust(RoomInstance ins) {
		tmp.clearSet(SETT.PATH().finders.maintenance.mask(ins.mX(), ins.mY()));
		tmp.xor(bitsIsFetching);
		tmp.xor(bitsHaveEnough);
		tmp.xor(bitsTimedOut);
		tmp.xor(bitsHaveAny);
		return tmp;
	}
	
	public RBITImp resMaskWorker(RoomInstance ins) {
		tmp.clearSet(SETT.PATH().finders.maintenance.mask(ins.mX(), ins.mY()));
		tmp.xor(bitsIsFetching);
		tmp.xor(bitsHaveAny);
		tmp.xor(bitsTimedOut);
		return tmp;
	}

	void update() {
		bitsTimedOut.clear();
	}

	public boolean resReserved(RESOURCE res) {
		return fetchAms.get(res.index()) > 0;
	}

	public void resReserve(JanitorInstance ins, RESOURCE res, boolean yes) {
		if (yes) {
			fetchAms.inc(res.index(), 1);
		}else
			fetchAms.inc(res.index(), -1);
//		LOG.ln(fetchAms.get(res.index()) + " " + (fetchAms.get(res.index())*4 >= maxAm(ins, res)));
		bitsIsFetching.set(res, resAms.get(res.index()) + fetchAms.get(res.index())*4 >= maxAm(ins, res));
	}

	void hover(GBox b, RESOURCE res, RoomInstance ins) {
	
		b.text("avai");
		b.add(b.text().add(bitsAvailable.has(res)));
		b.NL();
		b.text("max");
		b.add(b.text().add(maxAm((JanitorInstance) ins, res)));
		b.NL();
		
		b.text("enough");
		b.add(b.text().add(bitsHaveEnough.has(res)));
		b.NL();
		b.text("timed");
		b.add(b.text().add(bitsTimedOut.has(res)));
		b.NL();
		b.text("fetchReserved");
		b.add(b.text().add(fetchAms.get(res.index())));
		b.NL();
		b.text("isFetching");
		b.add(b.text().add(bitsIsFetching.has(res)));
		b.NL();
		b.text("missing");
		b.add(b.text().add(bitsMissing.has(res)));
		b.NL();
		b.text("globalHas");
		b.add(b.text().add(SETT.PATH().finders.maintenance.mask(ins.mX(), ins.mY()).has(res)));
		b.NL();
		b.text("fetch");
		b.add(b.text().add(resMaskFetcher(ins).has(res)));
		b.NL();
		b.text("fetch work");
		b.add(b.text().add(resMaskWorker(ins).has(res)));
		b.NL();
	}

}