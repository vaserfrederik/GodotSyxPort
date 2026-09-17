package settlement.room.main.furnisher;

import game.faction.FACTIONS;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.IndustryResource;
import settlement.room.industry.module.RoomBoost;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import settlement.room.service.module.RoomService.ROOM_SERVICE_HASER;
import settlement.room.water.RoomIrrigated.ROOM_IRRIGATED;
import settlement.tilemap.ground.Ground;
import snake2d.Errors;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GuiSection;
import snake2d.util.misc.CLAMP;
import snake2d.util.sets.INDEXED;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GGrid;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.info.INFO;
import util.text.D;
import util.text.Dic;
import view.sett.ui.room.UIRoomModule;

public abstract class FurnisherStat implements INDEXED, RoomBoost{

	protected final int index;
	private final CharSequence name;
	protected CharSequence desc;
	public final double min;
	private final INFO info;
	
	private static CharSequence ¤¤Services = "Services";
	private static CharSequence ¤¤serviceDesc = "Total amount of people that can be served simultaneously. The other (number) is an estimate of how many subjects the room will be able to serve, derived from your subjects' properties.";
	private static CharSequence ¤¤productionD = "Estimation of daily output of resources.";
	private static CharSequence ¤¤Efficiency = "Efficiency";
	private static CharSequence ¤¤EfficiencyD = "Efficiency is increased by certain items and can increase the usefulness of the room.";
	private static CharSequence ¤¤employeesD = "The amount of subjects needed to operate this room. The room might require less or more workers depending on circumstances.";
	private static CharSequence ¤¤wdesc = "Moisture is essential to all growing things. Most land has some moisture to begin with. This can be improved by fresh water access from irrigation or natural bodies of water at a later stage.";
	static {
		D.ts(FurnisherStat.class);
	}
	
	public FurnisherStat(Furnisher furnisher, CharSequence name, CharSequence desc, double min) {
		this.index = furnisher.stats.add(this);
		this.name = name;
		this.desc = desc;
		info = new INFO(name, desc);
		this.min = min;
	}
	
	public FurnisherStat(Furnisher furnisher) {
		if (Furnisher.jsonStat.length == furnisher.stats.size()) {
			throw new Errors.DataError("invalid number of stats have been declared");
		}
		
		this.name = Furnisher.jsonStat[furnisher.stats.size()].text("NAME");
		this.desc = Furnisher.jsonStat[furnisher.stats.size()].text("DESC");
		this.index = furnisher.stats.add(this);
		info = new INFO(name, desc);
		this.min = 0;
	}
	
	public FurnisherStat(Furnisher furnisher, double min) {
		if (Furnisher.jsonStat.length == furnisher.stats.size()) {
			throw new Errors.DataError("invalid number of stats have been declared");
		}
		
		this.name = Furnisher.jsonStat[furnisher.stats.size()].text("NAME");
		this.desc = Furnisher.jsonStat[furnisher.stats.size()].text("DESC");
		this.index = furnisher.stats.add(this);
		info = new INFO(name, desc);
		this.min = min;
		
	}
	
	public final CharSequence name() {
		return name;
	}

	public final CharSequence desc() {
		return desc;
	}
	
	@Override
	public INFO info() {
		return info;
	}

	public abstract GText format(GText t, double value);

	public double get(AREA area, double[] fromItems) {
		return get(area, fromItems[index]);
	}
	
	public abstract double get(AREA area, double acc);
	
	@Override
	public double get(RoomInstance r) {
		return r.stat(index);
	}
	
	@Override
	public final int index() {
		return index;
	};

	public void appendPanel(GuiSection section, GGrid grid, GETTER<? extends RoomInstance> getter, int x1, int y1) {
		
		grid.add(new GStat() {
			@Override
			public void update(GText text) {
				format(text, get(getter.get()));
			}
		}.hh(name).hoverInfoSet(desc));
		
	}
	
	public static class FurnisherStatRelative extends FurnisherStat {
		
		private final FurnisherStat other;
		private final double mod;
		
		public FurnisherStatRelative(Furnisher f, FurnisherStat other) {
			this(f, other, 1);
		}
		
