package settlement.room.infra.station;

import init.resources.RESOURCE;
import settlement.main.SETT;
import settlement.room.infra.transport.ROOM_TRANSPORT;

public class StationTally {

	private byte crates = 0;
	private int stored = 0;
	private int reserved = 0;
	
	StationTally(){
		
	}
	
	void remove(RESOURCE res, Crate crate, StationInstance ins) {
		SETT.ROOMS().STATION.tally(crate.resource()).remove(this, ins);
		crates --;
		stored -= crate.stored.get();
		reserved -= crate.reserved.get();
		ins.bamount.set(res, stored-reserved >= 0);
		ins.bcapacity.set(res, crates >= 0);

	}
	
	void add(RESOURCE res, Crate crate, StationInstance ins) {
		
		crates ++;
		stored += crate.stored.get();
		reserved += crate.reserved.get();
		ins.bamount.set(res, stored-reserved >= 0);
		ins.bcapacity.set(res, crates >= 0);
		SETT.ROOMS().STATION.tally(crate.resource()).add(this, ins);
	}
	
	public int stored() {
		return stored;
	}
	
	public int reserved() {
		return reserved;
	}
	
	public int crates() {
		return crates;
	}
	
	public int space() {
		return crates*SETT.ROOMS().STATION.crate.MAX_AM;
	}
	
	public int spaceAvailable() {
		return crates*SETT.ROOMS().STATION.crate.MAX_AM-stored;
	}
	
	void clear() {
		crates = 0;
		stored = 0;
		reserved = 0;
	}
	
	public static class Total {
		
		private byte crates = 0;
		private int stored = 0;
		private int reserved = 0;
		private int incoming = 0;
		private int available = 0;
		public final RESOURCE res;
		
		Total(RESOURCE res){
			this.res = res;
		}
		
		void remove(StationTally crate, StationInstance ins) {
			if (ins.accepting(res))
				available --;
			crates -= crate.crates;
			stored -= crate.stored;
			reserved -= crate.reserved;
			incoming -= ins.incoming(res);
			
		}
		
		void add(StationTally crate, StationInstance ins) {
			
			crates += crate.crates;
			stored += crate.stored;
			reserved += crate.reserved;
			incoming += ins.incoming(res);
			if (ins.accepting(res))
				available ++;
			
				
		}
		
		void debug() {
			int am = 0;
			for (int i = 0; i < SETT.ROOMS().STATION.instancesSize(); i++) {
				StationInstance ins = SETT.ROOMS().STATION.getInstance(i);
				if (ins.tally == null)
					return;
				if (ins.accepting(res))
					am++;
			}
			if (am != available)
				throw new RuntimeException(res + " " + am + " " + available);
		}
		
		public int stored() {
			return stored;
		}
		
		public int reserved() {
			return reserved;
		}
		
		public int incoming() {
			return incoming;
		}
		
		public int crates() {
			return crates;
		}
		
		public int space() {
			return crates*ROOM_TRANSPORT.MAX_LOAD;
		}
		
		public int accepting() {
			return available;
		}
		
		void clear() {
			crates = 0;
			stored = 0;
			reserved = 0;
			available = 0;
			incoming = 0;
		}
		
	}
	
}
