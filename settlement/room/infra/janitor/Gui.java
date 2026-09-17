package settlement.room.infra.janitor;

import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.settings.S;
import init.sprite.UI.UI;
import settlement.main.SETT;
import settlement.room.main.RoomInstance;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.Hoverable.HOVERABLE.HoverableAbs;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.sets.LISTE;
import snake2d.util.sprite.text.Str;
import util.colors.GCOLOR;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GGrid;
import util.gui.misc.GHeader;
import util.gui.table.GTableBuilder;
import util.gui.table.GTableBuilder.GRowBuilder;
import util.gui.table.GTableSorter.GTFilter;
import util.gui.table.GTableSorter.GTSort;
import util.info.GFORMAT;
import util.text.D;
import view.sett.ui.room.ModuleIndustry;
import view.sett.ui.room.UIRoomBulkApplier;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<JanitorInstance, ROOM_JANITOR> {

	private static CharSequence ¤¤Resources = "Stored Resources";
	private static CharSequence ¤¤Global = "Daily global consumption estimate.";
	private static CharSequence ¤¤Bad = "This resource can not be reached by this room, and maintenance requiring it can not be performed.";
	
	static {
		D.ts(Gui.class);
	}
	
	Gui(ROOM_JANITOR s) {

		super(s);


	}
	
	@Override
	public void hover(GBox box, JanitorInstance i) {
		super.hover(box, i);
	}

	private int maxAm = 0;
	
	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<JanitorInstance> getter, int x1, int y1) {
		
		int rows = 4;
		
		RESOURCE[] resourceI = new RESOURCE[RESOURCES.ALL().size()];

		GuiSection s = new GuiSection() {
			
			@Override
			public void render(SPRITE_RENDERER r, float ds) {
				maxAm = 0;
				for (RESOURCE res : RESOURCES.ALL()) {
					resourceI[res.index()] = null;
				}
				for (RESOURCE res : RESOURCES.ALL()) {
					
					if (SETT.MAINTENANCE().estimateGlobal(res) > 0) {
						resourceI[maxAm] = res;
						maxAm ++;
					}
				}
				SETT.OVERLAY().MAINTENANCE.add(getter.get());
				super.render(r, ds);
			}
			
		};
		
		GTableBuilder b = new GTableBuilder() {
			
			@Override
			public int nrOFEntries() {
				return (int) Math.ceil((double)maxAm/rows);
			}
		};
		
		int width = 80;
		
		for (int off = 0; off < 4; off++) {
			int k = off;
			b.column(null, width, new GRowBuilder() {
				
				@Override
				public RENDEROBJ build(GETTER<Integer> ier) {
					return new Res(width, resourceI, ier, k, getter);
				}
			});
		}
		
		s.add(new GHeader(¤¤Resources));
		s.addRelBody(8, DIR.E, ModuleIndustry.makeFetch(getter));
		s.body().incrW(48);
		
		s.addRelBody(8, DIR.S, b.create(5, false));
		
		section.addRelBody(8, DIR.S, s);
		
	}
	
	@Override
	protected void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts,
			LISTE<UIRoomBulkApplier> appliers) {
		
	}

	private static class Res extends HoverableAbs {

		private final RESOURCE[] resourceI;
		private final GETTER<Integer> ier;
		private final int off;
		private final GETTER<JanitorInstance> getter;
		
		Res(int width, RESOURCE[] resourceI, GETTER<Integer> ier, int off, GETTER<JanitorInstance> getter){
			super(width, 32);
			this.resourceI = resourceI;
			this.ier = ier;
			this.off = off;
			this.getter = getter;
		}
		
		@Override
		protected void render(SPRITE_RENDERER r, float ds, boolean isHovered) {
			RESOURCE res = resourceI[ier.get()*4+off];
			if (res == null)
				return;
			
			GButt.ButtPanel.renderBG(r, true, false, isHovered, body);
			
			res.icon().renderCY(r, body().x1()+8, body().cY());
			
			Str.TMP.clear();
			Str.TMP.add(getter.get().bits.resAm(res));
			if (getter.get().bits.resMissing(res))
				GCOLOR.T().IBAD.bind();
			
			UI.FONT().S.renderCY(r, body.x1()+40, body().cY(), Str.TMP);
			COLOR.unbind();
			
			GButt.ButtPanel.renderFrame(r, body);
			
		}
		
		
		@Override
		public void hoverInfoGet(GUI_BOX text) {
			RESOURCE res = resourceI[ier.get()*4+off];
			if (res == null)
				return;
			GBox b = (GBox) text;
			b.title(res.name);
			b.textLL(¤¤Global);
			b.NL();
			b.add(GFORMAT.f0(b.text(), -SETT.MAINTENANCE().estimateGlobal(res)));
			b.NL();
//			B.ADD(GFORMAT.F0(B.TEXT(), -SETT.MAINTENANCE().ESTIMATEGLOBALRAW(RES)));
			b.NL(8);
			
			if (getter.get().bits.resMissing(res))
				b.add(b.text().warnify().add(¤¤Bad));
			b.NL();
			
			if (S.get().developer) {
				getter.get().bits.hover(b, res, getter.get());
				b.NL();
			}
			
			super.hoverInfoGet(text);
		}
		
		
		
	}

}
