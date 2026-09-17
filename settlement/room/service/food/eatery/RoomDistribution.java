package settlement.room.service.food.eatery;

import java.io.IOException;
import java.io.Serializable;
import java.util.Arrays;

import game.GAME;
import game.audio.SoundRace;
import game.faction.FResources.RTYPE;
import init.race.RACES;
import init.race.Race;
import init.resources.Meal;
import init.resources.RBIT;
import init.resources.RBIT.RBITImp;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.resources.ResG;
import init.settings.S;
import init.sprite.UI.UI;
import settlement.entity.humanoid.Humanoid;
import settlement.entity.humanoid.ai.work.AIModule_Work;
import settlement.main.SETT;
import settlement.misc.job.JOBMANAGER_HASER;
import settlement.misc.job.JOB_MANAGER;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.ROOM_IDATA_INSTANCE;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.main.util.RoomBits;
import settlement.room.main.util.RoomState.RoomStateInstance;
import settlement.room.main.util.RoomTally;
import settlement.room.main.util.RoomTally.TallyEntry;
import settlement.room.service.module.ROOM_SERVICER;
import settlement.room.service.module.RoomServiceAccess.ROOM_SERVICE_ACCESS_HASER;
import snake2d.SPRITE_RENDERER;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.Bitmap1D;
import snake2d.util.sets.LIST;
import snake2d.util.sets.LISTE;
import snake2d.util.sets.Stack;
import snake2d.util.sprite.SPRITE;
import snake2d.util.sprite.text.Str;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GChart;
import util.gui.misc.GGrid;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;
import util.text.DicTime;
import view.sett.ui.room.ModuleIndustry;
import view.sett.ui.room.UIRoomModule;

public abstract class RoomDistribution implements SAVABLE{

	private static CharSequence ¤¤Consumed = "¤Consumed";
	private static CharSequence ¤¤uses = "¤Some or all stands are distributing this food. Click to disable this resource for all.";
	private static CharSequence ¤¤usesN = "¤No stands are distributing this food. Click to enable it for all";
	private static CharSequence ¤¤Preferred = "¤Preferred";
	private static CharSequence ¤¤worked = "¤How much this room is prepared. Some preparation is required before the resources can be consumed.";
	static {
		D.ts(RoomDistribution.class);
	}
	
	
	private final RoomBlueprintIns<? extends RoomDistributionIns> blue;
	private final ROOM_SERVICE_ACCESS_HASER ser;
	private final Industry industry;
	
	public final LIST<RESOURCE> all;
	private final int[] resourceIs = Alloc.ii(RESOURCES.ALL().size());
	private final RBITImp useMask;
	public final RoomTally tally = new RoomTally() {
		
		@Override
		protected int[] data(RoomInstance ins) {
			return ((RoomDistributionIns)ins).distributionData().tdata;
		}
	};
	public final TallyEntry tStored  = tally.make(Dic.¤¤Stored);
	public final TallyEntry tIncoming  = tally.make(Dic.¤¤Inbound);
	public final TallyEntry tReserved = tally.make("reserved");
	public final LIST<TallyEntry> allStored;
	public final LIST<TallyEntry> allIncoming;
	
	private final Crate crate = new Crate();
	private final int maxRations;
	
	private final ArrayList<RESOURCE> shuffle;
	private final Bitmap1D check;
	
	public RoomDistribution(RoomBlueprintIns<? extends RoomDistributionIns> e, ROOM_SERVICE_ACCESS_HASER ser, LIST<RESOURCE> all, RBITImp useMask,  int maxRations){
		this.blue = e;
		this.ser = ser;
		RESOURCE[] ires = new RESOURCE[all.size()];
		for (int i = 0; i < all.size(); i++)
			ires[i] = all.get(i);
		this.industry = new Industry(e, 
				ires, new double[all.size()], 
				null);;
		this.useMask = new RBITImp();
		this.useMask.clear();
		this.all = new ArrayList<RESOURCE>(all);
		Arrays.fill(resourceIs, -1);
		
		for (int i = 0; i < all.size(); i++) {
			resourceIs[all.get(i).index()] = i;
			if (useMask.has(all.get(i)))
				this.useMask.set(all.get(i), true);
		}
		
		this.maxRations = maxRations;
		ArrayListGrower<TallyEntry> tt = new ArrayListGrower<TallyEntry>();
		for (@SuppressWarnings("unused") RESOURCE t : all) {
			tt.add(tally.make(Dic.¤¤Stored, tStored));
		}
		this.allStored = tt;
		tt = new ArrayListGrower<TallyEntry>();
		for (@SuppressWarnings("unused") RESOURCE t : all) {
			tt.add(tally.make(Dic.¤¤Inbound, tIncoming));
		}
		this.allIncoming = tt;	
		
		shuffle = new ArrayList<RESOURCE>(all.size());
		check = new Bitmap1D(RESOURCES.ALL().size(), false);
		
		

	}
	
