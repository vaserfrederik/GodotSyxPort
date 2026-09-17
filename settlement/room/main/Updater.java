package settlement.room.main;

import java.io.IOException;

import game.time.TIME;
import settlement.main.SETT;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.sets.LIST;

class Updater implements SAVABLE{

	private int dayCurrent = TIME.days().bitsSinceStart();
	private boolean day;
	private final double UPDATE_INTERVAL = 64;
	private final double UPDATE_INTERVALI = 1.0/UPDATE_INTERVAL;
	private double acc = 0;
	private int ii = 0;
	
	Updater(LIST<RoomBlueprintIns<?>> all){

	}
	
	public void update(double ds) {
		
		final int max = SETT.ROOMS().map.max();
		double n = acc += ds*max*UPDATE_INTERVALI;
		int am = (int) n;
		acc = n-am;
		
		
		
		for (int i = 0; i < am; i++) {
			
			if (ii >= max) {
				day = false;
				if (dayCurrent != TIME.days().bitsSinceStart())
					day = true;
				dayCurrent = TIME.days().bitsSinceStart();
				ii = 0;
				return;
			}
			
			Room a = SETT.ROOMS().map.getByIndex(ii);
			if (a != null && a instanceof RoomInstance) {
				RoomInstance ins = (RoomInstance) a;
				ins.update(UPDATE_INTERVAL, day);
			}
			
			ii++;

		}
		
		
	}
	
	@Override
	public void save(FilePutter file) {
		file.bool(day);
		file.i(dayCurrent);
		file.i(ii);
		file.d(acc);
		
		
	}

	@Override
	public void load(FileGetter file) throws IOException {
		day = file.bool();
		dayCurrent = file.i();
		ii = file.i();
		acc = file.d();
		
	}

	@Override
	public void clear() {
		dayCurrent = TIME.days().bitsSinceStart();
		day = false;
		ii = 0;
		acc = 0;
		
	}



}