		public FurnisherStatRelative(Furnisher f, FurnisherStat other, double mod) {
			super(f);
			this.other = other;
			this.mod = mod;
		}
		
		@Override
		public GText format(GText t, double d) {
			return GFORMAT.perc(t, d);
		}

		@Override
		public double get(AREA area, double[] fromItems) {
			
			double i = fromItems[index];
			double o = other.get(area, fromItems);
			
			if (o == 0) {
				if (i == 0)
					return 0;
				return 1;
			}
			
			return CLAMP.d(mod*i/o, 0, 1);
		}

		@Override
		public final double get(AREA area, double acc) {
			return acc;
		}
	}
	
	public static class FurnisherStatEfficiency extends FurnisherStat {
		
		private final FurnisherStat other;
		protected final double mul;
		
		public FurnisherStatEfficiency(Furnisher f, FurnisherStat workers) {
			this(f, workers, 1);
		}
		
		public FurnisherStatEfficiency(Furnisher f, FurnisherStat workers, double mul) {
			super(f, ¤¤Efficiency, ¤¤EfficiencyD, 0);
			this.other = workers;
			this.mul = mul;
		}
		
		@Override
		public GText format(GText t, double d) {
			return GFORMAT.perc(t, ((int)(100*d))/100.0);
		}

		@Override
		public double get(AREA area, double[] fromItems) {

			double i = fromItems[index];
			double o = other.get(area, fromItems);
			
			if (o == 0) {
				if (i == 0)
					return 0.5;
				return 1;
			}
			
			
			
			return CLAMP.d(0.5 + mul*0.5*i/o, 0, 1);
		}
		
		@Override
		public final double get(AREA area, double acc) {
			return acc;
		}
		
		@Override
		public double min() {
			return 0.5;
		}
		
	}
	
	
	public static class FurnisherStatIrrigation extends FurnisherStat {
		
		private final boolean nop;
		private final ROOM_IRRIGATED ii;

		public FurnisherStatIrrigation(Furnisher f, ROOM_IRRIGATED ii) {
			super(f, Ground.¤¤moisture, ¤¤wdesc, 0);
			this.ii = ii;
			nop = false;
		}

		
		@Override
		public GText format(GText t, double d) {
			if (nop) {
				t.add('-').add('-');
				return t;
			}
			else
				return GFORMAT.perc(t, ((int)(100*d))/100.0);
		}

		@Override
		public double get(AREA area, double[] fromItems) {

			return ii.irrigation().valueProspect(area);
		}
		
		@Override
		public final double get(AREA area, double acc) {
			return acc;
		}
		
		@Override
		public double min() {
			return ii.irrigation().from;
		}
		
	}
	
	public static class FurnisherStatI extends FurnisherStat {
		
		public FurnisherStatI(Furnisher f) {
			super(f);
		}
		
		public FurnisherStatI(Furnisher f, int min) {
			super(f, min);
		}
		
		@Override
		public GText format(GText t, double acc) {
			return GFORMAT.i(t, (int)Math.ceil(acc));
		}

		@Override
		public double get(AREA area, double acc) {
			return acc;
		}
	}
	
	public static class FurnisherStatEmployees extends FurnisherStat {
		
		public FurnisherStatEmployees(Furnisher f, double min) {
			super(f, Dic.¤¤Employees, ¤¤employeesD, min);
		}
		
		public FurnisherStatEmployees(Furnisher f) {
			this(f, 1);
		}
		
		@Override
		public GText format(GText t, double acc) {
			return GFORMAT.f(t, acc, 2);
		}

		@Override
		public double get(AREA area, double acc) {
			return acc;
		}
	}
	
	public static class FurnisherStatEmployeesR extends FurnisherStat {
		
		private final FurnisherStat services;
		private final double mul;
		
		public FurnisherStatEmployeesR(Furnisher f, FurnisherStat services, double mul) {
			super(f, Dic.¤¤Employees, ¤¤employeesD, 0);
			this.services = services;
			this.mul = mul;
		}
		
		@Override
		public GText format(GText t, double acc) {
			return GFORMAT.i(t, (int)Math.ceil(acc));
		}

		@Override
		public double get(AREA area, double acc) {
			return acc;
		}
		