	protected abstract boolean isCrate(int tx, int ty);
	protected abstract boolean isDeposit(int tx, int ty);
	protected abstract boolean isPref(RESOURCE r, Race race);
	
	public TallyEntry stored(RESOURCE res) {
		return allStored.get(resourceIs[res.index()]);
	}
	
	@Override
	public void save(FilePutter file) {
		industry.save(file);
	}
	
	@Override
	public void load(FileGetter file) throws IOException {
		tally.clear();
		int ll = tally.makeInstanceData().length;
		for (int ii = 0; ii < blue.instancesSize(); ii++) {
			RoomInstance ins = blue.getInstance(ii);
			RoomDistributionIns d = (RoomDistributionIns) ins;
			InstanceData dd = d.distributionData();
			if (dd.tdata.length != ll) {
				dd.tdata = tally.makeInstanceData();
				dd.pdata = industry.makeData();
				dd.fetchMask.clear(useMask);
				dd.useMask.clear(useMask);
				for (COORDINATE c : ins.body()) {
					if (ins.is(c)) {
						if (init(c.x(), c.y())) {
							SETT.ROOMS().data.set(ins, c, 0);
						}
					}
				}
			}else {
				tally.load(ins);
			}
		}
		industry.load(file);
	}
	
	@Override
	public void clear() {
		tally.clear();
		industry.clear();
	}
	
	public InstanceData makeData(int maxAmount) {
		return new InstanceData(this, maxRations*maxAmount);
	}
	
	public SETT_JOB job(int tx, int ty) {
		if (!init(tx, ty))
			return null;
		if (isCrate(tx, ty))
			return crate.workCook;
		if (isDeposit(tx, ty))
			return crate.workRes;
		return null;
	}
	
	public FSERVICE service(int tx, int ty) {
		if (init(tx, ty) && isCrate(tx, ty))
			return crate.service;
		return null;
	}
	
	private boolean init(int tx, int ty) {
		RoomInstance ins = blue.getter.get(tx, ty);
		if (ins == null)
			return false;
		crate.ins = ins;
		crate.data = (RoomDistributionIns)ins;
		crate.coo.set(tx, ty);
		return true;
	}
	

	public int consume(LIST<ResG> prefs, int amount, int tx, int ty) {
		
		
		ResG pref = prefs.rnd();
		
		

		if (!init(tx, ty))
			return Meal.make(pref, 0, 0);
		int am = 0;
		double value = 0;
		
		am = consume(pref.resource, amount, tx, ty);
		value = am;
		amount -= am;
		
		if (amount > 0) {
			check.clear();
			shuffle.clearSloppy();
			for (ResG e : prefs) {
				check.set(e.resource.index(), true);
				shuffle.add(e.resource);
			}
			shuffle.shuffle();
			for (RESOURCE r : shuffle) {
				int a = consume(r, amount, tx, ty);
				am += a;
				value += 0.25*a;
				amount -= a;
				if (amount <= 0)
					break;
			}
			if (amount > 0) {
				shuffle.clearSloppy();
				for (RESOURCE e : all) {
					if (!check.get(e.index()))
						shuffle.add(e);
				}
				shuffle.shuffle();
				for (RESOURCE r : shuffle) {
					
					int a = consume(r, amount, tx, ty);

					am += a;
					value += 0.25*a;
					amount -= a;
					if (amount <= 0)
						break;
				}
				
			}
		}
		if (am > 0)
			value /= am;
		
		return Meal.make(pref, am, value);
		
	}
	
