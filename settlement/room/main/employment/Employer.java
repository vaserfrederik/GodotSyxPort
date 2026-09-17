package settlement.room.main.employment;

import java.util.Arrays;

import game.GAME;
import init.type.WGROUP;
import settlement.stats.STATS;
import snake2d.LOG;
import snake2d.util.file.Alloc;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import snake2d.util.sets.Tree;

final class Employer {

	private boolean log = false;
	private int state = 0;
	private final AA[] states;
	
	private boolean updateAll = false;
	
	private final Workers available = new Workers();
	private final Workers workforce = new Workers();

	private final Bucket[] buckets;
	
	Employer(LIST<RoomEmployment> all){
		
		buckets = new Bucket[all.size()];
		int bi = 0;
		for (RoomEmployment e : all) {
			buckets[bi++] = new Bucket(e);
		}
		
		states = new AA[] {
				new A_GInit()
				,new A_Target()
				,new A_Assign()
				,new A_Assign_Extra()
				,new A_Setter()
				
		};
		
		

		
	}

	
	
	void update() {
		

		
		if (updateAll) {
			for (AA s : states)
				s.clear();
			
			state = 0;
			while(state < states.length) {
				if (!states[state].update()) {
					
					state++;
				}
			}
			state = 0;
			updateAll = false;
			return;
		}
		
		if (GAME.SPEED.speed() == 0)
			return;
		
		if ((GAME.updateI() & 7) == 0) {
//			if (log) {
//				LOG.ln("-----------------------------------------");
//				LOG.ln("state " + state + " " + states[state]);
//			}
				
			if (!states[state].update()) {
				
				state++;
				state %= states.length;
			}
			
		}
		
	}
	
	public void updateAll() {
		updateAll = true;
		
	}

	private interface AA {
		
		boolean update();
		void clear();
	}

	private class A_GInit implements AA{
		
		@Override
		public boolean update() {
			
			available.clear();
			workforce.clear();
			for (WGROUP w : WGROUP.all()) {
				available.add(w, STATS.WORK().workforce(w));
				workforce.add(w, STATS.WORK().workforce(w));
			}
			return false;
		}

		@Override
		public void clear() {
			// TODO Auto-generated method stub
			
		}
		
		
	}
	
	/**
	 * sets target employees based on primary priority
	 */
	private class A_Target implements AA{

		private ArrayList<Bucket> samePrio = new ArrayList<Bucket>(buckets.length);
		private final Tree<Bucket> sort = new Tree<Bucket>(buckets.length) {
			@Override
			protected boolean isGreaterThan(Bucket current, Bucket cmp) {
				return current.prio*buckets.length+current.e.index() > cmp.prio*buckets.length+cmp.e.index();
			}
		};
		private int a;
		
		@Override
		public boolean update() {
			
			
			if (!sort.hasMore()) {
				a = available.tot();
				
				for (Bucket e : buckets) {
					e.clear();
					
					if (e.needed > 0) {
						sort.add(e);
						if (log) {
							LOG.ln("TARGET adding: " + e.e.blueprint().key + " " + e.needed);
						}
					}
				}
				if (!sort.hasMore())
					return false;
				return true;
			}
			Bucket f = sort.pollGreatest();
			samePrio.clearSloppy();
			samePrio.add(f);
			while(sort.hasMore() && sort.greatest().e.priority.get() == f.e.priority.get()) {
				samePrio.add(sort.pollGreatest());
			}
			
			int needed = 0;
			for (Bucket e : samePrio) {
				needed += e.e.neededWorkers();
			}
			
			double d = (double)a/needed;
			d = CLAMP.d(d, 0, 1);
			
			if (log) {
				LOG.ln("TARGET avaliable: " + a + "/" + needed + " " + d + " " + sort.hasMore());
			}
			
			for (Bucket e : samePrio) {
				int t = (int) Math.ceil(e.e.neededWorkers()*d);
				t = CLAMP.i(t, 0, a);
				a -= t;
				e.target = t;
				if (log && t > 0)
					LOG.ln("TARGET " + e.e.blueprint().key + " " + t);
			}
			
			return sort.hasMore();
		}

		@Override
		public void clear() {
			sort.clear();
		}
		
		
	}
	
