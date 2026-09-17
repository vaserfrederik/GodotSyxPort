package settlement.room.main.construction;

import settlement.main.SETT;
import settlement.room.main.MapRoomData;
import settlement.room.main.ROOMA;
import snake2d.util.bit.Bits;

public class ConstructionData {

	public static final Map dFloored = 
			new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0000_0001));
	public static final Map dConstructed = 
			new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0000_0010));
	public static final Map dBroken = 
			new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0000_0100));
	public static final Map dExpensive = 
			new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0000_1000));
	public static final Map dData = new Map(new Bits(0b01111));
	
	public static final Map dBlocked = 
			new Map(new Bits(0b0000_0000_0000_0000_0001_0000_0000_0000));
	
	public static final Map dWall = 
			new Map(new Bits(0b0000_0000_0000_0000_0000_1111_1111_0000));
	
	
	static final Map dMarked = 
			new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0001_0000));
	static final Map dError = 
			new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0010_0000));
	static final Map dTmpInstance = 
			new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0010_0000));
	static final Map[] dResourceNeeded = new Map[] {
			new Map(new Bits(0b0000_0000_0000_0000_0000_0000_1111_0000)),
			new Map(new Bits(0b0000_0000_0000_0000_0000_1111_0000_0000)),
			new Map(new Bits(0b0000_0000_0000_0000_1111_0000_0000_0000)),
			new Map(new Bits(0b0000_0000_0000_1111_0000_0000_0000_0000)),
	};
	static final Map dResourceNeededAll = 
			new Map(new Bits(0b0000_0000_0000_1111_1111_1111_1111_0000));
	static final Map dResAllocated = 
			new Map(new Bits(0b0000_0011_1111_0000_0000_0000_0000_0000));
	static final Map dWorkAmount = 
			new Map(new Bits(0b1111_1100_0000_0000_0000_0000_0000_0000));

	
	
	public static final class Map implements MapRoomData {

		private final Bits bits;
		public final int max;
		
		Map(Bits bits){
			this.bits = bits;
			this.max = bits.mask;
		}
		
		@Override
		public int get(int tile) {
			return bits.get(SETT.ROOMS().data.get(tile));
		}

		@Override
		public int get(int tx, int ty) {
			if (SETT.IN_BOUNDS(tx, ty))
				return get(tx+ty*SETT.TWIDTH);
			return 0;
		}

		@Override
		public void set(ROOMA r, int tile, int value) {
			if (value < 0 || value > max)
				throw new RuntimeException("" + value);
			int d = SETT.ROOMS().data.get(tile);
			d = bits.set(d, value);
			SETT.ROOMS().data.set(r, tile, d);
		}
		
	}
	

	
}
