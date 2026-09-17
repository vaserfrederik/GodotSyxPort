package settlement.room.spirit.temple;

import init.type.HTYPES;
import settlement.room.main.RoomInstance;
import settlement.room.spirit.temple.TempleAltar.Prisoner;
import settlement.stats.STATS;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.sets.LISTE;
import snake2d.util.sets.Stack;
import snake2d.util.sprite.text.Str;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GGrid;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.gui.table.GTableSorter.GTFilter;
import util.gui.table.GTableSorter.GTSort;
import util.info.GFORMAT;
import util.text.D;
import view.sett.ui.room.UIRoomBulkApplier;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<TempleInstance, ROOM_TEMPLE> {

	private static CharSequence ¤¤Respect = "The respect value of this temple. It is a combination of Sacrifices, room layout and priests.";
	private static CharSequence ¤¤Sacrifice = "¤Sacrificing";
	private static CharSequence ¤¤SacrificeD = "¤Sacrificed";
	private static CharSequence ¤¤SacrificeYearD = "¤Sacrificed this year.";
	private static CharSequence ¤¤SacrificingD = "¤How well this temple is sacrificing. In order to sacrifice, the priests must have access to resources. The temple requires {0} sacrifices per day.";
	private static CharSequence ¤¤SacrificingHuman = "¤This temple sacrifices prisoners. Currently there is a supply of {0} prisoners.";
	private static CharSequence ¤¤SacrificingAnimal = "¤This temple sacrifices animals. Livestock is needed";
	private static CharSequence ¤¤SacrificingResource = "This temple sacrifices {0}.";
	private static CharSequence ¤¤NoSacrifices = "No sacrifices are available!";
	private static CharSequence ¤¤PriestsD = "The staffing of this temple. Has an effect on respect. Temples must be fully staffed.";

	static {
		D.ts(Gui.class);
	}
	
	public Gui(ROOM_TEMPLE s) {
		super(s);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<TempleInstance> getter, int x1, int y1) {
		
		blueprint.constructor.decor.appendPanel(section, grid, getter, x1, y1);
		blueprint.constructor.grandure.appendPanel(section, grid, getter, x1, y1);
		blueprint.constructor.space.appendPanel(section, grid, getter, x1, y1);
		grid.add(new GStat() {
			@Override
			public void update(GText text) {
				GFORMAT.perc(text, getter.get().sacrificeValue());
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				{
					GText t = b.text();
					t.add(¤¤SacrificingD);
					t.insert(0, getter.get().jobs.size()*getter.get().blueprintI().STIME*0.5, 2);
					b.add(t);
				}
				b.NL(8);
				if (blueprint.altar instanceof TempleAltar.Resource) {
					GText t = b.text();
					t.add(¤¤SacrificingResource);
					t.insert(0, blueprint.resource.name);
					b.add(t);
				}else if (blueprint.altar instanceof TempleAltar.Animal) {
					GText t = b.text();
					t.add(¤¤SacrificingAnimal);
					t.insert(0, blueprint.resource.name);
					b.add(t);
				}else if (blueprint.altar instanceof Prisoner) {
					GText t = b.text();
					t.add(¤¤SacrificingHuman);
					t.insert(0, STATS.POP().pop(HTYPES.PRISONER()));
					b.add(t);
				}
				
				
			};
		}.hh(¤¤Sacrifice));
		
		grid.add(new GStat() {
			@Override
			public void update(GText text) {
				double d = (double)getter.get().employees().employed()/getter.get().employees().target();
				GFORMAT.perc(text, d);
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				b.text(¤¤PriestsD);
				b.NL(8);
			};
		}.hh(blueprint.constructor.priests.name()));
		
		grid.add(new GStat() {
			@Override
			public void update(GText text) {
				GFORMAT.i(text, getter.get().consumed);
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				b.text(¤¤SacrificeYearD);
				b.NL(8);
			};
		}.hh(¤¤SacrificeD));
		
		RENDEROBJ rr = new GStat() {
			@Override
			public void update(GText text) {
				GFORMAT.perc(text, getter.get().respect());
			}
			
			@Override
			public void hoverInfoGet(GBox b) {
				b.text(¤¤Respect);
				b.NL(8);
			};
		}.hv(STATS.RELIGION().TEMPLE.QUALITY.info().name);

		section.addRelBody(16, DIR.S, rr);
		
	}
	
	@Override
	protected void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts,
			LISTE<UIRoomBulkApplier> appliers) {
		
	}
	
	@Override
	protected void hover(GBox box, TempleInstance i) {
		super.hover(box, i);
		box.NL(8);
		box.textLL(¤¤Sacrifice);
		box.add(GFORMAT.perc(box.text(), i.sacrificeValue()));
	}
	
	@Override
	protected void problem(TempleInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings) {
		if (!i.resHas) {
			errors.add(¤¤NoSacrifices);
		}
	}


}