	/**
	 * assigns work groups according to the group priorities.
	 */
	private class A_Assign implements AA{

		private final ArrayList<WGROUP> same = new ArrayList<WGROUP>(WGROUP.all().size());

		private final Tree<Bucket> toBeAssigned = new Tree<Bucket>(buckets.length) {
			@Override
			protected boolean isGreaterThan(Bucket current, Bucket cmp) {
				return current.prio2*buckets.length+current.e.index() > cmp.prio2*buckets.length+cmp.e.index();
			}
		};
		
		@Override
		public boolean update() {
			
			
			if (!toBeAssigned.hasMore()) {
				
				for (Bucket b : buckets) {
					if (b.target > 0) {
						int hi = 0;
						for (int gi = 0; gi < WGROUP.all().size(); gi++) {
							WGROUP g = WGROUP.all().get(gi);
							if (workforce.get(g) > 0)
								hi = Math.max(hi, b.e.priorities.get(g));
						}
						b.prio2 = hi;
						toBeAssigned.add(b);
					}
				}
				return toBeAssigned.size() > 0;
			}
			
			Bucket b = toBeAssigned.pollGreatest();
			
			if (assign(b)) {
				int hi = 0;
				for (int gi = 0; gi < WGROUP.all().size(); gi++) {
					WGROUP g = WGROUP.all().get(gi);
					if (available.get(g) > 0)
						hi = Math.max(hi, b.e.priorities.get(g));
				}
				b.prio2 = hi;
				toBeAssigned.add(b);
				
			}
			return toBeAssigned.hasMore();
			
		}
		
		private boolean assign(Bucket b) {
			
			if (log) {
				LOG.ln("ASSIGN assigning: " + b.e.blueprint().key + " " + b.prio);
			}
			
			if (b.tot() >= b.target) {
				if (log) {
					LOG.ln("ASSIGN done");
				}
				return false;
			}
			
			
			
			int hi = 0;
			same.clearSloppy();

			for (int gi = 0; gi < WGROUP.all().size(); gi++) {
				WGROUP g = WGROUP.all().get(gi);
				if (workforce.get(g) > 0)
					hi = Math.max(hi, b.e.priorities.get(g));
			}
			
			if (hi == 0) {
				if (log) {
					LOG.ln("no one can work here");
				}
				
				return false;
			
			}
			
			hi = 0;
			for (int gi = 0; gi < WGROUP.all().size(); gi++) {
				WGROUP g = WGROUP.all().get(gi);
				if (available.get(g) > 0)
					hi = Math.max(hi, b.e.priorities.get(g));
			}
//			
			if (hi == 0) {
				
				if (log) {
					LOG.ln("special crap");
				}
				
				same.clearSloppy();
				for (WGROUP g : WGROUP.all()) {
					if (workforce.get(g) > 0 && b.e.priorities.get(g) > 0) {
						same.add(g);
						if (log) {
							LOG.ln("  candidate -> " + g.toString());
						}
					}
				}
				
				for (int gi = 0; gi < WGROUP.all().size(); gi++) {
					WGROUP g = WGROUP.all().get(gi);
					if (available.get(g) > 0) {
						if (log) {
							LOG.ln("  pushing -> " + g.toString());
						}
						if (pushIn(b, g, same)) {
							if (log) {
								LOG.ln("  pushed -> " + g.toString() + " " + b.tot() + "/" + b.target);
							}
							return true;
						}
						//available.add(g, -available.get(g));
						return false;
					}
				}
				
				
				return false;
			}
			
			for (int gi = 0; gi < WGROUP.all().size(); gi++) {
				WGROUP g = WGROUP.all().get(gi);
				if (available.get(g) > 0 && hi == b.e.priorities.get(g))
					same.add(g);
			}
			
			if (log) {
				LOG.ln("ASSIGN normal allocation");
			}
			
			allocate(b, same, hi);
			return true;
			
			
		}
		