	public int consume(RESOURCE res, int amount, int tx, int ty) {
		
		if (!init(tx, ty))
			return 0;
	
		if (crate.serviceReserved.get() == 1) {
			crate.serviceReserved.set(crate.ins, 0);
		}
		
		int ri = resourceIs[res.index()];
		int max = allStored.get(ri).get(crate.ins);
		max = Math.min(max, tStored.get(crate.ins));
		max = Math.min(amount, max);
		if (max > 0) {
			allStored.get(ri).inc(crate.ins, -max);
			crate.workAmount.inc(crate.ins, -1);
			industry.ins().get(ri).inc(crate.data.distributionData(), amount, false);
			GAME.player().res().inc(res, RTYPE.CONSUMED, -amount);
			setMask(res, crate.data.distributionData(), crate.ins);
			return max;
		}
		return 0;
	}
	
	public void usesToggle(RESOURCE e, RoomInstance ins) {
		InstanceData dd = ((RoomDistributionIns)ins).distributionData();
		
		dump(e, dd, ins);
		dd.useMask.toggle(e);
		for (COORDINATE c : ins.body()) {
			if (ins.is(c)) {
				if (init(c.x(), c.y()))
					crate.workAmount.set(ins, crate.workAmount.get());
			}
		}
		setMask(e, dd, ins);
		((RoomDistributionIns)ins).getWork().resetResourceSearch();
	}

	private void dump(RESOURCE e, InstanceData dd, RoomInstance ins) {
		if (dd.useMask.has(e)) {
			int am = stored(e).get(ins);
			allStored.get(resourceIs[e.index()]).set(ins, 0);
			allIncoming.get(resourceIs[e.index()]).set(ins, 0);
			if (am > 0) {
				SETT.THINGS().resources.create(ins.mX(), ins.mY(), e, am);
			}
		}
	}
	
	private void setMask(RESOURCE e, InstanceData dd, RoomInstance ins) {
		if (allStored.get(resourceIs[e.index()]).get(ins) + allIncoming.get(resourceIs[e.index()]).get(ins) + AIModule_Work.MAX_FETCH_AMOUNT < dd.maxAmount) {
			dd.fetchMask.or(e);
		}else {
			dd.fetchMask.clear(e);
		}
		dd.fetchMask.and(dd.useMask);
		
	}
	

	
	public void dispose(RoomInstance ins) {
		InstanceData dd = ((RoomDistributionIns)ins).distributionData();
		for (RESOURCE r : all){
			dump(r, dd, ins);
		}
		for (COORDINATE c : ins.body()) {
			if (ins.is(c)) {
				if (init(c.x(), c.y()))
					crate.workAmount.set(crate.ins, 0);
			}
		}

	}
	
	public boolean isWorked(int tx, int ty) {
		if (job(tx, ty) == crate.workCook)
			return crate.workAmount.get() >= maxRations;
		return false;
	}
	
	public int usedAmount(int tx, int ty) {
		if (job(tx, ty) == crate.workCook)
			return crate.serviceUsed.get();
		return 0;
	}
	

	public static class InstanceData implements Serializable, ROOM_IDATA_INSTANCE{
		
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		int[] tdata;
		public final int maxAmount;
		private final RBITImp fetchMask = new RBITImp().clearSet(RESOURCES.EDI().mask);
		private final RBITImp useMask = new RBITImp();
		float[] consumed;
		long[] pdata;
		
		InstanceData(RoomDistribution b, int maxAmount){
			this.tdata = b.tally.makeInstanceData();
			
			this.maxAmount = maxAmount;
			this.useMask.clearSet(b.useMask);
			
			pdata = b.industry.makeData();
			
			for (int ei = 0; ei < b.all.size(); ei++) {
				RESOURCE e = b.all.get(ei);
				if (b.blue.instancesSize() > 1 && !b.uses(e)) {
					useMask.set(e, false);
				}
			}
			fetchMask.clearSet(b.useMask);
		}

