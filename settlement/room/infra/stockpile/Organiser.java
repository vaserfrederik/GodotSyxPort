package settlement.room.infra.stockpile;

import init.resources.RESOURCES;
import settlement.misc.util.TILE_STORAGE;
import settlement.room.infra.logistics.MoveJob;
import settlement.room.main.job.StorageCrate;
import snake2d.util.file.Alloc;
import snake2d.util.sets.Bitmap1D;

final class Organiser {

	private Bitmap1D check = new Bitmap1D(RESOURCES.ALL().size(), false);
	private int[] amounts = Alloc.ii(RESOURCES.ALL().size());
	private int[] xs = Alloc.ii(RESOURCES.ALL().size());
	private int[] ys = Alloc.ii(RESOURCES.ALL().size());

	
	Organiser(ROOM_STOCKPILE b){
		
	}
	
	public MoveJob organise(StockpileInstance ins, int am) {
		check.clear();
		
		for (int i = 0; i < ins.crates.size(); i++) {
			ins.crates.set(i);
			TILE_STORAGE s = ins.crate(ins.crates.get().x(), ins.crates.get().y());
			if (s != null && s.resource() != null && s.storageReservable() > 0) {
				int ri = s.resource().index();
				if (!check.get(ri) || s.storageReservable() < amounts[ri]) {
					check.set(ri, true);
					amounts[ri] = s.storageReservable();
					xs[ri] = s.x();
					ys[ri] = s.y();
				}
			}
		}
		
		for (int i = 0; i < ins.crates.size(); i++) {
			ins.crates.set(i);
			StorageCrate s = ins.crate(ins.crates.get().x(), ins.crates.get().y());
			if (s != null && s.resource() != null && check.get(s.resource().index()) && s.reservable() > 0) {
				int ri = s.resource().index();
				if (s.x() == xs[ri] && s.y() == ys[ri])
					continue;
				if (s.storageReservable() < amounts[ri])
					continue;
				MoveJob j = MoveJob.TMP;
				am = Math.min(am, s.reservable());
				am = Math.min(am, amounts[ri]);
				j.maxAm = am;
				j.res = s.resource();
				j.stored = true;
				j.prio = ins.prioritizing();
				j.source.set(s);
				j.dest.set(xs[ri], ys[ri]);
				return j;
			}
		}
		
		return null;
	}
	
	
}