		private boolean pushIn(Bucket missingWorkplace, WGROUP toPushIn, LIST<WGROUP> candidates) {
			
			Bucket target = null;
			WGROUP toPushOut = null;
			int bestPrio = Integer.MIN_VALUE;
			
			for (Bucket b : buckets) {
				if (b == missingWorkplace)
					continue;
				int p = b.e.priorities.get(toPushIn);
				if (p == 0)
					continue;
				if (b.get(toPushIn) >= b.tot())
					continue;
				
				for (WGROUP g : candidates) {
					if (b.get(g) > 0 && g != toPushIn) {
						int po = missingWorkplace.e.priorities.get(g);
						if (po > 0) {
							int prio = po-b.e.priorities.get(g);
							prio += b.e.priorities.get(toPushOut)-p;
							if (prio > bestPrio) {
								target = b;
								toPushOut = g;
								bestPrio = prio;
							}
						}
						
					}
				}
				
				
			}
			
			if (target != null) {
				int am = target.get(toPushOut);
				am = CLAMP.i(am, 0, available.get(toPushIn));
				am = CLAMP.i(am, 0, missingWorkplace.target-missingWorkplace.tot());
				if (log) {
					LOG.ln("  pusham : " + am + " " + toPushOut + " " + available.get(toPushOut) + " -> " + toPushIn);
				}
				target.add(toPushOut, -am);
				target.add(toPushIn, am);
				available.add(toPushOut, am);
				available.add(toPushIn, -am);
				return true;
			}
			
			return false;
		}
		
		void allocate(Bucket e, LIST<WGROUP> same, int prio){
			
			allocatePref(e, same);
			
			int toAllocate = e.target-e.tot();
			int av = 0;
			for (WGROUP g : same) {
				av += CLAMP.i(available.get(g), 0, toAllocate);
			}
			
			double d = (double)toAllocate/av;
			d = CLAMP.d(d, 0, 1);
			
			if (log)
				LOG.ln("ASSIGN ALLOCATING: " +  e.e.blueprint().key + " " + e.target + " " + prio + " " + d + " " + same.size());
			
			for (WGROUP g : same) {
				int am = (int) Math.ceil(available.get(g)*d);
				am = CLAMP.i(am, 0, e.target-e.tot());
				available.add(g, -am);
				e.add(g, am);
				if (log)
					LOG.ln("ASSIGN A: " + g.race + " " + g.type + " " + am);
			}			
		}

		void allocatePref(Bucket e, LIST<WGROUP> same){
			
			double pref = 0;
			for (WGROUP g : same) {
				double p = e.e.neededWorkers(g);
				if (p > 0) {
					pref += p;
				}
			}
			

			int toAllocate = e.needed-e.tot();
			if (log)
				LOG.ln("ASSIGN PREF ALLOCATING: " +  e.e.blueprint().key + " " + e.needed + " " + e.tot() + " " + toAllocate + " " + same.size() + " " + pref);
			
			if (pref == 0)
				return;
			
			
			
			for (WGROUP g : same) {

				int am = (int) Math.ceil(e.e.neededWorkers(g)*e.e.neededWorkers(g)/pref);
				am -= e.get(g);
				if (am > available.get(g))
					am = available.get(g);
				int needed = e.needed-e.tot();
				if (am > needed)
					am = needed;
				available.add(g, -am);
				e.add(g, am);
				if (log)
					LOG.ln("ASSIGN AP: " + g.race + " " + g.type + " " + am + " " + e.e.neededWorkers());
			}
			
			for (WGROUP g : same) {

				int am = e.e.neededWorkers(g);
				am -= e.get(g);
				if (am > available.get(g))
					am = available.get(g);
				int needed = e.needed-e.tot();
				if (am > needed)
					am = needed;
				if (am > 0) {
					available.add(g, -am);
					e.add(g, am);
					if (log)
						LOG.ln("ASSIGN AP topup: " + g.race + " " + g.type + " " + am + " " + e.e.neededWorkers());
				}
				
			}	
		}
		
		
		
		@Override
		public void clear() {
			toBeAssigned.clear();
		}
		
		
		
	}
	
	private class A_Assign_Extra implements AA{

		private final Tree<Bucket> toBeAssigned = new Tree<Bucket>(buckets.length) {
			@Override
			protected boolean isGreaterThan(Bucket current, Bucket cmp) {
				return current.prio*buckets.length+current.e.index() > cmp.prio*buckets.length+cmp.e.index();
			}
		};
		
		private final ArrayList<WGROUP> same = new ArrayList<WGROUP>(WGROUP.all().size());
		