		@Override
		public double get(AREA area, double[] fromItems) {

			return fromItems[services.index]*mul;
		}
	}
	
	public static class FurnisherStatServices extends FurnisherStat {
		
		private final ROOM_SERVICE_HASER p;
		
		public FurnisherStatServices(Furnisher f, ROOM_SERVICE_HASER p) {
			this(f, p, 0);
		}
		
		public FurnisherStatServices(Furnisher f, ROOM_SERVICE_HASER p, int min) {
			super(f, ¤¤Services, ¤¤serviceDesc, min);
			this.p = p;
		}
		
		@Override
		public GText format(GText t, double acc) {
			GFORMAT.i(t, (int)Math.ceil(acc));
			t.s();
			t.add('(');
			GFORMAT.i(t, (int)Math.ceil(acc*p.service().totalMultiplier()));
			t.add(')');
			
			return t;
		}

		@Override
		public double get(AREA area, double acc) {
			return acc;
		}
	}
	
	public static class FurnisherStatProduction extends FurnisherStat {
		
		private final INDUSTRY_HASER p;
		private final FurnisherStat eff;
		
		public FurnisherStatProduction(Furnisher f, INDUSTRY_HASER ins, FurnisherStat efficiency) {
			this(f, ins, efficiency, 0);
		}
		
		public FurnisherStatProduction(Furnisher f, INDUSTRY_HASER ins, FurnisherStat efficiency, int min) {
			super(f, Dic.¤¤ProductionRate, ¤¤productionD, min);
			this.p = ins;
			this.eff = efficiency;
		}
		
		@Override
		public GText format(GText t, double acc) {
			acc*= p.industries().get(0).outs().get(0).rate;
			GFORMAT.f(t, acc, 2);
			return t;
		}

		@Override
		public double get(AREA area, double acc) {
			return acc;
		}
		
		@Override
		public double get(AREA area, double[] fromItems) {

			double i = super.get(area, fromItems);
			if (eff != null)
				i *= eff.get(area, fromItems);
			return i;
		}
		
		protected double getBase() {
			return 1.0;
		}
	}
	
	public static class FurnisherStatProduction2 extends FurnisherStat {
		
		protected final INDUSTRY_HASER p;
		
		public FurnisherStatProduction2(Furnisher f, INDUSTRY_HASER ins) {
			this(f, ins, 0.1);
		}
		
		public FurnisherStatProduction2(Furnisher f, INDUSTRY_HASER ins, double min) {
			super(f, Dic.¤¤Production, ¤¤productionD, min);
			this.p = ins;
		}
		
		@Override
		public GText format(GText t, double acc) {
			GFORMAT.f(t, acc, 2);
			return t;
		}

		@Override
		public double get(AREA area, double acc) {
			
			double rr = 0;
			for (IndustryResource r : p.industries().get(0).outs())
				rr += r.rate;
			return rr*p.industries().get(0).bonus().get(FACTIONS.player());
		}
		
		@Override
		public double get(AREA area, double[] fromItems) {
			return get(area, fromItems[index])*getBase(area, fromItems);
		}
		
		protected double getBase(AREA area, double[] fromItems) {
			return 1.0;
		}
	}
	
	public UIRoomModule applier(RoomBlueprintIns<?> blue) {
		return new UIRoomModule() {
			
			@Override
			public void appendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1) {
				section.addRelBody(4, DIR.S, new GStat() {
					@Override
					public void update(GText text) {
						format(text, get((RoomInstance) get.get()));
					}
				}.hh(name).hoverInfoSet(desc));
			}
			
			@Override
			public void appendManageScr(GGrid icons, GGrid text, GuiSection sExtra) {
				icons.NL();
				icons.add(new GStat() {
					
					@Override
					public void update(GText text) {
						format(text, blue.getStat(index()));
					}
				}.decrease().hh(info));
				super.appendManageScr(icons, text, sExtra);
			}
			
			@Override
			public void hover(GBox box, Room i, int rx, int ry) {
				box.NL();
				box.text(name);
				box.add(format(box.text(), get((RoomInstance) i)));
				box.NL();
				super.hover(box, i, rx, ry);
			}
			
		};
	}


}
