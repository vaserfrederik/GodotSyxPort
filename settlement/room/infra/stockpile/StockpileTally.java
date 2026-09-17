package settlement.room.infra.stockpile;

import java.io.IOException;
import java.util.Arrays;

import game.time.TIME;
import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.room.infra.logistics.MoveDic;
import settlement.room.main.job.StorageCrate;
import settlement.stats.STATS;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.sets.ArrayListGrower;
import util.data.DOUBLE_O;
import util.data.INT_O;
import util.info.INFO;
import util.statistics.HistoryResource;

public final class StockpileTally{

	

	private final HistoryResource amountDay = new HistoryResource(STATS.DAYS_SAVED, TIME.days(), true) {
		
		private final INFO info = new INFO(
				MoveDic.¤¤Stored, MoveDic.¤¤Stored);
			
			@Override
			public INFO info() {
				return info;
			};
	};
	

	
	
	final ArrayListGrower<TallyData> datas = new ArrayListGrower<>();
	public final TallyData crates = new TallyData(MoveDic.¤¤crates);
	public final TallyData space = new TallyData(MoveDic.¤¤capacity);
	public final TallyData spaceReserved = new TallyData(MoveDic.¤¤capacityRes);
	public final TallyData amount = new TallyData(MoveDic.¤¤Stored) {
		
		@Override
		void set(StockpileInstance ins, int ri, int am) {
			super.set(ins, ri, am);
			amountDay.set(RESOURCES.ALL().get(ri), total(ri));
		};
		
	};
	public final TallyData amountReserved = new TallyData(MoveDic.¤¤StoredRes);
	
	
	public final DOUBLE_O<RESOURCE> usage = new DOUBLE_O<RESOURCE>() {

		@Override
		public double getD(RESOURCE t) {
			int sp = (int)space.total(t);
			if (sp == 0)
				return 1.0;
			double used = (int) amount.total(t);
			return used/sp;
		}		
	};
	
	public final INT_O<RESOURCE> amountReservable = new INT_O<RESOURCE>() {

		@Override
		public int get(RESOURCE res) {
			return amount.total(res)-amountReserved.total(res);
		}

		@Override
		public int min(RESOURCE t) {
			return 0;
		}

		@Override
		public int max(RESOURCE t) {
			return Integer.MAX_VALUE;
		}		
	};
	
	void clear() {
		for (TallyData d : datas)
			d.clear();
	}
	
	public StockpileTally() {
		// TODO Auto-generated constructor stub
	}

	
	void init(StockpileInstance ins) {
		ins.tdata = Alloc.i2(datas.size(), RESOURCES.ALL().size()+1); 
		for (int i = 0; i < ins.crates.size(); i++) {
			ins.crates.set(i);
			StorageCrate cr = ins.crate(ins.crates.get().x(), ins.crates.get().y());
			report(cr, ins, 1);
		}
		ins.updateMasks();
	}
	
	void report(StorageCrate cr, StockpileInstance ins, int delta) {
		
		if (cr.resource() != null) {
			int ri = cr.resource().index();
			crates.inc(ins, ri, delta);
			space.inc(ins, ri, delta*ins.crateSize(cr.resource()));
			spaceReserved.inc(ins, ri, delta*cr.reservedSpace());
			amount.inc(ins, ri, delta*cr.amount());
			amountReserved.inc(ins, ri, delta*cr.reserved());
		}
	}

	public HistoryResource amountsDay() {
		return amountDay;
	}
	
	public double load(RESOURCE res) {
		if (space.total(res) == 0)
			return 1;
		return (double)amount.total(res)/space.total(res);
	}

	public int amountTotal(RESOURCE res) {
		return amount.total(res);
	}

	final SAVABLE saver = new SAVABLE() {
		
		@Override
		public void save(FilePutter file) {
			amountDay.save(file);
		}
		
		@Override
		public void load(FileGetter file) throws IOException {
			amountDay.load(file);
		}
		
		@Override
		public void clear() {
			amountDay.clear();
		}
	};

	
	public class TallyData {
		
		private final int[] ams = Alloc.ii(RESOURCES.ALL().size()+1);
		private final RBITImp bits = new RBITImp();
		public final CharSequence name;
		private final int index;
		
		TallyData(CharSequence name){
			this.name = name;
			index = datas.add(this);
		}
		
		public int total(int ri) {
			return ams[ri];
		}
		
		public int total(RESOURCE res) {
			if (res == null)
				return ams[RESOURCES.ALL().size()];
			return ams[res.index()];
		}
		
		public int get(int ri, StockpileInstance ins) {
			return ins.tdata[index][ri];
		}
		
		public int get(RESOURCE res, StockpileInstance ins) {
			if (res == null)
				return ins.tdata[index][RESOURCES.ALL().size()];
			return ins.tdata[index][res.index()];
		}
		
		void inc(StockpileInstance ins, int ri, int am) {
			set(ins, ri, get(ri, ins)+am);
		}
		
		void set(StockpileInstance ins, int ri, int am) {
			int old = ins.tdata[index][ri];
			ins.tdata[index][RESOURCES.ALL().size()] -= old;
			ins.tdata[index][ri] = am;
			ins.tdata[index][RESOURCES.ALL().size()] += am;
			
			ams[ri] += am-old;
			ams[RESOURCES.ALL().size()] += am-old;
			
			if (ams[ri] < 0)
				throw new RuntimeException("" + RESOURCES.ALL().get(ri) + " " + name);
			if (ams[ri] > 0)
				bits.or(RESOURCES.ALL().get(ri));
			else
				bits.clear(RESOURCES.ALL().get(ri));
		}
		
		void clear() {
			Arrays.fill(ams, 0);
			bits.clear();
		}
		
		public RBIT bits() {
			return bits;
		}
		
	}
	
}