		@Override
		public boolean update() {
			
			if (!toBeAssigned.hasMore()) {
				
				for (Bucket b : buckets) {
					if (b.tot() < b.needed)
						toBeAssigned.add(b);
				}
				if (log) {
					for (WGROUP g : WGROUP.all()) {
						if (available.get(g) > 0)
							LOG.ln(g + " " + workforce.get(g));
					}
				}
				return toBeAssigned.size() > 0;
			}
			
			Bucket b = toBeAssigned.pollGreatest();
			
			if (assign(b)) {
				toBeAssigned.add(b);
				
			}
			return toBeAssigned.hasMore();
			
		}
		
		private boolean assign(Bucket b) {
			
			if (log) {
				LOG.ln("assigning: " + b.e.blueprint().key);
			}
			
			if (b.tot() >= b.needed) {
				if (log) {
					LOG.ln("done done");
				}
				return false;
			}
			
			
			
			int hi = 0;
			
			same.clearSloppy();

			for (WGROUP g : WGROUP.all()) {
				if (available.get(g) > 0)
					hi = Math.max(hi, b.e.priorities.get(g));
			}
			
			if (hi == 0) {
				if (log) {
					LOG.ln("no workers");
				}
				return false;
			}
			
			for (WGROUP g : WGROUP.all()) {
				if (available.get(g) > 0 && hi == b.e.priorities.get(g))
					same.add(g);
			}
			
			if (log) {
				LOG.ln("normal allocation2");
			}
			
			allocate(b, same, hi);
			return true;
			
			
		}
		
		void allocate(Bucket e, LIST<WGROUP> same, int prio){
			
			
			int toAllocate = e.needed-e.tot();
			int av = 0;
			

			for (WGROUP g : same) {
				av += CLAMP.i(available.get(g), 0, toAllocate);
			}
			

			
			if (log)
				LOG.ln("ALLOCATING: " +  e.e.blueprint().key + " " + e.needed + " " + e.tot() + " " + toAllocate + " " + same.size() + " " +av);
			
			double d = (double)toAllocate/av;
			d = CLAMP.d(d, 0, 1);
			
			
			
			for (WGROUP g : same) {
				int am = (int) Math.ceil(available.get(g)*d);
				int needed = e.needed-e.tot();
				am = CLAMP.i(am, 0, needed);
				available.add(g, -am);
				e.add(g, am);
				if (log)
					LOG.ln("A: " + g.race + " " + g.type + " " + am);
			}			
		}

		@Override
		public void clear() {
			toBeAssigned.clear();
		}
		
		
		
	}
	
	private class A_Setter implements AA{
		
		int ei = 0;
		
		@Override
		public boolean update() {
			

			Bucket e = buckets[ei];
			e.e.target.clear();
			if (log) {
				LOG.ln("SET " +  e.e.blueprint().key + " " + e.needed + " " + e.target + " " + e.tot());
			}
			for (WGROUP g : WGROUP.all()) {
				
				if (e.get(g) > 0) {
					e.e.target.add(g, e.get(g));
					if (log) {
						LOG.ln("   " + g.toString() + " -> " +  e.get(g));
					}
					
				}
			}
			ei++;
			if (ei >= buckets.length) {
				ei = 0;
				return false;
			}
			return true;
		}

		@Override
		public void clear() {
			ei = 0;
		}
		
		
	}
	
	private static class Workers {
		
		private final int[] amount = Alloc.ii(WGROUP.all().size());
		private int total = 0;
		
		public void clear() {
			Arrays.fill(amount, 0);
			total = 0;
		}

		public void add(WGROUP w, int workforce) {
			amount[w.index()] += workforce;
			total += workforce;
		}
		
		public int get(WGROUP g) {
			return amount[g.index()];
		}
		
		public int tot() {
			return total;
		}
		
	}
	
	private static class Bucket extends Workers{
		
		public final RoomEmployment e;
		public int target = 0;
		public int prio = 0;
		public int prio2 = 0;
		private int needed;
		
		
		Bucket(RoomEmployment e){
			this.e = e;
			
		}
		
		@Override
		public void clear() {
			target = 0;
			prio = e.priority.get();
			needed = e.neededWorkers();
			super.clear();
		}
		
	}




}
