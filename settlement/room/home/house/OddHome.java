package settlement.room.home.house;

import java.io.IOException;

import init.type.HGROUP;
import init.type.HGROUP.HTypeBits;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.sets.QueueInteger;

public final class OddHome {

	private final QueueInteger[] queues = new QueueInteger[HGROUP.all().size()];
	
	OddHome(){
		for (int y = 0; y < queues.length; y++) {
			queues[y] = new QueueInteger(256);
		}
	}
	
	final SAVABLE saver = new SAVABLE() {
		
		@Override
		public void save(FilePutter file) {
			for (QueueInteger q : queues)
				q.save(file);
		}
		
		@Override
		public void load(FileGetter file) throws IOException {
			for (QueueInteger q : queues) {
				q.load(file);
			}
		}
		
		@Override
		public void clear() {
			for (QueueInteger q : queues)
				q.clear();
		}
	};
	
	void update(int tx, int ty) {
		
		HomeInstance h = test(tx, ty, this);
		if (h == null)
			return;
		HTypeBits s = h.availability();
		for (int ti = 0; ti < HGROUP.all().size(); ti++) {
			if (s.is(ti)) {
				HGROUP t = HGROUP.all().get(ti);
				QueueInteger i = queues[t.index()];
				if (!i.hasRoom())
					i.poll();
				i.push(tx+ty*SETT.TWIDTH);
			}
		}
	}

	
	public HomeInstance get(Humanoid h, Object user) {
		HGROUP t = HGROUP.get(h);
		while(queues[ t.index()].hasNext()) {
			int i = queues[t.index()].peek();
			int tx = i%SETT.TWIDTH;
			int ty = i/SETT.TWIDTH;
			
			HomeInstance ho = test(tx, ty, user);
			if (ho != null && ho.availability() != null && ho.availability().is(h)) {
				return ho;
			}else {
				queues[t.index()].poll();
			}
		}
		return null;
	}
	
	public boolean has(Humanoid h) {
		HGROUP t = HGROUP.get(h);
		while(queues[t.index()].hasNext()) {
			int i = queues[t.index()].peek();
			int tx = i%SETT.TWIDTH;
			int ty = i/SETT.TWIDTH;
			
			HomeInstance ho = test(tx, ty, this);
			if (ho != null) {
				return true;
			}
			queues[t.index()].poll();
		}
		return false;
	}
	
	private HomeInstance test(int tx, int ty, Object user) {
		
		if (!SETT.PATH().connectivity.is(tx, ty))
			return null;
		
		HomeInstance h = SETT.ROOMS().HOME.getter.get(tx, ty);
		if (h != null && h.serviceX() == tx && h.serviceY() == ty &&  h.occupants() < h.occupantsMax()) {
			return h;
		}

		return null;
	}
	
}