		@Override
		public long[] productionData() {
			return pdata;
		}

		@Override
		public JOB_MANAGER getWork() {
			return null;
		}
		
		public void update(RoomDistribution b) {
			b.industry.updateRoom(this);
		}
		
	}
	
	final class Crate {

		private final Coo coo = new Coo();
		private RoomInstance ins;
		private RoomDistributionIns data;
		private final RoomBits serviceReserved = new BB(				new Bits(0x0001));
		private final RoomBits serviceReservable = new RoomBits(coo, 	new Bits(0x0002));
		private final RoomBits freeWork = new RoomBits(coo, 			new Bits(0x0004));
		private final RoomBits workReserved = new RoomBits(coo, 		new Bits(0x0008));
		private final RoomBits serviceUsed = new RoomBits(coo, 			new Bits(0x00F0));
		private final RoomBits workAmount = new BB(						new Bits(0xFF00));
		
		
		
		Crate(){
			
		}
		
		private SETT_JOB workCook = new SETT_JOB() {
			
			private final int wt = 30;
			
			@Override
			public boolean jobUseTool() {
				return false;
			}
			
			@Override
			public void jobStartPerforming() {
				
			}
			
			@Override
			public SoundRace jobSound() {
				return blue.employment().sound();
			}
			
			@Override
			public RBIT jobResourceBitToFetch() {
				return null;
			}
			
			@Override
			public int jobResourcesNeeded(Humanoid skill) {
				return SETT.ROOMS().STOCKPILE.carryCap(skill);
			}
			
			@Override
			public boolean jobReservedIs(RESOURCE r) {
				return workReserved.get() == 1;
			}
			
			@Override
			public void jobReserveCancel(RESOURCE r) {
				workReserved.set(ins, 0);
			}
			
			@Override
			public boolean jobReserveCanBe() {
				return workReserved.get() == 0 && workAmount.get() <= maxRations*8 && tStored.get(ins) - tReserved.get(ins) > maxRations;
			}
			
		
			
			@Override
			public void jobReserve(RESOURCE r) {
				workReserved.set(ins, 1);
			}
			
			@Override
			public double jobPerformTime(Humanoid skill) {
				return freeWork.get() == 1 ? 1 : wt;
			}
			
			@Override
			public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram) {
				workReserved.set(ins, 0);
				workAmount.inc(ins, 1);
				if (ins.employees().fetchBonusConsume(wt+1)) {
					freeWork.set(ins, 1);
				}else {
					freeWork.set(ins, 0);
				}
				return null;
			}
			
			@Override
			public CharSequence jobName() {
				return blue.employment().verb;
			}
			
			@Override
			public COORDINATE jobCoo() {
				return coo;
			}
		};
		
		private final SETT_JOB workRes = new SETT_JOB() {
			
			@Override
			public boolean jobUseTool() {
				return false;
			}
			
			@Override
			public void jobStartPerforming() {
				
			}
			
			@Override
			public SoundRace jobSound() {
				return blue.employment().sound();
			}
			
			@Override
			public RBIT jobResourceBitToFetch() {
				return data.distributionData().fetchMask;
			}
			
			@Override
			public int jobResourcesNeeded(Humanoid skill) {
				return SETT.ROOMS().STOCKPILE.carryCap(skill);
			}
			
			@Override
			public boolean jobReservedIs(RESOURCE r) {
				if (r == null) {
					return false;
				}
				int ri = resourceIs[r.index()];
				if (ri < 0) {
					return false;
				}
					
				return allIncoming.get(ri).get(ins) > 0;
			}
			
			@Override
			public void jobReserveCancel(RESOURCE r) {
				if (r == null)
					return;
				int ri = resourceIs[r.index()];
				if (ri < 0)
					return;
				if (allIncoming.get(ri).get(ins) >= AIModule_Work.MAX_FETCH_AMOUNT)
					allIncoming.get(ri).inc(ins, -AIModule_Work.MAX_FETCH_AMOUNT);
				setMask(r, crate.data.distributionData(), crate.ins);
			}
			
			@Override
			public boolean jobReserveCanBe() {
				return !data.distributionData().fetchMask.isClear();
			}
			
			@Override
			public void jobReserve(RESOURCE r) {
				int ri = resourceIs[r.index()];
				allIncoming.get(ri).inc(ins, AIModule_Work.MAX_FETCH_AMOUNT);
				setMask(r, crate.data.distributionData(), crate.ins);
			}
			
			@Override
			public double jobPerformTime(Humanoid skill) {
				return 0;
			}
			
			@Override
			public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram) {
				int ri = resourceIs[r.index()];
				if (ri >= 0 && allIncoming.get(ri).get(ins) > 0) {
					allIncoming.get(ri).inc(ins, -AIModule_Work.MAX_FETCH_AMOUNT);
					allStored.get(ri).inc(ins, ram);
					workAmount.set(ins, workAmount.get());
					setMask(r, crate.data.distributionData(), crate.ins);
				}
				return null;
			}
			
