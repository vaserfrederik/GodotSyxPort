package settlement.room.home;

import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;

public interface HOME {

	/**
	 * Only called by the stat
	 * @param h
	 */
	public HOME vacate(Humanoid h);
	
	/**
	 * Only called by the stat
	 * @param h
	 */
	public HOME occupy(Humanoid h);
	
	public Humanoid occupant(int oi);
	
	public int occupants();
	public int occupantsMax();
	public int serviceX();
	public int serviceY();
	public int resourceAm(int ri);
	public double isolation();
	public boolean canOccupy(Humanoid h);
	
	public CharSequence typeName(int tx, int ty);
	
	public static HOME get(int tx, int ty) {
		HOME h = SETT.ROOMS().HOME.getter.get(tx,ty);
		if (h != null)
			return h;
		return SETT.ROOMS().CHAMBER.get(tx, ty);
	}
	
	public boolean is(int tx, int ty);

	public int area();


}
