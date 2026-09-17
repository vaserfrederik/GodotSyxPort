package settlement.room.service.food.canteen;

import init.resources.RESOURCES;
import init.resources.ResG;
import init.settings.S;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.renderable.RENDEROBJ;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GGrid;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<CanteenInstance, ROOM_CANTEEN> {
	
	private final CharSequence ¤¤Food = "¤Meals";
	
	Gui(ROOM_CANTEEN s) {
		super(s);
		D.t(this);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<CanteenInstance> g, int x1, int y1) {
		
		GuiSection s = new GuiSection();
		int i = 0;
		for (ResG e : RESOURCES.EDI().all()) {
			
			GButt.BSection ss = new GButt.BSection() {
				
				@Override
				public void hoverInfoGet(GUI_BOX text) {
					GBox b = (GBox) text;
					b.title(e.resource.name);
					b.textLL(¤¤Food).add(GFORMAT.i(b.text(), g.get().amount(e)));		
					if (S.get().developer) {
						b.NL();
						b.textL(Dic.¤¤Access);
						b.add(GFORMAT.i(b.text(), g.get().amountReserved(e)));
					}
				}
				
				@Override
				protected void renAction() {
					selectedSet(g.get().uses(e));
				}
				
				@Override
				protected void clickA() {
					g.get().usesToggle(e);
				}
			};
			
			
			ss.addRightC(4, e.resource.icon());
			
			ss.addRightC(4, new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.i(text, g.get().amount(e));
				}
			
			});
			
			ss.body().incrW(48);
			ss.pad(4);
			
			s.add(ss, (i%3)*ss.body().width(), (i/3)*ss.body().height());
			i++;
			
		}
		
		s.addRelBody(2, DIR.N, new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.iofk(text, g.get().amountTotal(), g.get().maxAmount*RESOURCES.EDI().all().size());
				
			}
		}.hh(¤¤Food));
		
		section.addRelBody(8, DIR.S, s);
		
	}
	
	@Override
	protected void hover(GBox b, CanteenInstance i) {
		b.NL();
		b.textLL(¤¤Food).add(GFORMAT.i(b.text(), i.amountTotal()));	
		b.NL();
	}

	@Override
	protected void appendMain(GGrid gg, GGrid text, GuiSection sExtra) {
		GuiSection s = new GuiSection();
		int i = 0;
		for (ResG e : RESOURCES.EDI().all()) {
			RENDEROBJ r = new GStat() {
				
				@Override
				public void update(GText text) {
					GFORMAT.i(text, blueprint.amount(e));
				}
				
				@Override
				public void hoverInfoGet(GBox b) {
					b.title(e.resource.name);
					b.textLL(¤¤Food).add(GFORMAT.i(b.text(), blueprint.amount(e)));
				};
			}.hv(e.resource.icon());
			
			s.add(r, (i%4)*42, (i/4)*48);
			i++;
			
		}
		
		s.add(new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.i(text, blueprint.total);
				
			}
		}.hh(¤¤Food), 0, s.body().y1()-16);
		
		text.add(s);
		
	}
	
	

}
