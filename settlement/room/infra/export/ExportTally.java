package settlement.room.infra.export;

import java.io.IOException;

import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.main.SETT;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import util.data.INT_O;
import util.keymap.RMapInt;

public final class ExportTally {
	

	private final RMapInt<RESOURCE> pAmount = new RMapInt<RESOURCE>(RESOURCES.map());
	private final RMapInt<RESOURCE> pCapacity =new RMapInt<RESOURCE>(RESOURCES.map());
	public final INT_O<RESOURCE> amount = pAmount;
	public final INT_O<RESOURCE> capacity = pCapacity;
	
	ExportTally() {

	}
	
	final SAVABLE saver = new SAVABLE() {
		@Override
		public void save(FilePutter file) {
			
			pAmount.save(file);
			pCapacity.save(file);
			
		}

		@Override
		public void load(FileGetter file) throws IOException {
			
			pAmount.load(file);
			pCapacity.load(file);
			pAmount.clear();
			for (ExportInstance i : SETT.ROOMS().EXPORT.all()) {
				if (i.resource() != null)
					pAmount.inc(i.resource(), i.amount);
			}
			
		}

		@Override
		public void clear() {
			pAmount.clear();
			pCapacity.clear();
			
		}
	};
	
	void inc(RESOURCE r, int amount, int capacity) {
		this.pAmount.inc(r, amount);
		this.pCapacity.inc(r, capacity);
	}	

}
