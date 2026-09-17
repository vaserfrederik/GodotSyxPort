package settlement.room.infra.hauler;

import java.util.Arrays;

import game.time.TIME;
import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.room.infra.logistics.MoveDic;
import settlement.room.main.job.StorageCrate;
import settlement.stats.STATS;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.Alloc;
import snake2d.util.sets.ArrayListGrower;
import util.data.DOUBLE_O;
import util.data.INT_O;
import util.info.INFO;
import util.statistics.HistoryResource;

public final class HaulerTally{
	

	private final HistoryResource amounts = new HistoryResource(64, TIME.seasons(), true) {
		
		private final INFO info = new INFO(
				MoveDic.¤¤Stored, MoveDic.¤¤StoredD);
		
		@Override
		public INFO info() {
			return info;
		};
	};
	private final HistoryResource amountDay = new HistoryResource(STATS.DAYS_SAVED, TIME.days(), true) {
		
		@Override
		public INFO info() {
			return amounts.info();
		};
	};

	final ArrayListGrower<TallyData> datas = new ArrayListGrower<>();
	public final TallyData crates = new TallyData(MoveDic.¤¤crates);
	public final TallyData space = new TallyData(MoveDic.¤¤capacity);
	public final TallyData spaceReserved = new TallyData(MoveDic.¤¤capacityRes);
	public final TallyData amount = new TallyData(MoveDic.¤¤Stored) {
		
		@Override
		void set(HaulerInstance ins,  int am) {
			super.set(ins, am);
			if (ins.resource() != null)
			amountDay.set(ins.resource(), total(ins.resource()));
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
	
	public HaulerTally() {
		// TODO Auto-generated constructor stub
	}

	
	void init(HaulerInstance ins) {
		ins.tdata = Alloc.ii(datas.size());
		for (COORDINATE c : ins.body()) {
			StorageCrate cr = ins.storage(c.x(), c.y());
			if (cr != null)
				report(cr, ins, 1);
		}
		ins.updateMasks();
	}
	
	void report(StorageCrate cr, HaulerInstance ins, int delta) {
		
		if (cr.resource() != null) {
			crates.inc(ins, delta);
			space.inc(ins, delta*Crate.size);
			spaceReserved.inc(ins, delta*cr.reservedSpace());
			amount.inc(ins, delta*cr.amount());
			amountReserved.inc(ins, delta*cr.reserved());
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
		
		public int get(HaulerInstance ins) {
			return ins.tdata[index];
		}
		
		void inc(HaulerInstance ins, int am) {
			set(ins, get(ins)+am);
		}
		
		void set(HaulerInstance ins, int am) {
			
			int old = ins.tdata[index];
			ins.tdata[index] = am;
			if (ins.resource() == null)
				return;
			int ri = ins.resource().index();
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
