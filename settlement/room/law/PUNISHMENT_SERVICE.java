package settlement.room.law;

import init.race.Race;
import util.data.BOOLEANO.BOOLEAN_OE;

public interface PUNISHMENT_SERVICE {

	public int punishTotal();
	public int punishUsed();

	public default BOOLEAN_OE<Race> punishEnabled(){
		return null;
	}
	
	
	
}