			@Override
			public CharSequence jobName() {
				return blue.employment().verb;
			}
			
			@Override
			public COORDINATE jobCoo() {
				return coo;
			}
		};
		
		private final FSERVICE service = new FSERVICE() {
			

			
			@Override
			public boolean findableReservedCanBe() {
				return serviceReservable.get() == 1 && serviceReserved.get() == 0;
			}

			@Override
			public void findableReserve() {
				serviceReserved.set(ins, 1);
			}

			@Override
			public boolean findableReservedIs() {
				return serviceReserved.get() == 1;
			}

			@Override
			public void findableReserveCancel() {
				serviceReserved.set(ins, 0);
				serviceUsed.set(ins, 0);
			}

			@Override
			public void startUsing() {
				serviceUsed.inc(ins, 1);
			};
			
			@Override
			public int x() {
				return coo.x();
			}

			@Override
			public int y() {
				return coo.y();
			}

			@Override
			public void consume() {
				findableReserveCancel();
			}
			
		};
		
		private class BB extends RoomBits {

			public BB(Bits bits) {
				super(coo, bits);
			}
			
			@Override
			protected void remove() {
				
				if (serviceReservable.get() == 1) {
					tReserved.inc(ins, -maxRations);
					if (serviceReserved.get() == 0) {
						data.service().report(service, ser.service(), -1);
					}
					
				}
				super.remove();
			}
			
			@Override
			protected void add() {
				serviceReservable.set(ins, 0);
				if (workAmount.get() >= 1 && tStored.get(ins) - tReserved.get(ins) >= maxRations) {
					serviceReservable.set(ins, 1);
					tReserved.inc(ins, maxRations);
					if (serviceReserved.get() == 0) {
						data.service().report(service, ser.service(), 1);
					}
				}
				super.add();
			}
			
		}


		
	}
	
	private static class State extends RoomStateInstance {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		private final RBITImp useMask = new RBITImp();
		
		
		public State(RoomDistributionIns ins, boolean broken) {
			super((RoomInstance)ins);
			useMask.clearSet(ins.distributionData().useMask);
			
		}
		
		@Override
		public void applyIns(RoomInstance ins) {
			if (ins instanceof RoomDistributionIns) {
				RoomDistributionIns s = (RoomDistributionIns) ins;
				for (RESOURCE g : s.distributionNlueData().all)
					if (useMask.has(g) != s.distributionData().useMask.has(g)) {
						s.distributionNlueData().usesToggle(g, ins);
					}
			}
			
		}
	}

	public State makeState(RoomDistributionIns ins, boolean broken) {
		return new State(ins, broken);
	}
	
	public interface RoomDistributionIns extends JOBMANAGER_HASER, ROOM_SERVICER {
		
		public abstract InstanceData distributionData();
		public abstract RoomDistribution distributionNlueData();
	}
	
	public void appendView(LISTE<UIRoomModule> mm, CharSequence food) {
		mm.add(new Gui(food));
	}
	
	class Gui extends UIRoomModule {
		
		private final CharSequence sFood;


		
		Gui(CharSequence food) {
			sFood = food;
		}

		
		
		@Override
		public void appendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1) {
			
			GuiSection s = new GuiSection();
			int i = 0;
			for (RESOURCE e : all) {
				final int ri = resourceIs[e.index()];
				GButt.BSection ss = new GButt.BSection() {
					
					@Override
					public void hoverInfoGet(GUI_BOX text) {
						RoomDistributionIns ins = (RoomDistributionIns) get.get();
						
						GBox b = (GBox) text;
						b.title(e.name);
						b.textLL(allStored.get(ri).name);
						b.tab(6);
						b.add((GFORMAT.iofkInv(b.text(), allStored.get(ri).get(get.get()), ins.distributionData().maxAmount)));
						b.NL();
						b.textLL(allIncoming.get(ri).name);
						b.tab(6);
						b.add(GFORMAT.i(b.text(), allIncoming.get(ri).get(get.get())));
						b.NL();
						
						
						b.textLL(¤¤Consumed + "(" + DicTime.¤¤Day + ")");
						b.tab(6);
						b.add(GFORMAT.i(b.text(), (int) -industry.ins().get(ri).day.getD(ins.distributionData())));
						b.NL();
						b.textLL(¤¤Consumed + "(" + DicTime.¤¤Year + ")");
						b.tab(6);
						b.add(GFORMAT.i(b.text(), (int) -industry.ins().get(ri).year.get(ins.distributionData())));
						b.NL();
						
						b.sep();
						b.textLL(¤¤Preferred);
						b.NL();
						for (Race r : RACES.all()) {
							if (isPref(e, r))
								b.add(r.appearance().icon);
						}
						
						
					}
					
					@Override
					protected void renAction() {
						RoomDistributionIns ins = (RoomDistributionIns) get.get();
						selectedSet(ins.distributionData().useMask.has(e));
					}
					
					@Override
					protected void clickA() {
						usesToggle(e, get.get());
					}
				};
				
				
				ss.addRightC(4, e.icon());
				
				ss.addRightC(4, new GStat() {
					
					@Override
					public void update(GText text) {
						GFORMAT.i(text, allStored.get(ri).get(get.get()));
					}
					
					@Override
					public void hoverInfoGet(GBox b) {
						b.title(e.name);
						b.textLL(sFood).add(GFORMAT.i(b.text(), allStored.get(ri).get(get.get())));					
					};
				});
				
				ss.body().incrW(48);
				ss.pad(4);
				
				s.add(ss, (i%3)*ss.body().width(), (i/3)*ss.body().height());
				i++;
				
			}
			
			GuiSection ss = new GuiSection();
			ss.add(new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.i(text, tStored.get(get.get()));
				}
				
				@Override
				public void hoverInfoGet(GBox b) {
					if (S.get().developer) {
						b.textLL(tStored.name);
						b.add(GFORMAT.i(b.text(), tStored.get(get.get())));
						b.NL();
						b.textLL(tIncoming.name);
						b.add(GFORMAT.i(b.text(), tIncoming.get(get.get())));
						b.NL();
						b.textLL(tReserved.name);
						b.add(GFORMAT.i(b.text(), tReserved.get(get.get())));
						b.NL();
					}
				};
				
			}.hh(sFood));
			
			ss.addRightC(48, new GStat() {
				
				@Override
				public void update(GText text) {
					
					double tot = 0;
					double worked = 0;
					
					for (COORDINATE c : get.get().body()) {
						if (get.get().is(c)) {
							if (isCrate(c.x(), c.y()) && job(c.x(), c.y()) != null) {
								tot += maxRations*8;
								worked += crate.workAmount.get();
							}
						}
					}
					
					GFORMAT.perc(text, worked/tot);
					
				}
			}.hh(UI.icons().s.hammer).hoverInfoSet(¤¤worked));
			
			ss.addRightC(48, ModuleIndustry.makeFetch(get));
			ss.body().incrW(48);
			
			s.addRelBody(2, DIR.N, ss);
			
			section.addRelBody(8, DIR.S, s);
			
		}
		
		@Override
		public void hover(GBox b, Room room, int rx, int ry) {
			RoomInstance ins = (RoomInstance)room;
			RoomDistributionIns dd = (RoomDistributionIns)room;
			b.NL();
			b.textLL(sFood).add(GFORMAT.i(b.text(), tStored.get(ins)));	
			b.NL();
			for (int i = 0; i < all.size(); i++) {
				RESOURCE r = all.get(i);
				b.add(r.icon());
				GText t = b.text();
				GFORMAT.i(t, allStored.get(i).get(ins));
				if (!dd.distributionData().useMask.has(r))
					t.errorify();
				b.add(t);
				b.space();
				if (i % 6 == 5) {
					b.NL();
				}
					
			}
			
			b.NL();
		}

		
		
		@Override
		public void appendManageScr(GGrid icons, GGrid text, GuiSection extra) {
			GuiSection s = new GuiSection();
			GChart cc = new GChart();
			
			int i = 0;
			int m = 5;
			
			
			
			for (RESOURCE e : all) {
				int ri = resourceIs[e.index()];
				
				SPRITE sp = new SPRITE.Imp(70, 24) {
					
					GText t = new GText(UI.FONT().S, 6);
					
					@Override
					public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
						e.icon().render(r, X1, Y1);
						t.clear();
						GFORMAT.i(t, allStored.get(ri).total.get());
						t.renderCY(r, X1+26, Y1+(Y2-Y1)/2);
					}
				};
				
				RENDEROBJ r = new GButt.ButtPanel(sp) {
					@Override
					protected void clickA() {
						if (uses(e)) {
							for (int i = 0; i < blue.instancesSize(); i++) {
								RoomDistributionIns ii = blue.getInstance(i);
								if (ii.distributionData().useMask.has(e)) {
									usesToggle(e, blue.getInstance(i));
								}
							}
						}else {
							for (int i = 0; i < blue.instancesSize(); i++) {
								RoomDistributionIns ii = blue.getInstance(i);
								if (!ii.distributionData().useMask.has(e)) {
									usesToggle(e, blue.getInstance(i));
								}
							}
						}
						super.clickA();
					}
					
					@Override
					public void hoverInfoGet(GUI_BOX text) {
						GBox b = (GBox) text;
						b.title(e.name);
						if (uses(e)) {
							b.text(¤¤uses);
						}else {
							b.text(¤¤usesN);
						}
						b.NL();
						
						b.title(e.name);
						b.textLL(allStored.get(ri).name);
						b.tab(6);
						b.add(GFORMAT.i(b.text(), allStored.get(ri).total.get()));
						b.NL(8);
						b.textLL(¤¤Consumed);
						b.tab(6);
						b.add(GFORMAT.i(b.text(), (int) -industry.ins().get(ri).history().get()));
						b.NL();
						cc.clear();
						cc.add(industry.ins().get(ri).history());
						b.add(cc);
						
						b.sep();
						b.textLL(¤¤Preferred);
						b.NL();
						for (Race r : RACES.all()) {
							if (r.pref().foodMask.has(e))
								b.add(r.appearance().icon);
						}
						
						
					}
					
					@Override
					protected void renAction() {
						selectedSet(uses(e));
					}
					
				};
				s.add(r, (i%m)*r.body().width(), (i/m)*r.body().height());
				i++;
			
			}
			
			s.add(new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.i(text, tStored.total.get());
					
				}
			}.hh(sFood), 0, s.body().y1()-16);
			
			text.add(s);

			
		}
		
		
		
		@Override
		public void problem(Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings, Room r, int rx, int ry) {
			ModuleIndustry.fetchProblem(free, errors, warnings, (RoomInstance)r);
		}
		

	}
	
	private int ci = -1;
	private final Bitmap1D ress = new Bitmap1D(RESOURCES.ALL().size(), false);
	public boolean uses(RESOURCE rr) {
		if (ci == GAME.updateI())
			return ress.get(rr.index());
		ci = GAME.updateI();
		ress.clear();
		
		for (int ri = 0; ri < all.size(); ri++) {
			RESOURCE r = all.get(ri);
			for (int i = 0; i < blue.instancesSize(); i++) {
				RoomDistributionIns ii = blue.getInstance(i);
				if (ii.distributionData() != null && ii.distributionData().useMask.has(r)) {
					ress.set(r.index(), true);
					break;
				}
			}
		}
		
		return ress.get(rr.index());
		
	}
	
}
