package settlement.room.infra.importt;

import java.io.IOException;

import init.resources.RESOURCE;
import init.resources.RESOURCES;
import snake2d.LOG;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import util.data.INT_O;
import util.keymap.RMapInt;

public final class ImportTally {

	
	
	private final RMapInt<RESOURCE> pAmount = new RMapInt<RESOURCE>(RESOURCES.map());
	private final RMapInt<RESOURCE> pCapacity = new RMapInt<RESOURCE>(RESOURCES.map());
	public final INT_O<RESOURCE> amount = pAmount;
	public final INT_O<RESOURCE> capacity = pCapacity;
	
	public void debug(RESOURCE res) {
		LOG.ln(res.name);
		LOG.ln("am " + pAmount.get(res));
		LOG.ln("ca " + capacity.get(res));
		LOG.ln(spaceForTribute(res));
		LOG.ln();
	}
	
	ImportTally() {

	}
	
	final SAVABLE saver = new SAVABLE() {
		@Override
		public void save(FilePutter file) {
			
		}

		@Override
		public void load(FileGetter file) throws IOException {
			pAmount.clear();
			pCapacity.clear();
			
		}

		@Override
		public void clear() {
			pAmount.clear();
			pCapacity.clear();
		}
	};
	
	void count(RESOURCE r, int amount, int capacity) {

		if (r != null) {
			this.pAmount.inc(r, amount);
			this.pCapacity.inc(r, capacity);
		}
	}



	public int spaceForTribute(RESOURCE res) {
		int am = capacity.get(res)-(amount.get(res));
		if (am < 0) {
			return 0;
		}
		return am;
	}
	

	
	


	

}
