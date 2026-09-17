package settlement.room.main.util;

import settlement.room.main.RoomInstance;
import snake2d.util.file.Alloc;
import snake2d.util.sets.ArrayListGrower;
import util.data.INT;
import util.data.INT.IntImp;
import util.data.INT_O.INT_OE;

public abstract class RoomTally {

	private ArrayListGrower<TallyEntry> all = new ArrayListGrower<TallyEntry>();
	
	public class TallyEntry implements INT_OE<RoomInstance>{
		
		public final CharSequence name;
		public final int index;
		private final IntImp ptot = new IntImp();
		public final INT total = ptot;
		public final TallyEntry tottot;
		
		
		TallyEntry(CharSequence name){
			this(name, null);
		}
		
		TallyEntry(CharSequence name, TallyEntry total){
			
			index = all.add(this);
			this.name = name;
			this.tottot = total;
		}
		
		@Override
		public int min(RoomInstance t) {
			return 0;
		}
		
		@Override
		public int max(RoomInstance t) {
			return Integer.MAX_VALUE;
		}
		
		@Override
		public int get(RoomInstance t) {
			return data(t)[index];
		}
		
		@Override
		public void set(RoomInstance t, int amount) {
			int[] data = data(t);
			if (tottot != null)
				tottot.inc(t, -data[index]);
			ptot.inc(-data[index]);
			data[index] = amount;
			if (tottot != null)
				tottot.inc(t, data[index]);
			ptot.inc(data[index]);
		}
	}
	
	public TallyEntry make(CharSequence name) {
		return new TallyEntry(name);
				
	}
	
	public TallyEntry make(CharSequence name, TallyEntry tot) {
		return new TallyEntry(name, tot);
				
	}
	
	protected abstract int[] data(RoomInstance ins);
	
	public int[] makeInstanceData() {
		return Alloc.ii(all.size());
	}
	
	public void clear() {
		for (TallyEntry e : all) {
			e.ptot.set(0);
		}
	}
	
	public void load(RoomInstance ins) {
		int[] dd = data(ins);
		for (int i = 0; i < all.size(); i++) {
			all.get(i).ptot.inc(dd[i]);
		}
	}
}
